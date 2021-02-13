using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Controllers
{
    [ApiController]
    public class StatisticController : ControllerBase
    {
        private readonly GlyticsDbContext _db;

        public StatisticController(GlyticsDbContext dbContext)
        {
            _db = dbContext;
        }

        private DateTime RoundTimeHour(DateTime time)
        {
            return new(time.Year, time.Month, time.Day, time.Hour, 0, 0);
        }
        
        [HttpPost("app/web")]
        public async Task<ActionResult> HandleStatisticRequest(StatisticRequest request)
        {
            request.Sent = DateTime.Now;
            
            if (request.Type != "view")
                return new BadRequestResult();
            
            Application site = await _db.Application.Include(app => app.Statistic).Include(app => app.PathStatistic).AsSplitQuery().OrderBy(a => a.Active).SingleAsync(app => app.TrackingCode == request.Id);

            if (site == null)
                return new BadRequestResult();
            
            ApplicationStatistic thisHour = site.Statistic.FirstOrDefault(stat => stat.Timestamp == RoundTimeHour(request.Sent));
            ApplicationStatisticPath thisHourPage = site.PathStatistic.FirstOrDefault(stat => stat.Timestamp == RoundTimeHour(request.Sent) && stat.Path == request.Path);

            if (thisHour == null)
            {
                thisHour = new ApplicationStatistic()
                {
                    PageViews = 0,
                    Visits = 0,
                    Timestamp = RoundTimeHour(request.Sent)
                };
                
                site.Statistic.Add(thisHour);
            }
            
            thisHour.PageViews++;
                
            if(request.Unique)
                thisHour.Visits++;

            if (thisHourPage == null)
            {
                thisHourPage = new ApplicationStatisticPath()
                {
                    PageViews = 0,
                    Visits = 0,
                    Timestamp = RoundTimeHour(request.Sent),
                    Path = request.Path
                };
                
                site.PathStatistic.Add(thisHourPage);
            }
            
            thisHourPage.PageViews++;
                
            if(request.Unique)
                thisHourPage.Visits++;
            
            await _db.SaveChangesAsync();
            
            return new OkResult();
        }
    }
}