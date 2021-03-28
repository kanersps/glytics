using System;
using System.Collections.Generic;
using glytics.Common.Models.Applications;
using NUnit.Framework;

namespace glytics.Tests
{
    public class ApplicationTests
    {
        private Application _application;

        private long[] _range = new[]
        {
            new DateTimeOffset(new DateTime(2000, 9, 20)).ToUnixTimeMilliseconds(),
            new DateTimeOffset(new DateTime(2000, 10, 20)).ToUnixTimeMilliseconds()
        };
        
        [SetUp]
        public void InitializeTests()
        {
            _application = new Application()
            {
                Active = true,
                Address = "example.com",
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
            };
        }

        [Test]
        public void TestGetDetailsAddress()
        {
            WebsiteDetails details = _application.GetDetails(_range);
            
            Assert.AreEqual(details.Address, "https://example.com");
        }

        [Test]
        public void TestGetDetailsHourly()
        {
            WebsiteDetails details = _application.GetDetails(_range);
            
            Assert.AreEqual(details.Hourly.Count, 1);
        }

        [Test]
        public void TestGetDetailsBrowsers()
        {
            WebsiteDetails details = _application.GetDetails(_range);
            
            Assert.AreEqual(details.HourlyBrowsers.Count, 1);
        }

        [Test]
        public void TestGetDetailsPaths()
        {
            WebsiteDetails details = _application.GetDetails(_range);
            
            Assert.AreEqual(details.HourlyPaths.Count, 1);
        }

        [Test]
        public void TestGetDetailsName()
        {
            WebsiteDetails details = _application.GetDetails(_range);
            
            Assert.AreEqual(details.Name, "example.com");
        }

        [Test]
        public void TestActivate()
        {
            _application.Activate();
            
            Assert.IsTrue(_application.Active);
        }

        [Test]
        public void TestDeactivate()
        {
            _application.Deactivate();
            
            Assert.IsFalse(_application.Active);
        }

        [Test]
        public void TestGenerateTrackingJavascript()
        {
            Assert.IsTrue(_application.GenerateTrackingJavascript().Contains("abcdefg"));
        }

        [Test]
        public void TestGetWebsiteLastHourViews()
        {
            Assert.AreEqual(_application.GetWebsite().LastHourViews, 10);
        }

        [Test]
        public void TestGetWebsiteLastHourVisitors()
        {
            Assert.AreEqual(_application.GetWebsite().LastHourVisitors, 10);
        }
    }
}