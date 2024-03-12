using System.Collections.Generic;
using RandomFortress.Common;
using RandomFortress.Common.Utils;
using RotaryHeart.Lib.SerializableDictionary;
using UnityEngine;

namespace RandomFortress.Manager
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource bgmPlayer;
        [SerializeField] private AudioSource sfxPlayer;

        private string currentPlayKey;
        private SerializableDictionaryBase<string, AudioClip> soundDic;
        
        
        public override void Reset()
        {
            JTDebug.LogColor("AudioManager Reset");

            soundDic = ResourceManager.Instance.SoundDic;
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
        
        public void PlayOneShot(string stringKey, float volume = 0.3f)
        {
            
            
            if (DataManager.Instance.stringTableDic.ContainsKey(stringKey))
            {
                string key = DataManager.Instance.stringTableDic[stringKey];
                if (soundDic.ContainsKey(key))
                    sfxPlayer.PlayOneShot(soundDic[key], volume);
                else Debug.Log("Not Found Sound Key : "+key);
            }
            else Debug.Log("Not Found String Key : "+stringKey);
                
        }

        public void PlayBgm(string stringKey, float volume = 1f)
        {
            if (DataManager.Instance.stringTableDic.ContainsKey(stringKey))
            {
                string key = DataManager.Instance.stringTableDic[stringKey];
                if (soundDic.ContainsKey(key))
                {
                    if (currentPlayKey == key)
                        return;

                    currentPlayKey = key;
                    if (bgmPlayer.isPlaying)
                        bgmPlayer.Stop();
                
                    bgmPlayer.clip = soundDic[key];
                    bgmPlayer.volume = volume;
                    bgmPlayer.Play();
                }
                else Debug.Log("Not Found Sound Key : "+key);
            }
            else Debug.Log("Not Found String Key : "+stringKey);
        }

        public void StopBgm()
        {
            bgmPlayer.Stop();
        }

        public void PauseBgm()
        {
            bgmPlayer.Pause();
        }
        
        public void ResumeBgm()
        {
            bgmPlayer.UnPause();
        }
    }
}