using System.Collections.Generic;
using System.Linq;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence.Accounts;
using Microsoft.EntityFrameworkCore;

namespace glytics.Data.Persistence.Applications
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(DbContext context) : base(context)
        {
        }

        public Application GetDetails(string trackingCode)
        {
            return GlyticsDbContext.Application
                .Include(app => app.BrowserStatistic)
                .Include(app => app.Statistic)
                .Include(app => app.PathStatistic)
                .AsSplitQuery()
                .OrderBy(a => a.Active)
                .First(w => w.TrackingCode == trackingCode);
        }
        
        public Application GetWithStatistics(string trackingCode)
        {
            return GlyticsDbContext.Application
                .Include(app => app.Statistic)
                .Include(app => app.PathStatistic)
                .Include(app => app.BrowserStatistic)
                .AsSplitQuery()
                .OrderBy(a => a.Active)
                .First(w => w.TrackingCode == trackingCode);
        }

        public List<Application> Search(Account account, string search)
        {
            return GlyticsDbContext.Application
                .Where(app => app.Account.Id == account.Id && app.Name.ToLower().Contains(search))
                .ToList();
        }
        
        public Application GetByAddress(Account account, string address)
        {
            return GlyticsDbContext.Application
                .FirstOrDefault(app => app.Account.Id == account.Id && app.Address.ToLower().Contains(address));
        }
        
        public Application GetByOwnerAndTrackingCode(Account account, string trackingCode)
        {
            return GlyticsDbContext.Application
                .First(w => w.TrackingCode == trackingCode && w.Account.Id == account.Id);
        }
        
        public List<Application> GetWebsitesByOwner(Account account)
        {
            return GlyticsDbContext.Application
                .Where(app => app.Account.Id == account.Id)
                .ToList();
        }

        private GlyticsDbContext GlyticsDbContext => _dbContext as GlyticsDbContext;
    }
}