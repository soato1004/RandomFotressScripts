using UnityEngine;
using UnityEngine.Rendering;

namespace RandomFortress
{
    public enum SoundKey
    {
        bgm_boss,
        bgm_game,
        bgm_lobby,
        bullet_hit,
        button_click,
        result_lose,
        result_win,
        result_clear,
        stage_skip,
        tower_create,
        tower_select,
        tower_sell,
        tower_upgrade,
        bullet_hit_base,
        superpass_buy,
    }
    
    public enum SoundType
    {
        ALL,
        BGM, 
        SFX
    }
    
    public class SoundManager : Singleton<SoundManager>
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource sfxSource;

        private SoundKey currentBgmKey;
        public SerializedDictionary<SoundKey, AudioClip> audioClips = new SerializedDictionary<SoundKey, AudioClip>();
        public SerializedDictionary<string, string> audioFileNames = new SerializedDictionary<string, string>();

        private const string BGM_VOLUME_KEY = "BGMVolume";
        private const string SFX_VOLUME_KEY = "SFXVolume";

        public void PlayBgm(SoundKey key, float volume = 0.3f)
        {
            PlaySound(key, SoundType.BGM, volume);
        }
        
        public void PlayOneShot(SoundKey key, float volume = 0.3f)
        {
            PlaySound(key, SoundType.SFX, volume);
        }

        public void PlaySound(SoundKey key, SoundType type = SoundType.SFX, float volume = 1f)
        {
            if (!audioClips.TryGetValue(key, out AudioClip clip))
            {
                Debug.LogWarning($"Audio clip not found for key: {key}");
                return;
            }

            switch (type)
            {
                case SoundType.BGM:
                    PlayBGM(clip, volume);
                    break;
                case SoundType.SFX:
                    PlaySFX(clip, volume);
                    break;
            }
        }

        private void PlayBGM(AudioClip clip, float volume)
        {
            if (bgmSource.clip == clip && bgmSource.isPlaying)
                return;

            bgmSource.Stop();
            bgmSource.clip = clip;
            bgmSource.volume = volume;
            bgmSource.loop = true;
            bgmSource.Play();
            currentBgmKey = GetKeyFromClip(clip);
        }

        private void PlaySFX(AudioClip clip, float volume)
        {
            sfxSource.PlayOneShot(clip, volume);
        }

        public void StopSound(SoundType type)
        {
            switch (type)
            {
                case SoundType.BGM:
                    bgmSource.Stop();
                    currentBgmKey = default;
                    break;
                case SoundType.SFX:
                    sfxSource.Stop();
                    break;
            }
        }

        public void PauseSound(SoundType type = SoundType.ALL)
        {
            switch (type)
            {
                case SoundType.ALL:
                    bgmSource.Pause();
                    sfxSource.Pause();
                    break;
                case SoundType.BGM:
                    bgmSource.Pause();
                    break;
                case SoundType.SFX:
                    sfxSource.Pause();
                    break;
            }
        }

        public void ResumeSound(SoundType type = SoundType.ALL)
        {
            switch (type)
            {
                case SoundType.ALL:
                    bgmSource.UnPause();
                    sfxSource.UnPause();
                    break;
                case SoundType.BGM:
                    bgmSource.UnPause();
                    break;
                case SoundType.SFX:
                    sfxSource.UnPause();
                    break;
            }
        }

        public bool IsPlayingBGM(SoundKey key)
        {
            return key == currentBgmKey && bgmSource.isPlaying;
        }

        public void StopAllSounds()
        {
            StopSound(SoundType.BGM);
            StopSound(SoundType.SFX);
        }

        private SoundKey GetKeyFromClip(AudioClip clip)
        {
            foreach (var pair in audioClips)
            {
                if (pair.Value == clip)
                    return pair.Key;
            }
            return default;
        }
    }
}