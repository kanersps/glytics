using System.ComponentModel.DataAnnotations;

namespace glytics.Common.Models.Applications
{
    public class Website : Application
    {
        public Website()
        {
            Type = "website";
        }
    }
}