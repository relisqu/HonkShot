using UnityEngine;
using UnityEngine.Serialization;

namespace Scripts.Player
{
    public class BallRotation : MonoBehaviour
    {
        public float PerspectiveCoeff = 0.1f;
        public float PerspectiveCoeff2 = 0.1f;
        public float PerspectiveCoeff3 = 0.1f;
        public SpriteRenderer BandanaBallArt;
        public SpriteRenderer EyesBallArt; // Primary ball sprite
        public SpriteRenderer BeakBallArt; // Secondary ball sprite (optional, for layered rotation effects)
        public Transform MyTransform; // Transform of the ball
        public Rigidbody2D MyRigidbody; // Rigidbody2D for movement and physics

        private Vector2 eyesArtOffset;
        private Vector2 beakArtOffset;
        private Vector2 bandanaArtOffset;
        private Vector2 lastPosition;
        private Vector2 size;

        void Start()
        {
            // Initialize offsets and size

            eyesArtOffset = EyesBallArt.transform.localPosition;
            beakArtOffset = BeakBallArt.transform.localPosition;
            bandanaArtOffset = BandanaBallArt.transform.localPosition;
            lastPosition = MyTransform.position;

            size = GetSpriteSize(EyesBallArt.sprite); // Utility function to get sprite size
        }

        void Update()
        {
            // Update rotation offsets based on movement distance
            Vector2 movementDelta = GetMovementDelta();
            eyesArtOffset += movementDelta;
            beakArtOffset += movementDelta;
            bandanaArtOffset += movementDelta;
            // Wrap offsets to prevent drifting too far
            ModPosition();

            // Apply offsets to the sprites
            SetOffsets();

            // Store the current position for the next frame
            lastPosition = MyTransform.position;
        }

        private Vector2 GetMovementDelta()
        {
            // Calculate the movement delta and rotate it based on the ball's rotation
            Vector2 delta = (Vector2)MyTransform.position - lastPosition;
            float rotationRadians = -MyTransform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            return RotateVector2(delta, rotationRadians); // Utility function for rotating vectors
        }

        private void ModPosition()
        {
            // Wrap the offsets within the sprite size to simulate rotation
            eyesArtOffset = Vector2Mod(eyesArtOffset + GetSpriteSize(EyesBallArt.sprite) / 2, GetSpriteSize(EyesBallArt.sprite)) - GetSpriteSize(EyesBallArt.sprite) / 2;
            beakArtOffset = Vector2Mod(beakArtOffset +  GetSpriteSize(BeakBallArt.sprite) / 2, GetSpriteSize(BeakBallArt.sprite)) - GetSpriteSize(BeakBallArt.sprite) / 2;
            bandanaArtOffset = Vector2Mod(bandanaArtOffset + GetSpriteSize(BandanaBallArt.sprite) / 2,  GetSpriteSize(BandanaBallArt.sprite)) -  GetSpriteSize(BandanaBallArt.sprite) / 2;
        }

        private void SetOffsets()
        {
            // Apply the calculated offsets to the sprite renderers
            EyesBallArt.transform.localPosition = eyesArtOffset;
            BeakBallArt.transform.localPosition = beakArtOffset;
            BandanaBallArt.transform.localPosition = new Vector3(0, bandanaArtOffset.y, 0);
            EyesBallArt.transform.localPosition =
                new Vector3(eyesArtOffset.x, TopDownOffsetVector2(eyesArtOffset, EyesBallArt).y, 0);
            BeakBallArt.transform.localPosition =
                new Vector3(beakArtOffset.x, TopDownOffsetVector2(beakArtOffset, BeakBallArt).y, 0);
        }


        public static float FloatMod(float a, float m)
        {
            if (a < 0)
            {
                do
                {
                    a += m;
                } while (a < 0);

                return a;
            }

            if (a < m)
            {
                return a;
            }

            do
            {
                a -= m;
            } while (a >= m);

            return a;
        }

        public static Vector2 Vector2Mod(Vector2 a, Vector2 m)
        {
            return new Vector2(FloatMod(a.x, m.x), FloatMod(a.y, m.y));
        }

        public Vector2 RotateVector2(Vector2 a, float r)
        {
            float s = Mathf.Sin(r);
            float c = Mathf.Cos(r);
            var rotatedVector = new Vector2(a.x * c - a.y * s, a.y * c + a.x * s);
            Debug.Log(rotatedVector);
            return rotatedVector;
        }

        public Vector2 TopDownOffsetVector2(Vector2 offset, SpriteRenderer renderer)
        {
            var perspOffset = offset.x / (GetSpriteSize(renderer.sprite).x / 2);
            var a = offset.y + PerspectiveCoeff3 - Mathf.Cos(perspOffset * PerspectiveCoeff) * PerspectiveCoeff2;
            Debug.Log(a);
            return new Vector2(0, a);
        }


        public static Vector2 GetSpriteSize(Sprite s)
        {
            Rect tem = s.rect;
            return new Vector2(tem.width, tem.height) / 32;
        }
    }
}