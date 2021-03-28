using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using glytics.Common.Models;
using glytics.Common.Models.Applications;
using glytics.Logic;
using Isopoh.Cryptography.Argon2;
using NUnit.Framework;

namespace glytics.Tests
{
    public class AccountTests
    {
        private Account _testAccount;

        [SetUp]
        public void InitializeTests()
        {
            _testAccount = new Account()
            {
                Password = Argon2.Hash("P@ssw0rd"),
                Id = Guid.Empty,
                Applications = new List<Application>()
                {
                    new Application()
                    {
                        Active = true,
                        Address = "http://example.com",
                        TrackingCode = "abcdefg",
                        Type = "web",
                        Name = "example.com",
                        Statistic = new List<ApplicationStatistic>()
                        {
                            new ApplicationStatistic()
                            {
                                PageViews = 10,
                                Visits = 10,
                                Timestamp = new DateTime(2000, 10, 1),
                            }
                        },
                        BrowserStatistic = new List<ApplicationStatisticBrowser>(){
                            new ApplicationStatisticBrowser()
                            {
                                PageViews = 10,
                                Visits = 10,
                                Timestamp = new DateTime(2000, 10, 1),
                                Browser = "TestBrowser"
                            }
                        },
                        PathStatistic = new List<ApplicationStatisticPath>(){
                            new ApplicationStatisticPath()
                            {
                                PageViews = 10,
                                Visits = 10,
                                Timestamp = new DateTime(2000, 10, 1),
                                Path = "/test/example"
                            }
                        }
                    }
                }
            };
        }
        
        [Test]
        public void TestAccountPasswordCorrect()
        {
            Assert.IsTrue(_testAccount.VerifyPassword("P@ssw0rd"));
        }
        
        [Test]
        public void TestAccountPasswordIncorrect()
        {
            Assert.IsFalse(_testAccount.VerifyPassword("NotTheP@ssw0rd"));
        }
        
        [Test]
        public async Task TestAccountApplicationSearch()
        {
            SearchResults results = await _testAccount.Search(new SearchRequest()
            {
                Term = "example.com"
            });

            Assert.AreEqual(results.Results.Count, 1);
        }
        
        [Test]
        public async Task TestAccountApplicationSearchFuzzy()
        {
            SearchResults results = await _testAccount.Search(new SearchRequest()
            {
                Term = "example"
            });

            Assert.AreEqual(results.Results.Count, 1);
        }
        
        [Test]
        public async Task TestAccountApplicationSearchNone()
        {
            SearchResults results = await _testAccount.Search(new SearchRequest()
            {
                Term = "exampledoesntexist"
            });

            Assert.AreEqual(results.Results.Count, 0);
        }
        
        [Test]
        public async Task TestAccountJwt()
        {
            string jwt = _testAccount.GenerateJwt();

            string uuid = Utility.ValidateJwt(jwt);
            
            Assert.AreEqual(uuid, Guid.Empty.ToString());
        }
        
        [Test]
        public async Task TestAccountJwtIncorrect()
        {
            _testAccount.Id = Guid.NewGuid();
            
            string jwt = _testAccount.GenerateJwt();

            string uuid = Utility.ValidateJwt(jwt);
            
            Assert.AreNotEqual(uuid, Guid.Empty.ToString());
        }
        
        [Test]
        public async Task TestAccountCreateWebsite()
        {
            _testAccount.CreateWebsite(new Website()
            {
                Active = true,
                Name = "test2",
                Address = "http://test2.com"
            });
            
            Assert.AreEqual(_testAccount.Applications.Count, 2);
        }
    }
}