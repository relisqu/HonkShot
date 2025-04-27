using DG.Tweening;
using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;
using System.Collections.Generic;

namespace Scripts.LevelSystem.LevelObjects
{
    public class Hole : MonoBehaviour
    {
        [SerializeField] private float swirlDuration = 0.6f;
        [SerializeField] private float swirlScale = 0.2f;
        [SerializeField] private float teleportOffset = 0.2f;

        private Dictionary<Transform, Sequence> _activeTweens = new Dictionary<Transform, Sequence>();

        private void OnDisable()
        {
            // Clean up all active tweens when the hole is disabled
            _activeTweens.Clear();
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out HoleDamageable holeDamageable)) return;

            var targetTransform = holeDamageable.transform;

            // Cancel any existing tween for this transform
            if (_activeTweens.TryGetValue(targetTransform, out var existingTween))
            {
                existingTween.Kill();
                ForceResetTransform(targetTransform);
            }

            holeDamageable.GetComponent<Animator>().speed = 0;
            holeDamageable.Rigidbody2D.velocity = Vector2.zero;

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

            swirlSequence.OnComplete(() =>
            {
                TeleportObject(targetTransform, holeDamageable);
                CleanupTween(targetTransform);
            });

            swirlSequence.OnKill(() => CleanupTween(targetTransform));
        }

        private void TeleportObject(Transform targetTransform, HoleDamageable holeDamageable)
        {
            Transform spawn = LevelManager.Instance.CurrentRoom.SpawnPointTransform;
            if (spawn != null)
            {
                Vector2 offset = Random.insideUnitCircle * teleportOffset;
                targetTransform.position = spawn.position + (Vector3)offset;
            }

            holeDamageable.TakeDamage();
            ForceResetTransform(targetTransform);
            targetTransform.GetComponent<Animator>().speed = 1;
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