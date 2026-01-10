using UnityEngine;
using DG.Tweening;

namespace Scripts.Health
{
    public class Shield : MonoBehaviour
    {
        [Header("Shield Settings")] 
        public float damageThreshold = 5f;
        
        [Header("Visual Settings")]
        [SerializeField] private ShieldVisual _shieldVisual;
        [SerializeField] private SpriteRenderer _spriteRenderer;
        
        

        private bool _isActive = true;


        public bool CanBlockDamage(float damage)
        {
            return _isActive && damage <= damageThreshold;
        }

        public float AbsorbDamage(float damage)
        {
            if (!CanBlockDamage(damage))
            {
                // Shield will be destroyed
                _isActive = false;
                return damage;
            }

            return 0f; // Damage below threshold, ignored
        }

        public virtual void DestroyShield()
        {
            Debug.Log("Shield destroyed");
            
            // Play destruction animation
            if (_spriteRenderer)
            {
                _spriteRenderer.DOFade(0f, 0.3f)
                    .SetEase(Ease.InQuad)
                    .OnComplete(() => {
                        if (_shieldVisual)
                        {
                            _shieldVisual.SetActive(false);
                        }
                        Destroy(gameObject);
                    });
                
                transform.DOScale(0f, 0.3f)
                    .SetEase(Ease.InBack);
            }
            else
            {
                Destroy(gameObject);
            }
        }

        public virtual void ActivateShield()
        {
            _isActive = true;
            
            if (_spriteRenderer)
            {
                _spriteRenderer.DOFade(1f, 0.2f);
            }
            
            if (_shieldVisual)
            {
                _shieldVisual.SetActive(true);
            }
        }
      


    }
}