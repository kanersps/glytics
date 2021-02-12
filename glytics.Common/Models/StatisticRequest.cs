using System;

namespace glytics.Common.Models
{
    public class StatisticRequest
    {
        public string Type { get; set; }
        public string Id { get; set; }
        public string Timezone { get; set; }
        public DateTime Sent { get; set; }
        public bool Unique { get; set; }
    }
}