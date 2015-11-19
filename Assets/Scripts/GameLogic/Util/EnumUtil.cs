using System;


public sealed class EnumUtil
{
    /// <summary>
    /// 在flag的基础上附加vt表示的特性
    /// </summary>
    public static void AddFlag(ref int flag, ValueType vt)
    {
        flag |= (1 << (int)vt);
    }

    /// <summary>
    /// 检测flag中是否包含有指定的vt特性
    /// </summary>
    public static bool CheckFlag(int flag, ValueType vt)
    {
        return (flag & (1 << (int)vt)) != 0;
    }

    /// <summary>
    /// 移除flag中指定的vt特性
    /// </summary>
    public static void RemoveFlag(ref int flag, ValueType vt)
    {
        if (CheckFlag(flag, vt))
        {
            flag &= ~(1 << (int)vt);
        }
    }
}
