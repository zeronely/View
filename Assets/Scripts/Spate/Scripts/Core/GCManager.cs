using System;
using UnityEngine;

public static class GCManager
{
    public static void GC()
    {
        Resources.UnloadUnusedAssets();
        System.GC.Collect();
        //System.GC.WaitForPendingFinalizers();
        //System.GC.Collect();
    }
}
