using System;
using System.ComponentModel.DataAnnotations;

namespace glytics.Common.Models
{
    public class APIKey
    {
        [Key]
        public Guid Key { get; set; }
        public Account Account { get; set; }
        public string Description { get; set; }
    }
}