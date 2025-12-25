using System;
using System.Collections.Generic;
using Scripts.Health;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Scripts.Player
{
    public class FireParticleSystem : MonoBehaviour
    {
        [SerializeField] private PlayerAttackController _playerAttackController;
        [SerializeField] private PlayerMovement _playerMovement;
        [SerializeField] private List<ParticleSystem> _particleSystems;
        [SerializeField] private GooseFireSystem _gooseFireSystem;

        [Header("Fire Effects")] [SerializeField]
        private float _honkRateMultiplier = 3f;

        [SerializeField] private float _ultimateRateMultiplier = 5f;
        [SerializeField] private Gradient _defaultGradient;
        [SerializeField] private Gradient _fireTrailGradient;
        [SerializeField] private Gradient _ultimateGradient;
        [SerializeField] private Gradient _honkGradient;
        [SerializeField] private TrailRenderer _trailRenderer;
        [SerializeField] private SpriteRenderer _gooseSpriteRenderer;
        [SerializeField] private SpriteRenderer _rotatedDetailsSpriteRenderer;
        [SerializeField] private Sprite _blockedSprite;
        [SerializeField] private Sprite _defaultSprite;

        private float _defaultRateOverTime;
        private float _defaultTrailRendererTime;

        private void Start()
        {
            _defaultTrailRendererTime = _trailRenderer.time;
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
                    _rotatedDetailsSpriteRenderer.sprite = _blockedSprite;
                    _gooseSpriteRenderer.color = new Color(22f / 255f, 147f / 255f, 105f / 255f, 1f);
                }
                else if (_gooseFireSystem.IsUltimate)
                {
                    _trailRenderer.time = _defaultTrailRendererTime * 2f;
                    rateMultiplier = _ultimateRateMultiplier;
                    float t = _gooseFireSystem.Fire / _gooseFireSystem.MaxFire;
                    Gradient lerpedGradient = LerpGradient(_ultimateGradient, _defaultGradient, 1f - t);
                    gradient = new ParticleSystem.MinMaxGradient(lerpedGradient);
                    _trailRenderer.colorGradient = _fireTrailGradient;
                    
                    _rotatedDetailsSpriteRenderer.sprite = _defaultSprite;
                    _gooseSpriteRenderer.color = new Color(255f / 255f, 231f / 255f, 55f / 255f, 1f);
                }
                else
                {
                    _trailRenderer.colorGradient = new Gradient();
                    _gooseSpriteRenderer.color = Color.white;
                    _rotatedDetailsSpriteRenderer.sprite = _defaultSprite;
                    _trailRenderer.time = _defaultTrailRendererTime;
                }
            }
            else
            {
                _gooseSpriteRenderer.color = Color.white;
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
                if (_gooseFireSystem && _gooseFireSystem.IsUltimate)
                {
                    particleSystem.Emit(1);
                }

                if (_gooseFireSystem && _gooseFireSystem.IsHonk)
                {
                    particleSystem.Emit(Random.Range(0, 2) * 1);
                }
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