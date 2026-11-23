using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Scripts.Services.Settings;
using UnityEngine;
using UnityEngine.Serialization;
using Zenject;

namespace Scripts.Audio
{
    public class AudioManager : MonoBehaviour
    {
        public static AudioManager Instance;

        private const float DefaultVolume = 1f;

        [FormerlySerializedAs("_sounds")] [SerializeField]
        private SoundTableSO soundTable;

        [Header("Volume Settings")]
        [SerializeField] private SoundChanelType[] _musicChannels = { SoundChanelType.Music };

        [SerializeField] private SoundChanelType[] _effectChannels =
        {
            SoundChanelType.Player,
            SoundChanelType.Enemy,
            SoundChanelType.LevelObjects,
            SoundChanelType.Environment
        };

        [Tooltip("Channels that keep playing while the game is paused.")]
        [SerializeField] private SoundChanelType[] _pauseIgnoringChannels = { SoundChanelType.UI };

        private Dictionary<SoundChanelType, AudioSource> _audioSources;
        private Dictionary<SoundChanelType, float> _volumes;
        private Dictionary<SoundChanelType, float> _baseVolumes;
        private Dictionary<SoundChanelType, Coroutine> _playSmoothCoroutines;
        private SettingsService _settingsService;

        [Inject]
        private void Construct(SettingsService settingsService)
        {
            _settingsService = settingsService;
        }

        public void Awake()
        {
            Instance = this;

            gameObject.AddComponent<AudioListener>();

            _audioSources = new Dictionary<SoundChanelType, AudioSource>();
            _volumes = new Dictionary<SoundChanelType, float>();
            _baseVolumes = new Dictionary<SoundChanelType, float>();
            _playSmoothCoroutines = new Dictionary<SoundChanelType, Coroutine>();

            foreach (SoundChanelType chanelType in Enum.GetValues(typeof(SoundChanelType)))
            {
                Debug.Log(chanelType);
                _audioSources.Add(chanelType, gameObject.AddComponent<AudioSource>());
                _volumes.Add(chanelType, DefaultVolume);
                _baseVolumes.Add(chanelType, DefaultVolume);
            }

            foreach (var chanelType in _pauseIgnoringChannels)
                _audioSources[chanelType].ignoreListenerPause = true;
        }

        private void Start()
        {
            if (_settingsService == null) return;

            foreach (VolumeCategory category in Enum.GetValues(typeof(VolumeCategory)))
                ApplyVolume(category, _settingsService.GetVolume(category));

            _settingsService.VolumeChanged += SettingsService_VolumeChanged;
        }

        private void OnDestroy()
        {
            if (_settingsService != null)
                _settingsService.VolumeChanged -= SettingsService_VolumeChanged;
        }

        private void SettingsService_VolumeChanged(VolumeCategory category, float volume) =>
            ApplyVolume(category, volume);

        private void ApplyVolume(VolumeCategory category, float volume)
        {
            switch (category)
            {
                case VolumeCategory.Master:
                    TotalVolume = volume;
                    break;
                case VolumeCategory.Music:
                    SetChannelsVolume(_musicChannels, volume);
                    break;
                case VolumeCategory.Effects:
                    SetChannelsVolume(_effectChannels, volume);
                    break;
            }
        }

        private void SetChannelsVolume(SoundChanelType[] channels, float volume)
        {
            foreach (var chanelType in channels)
                SetChannelVolume(chanelType, volume);
        }

        public float GetChannelVolume(SoundChanelType soundChanelType) => _volumes[soundChanelType];

        public void SetChannelVolume(SoundChanelType soundChanelType, float volume)
        {
            _volumes[soundChanelType] = volume;
            ApplyChannelVolume(soundChanelType);
        }

        private void CaptureBaseVolume(SoundChanelType soundChanelType)
        {
            _baseVolumes[soundChanelType] = _audioSources[soundChanelType].volume;
            ApplyChannelVolume(soundChanelType);
        }

        private void ApplyChannelVolume(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].volume = _baseVolumes[soundChanelType] * _volumes[soundChanelType];

        public float TotalVolume
        {
            get => AudioListener.volume;
            set => AudioListener.volume = value;
        }

        public bool TotalMute
        {
            get => AudioListener.pause;
            set => AudioListener.pause = value;
        }

        public SoundEffectSO GetSoundEffectSO(string soundName)
        {
            if (soundTable.Sounds.TryGetValue(soundName, out var so))
            {
                return so;
            }

            Debug.LogWarning($"Sound {soundName} not found.");
            return null;
        }


        public void SetMute(SoundChanelType soundChanelType, bool value) =>
            _audioSources[soundChanelType].mute = value;

        public bool GetMute(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].mute;


        public void PlayOneShot(SoundChanelType soundChanelType, string soundName, float volumeScale) =>
            PlayOneShot(soundChanelType, GetSoundEffectSO(soundName), volumeScale);

        public void PlayOneShot(SoundChanelType soundChanelType, string soundName) =>
            PlayOneShot(soundChanelType, GetSoundEffectSO(soundName));

