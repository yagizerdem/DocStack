using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Models.Entity;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace DataAccess
{
    public class AppDbContext : DbContext
    {

        public DbSet<PaperEntity> Papers { get; set; }

        public DbSet<StarredEntity> Starred { get; set; }

        public DbSet<MyDocuments> MyDocuments { get; set; }

        // ensure 1 thread can use db context 
        public SemaphoreSlim _semaphore = new SemaphoreSlim(1, 1);

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<PaperEntity>()
                .Property(e => e.Inserted)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default timestamp when row is inserted

            modelBuilder.Entity<PaperEntity>()
                .Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP") // Set initial value
                .ValueGeneratedOnAddOrUpdate(); // Auto-update on modifications


            modelBuilder.Entity<StarredEntity>()
                .Property(e => e.Inserted)
                .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default timestamp when row is inserted

            modelBuilder.Entity<StarredEntity>()
                .Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP") // Set initial value
                .ValueGeneratedOnAddOrUpdate(); // Auto-update on modifications

            modelBuilder.Entity<MyDocuments>()
    .Property(e => e.Inserted)
    .HasDefaultValueSql("CURRENT_TIMESTAMP"); // Default timestamp when row is inserted

            modelBuilder.Entity<MyDocuments>()
                .Property(e => e.LastUpdated)
                .HasDefaultValueSql("CURRENT_TIMESTAMP") // Set initial value
                .ValueGeneratedOnAddOrUpdate(); // Auto-update on modifications


            modelBuilder.Entity<PaperEntity>()
            .HasOne(a => a.StarredEntity)
            .WithOne(a => a.PaperEntity)
            .HasForeignKey<StarredEntity>(c => c.PaperEntityId);

        }

    }
}
