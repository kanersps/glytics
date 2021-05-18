using glytics.Common.Interface.Account;
using glytics.Data.Persistence.Accounts;

namespace glytics.Data.Persistence
{
    public class UnitOfWorkAccount : UnitOfWork, IAccountDAL, IUnitOfWorkAccount
    {
        public new IAccount Account { get; }
        
        public UnitOfWorkAccount(GlyticsDbContext context) : base(context)
        {
            Account = new AccountRepository(context);
        }
    }
}