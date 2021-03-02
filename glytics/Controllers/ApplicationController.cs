using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using glytics.Attributes;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data;
using glytics.Data.Persistence;
using glytics.Logic;
using glytics.Logic.Application;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Controllers
{
    [ApiController]
    public class ApplicationController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        private readonly ApplicationService _app;
        
        public ApplicationController(GlyticsDbContext dbContext, ApplicationService app)
        {
            _db = dbContext;
            _app = app;
        }

        private string GenerateTrackingCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            uint uniqueIDs = BitConverter.ToUInt32(bytes, 0) % 100000000;

            return $"GL-{uniqueIDs}";
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
                return _app.GetWebsiteDetails(curRange, trackingCode.trackingCode);
            }

            return new UnauthorizedResult();
        }
        
        [HttpPost("application/website/details/simple")]
        [Authenticated]
        public async Task<ActionResult<SimpleWebsiteDetails>> WebsiteSimpleDetails(TrackingCode trackingCode)
        {
            SimpleWebsiteDetails result = await _app.GetWebsite(trackingCode);
            
            if (result != null)
                return result;

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