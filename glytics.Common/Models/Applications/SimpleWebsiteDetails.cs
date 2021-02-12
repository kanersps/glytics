using System.Collections.Generic;

namespace glytics.Common.Models.Applications
{
    public class SimpleWebsiteDetails : IWebsiteDetail
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public string TrackingSnippet { get; set; }
        public int LastHourViews { get; set; }
        public int LastHourVisitors { get; set; }
        public int LastMonthViews { get; set; }
        public int LastMonthVisitors { get; set; }
    }
}