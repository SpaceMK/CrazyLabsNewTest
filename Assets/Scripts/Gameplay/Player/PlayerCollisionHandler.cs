using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Handles player collisions with obstacles and collectibles.
    /// 
    /// Depends on IEntityDespawner (generic) to return collected entities to pool — DIP.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerCollisionHandler : MonoBehaviour, ICollisionHandler
    {
        private const string ObstacleTag = "Obstacle";
        private const string CoinTag = "Coin";

        private IEntityDespawner _entityDespawner;

        public event Action OnCrash;
        public event Action<int> OnCoinCollected;

        public void Initialize(IEntityDespawner entityDespawner)
        {
            _entityDespawner = entityDespawner;
            Debug.Log("[PlayerCollisionHandler] Initialized with IEntityDespawner.");
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
                _entityDespawner?.DespawnEntity(other.gameObject);
            }
        }
    }
}