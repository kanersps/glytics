using glytics.Common.Interface.Application;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWorkApplication
    {
        public IApplication Application { get; }
        int Save();
    }
}