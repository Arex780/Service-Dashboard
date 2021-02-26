using System;
using System.Net;

namespace WebArchive.Utils
{
    public static class DnsHandler
    {
        public static string GetHostIPAddress()
        {
            return Dns.GetHostEntry(Dns.GetHostName()).AddressList[0].ToString();
        }

        public static string GetHostDNSAddress()
        {
            if (!string.IsNullOrEmpty(Environment.GetEnvironmentVariable("WEBSITE_DNS")))
            {
                return Environment.GetEnvironmentVariable("WEBSITE_DNS");
            }
            else
            {
                return GetHostIPAddress();
            }
        }
    }
}
