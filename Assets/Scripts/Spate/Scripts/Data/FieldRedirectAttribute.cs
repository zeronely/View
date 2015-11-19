using System;

namespace Spate
{
    [AttributeUsage(AttributeTargets.Field, Inherited = false)]
    public sealed class FieldRedirectAttribute : Attribute
    {
        private string mRedirect = "";
        public FieldRedirectAttribute(string redirect)
        {
            if (string.IsNullOrEmpty(redirect))
                throw new Exception("redirect value can not be null or empty!");
            mRedirect = redirect;
        }

        public string Value
        {
            get
            {
                return mRedirect;
            }
        }
    }
}
