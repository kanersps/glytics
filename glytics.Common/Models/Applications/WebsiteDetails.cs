using System.Collections.Generic;

namespace glytics.Common.Models.Applications
{
    public class WebsiteDetailPath
    {
        public  System.DateTime Timestamp { get; set; }
        public  int Visits { get; set; }
        public  int PageViews { get; set; }
        public string Path { get; set; }
    }
    public class WebsiteDetailBrowser
    {
        public  System.DateTime Timestamp { get; set; }
        public  int Visits { get; set; }
        public  int PageViews { get; set; }
        public string Browser { get; set; }
    }
    
    public class WebsiteDetail
    {
        public  System.DateTime Timestamp { get; set; }
        public  int Visits { get; set; }
        public  int PageViews { get; set; }
    }
    public class WebsiteDetails : IWebsiteDetail
    {
        public List<WebsiteDetail> Hourly { get; set; }
        public List<WebsiteDetailPath> HourlyPaths { get; set; }
        public List<WebsiteDetailBrowser> HourlyBrowsers { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}