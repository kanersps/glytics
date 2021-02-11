using System.Collections.Generic;

namespace glytics.Models
{
    public class SimpleWebsiteDetails
    {
        public string Name { get; set; }
        public string Address { get; set; }
        public int LasthourViews { get; set; }
        public int LasthourVisitors { get; set; }
        
        public List<int> HourlyViews { get; set; }
        public List<int> HourlyVisitors { get; set; }
    }
}