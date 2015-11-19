using System;

namespace Spate
{
    public static class NetHost
    {
        public static string HostUrl { get; private set; }

        public static void SetHost(string host)
        {
            // http://192.168.1.100:9000
            if (host.EndsWith("/"))
                host.Substring(0, host.Length - 1);
            HostUrl = host;
        }
    }
}
