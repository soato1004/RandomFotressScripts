using System;
using System.Collections.Generic;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class SoundManager : Singleton<SoundManager>
    {
        public Dictionary<string, AudioClip> soundDic;

        private AudioSource bgmPlayer;
        // private AudioSource sfxPlayer;

        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        
        
        public override void Reset()
        {
            JTDebug.LogColor("ResourceManager Reset");

            soundDic = ResourceManager.Instance.SoundDic;
            
            bgmPlayer = gameObject.AddComponent<AudioSource>();
            bgmPlayer.loop = true;
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("ResourceManager Terminate");
        }

        public enum SoundType
        {
            Bgm, Effect
        }

        public enum SoundKey {
            Shot, Attack
        };
        
        public void PlayOneShot(string key, float volume = 0.3f)
        {
            if (soundDic.ContainsKey(key)) 
                bgmPlayer.PlayOneShot(soundDic[key], volume);
        }

        public void PlayBgm(string key)
        {
            if (soundDic.ContainsKey(key))
            {
                bgmPlayer.clip = soundDic[key];
                bgmPlayer.Play();
            }
        }
    }
}