using System;

namespace Spate
{
    public static class ResHost
    {
        private static string mHost;
        private static string mVersion;
        private static string mPlatform;

        public static string HostUrl { get; private set; }

        public static bool FromLogicServer { get; private set; }

        public static void SetHost(string host)
        {
            // http://192.168.1.100:8080/Supreme
            FromLogicServer = (host == null);
            if (FromLogicServer)
                return;
            if (host.EndsWith("/"))
                host = host.Substring(0, host.Length - 1);
            mHost = host;
        }

        public static void SetPlatform(string platform)
        {
            if (platform.StartsWith("/"))
                platform = platform.Substring(1);
            if (platform.EndsWith("/"))
                platform = platform.Substring(0, platform.Length - 1);
            mPlatform = platform;
        }

        public static void SetVersion(string version)
        {
            if (version.StartsWith("/"))
                version = version.Substring(1);
            if (version.EndsWith("/"))
                version = version.Substring(0, version.Length - 1);
            mVersion = version;

            BuildHostUrl();
        }

        public static void BuildHostUrl()
        {
            HostUrl = string.Format("{0}/{1}/{2}", mHost, mPlatform, mVersion);
        }


        // 是否使用Csv作为GameData来源?(仅Editor环境下有效)
        public static bool UseCsv { get; private set; }

        public static void SetUseCsv(bool useCsv)
        {
            UseCsv = useCsv;
        }
    }
}
