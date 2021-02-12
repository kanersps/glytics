using System.ComponentModel.DataAnnotations;

namespace glytics.Common.Models
{
    public class RegisterAccount
    {
        [Required]
        [MinLength(3)]
        public string Username { get; set; }
        
        [Required]
        [MinLength(3)]
        public string Password { get; set; }
        
        [Required]
        [EmailAddress]
        public string Email { get; set; }
    }
}