namespace glytics.Common.Interface.Application
{
    public interface IApplicationDetails
    {
        public Models.Applications.Application GetDetails(string trackingCode);
        public Models.Applications.Application GetWithStatistics(string trackingCode);
    }
}