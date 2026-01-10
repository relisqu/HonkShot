using System;
using System.Collections;
using System.Collections.Generic;
using Scripts.Health;
using Scripts.LevelSystem;
using UnityEngine;
using Zenject;
using Random = UnityEngine.Random;

namespace Scripts.Player
{
    public class FireParticleSystem : MonoBehaviour
    {
        private LevelTransitionManager _levelTransitionManager;

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
        [SerializeField] private Sprite _defaultSprite;
        [SerializeField] private Color _ultimateColor = new Color(255f / 255f, 231f / 255f, 55f / 255f, 1f);
        private float _defaultRateOverTime;
        private float _defaultTrailRendererTime;


        [Inject]
        public void Construct(LevelTransitionManager levelTransitionManager)
        {
            _levelTransitionManager = levelTransitionManager;
        }

        private void Start()
        {
            _defaultTrailRendererTime = _trailRenderer.time;
            _defaultRateOverTime = _particleSystems[0].emission.rateOverTime.constant;
            if (_gooseFireSystem == null)
                _gooseFireSystem = FindFirstObjectByType<GooseFireSystem>();


            _levelTransitionManager.LevelTransitionStarted += LevelTransitionManager_LevelTransitionStarted;
            _levelTransitionManager.LevelTransitionFinished += LevelTransitionManager_LevelTransitionFinished;
        }


        private void OnDestroy()
        {
            _levelTransitionManager.LevelTransitionStarted -= LevelTransitionManager_LevelTransitionStarted;
            _levelTransitionManager.LevelTransitionFinished -= LevelTransitionManager_LevelTransitionFinished;
        }

        private void LevelTransitionManager_LevelTransitionFinished()
        {
            ResumeParticleSystems();
        }

        private void LevelTransitionManager_LevelTransitionStarted()
        {
            ClearParticleSystems();
        }

        public void ClearParticleSystems()
        {
            Debug.Log("FireParticleSystem:: ClearParticleSystems");
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.Clear();
                particleSystem.Stop();
            }

            if (_clearParticleCoroutine != null)
            {
                StopCoroutine(_clearParticleCoroutine);
            }
            _clearParticleCoroutine = StartCoroutine(ClearTrailCoroutine());
        }

        private Coroutine _clearParticleCoroutine;

        private IEnumerator ClearTrailCoroutine()
        {
            if (_clearParticleCoroutine != null)
                StopCoroutine(_clearParticleCoroutine);

            _trailRenderer.time = 0.1f;
            yield return new WaitForSeconds(0.1f);
            Debug.Log("FireParticleSystem:: Clear Trail");
            _trailRenderer.gameObject.SetActive(false);
        }

        public void ResumeParticleSystems()
        {
            Debug.Log("FireParticleSystem:: ResumeParticleSystems");
            foreach (var particleSystem in _particleSystems)
            {
                particleSystem.Play();
            }

            _trailRenderer.gameObject.SetActive(true);
            SetTrailVisual();
        }

        private void SetTrailVisual()
        {
            if (_gooseFireSystem)
            {
                if (_gooseFireSystem.IsUltimate)
                {
                    _trailRenderer.time = _defaultTrailRendererTime * 2f;
                    _trailRenderer.colorGradient = _fireTrailGradient;
                }
                else
                {
                    _trailRenderer.time = _defaultTrailRendererTime;
                    _trailRenderer.colorGradient = new Gradient();
                    _gooseSpriteRenderer.color = Color.white;
                }
            }
        }

        private void Update()
        {
            float rateMultiplier = 1f;
            ParticleSystem.MinMaxGradient particleGradient = new ParticleSystem.MinMaxGradient(_defaultGradient);

            if (_trailRenderer.gameObject.activeInHierarchy)
            {
                SetTrailVisual();
            }

            if (_gooseFireSystem)
            {
                if (_gooseFireSystem.IsHonk)
                {
                    rateMultiplier = _honkRateMultiplier;
                    particleGradient = new ParticleSystem.MinMaxGradient(_honkGradient);
                }
                else if (_gooseFireSystem.IsUltimate)
                {
                    _trailRenderer.time = _defaultTrailRendererTime * 2f;
                    rateMultiplier = _ultimateRateMultiplier;
                    float t = _gooseFireSystem.Fire / _gooseFireSystem.MaxFire;
                    Gradient lerpedGradient = LerpGradient(_ultimateGradient, _defaultGradient, 1f - t);
                    particleGradient = new ParticleSystem.MinMaxGradient(lerpedGradient);
                    _gooseSpriteRenderer.color = _ultimateColor;
                }
                else
                {
                    _gooseSpriteRenderer.color = Color.white;
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
                main.startColor = particleGradient;

                emission.rateOverTime = _defaultRateOverTime *
                                        _playerAttackController.GetSpeedModifier() *
                                        _playerAttackController.GetSpeedModifier() *
                                        rateMultiplier;
                if (_gooseFireSystem && _gooseFireSystem.IsUltimate && particleSystem.isPlaying)
                {
                    particleSystem.Emit(1);
                }

                if (_gooseFireSystem && _gooseFireSystem.IsHonk && particleSystem.isPlaying)
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