using System;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.Player
{
    public class GrassTrail : MonoBehaviour
    {
        [SerializeField] private PlayerStatus _playerStatus;
        [SerializeField] private GooseFireSystem _gooseFireSystem;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private ParticleSystem _trailParticles;
        [SerializeField] private Color _defaultColor;
        [SerializeField] private Color _ultColor;
        

        private Vector3 _startTrailParticleSystemScale;

        private void OnEnable()
        {
            _startTrailParticleSystemScale = _trailParticles.shape.scale;
        }

        private void Start()
        {
            LevelManager.Instance.EnteredRoom += () =>
            {
                if (_trailParticles)
                    _trailParticles.Clear();
            };
        }

        private void Update()
        {
            var distance = (_trailParticles.transform.position - _playerMovement.transform.position).magnitude;


            _trailParticles.transform.position = _playerMovement.transform.position;
            if (distance < 1f && _playerMovement.GetVelocity().sqrMagnitude > 0.1f)
            {
                if (!_trailParticles.isEmitting)
                {
                    _trailParticles.Play();
                }
            }
            else
            {
                _trailParticles.Stop();
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

            if (_gooseFireSystem && _gooseFireSystem.IsUltimate)
            {
                var main = _trailParticles.main;
                main.startColor = _ultColor;
            }
            else
            {
                var main = _trailParticles.main;
                main.startColor = _defaultColor;
            }
        }
    }
}