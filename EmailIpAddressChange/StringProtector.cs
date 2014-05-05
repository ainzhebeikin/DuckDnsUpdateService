using System;
using System.Security.Cryptography;
using System.Text;

namespace EmailIpAddressChange
{
    internal class StringProtector
    {
        private static readonly byte[] Entropy = {0xF4, 0x16, 0xB9, 0x82, 0xD1, 0x3A, 0x42, 0x36, 0xB0, 0x52, 0x11, 0xDB, 0xC5, 0x77, 0x9C, 0x16};

        public static string UnprotectString(string value)
        {
            return Encoding.UTF8.GetString(ProtectedData.Unprotect(Convert.FromBase64String(value), Entropy, DataProtectionScope.CurrentUser));
        }

        public static string ProtectString(string value)
        {
            return Convert.ToBase64String(ProtectedData.Protect(Encoding.UTF8.GetBytes(value), Entropy, DataProtectionScope.CurrentUser));
        }
    }
}