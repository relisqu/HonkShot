using UnityEngine;

namespace Scripts.Other
{
    public static class Collider2DExtensions
    {
        /// <summary>
        /// Looks for the component on the collider's own GameObject first, then on the Rigidbody2D the
        /// collider belongs to. The player keeps its colliders on child objects (one per form) while its
        /// gameplay components sit on the root next to the body, so a plain TryGetComponent on the
        /// collider misses them.
        /// </summary>
        public static bool TryGetComponentInBody<T>(this Collider2D collider, out T component)
        {
            if (collider.TryGetComponent(out component))
                return true;

            var body = collider.attachedRigidbody;
            return body && body.gameObject != collider.gameObject && body.TryGetComponent(out component);
        }
    }
}
