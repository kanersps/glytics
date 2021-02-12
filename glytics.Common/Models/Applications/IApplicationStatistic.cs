using System;

namespace glytics.Common.Models.Applications
{
    public interface IApplicationStatistic
    {
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int Visits { get; set; }
        public int PageViews { get; set; }
        
        public Application Application { get; set; }
    }
}