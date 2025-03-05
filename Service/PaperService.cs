using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models;
using Models.ApiResponse;
using Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace Service
{
    public class PaperService
    {
        private readonly AppDbContext _appDbContext;
        public PaperService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ServiceResponse<PaperEntity>> AddPaperAsync(PaperEntity paper)
        {
            if (paper == null)
                return ServiceResponse<PaperEntity>.Fail("Paper entity cannot be null");

            try
            {
                await _appDbContext._semaphore.WaitAsync();

                await _appDbContext.Papers.AddAsync(paper).ConfigureAwait(false);
                await _appDbContext.SaveChangesAsync().ConfigureAwait(false);

                return ServiceResponse<PaperEntity>.Success(paper, "Paper added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PaperEntity>.Fail("An error occurred while adding the paper", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<List<PaperEntity>>> GetPapersAsync(
            string search,
            int offset, 
            int limit,
            bool titleFilter,
            bool authroFilter,
            bool publisherFilter,
            bool yearFilter,
            bool orderByAscending = true)
        {
            try
            {
                await _appDbContext._semaphore.WaitAsync();

                if (string.IsNullOrEmpty(search)) search = "";

                var query = _appDbContext.Papers
                    .Include(p => p.StarredEntity) // Ensure left join
                    .AsQueryable(); 

                // Apply ordering separately
                IOrderedQueryable<PaperEntity> orderedQuery = orderByAscending
                    ? query.OrderBy(x => x.Year)
                    : query.OrderByDescending(x => x.Year);

                // apply pagination
                query = orderedQuery
                    .Skip(offset)
                    .Take(limit)
                    .AsQueryable();

                query = query.Where(x =>
                    (titleFilter && (x.Title ?? "").ToLower().Contains(search.ToLower())) ||
                    (authroFilter && (x.Authors ?? "").ToLower().Contains(search.ToLower())) ||
                    (yearFilter && (x.Year ?? "").ToLower().Contains(search.ToLower())) ||
                    (publisherFilter && (x.Publisher ?? "").ToLower().Contains(search.ToLower()))
                );

                List<PaperEntity> papersFromDb= await query.ToListAsync().ConfigureAwait(false);


                return ServiceResponse<List<PaperEntity>>.Success(papersFromDb, "Papers retrieved successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<PaperEntity>>.Fail("An error occurred while retrieving papers", ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }


        public async Task<ServiceResponse<PaperEntity>> GetById(Guid paperId)
        {
            if (!isValidGUID(paperId.ToString()))
            {
                return ServiceResponse<PaperEntity>.Fail("guid is invalid");
            }
            try
            {
                await _appDbContext._semaphore.WaitAsync();
                PaperEntity? paperFromDb = await _appDbContext.Papers.FirstOrDefaultAsync(x => x.Id == paperId);
                return ServiceResponse<PaperEntity>.Success(paperFromDb);
            }
            catch (Exception ex)
            {
                return ServiceResponse<PaperEntity>.Fail(ex.Message, ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }

        public async Task<ServiceResponse<PaperEntity>> DeleteById(Guid paperId)
        {
            if (!isValidGUID(paperId.ToString()))
            {
                return ServiceResponse<PaperEntity>.Fail("guid is invalid");
            }
            try
            {
                await _appDbContext._semaphore.WaitAsync();
                PaperEntity? paperFromDb = await _appDbContext.Papers.FirstOrDefaultAsync(x => x.Id == paperId);
                if(paperFromDb == null) throw new Exception($"papaer with id {paperId} not exist");

                // remove from starred
                StarredEntity? starred = await _appDbContext.Starred.FirstOrDefaultAsync(x => x.PaperEntityId == paperId);

                if(starred != null)
                {
                    _appDbContext.Starred.Remove(starred);  
                }

                _appDbContext.Papers.Remove(paperFromDb);
                await _appDbContext.SaveChangesAsync();

                return ServiceResponse<PaperEntity>.Success(paperFromDb);
            }
            catch (Exception ex)
            {
                return ServiceResponse<PaperEntity>.Fail(ex.Message, ex);
            }
            finally
            {
                _appDbContext._semaphore.Release();
            }
        }
        public static bool isValidGUID(string str)
        {
            string strRegex = @"^[{]?[0-9a-fA-F]{8}-([0-9a-fA-F]{4}-){3}[0-9a-fA-F]{12}[}]?$";
            Regex re = new Regex(strRegex);
            if (re.IsMatch(str))
                return (true);
            else
                return (false);
        }
    }
}
