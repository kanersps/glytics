using glytics.Common.Interface;
using glytics.Common.Interface.Application;
using glytics.Data.Persistence.Applications;

namespace glytics.Data.Persistence
{
    public class UnitOfWorkApplication : UnitOfWork, IApplicationDAL
    {
        public new IApplication Application { get; }
        
        public UnitOfWorkApplication(GlyticsDbContext context) : base(context)
        {
            Application = new ApplicationRepository(context);
        }
    }
}