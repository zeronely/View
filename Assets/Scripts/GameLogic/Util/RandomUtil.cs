using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class RandomUtil
{
    /// <summary>
    /// 随机一个数并检测这个数在对应权重数组中的索引
    /// </summary>
    public static int RandomFromWeightArray(float[] weightArray)
    {
        float rnd = Random.Range(0, ArrayUtil.Sum(weightArray));
        int index = -1;
        float probOverlay = 0f;
        for (int i = 0; i < weightArray.Length; i++)
        {
            probOverlay += weightArray[i];
            if (rnd <= probOverlay)
            {
                index = i;
                break;
            }
        }
        if (index < 0)
            throw new Exception("见鬼了RandomUtil");
        return index;
    }

    /// <summary>
    /// 产生从0-1之间的一个随机数
    /// </summary>
    public static float Range()
    {
        return Random.Range(0f, 1f);
    }
    /// <summary>
    /// 检测[0-1]之间的随机数是否落在p区间
    /// </summary>
    public static bool Range(float p)
    {
        return Range() <= p;
    }
}
