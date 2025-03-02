using DataAccess;
using Microsoft.EntityFrameworkCore;
using Models;
using Models.ApiResponse;
using Models.Entity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service
{
    public class StarredService
    {
        private readonly AppDbContext _appDbContext;
        public StarredService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task<ServiceResponse<StarredEntity>> AddPaperToStarred(PaperEntity paper)
        {
            try
            {
                if (paper == null) throw new ArgumentException("invalid argument");

                StarredEntity? starredFromDb = await  _appDbContext.Starred.
                    Include(s => s.PaperEntity).FirstOrDefaultAsync<StarredEntity>(x => x.Id == paper.Id);
                if (starredFromDb != null) throw new Exception("already starred");

                starredFromDb = new();
                starredFromDb.PaperEntity = paper;
                starredFromDb.PaperEntityId = paper.Id;

                await _appDbContext.Starred.AddAsync(starredFromDb);

                PaperEntity? paperEntityFromDb = _appDbContext.Papers.FirstOrDefault(x => x.Id.Equals(paper.Id));

                if (paperEntityFromDb == null) throw new Exception($"Id : {paper.Id} is not valid");

                paperEntityFromDb.StarredEntity = starredFromDb;
                paperEntityFromDb.StarredEntityId = starredFromDb.Id;
                
                await _appDbContext.SaveChangesAsync();

                return ServiceResponse<StarredEntity>.Success(starredFromDb);
            }
            catch (Exception ex)
            {
                return ServiceResponse<StarredEntity>.Fail(ex.Message, ex);
            }
        }

        public async Task<ServiceResponse<StarredEntity>> RemoveStarred(Guid StarredId)
        {
            try
            {
                if (!isValidGUID(StarredId.ToString())) throw new ArgumentException("invalid guid");

                StarredEntity? StarredFromDb = await _appDbContext.Starred.FirstOrDefaultAsync(x => x.Id == StarredId);
                if (StarredFromDb == null) throw new Exception("starred paper not exist");

                PaperEntity? paperFromDb = _appDbContext.Papers.FirstOrDefault(x => x.Id == StarredFromDb.PaperEntityId);
                if (paperFromDb == null) throw new Exception("paper not exist");

                paperFromDb.StarredEntityId = null;

                _appDbContext.Starred.Remove(StarredFromDb);
                await _appDbContext.SaveChangesAsync();

                return ServiceResponse<StarredEntity>.Success(StarredFromDb);
            }
            catch (Exception ex)
            {
                return ServiceResponse<StarredEntity>.Fail(ex.Message, ex);
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
