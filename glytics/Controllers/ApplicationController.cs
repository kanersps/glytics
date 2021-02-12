using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Controllers
{
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        private readonly Authentication _apiHandler;
        
        public ApplicationController(GlyticsDbContext dbContext, Authentication apiHandler)
        {
            _db = dbContext;
            _apiHandler = apiHandler;
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
            return "<script src=\"https://localhost:5001/analytics.js\"></script>\n<script>\n\tgl(\"" + id + "\").send('view')\n</script>";
        }

        public class TrackingCode
        {
            public string trackingCode { get; set; }
        }
        
        [HttpPost("application/website/details")]
        public async Task<ActionResult<WebsiteDetails>> WebsiteDetails(TrackingCode trackingCode)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();

            Account account = await _db.Account.FirstOrDefaultAsync(acc => acc.Id == key.Account.Id);
            
            if (account != null)
            {
                Application web = _db.Application.Include(app => app.Statistic).FirstOrDefault(w => w.TrackingCode == trackingCode.trackingCode);

                if (web == null)
                    return NotFound();

                var hourly = new List<ApplicationStatistic>();

                if (web.Statistic.Count > 0)
                {
                    List<ApplicationStatistic> lastMonth =
                        web.Statistic.OrderByDescending(stat => stat.Timestamp).Take(30 * 24).ToList();

                    foreach (ApplicationStatistic stat in lastMonth)
                    {
                        hourly.Add(stat);
                    }
                }

                return new WebsiteDetails()
                {
                    Address = $"https://{web.Address}",
                    Name = web.Name,
                    Hourly = hourly.Select(h => new WebsiteDetail{Timestamp = h.Timestamp, Visits = h.Visits, PageViews = h.PageViews}).ToList()
                };
            }

            return new UnauthorizedResult();
        }
        
        [HttpPost("application/website/details/simple")]
        public async Task<ActionResult<SimpleWebsiteDetails>> WebsiteSimpleDetails(TrackingCode trackingCode)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();

            Account account = await _db.Account.FirstOrDefaultAsync(acc => acc.Id == key.Account.Id);
            
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
        public async Task<ActionResult> DeactivateWebsite(ApplicationRemove website)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();

            Account account = await _db.Account.FirstOrDefaultAsync(acc => acc.Id == key.Account.Id);

            if (account != null)
            {
                Application web = account.Applications.FirstOrDefault(w => w.TrackingCode == website.TrackingCode);

                if (web != null) web.Active = false;

                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }
        
        [HttpPost("application/website/activate")]
        public async Task<ActionResult> ActivateWebsite(ApplicationRemove website)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();

            Account account = await _db.Account.FirstOrDefaultAsync(acc => acc.Id == key.Account.Id);

            if (account != null)
            {
                Application web = account.Applications.FirstOrDefault(w => w.TrackingCode == website.TrackingCode);

                if (web != null) web.Active = true;

                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpPost("application/website/delete")]
        public async Task<ActionResult> DeleteWebsite(ApplicationRemove website)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();

            Account account = await _db.Account.FirstOrDefaultAsync(acc => acc.Id == key.Account.Id);
            if (account != null)
            {
                Application web = account.Applications.FirstOrDefault(w => w.TrackingCode == website.TrackingCode);

                _db.Application.Remove(web!);
                
                await _db.SaveChangesAsync();
                
                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpGet("application/website/all")]
        public async Task<ActionResult<IList>> GetWebsites()
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();
            
            

            return key.Account.Applications.Select(app => new {app.Address, app.Name, app.TrackingCode, app.Active}).ToList();
        }
        
        [HttpPost("application/website/create")]
        public async Task<ActionResult<ApplicationCreateMessage>> AddWebsite(Website website)
        {
            APIKey key = await _apiHandler.Authorized(Request.Headers["key"]);

            if (key == null)
                return new UnauthorizedResult();
            
            if (ModelState.IsValid)
            {
                Account account = await _db.Account.Include(a => a.Applications).FirstOrDefaultAsync(u => u.Id == key.Account.Id);

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