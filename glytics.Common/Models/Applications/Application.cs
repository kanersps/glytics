using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace glytics.Common.Models.Applications
{
    public class Application
    {
        [Key]
        public string TrackingCode { get; set; }
        public string Type { get; set; }
        
        [Required]
        public string Name { get; set; }
        [Required]
        public string Address { get; set; }
        
        public bool? Active { get; set; }
        
        public Account Account { get; set; }
        
        public List<ApplicationStatistic> Statistic { get; set; }
        public List<ApplicationStatisticPath> PathStatistic { get; set; }
    }
}