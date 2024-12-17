using UnityEngine;

namespace Scripts.Player
{
    public class DefaultBallLayer : BallLayer
    {
        [SerializeField] private float _perspectiveCoeff = 2f;
        [SerializeField] private float _perspectiveCoeff2 = 0.1f;
        [SerializeField] private float _perspectiveCoeff3 = 0.1f;
        protected override void ApplyOffset()
        {
            Vector2 spriteSize = GetSpriteSize(Renderer.sprite);
            Vector2 adjustedOffset = new Vector2(
                Offset.x,
                TopDownOffset(Offset, spriteSize)
            );

            Renderer.transform.localPosition = adjustedOffset;
        }

        private float TopDownOffset(Vector2 offset, Vector2 spriteSize)
        {
            float perspectiveOffset = offset.x / (spriteSize.x / 2);
            return offset.y + _perspectiveCoeff3 -
                   Mathf.Cos(perspectiveOffset * _perspectiveCoeff) * _perspectiveCoeff2;
        }
    }
}