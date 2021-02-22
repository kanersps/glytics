using System;
using System.Collections.Generic;
using System.Linq;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.EntityFrameworkCore;

namespace glytics.Logic.Analytics
{
    public class ApplicationService
    {
        public WebsiteDetails GetWebsiteDetails(GlyticsDbContext _db, long[] curRange, string trackingCode)
        {
                Application web = _db.Application.Include(app => app.BrowserStatistic)
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
                        web.Statistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();
                    
                    List<ApplicationStatisticPath> lastMonthPaths =
                        web.PathStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();
                    
                    List<ApplicationStatisticBrowser> lastMonthBrowsers =
                        web.BrowserStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                    foreach (ApplicationStatistic stat in period)
                    {
                        hourly.Add(new []{ ((DateTimeOffset)stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews });
                    }
                    
                    foreach (ApplicationStatisticPath stat in lastMonthPaths)
                    {
                        hourlyPath.Add(new dynamic[]{ ((DateTimeOffset)stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews, stat.Path });
                    }
                    
                    foreach (ApplicationStatisticBrowser stat in lastMonthBrowsers)
                    {
                        hourlyBrowser.Add(new dynamic[]{ ((DateTimeOffset)stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews, stat.Browser });
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
    }
}