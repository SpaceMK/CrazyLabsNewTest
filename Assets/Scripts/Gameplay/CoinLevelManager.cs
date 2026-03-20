using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Manages coin placement on the level.
    /// Avoids spawning near obstacles by querying ISpawnPositionProvider (injected via DI).
    /// 
    /// Implements:
    /// - ILevelManager: so GameplayFlow can reset coins without knowing this concrete type.
    /// - ICoinDespawner: so PlayerCollisionHandler can despawn coins without a direct dependency.
    /// 
    /// NOTE: Initialize() only wires dependencies — it does NOT spawn.
    /// Spawning is triggered by GameplayFlow.Start() via ILevelManager.Reset(),
    /// which runs after all [Inject] methods complete, guaranteeing pools are ready.
    /// </summary>
    public class CoinLevelManager : MonoBehaviour, ILevelManager, ICoinDespawner
    {
        [Header("Spawn Bounds")]
        [SerializeField] private Vector3 _minBounds = new(-5f, 1f, 10f);
        [SerializeField] private Vector3 _maxBounds = new(5f, 1f, 200f);

        [Header("Settings")]
        [SerializeField] private int _coinCount = 30;
        [SerializeField] private float _coinHeight = 1f;

        [Header("Dispersion")]
        [SerializeField] private float _minDistanceBetweenCoins = 2f;
        [SerializeField] private float _minDistanceFromObstacles = 4f;
        [SerializeField] private int _maxSpawnAttempts = 50;

        private IPoolManager _poolManager;
        private ISpawnPositionProvider _obstaclePositionProvider;
        private readonly List<IPoolingObject> _activeCoins = new();
        private readonly List<Vector3> _spawnedPositions = new();

        public IReadOnlyList<IPoolingObject> ActiveCoins => _activeCoins;

        /// <summary>
        /// DI injection. ISpawnPositionProvider is optional (null if no obstacles exist).
        /// Wires dependencies only — does NOT spawn. See class summary.
        /// </summary>
        [Inject]
        public void Initialize(IPoolManager poolManager, ISpawnPositionProvider obstaclePositionProvider = null)
        {
            _poolManager = poolManager;
            _obstaclePositionProvider = obstaclePositionProvider;

            Debug.Log($"[CoinLevelManager] Initialized. ObstacleProvider: {_obstaclePositionProvider != null}");
        }

        public void SpawnAllCoins()
        {
            _spawnedPositions.Clear();
            int spawned = 0;

            for (int i = 0; i < _coinCount; i++)
            {
                Vector3? position = FindValidPosition();

                if (position.HasValue)
                {
                    SpawnCoin(position.Value);
                    spawned++;
                }
            }

            Debug.Log($"[CoinLevelManager] Spawned {spawned}/{_coinCount} coins.");
        }

        private Vector3? FindValidPosition()
        {
            for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
            {
                Vector3 candidate = GetRandomPosition();

                if (IsValidPosition(candidate))
                {
                    return candidate;
                }
            }

            return null;
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(_minBounds.x, _maxBounds.x);
            float z = Random.Range(_minBounds.z, _maxBounds.z);

            return new Vector3(x, _coinHeight, z);
        }

        private bool IsValidPosition(Vector3 position)
        {
            foreach (var existing in _spawnedPositions)
            {
                float distance = Vector3.Distance(
                    new Vector3(position.x, 0, position.z),
                    new Vector3(existing.x, 0, existing.z)
                );

                if (distance < _minDistanceBetweenCoins)
                {
                    return false;
                }
            }

            if (_obstaclePositionProvider != null)
            {
                var obstaclePositions = _obstaclePositionProvider.GetOccupiedPositions();

                foreach (var obstaclePos in obstaclePositions)
                {
                    float distance = Vector3.Distance(
                        new Vector3(position.x, 0, position.z),
                        new Vector3(obstaclePos.x, 0, obstaclePos.z)
                    );

                    if (distance < _minDistanceFromObstacles)
                    {
                        return false;
                    }
                }
            }

            return true;
        }

        public IPoolingObject SpawnCoin(Vector3 position)
        {
            var coin = _poolManager.Get(PoolObjectType.Coin);
            if (coin == null) return null;

            coin.GameObject.transform.position = position;
            _activeCoins.Add(coin);
            _spawnedPositions.Add(position);

            return coin;
        }

        public void DespawnCoin(IPoolingObject coin)
        {
            if (coin == null) return;

            _activeCoins.Remove(coin);
            _poolManager.Return(coin);
        }

        /// <summary>
        /// ICoinDespawner implementation.
        /// </summary>
        public void DespawnCoin(GameObject coinGameObject)
        {
            var coin = _activeCoins.Find(c => c.GameObject == coinGameObject);
            if (coin != null)
            {
                DespawnCoin(coin);
            }
        }

        public void DespawnAll()
        {
            for (int i = _activeCoins.Count - 1; i >= 0; i--)
            {
                _poolManager.Return(_activeCoins[i]);
            }
            _activeCoins.Clear();
            _spawnedPositions.Clear();
        }

        /// <summary>
        /// ILevelManager implementation.
        /// </summary>
        public void Reset()
        {
            DespawnAll();
            SpawnAllCoins();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 1f, 0f, 0.3f);

            Vector3 center = (_minBounds + _maxBounds) / 2f;
            Vector3 size = _maxBounds - _minBounds;

            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}