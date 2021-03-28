using System;
using NUnit.Framework;
using glytics.Logic;

namespace glytics.Tests
{
    public class UtilityTest
    {
        [Test]
        public void TestDateTimeRounding()
        {
            Assert.IsTrue(Utility.RoundTimeHour(new DateTime(2001, 4, 13, 10, 16, 22)) == new DateTime(2001, 4, 13, 10, 0, 0));
        }
    }
}