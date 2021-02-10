using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using glytics.Models;
using glytics.Persistence;
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