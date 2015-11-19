using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Spate;

public static class StringUtil
{
    public static void RemoveClone(GameObject go)
    {
        go.name = go.name.Replace("(Clone)", "");
    }

    public static bool CheckString(string effect)
    {
        return !string.IsNullOrEmpty(effect) && !string.Equals(effect.Trim(), "0");
    }

    public static bool IsChineseChar(char ch)
    {
        int code = char.ConvertToUtf32(ch.ToString(), 0);
        return code >= 0x4e00 && code <= 0x9fff;
    }

    public static int StringToInt(string str)
    {
        if (string.IsNullOrEmpty(str))
            return 0;
        else
            return int.Parse(str);
    }

    public static string GetRatioOrValueText(float ratio)
    {
        string str = "";
        if (ratio < 1)
        {
            str = ratio * 100 + "%";
        }
        else
        {
            str = ratio.ToString();
        }
        return str;
    }

    public static int GetNguiLayer()
    {
        return LayerMask.NameToLayer(PrefsAPI.LAYER_UI);
    }
    //
    public static int GetStringLength(string str)
    {
        string temp = str;
        int j = 0;
        for (int i = 0; i < temp.Length; i++)
        {
            if (Regex.IsMatch(temp.Substring(i, 1), @"[\u4e00-\u9fa5]+"))
            {
                j += 2;
            }
            else
            {
                j += 1;
            }

        }
        return j;
    }
    public static string GetStringUTF8(string name)
    {
        byte[] buffer = System.Text.Encoding.UTF8.GetBytes(name);
        string str = string.Empty;
        foreach (byte b in buffer)
        {
            str += string.Format("%{0:X}", b);
        }
        return str;
    }
    //
    public static DateTime TransStringToDateTimeSecond(string str)
    {
        if (str.Trim().Equals("0"))
        {
            //Log.Log_hjx("str :" + str + ":");
            return DateTime.MinValue;
        }
        //Log.Log_hjx("str " + str);
        int year = int.Parse(str.Substring(0, 4));
        int month = int.Parse(str.Substring(4, 2));
        int day = int.Parse(str.Substring(6, 2));
        int hours = int.Parse(str.Substring(8, 2));
        int minute = int.Parse(str.Substring(10, 2));
        int second = int.Parse(str.Substring(12, 2));
        //Log.Log_hjx("year " + year + "month " + month + "day " + day + " hours " + hours + " minute " + minute + "second " + second);
        DateTime d = new DateTime(year, month, day, hours, minute, second, DateTimeKind.Utc);
        return d;
    }

    public static DateTime TransStringToDateTimeMinute(string str)
    {
        if (string.IsNullOrEmpty(str) || str.Trim().Equals("0"))
        {
            //Log.Log_hjx("str :" + str + ":");
            return DateTime.MinValue;
        }
        int year = int.Parse(str.Substring(0, 4));
        int month = int.Parse(str.Substring(4, 2));
        int day = int.Parse(str.Substring(6, 2));
        int hours = int.Parse(str.Substring(8, 2));
        int minute = int.Parse(str.Substring(10, 2));
        //Log.Log_hjx("year " + year + "month " + month + "day " + day + " hours " + hours + " minute " + minute);
        DateTime d = new DateTime(year, month, day, hours, minute, 0, DateTimeKind.Utc);
        return d;
    }

    public static string TransDateTimeSecondToString(DateTime time)
    {
        string str = "";
        //if(time != null)
        //{
        str = time.Year + "";
        str += time.Month < 10 ? "0" + time.Month : time.Month + "";
        str += time.Day < 10 ? "0" + time.Day : time.Day + "";
        str += time.Hour < 10 ? "0" + time.Hour : time.Hour + "";
        str += time.Minute < 10 ? "0" + time.Minute : time.Minute + "";
        str += time.Second < 10 ? "0" + time.Second : time.Second + "";
        //}
        return str;
    }

