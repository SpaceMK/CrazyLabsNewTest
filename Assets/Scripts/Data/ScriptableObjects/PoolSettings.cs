using System;
using System.Collections.Generic;
using SledSurfers.Pooling;
using UnityEngine;

namespace SledSurfers.Data.ScriptableObjects
{
    /// <summary>
    /// Configuration for object pools.
    /// Assign prefabs and their pool settings here.
    /// </summary>
    [CreateAssetMenu(fileName = "PoolSettings", menuName = "SledSurfers/Pool Settings")]
    public class PoolSettings : ScriptableObject
    {
        [Serializable]
        public class PoolConfig
        {
            [Tooltip("The prefab to pool (must have PoolableObject component)")]
            public PoolableObject Prefab;

            [Tooltip("How many to pre-instantiate")]
            [Range(1, 100)]
            public int InitialSize = 10;

            [Tooltip("Maximum pool size (prevents memory issues)")]
            [Range(10, 500)]
            public int MaxSize = 50;
        }

        [Header("Pool Configurations")]
        [Tooltip("Configure all poolable prefabs here")]
        public List<PoolConfig> Pools = new();

#if UNITY_EDITOR
        private void OnValidate()
        {
            foreach (var pool in Pools)
            {
                if (pool.InitialSize > pool.MaxSize)
                {
                    pool.InitialSize = pool.MaxSize;
                }
            }
        }
#endif
    }
}
