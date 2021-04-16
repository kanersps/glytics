using glytics.Common.Interface;
using glytics.Common.Interface.Application;
using glytics.Data.Persistence.Applications;

namespace glytics.Data.Persistence
{
    public class UnitOfWorkApplicationSearch : UnitOfWork, IApplicationSearchDAL
    {
        public new IApplicationSearch Application { get; }
        
        public UnitOfWorkApplicationSearch(GlyticsDbContext context) : base(context)
        {
            Application = new ApplicationRepository(context);
        }
    }
}