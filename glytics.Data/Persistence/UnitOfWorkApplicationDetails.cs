using glytics.Common.Interface;
using glytics.Common.Interface.Application;
using glytics.Data.Persistence.Applications;

namespace glytics.Data.Persistence
{
    public class UnitOfWorkApplicationDetails : UnitOfWork, IApplicationStatisticsDAL
    {
        public new IApplicationDetails Application { get; }
        
        public UnitOfWorkApplicationDetails(GlyticsDbContext context) : base(context)
        {
            Application = new ApplicationRepository(context);
        }
    }
}