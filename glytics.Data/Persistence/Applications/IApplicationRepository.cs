using System.Collections.Generic;
using glytics.Common.Models;
using glytics.Common.Models.Applications;

namespace glytics.Data.Persistence.Applications
{
    public interface IApplicationRepository : IRepository<Application>
    {
        Application GetDetails(string trackingCode);
        Application GetWithStatistics(string trackingCode);
        List<Application> Search(Account account, string search);
        Application GetByOwnerAndTrackingCode(Account account, string id);
        List<Application> GetWebsitesByOwner(Account account);
    }
}