using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Manages obstacle placement on the level.
    /// Implements ISpawnPositionProvider so CoinLevelManager can avoid spawning near obstacles.
    /// Implements ILevelManager so GameplayFlow can reset without knowing the concrete type.
    /// 
    /// NOTE: Initialize() only wires dependencies — it does NOT spawn.
    /// Spawning is triggered by GameplayFlow.Start() via ILevelManager.Reset(),
    /// which runs after all [Inject] methods complete, guaranteeing pools are ready.
    /// </summary>
    public class ObstacleLevelManager : MonoBehaviour, ISpawnPositionProvider, ILevelManager
    {
        [Header("Spawn Bounds")]
        [SerializeField] private Vector3 _minBounds = new(-5f, 0f, 20f);
        [SerializeField] private Vector3 _maxBounds = new(5f, 0f, 200f);

        [Header("Settings")]
        [SerializeField] private int _obstacleCount = 15;
        [SerializeField] private float _obstacleHeight = 0f;
        [SerializeField] private bool _randomRotation = true;

        [Header("Dispersion")]
        [SerializeField] private float _minDistanceBetweenObstacles = 5f;
        [SerializeField] private int _maxSpawnAttempts = 50;

        private IPoolManager _poolManager;
        private readonly List<IPoolingObject> _activeObstacles = new();
        private readonly List<Vector3> _spawnedPositions = new();

        public IReadOnlyList<IPoolingObject> ActiveObstacles => _activeObstacles;

        /// <summary>
        /// Wires dependencies only — does NOT spawn. See class summary.
        /// </summary>
        [Inject]
        public void Initialize(IPoolManager poolManager)
        {
            _poolManager = poolManager;
            Debug.Log("[ObstacleLevelManager] Initialized.");
        }

        public IReadOnlyList<Vector3> GetOccupiedPositions()
        {
            return _spawnedPositions;
        }

        public void SpawnAllObstacles()
        {
            _spawnedPositions.Clear();
            int spawned = 0;

            for (int i = 0; i < _obstacleCount; i++)
            {
                Vector3? position = FindValidPosition();

                if (position.HasValue)
                {
                    SpawnObstacle(position.Value);
                    spawned++;
                }
            }

            Debug.Log($"[ObstacleLevelManager] Spawned {spawned}/{_obstacleCount} obstacles.");
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

            return new Vector3(x, _obstacleHeight, z);
        }

        private bool IsValidPosition(Vector3 position)
        {
            foreach (var existing in _spawnedPositions)
            {
                float distance = Vector3.Distance(
                    new Vector3(position.x, 0, position.z),
                    new Vector3(existing.x, 0, existing.z)
                );

                if (distance < _minDistanceBetweenObstacles)
                {
                    return false;
                }
            }

            return true;
        }

        public IPoolingObject SpawnObstacle(Vector3 position)
        {
            var obstacle = _poolManager.Get(PoolObjectType.Obstacle);
            if (obstacle == null) return null;

            obstacle.GameObject.transform.position = position;

            if (_randomRotation)
            {
                float randomY = Random.Range(0f, 360f);
                obstacle.GameObject.transform.rotation = Quaternion.Euler(0f, randomY, 0f);
            }

            _activeObstacles.Add(obstacle);
            _spawnedPositions.Add(position);

            return obstacle;
        }

        public void DespawnObstacle(IPoolingObject obstacle)
        {
            if (obstacle == null) return;

            _activeObstacles.Remove(obstacle);
            _poolManager.Return(obstacle);
        }

        public void DespawnObstacle(GameObject obstacleGameObject)
        {
            var obstacle = _activeObstacles.Find(o => o.GameObject == obstacleGameObject);
            if (obstacle != null)
            {
                DespawnObstacle(obstacle);
            }
        }

        public void DespawnAll()
        {
            for (int i = _activeObstacles.Count - 1; i >= 0; i--)
            {
                _poolManager.Return(_activeObstacles[i]);
            }
            _activeObstacles.Clear();
            _spawnedPositions.Clear();
        }

        /// <summary>
        /// ILevelManager implementation.
        /// </summary>
        public void Reset()
        {
            DespawnAll();
            SpawnAllObstacles();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);

            Vector3 center = (_minBounds + _maxBounds) / 2f;
            Vector3 size = _maxBounds - _minBounds;

            Gizmos.DrawCube(center, size);

            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);
        }
#endif
    }
}