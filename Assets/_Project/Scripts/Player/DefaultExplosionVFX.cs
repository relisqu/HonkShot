using DG.Tweening;
using UnityEngine;

namespace Scripts.Player
{
    public class DefaultExplosionVFX : MonoBehaviour
    {
        [SerializeField] private float _scaleUpDuration = 0.15f;
        [SerializeField] private float _scaleDownDuration = 0.2f;
        [SerializeField] private Explosion _explosion;

        private void OnEnable()
        {
            transform.localScale = Vector3.zero;
            transform.DOScale(Vector3.one, _scaleUpDuration)
                .SetEase(Ease.OutBack)
                .OnComplete(() =>
                {
                    _explosion.
                    transform.DOScale(Vector3.zero, _scaleDownDuration)
                        .SetEase(Ease.InBack);
                });
        }

        private void OnDisable()
        {
            transform.DOKill();
        }
    }
}
