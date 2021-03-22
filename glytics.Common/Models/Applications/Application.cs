using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        public List<ApplicationStatisticBrowser> BrowserStatistic { get; set; }

        private List<ApplicationStatistic> GetGeneralStatisticsByRange(long[] range)
        {
            return Statistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(range[0]) &&
                stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(range[1])).ToList();
        }

        private List<ApplicationStatisticPath> GetPathStatisticsByRange(long[] range)
        {
            return PathStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(range[0]) &&
                stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(range[1])).ToList();
        }

        private List<ApplicationStatisticBrowser> GetBrowserStatisticsByRange(long[] range)
        {
            return BrowserStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(range[0]) &&
                stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(range[1])).ToList();
        }

        #nullable enable
        public WebsiteDetails GetDetails(long[]? range)
        {
            if(range == null)
                range = new []{ DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

            List<long[]> hourly = new List<long[]>();
            List<dynamic[]> hourlyPath = new List<dynamic[]>();
            List<dynamic[]> hourlyBrowser = new List<dynamic[]>();
            
            foreach (ApplicationStatistic stat in GetGeneralStatisticsByRange(range))
            {
                hourly.Add(new[]
                    {((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews});
            }

            foreach (ApplicationStatisticPath stat in GetPathStatisticsByRange(range))
            {
                hourlyPath.Add(new dynamic[]
                {
                    ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                    stat.Path
                });
            }

            foreach (ApplicationStatisticBrowser stat in GetBrowserStatisticsByRange(range))
            {
                hourlyBrowser.Add(new dynamic[]
                {
                    ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                    stat.Browser
                });
            }

            return new WebsiteDetails()
            {
                Address = $"https://{Address}",
                Name = Name,
                Hourly = hourly,
                HourlyPaths = hourlyPath,
                HourlyBrowsers = hourlyBrowser
            };
        }

        public SimpleWebsiteDetails GetWebsite()
        {
            var hourlyVisitors = 0;
            var hourlyViews = 0;
                
            var monthlyVisitors = 0;
            var monthlyViews = 0;

            if (Statistic.Count > 0)
            {
                ApplicationStatistic last = Statistic[^1];

                hourlyVisitors = last.Visits;
                hourlyViews = last.PageViews;

                List<ApplicationStatistic> lastMonth =
                    Statistic.OrderByDescending(stat => stat.Timestamp).Take(30 * 24).ToList();

                foreach (ApplicationStatistic stat in lastMonth)
                {
                    monthlyViews += stat.PageViews;
                    monthlyVisitors += stat.Visits;
                }

            }

            return new SimpleWebsiteDetails()
            {
                Address = $"https://{Address}",
                Name = Name,
                LastHourViews = hourlyViews,
                LastHourVisitors = hourlyVisitors,
                LastMonthViews = monthlyViews,
                LastMonthVisitors = monthlyVisitors,
                TrackingSnippet = GenerateTrackingJavascript()
            };
        }
        
        public string GenerateTrackingJavascript()
        {
            string url = "https://localhost:5001";

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_URL")))
                url = Environment.GetEnvironmentVariable("API_URL");
            
            return $"<script src=\"{url}/analytics.js\"></script>\n<script>\n\tgl(\"" + TrackingCode + "\").send('view')\n</script>";
        }

        public void Deactivate()
        {
            Active = false;
        }

        public void Activate()
        {
            Active = true;
        }
    }
}