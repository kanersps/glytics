using System.Collections.Generic;

namespace glytics.Models
{
    public class WebsiteDetail
    {
        public  System.DateTime Timestamp { get; set; }
        public  int Visits { get; set; }
        public  int PageViews { get; set; }
    }
    public class WebsiteDetails : IWebsiteDetail
    {
        public List<WebsiteDetail> Hourly { get; set; }
        public string Name { get; set; }
        public string Address { get; set; }
    }
}