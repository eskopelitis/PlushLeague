using UnityEngine;
using UnityEngine.Audio;
using System.Collections.Generic;
using System.Collections;

namespace PlushLeague.Audio
{
    /// <summary>
    /// Centralized audio management system for Plush League.
    /// Handles all SFX, music, and audio feedback with plush-themed audio cues.
    /// </summary>
    public class AudioManager : MonoBehaviour
    {
        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;
        [SerializeField] private AudioSource uiSource;
        [SerializeField] private AudioSource ambientSource;
        
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixerGroup musicMixerGroup;
        [SerializeField] private AudioMixerGroup sfxMixerGroup;
        [SerializeField] private AudioMixerGroup uiMixerGroup;
        [SerializeField] private AudioMixerGroup masterMixerGroup;
        
        [Header("Music")]
        [SerializeField] private AudioClip menuMusicLoop;
        [SerializeField] private AudioClip powerSelectionMusic;
        [SerializeField] private AudioClip gameplayMusic;
        [SerializeField] private AudioClip victoryMusic;
        [SerializeField] private AudioClip defeatMusic;
        
        [Header("Gameplay SFX")]
        [SerializeField] private AudioClip[] kickSounds;
        [SerializeField] private AudioClip[] bounceSounds;
        [SerializeField] private AudioClip[] tackleSounds;
        [SerializeField] private AudioClip goalScored;
        [SerializeField] private AudioClip goalSaved;
        [SerializeField] private AudioClip whistleStart;
        [SerializeField] private AudioClip whistleEnd;
        [SerializeField] private AudioClip[] superpowerActivation;
        [SerializeField] private AudioClip[] superpowerImpact;
        
        [Header("UI SFX")]
        [SerializeField] private AudioClip buttonClick;
        [SerializeField] private AudioClip buttonHover;
        [SerializeField] private AudioClip navigationSound;
        [SerializeField] private AudioClip confirmSound;
        [SerializeField] private AudioClip cancelSound;
        [SerializeField] private AudioClip errorSound;
        [SerializeField] private AudioClip[] countdownBeeps;
        [SerializeField] private AudioClip countdownFinal;
        
        [Header("Ambient SFX")]
        [SerializeField] private AudioClip crowdCheer;
        [SerializeField] private AudioClip crowdBoo;
        [SerializeField] private AudioClip crowdAmbient;
        [SerializeField] private AudioClip windAmbient;
        
        [Header("Audio Settings")]
        [SerializeField] private bool enableSFX = true;
        [SerializeField] private bool enableMusic = true;
        [SerializeField] private bool enableAmbient = true;
        [SerializeField] private float sfxVolume = 1.0f;
        [SerializeField] private float musicVolume = 0.7f;
        [SerializeField] private float uiVolume = 0.8f;
        [SerializeField] private float ambientVolume = 0.3f;
        [SerializeField] private float fadeInDuration = 1.0f;
        [SerializeField] private float fadeOutDuration = 1.0f;
        
