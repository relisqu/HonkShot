using System.Collections;
using System.Collections.Generic;
using Scripts.Health;
using Scripts.Player;
using UnityEngine;

namespace Scripts.Other
{
    public class DamageBlinkModule : MonoBehaviour
    {
        [SerializeField] private HealthController _healthController;
        [SerializeField] private AnimationCurve _blinkCurve;
        [SerializeField] private float _blinkDuration = 0.5f;
        private List<SpriteRenderer> _spriteRenderers = new();
        private Coroutine _blinkCoroutine;


        private void Awake()
        {
            _healthController.OnDamaged += HealthController_Damaged;
            _spriteRenderers = new List<SpriteRenderer>(_healthController.GetComponentsInChildren<SpriteRenderer>());
        }

        private void OnDestroy()
        {
            _healthController.OnDamaged -= HealthController_Damaged;
        }

        private void HealthController_Damaged()
        {
            if (_blinkCoroutine == null)
                _blinkCoroutine = StartCoroutine(BlinkRoutine());
        }


        private IEnumerator BlinkRoutine()
        {
            float timer = 0f;

            while (_healthController.IsInvincible)
            {
                float alpha = _blinkCurve.Evaluate(timer / _blinkDuration);
                SetSpriteAlpha(alpha);

                timer += Time.deltaTime;
                if (timer >= _blinkDuration)
                {
                    timer = 0f; // Loop animation
                }

                yield return null;
            }

            SetSpriteAlpha(1f);
        }

        private void SetSpriteAlpha(float alpha)
        {
            foreach (var sprite in _spriteRenderers)
            {
                if (sprite != null)
                {
                    Color color = sprite.color;
                    color.a = alpha;
                    sprite.color = color;
                }
            }
        }
    }
}