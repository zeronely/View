using System;
using System.Collections.Generic;
using UnityEngine;

/*本地数据存储
*/
public class PrefsAPI
{
    private const string DEFAULT_SERVER = "DefaultServer"; //设置登陆默认服务器
    //
    public const string USER_NAME = "username";  //用户名
    public const string PWD = "pwd";//用户密码
    //
    public const string LAYER_UI = "UI";
    //
    private const string BGM_VOl = "bgmVol";
    private const string SE_VOl = "seVol";
    private const string VOICE_VOL = "voiceVol";

    private const string KEY_SP_VIT_SWITCH = "KEY_SP_VIT_SWITCH";
    private const string KEY_SP_VIT_GET12_SWITCH = "KEY_SP_VIT_GET12_SWITCH";
    private const string KEY_SP_VIT_GET18_SWITCH = "KEY_SP_VIT_GET18_SWITCH";
    private const string KEEP_CAMERA_RIGHT_POS = "KEEP_CAMERA_RIGHT_POS";
    private const string KEEP_CAMERA_LEFT_POS = "KEEP_CAMERA_RIGHT_POS";

    public const int APTIME = 480;//ap回复时间，暂定8分钟

    public const string MAIN_CAMERA_POS = "Main_Camera_Pos";
    public const string MAP_CAMERA_POS = "Map_Camera_Pos";
    public const string STAGE_CHAPTERID = "Stage_Chapterid";

    #region DefaultServer
    public static int GetDefaultServer()
    {
        return PlayerPrefs.GetInt(DEFAULT_SERVER, 0);
    }

    public static void SetDefaultServer(int defaultServer)
    {
        PlayerPrefs.SetInt(DEFAULT_SERVER, defaultServer);
    }
    #endregion

    #region RoleInfo
    public static string GetUserName()
    {
        return PlayerPrefs.GetString(USER_NAME, string.Empty);
    }

    public static void SetUserName(string username)
    {
        PlayerPrefs.SetString(USER_NAME, username);
    }

    public static string GetPassWord()
    {
        return PlayerPrefs.GetString(PWD, string.Empty);
    }

    public static void SetPassWord(string pwd)
    {
        PlayerPrefs.SetString(PWD, pwd);
    }
    #endregion

    #region 音量
    public static float GetBgmVolume()
    {
        return PlayerPrefs.GetFloat(BGM_VOl, 1f);
    }

    public static void SetBgmVolume(float vol)
    {
        PlayerPrefs.SetFloat(BGM_VOl, vol);
    }

    public static float GetSeVolume()
    {
        return PlayerPrefs.GetFloat(SE_VOl, 1f);
    }

    public static void SetSeVolume(float vol)
    {
        PlayerPrefs.SetFloat(SE_VOl, vol);
    }

    public static float GetVoiceVolume()
    {
        return PlayerPrefs.GetFloat(VOICE_VOL, 1f);
    }

    public static void SetVoiceVolume(float vol)
    {
        PlayerPrefs.SetFloat(VOICE_VOL, vol);
    }
    #endregion

    #region 通知
    public static void SetSpVitSwitch(bool state)
    {
        PlayerPrefs.SetInt(KEY_SP_VIT_SWITCH, state ? 1 : 0);
    }
    public static void SetSpVitGet12Switch(bool state)
    {
        PlayerPrefs.SetInt(KEY_SP_VIT_GET12_SWITCH, state ? 1 : 0);
    }
    public static void SetSpVitGet18Switch(bool state)
    {
        PlayerPrefs.SetInt(KEY_SP_VIT_GET18_SWITCH, state ? 1 : 0);
    }

    public static bool GetSpVitSwitch()
    {
        return PlayerPrefs.GetInt(KEY_SP_VIT_SWITCH, 1) == 1;
    }

    public static bool GetSpVitGet12Switch()
    {
        return PlayerPrefs.GetInt(KEY_SP_VIT_GET12_SWITCH, 1) == 1;
    }

    public static bool GetSpVitGet18Switch()
    {
        return PlayerPrefs.GetInt(KEY_SP_VIT_GET18_SWITCH, 1) == 1;
    }
    #endregion

    public static void SetMainCameraPos(float Pos_x)
    {
        PlayerPrefs.SetFloat(MAIN_CAMERA_POS, Pos_x);
    }

    public static float GetMainCameraPos()
    {
        return PlayerPrefs.GetFloat(MAIN_CAMERA_POS, 0.0f);
    }

    public static void DeletePlayPrefs(string str)
    {
        PrefsAPI.DeletePlayPrefs(str);
    }

    public static void SetMapCameraPos(float Pos_x)
    {
        PlayerPrefs.SetFloat(MAP_CAMERA_POS, Pos_x);
    }
    public static float GetMapCameraPos()
    {
        return PlayerPrefs.GetFloat(MAP_CAMERA_POS, 135.5f);
    }

 
}

