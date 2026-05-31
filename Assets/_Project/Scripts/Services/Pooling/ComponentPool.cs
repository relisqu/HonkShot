using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Services.Pooling
{
    public class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly Queue<T> _inactivePool = new(); // queue is better in terms of you know O-complexity
        private readonly List<T> _allObjects = new(); // code readability though

        [SerializeField] private int _maxPoolSize = 100; // we need the limit
        
        public ComponentPool(T prefab, int initialSize = 10, Transform parent = null, int maxSize = 100)
        {
            _prefab = prefab;
            _parent = parent;
            _maxPoolSize = maxSize;
            Prewarm(initialSize);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                AddNewObject();
            }
        }
        public void Initialize(int initialSize = 10)
        {
            Prewarm(initialSize);
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            T obj;
            
            // look for inactive
            if (_inactivePool.Count > 0)
            {
                obj = _inactivePool.Dequeue();
            }
            else if (_allObjects.Count < _maxPoolSize)
            {
                obj = AddNewObject();
            }
            else
            {
                Debug.LogWarning($"Pool {_prefab.name} overflow! Max size reached.");
                return null;
            }

            // setup
            obj.transform.SetParent(null);
            obj.transform.position = position;
            obj.transform.rotation = rotation;
            obj.transform.localScale = Vector3.one;
            obj.gameObject.SetActive(true);
            
            return obj;
        }

        public void Return(T obj)
        {
            if (obj == null || !obj.gameObject.activeInHierarchy) 
                return;

            // deactivate & return
            obj.gameObject.SetActive(false);
            if (_parent != null)
                obj.transform.SetParent(_parent);
            
            obj.transform.localPosition = Vector3.zero;
            obj.transform.localRotation = Quaternion.identity;
            
            _inactivePool.Enqueue(obj);
        }

        public void ReturnAll()
        {
            foreach (var obj in _allObjects)
            {
                if (obj && obj.gameObject.activeInHierarchy)
                    Return(obj);
            }
        }

        private T AddNewObject()
        {
            var obj = Object.Instantiate(_prefab, _parent);
            obj.gameObject.SetActive(false);
            _allObjects.Add(obj);
            _inactivePool.Enqueue(obj);
            return obj;
        }

        public int ActiveCount => _allObjects.Count - _inactivePool.Count;
        public int InactiveCount => _inactivePool.Count;
        public int TotalCount => _allObjects.Count;
    }
}