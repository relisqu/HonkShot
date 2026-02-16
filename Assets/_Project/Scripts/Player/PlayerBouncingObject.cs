using Scripts.LevelSystem.LevelObjects;
using UnityEngine;

namespace Scripts.Player
{
    public class PlayerBouncingObject : BouncingObject
    {
        [SerializeField] private CircleCollider2D _circleCollider;

        protected override Vector2 GetBounceNormal(Collision2D collision)
        {
            if (!_circleCollider || CurrentVelocity.sqrMagnitude < 0.01f)
                return base.GetBounceNormal(collision);

            Vector2 ballCenter = (Vector2)transform.TransformPoint(_circleCollider.offset);
            Vector2 closestPoint = collision.collider.ClosestPoint(ballCenter);
            Vector2 normal = (ballCenter - closestPoint).normalized;

            if (normal.sqrMagnitude < 0.01f)
                return base.GetBounceNormal(collision);

            return normal;
        }
    }
}
