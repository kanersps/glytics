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

        [HttpPost("application/search")]
        [Authenticated]
        public async Task<ActionResult<SearchResults>> Search(SearchRequest searchRequest)
        {
            Account account = (Account) HttpContext.Items["Account"];
            
            return await _app.Search(account, searchRequest);
        }
        
        [HttpPost("application/website/details")]
        [Authenticated]
        public ActionResult<WebsiteDetails> WebsiteDetails(TrackingCode trackingCode)
        {
            long[] curRange = { DateTimeOffset.UtcNow.AddDays(-30).ToUnixTimeMilliseconds(), DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() };

            if (trackingCode.range.Length == 2)
            {
                curRange = new []{ trackingCode.range[0], trackingCode.range[1] };
            }
            
            return _app.GetWebsiteDetails(curRange, trackingCode.trackingCode);
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

            if (await _app.Deactivate(account, website))
            {
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }
        
        [HttpPost("application/website/activate")]
        [Authenticated]
        public async Task<ActionResult> ActivateWebsite(ApplicationRemove website)
        {
            Account account = (Account) HttpContext.Items["Account"];

            if (await _app.Activate(account, website))
            {
                return new OkResult();
            }
            else
            {
                return new BadRequestResult();
            }
        }

        [HttpPost("application/website/delete")]
        [Authenticated]
        public async Task<ActionResult> DeleteWebsite(ApplicationRemove website)
        {
            Account account = (Account) HttpContext.Items["Account"];
            
            if (await _app.Delete(account, website))
            {
                return new OkResult();
            }

            return new BadRequestResult();
        }

        [HttpGet("application/website/all")]
        [Authenticated]
        public async Task<ActionResult<IList>> GetWebsites()
        {
            return await _app.GetWebsites(((Account) HttpContext.Items["Account"]));
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
            
            if (ModelState.IsValid)
            {
                Account account = (Account) HttpContext.Items["Account"];

                return await _app.CreateWebsite(account, website);
            }
            
            return new ApplicationCreateMessage()
            {
                Success = false,
                Message = "Unknown"
            };
        }
    }
}