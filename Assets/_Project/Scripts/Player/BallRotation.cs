using UnityEngine;

namespace Scripts.Player
{
    public class BallRotation : MonoBehaviour
    {
        public SpriteRenderer BallArt; // Primary ball sprite
        public SpriteRenderer BallArt2; // Secondary ball sprite (optional, for layered rotation effects)
        public Transform MyTransform; // Transform of the ball
        public Rigidbody2D MyRigidbody; // Rigidbody2D for movement and physics

        private Vector2 ballArtOffset;
        private Vector2 ballArtOffset2;
        private Vector2 lastPosition;
        private Vector2 size;

        void Start()
        {
            // Initialize offsets and size
            ballArtOffset = BallArt.transform.localPosition;
            lastPosition = MyTransform.position;
            size = GetSpriteSize(BallArt.sprite); // Utility function to get sprite size
        }

        void Update()
        {
            // Update rotation offsets based on movement distance
            Vector2 movementDelta = GetMovementDelta();
            ballArtOffset += movementDelta;
            ballArtOffset2 -= movementDelta;

            // Wrap offsets to prevent drifting too far
            ModPosition();

            // Apply offsets to the sprites
            SetOffset(ballArtOffset);

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
            ballArtOffset = Vector2Mod(ballArtOffset + size / 2, size) - size / 2;
            ballArtOffset2 = Vector2Mod(ballArtOffset2 + size / 2, size) - size / 2;
        }

        private void SetOffset(Vector2 offset)
        {
            // Apply the calculated offsets to the sprite renderers
            BallArt.transform.localPosition = offset;
            if (BallArt2 != null)
            {
                BallArt2.transform.localPosition = ballArtOffset2;
            }
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
            else if (a < m)
            {
                return a;
            }
            else
            {
                do
                {
                    a -= m;
                } while (a >= m);

                return a;
            }
        }

        public static Vector2 Vector2Mod(Vector2 a, Vector2 m)
        {
            return new Vector2(FloatMod(a.x, m.x), FloatMod(a.y, m.y));
        }

        public static Vector2 RotateVector2(Vector2 a, float r)
        {
            float s = Mathf.Sin(r);
            float c = Mathf.Cos(r);
            return new Vector2(a.x * c - a.y * s, a.y * c + a.x * s);
        }

        public static float Lerp(float a, float b, float speed, float time, float minSpeed = 0)
        {
            if (a == b)
            {
                return a;
            }

            bool min = a < b;
            a = Mathf.Lerp(a, b, speed * time * 0.01f) + ((b > a) ? minSpeed : -minSpeed) * 0.01f;
            if (min)
            {
                if (a >= b)
                {
                    a = b;
                }
            }
            else
            {
                if (a <= b)
                {
                    a = b;
                }
            }

            return a;
        }

        public static bool PointInRect(Vector2 p, Rect r)
        {
            return (p.x >= r.x && p.x < r.x + r.width && p.y >= r.y && p.y < r.y + r.height);
        }

        public static bool PointInRound(CircleCollider2D r, Vector2 p)
        {
            return ((Vector2)r.transform.position - p).sqrMagnitude < r.radius * r.radius;
        }

        public static float AngleVector(Vector2 v)
        {
            return Vector2.Angle(Vector2.right, v) * ((v.y > 0) ? 1 : -1) * Mathf.PI / 180;
        }

        public static void SetApha(SpriteRenderer s, float a)
        {
            Color c = s.color;
            c.a = a;
            s.color = c;
        }

        public static void SetRotationZ(Transform t, float r)
        {
            t.localRotation = Quaternion.AngleAxis(r, Vector3.forward);
        }

        public static void SetRotationZR(Transform t, float r)
        {
            SetRotationZ(t, r / Mathf.PI * 180);
        }

        public static Vector2 GetSpriteSize(Sprite s)
        {
            Rect tem = s.rect;
            return new Vector2(tem.width, tem.height) / 32;
        }
    }
}