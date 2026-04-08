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

            // The entry portal accepts any incoming direction. The paired portal acts like a
            // launcher — the player always exits along its world normal, preserving the
            // incoming speed magnitude.
            Vector2 entryVel = teleportableEntity.GetVelocity();
            float speed = entryVel.magnitude;
            Vector2 exitVel = _pairedPortal.WorldNormal * speed;

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
