using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic
{
    public static class CryptoHelpers
    {
        public static string Encode(string value)
        {
            SHA1 hash = SHA1.Create();
            var encoder = new ASCIIEncoding();
            byte[] combined = encoder.GetBytes(value ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }
    }
}