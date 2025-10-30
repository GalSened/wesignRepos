using System;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;

namespace Common.Extensions
{
    public static class StringExtentions
    {
        public static string ToHashString(this string value)
        {

            if (string.IsNullOrWhiteSpace(value))
            {
                return "";
            }
            StringBuilder sb = new StringBuilder();
            foreach (byte b in GetHash(value.Trim()))
                sb.Append(b.ToString("X2"));

            return sb.ToString();
        }


        private static byte[] GetHash(string inputString)
        {
            using (HashAlgorithm algorithm = SHA256.Create())
                return algorithm.ComputeHash(Encoding.UTF8.GetBytes(inputString));
        }
    }
}
