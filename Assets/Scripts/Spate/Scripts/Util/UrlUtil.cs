using System;

namespace Spate
{
    public sealed class UrlUtil
    {
        public static string Combine(string url1, string url2)
        {
            if (!url1.EndsWith("/"))
                url1 = url1 + "/";
            if (url2.StartsWith("/"))
                url2 = url2.Substring(1);
            return url1 + url2;
        }
    }
}
