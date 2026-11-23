using DG.Tweening;
using UnityEngine;

namespace Scripts.VFX
{
    // Temporarily swaps a set of sprites to a flash material and restores each sprite's own material.
    public class SpriteFlasher
    {
        private readonly SpriteRenderer[] _renderers;
        private readonly Material[] _originalMaterials;
        private readonly bool[] _flashed;
        private Tween _restoreTimer;

        public SpriteFlasher(SpriteRenderer[] renderers)
        {
            _renderers = renderers;
            _originalMaterials = new Material[renderers.Length];
            _flashed = new bool[renderers.Length];
        }

        public bool IsFlashing => _restoreTimer != null && _restoreTimer.IsActive();

        // Hidden or fully transparent sprites are skipped so they don't show up as white shapes.
        public void Flash(Material flashMaterial, float duration, GameObject link)
        {
            if (!flashMaterial) return;

            if (IsFlashing)
                _restoreTimer.Kill();
            else
                ApplyFlashMaterial(flashMaterial);

            _restoreTimer = DOVirtual.DelayedCall(duration, RestoreTimer_Completed, false).SetLink(link);
        }

        public void Restore()
        {
            if (IsFlashing)
                _restoreTimer.Kill();
            _restoreTimer = null;
            RestoreMaterials();
        }

        private void RestoreTimer_Completed()
        {
            _restoreTimer = null;
            RestoreMaterials();
        }

        private void RestoreMaterials()
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                if (!_flashed[i]) continue;
                _flashed[i] = false;
                if (_renderers[i])
                    _renderers[i].sharedMaterial = _originalMaterials[i];
            }
        }

        private void ApplyFlashMaterial(Material flashMaterial)
        {
            for (int i = 0; i < _renderers.Length; i++)
            {
                var spriteRenderer = _renderers[i];
                _flashed[i] = false;
                if (!spriteRenderer || !spriteRenderer.enabled || !spriteRenderer.gameObject.activeInHierarchy) continue;
                if (spriteRenderer.color.a <= 0f) continue;

                _originalMaterials[i] = spriteRenderer.sharedMaterial;
                spriteRenderer.sharedMaterial = flashMaterial;
                _flashed[i] = true;
            }
        }
    }
}
