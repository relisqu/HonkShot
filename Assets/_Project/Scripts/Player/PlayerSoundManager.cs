using Scripts.Audio;
using Scripts.LevelSystem.LevelObjects;
using Scripts.Player.InputHandling;
using UnityEngine;

namespace Scripts.Player
{
    /// <summary>
    /// Owns the player's non-impact audio: aiming a dash, the stretch while pulling, releasing it,
    /// the ultimate, and warping. Bounce impacts are handled by <see cref="PlayerBounceSoundEmitter"/>,
    /// which needs the collision.
    /// </summary>
    public class PlayerSoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private TeleportableEntity _teleportableEntity;
        [SerializeField] private GooseFireSystem _gooseFireSystem;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private PlayerBallMovement _playerBallMovement;

        [Header("Sound keys")]
        [Tooltip("Played when the player starts aiming a dash.")]
        [SerializeField] private string _dashStartSound = "dashStart";

        [Tooltip("Played when the player presses to dash with no charges left.")]
        [SerializeField] private string _dashFailSound = "dashFail";

        [SerializeField] private string _releaseSound = "gooseRelease";
        [SerializeField] private string _ultimateStartSound = "ultStart";
        [SerializeField] private string _ultimateEndSound = "ultEnd";
        [SerializeField] private string _warpSound = "warp";
        [SerializeField] private string _stepSound = "step";
        [SerializeField] private string _rollSound = "roll";

        [Tooltip("Looped tension while the goose is stretched for a dash.")]
        [SerializeField] private string _stretchSound = "stretch";

        [Header("Stretch")]
        [Tooltip("Volume multiplier at no pull (x) and full pull (y).")]
        [SerializeField] private Vector2 _stretchVolumeRange = new Vector2(0.3f, 1f);

        [Tooltip("Pitch multiplier at no pull (x) and full pull (y).")]
        [SerializeField] private Vector2 _stretchPitchRange = new Vector2(0.9f, 1.3f);

        [Tooltip("Shapes how pull strength maps onto the volume and pitch ranges.")]
        [SerializeField] private AnimationCurve _stretchResponse = AnimationCurve.Linear(0f, 0f, 1f, 1f);

        [Tooltip("Real-time seconds the sound takes to catch up with the pull. Smooths out jittery input.")]
        [SerializeField] private float _stretchFollowTime = 0.06f;

        [Tooltip("Real-time seconds to fade out after release or cancel.")]
        [SerializeField] private float _stretchFadeOutTime = 0.08f;

        private AudioSource _stretchSource;
        private bool _isStretching;
        private float _stretchPull;
        private float _stretchBaseVolume;
        private float _stretchBasePitch;
        private float _stretchFadeVolume;

        private void Awake()
        {
            if (!_inputHandler)
                _inputHandler = GetComponent<InputHandler>();
            if (!_playerBallMovement)
                _playerBallMovement = GetComponent<PlayerBallMovement>();

            // The Player channel shares one AudioSource with every other player sound, so the
            // stretch gets its own: it has to change volume and pitch live and stop on release
            // without cutting off the dash-start and release one-shots.
            _stretchSource = gameObject.AddComponent<AudioSource>();
            _stretchSource.playOnAwake = false;
            _stretchSource.loop = true;
        }

        private void OnEnable()
        {
            if (_teleportableEntity)
                _teleportableEntity.Teleported += TeleportableEntity_Teleported;

            if (_playerMovement)
            {
                _playerMovement.DragStarted += PlayerMovement_DragStarted;
                _playerMovement.DragFinished += PlayerMovement_DragFinished;
                _playerMovement.InputCancelled += PlayerMovement_InputCancelled;
                _playerMovement.DashFailed += PlayerMovement_DashFailed;
            }

            if (_gooseFireSystem)
            {
                _gooseFireSystem.UltimateStarted += GooseFireSystem_UltimateStarted;
                _gooseFireSystem.UltimateEnded += GooseFireSystem_UltimateEnded;
            }
        }

        private void OnDisable()
        {
            if (_teleportableEntity)
                _teleportableEntity.Teleported -= TeleportableEntity_Teleported;

            if (_playerMovement)
            {
                _playerMovement.DragStarted -= PlayerMovement_DragStarted;
                _playerMovement.DragFinished -= PlayerMovement_DragFinished;
                _playerMovement.InputCancelled -= PlayerMovement_InputCancelled;
                _playerMovement.DashFailed -= PlayerMovement_DashFailed;
            }

            if (_gooseFireSystem)
            {
                _gooseFireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;
                _gooseFireSystem.UltimateEnded -= GooseFireSystem_UltimateEnded;
            }

            _isStretching = false;
            if (_stretchSource)
                _stretchSource.Stop();
        }

