using UnityEngine;

namespace Scripts.Player
{
    public class PlayerBandanaLayer : BallLayer
    {
        protected override void ApplyOffset()
        {
            Renderer.transform.localPosition = new Vector3(0, Offset.y, 0);
        }
    }
}