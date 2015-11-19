using System;
using System.Text;
using System.Collections.Generic;

public static class ArrayUtil
{
    /// <summary>
    /// 获取数组的元素总和
    /// </summary>
    public static float Sum(float[] array)
    {
        float sum = 0f;
        foreach (float f in array)
        {
            sum += f;
        }
        return sum;
    }

    /// <summary>
    /// 检查制定元素是否在指定数组中
    /// </summary>
    public static bool Exists(int id, int[] array)
    {
        bool ret = false;
        foreach (int tmp in array)
        {
            if (tmp == id)
            {
                ret = true;
                break;
            }
        }
        return ret;
    }

    public static void SetAllInt(int[] array, int value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static bool IsAll(int[] array, int value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != value)
                return false;
        }
        return true;
    }

    public static void SetAllBool(bool[] array, bool value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            array[i] = value;
        }
    }

    public static bool IsAll(bool[] array, bool value)
    {
        for (int i = 0; i < array.Length; i++)
        {
            if (array[i] != value)
                return false;
        }
        return true;
    }

    public static string ToString(Array array, char separator = ';')
    {
        if (array == null || array.Length == 0)
            return string.Empty;
        StringBuilder sb = new StringBuilder();
        int len = array.Length;
        for (int i = 0; i < len; i++)
        {
            if (sb.Length > 0)
                sb.Append(separator);
            sb.Append(array.GetValue(i) + "");
        }
        return sb.ToString();
    }
}
