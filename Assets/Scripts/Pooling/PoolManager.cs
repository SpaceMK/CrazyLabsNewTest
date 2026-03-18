using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using UnityEngine;
using UnityEngine.Pool;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Manages object pools using Unity's built-in ObjectPool.
    /// 
    /// Each prefab gets its own pool. Objects implement IPoolable
    /// to receive spawn/despawn callbacks.
    /// </summary>
    public sealed class PoolManager : IPoolManager
    {
        private readonly Dictionary<int, object> _pools = new();
        private readonly Dictionary<int, int> _instanceToPrefabId = new();
        private readonly Transform _poolRoot;

        private const int DefaultCapacity = 10;
        private const int MaxPoolSize = 100;

        public PoolManager()
        {
            var rootGo = new GameObject("[PoolManager]");
            Object.DontDestroyOnLoad(rootGo);
            _poolRoot = rootGo.transform;

            Debug.Log("[PoolManager] Initialized.");
        }

        public T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component, IPoolable
        {
            var pool = GetOrCreatePool(prefab);
            var instance = pool.Get();

            instance.transform.SetPositionAndRotation(position, rotation);
            instance.gameObject.SetActive(true);
            instance.OnSpawn();

            return instance;
        }

        public void Despawn<T>(T instance) where T : Component, IPoolable
        {
            if (instance == null) return;

            int instanceId = instance.GetInstanceID();

            if (!_instanceToPrefabId.TryGetValue(instanceId, out int prefabId))
            {
                Debug.LogWarning($"[PoolManager] Object not from pool: {instance.name}. Destroying.");
                Object.Destroy(instance.gameObject);
                return;
            }

            if (!_pools.TryGetValue(prefabId, out object poolObj))
            {
                Debug.LogWarning($"[PoolManager] Pool not found. Destroying object.");
                Object.Destroy(instance.gameObject);
                return;
            }

            instance.OnDespawn();
            instance.gameObject.SetActive(false);

            var pool = (ObjectPool<T>)poolObj;
            pool.Release(instance);
        }

        public void Prewarm<T>(T prefab, int count) where T : Component, IPoolable
        {
            var pool = GetOrCreatePool(prefab);
            var instances = new List<T>(count);

            for (int i = 0; i < count; i++)
            {
                instances.Add(pool.Get());
            }

            foreach (var instance in instances)
            {
                instance.gameObject.SetActive(false);
                pool.Release(instance);
            }

            Debug.Log($"[PoolManager] Prewarmed {count} instances of {prefab.name}");
        }

        public void ClearAll()
        {
            foreach (var poolObj in _pools.Values)
            {
                if (poolObj is System.IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }

            _pools.Clear();
            _instanceToPrefabId.Clear();

            if (_poolRoot != null)
            {
                foreach (Transform child in _poolRoot)
                {
                    Object.Destroy(child.gameObject);
                }
            }

            Debug.Log("[PoolManager] All pools cleared.");
        }

        private ObjectPool<T> GetOrCreatePool<T>(T prefab) where T : Component, IPoolable
        {
            int prefabId = prefab.GetInstanceID();

            if (_pools.TryGetValue(prefabId, out object existingPool))
            {
                return (ObjectPool<T>)existingPool;
            }

            var container = new GameObject($"Pool_{prefab.name}");
            container.transform.SetParent(_poolRoot);

            var pool = new ObjectPool<T>(
                createFunc: () => CreateInstance(prefab, container.transform),
                actionOnGet: instance => { },
                actionOnRelease: instance => { instance.transform.SetParent(container.transform); },
                actionOnDestroy: instance => Object.Destroy(instance.gameObject),
                collectionCheck: true,
                defaultCapacity: DefaultCapacity,
                maxSize: MaxPoolSize
            );

            _pools[prefabId] = pool;

            Debug.Log($"[PoolManager] Created pool for: {prefab.name}");
            return pool;
        }

        private T CreateInstance<T>(T prefab, Transform parent) where T : Component, IPoolable
        {
            var instance = Object.Instantiate(prefab, parent);
            instance.gameObject.SetActive(false);

            int prefabId = prefab.GetInstanceID();
            _instanceToPrefabId[instance.GetInstanceID()] = prefabId;

            return instance;
        }
    }
}