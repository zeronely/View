using System;

namespace Spate
{
    /// <summary>
    /// 仅用于标识当前的数据为CSVData
    /// </summary>
    public abstract class BaseCsvData : BaseData
    {
        public override object PrimaryKey
        {
            get { return GetHashCode(); }
        }
    }
}