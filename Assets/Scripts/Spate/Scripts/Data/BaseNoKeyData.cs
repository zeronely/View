using System;
using Spate;
/// <summary>
/// 无主键数据的基类,无主键就采用整个对象的HashCode作为主键
/// </summary>
public abstract class BaseNoKeyData : BaseData
{
    public override object PrimaryKey
    {
        get { return GetHashCode(); }
    }

    /// <summary>
    /// 无主键，但可并列存储
    /// </summary>
    public virtual bool NoKeyButList
    {
        get { return false; }
    }
}
