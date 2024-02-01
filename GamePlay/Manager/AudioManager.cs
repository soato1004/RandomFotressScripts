using System.Collections.Generic;
using RandomFortress.Common;
using RandomFortress.Common.Util;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class AudioManager : Singleton<AudioManager>
    {
        public Dictionary<string, AudioClip> soundDic;

        private AudioSource bgmPlayer;
        // private AudioSource sfxPlayer;

        private string currentPlayKey;

        public float bgmVolume = 1f;
        public float sfxVolume = 1f;
        
        
        public override void Reset()
        {
            JTDebug.LogColor("AudioManager Reset");

            soundDic = ResourceManager.Instance.SoundDic;
            
            bgmPlayer = gameObject.AddComponent<AudioSource>();
            bgmPlayer.loop = true;
            bgmPlayer.volume = 0.3f;
        }
        
        public override void Terminate() 
        {
            JTDebug.LogColor("AudioManager Terminate");
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
                if (currentPlayKey == key)
                    return;

                currentPlayKey = key;
                if (bgmPlayer.isPlaying)
                    bgmPlayer.Stop();
                
                bgmPlayer.clip = soundDic[key];
                bgmPlayer.Play();
            }
        }

        public void StopBgm()
        {
            bgmPlayer.Stop();
        }
    }
}