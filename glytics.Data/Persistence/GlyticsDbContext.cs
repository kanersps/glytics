using glytics.Common.Models;
using glytics.Common.Models.Applications;
using Microsoft.EntityFrameworkCore;

namespace glytics.Data.Persistence
{
    public class GlyticsDbContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<APIKey> ApiKey { get; set; }
        
        public DbSet<Application> Application { get; set; }
        public DbSet<ApplicationStatistic> Statistic { get; set; }
        
        public GlyticsDbContext(DbContextOptions options) : base(options) {}

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Application>()
                .Property(p => p.Active)
                .HasDefaultValue(true);
        }
    }
}