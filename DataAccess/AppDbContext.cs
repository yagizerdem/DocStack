using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Entity;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {

        public DbSet<WorkEntity> Works { get; set; }
        public AppDbContext()
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            // Retrieve the database path from an environment variable
            string dbPath = Environment.GetEnvironmentVariable("dbPath", EnvironmentVariableTarget.Process);

            if (string.IsNullOrEmpty(dbPath))
            {
                string documentsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                dbPath = Path.Combine(documentsPath, "docstack.services.db");
            }

            optionsBuilder.UseSqlite($"Data Source={dbPath}");
        }


    }
}
