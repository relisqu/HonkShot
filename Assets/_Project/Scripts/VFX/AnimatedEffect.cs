using System.Collections;
using Scripts.Services.Pooling;
using UnityEngine;

namespace Scripts.VFX
{
    /// <summary>
    /// One-shot effect driven by its Animator. The Animator restarts its default state every time the
    /// pooled object is re-enabled; once that state has played through, the effect goes back to its pool.
    /// </summary>
    public class AnimatedEffect : MonoBehaviour, IPoolable<AnimatedEffect>
    {
        [SerializeField] private Animator _animator;

        private ComponentPool<AnimatedEffect> _pool;

        public void SetPool(ComponentPool<AnimatedEffect> pool) =>
            _pool = pool;

        private void OnEnable()
        {
            StartCoroutine(ReturnAfterAnimation());
        }

        private IEnumerator ReturnAfterAnimation()
        {
            // The state info is only valid once the Animator has been updated after enabling.
            yield return null;
            yield return new WaitForSeconds(_animator.GetCurrentAnimatorStateInfo(0).length);

            if (_pool != null)
                _pool.Return(this);
            else
                Destroy(gameObject);
        }
    }
}
