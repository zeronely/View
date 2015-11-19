using UnityEngine;
using System.Collections.Generic;
using Spate;
using System;

public class SortUtil
{
    /// <summary>
    /// 降序
    /// </summary>
    public static int CompareDescending(int a, int b)
    {
        if (a > b)
        {
            return -1;
        }
        else if (a < b)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
    /// <summary>
    /// 升序
    /// </summary>
    public static int CompareAscending(int a, int b)
    {
        if (a > b)
        {
            return 1;
        }
        else if (a < b)
        {
            return -1;
        }
        else
        {
            return 0;
        }
    }

    /// <summary>
    /// 降序long
    /// </summary>
    public static int CompareLongDescending(long a, long b)
    {
        if (a > b)
        {
            return -1;
        }
        else if (a < b)
        {
            return 1;
        }
        else
        {
            return 0;
        }
    }
}