        public void PlayOneShot(SoundChanelType soundChanelType, SoundEffectSO soundEffect, float volumeScale = 1F)
        {
            if (!soundEffect) return;

            // Pull the clip once. Setting the source up and then asking for a second clip would
            // advance the variation index twice and play a different clip than the one configured.
            var clip = soundEffect.GetAudioClip();
            if (!clip) return;

            var audioSource = _audioSources[soundChanelType];
            soundEffect.ApplyRandomSettings(audioSource);
            audioSource.volume *= volumeScale;
            CaptureBaseVolume(soundChanelType);
            audioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Like <see cref="PlayOneShot(SoundChanelType, string, float)"/>, but loudness above 1 is not lost
        /// to the AudioSource volume cap: the excess is applied as one-shot gain on top.
        /// </summary>
        public void PlayOneShotWithGain(SoundChanelType soundChanelType, string soundName, float loudness)
        {
            var soundEffect = GetSoundEffectSO(soundName);
            if (!soundEffect) return;

            var clip = soundEffect.GetAudioClip();
            if (!clip) return;

            var audioSource = _audioSources[soundChanelType];
            soundEffect.ApplyRandomSettings(audioSource);

            float rolledVolume = audioSource.volume;
            float targetVolume = rolledVolume * loudness;
            audioSource.volume = Mathf.Min(1f, targetVolume);
            float gain = audioSource.volume > 0f ? Mathf.Max(1f, targetVolume / audioSource.volume) : 1f;

            CaptureBaseVolume(soundChanelType);
            audioSource.PlayOneShot(clip, gain);
        }

        public void PlayOneShotWithDelay(SoundChanelType soundChanelType, string soundName, float delay) =>
            PlayOneShotWithDelay(soundChanelType, GetSoundEffectSO(soundName), delay);

        public void PlayOneShotWithDelay(SoundChanelType soundChanelType, SoundEffectSO soundEffect, float delay)
        {
            if (!soundEffect) return;
            StartCoroutine(PlayOneShotWithDelayCoroutine(soundChanelType, soundEffect, delay));
        }

        private IEnumerator PlayOneShotWithDelayCoroutine(SoundChanelType soundChanelType, SoundEffectSO soundEffect,
            float delay)
        {
            yield return new WaitForSeconds(delay);
            PlayOneShot(soundChanelType, soundEffect);
        }

        public void Play(SoundChanelType soundChanelType, string audioClipName, bool loop = false, float offset = 0f) =>
            Play(soundChanelType, GetSoundEffectSO(audioClipName), loop, offset);

        public void Play(SoundChanelType soundChanelType, SoundEffectSO soundEffect, bool loop = false,
            float offset = 0f)
        {
            if (!soundEffect) return;

            soundEffect.SetupAudioSource(_audioSources[soundChanelType]);
            _audioSources[soundChanelType].clip = soundEffect.GetAudioClip();
            _audioSources[soundChanelType].loop = loop;
            _audioSources[soundChanelType].time = offset;
            CaptureBaseVolume(soundChanelType);
            _audioSources[soundChanelType].Play();
        }

        public void Play(SoundChanelType soundChanelType, string audioClipName) =>
            Play(soundChanelType, GetSoundEffectSO(audioClipName));

        public void Play(SoundChanelType soundChanelType, SoundEffectSO soundEffect)
        {
            if (!soundEffect) return;

            soundEffect.SetupAudioSource(_audioSources[soundChanelType]);
            CaptureBaseVolume(soundChanelType);
            _audioSources[soundChanelType].Play();
        }

        public bool IsPlaying(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].isPlaying;

        public bool IsPlaying(SoundChanelType soundChanelType, string audioClipName) =>
            IsPlaying(soundChanelType, GetSoundEffectSO(audioClipName));

        public bool IsPlaying(SoundChanelType soundChanelType, SoundEffectSO soundEffect) =>
            soundEffect
            && _audioSources[soundChanelType].isPlaying
            && soundEffect.clips != null
            && Array.IndexOf(soundEffect.clips, _audioSources[soundChanelType].clip) >= 0;

        public void Stop(SoundChanelType soundChanelType, string soundName = null)
        {
            if (soundName != null)
            {
                if (IsPlaying(soundChanelType, soundName))
                    _audioSources[soundChanelType].Stop();
            }
            else
            {
                _audioSources[soundChanelType].Stop();
            }
        }

        public void Pause(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].Stop();

        public void Continue(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].Play();

        public void PlaySmooth(SoundChanelType soundChanelType, string audioClipName, bool loop = false,
            float offset = 0f) =>
            PlaySmooth(soundChanelType, GetSoundEffectSO(audioClipName), loop, offset);

        public void PlaySmooth(SoundChanelType soundChanelType, SoundEffectSO soundEffect, bool loop = false,
            float offset = 0f)
        {
            if (!soundEffect) return;

            if (_playSmoothCoroutines.ContainsKey(soundChanelType))
            {
                StopCoroutine(_playSmoothCoroutines[soundChanelType]);
                _playSmoothCoroutines.Remove(soundChanelType);
            }

            _playSmoothCoroutines.Add(
                soundChanelType,
                StartCoroutine(PlaySmoothCoroutine(soundChanelType, soundEffect, loop, offset)));
        }

        private IEnumerator PlaySmoothCoroutine(SoundChanelType soundChanelType, SoundEffectSO soundEffect,
            bool loop = false,
            float offset = 0f, float smoothSpeed = 0.3f)
        {
            yield return _audioSources[soundChanelType].DOFade(0f, smoothSpeed).WaitForCompletion();
            SetMute(soundChanelType, true);
            Play(soundChanelType, soundEffect, loop, offset);
            var audioSourceVolume = _audioSources[soundChanelType].volume;
            _audioSources[soundChanelType].volume = 0;
            SetMute(soundChanelType, false);

            yield return _audioSources[soundChanelType].DOFade(audioSourceVolume, smoothSpeed).WaitForCompletion();

            ApplyChannelVolume(soundChanelType);

            _playSmoothCoroutines.Remove(soundChanelType);
        }
    }
}