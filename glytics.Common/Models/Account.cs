using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using glytics.Common.Models.Applications;

namespace glytics.Common.Models
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public List<APIKey> ApiKeys { get; set; }
        public List<Application> Applications { get; set; }
    }
}