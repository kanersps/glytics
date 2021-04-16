using System.Collections.Generic;
using glytics.Common.Interface;
using glytics.Common.Interface.Application;
using glytics.Common.Models;
using glytics.Common.Models.Applications;

namespace glytics.Data.Persistence.Applications
{
    public interface IApplicationRepository : IRepository<Application>, IApplicationDetails, IApplicationSearch, IApplication
    {
    }
}