using System;
using System.Security.Cryptography;

namespace glytics.Logic
{
    public static class Utility
    {
        public static DateTime RoundTimeHour(DateTime time)
        {
            return new(time.Year, time.Month, time.Day, time.Hour, 0, 0);
        }
    }
}