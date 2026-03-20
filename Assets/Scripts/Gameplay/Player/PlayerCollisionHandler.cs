using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Handles player collisions with obstacles and collectibles.
    /// 
    /// Depends on ICoinDespawner (not CoinLevelManager) to return
    /// collected coins to pool — DIP.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerCollisionHandler : MonoBehaviour, ICollisionHandler
    {
        private const string ObstacleTag = "Obstacle";
        private const string CoinTag = "Coin";

        private ICoinDespawner _coinDespawner;

        public event Action OnCrash;
        public event Action<int> OnCoinCollected;

        public void Initialize(ICoinDespawner coinDespawner)
        {
            _coinDespawner = coinDespawner;
            Debug.Log("[PlayerCollisionHandler] Initialized with ICoinDespawner.");
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
            if (other.CompareTag(CoinTag))
            {
                Debug.Log($"[Collision] Coin collected: {other.gameObject.name}");

                OnCoinCollected?.Invoke(1);
                _coinDespawner?.DespawnCoin(other.gameObject);
            }
        }
    }
}