        // Singleton instance
        private static AudioManager _instance;
        public static AudioManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = FindFirstObjectByType<AudioManager>();
                    if (_instance == null)
                    {
                        var audioManagerObject = new GameObject("AudioManager");
                        _instance = audioManagerObject.AddComponent<AudioManager>();
                    }
                }
                return _instance;
            }
        }
        
        // Audio management
        private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
        private Dictionary<string, float> lastPlayTime = new Dictionary<string, float>();
        private float sfxCooldown = 0.1f; // Prevent audio spam
        
        // Current state
        private AudioClip currentMusic;
        private bool isMusicFading = false;
        
        #region Unity Lifecycle
        
        private void Awake()
        {
            // Singleton setup
            if (_instance == null)
            {
                _instance = this;
                DontDestroyOnLoad(gameObject);
                InitializeAudioSources();
                RegisterAudioClips();
            }
            else if (_instance != this)
            {
                Destroy(gameObject);
            }
        }
        
        private void Start()
        {
            LoadAudioSettings();
            StartMenuMusic();
        }
        
        #endregion
        
        #region Initialization
        
        /// <summary>
        /// Initialize audio sources if not assigned
        /// </summary>
        private void InitializeAudioSources()
        {
            if (musicSource == null)
            {
                musicSource = gameObject.AddComponent<AudioSource>();
                musicSource.loop = true;
                musicSource.playOnAwake = false;
                musicSource.outputAudioMixerGroup = musicMixerGroup;
            }
            
            if (sfxSource == null)
            {
                sfxSource = gameObject.AddComponent<AudioSource>();
                sfxSource.loop = false;
                sfxSource.playOnAwake = false;
                sfxSource.outputAudioMixerGroup = sfxMixerGroup;
            }
            
            if (uiSource == null)
            {
                uiSource = gameObject.AddComponent<AudioSource>();
                uiSource.loop = false;
                uiSource.playOnAwake = false;
                uiSource.outputAudioMixerGroup = uiMixerGroup;
            }
            
            if (ambientSource == null)
            {
                ambientSource = gameObject.AddComponent<AudioSource>();
                ambientSource.loop = true;
                ambientSource.playOnAwake = false;
                ambientSource.outputAudioMixerGroup = sfxMixerGroup;
            }
        }
        
        /// <summary>
        /// Register all audio clips for easy access
        /// </summary>
        private void RegisterAudioClips()
        {
            // Music
            if (menuMusicLoop != null) audioClips["menu_music"] = menuMusicLoop;
            if (powerSelectionMusic != null) audioClips["power_music"] = powerSelectionMusic;
            if (gameplayMusic != null) audioClips["gameplay_music"] = gameplayMusic;
            if (victoryMusic != null) audioClips["victory_music"] = victoryMusic;
            if (defeatMusic != null) audioClips["defeat_music"] = defeatMusic;
            
            // Gameplay SFX
            if (goalScored != null) audioClips["goal_scored"] = goalScored;
            if (goalSaved != null) audioClips["goal_saved"] = goalSaved;
            if (whistleStart != null) audioClips["whistle_start"] = whistleStart;
            if (whistleEnd != null) audioClips["whistle_end"] = whistleEnd;
            
            // UI SFX
            if (buttonClick != null) audioClips["button_click"] = buttonClick;
            if (buttonHover != null) audioClips["button_hover"] = buttonHover;
            if (navigationSound != null) audioClips["navigation"] = navigationSound;
            if (confirmSound != null) audioClips["confirm"] = confirmSound;
            if (cancelSound != null) audioClips["cancel"] = cancelSound;
            if (errorSound != null) audioClips["error"] = errorSound;
            if (countdownFinal != null) audioClips["countdown_final"] = countdownFinal;
            
            // Ambient
            if (crowdCheer != null) audioClips["crowd_cheer"] = crowdCheer;
            if (crowdBoo != null) audioClips["crowd_boo"] = crowdBoo;
            if (crowdAmbient != null) audioClips["crowd_ambient"] = crowdAmbient;
            if (windAmbient != null) audioClips["wind_ambient"] = windAmbient;
        }
        
        #endregion
        
        #region Public API
        
        /// <summary>
        /// Play a sound effect by name
        /// </summary>
        public void PlaySFX(string sfxName)
        {
            if (!enableSFX) return;
            
            // Check for cooldown to prevent spam
            if (lastPlayTime.ContainsKey(sfxName) && Time.time - lastPlayTime[sfxName] < sfxCooldown)
            {
                return;
            }
            
            AudioClip clip = GetAudioClip(sfxName);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, sfxVolume);
                lastPlayTime[sfxName] = Time.time;
            }
            else
            {
                UnityEngine.Debug.LogWarning($"AudioManager: SFX '{sfxName}' not found");
            }
        }
        
        /// <summary>
        /// Play a sound effect with custom volume
        /// </summary>
        public void PlaySFX(string sfxName, float volume)
        {
            if (!enableSFX) return;
            
            AudioClip clip = GetAudioClip(sfxName);
            if (clip != null)
            {
                sfxSource.PlayOneShot(clip, volume * sfxVolume);
                lastPlayTime[sfxName] = Time.time;
            }
        }
        
        /// <summary>
        /// Play a random sound from an array
        /// </summary>
        public void PlayRandomSFX(AudioClip[] clips)
        {
            if (!enableSFX || clips == null || clips.Length == 0) return;
            
            AudioClip randomClip = clips[Random.Range(0, clips.Length)];
            if (randomClip != null)
            {
                sfxSource.PlayOneShot(randomClip, sfxVolume);
            }
        }
        
        /// <summary>
        /// Play kick sound with variation
        /// </summary>
        public void PlayKickSound()
        {
            PlayRandomSFX(kickSounds);
        }
        
        /// <summary>
        /// Play bounce sound with variation
        /// </summary>
        public void PlayBounceSound()
        {
            PlayRandomSFX(bounceSounds);
        }
        
        /// <summary>
        /// Play tackle sound with variation
        /// </summary>
        public void PlayTackleSound()
        {
            PlayRandomSFX(tackleSounds);
        }
        
        /// <summary>
        /// Play superpower activation sound
        /// </summary>
        public void PlaySuperpowerActivation()
        {
            PlayRandomSFX(superpowerActivation);
        }
        
        /// <summary>
        /// Play superpower impact sound
        /// </summary>
        public void PlaySuperpowerImpact()
        {
            PlayRandomSFX(superpowerImpact);
        }
        
        /// <summary>
        /// Play countdown beep
        /// </summary>
        public void PlayCountdownBeep(int count)
        {
            if (countdownBeeps != null && count > 0 && count <= countdownBeeps.Length)
            {
                AudioClip beep = countdownBeeps[count - 1];
                if (beep != null)
                {
                    uiSource.PlayOneShot(beep, uiVolume);
                }
            }
        }
        
        /// <summary>
        /// Play UI sound effect
        /// </summary>
        public void PlayUISound(string soundName)
        {
            AudioClip clip = GetAudioClip(soundName);
            if (clip != null)
            {
                uiSource.PlayOneShot(clip, uiVolume);
            }
        }
        
        /// <summary>
        /// Play music with smooth transition
        /// </summary>
        public void PlayMusic(string musicName)
        {
            if (!enableMusic) return;
            
            AudioClip clip = GetAudioClip(musicName);
            if (clip != null && clip != currentMusic)
            {
                StartCoroutine(TransitionMusic(clip));
            }
        }
        
        /// <summary>
        /// Stop current music
        /// </summary>
        public void StopMusic()
        {
            if (musicSource.isPlaying)
            {
                StartCoroutine(FadeOutMusic());
            }
        }
        
        /// <summary>
        /// Play ambient sound
        /// </summary>
        public void PlayAmbient(string ambientName)
        {
            if (!enableAmbient) return;
            
            AudioClip clip = GetAudioClip(ambientName);
            if (clip != null)
            {
                ambientSource.clip = clip;
                ambientSource.volume = ambientVolume;
                ambientSource.Play();
            }
        }
        
        /// <summary>
        /// Stop ambient sound
        /// </summary>
        public void StopAmbient()
        {
            if (ambientSource.isPlaying)
            {
                ambientSource.Stop();
            }
        }
        
        #endregion
        
        #region Music Management
        
        /// <summary>
        /// Start menu music
        /// </summary>
        public void StartMenuMusic()
        {
            PlayMusic("menu_music");
        }
        
        /// <summary>
        /// Start power selection music
        /// </summary>
        public void StartPowerSelectionMusic()
        {
            PlayMusic("power_music");
        }
        
        /// <summary>
        /// Start gameplay music
        /// </summary>
        public void StartGameplayMusic()
        {
            PlayMusic("gameplay_music");
        }
        
        /// <summary>
        /// Play victory music
        /// </summary>
        public void PlayVictoryMusic()
        {
            PlayMusic("victory_music");
        }
        
        /// <summary>
        /// Play defeat music
        /// </summary>
        public void PlayDefeatMusic()
        {
            PlayMusic("defeat_music");
        }
        
        /// <summary>
        /// Transition between music tracks
        /// </summary>
        private IEnumerator TransitionMusic(AudioClip newClip)
        {
            if (isMusicFading) yield break;
            
            isMusicFading = true;
            
            // Fade out current music
            if (musicSource.isPlaying)
            {
                float startVolume = musicSource.volume;
                float fadeTimer = 0f;
                
                while (fadeTimer < fadeOutDuration)
                {
                    fadeTimer += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeOutDuration);
                    yield return null;
                }
                
                musicSource.Stop();
            }
            
            // Start new music
            if (newClip != null)
            {
                musicSource.clip = newClip;
                musicSource.volume = 0f;
                musicSource.Play();
                currentMusic = newClip;
                
                // Fade in new music
                float fadeTimer = 0f;
                while (fadeTimer < fadeInDuration)
                {
                    fadeTimer += Time.deltaTime;
                    musicSource.volume = Mathf.Lerp(0f, musicVolume, fadeTimer / fadeInDuration);
                    yield return null;
                }
            }
            
            isMusicFading = false;
        }
        
        /// <summary>
        /// Fade out current music
        /// </summary>
        private IEnumerator FadeOutMusic()
        {
            if (isMusicFading) yield break;
            
            isMusicFading = true;
            float startVolume = musicSource.volume;
            float fadeTimer = 0f;
            
            while (fadeTimer < fadeOutDuration)
            {
                fadeTimer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, fadeTimer / fadeOutDuration);
                yield return null;
            }
            
            musicSource.Stop();
            currentMusic = null;
            isMusicFading = false;
        }
        
        #endregion
        
        #region Settings Management
        
        /// <summary>
        /// Set master volume
        /// </summary>
        public void SetMasterVolume(float volume)
        {
            if (masterMixerGroup != null)
            {
                masterMixerGroup.audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// Set music volume
        /// </summary>
        public void SetMusicVolume(float volume)
        {
            musicVolume = volume;
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
            }
            if (musicMixerGroup != null)
            {
                musicMixerGroup.audioMixer.SetFloat("MusicVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// Set SFX volume
        /// </summary>
        public void SetSFXVolume(float volume)
        {
            sfxVolume = volume;
            if (sfxMixerGroup != null)
            {
                sfxMixerGroup.audioMixer.SetFloat("SFXVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// Set UI volume
        /// </summary>
        public void SetUIVolume(float volume)
        {
            uiVolume = volume;
            if (uiMixerGroup != null)
            {
                uiMixerGroup.audioMixer.SetFloat("UIVolume", Mathf.Log10(volume) * 20);
            }
        }
        
        /// <summary>
        /// Toggle SFX on/off
        /// </summary>
        public void ToggleSFX(bool enabled)
        {
            enableSFX = enabled;
        }
        
        /// <summary>
        /// Toggle music on/off
        /// </summary>
        public void ToggleMusic(bool enabled)
        {
            enableMusic = enabled;
            if (!enabled && musicSource.isPlaying)
            {
                StopMusic();
            }
        }
        
        /// <summary>
        /// Load audio settings from PlayerPrefs
        /// </summary>
        private void LoadAudioSettings()
        {
            SetMasterVolume(PlayerPrefs.GetFloat("MasterVolume", 1.0f));
            SetMusicVolume(PlayerPrefs.GetFloat("MusicVolume", 0.7f));
            SetSFXVolume(PlayerPrefs.GetFloat("SFXVolume", 1.0f));
            SetUIVolume(PlayerPrefs.GetFloat("UIVolume", 0.8f));
            
            enableSFX = PlayerPrefs.GetInt("EnableSFX", 1) == 1;
            enableMusic = PlayerPrefs.GetInt("EnableMusic", 1) == 1;
            enableAmbient = PlayerPrefs.GetInt("EnableAmbient", 1) == 1;
        }
        
        /// <summary>
        /// Save audio settings to PlayerPrefs
        /// </summary>
        public void SaveAudioSettings()
        {
            PlayerPrefs.SetFloat("MasterVolume", musicSource.volume);
            PlayerPrefs.SetFloat("MusicVolume", musicVolume);
            PlayerPrefs.SetFloat("SFXVolume", sfxVolume);
            PlayerPrefs.SetFloat("UIVolume", uiVolume);
            
            PlayerPrefs.SetInt("EnableSFX", enableSFX ? 1 : 0);
            PlayerPrefs.SetInt("EnableMusic", enableMusic ? 1 : 0);
            PlayerPrefs.SetInt("EnableAmbient", enableAmbient ? 1 : 0);
            
            PlayerPrefs.Save();
        }
        
        #endregion
        
        #region Utility Methods
        
        /// <summary>
        /// Get audio clip by name
        /// </summary>
        private AudioClip GetAudioClip(string clipName)
        {
            if (audioClips.ContainsKey(clipName))
            {
                return audioClips[clipName];
            }
            return null;
        }
        
        /// <summary>
        /// Check if audio is currently playing
        /// </summary>
        public bool IsPlaying(string clipName)
        {
            AudioClip clip = GetAudioClip(clipName);
            return clip != null && (musicSource.clip == clip || sfxSource.clip == clip || uiSource.clip == clip);
        }
        
        #endregion
    }
}
