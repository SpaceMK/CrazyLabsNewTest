using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Level
{
    [System.Serializable]
    public class ObstacleSpawnEntry
    {
        public PoolObjectType ObstacleType;

        [Tooltip("Relative spawn weight. Higher = more likely to spawn.")]
        [Range(0.01f, 100f)]
        public float weight = 1f;
    }

    public class ObstacleLevelManager : MonoBehaviour, ISpawnPositionProvider, ILevelManager
    {
        [Header("Spawn Bounds (XZ area to scatter within)")]
        [SerializeField] private float _minX = -5f;
        [SerializeField] private float _maxX = 5f;
        [SerializeField] private float _minZ = 20f;
        [SerializeField] private float _maxZ = 200f;

        [Header("Raycast")]
        [Tooltip("Height from which to raycast downward to find the ground.")]
        [SerializeField] private float _raycastOriginY = 50f;
        [Tooltip("Maximum raycast distance.")]
        [SerializeField] private float _raycastDistance = 100f;
        [Tooltip("Layer mask for ground surfaces. Set this to your ground layer.")]
        [SerializeField] private LayerMask _groundLayer = ~0;

        [Header("Settings")]
        [SerializeField] private int _obstacleCount = 15;
        [Tooltip("Height offset above the ground surface.")]
        [SerializeField] private float _surfaceOffset = 0f;
        [SerializeField] private bool _alignToSurface = true;
        [SerializeField] private bool _randomYRotation = true;

        [Header("Dispersion")]
        [SerializeField] private float _minDistanceBetweenObstacles = 5f;
        [SerializeField] private int _maxSpawnAttempts = 50;

        [Header("Obstacle Types")]
        [Tooltip("Which obstacle pool types to spawn and their relative weights.")]
        [SerializeField]
        private List<ObstacleSpawnEntry> _obstacleTypes = new List<ObstacleSpawnEntry>();

        private float _totalWeight;
        private IPoolManager _poolManager;
        private readonly List<IPoolingObject> _activeObstacles = new();
        private readonly List<Vector3> _spawnedPositions = new();

        public IReadOnlyList<IPoolingObject> ActiveObstacles => _activeObstacles;
        [Inject]
        public void Initialize(IPoolManager poolManager)
        {
            _poolManager = poolManager;
            CacheTotalWeight();
            Debug.Log("[ObstacleLevelManager] Initialized.");
        }

        private void CacheTotalWeight()
        {
            _totalWeight = 0f;
            foreach (var entry in _obstacleTypes)
                _totalWeight += entry.weight;
        }

        private PoolObjectType GetRandomObstacleType()
        {
            float roll = Random.Range(0f, _totalWeight);
            float cumulative = 0f;

            foreach (var entry in _obstacleTypes)
            {
                cumulative += entry.weight;
                if (roll <= cumulative)
                    return entry.ObstacleType;
            }

            return _obstacleTypes[_obstacleTypes.Count - 1].ObstacleType;
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
                if (TryFindValidPosition(out Vector3 position, out Vector3 surfaceNormal))
                {
                    SpawnObstacle(position, surfaceNormal);
                    spawned++;
                }
            }

            Debug.Log($"[ObstacleLevelManager] Spawned {spawned}/{_obstacleCount} obstacles.");
        }

        private bool TryFindValidPosition(out Vector3 position, out Vector3 normal)
        {
            for (int attempt = 0; attempt < _maxSpawnAttempts; attempt++)
            {
                float x = Random.Range(_minX, _maxX);
                float z = Random.Range(_minZ, _maxZ);

                Vector3 rayOrigin = new Vector3(x, _raycastOriginY, z);

                if (Physics.Raycast(rayOrigin, Vector3.down, out RaycastHit hit, _raycastDistance, _groundLayer))
                {
                    Vector3 candidate = hit.point + hit.normal * _surfaceOffset;

                    if (IsValidPosition(candidate))
                    {
                        position = candidate;
                        normal = hit.normal;
                        return true;
                    }
                }
            }

            position = Vector3.zero;
            normal = Vector3.up;
            return false;
        }

        private bool IsValidPosition(Vector3 position)
        {
            foreach (var existing in _spawnedPositions)
            {
                float dx = position.x - existing.x;
                float dz = position.z - existing.z;
                float distXZ = Mathf.Sqrt(dx * dx + dz * dz);

                if (distXZ < _minDistanceBetweenObstacles)
                {
                    return false;
                }
            }

            return true;
        }

        public IPoolingObject SpawnObstacle(Vector3 position, Vector3 surfaceNormal)
        {
            var obstacle = _poolManager.Get(GetRandomObstacleType());
            if (obstacle == null) return null;

            var t = obstacle.GameObject.transform;
            t.position = position;

            if (_alignToSurface)
            {
                Quaternion surfaceAlignment = Quaternion.FromToRotation(Vector3.up, surfaceNormal);
                float yAngle = _randomYRotation ? Random.Range(0f, 360f) : 0f;
                t.rotation = surfaceAlignment * Quaternion.Euler(0f, yAngle, 0f);
            }
            else if (_randomYRotation)
            {
                t.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            }

            _activeObstacles.Add(obstacle);
            _spawnedPositions.Add(position);

            return obstacle;
        }

        public IPoolingObject SpawnObstacle(Vector3 position)
        {
            return SpawnObstacle(position, Vector3.up);
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

        public void Reset()
        {
            DespawnAll();
            SpawnAllObstacles();
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Vector3 center = new Vector3(
                (_minX + _maxX) / 2f,
                _raycastOriginY,
                (_minZ + _maxZ) / 2f
            );
            Vector3 size = new Vector3(_maxX - _minX, 0.1f, _maxZ - _minZ);

            Gizmos.color = new Color(1f, 0f, 0f, 0.15f);
            Gizmos.DrawCube(center, size);
            Gizmos.color = Color.red;
            Gizmos.DrawWireCube(center, size);

            Gizmos.color = new Color(1f, 0f, 0f, 0.3f);
            DrawRayGizmo(_minX, _minZ);
            DrawRayGizmo(_maxX, _minZ);
            DrawRayGizmo(_minX, _maxZ);
            DrawRayGizmo(_maxX, _maxZ);
        }

        private void DrawRayGizmo(float x, float z)
        {
            Vector3 from = new Vector3(x, _raycastOriginY, z);
            Vector3 to = new Vector3(x, _raycastOriginY - _raycastDistance, z);
            Gizmos.DrawLine(from, to);
        }
#endif
    }
}