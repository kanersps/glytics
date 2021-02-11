using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace glytics.Models
{
    [Table("ApplicationStatistic")]
    public class ApplicationStatistic
    {
        [Key]
        public Guid Id { get; set; }
        public DateTime Timestamp { get; set; }
        public int Visits { get; set; }
        public int PageViews { get; set; }
        
        public Application Application { get; set; }
    }
}