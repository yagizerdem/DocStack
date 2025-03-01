using DataAccess;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Models;
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

    }
}
