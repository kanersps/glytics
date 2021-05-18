using glytics.Common.Interface.Account;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWorkAccountSearch
    {
        public IAccountSearch Account { get; }
        int Save();
    }
}