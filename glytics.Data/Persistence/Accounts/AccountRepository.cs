using System.Linq;
using glytics.Common.Models;
using Microsoft.EntityFrameworkCore;

namespace glytics.Data.Persistence.Accounts
{
    public class AccountRepository : Repository<Account>, IAccountRepository
    {
        public AccountRepository(DbContext context) : base(context)
        {
        }

        public Account GetById(string id)
        {
            return GlyticsDbContext.Account.FirstOrDefault(account => account.Id.ToString() == id);
        }

        public Account GetByUsername(string username)
        {
            return GlyticsDbContext.Account.FirstOrDefault(account => account.Username == username);
        }
        
        public Account GetByEmail(string username)
        {
            return GlyticsDbContext.Account.FirstOrDefault(account => account.Username == username);
        }

        private GlyticsDbContext GlyticsDbContext => _dbContext as GlyticsDbContext;
    }
}