using System;
using System.Threading.Tasks;
using glytics.Data.Persistence.Accounts;
using glytics.Data.Persistence.Applications;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWork : IDisposable
    {
        IAccountRepository Account { get; }
        IApplicationRepository Application { get; }
        
        int Save();
    }
}