using System;
using System.Collections;
using Scripts.Player;
using UnityEngine;

namespace Scripts.LevelSystem
{
    public class Portal : MonoBehaviour
    {
        public Portal pairedPortal; // Reference to the other portal's script

        private bool isTeleporting; // Prevents teleportation loops for this portal

        private void OnCollisionEnter2D(Collision2D other)
        {
            Debug.Log(other.gameObject.name);
            
            if (other.gameObject.TryGetComponent(out PlayerMovement playerMovement))
            {
                Teleport(playerMovement.GetRigidbody());
            }
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Debug.Log(collision.gameObject.name);
            if (collision.TryGetComponent(out PlayerMovement playerMovement))
            {
                Teleport(playerMovement.GetRigidbody());
            }
        }

        private void Teleport(Rigidbody2D rb)
        {
            // Prevent this portal and the paired portal from teleporting again immediately
            isTeleporting = true;
            pairedPortal.isTeleporting = true;

            Vector2 localVelocity = transform.InverseTransformDirection(rb.velocity);

            Vector2 incomingDirection = rb.velocity.normalized;
            Vector2 portalNormal = transform.right; // The normal of the source portal (assuming right is forward)
            Vector2 pairedPortalNormal = pairedPortal.transform.right; // The normal of the paired portal

            // Reflect the incoming direction relative to the portals' normals
            float angleDifference = Vector2.SignedAngle(portalNormal, pairedPortalNormal);

            // Rotate the velocity to match the paired portal's orientation
            Vector2 adjustedVelocity = Quaternion.Euler(0, 0, angleDifference) * rb.velocity;


            // Reset the teleporting flags on the next frame
            Invoke(nameof(ResetTeleport), 0.1f); // Reset this portal's flag
            pairedPortal.Invoke(nameof(pairedPortal.ResetTeleport), 0.1f); // Reset the paired portal's flag
        }

        private void ResetTeleport()
        {
            isTeleporting = false;
        }
    }
}