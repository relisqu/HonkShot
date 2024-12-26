using System;
using UnityEngine;

namespace Scripts.Player
{
    public class GrassTrail : MonoBehaviour
    {
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private ParticleSystem _trailParticles;

        private Vector3 _startTrailParticleSystemScale;

        private void OnEnable()
        {
            _startTrailParticleSystemScale = _trailParticles.shape.scale;
        }


        private void Update()
        {
            var distance = (_trailParticles.transform.position - _playerMovement.transform.position).magnitude;
            

            _trailParticles.transform.position = _playerMovement.transform.position;
            if (distance < 1f && _playerMovement.GetVelocity().sqrMagnitude > 0.1f)
            {
                Debug.Log("Playing");
                if (!_trailParticles.isEmitting)
                {
                    _trailParticles.Play();
                }
            }
            else
            {
                _trailParticles.Stop();
                Debug.Log("Stop");
                // _trailParticles.Stop();
            }

            var shape = _trailParticles.shape;
            if (_playerStatus.PlayerState == PlayerState.Ball)
            {
                shape.scale = _startTrailParticleSystemScale * 1.3f;
            }
            else
            {
                shape.scale = _startTrailParticleSystemScale * 1f;
            }
        }
    }
}