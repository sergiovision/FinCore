using System;
using System.Security.Cryptography;
using System.Text;

namespace BusinessLogic
{
    public static class CryptoHelpers
    {
        public static string Encode(string value)
        {
            var hash = SHA1.Create();
            var encoder = new ASCIIEncoding();
            var combined = encoder.GetBytes(value ?? "");
            return BitConverter.ToString(hash.ComputeHash(combined)).ToLower().Replace("-", "");
        }
    }
}