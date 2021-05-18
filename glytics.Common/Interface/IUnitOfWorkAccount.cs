using glytics.Common.Interface.Account;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWorkAccount
    {
        public IAccount Account { get; }
        int Save();
    }
}