using glytics.Common.Interface;
using glytics.Common.Interface.Account;
using glytics.Common.Models;

namespace glytics.Data.Persistence.Accounts
{
    public interface IAccountRepository : IRepository<Account>, IAccountSearch, IAccount
    {
    }
}