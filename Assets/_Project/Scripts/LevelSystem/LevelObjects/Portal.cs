using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private Portal _pairedPortal;

        [Tooltip("Local-space normal / face direction of this portal. The player is spawned along the paired portal's world-space version of this vector.")]
        [SerializeField] private Vector2 _localNormal = Vector2.right;

        [Tooltip("World-units offset along the paired portal's outward normal where the entity is respawned.")]
        [SerializeField] private float _exitOffset = 0.6f;

        public Portal PairedPortal => _pairedPortal;
        public Vector2 WorldNormal => ((Vector2)transform.TransformDirection(_localNormal.normalized)).normalized;

        public void Teleport(Collider2D collision, TeleportableEntity teleportableEntity)
        {
            if (!teleportableEntity.CanTeleport() || !_pairedPortal) return;

            teleportableEntity.DisableTeleporting();

            // Direction + power rotated around portal:
            // convert entry velocity to THIS portal's local frame, then back out through the
            // paired portal's frame. Equivalent to rotating the velocity by the angular delta
            // between the two portals.
            Vector2 entryVel = teleportableEntity.GetVelocity();
            Vector2 localVel = transform.InverseTransformDirection(entryVel);
            Vector2 exitVel = _pairedPortal.transform.TransformDirection(localVel);

            // Spawn along the exit direction so the player moves away from the paired portal
            // and doesn't instantly re-trigger it. Fall back to the paired portal's normal if
            // exit velocity is effectively zero.
            Vector2 exitDir = exitVel.sqrMagnitude > 1e-4f
                ? exitVel.normalized
                : _pairedPortal.WorldNormal;

            Vector2 exitPos = (Vector2)_pairedPortal.transform.position + exitDir * _exitOffset;

            teleportableEntity.Teleport(exitPos, exitVel);
            teleportableEntity.EnableTeleporting();
        }

#if UNITY_EDITOR
        private void OnDrawGizmos()
        {
            Vector3 normalWorld = transform.TransformDirection(_localNormal.normalized);

            Gizmos.color = Color.cyan;
            Gizmos.DrawLine(transform.position, transform.position + normalWorld * 1.2f);
            Gizmos.DrawWireSphere(transform.position, 0.1f);

            if (_pairedPortal)
            {
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(transform.position, _pairedPortal.transform.position);
            }
        }
#endif
    }
}
