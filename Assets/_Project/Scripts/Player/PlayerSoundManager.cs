using Scripts.Audio;
using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Player
{
    /// <summary>
    /// Owns the player's non-impact audio: aiming a dash, releasing it, the ultimate, and warping.
    /// Bounce impacts are handled by <see cref="PlayerBounceSoundEmitter"/>, which needs the collision.
    /// </summary>
    public class PlayerSoundManager : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private TeleportableEntity _teleportableEntity;
        [SerializeField] private GooseFireSystem _gooseFireSystem;

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
        [SerializeField] private string _stretchSound = "stretch";

        private void OnEnable()
        {
            if (_teleportableEntity)
                _teleportableEntity.Teleported += TeleportableEntity_Teleported;

            if (_playerMovement)
            {
                _playerMovement.DragStarted += PlayerMovement_DragStarted;
                _playerMovement.DragFinished += PlayerMovement_DragFinished;
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
                _playerMovement.DashFailed -= PlayerMovement_DashFailed;
            }

            if (_gooseFireSystem)
            {
                _gooseFireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;
                _gooseFireSystem.UltimateEnded -= GooseFireSystem_UltimateEnded;
            }
        }

        private void TeleportableEntity_Teleported() => PlayOneShot(_warpSound);

        // DragStarted only fires once the dash charge was actually spent, so this cannot double up
        // with the failure sound below.
        private void PlayerMovement_DragStarted() => PlayOneShot(_dashStartSound);

        // DragFinished is raised only for a drag that really became an aim; a cancelled input goes
        // through InputCancelled instead and stays silent.
        private void PlayerMovement_DragFinished(Vector2 _) => PlayOneShot(_releaseSound);

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

        public void PlayStretchSound()
        {
            if (string.IsNullOrEmpty(_stretchSound) || !AudioManager.Instance) return;
            AudioManager.Instance.Play(SoundChanelType.Player, _stretchSound);
        }

        public void PlayReleaseSound() => PlayOneShot(_releaseSound);

        private void PlayOneShot(string soundName)
        {
            if (string.IsNullOrEmpty(soundName) || !AudioManager.Instance) return;
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, soundName);
        }
    }
}
