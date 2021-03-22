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
        
        public WebsiteDetails GetWebsiteDetails(long[] curRange, string trackingCode)
        {
            Common.Models.Applications.Application web = _unitOfWork.Application.GetDetails(trackingCode);

            return web.GetDetails(curRange);
        }

        public async Task<SimpleWebsiteDetails> GetWebsite(TrackingCode trackingCode)
        {
            Common.Models.Applications.Application web =
                _unitOfWork.Application.GetWithStatistics(trackingCode.trackingCode);

            if (web == null)
                return null;

            return web.GetWebsite();
        }

        public async Task<SearchResults> Search(Common.Models.Account _account, SearchRequest searchRequest)
        {
            Common.Models.Account account = _unitOfWork.Account.GetWithApplications(_account);
            
            return await account.Search(searchRequest);
        }

        public async Task<bool> Deactivate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web = _unitOfWork.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Activate();
                
                _unitOfWork.Save();
                    
                return true;
            }

            return false;
        }
        
        public async Task<bool> Activate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web =
                _unitOfWork.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Deactivate();
                
                _unitOfWork.Save();
                    
                return true;
            }

            return false;
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
            Common.Models.Applications.Application application =
                _unitOfWork.Application.GetByAddress(account, website.Address);

            if (application == null)
            {
                account = _unitOfWork.Account.GetWithApplications(account);

                account.CreateWebsite(website);
                
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