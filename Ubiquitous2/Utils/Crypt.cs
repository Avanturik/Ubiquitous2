using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;

namespace UB.Utils
{
    public static class Crypt
    {
        public static string GetSHA256Hash( string input )
        {
            SHA256 sha256 = SHA256Managed.Create();
            byte[] bytes = System.Text.Encoding.Default.GetBytes(input);
            return Encoding.Default.GetString( sha256.ComputeHash(bytes));
        }

        public static string Base64Encode(this string input)
        {
            var plainTextBytes = System.Text.Encoding.Default.GetBytes(input);
            return System.Convert.ToBase64String(plainTextBytes);
        }
    }
}
