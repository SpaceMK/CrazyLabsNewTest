using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Loads pool configuration from ScriptableObject and initializes PoolManager.
    /// </summary>
    public class LevelPoolProvider : MonoBehaviour
    {
        [SerializeField] private PoolConfig _config;
        [SerializeField] private Transform _poolParent;

        [Inject]
        public void Initialize(IPoolManager poolManager)
        {
            if (_config == null)
            {
                Debug.LogError("[LevelPoolProvider] PoolConfig not assigned!");
                return;
            }

            // Create pool parent if not assigned
            if (_poolParent == null)
            {
                var go = new GameObject("[Pools]");
                go.transform.SetParent(transform);
                _poolParent = go.transform;
            }

            // Initialize each pool from config
            foreach (var poolData in _config.Pools)
            {
                if (poolData.Prefab == null)
                {
                    Debug.LogWarning($"[LevelPoolProvider] Prefab for {poolData.Type} is null.");
                    continue;
                }

                // Create container for this pool type
                var container = new GameObject($"Pool_{poolData.Type}");
                container.transform.SetParent(_poolParent);

                poolManager.InitializePool(
                    poolData.Type,
                    poolData.Prefab,
                    poolData.InitialCount,
                    container.transform
                );
            }

            Debug.Log($"[LevelPoolProvider] Loaded {_config.Pools.Count} pools from config.");
        }
    }
}
