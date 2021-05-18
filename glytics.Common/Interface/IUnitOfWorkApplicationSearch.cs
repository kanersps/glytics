using glytics.Common.Interface.Application;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWorkApplicationSearch
    {
        public IApplicationSearch Application { get; }
        int Save();
    }
}