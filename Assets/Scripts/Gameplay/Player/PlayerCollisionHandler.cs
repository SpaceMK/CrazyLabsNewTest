using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// MonoBehaviour that listens for Unity collision/trigger events and
    /// raises typed events through ICollisionHandler.
    /// 
    /// SRP - only translates Unity callbacks into domain events.
    /// Must live on the player GameObject (requires Collider + Rigidbody).
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(Collider))]
    public sealed class PlayerCollisionHandler : MonoBehaviour, ICollisionHandler
    {
        private const string ObstacleTag = "Obstacle";
        private const string CoinTag = "Coin";

        public event Action OnCrash;
        public event Action<int> OnCoinCollected;

        private void OnCollisionEnter(Collision collision)
        {
            if (collision.gameObject.CompareTag(ObstacleTag))
            {
                Debug.Log($"[Collision] Crashed into obstacle: {collision.gameObject.name}");
                OnCrash?.Invoke();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag(CoinTag))
            {
                Debug.Log($"[Collision] Coin collected: {other.gameObject.name}");
                OnCoinCollected?.Invoke(1);
                other.gameObject.SetActive(false);
            }
        }
    }
}
