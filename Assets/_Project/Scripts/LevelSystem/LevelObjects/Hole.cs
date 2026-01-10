using System;
using DG.Tweening;
using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;
using System.Collections.Generic;
using Scripts.Player;
using Random = UnityEngine.Random;

namespace Scripts.LevelSystem.LevelObjects
{
    public class Hole : MonoBehaviour
    {
        [SerializeField] private float swirlDuration = 0.6f;
        [SerializeField] private float swirlScale = 0.2f;
        [SerializeField] private float teleportOffset = 0.2f;
        [SerializeField] private Transform _teleportTransformPoint;


        private void Awake()
        {
            if (_teleportTransformPoint) return;
            try
            {
                _teleportTransformPoint = transform.parent.parent.GetComponent<Room>().SpawnPointTransform;
            }
            catch (Exception e)
            {
                _teleportTransformPoint = LevelManager.Instance.CurrentRoom.SpawnPointTransform;
            }
        }

        private Dictionary<Transform, Sequence> _activeTweens = new Dictionary<Transform, Sequence>();

        private void OnDisable()
        {
            // Clean up all active tweens when the hole is disabled
            _activeTweens.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out HoleDamageable holeDamageable)) return;

            if (holeDamageable.HealthController.IsInvincible()) return;

            var targetTransform = holeDamageable.transform;

            // Cancel any existing tween for this transform
            if (_activeTweens.TryGetValue(targetTransform, out var existingTween))
            {
                return;
            }

            holeDamageable.GetComponent<Animator>().speed = 0;
            holeDamageable.Rigidbody2D.linearVelocity = Vector2.zero;

            // Create and track new sequence
            Sequence swirlSequence = DOTween.Sequence();
            _activeTweens[targetTransform] = swirlSequence;

            swirlSequence.Append(targetTransform.DOScale(swirlScale, swirlDuration)
                .SetEase(Ease.InCirc)
                .OnKill(() => ForceResetTransform(targetTransform)));

            swirlSequence.Join(targetTransform.DORotate(new Vector3(0, 0, 720f), swirlDuration,
                RotateMode.FastBeyond360));

            swirlSequence.Join(targetTransform.DOMove(transform.position, swirlDuration * 0.8f)
                .SetEase(Ease.Linear));

            if (collision.transform.TryGetComponent<FireParticleSystem>(out var fireParticleSystem))
            {
                fireParticleSystem.ClearParticleSystems();
            }

            swirlSequence.OnComplete(() =>
            {
                TeleportObject(targetTransform, holeDamageable, fireParticleSystem);
                CleanupTween(targetTransform);
            });

            swirlSequence.OnKill(() => CleanupTween(targetTransform));
        }

        private void TeleportObject(Transform targetTransform, HoleDamageable holeDamageable,
            FireParticleSystem fireParticleSystem = null)
        {
            holeDamageable.TakeDamage();
            if (!holeDamageable || !holeDamageable.isActiveAndEnabled || !holeDamageable.HealthController.IsAlive)
                return;

            ForceResetTransform(targetTransform);
            targetTransform.GetComponent<Animator>().speed = 1;
            if (_teleportTransformPoint != null)
            {
                Vector2 offset = Random.insideUnitCircle * teleportOffset;
                targetTransform.position = _teleportTransformPoint.position + (Vector3)offset;
            }

            if (fireParticleSystem != null)
            {
                fireParticleSystem.ResumeParticleSystems();
            }
        }

        private void ForceResetTransform(Transform target)
        {
            if (target == null) return;

            // Immediate reset without tween
            target.localScale = Vector3.one;
            target.rotation = Quaternion.identity;

            // Stop any lingering tweens
            DOTween.Kill(target);
        }

        private void CleanupTween(Transform target)
        {
            if (_activeTweens.ContainsKey(target))
            {
                _activeTweens.Remove(target);
            }
        }
    }
}