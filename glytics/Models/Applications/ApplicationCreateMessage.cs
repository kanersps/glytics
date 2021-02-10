using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace glytics.Models
{
    public class ApplicationCreateMessage
    {
        public bool Success { get; set; }
        public string Message { get; set; }
    }
}