using System;

namespace Spate
{
    /// <summary>
    /// 写在SvrData中用来表示关联CsvData的主键的名称(CsvData中的字段名)
    /// </summary>
    public sealed class CsvForeginKeyAttribute : Attribute
    {
        private string mForeginKey;

        public CsvForeginKeyAttribute(string foreginKey)
        {
            if (string.IsNullOrEmpty(foreginKey))
                throw new Exception("foreginKey value can not be null or empty!");
            mForeginKey = foreginKey;
        }

        public string Value
        {
            get
            {
                return mForeginKey;
            }
        }
    }
}
