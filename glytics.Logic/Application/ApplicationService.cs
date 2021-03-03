using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using glytics.Common.Models.Applications;
using glytics.Data.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace glytics.Logic.Application
{
    public class ApplicationService
    {
        private readonly UnitOfWork _unitOfWork;
        
        public ApplicationService(UnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        private string GenerateTrackingJavascript(string id)
        {
            string url = "https://localhost:5001";

            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("API_URL")))
                url = Environment.GetEnvironmentVariable("API_URL");
            
            return $"<script src=\"{url}/analytics.js\"></script>\n<script>\n\tgl(\"" + id + "\").send('view')\n</script>";
        }
        
        private string GenerateTrackingCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            uint uniqueIDs = BitConverter.ToUInt32(bytes, 0) % 100000000;

            return $"GL-{uniqueIDs}";
        }
        
        public WebsiteDetails GetWebsiteDetails(long[] curRange, string trackingCode)
        {
            Common.Models.Applications.Application web = _unitOfWork.Application.GetDetails(trackingCode);

            var hourly = new List<long[]>();
            var hourlyPath = new List<dynamic[]>();
            var hourlyBrowser = new List<dynamic[]>();

            if (web != null && web.Statistic.Count > 0)
            {
                List<ApplicationStatistic> period =
                    web.Statistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                List<ApplicationStatisticPath> lastMonthPaths =
                    web.PathStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                List<ApplicationStatisticBrowser> lastMonthBrowsers =
                    web.BrowserStatistic.OrderByDescending(stat => stat.Timestamp).Where(stat =>
                        stat.Timestamp > DateTimeOffset.FromUnixTimeMilliseconds(curRange[0]) &&
                        stat.Timestamp < DateTimeOffset.FromUnixTimeMilliseconds(curRange[1])).ToList();

                foreach (ApplicationStatistic stat in period)
                {
                    hourly.Add(new[]
                        {((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews});
                }

                foreach (ApplicationStatisticPath stat in lastMonthPaths)
                {
                    hourlyPath.Add(new dynamic[]
                    {
                        ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                        stat.Path
                    });
                }

                foreach (ApplicationStatisticBrowser stat in lastMonthBrowsers)
                {
                    hourlyBrowser.Add(new dynamic[]
                    {
                        ((DateTimeOffset) stat.Timestamp).ToUnixTimeMilliseconds(), stat.Visits, stat.PageViews,
                        stat.Browser
                    });
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

        public async Task<SimpleWebsiteDetails> GetWebsite(TrackingCode trackingCode)
        {
            Common.Models.Applications.Application web =
                _unitOfWork.Application.GetWithStatistics(trackingCode.trackingCode);

            if (web == null)
                return null;

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

        public async Task<SearchResults> Search(Common.Models.Account account, SearchRequest searchRequest)
        {
            List<Common.Models.Applications.Application> applications = new List<Common.Models.Applications.Application>();

            if (searchRequest.Term.Length > 1)
            {
                applications = _unitOfWork.Application.Search(account, searchRequest.Term);
            }

            SearchResults results = new SearchResults()
            {
                Results = new List<SearchResult>()
            };
            
            foreach (Common.Models.Applications.Application application in applications)
            {
                results.Results.Add(new SearchResult()
                {
                    Title = application.Name,
                    Location = $"/applications/website/{application.TrackingCode}"
                });
            }

            return results;
        }

        public async Task<bool> Deactivate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web = _unitOfWork.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Active = false;
                
                _unitOfWork.Save();
                    
                return true;
            }
            else
            {
                return false;
            }
        }
        
        public async Task<bool> Activate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web =
                _unitOfWork.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Active = true;
                
                _unitOfWork.Save();

                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> Delete(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web = _unitOfWork.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web == null)
                return false;
            
            _unitOfWork.Application.Remove(web);
                
            _unitOfWork.Save();

            return true;
        }

        public async Task<ActionResult<IList>> GetWebsites(Common.Models.Account account)
        {
            return _unitOfWork.Application.GetWebsitesByOwner(account).Select(app => new {app.Address, app.Name, app.TrackingCode, app.Active}).ToList();
        }

        public async Task<ApplicationCreateMessage> CreateWebsite(Common.Models.Account account, Website website)
        {
            Common.Models.Applications.Application application = account.Applications.FirstOrDefault(app => app.Address == website.Address);

            if (application == null)
            {
                website.TrackingCode = GenerateTrackingCode();
                account.Applications.Add(website);
                _unitOfWork.Save();
                    
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
    }
}