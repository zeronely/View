using System;
using UnityEngine;
using System.Collections;
using Spate;
using System.IO;

public static class SceneAPI
{
    public const string SCENE_LAUNCHER = "launcher";
    public const string SCENE_UPDATE = "update";
    public const string SCENE_BATTLE = "battle";

    public static void BegineLauncher(Action onFadeFinish)
    {
        BegineUnityScene(SCENE_LAUNCHER, onFadeFinish);
    }

    public static void DirectUpdate()
    {
        Application.LoadLevel(SCENE_UPDATE);
    }

    public static void BeginUpdate(Action onFadeFinish)
    {
        BegineUnityScene(SCENE_UPDATE, onFadeFinish);
    }

    // 加载c_main资源
    public static void BegineGame(Action onSceneLoaded)
    {
        BegineAssetScene("c_main", () =>
        {
            // 不要直接查找Find("/main"),因为有NGUI Atlas也叫main的隐藏GameObject(UIDrawCall)
            //GameObject go = GameObject.Find("/main/mainCamera");
            //if (go != null)
            //    go.transform.parent.gameObject.AddComponent<GameLauncher>();
            // 
            WindowManager.ChangeUICameraFlags(CameraClearFlags.Depth);
            if (onSceneLoaded != null)
                onSceneLoaded();
        });
    }
    // 加载c_worldmap资源
    public static void BegineWorldMap(Action onSceneLoaded)
    {
        BegineAssetScene("c_worldmap", () =>
        {
            //GameObject go = GameObject.Find("/c_worldmap");
            //if (go != null)
            //    go.AddComponent<MapLauncher>();

            WindowManager.ChangeUICameraFlags(CameraClearFlags.Depth);
            //LoadingWindow.Stop();
            if (onSceneLoaded != null)
                onSceneLoaded();
        });
    }
    // c_yanwucheng
    public static void BegineChapter(string chapterSceneName, Action onSceneLoaded)
    {
        BegineAssetScene(chapterSceneName, () =>
        {
            //GameObject go = GameObject.Find("/" + chapterSceneName);
            //if (go != null)
            //    go.AddComponent<ChapterLauncher>();

            WindowManager.ChangeUICameraFlags(CameraClearFlags.Depth);
            //LoadingWindow.Stop();
            if (onSceneLoaded != null)
                onSceneLoaded();
        });
    }

    // c_001
    public static void BegineBattle(string battleScene, Action onSceneLoaded)
    {
        BegineAssetScene(battleScene, () =>
        {
            WindowManager.ChangeUICameraFlags(CameraClearFlags.Depth);
            if (onSceneLoaded != null)
                onSceneLoaded();
        });
    }

    public static void BegineUnityScene(string level, Action onFadeFinish)
    {
        if (mBundle != null)
            TryUnloadBundle();
        //LoadingWindow.Start(() =>
        //{
        //    if (onFadeFinish != null) onFadeFinish();
        //    Application.LoadLevelAsync(level);
        //});
    }

    public static void BegineAssetScene(string sceneName, Action onSceneLoaded)
    {
        //LoadingWindow.Start(() =>
        //{
        //    // 已FadeIn，在这里做AssetBundle构建
        //    AsyncManager.StartCoroutine(LoadAssetScene(sceneName, onSceneLoaded));
        //});
    }

    private static IEnumerator LoadAssetScene(string sceneName, Action onSceneLoaded)
    {
        if (mBundle != null)
        {
            TryUnloadBundle();
            yield return 1;
        }
        string scenePath = UrlUtil.Combine(Settings.UNITY_RAW_FOLDER, UrlUtil.Combine("scene/", sceneName + ".ab"));
        if (!File.Exists(scenePath))
            throw new Exception("指定场景资源不存在:" + scenePath);
        mBundle = AssetBundle.CreateFromFile(scenePath);
        Application.LoadLevel(sceneName);
        Resources.UnloadUnusedAssets();
        yield return 1;
        WindowManager.ClearGhost();
        onSceneLoaded();
    }

    private static bool TryUnloadBundle()
    {
        if (mBundle == null)
            return false;
        mBundle.Unload(true);
        mBundle = null;
        return true;
    }

    private static AssetBundle mBundle;
}
