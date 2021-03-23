using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using glytics.Common.Models.Applications;
using glytics.Common.Models.Auth;
using Isopoh.Cryptography.Argon2;

namespace glytics.Common.Models
{
    public class Account
    {
        [Key]
        public Guid Id { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        
        public List<APIKey> ApiKeys { get; set; }
        public List<Application> Applications { get; set; }
        
        public async Task<SearchResults> Search(SearchRequest searchRequest)
        {
            List<Application> _applications = new List<Application>();

            if (searchRequest.Term.Length > 1)
            {
                _applications = Applications.Where(app => app.Name.Contains(searchRequest.Term)).ToList();
            }

            SearchResults results = new SearchResults()
            {
                Results = new List<SearchResult>()
            };
            
            foreach (Application application in _applications)
            {
                results.Results.Add(new SearchResult()
                {
                    Title = application.Name,
                    Location = $"/applications/website/{application.TrackingCode}"
                });
            }

            return results;
        }

        public bool VerifyPassword(string password)
        {
            if (Argon2.Verify(Password, password))
            {
                return true;
            }
            
            return true;
        }

        public void CreateWebsite(Website website)
        {
            website.TrackingCode = Utility.GenerateTrackingCode();
            Applications.Add(website);
        }
    }
}