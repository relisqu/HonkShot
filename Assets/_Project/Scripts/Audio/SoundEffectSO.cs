using Sirenix.OdinInspector;
using UnityEditor;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Audio
{
    [CreateAssetMenu(fileName = "SoundEffect", menuName = "Audio/New Sound Effect")]
    public class SoundEffectSO : ScriptableObject
    {
        #region config

        private static readonly float SEMITONES_TO_PITCH_CONVERSION_UNIT = 1.05946f;

        [Required] public AudioClip[] clips;

        [MinMaxSlider(0, 1)] [BoxGroup("config")]
        public Vector2 volume = new Vector2(0.5f, 0.5f);

        // Pitch / Semitones
        [LabelWidth(100)] [HorizontalGroup("config/pitch")]
        public bool useSemitones;

        [HideLabel]
        [ShowIf("useSemitones")]
        [HorizontalGroup("config/pitch")]
        [MinMaxSlider(-10, 10)]
        [OnValueChanged("SyncPitchAndSemitones")]
        public Vector2Int semitones = new Vector2Int(0, 0);

        [HideLabel]
        [HideIf("useSemitones")]
        [MinMaxSlider(0, 3)]
        [HorizontalGroup("config/pitch")]
        [OnValueChanged("SyncPitchAndSemitones")]
        public Vector2 pitch = new Vector2(1, 1);

        [BoxGroup("config")] [SerializeField] private SoundClipPlayOrder playOrder;

        [DisplayAsString] [BoxGroup("config")] [SerializeField]
        private int playIndex = 0;

        #endregion

        #region PreviewCode

#if UNITY_EDITOR
        private AudioSource previewer;

        private void OnEnable()
        {
            previewer = EditorUtility
                .CreateGameObjectWithHideFlags("AudioPreview", HideFlags.HideAndDontSave,
                    typeof(AudioSource))
                .GetComponent<AudioSource>();
        }

        private void OnDisable()
        {
            DestroyImmediate(previewer.gameObject);
        }


        [ButtonGroup("previewControls")]
        [GUIColor(.3f, 1f, .3f)]
        [Button(ButtonSizes.Gigantic)]
        private void PlayPreview()
        {
            Play(previewer);
        }

        [ButtonGroup("previewControls")]
        [GUIColor(1, .3f, .3f)]
        [Button(ButtonSizes.Gigantic)]
        [EnableIf("@previewer.isPlaying")]
        private void StopPreview()
        {
            previewer.Stop();
        }
#endif

        #endregion

        public void SyncPitchAndSemitones()
        {
            if (useSemitones)
            {
                pitch.x = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.x);
                pitch.y = Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, semitones.y);
            }
            else
            {
                semitones.x = Mathf.RoundToInt(Mathf.Log10(pitch.x) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
                semitones.y = Mathf.RoundToInt(Mathf.Log10(pitch.y) / Mathf.Log10(SEMITONES_TO_PITCH_CONVERSION_UNIT));
            }
        }

        public AudioClip GetAudioClip()
        {
            var clip = clips[playIndex >= clips.Length ? 0 : playIndex];

            switch (playOrder)
            {
                case SoundClipPlayOrder.InOrder:
                    playIndex = (playIndex + 1) % clips.Length;
                    break;
                case SoundClipPlayOrder.Random:
                    playIndex = Random.Range(0, clips.Length);
                    break;
                case SoundClipPlayOrder.InReverseOrder:
                    playIndex = (playIndex + clips.Length - 1) % clips.Length;
                    break;
            }

            return clip;
        }
        public void SetupAudioSource(AudioSource audioSource)
        {
            if (clips.Length == 0)
            {
                Debug.LogWarning($"Missing sound clips for {name}");
            }

            if (audioSource == null)
            {
                var _obj = new GameObject("Sound", typeof(AudioSource));
                audioSource = _obj.GetComponent<AudioSource>();
            }

            audioSource.clip = GetAudioClip();
            audioSource.volume = Random.Range(volume.x, volume.y);
            audioSource.pitch = useSemitones
                ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(semitones.x, semitones.y))
                : Random.Range(pitch.x, pitch.y);

        }

        public AudioSource Play(AudioSource audioSourceParam = null)
        {
            if (clips.Length == 0)
            {
                Debug.LogWarning($"Missing sound clips for {name}");
                return null;
            }

            var source = audioSourceParam;
            if (source == null)
            {
                var _obj = new GameObject("Sound", typeof(AudioSource));
                source = _obj.GetComponent<AudioSource>();
            }

            source.clip = GetAudioClip();
            source.volume = Random.Range(volume.x, volume.y);
            source.pitch = useSemitones
                ? Mathf.Pow(SEMITONES_TO_PITCH_CONVERSION_UNIT, Random.Range(semitones.x, semitones.y))
                : Random.Range(pitch.x, pitch.y);

            source.Play();

#if UNITY_EDITOR
            if (source != previewer)
            {
                Destroy(source.gameObject, source.clip.length / source.pitch);
            }
#else
                Destroy(source.gameObject, source.clip.length / source.pitch);
#endif
            return source;
        }

        enum SoundClipPlayOrder
        {
            Random,
            InOrder,
            InReverseOrder
        }

    }
}