using UnityEngine;

namespace Scripts.LevelSystem.LevelObjects
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(PlatformEffector2D))]
    public class OneWayWall : MonoBehaviour
    {
        [Header("One-Way Settings")]
        [SerializeField] private float _surfaceArc = 180f;

        [Header("Visual")]
        [SerializeField] private SpriteRenderer _arrowIndicator;

        private PlatformEffector2D _effector;
        private Collider2D _collider;

        private void Awake()
        {
            _effector = GetComponent<PlatformEffector2D>();
            _collider = GetComponent<Collider2D>();

            _collider.usedByEffector = true;

            _effector.useOneWay = true;
            _effector.surfaceArc = _surfaceArc;
            _effector.useOneWayGrouping = true;
        }

        private void OnDrawGizmos()
        {
            Vector2 passDirection = transform.up;
            Vector2 blockDirection = -transform.up;

            // Green arrow = pass through direction
            Gizmos.color = Color.green;
            Vector3 pos = transform.position;
            Gizmos.DrawLine(pos, pos + (Vector3)(passDirection * 1.5f));
            Gizmos.DrawSphere(pos + (Vector3)(passDirection * 1.5f), 0.1f);

            // Red arrow = bounce direction
            Gizmos.color = Color.red;
            Gizmos.DrawLine(pos, pos + (Vector3)(blockDirection * 1f));
            Gizmos.DrawCube(pos + (Vector3)(blockDirection * 1f), Vector3.one * 0.15f);

            // Arc visualization
            Gizmos.color = new Color(1f, 0f, 0f, 0.2f);
            float halfArc = (_surfaceArc > 0 ? _surfaceArc : 180f) * 0.5f;
            float baseAngle = transform.eulerAngles.z + 90f;
            for (float a = -halfArc; a < halfArc; a += 5f)
            {
                float rad = (baseAngle + a) * Mathf.Deg2Rad;
                Vector3 dir = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f);
                Gizmos.DrawLine(pos, pos + dir * 0.8f);
            }
        }
    }
}
