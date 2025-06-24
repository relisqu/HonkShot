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
        [SerializeField] private GooseFireSystem _gooseFireSystem;
        [Header("Fire Effects")]
        [SerializeField] private float _honkRateMultiplier = 3f;
        [SerializeField] private float _ultimateRateMultiplier = 5f;
        [SerializeField] private Gradient _defaultGradient;
        [SerializeField] private Gradient _ultimateGradient;
        [SerializeField] private Gradient _honkGradient;

        private float _defaultRateOverTime;

        private void Start()
        {
            _defaultRateOverTime = _particleSystems[0].emission.rateOverTime.constant;
            if (_gooseFireSystem == null)
                _gooseFireSystem = FindObjectOfType<GooseFireSystem>();
        }

        private void Update()
        {
            float rateMultiplier = 1f;
            ParticleSystem.MinMaxGradient gradient = new ParticleSystem.MinMaxGradient(_defaultGradient);

            if (_gooseFireSystem)
            {
                if (_gooseFireSystem.IsHonk)
                {
                    rateMultiplier = _honkRateMultiplier;
                    gradient = new ParticleSystem.MinMaxGradient(_honkGradient);
                }
                else if (_gooseFireSystem.IsUltimate)
                {
                    rateMultiplier = _ultimateRateMultiplier;
                    float t = _gooseFireSystem.Fire / _gooseFireSystem.MaxFire;
                    Gradient lerpedGradient = LerpGradient(_ultimateGradient, _defaultGradient, 1f - t);
                    gradient = new ParticleSystem.MinMaxGradient(lerpedGradient);
                }
            }

            foreach (var particleSystem in _particleSystems)
            {
                var emission = particleSystem.emission;
                var main = particleSystem.main;
                main.emitterVelocity = new Vector3(_playerMovement.GetVelocity().x, 0, 0);
                main.startColor = gradient;

                emission.rateOverTime = _defaultRateOverTime *
                                        _playerAttackController.GetSpeedModifier() *
                                        _playerAttackController.GetSpeedModifier() *
                                        rateMultiplier;
            }
        }

        private Gradient LerpGradient(Gradient a, Gradient b, float t)
        {
            Gradient result = new Gradient();
            GradientColorKey[] colorKeys = new GradientColorKey[a.colorKeys.Length];
            GradientAlphaKey[] alphaKeys = new GradientAlphaKey[a.alphaKeys.Length];
            for (int i = 0; i < colorKeys.Length; i++)
            {
                Color colorA = a.colorKeys[i].color;
                Color colorB = b.colorKeys.Length > i ? b.colorKeys[i].color : b.colorKeys[0].color;
                float time = a.colorKeys[i].time;
                colorKeys[i] = new GradientColorKey(Color.Lerp(colorA, colorB, t), time);
            }
            for (int i = 0; i < alphaKeys.Length; i++)
            {
                float alphaA = a.alphaKeys[i].alpha;
                float alphaB = b.alphaKeys.Length > i ? b.alphaKeys[i].alpha : b.alphaKeys[0].alpha;
                float time = a.alphaKeys[i].time;
                alphaKeys[i] = new GradientAlphaKey(Mathf.Lerp(alphaA, alphaB, t), time);
            }
            result.SetKeys(colorKeys, alphaKeys);
            return result;
        }
    }
}