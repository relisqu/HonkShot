using System.Collections.Generic;
using Scripts.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Scripts.UI
{
    public class HpBarFireEffect : MonoBehaviour
    {
        [SerializeField] private List<ParticleSystem> _fireParticles;
        [SerializeField] private ParticleSystem _mainParticle;
        [SerializeField] private ParticleSystem _ultBurstParticle;
        [SerializeField] private Slider _hpSlider;
        [SerializeField] private float _maxEmitterScale = 5f;

        [Header("Threshold")] [SerializeField] [Range(0f, 1f)]
        private float _fireThreshold = 0.15f;

        [Header("Lifetime")] [SerializeField] private float _minLifetime = 0.3f;
        [SerializeField] private float _maxLifetime = 1.5f;
        [SerializeField] private float _lifetimeLerpSpeed = 3f;

        [Header("Emission")] [SerializeField] private float _minEmissionRate = 5f;
        [SerializeField] private float _maxEmissionRate = 40f;

        [Header("Gradient")] [SerializeField] private Gradient _lowFireGradient;
        [SerializeField] private Gradient _highFireGradient;

        [Header("Ult Burst")] [SerializeField] private int _ultBurstCount = 20;

        private GooseFireSystem _gooseFireSystem;
        private float _targetLifetime;
        private float _currentLifetime;
        private float _fireT;
        private bool _isActive;

        private void Start()
        {
            _gooseFireSystem = GooseFireSystem.Instance;
            if (!_gooseFireSystem) return;

            _currentLifetime = _minLifetime;
            _targetLifetime = _minLifetime;
            SetParticlesActive(false);

            _gooseFireSystem.FireChanged += GooseFireSystem_FireChanged;
            _gooseFireSystem.UltimateStarted += GooseFireSystem_UltimateStarted;
        }

        private void OnDestroy()
        {
            if (!_gooseFireSystem) return;
            _gooseFireSystem.FireChanged -= GooseFireSystem_FireChanged;
            _gooseFireSystem.UltimateStarted -= GooseFireSystem_UltimateStarted;
        }

        private void GooseFireSystem_FireChanged(float fire)
        {
            float rawT = fire / _gooseFireSystem.MaxFire;
            bool shouldBeActive = rawT >= _fireThreshold;

            if (shouldBeActive != _isActive)
                SetParticlesActive(shouldBeActive);

            _fireT = shouldBeActive ? (rawT - _fireThreshold) / (1f - _fireThreshold) : 0f;
            _targetLifetime = Mathf.Lerp(_minLifetime, _maxLifetime, _fireT);
        }

        private void SetParticlesActive(bool active)
        {
            _isActive = active;
            if (active) _mainParticle.Play();
            else _mainParticle.Stop(true, ParticleSystemStopBehavior.StopEmitting);
        }

        private void GooseFireSystem_UltimateStarted()
        {
            if (_ultBurstParticle)
                _ultBurstParticle.Emit(_ultBurstCount);
        }

        private void Update()
        {
            if (!_gooseFireSystem || !_isActive) return;

            _currentLifetime = Mathf.Lerp(_currentLifetime, _targetLifetime, _lifetimeLerpSpeed * Time.deltaTime);
            Gradient lerpedGradient = LerpGradient(_lowFireGradient, _highFireGradient, _fireT);
            float scaleX = _maxEmitterScale * _hpSlider.value;
            float scaleRatio = Mathf.Max(scaleX / _maxEmitterScale, 0.01f);
            float baseRate = Mathf.Lerp(_minEmissionRate, _maxEmissionRate, _fireT);
            float emissionRate = baseRate * scaleRatio;

            var main = _mainParticle.main;
            main.startLifetime = _currentLifetime;

            var emission = _mainParticle.emission;
            emission.rateOverTime = emissionRate;

            var colorOverLifetime = _mainParticle.colorOverLifetime;
            colorOverLifetime.color = new ParticleSystem.MinMaxGradient(lerpedGradient);

            foreach (var ps in _fireParticles)
            {
                var shape = ps.shape;
                shape.scale = new Vector3(scaleX, shape.scale.y, shape.scale.z);
            }
        }

        private Gradient LerpGradient(Gradient a, Gradient b, float t)
        {
            var result = new Gradient();
            var colorKeys = new GradientColorKey[a.colorKeys.Length];
            var alphaKeys = new GradientAlphaKey[a.alphaKeys.Length];

            for (int i = 0; i < colorKeys.Length; i++)
            {
                Color colorA = a.colorKeys[i].color;
                Color colorB = b.colorKeys.Length > i ? b.colorKeys[i].color : b.colorKeys[0].color;
                colorKeys[i] = new GradientColorKey(Color.Lerp(colorA, colorB, t), a.colorKeys[i].time);
            }

            for (int i = 0; i < alphaKeys.Length; i++)
            {
                float alphaA = a.alphaKeys[i].alpha;
                float alphaB = b.alphaKeys.Length > i ? b.alphaKeys[i].alpha : b.alphaKeys[0].alpha;
                alphaKeys[i] = new GradientAlphaKey(Mathf.Lerp(alphaA, alphaB, t), a.alphaKeys[i].time);
            }

            result.SetKeys(colorKeys, alphaKeys);
            return result;
        }
    }
}