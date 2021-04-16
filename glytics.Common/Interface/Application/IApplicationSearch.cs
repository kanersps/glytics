using System.Collections.Generic;

namespace glytics.Common.Interface.Application
{
    public interface IApplicationSearch
    {
        Models.Applications.Application GetByOwnerAndTrackingCode(Models.Account account, string trackingCode);
        List<Models.Applications.Application> GetWebsitesByOwner(Models.Account account);
        Models.Applications.Application GetByAddress(Models.Account account, string address);
        List<Models.Applications.Application> Search(Models.Account account, string search);
    }
}