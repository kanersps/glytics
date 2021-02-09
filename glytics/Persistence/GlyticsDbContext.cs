using Microsoft.EntityFrameworkCore;

namespace glytics.Persistence
{
    public class GlyticsDbContext : DbContext
    {
        public GlyticsDbContext(DbContextOptions options) : base(options) {}
    }
}