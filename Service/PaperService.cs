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
                await _appDbContext.Papers.AddAsync(paper).ConfigureAwait(false);
                await _appDbContext.SaveChangesAsync().ConfigureAwait(false);

                return ServiceResponse<PaperEntity>.Success(paper, "Paper added successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<PaperEntity>.Fail("An error occurred while adding the paper", ex);
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

                if (titleFilter)
                    query = query.Where(x => (x.Title ?? "").Contains(search));
                if (authroFilter)
                    query = query.Where(x => (x.Authors ?? "").Contains(search));

                if (yearFilter)
                    query = query.Where(x => (x.Year ?? "").Contains(search));

                if (publisherFilter)
                    query = query.Where(x => (x.Publisher ?? "").Contains(search));

                List<PaperEntity> papersFromDb= await query.ToListAsync().ConfigureAwait(false);


                return ServiceResponse<List<PaperEntity>>.Success(papersFromDb, "Papers retrieved successfully");
            }
            catch (Exception ex)
            {
                return ServiceResponse<List<PaperEntity>>.Fail("An error occurred while retrieving papers", ex);
            }
        }

    }
}
