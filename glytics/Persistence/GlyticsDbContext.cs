using glytics.Models;
using Microsoft.EntityFrameworkCore;

namespace glytics.Persistence
{
    public class GlyticsDbContext : DbContext
    {
        public DbSet<Account> Account { get; set; }
        public DbSet<APIKey> ApiKey { get; set; }
        
        public DbSet<Application> Application { get; set; }
        
        public GlyticsDbContext(DbContextOptions options) : base(options) {}
    }
}