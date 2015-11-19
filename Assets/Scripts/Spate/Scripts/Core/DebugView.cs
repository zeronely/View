using UnityEngine;
using System.Collections;

/// <summary>
/// 显示FPS和内存信息
/// </summary>
public class DebugView : MonoBehaviour
{
    private int mFrames;
    private float mTimeLeft;
    private float mAccum;
    private string mFPS;

    void Update()
    {
        // 计算FPS
        mTimeLeft -= Time.deltaTime;
        mAccum += Time.timeScale / Time.deltaTime;
        mFrames++;
        if (mTimeLeft <= 0f)
        {
            mFPS = (mAccum / mFrames).ToString("f2");
            mTimeLeft = 0.5f;
            mAccum = 0f;
            mFrames = 0;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10, Screen.height - 70, 150, 60));
        GUI.contentColor = Color.red;
        GUI.backgroundColor = Color.black;
        GUILayout.Box("", GUILayout.Width(150), GUILayout.Height(60));
        // 显示FPS
        GUI.Label(new Rect(0, 0, 100, 30), string.Format("FPS:{0}", mFPS));
        // 显示MEM
        GUI.Label(new Rect(0, 30, 100, 30), string.Format("Mem:{0}MB", GetUsedMemoryMB().ToString("f2")));
        GUILayout.EndArea();
    }

    private float GetUsedMemoryMB()
    {
        return (Profiler.usedHeapSize * 1.0f / 1024) / 1024;
    }
}
