using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using glytics.Logic.Application.Web;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shyjus.BrowserDetection;

namespace glytics.Controllers
{
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly GlyticsDbContext _db;
        private readonly IBrowserDetector _browserDetector;
        private readonly Analytic _analytic;

        public StatisticController(GlyticsDbContext dbContext, IBrowserDetector browserDetector, Analytic analytic)
        {
            _db = dbContext;
            _browserDetector = browserDetector;
            _analytic = analytic;
        }
        
        [HttpPost("app/web")]
        public async Task<ActionResult> HandleStatisticRequest(StatisticRequest request)
        {
            request.Sent = DateTime.Now.ToUniversalTime();

            if (request.Type != "view")
                return new BadRequestResult();

            Application site = await _db.Application.Include(app => app.Statistic).Include(app => app.BrowserStatistic).Include(app => app.PathStatistic).AsSplitQuery().OrderBy(a => a.Active).SingleAsync(app => app.TrackingCode == request.Id);

            if (site == null || site.Active == false)
                return new BadRequestResult();

            await _analytic.New(site, _db, request, _browserDetector.Browser, Request);
            
            return new OkResult();
        }
    }
}