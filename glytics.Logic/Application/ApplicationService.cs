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
        private readonly IUnitOfWorkApplicationDetails _unitOfWorkApplicationDetails;
        private readonly IUnitOfWorkApplicationSearch _unitOfWorkApplicationSearch;
        private readonly IUnitOfWorkApplication _unitOfWorkApplication;
        private readonly IUnitOfWorkAccountSearch _unitOfWorkAccountSearch;
        
        public ApplicationService(IUnitOfWorkApplicationDetails unitOfWork, IUnitOfWorkApplicationSearch unitOfWorkSearch, IUnitOfWorkApplication unitOfWorkApplication, IUnitOfWorkAccountSearch unitOfWorkAccountSearch)
        {
            _unitOfWorkApplicationDetails = unitOfWork;
            _unitOfWorkApplicationSearch = unitOfWorkSearch;
            _unitOfWorkApplication = unitOfWorkApplication;
            _unitOfWorkAccountSearch = unitOfWorkAccountSearch;
        }
        
        public WebsiteDetails GetWebsiteDetails(long[] curRange, string trackingCode)
        {
            Common.Models.Applications.Application web = _unitOfWorkApplicationDetails.Application.GetDetails(trackingCode);

            return web.GetDetails(curRange);
        }

        public async Task<SimpleWebsiteDetails> GetWebsite(TrackingCode trackingCode)
        {
            Common.Models.Applications.Application web =
                _unitOfWorkApplicationDetails.Application.GetWithStatistics(trackingCode.trackingCode);

            if (web == null)
                return null;

            return web.GetWebsite();
        }

        public async Task<SearchResults> Search(Common.Models.Account _account, SearchRequest searchRequest)
        {
            Common.Models.Account account = _unitOfWorkAccountSearch.Account.GetWithApplications(_account);
            
            return await account.Search(searchRequest);
        }

        public async Task<bool> Deactivate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web = _unitOfWorkApplicationSearch.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Activate();
                
                _unitOfWorkApplicationSearch.Save();
                    
                return true;
            }

            return false;
        }
        
        public async Task<bool> Activate(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web =
                _unitOfWorkApplicationSearch.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web != null)
            {
                web.Deactivate();
                
                _unitOfWorkApplicationSearch.Save();
                    
                return true;
            }

            return false;
        }

        public async Task<bool> Delete(Common.Models.Account account, ApplicationRemove website)
        {
            Common.Models.Applications.Application web = _unitOfWorkApplicationSearch.Application.GetByOwnerAndTrackingCode(account, website.TrackingCode);

            if (web == null)
                return false;
            
            _unitOfWorkApplication.Application.Remove(web);
                
            _unitOfWorkApplication.Save();

            return true;
        }

        public async Task<ActionResult<IList>> GetWebsites(Common.Models.Account account)
        {
            return _unitOfWorkApplicationSearch.Application.GetWebsitesByOwner(account).Select(app => new {app.Address, app.Name, app.TrackingCode, app.Active}).ToList();
        }

        public async Task<ApplicationCreateMessage> CreateWebsite(Common.Models.Account account, Website website)
        {
            Common.Models.Applications.Application application =
                _unitOfWorkApplicationSearch.Application.GetByAddress(account, website.Address);

            if (application == null)
            {
                account = _unitOfWorkAccountSearch.Account.GetWithApplications(account);

                account.CreateWebsite(website);
                
                _unitOfWorkApplicationSearch.Save();
                
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