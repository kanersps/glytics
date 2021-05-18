using glytics.Common.Interface.Application;

namespace glytics.Data.Persistence
{
    public interface IUnitOfWorkApplicationDetails
    {
        public IApplicationDetails Application { get; }
        int Save();
    }
}