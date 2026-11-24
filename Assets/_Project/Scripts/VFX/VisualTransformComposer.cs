using System.Collections.Generic;
using UnityEngine;

namespace Scripts.VFX
{
    // Single owner of the juice applied to one visual transform. Several effects (hit wobble, shooter recoil...)
    // post their scale multiplier / world offset each frame; this combines them and applies once, after all of
    // them, so independent effects never compound on each other.
    [DefaultExecutionOrder(1000)]
    [DisallowMultipleComponent]
    public class VisualTransformComposer : MonoBehaviour
    {
        private struct Contribution
        {
            public Vector3 ScaleMultiplier;
            public Vector3 WorldOffset;
        }

        private readonly Dictionary<Object, Contribution> _contributions = new();
        private VisualTransformLayer _layer;

        public static VisualTransformComposer GetOrAdd(Transform target)
        {
            return target.TryGetComponent(out VisualTransformComposer composer)
                ? composer
                : target.gameObject.AddComponent<VisualTransformComposer>();
        }

        private void Awake()
        {
            _layer = new VisualTransformLayer(transform);
        }

        public void Set(Object owner, Vector3 scaleMultiplier, Vector3 worldOffset)
        {
            _contributions[owner] = new Contribution { ScaleMultiplier = scaleMultiplier, WorldOffset = worldOffset };
        }

        public void Clear(Object owner)
        {
            _contributions.Remove(owner);
        }

        private void LateUpdate()
        {
            var scale = Vector3.one;
            var offset = Vector3.zero;
            foreach (var contribution in _contributions.Values)
            {
                scale = Vector3.Scale(scale, contribution.ScaleMultiplier);
                offset += contribution.WorldOffset;
            }

            _layer.Apply(scale, offset);
        }

        private void OnDisable()
        {
            _layer.Restore();
        }
    }
}
