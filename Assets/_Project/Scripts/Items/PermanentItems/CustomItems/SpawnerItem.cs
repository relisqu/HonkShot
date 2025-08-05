using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using Scripts.Items.PlayerItemManager;
using Scripts.LevelSystem.LevelGeneration;
using UnityEngine.Serialization;

namespace Scripts.Items.PermanentItems.CustomItems
{
    public class SpawnerItem : Item
    {
        [Header("Spawn Settings")] [SerializeField]
        private float _spawnInterval = 0.5f;

        [Header("Pool Settings")] [SerializeField]
        private GameObject enemyDamageObjectPrefab;

        [SerializeField] private int _poolSize = 40;
        [SerializeField] private Transform _poolParent;

        private List<GameObject> _poolObjects = new();


        private bool _isActive = true;
        private Coroutine _spawnCoroutine;
        private float _spawnAngle = 0f;

        private Transform LastFireTransform;


        public override void InitItem(PlayerItemSO playerItemSO)
        {
            // Initialize with item ID if needed
        }

        void Start()
        {
            for (int i = 0; i < _poolSize; i++)
            {
                var fireObject = Instantiate(enemyDamageObjectPrefab, null);
                fireObject.gameObject.SetActive(false);
                fireObject.transform.localScale = Vector3.zero;

                _poolObjects.Add(fireObject);
            }

            LevelManager.Instance.EnteredRoom += LevelManager_CompletedRoom;
            StartSpawning();
        }

        private void LevelManager_CompletedRoom()
        {
            foreach (var fireObject in _poolObjects)
            {
                if (fireObject)
                    fireObject.gameObject.SetActive(false);
            }
        }

        private void StartSpawning()
        {
            if (_spawnCoroutine != null)
            {
                StopCoroutine(_spawnCoroutine);
            }

            _spawnCoroutine = StartCoroutine(SpawnRoutine());
        }

        private IEnumerator SpawnRoutine()
        {
            while (true)
            {
                if (!LastFireTransform || !LastFireTransform.gameObject.activeInHierarchy ||
                    Vector2.Distance(LastFireTransform.position, transform.position) > _spawnInterval)
                    SpawnFireObject(transform.position);
                yield return new WaitForSeconds(0.01f);
            }

            yield return null;
        }

        public GameObject GetFromPool(Vector3 position, Quaternion rotation = default)
        {
            foreach (var enemyDamageObject in _poolObjects)
            {
                if (!enemyDamageObject.gameObject.activeInHierarchy)
                {
                    return enemyDamageObject;
                }
            }

            return null;
        }

        private void SpawnFireObject(Vector3 position)
        {
            var fireObject = GetFromPool(position);
            if (!fireObject) return;
            fireObject.gameObject.SetActive(true);
            fireObject.transform.DOScale(Vector3.one, 0.1f);
            fireObject.transform.position = position;
            LastFireTransform = fireObject.transform;
        }


        void OnDestroy()
        {
            LevelManager.Instance.CompletedRoom -= LevelManager_CompletedRoom;
            foreach (var fireObject in _poolObjects)
            {
                if (fireObject)
                    Destroy(fireObject.gameObject);
            }
        }
    }
}