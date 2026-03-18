using System;
using SledSurfers.Core.Interfaces;
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

        public event Action OnCrash;
        public event Action<int> OnCoinCollected;

        /// <summary>
        /// Initialize with pool manager reference.
        /// </summary>
       

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

                // Notify listeners
                OnCoinCollected?.Invoke(1);

                // Return to pool
               /* if (_poolManager != null)
                {
                    _poolManager.Despawn(other.gameObject);
                }
                else
                {
                    other.gameObject.SetActive(false);
                }*/
            }
        }
    }
}