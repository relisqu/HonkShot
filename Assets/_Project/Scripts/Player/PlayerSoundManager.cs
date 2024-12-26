using System;
using Scripts.Audio;
using Scripts.LevelSystem;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerSoundManager : MonoBehaviour
    {
        [SerializeField] private PlayerBallMovement _playerBallMovement;
        [SerializeField] private InputHandler _inputHandler;
        [SerializeField] private TeleportableEntity _teleportableEntity;
        [SerializeField] private float _stretchSoundDragForce;

        private bool _playedStretchSound;

        private void OnEnable()
        {
            _inputHandler.OnDragFinished += InputHandler_DragFinished;
            _teleportableEntity.Teleported += TeleportableEntity_Teleported;
        }

        private void OnDisable()
        {
            _inputHandler.OnDragFinished -= InputHandler_DragFinished;
            _teleportableEntity.Teleported -= TeleportableEntity_Teleported;
        }

        private void TeleportableEntity_Teleported()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "warp");
        }

        private void InputHandler_DragFinished(Vector2 _)
        {
            PlayReleaseSound();
        }

        private void Update()
        {
            if (_inputHandler.IsDragging)
            {
                InputHandler_Drag(_inputHandler.GetCurrentDragMagnitude());
            }
        }

        private void InputHandler_Drag(float magnitude)
        {
            if (!_playedStretchSound && magnitude > _stretchSoundDragForce)
            {
                PlayStretchSound();
                _playedStretchSound = true;
            }

            if (magnitude < _stretchSoundDragForce)
            {
                _playedStretchSound = false;
            }
        }


        public void PlayStepSound()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "step");
        }


        public void PlayRollSound()
        {
            AudioManager.Instance.Play(SoundChanelType.Player, "roll", loop: true);
        }

        public void StopRollSound()
        {
            AudioManager.Instance.Stop(SoundChanelType.Player, "roll");
        }


        public void PlayStretchSound()
        {
            AudioManager.Instance.Play(SoundChanelType.Player, "stretch");
        }

        public void PlayReleaseSound()
        {
            AudioManager.Instance.PlayOneShot(SoundChanelType.Player, "gooseRelease");
        }
    }
}