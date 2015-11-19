using System;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 背景音乐播放器
    /// </summary>
    public sealed class BgmPlayer
    {
        private AudioSource mSource;
        private float mVolume;//当前设置的音量
        private float mVolumeFactor;//当前的音量比例,用于过渡

        private float mFadeDuration = 0f;
        private float mFactorFrom = 1.0f;
        private float mFactorTo = 1.0f;
        private float mFacotrRate = 0f;
        private bool mStopAfterFade = false;

        public BgmPlayer(float volume)
        {
            mVolumeFactor = 1f;
            mSource = SoundManager.NewAudioSource();
            mSource.loop = true;
            Volume = volume;
        }

        // 获取或设置背景音乐的音量
        public float Volume
        {
            get
            {
                return mVolume;
            }
            set
            {
                mVolume = value * 0.5f;
                mSource.volume = mVolume * mVolumeFactor;
            }
        }
        // 背景音乐淡入
        public void FadeIn(AudioClip clip)
        {
            FadeIn(clip, 0.8f);
        }
        // 背景音乐淡入
        public void FadeIn(AudioClip clip, float second)
        {
            FadeIn(clip, second, 0f);
        }
        // 背景音乐淡入并制定起始比例,如果0.8则表示当前设置的音量的80%开始过渡到100%
        public void FadeIn(AudioClip clip, float second, float factorFrom)
        {
            FadeIn(clip, second, factorFrom, 1f);
        }
        // 背景音乐淡入
        public void FadeIn(AudioClip clip, float second, float factorFrom, float factorTo)
        {
            if (second <= 0)
                throw new ArgumentException("second must greater than 0");
            if (factorTo < factorFrom)
                throw new ArgumentException("factorTo must greater than or equal to factorFrom");
            Play(clip);
            mFadeDuration = second;
            mFactorFrom = factorFrom;
            mFactorTo = factorTo;
            mFacotrRate = (mFactorTo - mFactorFrom) / second;
            SetVolumeFactor(factorFrom);
            mStopAfterFade = false;
        }

        // 淡出背景音乐并停止播放
        public void FadeOut()
        {
            FadeOut(0.5f);
        }
        // 淡出背景音乐并停止播放
        public void FadeOut(float second)
        {
            FadeOut(second, true);
        }
        // 淡出背景音乐并制定是否淡出后停止播放
        public void FadeOut(float second, bool stopAlfterFade)
        {
            FadeOut(second, 1f, 0f, stopAlfterFade);
        }
        // 淡出背景音乐并制定是否淡出后停止播放
        public void FadeOut(float second, float factorFrom, float factorTo, bool stopAlfterFade)
        {
            if (second <= 0)
                throw new ArgumentException("second must greater than 0");
            if (factorFrom < factorTo)
                throw new ArgumentException("factorFrom must greater than or equal to factorTo");
            if (mSource == null || mSource.clip == null)
                return;
            mFadeDuration = second;
            mFactorFrom = factorFrom;
            mFactorTo = factorTo;
            mFacotrRate = (mFactorFrom - mFactorTo) / second;
            SetVolumeFactor(factorFrom);
            mStopAfterFade = stopAlfterFade;
        }

        /// <summary>
        /// 暂停背景音乐
        /// </summary>
        public void Pause()
        {
            if (mSource != null)
                mSource.Pause();
        }

        /// <summary>
        /// 停止背景音乐
        /// </summary>
        public void Stop()
        {
            if (mSource != null)
            {
                mSource.Stop();
                mSource.clip = null;
            }
            mFadeDuration = 0f;
        }
        /// <summary>
        /// 恢复播放背景音乐
        /// </summary>
        public void Resume()
        {
            if (!IsPlaying && mSource != null)
                mSource.Play();
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

        /// <summary>
        /// 播放背景音乐
        /// </summary>
        public void Play(AudioClip clip)
        {
            if (IsPlaying && mSource.clip == clip)
                return;
            Stop();
            if (clip != null)
            {
                mSource.clip = clip;
                SetVolumeFactor(1f);
                mSource.Play();
            }
        }

        internal void Update()
        {
            // 检查是否有过渡
            if (mFadeDuration > 0 && mSource.clip != null)
            {
                mFadeDuration -= Time.deltaTime;
                if (mFactorFrom < mFactorTo)
                {
                    // FadeIn
                    float factor = mFactorFrom + mFacotrRate * Time.deltaTime;
                    SetVolumeFactor(factor);
                }
                else if (mFactorFrom > mFactorTo)
                {
                    // FadeOut
                    float factor = mFactorFrom - mFacotrRate * Time.deltaTime;
                    SetVolumeFactor(factor);
                }
                // Fade结束
                if (mFadeDuration <= 0)
                {
                    SetVolumeFactor(mFactorTo);
                    if (mStopAfterFade)
                        Stop();
                    mFactorFrom = mFactorTo = mFacotrRate = mFadeDuration = 0f;
                }
            }
        }

        // 设置Volume缩放比例
        public void SetVolumeFactor(float factor)
        {
            mVolumeFactor = factor;
            // 更新Volume
            mSource.volume = mVolume * mVolumeFactor;
        }
    }
}