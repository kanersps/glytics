using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace glytics.Logic.Application
{
    public class ApplicationService
    {
        private readonly GlyticsDbContext _dbContext;
        
        public ApplicationService(GlyticsDbContext _db)
        {
            _dbContext = _db;
        }
        
        private string GenerateTrackingJavascript(string id)
        {
            string url = "https://localhost:5001";

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_URL")))
                url = Environment.GetEnvironmentVariable("API_URL");
            
            return $"<script src=\"{url}/analytics.js\"></script>\n<script>\n\tgl(\"" + id + "\").send('view')\n</script>";
        }
        
        public WebsiteDetails GetWebsiteDetails(long[] curRange, string trackingCode)
        {
            Common.Models.Applications.Application web = _dbContext.Application.Include(app => app.BrowserStatistic)
                .Include(app => app.Statistic)
                .Include(app => app.PathStatistic)
                .AsSplitQuery()
                .OrderBy(a => a.Active)
                .FirstOrDefault(w => w.TrackingCode == trackingCode);

            var hourly = new List<long[]>();
            var hourlyPath = new List<dynamic[]>();
            var hourlyBrowser = new List<dynamic[]>();

            if (web != null && web.Statistic.Count > 0)
            {
                List<ApplicationStatistic> period =
                    web.Statistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                List<ApplicationStatisticPath> lastMonthPaths =
                    web.PathStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                List<ApplicationStatisticBrowser> lastMonthBrowsers =
                    web.BrowserStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                foreach (ApplicationStatistic stat in period)
                {
                    hourly.Add(new[]
                        {((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews});
                }

                foreach (ApplicationStatisticPath stat in lastMonthPaths)
                {
                    hourlyPath.Add(new dynamic[]
                    {
                        ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                        stat.Path
                    });
                }

                foreach (ApplicationStatisticBrowser stat in lastMonthBrowsers)
                {
                    hourlyBrowser.Add(new dynamic[]
                    {
                        ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                        stat.Browser
                    });
                }
            }

            return new WebsiteDetails()
            {
                Address = $"https://{web.Address}",
                Name = web.Name,
                Hourly = hourly,
                HourlyPaths = hourlyPath,
                HourlyBrowsers = hourlyBrowser
            };
        }

        public async Task<SimpleWebsiteDetails> GetWebsite(TrackingCode trackingCode)
        {
            Common.Models.Applications.Application web = _dbContext.Application.Include(app => app.Statistic).FirstOrDefault(w => w.TrackingCode == trackingCode.trackingCode);

            if (web == null)
                return null;

            var hourlyVisitors = 0;
            var hourlyViews = 0;
                
            var monthlyVisitors = 0;
            var monthlyViews = 0;

            if (web.Statistic.Count > 0)
            {
                ApplicationStatistic last = web.Statistic[^1];

                hourlyVisitors = last.Visits;
                hourlyViews = last.PageViews;

                List<ApplicationStatistic> lastMonth =
                    web.Statistic.OrderByDescending(stat => stat.Timestamp).Take(30 * 24).ToList();

                foreach (ApplicationStatistic stat in lastMonth)
                {
                    monthlyViews += stat.PageViews;
                    monthlyVisitors += stat.Visits;
                }

            }

            return new SimpleWebsiteDetails()
            {
                Address = $"https://{web.Address}",
                Name = web.Name,
                LastHourViews = hourlyViews,
                LastHourVisitors = hourlyVisitors,
                LastMonthViews = monthlyViews,
                LastMonthVisitors = monthlyVisitors,
                TrackingSnippet = GenerateTrackingJavascript(web.TrackingCode)
            };
        }

        public async Task<SearchResults> Search(Common.Models.Account account, SearchRequest searchRequest)
        {
            List<Common.Models.Applications.Application> applications = new List<Common.Models.Applications.Application>();

            if (searchRequest.Term.Length > 1)
            {
                applications = _dbContext.Application.Where(app => app.Account.Id == account.Id && app.Name.ToLower().Contains(searchRequest.Term)).ToList();
            }

            SearchResults results = new SearchResults()
            {
                Results = new List<SearchResult>()
            };
            
            foreach (Common.Models.Applications.Application application in applications)
            {
                results.Results.Add(new SearchResult()
                {
                    Title = application.Name,
                    Location = $"/applications/website/{application.TrackingCode}"
                });
            }

            return results;
        }
    }
}