using DG.Tweening;
using Scripts.Health;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class Hole : MonoBehaviour
    {
        [SerializeField] private float swirlDuration = 0.6f;
        [SerializeField] private float swirlScale = 0.2f;
        [SerializeField] private float teleportOffset = 0.2f;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!collision.TryGetComponent(out HoleDamageable holeDamageable)) return;
            var targetTransform = holeDamageable.transform;
            Vector3 originalScale = holeDamageable.transform.localScale;


            // Animate swirl
            Sequence swirlSequence = DOTween.Sequence();
            swirlSequence.Append(targetTransform.DOScale(swirlScale, swirlDuration).SetEase(Ease.InCirc));
            swirlSequence.Join(targetTransform.DORotate(new Vector3(0, 0, 2 * 360f), swirlDuration,
                RotateMode.FastBeyond360));
            swirlSequence.Join(targetTransform.transform.DOMove(transform.position, swirlDuration * 0.8f)
                .SetEase(Ease.Linear));
            swirlSequence.OnComplete(() =>
            {
                // Determine spawn point
                Transform spawn = LevelManager.Instance.CurrentRoom.SpawnPointTransform;

                if (spawn != null)
                {
                    Vector2 offset = Random.insideUnitCircle * teleportOffset;
                    targetTransform.position = spawn.position + (Vector3)offset;
                }


                // Damage if has health
                holeDamageable.TakeDamage();

                targetTransform.localScale = Vector3.one;
                targetTransform.rotation = Quaternion.identity;
            });
        }
    }
}