        private void Update()
        {
            if (!_stretchSource.isPlaying) return;

            // Aiming slows the game to a crawl, so everything here runs on real time.
            float deltaTime = Time.unscaledDeltaTime;

            if (_isStretching)
            {
                float targetPull = GetPull();
                _stretchPull = _stretchFollowTime > 0f
                    ? Mathf.MoveTowards(_stretchPull, targetPull, deltaTime / _stretchFollowTime)
                    : targetPull;
            }
            else
            {
                _stretchFadeVolume = _stretchFadeOutTime > 0f
                    ? Mathf.MoveTowards(_stretchFadeVolume, 0f, deltaTime / _stretchFadeOutTime)
                    : 0f;

                if (_stretchFadeVolume <= 0f)
                {
                    _stretchSource.Stop();
                    return;
                }
            }

            ApplyStretchMix();
        }

        private void TeleportableEntity_Teleported() => PlayOneShot(_warpSound);

        // DragStarted only fires once the dash charge was actually spent, so this cannot double up
        // with the failure sound below.
        private void PlayerMovement_DragStarted()
        {
            PlayOneShot(_dashStartSound);
            StartStretch();
        }

        // DragFinished is raised only for a drag that really became an aim; a cancelled input goes
        // through InputCancelled instead and stays silent.
        private void PlayerMovement_DragFinished(Vector2 _)
        {
            StopStretch();
            PlayOneShot(_releaseSound);
        }

        private void PlayerMovement_InputCancelled() => StopStretch();

        private void PlayerMovement_DashFailed() => PlayOneShot(_dashFailSound);

        private void GooseFireSystem_UltimateStarted() => PlayOneShot(_ultimateStartSound);

        private void GooseFireSystem_UltimateEnded() => PlayOneShot(_ultimateEndSound);

        public void PlayStepSound() => PlayOneShot(_stepSound);

        public void PlayRollSound()
        {
            if (string.IsNullOrEmpty(_rollSound) || !AudioManager.Instance) return;
            AudioManager.Instance.Play(SoundChanelType.Player, _rollSound, loop: true);
        }

        public void StopRollSound()
        {
            if (string.IsNullOrEmpty(_rollSound) || !AudioManager.Instance) return;
            AudioManager.Instance.Stop(SoundChanelType.Player, _rollSound);
        }

        public void PlayReleaseSound() => PlayOneShot(_releaseSound);

        private void StartStretch()
        {
            if (string.IsNullOrEmpty(_stretchSound) || !AudioManager.Instance) return;

            var soundEffect = AudioManager.Instance.GetSoundEffectSO(_stretchSound);
            if (!soundEffect) return;

            var clip = soundEffect.GetAudioClip();
            if (!clip) return;

            // Roll the effect's own volume/pitch variation once per stretch; the pull scales on top.
            soundEffect.ApplyRandomSettings(_stretchSource);
            _stretchBaseVolume = _stretchSource.volume;
            _stretchBasePitch = _stretchSource.pitch;

            _isStretching = true;
            _stretchPull = GetPull();
            _stretchFadeVolume = 1f;

            _stretchSource.clip = clip;
            ApplyStretchMix();
            _stretchSource.Play();
        }

        private void StopStretch() => _isStretching = false;

        private void ApplyStretchMix()
        {
            float response = _stretchResponse.Evaluate(_stretchPull);
            float channelVolume = AudioManager.Instance
                ? AudioManager.Instance.GetChannelVolume(SoundChanelType.Player)
                : 1f;

            _stretchSource.volume = _stretchBaseVolume
                                    * Mathf.Lerp(_stretchVolumeRange.x, _stretchVolumeRange.y, response)
                                    * _stretchFadeVolume
                                    * channelVolume;
            _stretchSource.pitch = _stretchBasePitch
                                   * Mathf.Lerp(_stretchPitchRange.x, _stretchPitchRange.y, response);
        }

        private float GetPull()
        {
            if (!_inputHandler || !_playerBallMovement || _playerBallMovement.MaxForceMagnitude <= 0f) return 0f;
            return Mathf.Clamp01(_inputHandler.GetCurrentDrag().magnitude / _playerBallMovement.MaxForceMagnitude);
        }

        private void PlayOneShot(string soundName)
        {
            if (string.IsNullOrEmpty(soundName) || !AudioManager.Instance) return;
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, soundName);
        }
    }
}
