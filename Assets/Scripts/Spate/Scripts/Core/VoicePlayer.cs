using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 语音播放器
    /// </summary>
    public sealed class VoicePlayer
    {
        private AudioSource mSource;
        private float mVolume;//当前设置的音量

        public VoicePlayer(float volume)
        {
            mSource = SoundManager.NewAudioSource();
            mSource.loop = false;
            Volume = volume;
        }

        // 获取或设置播放器的音量
        public float Volume
        {
            get
            {
                return mVolume;
            }
            set
            {
                mVolume = value;
                if (mSource != null)
                    mSource.volume = mVolume;
            }
        }

        /// <summary>
        /// 当前是否有背景音乐正在播放
        /// </summary>
        public bool IsPlaying
        {
            get
            {
                return mSource.isPlaying;
            }
        }

        public bool IsPaused { get; private set; }


        public void PlayVoice(AudioClip clip)
        {
            Stop();
            if (clip != null)
            {
                mSource.clip = clip;
                mSource.Play();
            }
        }

        public void Pause()
        {
            if (mSource != null)
            {
                IsPaused = true;
                mSource.Pause();
            }
        }

        public void Resume()
        {
            if (!IsPlaying)
            {
                IsPaused = false;
                mSource.Play();
            }
        }

        public void Stop()
        {
            if (mSource != null)
            {
                mSource.Stop();
                mSource.clip = null;
            }
        }

        internal void Update()
        {
            if (mSource && !IsPaused && !mSource.isPlaying)
            {
                Stop();
            }
        }
    }
}
