using UnityEngine;
using Sirenix.OdinInspector;
using System.Collections.Generic;
using Scripts.Enemies;
using Scripts.LevelSystem.LevelObjects;
using Scripts.PointSystem;
using UnityEngine;


namespace Scripts.Other
{
    [ExecuteInEditMode]
    public class RoomValidChecker : MonoBehaviour
    {
            [Header("Enemy Parent Container")] [SerializeField]
            private Transform _enemyContainer;

            [InfoBox("Some enemies are outside of the designated container!", InfoMessageType.Warning, "HasEnemiesOutside")]
            [SerializeField]
            [ShowIf("HasEnemiesOutside")]
            private List<GameObject> _invalidEnemies = new List<GameObject>();

            
            private void OnValidate()
            {
                ValidateEnemies();
            }

            private int nextUpdate = 3;

            private void Update()
            {
                if (Time.time < nextUpdate) return;

                nextUpdate = Mathf.FloorToInt(Time.time) + 1;
                ValidateEnemies();
            }

            [Button("Check Enemies")]
            private void ValidateEnemies()
            {
                _invalidEnemies.Clear();
                if (!_enemyContainer) return;

                foreach (var enemy in transform.GetComponentsInChildren<EnemyHealth>())
                {
                    if (!enemy.transform.IsChildOf(_enemyContainer))
                    {
                        _invalidEnemies.Add(enemy.gameObject);
                    }
                }

                foreach (var enemy in transform.GetComponentsInChildren<AttackPointsObject>())
                {
                    if (!enemy.transform.IsChildOf(_enemyContainer))
                    {
                        _invalidEnemies.Add(enemy.gameObject);
                    }
                }
            }

            private bool HasEnemiesOutside()
            {
                return _invalidEnemies.Count > 0;
            }
        }
}