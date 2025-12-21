using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Player
{
    public class BallRotationModule : MonoBehaviour
    {
        [SerializeField] private List<BallLayer> _layers = new();
        [SerializeField] private Transform _transform;

        [SerializeField] private Transform _visualTransform;
        [SerializeField] private float _rotationSpeed;
        private Vector2 _lastPosition;

        void Start()
        {
            _lastPosition = _transform.position;

            foreach (var layer in _layers)
            {
                if (layer.Renderer != null)
                {
                    layer.Offset = layer.Renderer.transform.localPosition;
                }
            }
        }

        void Update()
        {
            Vector2 movementDelta = GetMovementDelta();

            _visualTransform.Rotate(Vector3.forward *
                                    (Mathf.Sign(movementDelta.x) * movementDelta.magnitude * _rotationSpeed),
                Space.Self);

            _lastPosition = _transform.position;
        }

        private Vector2 GetMovementDelta()
        {
            Vector2 delta = (Vector2)_transform.position - _lastPosition;
            float rotationRadians = -_transform.rotation.eulerAngles.z * Mathf.Deg2Rad;
            return RotateVector2(delta, rotationRadians);
        }

        private static Vector2 RotateVector2(Vector2 vector, float radians)
        {
            float sin = Mathf.Sin(radians);
            float cos = Mathf.Cos(radians);
            return new Vector2(vector.x * cos - vector.y * sin, vector.y * cos + vector.x * sin);
        }
    }
}