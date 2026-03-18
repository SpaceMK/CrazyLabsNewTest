using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay.Level;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Handles player collisions with obstacles and collectibles.
    /// Notifies PoolManager to return collected coins to pool.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerCollisionHandler : MonoBehaviour, ICollisionHandler
    {
        private const string ObstacleTag = "Obstacle";
        private const string CoinTag = "Coin";
        private CoinLevelManager _coinLevelManager;
        public event Action OnCrash;
        public event Action<int> OnCoinCollected;

        public void Initialize(CoinLevelManager coinLevelManager)
        {
            _coinLevelManager = coinLevelManager;
            Debug.Log($"[PlayerCollisionHandler] PoolManager injected successfully");
        }
       

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(ObstacleTag))
            {
                Debug.Log($"[Collision] Crashed into: {collision.gameObject.name}");
                OnCrash?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            Debug.Log($"[Trigger] Entered trigger: {other.gameObject.name} with tag {other.gameObject.tag}");
            if (other.CompareTag(CoinTag))
            {
                Debug.Log($"[Collision] Coin collected: {other.gameObject.name}");

                // Notify listeners
                OnCoinCollected?.Invoke(1);
                _coinLevelManager.DespawnCoin(other.gameObject);
               
            }
        }
    }
}