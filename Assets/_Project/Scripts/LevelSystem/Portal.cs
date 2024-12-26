using System;
using System.Collections;
using Scripts.Player;
using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.LevelSystem
{
    public class Portal : MonoBehaviour
    {
        [SerializeField] private Portal _pairedPortal;


        public void Teleport(Collider2D collision, TeleportableEntity teleportableEntity)
        {
            if (!teleportableEntity.CanTeleport()) return;
            teleportableEntity.DisableTeleporting();

            var localVelocity = transform.InverseTransformDirection(teleportableEntity.GetVelocity());
            var angle = Mathf.Atan2(localVelocity.z, localVelocity.x) * Mathf.Rad2Deg;
            Debug.Log($"{teleportableEntity.name} Teleported with angle: {angle}");
            var newVelocity = _pairedPortal.transform.TransformDirection(localVelocity);

            var contactPoint = collision.bounds.ClosestPoint(transform.position) ;
            var playerOffset =
                teleportableEntity.GetPosition() - contactPoint;
            var localContactPoint = transform.InverseTransformPoint(contactPoint+ Vector3.up * 0.1f);
            var newContactPoint =
                _pairedPortal.transform
                    .TransformPoint(localContactPoint);
            Vector2 newPosition =
                newContactPoint +
                _pairedPortal.transform
                    .TransformDirection(playerOffset);


            teleportableEntity.Teleport(newPosition, -newVelocity);

            teleportableEntity.EnableTeleporting();
        }
    }
}