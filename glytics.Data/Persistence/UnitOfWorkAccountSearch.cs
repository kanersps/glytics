using glytics.Common.Interface.Account;
using glytics.Data.Persistence.Accounts;

namespace glytics.Data.Persistence
{
    public class UnitOfWorkAccountSearch : UnitOfWork, IAccountSearchDAL, IUnitOfWorkAccountSearch
    {
        public new IAccountSearch Account { get; }
        
        public UnitOfWorkAccountSearch(GlyticsDbContext context) : base(context)
        {
            Account = new AccountRepository(context);
        }
    }
}