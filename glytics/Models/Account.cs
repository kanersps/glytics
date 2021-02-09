using System;
using System.ComponentModel.DataAnnotations;

namespace glytics.Models
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}