    public static string TransStringToTimeSecondString(string str)
    {
        if (str.Trim().Equals("0"))
        {
            //Log.Log_hjx("str :" + str + ":");
            return "";
        }
        string year = str.Substring(0, 4);
        string month = str.Substring(4, 2);
        string day = str.Substring(6, 2);
        string hours = string.Format("{0:D2}", str.Substring(8, 2));
        string minute = string.Format("{0:D2}", str.Substring(10, 2));
        string second = string.Format("{0:D2}", str.Substring(12, 2));
        //Log.Log_hjx("year " + year + "month " + month + "day " + day + " hours " + hours + " minute " + minute);
        String timestr = year + "." + month + "." + day + "  " + hours + ":" + minute + ":" + second;
        return timestr;
    }
    public static string TransStringToHourMinuSecondString(string str)
    {
        if (str.Trim().Equals("0"))
        {
            return "";
        }
        string hours = string.Format("{0:D2}", str.Substring(8, 2));
        string minute = string.Format("{0:D2}", str.Substring(10, 2));
        string second = string.Format("{0:D2}", str.Substring(12, 2));
        String timestr = hours + ":" + minute + ":" + second;
        return timestr;
    }

    /// <summary>
    /// str 时钟和分钟
    /// </summary>
    public static DateTime TransStringToHourMinuSecond(string str)
    {
        if (str.Trim().Equals("0"))
        {
            return DateTime.MinValue;
        }
        DateTime curTime = AlarmManager.GetCurrentDateTime();
        string time = curTime.ToString("yyyyMMdd") + str + "00";
        return TransStringToDateTimeSecond(time);
    }

    public static string TransSecondToStringTime(long t, bool showhour = false)
    {
        if (t > 0)
        {
            int hour = 0;
            int minu = 0;
            int second = 0;
            hour = (int)(t / 3600);
            minu = (int)((t % 3600) / 60);
            second = (int)(t % 60);
            string HOUR = hour > 9 ? hour.ToString() : ("0" + hour);
            string MINU = minu > 9 ? minu.ToString() : ("0" + minu);
            string SEC = second > 9 ? second.ToString() : ("0" + second);
            if (showhour)
            {
                return HOUR + ":" + MINU + ":" + SEC;
            }
            else
            {
                return MINU + ":" + SEC;
            }
        }
        else
        {
            return "";
        }
    }

    public static void ParseArgs<T>(string args, ref T val1)
    {
        string[] array = args.Split(';');
        val1 = (T)Convert.ChangeType(array[0], typeof(T));
    }
    public static void ParseArgs<T, U>(string args, ref T val1, ref U val2)
    {
        string[] array = args.Split(';');
        val1 = (T)Convert.ChangeType(array[0], typeof(T));
        val2 = (U)Convert.ChangeType(array[1], typeof(U));
    }
    public static void ParseArgs<T, U, V>(string args, ref T val1, ref U val2, ref V val3)
    {
        string[] array = args.Split(';');
        val1 = (T)Convert.ChangeType(array[0], typeof(T));
        val2 = (U)Convert.ChangeType(array[1], typeof(U));
        val3 = (V)Convert.ChangeType(array[2], typeof(V));
    }
    public static void ParseArgs<T, U, V, Q>(string args, ref T val1, ref U val2, ref V val3, ref Q val4)
    {
        string[] array = args.Split(';');
        val1 = (T)Convert.ChangeType(array[0], typeof(T));
        val2 = (U)Convert.ChangeType(array[1], typeof(U));
        val3 = (V)Convert.ChangeType(array[2], typeof(V));
        val4 = (Q)Convert.ChangeType(array[3], typeof(Q));
    }
    public static void ParseArgs<T, U, V, Q, W>(string args, ref T val1, ref U val2, ref V val3, ref Q val4, ref W val5)
    {
        string[] array = args.Split(';');
        val1 = (T)Convert.ChangeType(array[0], typeof(T));
        val2 = (U)Convert.ChangeType(array[1], typeof(U));
        val3 = (V)Convert.ChangeType(array[2], typeof(V));
        val4 = (Q)Convert.ChangeType(array[3], typeof(Q));
        val5 = (W)Convert.ChangeType(array[4], typeof(W));
    }
}
