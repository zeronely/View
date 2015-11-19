using UnityEngine;

public static class NetCounter
{
    private static int sequence = 0;

    public static void Reset()
    {
        sequence = 0;
    }
    public static int AutoIncrement()
    {
        // 是否要检测当前请求列表中还存在sequence为0的请求
        if (sequence == int.MaxValue) sequence = 0;
        return ++sequence;
    }
}
