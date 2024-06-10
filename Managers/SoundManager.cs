using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource bgmPlayer;
        [SerializeField] private AudioSource sfxPlayer;

        private string currentPlayKey;
        private SerializedDictionary<string, AudioClip> soundDic;
        
        
        public override void Reset()
        {
            JustDebug.LogColor("SoundManager Reset");

            soundDic = ResourceManager.Instance.SoundDic;
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

        public bool IsBgmPlaying(string key)
        {
            return key == currentPlayKey;
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