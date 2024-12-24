using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Audio
{
    public class AudioManager : MonoBehaviour
    {
        private const float DefaultVolume = 1f;

        [SerializeField] private SoundsSO _sounds;

        private Dictionary<SoundChanelType, AudioSource> _audioSources;
        private Dictionary<SoundChanelType, float> _volumes;
        private Dictionary<SoundChanelType, Coroutine> _playSmoothCoroutines;

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

        private SoundEffectSO GetSoundEffectSO(string soundName)
        {
            if (_sounds.Sounds.TryGetValue(soundName, out var so))
            {
                return so;
            }

            Debug.LogWarning($"Sound {soundName} not found.");
            return null;
        }

        public void Awake()
        {
            gameObject.AddComponent<AudioListener>();

            _audioSources = new Dictionary<SoundChanelType, AudioSource>();
            _volumes = new Dictionary<SoundChanelType, float>();
            _playSmoothCoroutines = new Dictionary<SoundChanelType, Coroutine>();

            foreach (SoundChanelType chanelType in Enum.GetValues(typeof(SoundChanelType)))
            {
                _audioSources.Add(chanelType, gameObject.AddComponent<AudioSource>());
                _volumes.Add(chanelType, DefaultVolume);
            }
        }

        public void SetMute(SoundChanelType soundChanelType, bool value) =>
            _audioSources[soundChanelType].mute = value;

        public bool GetMute(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].mute;


        public void PlayOneShot(SoundChanelType soundChanelType, string soundName) =>
            PlayOneShot(soundChanelType, GetSoundEffectSO(soundName));

        public void PlayOneShot(SoundChanelType soundChanelType, SoundEffectSO soundEffect)
        {
            soundEffect.SetupAudioSource(_audioSources[soundChanelType]);
            _audioSources[soundChanelType].PlayOneShot(soundEffect.GetAudioClip());
        }

        public void PlayOneShotWithDelay(SoundChanelType soundChanelType, string soundName, float delay) =>
            PlayOneShotWithDelay(soundChanelType, GetSoundEffectSO(soundName), delay);

        public void PlayOneShotWithDelay(SoundChanelType soundChanelType, SoundEffectSO soundEffect, float delay) =>
            StartCoroutine(PlayOneShotWithDelayCoroutine(soundChanelType, soundEffect, delay));

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
            soundEffect.SetupAudioSource(_audioSources[soundChanelType]);
            _audioSources[soundChanelType].clip = soundEffect.GetAudioClip();
            _audioSources[soundChanelType].loop = loop;
            _audioSources[soundChanelType].time = offset;
            _audioSources[soundChanelType].Play();
        }

        public void Play(SoundChanelType soundChanelType, string audioClipName) =>
            Play(soundChanelType, GetSoundEffectSO(audioClipName));

        public void Play(SoundChanelType soundChanelType, SoundEffectSO soundEffect)
        {
            soundEffect.SetupAudioSource(_audioSources[soundChanelType]);
            _audioSources[soundChanelType].Play();
        }

        public bool IsPlaying(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].isPlaying;

        public bool IsPlaying(SoundChanelType soundChanelType, string audioClipName) =>
            IsPlaying(soundChanelType, GetSoundEffectSO(audioClipName));

        public bool IsPlaying(SoundChanelType soundChanelType, SoundEffectSO soundEffect) =>
            _audioSources[soundChanelType].isPlaying
            && _audioSources[soundChanelType].clip == soundEffect.GetAudioClip();

        public void Stop(SoundChanelType soundChanelType) =>
            _audioSources[soundChanelType].Stop();

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
            if (_playSmoothCoroutines.ContainsKey(soundChanelType))
            {
                StopCoroutine(_playSmoothCoroutines[soundChanelType]);
                _playSmoothCoroutines.Remove(soundChanelType);
            }

            _playSmoothCoroutines.Add(
                soundChanelType,
                StartCoroutine(PlaySmoothCoroutine(soundChanelType, soundEffect, loop, offset)));
        }

        private IEnumerator PlaySmoothCoroutine(SoundChanelType soundChanelType, SoundEffectSO soundEffect, bool loop = false,
            float offset = 0f)
        {
            while (_audioSources[soundChanelType].volume > float.Epsilon)
            {
                _audioSources[soundChanelType].volume -= UnityEngine.Time.unscaledDeltaTime;
                yield return null;
            }

            Play(soundChanelType, soundEffect, loop, offset);

            while (_audioSources[soundChanelType].volume < _volumes[soundChanelType])
            {
                _audioSources[SoundChanelType.Music].volume += UnityEngine.Time.unscaledDeltaTime;
                yield return null;
            }

            _audioSources[SoundChanelType.Music].volume = _volumes[soundChanelType];

            _playSmoothCoroutines.Remove(soundChanelType);
        }
    }
}