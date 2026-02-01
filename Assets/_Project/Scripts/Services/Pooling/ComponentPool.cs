using System.Collections.Generic;
using UnityEngine;

namespace Scripts.Services.Pooling
{
    public class ComponentPool<T> where T : Component
    {
        private readonly T _prefab;
        private readonly Transform _parent;
        private readonly List<T> _pool = new();

        public ComponentPool(T prefab, int initialSize, Transform parent = null)
        {
            _prefab = prefab;
            _parent = parent;
            Prewarm(initialSize);
        }

        private void Prewarm(int count)
        {
            for (int i = 0; i < count; i++)
            {
                var obj = Object.Instantiate(_prefab, _parent);
                obj.gameObject.SetActive(false);
                _pool.Add(obj);
            }
        }

        public T Get(Vector3 position, Quaternion rotation)
        {
            foreach (var obj in _pool)
            {
                if (!obj.gameObject.activeInHierarchy)
                {
                    obj.transform.SetParent(null);
                    obj.transform.position = position;
                    obj.transform.rotation = rotation;
                    obj.transform.localScale = Vector3.one;
                    obj.gameObject.SetActive(true);
                    return obj;
                }
            }

            var newObj = Object.Instantiate(_prefab, position, rotation);
            _pool.Add(newObj);
            return newObj;
        }

        public void Return(T obj)
        {
            if (obj)
                obj.gameObject.SetActive(false);
        }

        public void ReturnAll()
        {
            foreach (var obj in _pool)
            {
                if (obj && obj.gameObject.activeInHierarchy)
                    obj.gameObject.SetActive(false);
            }
        }
    }
}
