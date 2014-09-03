using System;
using System.Net;

namespace EmailIpAddressChange
{
    internal class DuckDnsUpdater
    {
        public static bool Update(string domain, string token, string address)
        {
            try
            {
                using (var client = new WebClient())
                {
                    var request = string.Format("https://www.duckdns.org/update?domains={0}&token={1}&ip={2}", domain, token, address);
                    var response = client.DownloadString(request);
                    return string.Equals("OK", response, StringComparison.OrdinalIgnoreCase);
                }
            }
            catch
            {
                return false;
            }
        }
    }
}