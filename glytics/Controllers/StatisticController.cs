using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using glytics.Models;
using glytics.Persistence;
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
            if (request.Type != "view")
                return new BadRequestResult();
            
            Application site = _db.Application.Include(app => app.Statistic).FirstOrDefault(app => app.TrackingCode == request.Id);

            if (site == null)
                return new BadRequestResult();
            
            ApplicationStatistic thisHour = site.Statistic.FirstOrDefault(stat => stat.Timestamp == RoundTimeHour(request.Sent));

            if (thisHour == null)
            {
                thisHour = new ApplicationStatistic()
                {
                    PageViews = 1,
                    Visits = 1,
                    Timestamp = RoundTimeHour(request.Sent)
                };
                
                site.Statistic.Add(thisHour);
            }
            else
            {
                thisHour.PageViews++;
                
                if(request.Unique)
                    thisHour.Visits++;
            }
            
            await _db.SaveChangesAsync();
            
            return new OkResult();
        }
    }
}