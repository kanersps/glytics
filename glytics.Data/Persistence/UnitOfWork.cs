using System.Threading.Tasks;
using glytics.Data.Persistence.Accounts;
using glytics.Data.Persistence.Applications;

namespace glytics.Data.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        public IAccountRepository Account { get; private set; }
        public IApplicationRepository Application { get; private set; }
        
        private readonly GlyticsDbContext _context;
        
        public UnitOfWork(GlyticsDbContext context)
        {
            _context = context;
            Account = new AccountRepository(_context);
            Application = new ApplicationRepository(_context);
        }
        
        public void Dispose()
        {
            _context.Dispose();
        }

        public int Save()
        {
            return _context.SaveChanges();
        }
    }
}