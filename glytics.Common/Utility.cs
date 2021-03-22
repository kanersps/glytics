using System;
using System.Security.Cryptography;

namespace glytics.Common
{
    public static class Utility
    {
        public static string GenerateTrackingCode()
        {
            byte[] bytes = new byte[4];
            RandomNumberGenerator rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);

            uint uniqueIDs = BitConverter.ToUInt32(bytes, 0) % 100000000;

            return $"GL-{uniqueIDs}";
        }
    }
}