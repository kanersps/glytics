using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shyjus.BrowserDetection.Browsers;

namespace glytics.Logic.Analytics.Web
{
    public class Analytic
    {
        public async Task New(Application site, GlyticsDbContext db, StatisticRequest request, IBrowser browserDetector, HttpRequest httpRequest)
        {
            ApplicationStatistic thisHour = site.Statistic.FirstOrDefault(stat => stat.Timestamp == Utility.RoundTimeHour(request.Sent));
            ApplicationStatisticPath thisHourPage = site.PathStatistic.FirstOrDefault(stat => stat.Timestamp == Utility.RoundTimeHour(request.Sent) && stat.Path == request.Path);

            if (thisHour == null)
            {
                thisHour = new ApplicationStatistic()
                {
                    PageViews = 0,
                    Visits = 0,
                    Timestamp = Utility.RoundTimeHour(request.Sent)
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
                    Timestamp = Utility.RoundTimeHour(request.Sent),
                    Path = request.Path
                };
                
                site.PathStatistic.Add(thisHourPage);
            }
            
            thisHourPage.PageViews++;
                
            if(request.Unique)
                thisHourPage.Visits++;
            
            if (httpRequest.Headers.ContainsKey("User-Agent"))
            {
                ApplicationStatisticBrowser thisHourBrowser = site.BrowserStatistic.FirstOrDefault(stat =>
                    stat.Timestamp == Utility.RoundTimeHour(request.Sent) && stat.Browser == browserDetector.Name);
                
                if (thisHourBrowser == null)
                {
                    thisHourBrowser = new ApplicationStatisticBrowser()
                    {
                        PageViews = 0,
                        Visits = 0,
                        Timestamp = Utility.RoundTimeHour(request.Sent),
                        Browser = browserDetector.Name
                    };
                
                    site.BrowserStatistic.Add(thisHourBrowser);
                }
            
                thisHourBrowser.PageViews++;
                
                if(request.Unique)
                    thisHourBrowser.Visits++;
            }
            
            await db.SaveChangesAsync();
        }
    }
}