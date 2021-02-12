using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace glytics.Common.Models.Applications
{
    public class ApplicationStatisticPath : IApplicationStatistic
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int Visits { get; set; }
        public int PageViews { get; set; }
        public string Path { get; set; }
        
        public Application Application { get; set; }
    }
}