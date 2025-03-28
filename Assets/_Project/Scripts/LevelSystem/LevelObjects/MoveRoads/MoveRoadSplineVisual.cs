using System;
using System.Collections;
using System.Collections.Generic;
using Sirenix.OdinInspector;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Splines;

namespace Scripts.LevelSystem.LevelObjects.MoveRoads
{
    [ExecuteAlways]
    public class MoveRoadSplineVisual : MonoBehaviour
    {
        [SerializeField] private SplineContainer _mainSplineContainer;
        [SerializeField] private List<SplineContainer> _secondarySplineContainers;
        [SerializeField] private List<SplineInstantiate> _secondarySplineInstantiators;
        [SerializeField] private Transform _startPoint;
        [SerializeField] private Transform _finishPoint;
        [SerializeField] private float _pointDistance;

        private IEnumerator Start()
        {
           //UpdateSecondSpline();
            yield return new WaitForSeconds(0.1f);
        }


        [Button]
        public void UpdateSecondSpline()
        {
            foreach (var secondarySplineContainer in _secondarySplineContainers)
            {
                secondarySplineContainer.Spline = _mainSplineContainer.Spline;
            }

            foreach (var secondarySplineContainer in _secondarySplineInstantiators)
            {
                secondarySplineContainer.Clear();
                secondarySplineContainer.UpdateInstances();
            }

            float3 position = Vector3.zero;
            float3 tangent = Vector3.zero;
            float3 upVector = Vector3.zero;
            _mainSplineContainer.Evaluate(_secondarySplineContainers[0].Spline, 0f, out position, out tangent,
                out upVector);

            var startPointAngle = Mathf.Atan2(tangent.y, tangent.x) * 180 / Mathf.PI;
            _startPoint.position = position - _pointDistance * tangent;
            _startPoint.rotation = Quaternion.Euler(0f, 0f, startPointAngle);

            _mainSplineContainer.Evaluate(_secondarySplineContainers[0].Spline, 1f, out position, out tangent,
                out upVector);
            startPointAngle = Mathf.Atan2(tangent.y, tangent.x) * 180 / Mathf.PI;
            _finishPoint.position = position + _pointDistance * tangent;
            _finishPoint.rotation = Quaternion.Euler(0f, 0f, startPointAngle + 180);
        }

     
        private void Update()
        {
#if UNITY_EDITOR
            UpdateSecondSpline();
#endif
        }
    }
}