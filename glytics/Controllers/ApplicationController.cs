using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using glytics.Common.Attributes;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data;
using glytics.Data.Persistence;
using glytics.Logic;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Controllers
{
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        
        public ApplicationController(GlyticsDbContext dbContext)
        {
            _db = dbContext;
        }

        private string GenerateTrackingCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            uint uniqueIDs = BitConverter.ToUInt32(bytes, 0) % 100000000;

            return $"GL-{uniqueIDs}";
        }

        private string GenerateTrackingJavascript(string id)
        {
            string url = "https://localhost:5001";

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_URL")))
                url = Environment.GetEnvironmentVariable("API_URL");
            
            return $"<script src=\"{url}/analytics.js\"></script>\n<script>\n\tgl(\"" + id + "\").send('view')\n</script>";
        }

        [HttpPost("application/search")]
        [Authenticated]
        public ActionResult<SearchResults> Search(SearchRequest searchRequest)
        {
            Account account = (Account) HttpContext.Items["Account"];

            List<Application> applications = new List<Application>();

            if (searchRequest.Term.Length > 1)
            {
                applications = _db.Application.Where(app => app.Account.Id == account.Id && app.Name.ToLower().Contains(searchRequest.Term)).ToList();
            }

            SearchResults results = new SearchResults()
            {
                Results = new List<SearchResult>()
            };
            
            foreach (Application application in applications)
            {
                results.Results.Add(new SearchResult()
                {
                    Title = application.Name,
                    Location = $"/applications/website/{application.TrackingCode}"
                });
            }

            return results;
        }

        public class TrackingCode
        {
            public string trackingCode { get; set; }
            public long[] range { get; set; }
        }
        
        [HttpPost("application/website/details")]
        [Authenticated]
        public ActionResult<WebsiteDetails> WebsiteDetails(TrackingCode trackingCode)
        {
            Account account = (Account) HttpContext.Items["Account"];

            long[] curRange = { DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

            if (trackingCode.range.Length == 2)
            {
                curRange = new []{ trackingCode.range[0], trackingCode.range[1] };
            }
            
            if (account != null)
            {
                Application web = _db.Application.Include(app => app.BrowserStatistic).Include(app => app.Statistic).Include(app => app.PathStatistic).AsSplitQuery().OrderBy(a => a.Active).FirstOrDefault(w => w.TrackingCode == trackingCode.trackingCode);

                if (web == null)
                    return NotFound();

                var hourly = new List<ApplicationStatistic>();
                var hourlyPath = new List<ApplicationStatisticPath>();
                var hourlyBrowser = new List<ApplicationStatisticBrowser>();

                if (web.Statistic.Count > 0)
                {
                    List<ApplicationStatistic> lastMonth =
                        web.Statistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();
                    
                    List<ApplicationStatisticPath> lastMonthPaths =
                        web.PathStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();
                    
                    List<ApplicationStatisticBrowser> lastMonthBrowsers =
                        web.BrowserStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat => stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) && stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                    foreach (ApplicationStatistic stat in lastMonth)
                    {
                        hourly.Add(stat);
                    }
                    
                    foreach (ApplicationStatisticPath stat in lastMonthPaths)
                    {
                        hourlyPath.Add(stat);
                    }
                    
                    foreach (ApplicationStatisticBrowser stat in lastMonthBrowsers)
                    {
                        hourlyBrowser.Add(stat);
                    }
                }

                return new WebsiteDetails()
                {
                    Address = $"https://{web.Address}",
                    Name = web.Name,
                    Hourly = hourly.Select(h => new WebsiteDetail{Timestamp = h.Timestamp, Visits = h.Visits, PageViews = h.PageViews}).ToList(),
                    HourlyPaths = hourlyPath.Select(h => new WebsiteDetailPath{Timestamp = h.Timestamp, Visits = h.Visits, PageViews = h.PageViews, Path = h.Path}).ToList(),
                    HourlyBrowsers = hourlyBrowser.Select(h => new WebsiteDetailBrowser{Timestamp = h.Timestamp, Visits = h.Visits, PageViews = h.PageViews, Browser = h.Browser}).ToList()
                };
            }

            return new UnauthorizedResult();
        }
        
        [HttpPost("application/website/details/simple")]
        [Authenticated]
        public ActionResult<SimpleWebsiteDetails> WebsiteSimpleDetails(TrackingCode trackingCode)
        {
            Account account = (Account) HttpContext.Items["Account"];
            
            if (account != null)
            {
                Application web = _db.Application.Include(app => app.Statistic).FirstOrDefault(w => w.TrackingCode == trackingCode.trackingCode);

                if (web == null)
                    return NotFound();

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

            return new UnauthorizedResult();
        }
        
        [HttpPost("application/website/deactivate")]
        [Authenticated]
        public async Task<ActionResult> DeactivateWebsite(ApplicationRemove website)
        {
            Account account = (Account) HttpContext.Items["Account"];

            if (account != null)
            {
                Application web = _db.Application.FirstOrDefault(w => w.TrackingCode == website.TrackingCode && w.Account.Id == account.Id);

                if (web != null) web.Active = false;

                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }
        
        [HttpPost("application/website/activate")]
        [Authenticated]
        public async Task<ActionResult> ActivateWebsite(ApplicationRemove website)
        {
            Account account = (Account) HttpContext.Items["Account"];

            if (account != null)
            {
                Application web = _db.Application.FirstOrDefault(w => w.TrackingCode == website.TrackingCode && w.Account.Id == account.Id);

                if (web != null) web.Active = true;

                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("application/website/delete")]
        [Authenticated]
        public async Task<ActionResult> DeleteWebsite(ApplicationRemove website)
        {
            Account account = (Account) HttpContext.Items["Account"];
            
            if (account != null)
            {
                Application web = _db.Application.FirstOrDefault(w => w.TrackingCode == website.TrackingCode && w.Account.Id == account.Id);

                _db.Application.Remove(web!);
                
                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpGet("application/website/all")]
        [Authenticated]
        public async Task<ActionResult<IList>> GetWebsites()
        {
            Account account = await _db.Account.Include(acc => acc.Applications).FirstOrDefaultAsync(acc => acc.Id == ((Account) HttpContext.Items["Account"]).Id);

            return account?.Applications.Select(app => new {app.Address, app.Name, app.TrackingCode, app.Active}).ToList();
        }
        
        [HttpPost("application/website/create")]
        [Authenticated]
        public async Task<ActionResult<ApplicationCreateMessage>> AddWebsite(Website website)
        {
            CaptchaCheck captcha = new CaptchaCheck(website.RecaptchaToken);
            if(!captcha.Verify())
                return new ApplicationCreateMessage()
                {
                    Success = false,
                    Message = "Invalid captcha! Please try again."
                };
            
            Account _account = (Account) HttpContext.Items["Account"];
            
            if (ModelState.IsValid)
            {
                Account account = await _db.Account.Include(a => a.Applications).FirstOrDefaultAsync(u => u.Id == _account.Id);

                Application application = account.Applications.FirstOrDefault(app => app.Address == website.Address);

                if (application == null)
                {
                    website.TrackingCode = GenerateTrackingCode();
                    account.Applications.Add(website);
                    await _db.SaveChangesAsync();
                    
                    return new ApplicationCreateMessage()
                    {
                        Success = true,
                        Message = ""
                    };
                }

                return new ApplicationCreateMessage()
                {
                    Success = false,
                    Message = "You already have an application with this address"
                };
            }
            
            return new ApplicationCreateMessage()
            {
                Success = false,
                Message = "Unknown"
            };
        }
    }
}