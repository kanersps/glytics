using glytics.Common.Interface.Application;

namespace glytics.Common.Interface
{
    public interface IApplicationStatisticsDAL
    {
        public IApplicationDetails Application { get; }
    }
}