using System;
using System.Collections.Generic;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Spate
{
    /// <summary>
    /// 音效播放器
    /// </summary>
    public sealed class SePlayer
    {
        private Dictionary<string, AudioClip> mClipMap;
        private List<AudioSource> mPlayerList;

        public SePlayer(float volume)
        {
            Volume = volume;
            mClipMap = new Dictionary<string, AudioClip>(50);
            mPlayerList = new List<AudioSource>(10);
        }

        // 获取或设置播放器的音量
        public float Volume { get; set; }

        public void Add(AudioClip clip)
        {
            if (clip != null)
            {
                string name = clip.name;
                mClipMap.Add(name, clip);
            }
        }
        public void AddRange(IList<AudioClip> listClip)
        {
            if (listClip == null) return;
            foreach (AudioClip clip in listClip)
            {
                Add(clip);
            }
        }
        public void Play(string name, bool loop = false)
        {
            if (!mClipMap.ContainsKey(name))
            {
                AudioClip clip = Resources.Load<AudioClip>("se/" + name);
                if (clip == null)
                {
                    Logger.LogError("指定音效资不存在:" + name);
                    return;
                }
                mClipMap.Add(name, clip);
            }
            // 取得播放器,优先从缓存中查找,如果没有再新建并加入到缓存
            AudioSource player = null;
            foreach (AudioSource source in mPlayerList)
            {
                if (source.clip == null)
                {
                    player = source;
                    break;
                }
            }
            if (player == null)
            {
                player = SoundManager.NewAudioSource();
                mPlayerList.Add(player);
            }
            // 准备播放
            player.clip = mClipMap[name];
            player.volume = Volume;
            player.loop = loop;
            player.Play();
        }
        public void Pause()
        {
            foreach (AudioSource player in mPlayerList)
            {
                if (player.clip != null)
                    player.Pause();
            }
        }
        public void Resume()
        {
            foreach (AudioSource player in mPlayerList)
            {
                if (player.clip != null)
                    player.Play();
            }
        }
        /// <summary>
        /// 停止某个音效的播放
        /// </summary>
        /// <param name="clipName">短名称,不包括路径不包括扩展名</param>
        public void Stop(string clipName = null)
        {
            foreach (AudioSource player in mPlayerList)
            {
                if (player.clip != null && (clipName == null || string.Equals(player.clip.name, clipName)))
                {
                    player.Stop();
                    player.clip = null;
                    if (player != null)
                        break;
                }
            }
        }
        public void StopAndClear()
        {
            foreach (AudioSource player in mPlayerList)
            {
                if (player.clip != null)
                {
                    player.Stop();
                    player.clip = null;
                }
                Object.Destroy(player);
            }
            mPlayerList.Clear();
        }
        internal void Update()
        {
            for (index = 0; index != mPlayerList.Count; index++)
            {
                if (mPlayerList[index].clip != null && !mPlayerList[index].isPlaying)
                {
                    // 说明该player已经播放完毕,可以切断clip的引用了
                    mPlayerList[index].clip = null;
                }
            }
        }
        private int index = 0;
    }
}
