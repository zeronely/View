using UnityEngine;
using System.Collections;
using System.Collections.Generic;

/// <summary>
/// 扩展方法集合
/// </summary>
public static class Extension
{
    /// <summary>
    /// 扩展WWW,判定www对象是否包含错误信息
    /// </summary>
    public static bool HasError(this WWW www)
    {
        return !string.IsNullOrEmpty(www.error);
    }

    /// <summary>
    /// 扩展字符串的ToUpper方法，增加index和count参数
    /// </summary>
    public static string ToUpper(this string src, int index, int count)
    {
        char[] array = src.ToCharArray();
        for (int i = index, len = index + count; i < len; i++)
        {
            array[i] = char.ToUpper(array[i]);
        }
        return new string(array);
    }
    /// <summary>
    /// 扩展字符串的ToLower方法，增加index和count参数
    /// </summary>
    public static string ToLower(this string src, int index, int count)
    {
        char[] array = src.ToCharArray();
        for (int i = index, len = index + count; i < len; i++)
        {
            array[i] = char.ToLower(array[i]);
        }
        return new string(array);
    }

    /// <summary>
    /// 扩展IDictionary,获取第一个Pair的Key
    /// </summary>
    public static object FirstKey(this IDictionary dic)
    {
        IDictionaryEnumerator erator = dic.GetEnumerator();
        erator.MoveNext();
        return erator.Key;
    }

    /// <summary>
    /// 扩展IDictionary,获取第一个Pair的Value
    /// </summary>
    public static object FirstValue(this IDictionary dic)
    {
        IDictionaryEnumerator erator = dic.GetEnumerator();
        erator.MoveNext();
        return erator.Value;
    }
}
