using DG.Tweening;
using UnityEngine;

namespace Scripts.Player
{
    public class DefaultExplosionVFX : MonoBehaviour
    {
        [SerializeField] private float _scaleUpDuration = 0.15f;
        [SerializeField] private float _scaleDownDuration = 0.2f;
        [SerializeField] private Explosion _explosion;
        [SerializeField] private ParticleSystem _particleSystem;
        [SerializeField] private float _particleFadeOutTime = 0.3f;
        [SerializeField] private float _ratePerDistanceMultiplier = 1f;

        private void OnEnable()
        {
            transform.localScale = Vector3.zero;

            if (_particleSystem)
            {
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
                float radius = _explosion ? _explosion.Radius : 2f;
                float halfRadius = radius * 0.5f;
                var shape = _particleSystem.shape;
                shape.radius = halfRadius;
                var emission = _particleSystem.emission;
                emission.rateOverTimeMultiplier = 1f;
                emission.rateOverDistanceMultiplier = halfRadius * _ratePerDistanceMultiplier;
                _particleSystem.Play();
            }

            transform.DOScale(Vector3.one, _scaleUpDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    if (_particleSystem)
                        FadeOutParticles();

                    transform.DOScale(Vector3.zero, _scaleDownDuration)
                        .SetEase(Ease.InBack);
                });
        }

        private void FadeOutParticles()
        {
            var emission = _particleSystem.emission;
            DOTween.To(
                () => emission.rateOverTimeMultiplier,
                value => emission.rateOverTimeMultiplier = value,
                0f,
                _particleFadeOutTime
            );
        }

        private void OnDisable()
        {
            transform.DOKill();
            if (_particleSystem)
                _particleSystem.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }
}
