using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Entity;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {

        public DbSet<WorkEntity> Works { get; set; }
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }
    

    }
}
