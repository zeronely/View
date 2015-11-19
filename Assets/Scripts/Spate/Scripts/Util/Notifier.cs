using System;
using System.Collections.Generic;

public delegate bool Observer(object code, params object[] args);

public delegate bool Observer<T>(object code, T arg);

public delegate bool Observer<T, U>(object code, T arg1, U arg2);

public delegate bool Observer<T, U, V>(object code, T arg1, U arg2, V arg3);


/// <summary>
/// 通告者,用于"订阅"、"退订"、"发布"通告,通告接受者必须为Observer
/// <para>注意:params object[]能自动接收a,b,c...参数，同时C#编译器也能智能推导出泛型类型,但是后者优先级高,所以建议给param object[]发送明确的object[]参数</para>
/// </summary>
public sealed class Notifier
{
    private static Dictionary<object, List<Delegate>> dicObserver = new Dictionary<object, List<Delegate>>();

    #region Reg

    public static bool Reg(object code, Observer observer)
    {
        return RegDelegate(code, observer);
    }

    public static bool Reg<T>(object code, Observer<T> observer)
    {
        return RegDelegate(code, observer);
    }

    public static bool Reg<T, U>(object code, Observer<T, U> observer)
    {
        return RegDelegate(code, observer);
    }

    public static bool Reg<T, U, V>(object code, Observer<T, U, V> observer)
    {
        return RegDelegate(code, observer);
    }

    private static bool RegDelegate(object code, Delegate observer)
    {
        if (observer == null) throw new ArgumentException("observer can not be null!");
        List<Delegate> list = null;
        if (!dicObserver.TryGetValue(code, out list))
        {
            list = new List<Delegate>();
            dicObserver.Add(code, list);
        }
        bool ret = !list.Contains(observer);
        if (ret) list.Add(observer);
        return ret;
    }
    #endregion

    #region Unreg

    /// <summary>
    /// 清除所有的订阅者
    /// </summary>
    public static void Unreg()
    {
        dicObserver.Clear();
    }

    /// <summary>
    /// 移除所有的code订阅者
    /// </summary>
    public static void Unreg(object code)
    {
        dicObserver.Remove(code);
    }

    public static void Unreg(object code, Observer observer)
    {
        UnregDelegate(code, observer);
    }

    public static void Unreg<T>(object code, Observer<T> observer)
    {
        UnregDelegate(code, observer);
    }

    public static void Unreg<T, U>(object code, Observer<T, U> observer)
    {
        UnregDelegate(code, observer);
    }

    public static void Unreg<T, U, V>(object code, Observer<T, U, V> observer)
    {
        UnregDelegate(code, observer);
    }

    private static void UnregDelegate(object code, Delegate observer)
    {
        List<Delegate> list = null;
        if (!dicObserver.TryGetValue(code, out list)) return;
        list.Remove(observer);
    }

    #endregion

    #region Notify

    public static Dictionary<object, bool> tempCodeDic = new Dictionary<object, bool>();

    public static void Notify(object code, params object[] args)
    {
        if (tempCodeDic.ContainsKey(code))
            return;
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            for (int i = list.Count - 1; i >= 0; i--)
            {
                Observer observer = list[i] as Observer;
                if (observer != null) observer(code, args);
            }
        }
        tempCodeDic.Add(code, true);
    }

    public static void NotifyOrdered(object code, params object[] args)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer observer = del as Observer;
                if (observer != null)
                {
                    if (observer(code, args)) break;
                }
            }
        }
    }

    public static void Notify<T>(object code, T arg)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T> observer = del as Observer<T>;
                if (observer != null) observer(code, arg);
            }
        }
    }

    public static void NotifyOrdered<T>(object code, T arg)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T> observer = del as Observer<T>;
                if (observer != null)
                {
                    if (observer(code, arg)) break;
                }
            }
        }
    }

    public static void Notify<T, U>(object code, T arg1, U arg2)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T, U> observer = del as Observer<T, U>;
                if (observer != null) observer(code, arg1, arg2);
            }
        }
    }

    public static void NotifyOrdered<T, U>(object code, T arg1, U arg2)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T, U> observer = del as Observer<T, U>;
                if (observer != null)
                {
                    if (observer(code, arg1, arg2)) break;
                }
            }
        }
    }

    public static void Notify<T, U, V>(object code, T arg1, U arg2, V arg3)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T, U, V> observer = del as Observer<T, U, V>;
                if (observer != null) observer(code, arg1, arg2, arg3);
            }
        }
    }

    public static void NotifyOrdered<T, U, V>(object code, T arg1, U arg2, V arg3)
    {
        List<Delegate> list = null;
        if (dicObserver.TryGetValue(code, out list))
        {
            foreach (Delegate del in list)
            {
                Observer<T, U, V> observer = del as Observer<T, U, V>;
                if (observer != null)
                {
                    if (observer(code, arg1, arg2, arg3)) break;
                }
            }
        }
    }

    #endregion
}
