using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 声音管理
    /// </summary>
    public sealed class SoundManager : BaseManager
    {
        // 单例
        private static BgmPlayer mBgmPlayer;
        // 缓存池控制
        private static SePlayer mSePlayer;
        // VoicePlayer
        private static VoicePlayer mVoicePlayer;

        static SoundManager()
        {
            // 确保挂载了AudioListener
            GameObject go = Engine.Inst.CachedGameObject;
            if (go.GetComponent<AudioListener>() == null)
                go.AddComponent<AudioListener>();
        }

        /// <summary>
        /// 获取背景音乐播放器
        /// </summary>
        public static BgmPlayer GetBgmPlayer()
        {
            if (mBgmPlayer == null)
                mBgmPlayer = new BgmPlayer(PrefsAPI.GetBgmVolume());
            return mBgmPlayer;
        }

        /// <summary>
        /// 获取音效播放器
        /// </summary>
        public static SePlayer GetSePlayer()
        {
            if (mSePlayer == null)
                mSePlayer = new SePlayer(PrefsAPI.GetSeVolume());
            return mSePlayer;
        }

        public static VoicePlayer GetVoicePlayer()
        {
            if (mVoicePlayer == null)
                mVoicePlayer = new VoicePlayer(PrefsAPI.GetVoiceVolume());
            return mVoicePlayer;
        }

        public override void OnUpdate()
        {
            if (mBgmPlayer != null)
                mBgmPlayer.Update();
            if (mSePlayer != null)
                mSePlayer.Update();
        }

        public override void OnApplicationPause(bool pause)
        {
            if (pause)
            {
                if (mBgmPlayer != null)
                    mBgmPlayer.Pause();
                if (mSePlayer != null)
                    mSePlayer.Pause();
                if (mVoicePlayer != null)
                    mVoicePlayer.Pause();
            }
            else
            {
                if (mBgmPlayer != null)
                    mBgmPlayer.Resume();
                if (mSePlayer != null)
                    mSePlayer.Resume();
                if (mVoicePlayer != null)
                    mVoicePlayer.Resume();
            }
        }

        public static AudioSource NewAudioSource()
        {
            GameObject go = Engine.Inst.CachedGameObject;
            AudioSource source = go.AddComponent<AudioSource>();
            source.mute = false;
            source.playOnAwake = false;
            return source;
        }
    }
}
