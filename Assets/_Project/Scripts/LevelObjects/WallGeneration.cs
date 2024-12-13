using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.U2D;

namespace Scripts.LevelObjects
{
    public class WallGeneration : MonoBehaviour
    {
        [SerializeField] private GameObject _wallPrefab;
        [SerializeField] private Transform _wallsContainer; 
        [SerializeField] private SpriteShapeController _levelField;

        private void Start()
        {
            GenerateWalls();
        }

        [Button]
        public void GenerateWalls()
        {
            if (_wallPrefab == null || _wallsContainer == null || _levelField == null)
            {
                Debug.LogError("Please assign all required references.");
                return;
            }

            ClearContainer();
            Spline spline = _levelField.spline;

            for (int i = 0; i < spline.GetPointCount(); i++)
            {
                Vector3 currentPoint = spline.GetPosition(i); // Get the position of the current control point
                Vector3 nextPoint =
                    spline.GetPosition((i + 1) %
                                       spline
                                           .GetPointCount()); // Get the next control point (looping back to the first)

                Vector3 worldCurrentPoint = _levelField.transform.TransformPoint(currentPoint);
                Vector3 worldNextPoint = _levelField.transform.TransformPoint(nextPoint);
                Vector3 midpoint = (worldCurrentPoint + worldNextPoint) / 2f;

                Vector3 direction = worldNextPoint - worldCurrentPoint;
                float rotationZ = 90+Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                GameObject wall = Instantiate(_wallPrefab, midpoint, Quaternion.Euler(0, 0, rotationZ),
                    _wallsContainer);

                float wallLength = direction.magnitude;
                wall.transform.localScale =
                    new Vector3(wall.transform.localScale.x, wallLength+0.2f, 1);
            }
        }

        private void ClearContainer()
        {
            foreach (Transform child in _wallsContainer)
            {
                Destroy(child.gameObject);
            }
        }
    }
}