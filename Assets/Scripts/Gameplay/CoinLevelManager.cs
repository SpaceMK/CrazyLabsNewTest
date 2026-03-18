using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Manages coin placement on the level.
    /// Spawns coins at random positions within defined bounds.
    /// </summary>
    public class CoinLevelManager : MonoBehaviour
    {
        [Header("Spawn Bounds")]
        [SerializeField] private Vector3 _minBounds = new(-5f, 1f, 10f);
        [SerializeField] private Vector3 _maxBounds = new(5f, 1f, 100f);

        [Header("Settings")]
        [SerializeField] private int _coinCount = 20;
        [SerializeField] private float _coinHeight = 1f;

        private IPoolManager _poolManager;
        private readonly List<IPoolingObject> _activeCoins = new();

        public IReadOnlyList<IPoolingObject> ActiveCoins => _activeCoins;

        [Inject]
        public void Initialize(IPoolManager poolManager)
        {
            _poolManager = poolManager;
            Debug.Log($"[CoinLevelManager] PoolManager injected successfully. {poolManager == null}");
            SpawnAllCoins();
        }

        public void SpawnAllCoins()
        {
            for (int i = 0; i < _coinCount; i++)
            {
                Vector3 randomPosition = GetRandomPosition();
                SpawnCoin(randomPosition);
            }

            Debug.Log($"[CoinLevelManager] Spawned {_activeCoins.Count} coins.");
        }

        private Vector3 GetRandomPosition()
        {
            float x = Random.Range(_minBounds.x, _maxBounds.x);
            float z = Random.Range(_minBounds.z, _maxBounds.z);

            return new Vector3(x, _coinHeight, z);
        }

        public IPoolingObject SpawnCoin(Vector3 position)
        {
            var coin = _poolManager.Get(PoolObjectType.Coin);
            if (coin == null) return null;

            coin.GameObject.transform.position = position;
            _activeCoins.Add(coin);

            return coin;
        }

        public void DespawnCoin(IPoolingObject coin)
        {
            if (coin == null) return;

            _activeCoins.Remove(coin);
            _poolManager.Return(coin);
        }

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
        }

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