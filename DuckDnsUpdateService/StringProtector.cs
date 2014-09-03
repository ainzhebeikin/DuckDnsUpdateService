using System;
using System.Security.Cryptography;
using System.Text;

namespace DuckDnsUpdateService
{
    internal static class StringProtector
    {
        private static readonly byte[] Entropy = {0xF4, 0x16, 0xB9, 0x82, 0xD1, 0x3A, 0x42, 0x36, 0xB0, 0x52, 0x11, 0xDB, 0xC5, 0x77, 0x9C, 0x16};

        public static bool IsProtected(this string value)
        {
            try
            {
                value.Unprotect();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static string Unprotect(this string value)
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value), Entropy, DataProtectionScope.CurrentUser));
        }

        public static string Protect(this string value)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Entropy, DataProtectionScope.CurrentUser));
        }
    }
}