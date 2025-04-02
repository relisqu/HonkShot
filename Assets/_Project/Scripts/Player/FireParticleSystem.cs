using System;
using System.Collections.Generic;
using Scripts.Health;
using UnityEngine;

namespace Scripts.Player
{
    public class FireParticleSystem : MonoBehaviour
    {
        [SerializeField] private PlayerAttackController _playerAttackController;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private List<ParticleSystem> _particleSystems;

        private float _defaultRateOverTime;

        private void Start()
        {
            _defaultRateOverTime = _particleSystems[0].emission.rateOverTime.constant;
        }

        private void Update()
        {
            foreach (var particleSystem in _particleSystems)
            {
                var module = particleSystem.emission;
                var mainModule = particleSystem.main;
                mainModule.emitterVelocity = new Vector3(_playerMovement.GetVelocity().x, 0, 0);

                module.rateOverTime = _defaultRateOverTime *
                                      _playerAttackController.GetSpeedModifier()*
                                          _playerAttackController.GetSpeedModifier();
            }
        }
    }
}