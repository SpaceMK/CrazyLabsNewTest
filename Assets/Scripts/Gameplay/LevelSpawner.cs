using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay.Collectibles;
using SledSurfers.Gameplay.Obstacles;
using UnityEngine;

namespace SledSurfers.Gameplay.Level
{
    public class LevelSpawner : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private Coin _coinPrefab;
        [SerializeField] private Obstacle _obstaclePrefab;

        [Header("Pool Settings")]
        [SerializeField] private int _prewarmCoins = 20;
        [SerializeField] private int _prewarmObstacles = 15;

        [Header("Lane Settings")]
        [SerializeField] private float _laneWidth = 3f;
        [SerializeField] private int _laneCount = 3;

        [Header("Initial Spawn (Before Launch)")]
        [SerializeField] private bool _spawnOnStart = true;
        [SerializeField] private float _initialSpawnDistance = 100f;
        [SerializeField] private float _initialSpawnInterval = 5f;
        [SerializeField] private float _initialSpawnStartZ = 10f;

        [Header("Runtime Spawn (During Gameplay)")]
        [SerializeField] private bool _spawnDuringGameplay = true;
        [SerializeField] private float _spawnAheadDistance = 50f;
        [SerializeField] private float _spawnInterval = 5f;
        [SerializeField][Range(0f, 1f)] private float _obstacleChance = 0.5f;

        private IPoolManager _poolManager;
        private Transform _player;
        private float _lastSpawnZ;
        private bool _isRuntimeSpawningEnabled;

        /// <summary>
        /// Initialize with dependencies. Call from GameplayFlow.Start().
        /// </summary>
        public void Initialize(IPoolManager poolManager, Transform player)
        {
            _poolManager = poolManager;
            _player = player;

            // 1. Prewarm pools
            Prewarm();

            // 2. Spawn initial level layout
            if (_spawnOnStart)
            {
                SpawnInitialLevel();
            }

            _lastSpawnZ = _player.position.z + _initialSpawnDistance;
        }

        /// <summary>
        /// Start runtime spawning. Call when player launches.
        /// </summary>
        public void StartRuntimeSpawning()
        {
            _isRuntimeSpawningEnabled = _spawnDuringGameplay;
        }

        /// <summary>
        /// Stop runtime spawning. Call when run ends.
        /// </summary>
        public void StopRuntimeSpawning()
        {
            _isRuntimeSpawningEnabled = false;
        }

        private void Prewarm()
        {
            if (_coinPrefab != null)
                _poolManager.Prewarm(_coinPrefab, _prewarmCoins);

            if (_obstaclePrefab != null)
                _poolManager.Prewarm(_obstaclePrefab, _prewarmObstacles);

            Debug.Log($"[LevelSpawner] Prewarmed {_prewarmCoins} coins, {_prewarmObstacles} obstacles.");
        }

        private void SpawnInitialLevel()
        {
            float z = _initialSpawnStartZ;
            float endZ = _initialSpawnStartZ + _initialSpawnDistance;

            while (z < endZ)
            {
                SpawnRow(z);
                z += _initialSpawnInterval;
            }

            Debug.Log($"[LevelSpawner] Spawned initial level from Z={_initialSpawnStartZ} to Z={endZ}.");
        }

        private void Update()
        {
            if (!_isRuntimeSpawningEnabled || _player == null) return;

            float targetZ = _player.position.z + _spawnAheadDistance;

            while (_lastSpawnZ < targetZ)
            {
                _lastSpawnZ += _spawnInterval;
                SpawnRow(_lastSpawnZ);
            }
        }

        /// <summary>
        /// Spawn a row of coins and possibly obstacles at the given Z position.
        /// </summary>
        public void SpawnRow(float zPosition)
        {
            int coinLane = Random.Range(0, _laneCount);
            int obstacleLane = Random.Range(0, _laneCount);

            // Don't spawn coin and obstacle in same lane
            while (obstacleLane == coinLane && _laneCount > 1)
            {
                obstacleLane = Random.Range(0, _laneCount);
            }

            float startX = -(_laneCount - 1) * _laneWidth / 2f;

            // Always spawn coin
            SpawnCoin(new Vector3(
                startX + coinLane * _laneWidth,
                1f,
                zPosition
            ));

            // Chance to spawn obstacle
            if (Random.value < _obstacleChance)
            {
                SpawnObstacle(new Vector3(
                    startX + obstacleLane * _laneWidth,
                    0f,
                    zPosition
                ));
            }
        }

        /// <summary>
        /// Spawn a coin at position.
        /// </summary>
        public Coin SpawnCoin(Vector3 position)
        {
            if (_coinPrefab == null || _poolManager == null) return null;

            var coin = _poolManager.Spawn(_coinPrefab, position, Quaternion.identity);
           // coin.Initialize(_poolManager);
            return coin;
        }

        /// <summary>
        /// Spawn an obstacle at position.
        /// </summary>
        public Obstacle SpawnObstacle(Vector3 position)
        {
            if (_obstaclePrefab == null || _poolManager == null) return null;

            return _poolManager.Spawn(_obstaclePrefab, position, Quaternion.identity);
        }

        /// <summary>
        /// Clear all spawned objects and reset. Call on retry.
        /// </summary>
        public void Reset()
        {
            _isRuntimeSpawningEnabled = false;
            _poolManager.ClearAll();
            Prewarm();

            if (_spawnOnStart)
            {
                SpawnInitialLevel();
            }

            _lastSpawnZ = _player != null
                ? _player.position.z + _initialSpawnDistance
                : _initialSpawnDistance;

            Debug.Log("[LevelSpawner] Reset complete.");
        }
    }
}