using DataAccess;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service
{
    public class MyDocumentsService 
    {
        private readonly AppDbContext _appDbContext;
        public MyDocumentsService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ServiceResponse<MyDocuments>> AddAsync(MyDocuments newDocument)
        {
            if (newDocument == null)
                return ServiceResponse<MyDocuments>.Fail("Document cannot be null.");

            try
            {
                await _appDbContext._semaphore.WaitAsync();

                _appDbContext.MyDocuments.Add(newDocument);
                await _appDbContext.SaveChangesAsync();

                return ServiceResponse<MyDocuments>.Success(newDocument);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error adding document: {ex.Message}");
                return ServiceResponse<MyDocuments>.Fail("An error occurred while saving the document.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<List<MyDocuments>>> FetchAllAsync()
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();

                List<MyDocuments> myDocumentsFromDb = await _appDbContext.MyDocuments
                    .Select(doc => new MyDocuments
                    {
                        Id = doc.Id,
                        Year = doc.Year,
                        Publisher = doc.Publisher,
                        Authors = doc.Authors,
                        Title = doc.Title
                    })
                    .ToListAsync();

                return ServiceResponse<List<MyDocuments>>.Success(myDocumentsFromDb);

            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ServiceResponse <List<MyDocuments>>.Fail("An error occurred while fetching documents.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<MyDocuments>> DeleteByIdAsync(Guid documentId)
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();
                MyDocuments? documentFromDb = await _appDbContext.MyDocuments.FirstOrDefaultAsync(x => x.Id == documentId);

                if(documentFromDb == null)
                {
                    return ServiceResponse<MyDocuments>.Fail($"Document with id {documentId} not exist");
                }

                _appDbContext.MyDocuments.Remove(documentFromDb);
                await _appDbContext.SaveChangesAsync();
                return ServiceResponse<MyDocuments>.Success(documentFromDb);
            }
            catch(Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ServiceResponse<MyDocuments>.Fail("An error occurred while deleting documents.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<List<MyDocuments>>> GetWithPaginationAsync(int offset, int limit)
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();

                List<MyDocuments> myDocumentsFromDb = await _appDbContext.MyDocuments
                    .Skip(offset)
                    .Take(limit)
                    .ToListAsync();

                return ServiceResponse<List<MyDocuments>>.Success(myDocumentsFromDb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ServiceResponse<List<MyDocuments>>.Fail("An error occurred while fetching documents.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<MyDocuments>>  GetFileAsync(Guid documentId)
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();
                MyDocuments? documentFromDb = await _appDbContext.MyDocuments.FirstOrDefaultAsync(x => x.Id == documentId);

                if (documentFromDb == null)
                {
                    return ServiceResponse<MyDocuments>.Fail($"Document with id {documentId} not exist");
                }

                return ServiceResponse<MyDocuments>.Success(documentFromDb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ServiceResponse<MyDocuments>.Fail("An error occurred while deleting documents.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<MyDocuments>> ChangeFileAsync(Guid documentId, byte[] blob)
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();
                MyDocuments? documentFromDb = await _appDbContext.MyDocuments.FirstOrDefaultAsync(x => x.Id == documentId);

                if (documentFromDb == null)
                {
                    return ServiceResponse<MyDocuments>.Fail($"Document with id {documentId} not exist");
                }

                documentFromDb.FileData = blob;
                await _appDbContext.SaveChangesAsync();    

                return ServiceResponse<MyDocuments>.Success(documentFromDb);
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine(ex.Message);
                return ServiceResponse<MyDocuments>.Fail("An error occurred while deleting documents.", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

    }
}
