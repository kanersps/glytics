using glytics.Common.Models;

namespace glytics.Data.Persistence.Accounts
{
    public interface IAccountRepository : IRepository<Account>
    {
        Account GetById(string id);
        Account GetByUsername(string username);
        Account GetByEmail(string email);
        Account GetWithApplications(Account account);
    }
}