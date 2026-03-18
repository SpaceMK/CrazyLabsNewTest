using SledSurfers.Pooling;
using UnityEngine;

namespace SledSurfers.Gameplay.Obstacles
{
    /// <summary>
    /// Poolable obstacle.
    /// Can be recycled when player passes it.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Obstacle : PoolableObject
    {
        [Header("Obstacle Settings")]
        [SerializeField] private float _despawnDistance = 20f;

        private Transform _playerTransform;
        private bool _isActive;

        public override void OnSpawn()
        {
            _isActive = true;
            
            // Find player (cached for performance)
            if (_playerTransform == null)
            {
                var player = FindFirstObjectByType<Player.PlayerManager>();
                if (player != null)
                {
                    _playerTransform = player.transform;
                }
            }
        }

        public override void OnDespawn()
        {
            _isActive = false;
        }

        private void Update()
        {
            if (!_isActive || _playerTransform == null) return;

            // Check if player has passed this obstacle
            if (transform.position.z < _playerTransform.position.z - _despawnDistance)
            {
                ReturnToPool();
            }
        }
    }
}
