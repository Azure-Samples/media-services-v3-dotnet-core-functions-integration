//
// Azure Media Services REST API v3 - Functions
//
// Shared Library
//

using System;

namespace advanced_vod_functions_v3.SharedLibs
{
    class GenericHelper
    {
        public static byte[] GetRandomBuffer(int length)
        {
            var returnValue = new byte[length];

            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(returnValue);
            }

            return returnValue;
        }
    }
}
