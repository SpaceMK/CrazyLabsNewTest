using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;
using static UnityEngine.EventSystems.EventTrigger;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Manages coin placement on the level.
    /// Avoids spawning near obstacles by querying ISpawnPositionProvider (injected via DI).
    /// 
    /// Implements:
    /// - ILevelManager: so GameplayFlow can reset coins without knowing this concrete type.
    /// - IEntityDespawner: so PlayerCollisionHandler can despawn coins without a direct dependency.
    /// 
    /// NOTE: Initialize() only wires dependencies — it does NOT spawn.
    /// Spawning is triggered by GameplayFlow.Start() via ILevelManager.Reset().
    /// </summary>
    public class CoinLevelManager : MonoBehaviour, ILevelManager, IEntityDespawner
    {
        [Header("Spawn Bounds (XZ area to scatter within)")]
        [SerializeField] private float _minX = -5f;
        [SerializeField] private float _maxX = 5f;
        [SerializeField] private float _minZ = 10f;
        [SerializeField] private float _maxZ = 200f;

        [Header("Raycast")]
        [SerializeField] private float _raycastOriginY = 50f;
        [SerializeField] private float _raycastDistance = 100f;
        [SerializeField] private LayerMask _groundLayer = ~0;

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
                if (TryFindValidPosition(out Vector3 position))
                {
                    SpawnCoin(position);
                    spawned++;
                }
            }

            Debug.Log($"[CoinLevelManager] Spawned {spawned}/{_coinCount} coins.");
        }

        private bool TryFindValidPosition(out Vector3 position)
        {
            for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
            {
                float x = Random.Range(_minX, _maxX);
                float z = Random.Range(_minZ, _maxZ);

                Vector3 rayOrigin = new Vector3(x, _raycastOriginY, z);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
                {
                    Vector3 candidate = hit.point + hit.normal * _coinHeight;

                    if (IsValidPosition(candidate))
                    {
                        position = candidate;
                        return true;
                    }
                }
            }

            position = Vector3.zero;
            return false;
        }

        private bool IsValidPosition(Vector3 position)
        {
            foreach (var existing in _spawnedPositions)
            {
                float dx = position.x - existing.x;
                float dz = position.z - existing.z;
                float distXZ = Mathf.Sqrt(dx * dx + dz * dz);

                if (distXZ < _minDistanceBetweenCoins)
                    return false;
            }

            if (_obstaclePositionProvider != null)
            {
                var obstaclePositions = _obstaclePositionProvider.GetOccupiedPositions();

                foreach (var obstaclePos in obstaclePositions)
                {
                    float dx = position.x - obstaclePos.x;
                    float dz = position.z - obstaclePos.z;
                    float distXZ = Mathf.Sqrt(dx * dx + dz * dz);

                    if (distXZ < _minDistanceFromObstacles)
                        return false;
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

        public void DespawnEntity(GameObject entity)
        {
            var coin = _activeCoins.Find(c => c.GameObject == entity);
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

        public void Reset()
        {
            DespawnAll();
            SpawnAllCoins();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3((_minX + _maxX) / 2f, _raycastOriginY, (_minZ + _maxZ) / 2f);
            Vector3 size = new Vector3(_maxX - _minX, 0.1f, _maxZ - _minZ);

            Gizmos.color = new Color(1f, 1f, 0f, 0.15f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}