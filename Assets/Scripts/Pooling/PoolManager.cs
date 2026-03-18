using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Manages multiple pools by PoolObjectType.
    /// </summary>
    public class PoolManager : IPoolManager
    {
        private class Pool
        {
            public GameObject Prefab;
            public Transform Parent;
            public Queue<IPoolingObject> Available = new();
            public List<IPoolingObject> All = new();
        }

        private readonly Dictionary<PoolObjectType, Pool> _pools = new();

        public void InitializePool(PoolObjectType type, GameObject prefab, int initialCount, Transform parent)
        {
            if (_pools.ContainsKey(type))
            {
                Debug.LogWarning($"[PoolManager] Pool for {type} already exists.");
                return;
            }

            var pool = new Pool
            {
                Prefab = prefab,
                Parent = parent
            };

            _pools[type] = pool;

            for (int i = 0; i < initialCount; i++)
            {
                CreateNew(type, pool);
            }

            Debug.Log($"[PoolManager] Initialized {type} pool with {initialCount} objects.");
        }

        public IPoolingObject Get(PoolObjectType type)
        {
            if (!_pools.TryGetValue(type, out var pool))
            {
                Debug.LogError($"[PoolManager] Pool for {type} not found.");
                return null;
            }

            IPoolingObject poolObject;

            if (pool.Available.Count > 0)
            {
                poolObject = pool.Available.Dequeue();
            }
            else
            {
                poolObject = CreateNew(type, pool);
            }

            poolObject.GameObject.SetActive(true);
            return poolObject;
        }

        public void Return(IPoolingObject poolObject)
        {
            if (poolObject == null) return;

            if (!_pools.TryGetValue(poolObject.Type, out var pool))
            {
                Debug.LogWarning($"[PoolManager] Pool for {poolObject.Type} not found.");
                return;
            }

            ResetForPool(poolObject, pool);

            if (!pool.Available.Contains(poolObject))
            {
                pool.Available.Enqueue(poolObject);
            }
        }

        public void Clear()
        {
            foreach (var pool in _pools.Values)
            {
                foreach (var obj in pool.All)
                {
                    if (obj.GameObject != null)
                    {
                        Object.Destroy(obj.GameObject);
                    }
                }

                pool.Available.Clear();
                pool.All.Clear();
            }

            _pools.Clear();
            Debug.Log("[PoolManager] Cleared all pools.");
        }

        private IPoolingObject CreateNew(PoolObjectType type, Pool pool)
        {
            var go = Object.Instantiate(pool.Prefab, pool.Parent);
            go.SetActive(false);
            go.name = $"{type}_{pool.All.Count}";

            var poolObject = new PoolingObject(type, go);

            pool.Available.Enqueue(poolObject);
            pool.All.Add(poolObject);

            return poolObject;
        }

        private void ResetForPool(IPoolingObject poolObject, Pool pool)
        {
            if (poolObject.GameObject != null)
            {
                poolObject.GameObject.SetActive(false);
                poolObject.GameObject.transform.localScale = Vector3.one;
                poolObject.GameObject.transform.SetParent(pool.Parent);
            }
        }
    }
}
