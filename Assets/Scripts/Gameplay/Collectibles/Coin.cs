using SledSurfers.Pooling;
using UnityEngine;

namespace SledSurfers.Gameplay.Collectibles
{
    /// <summary>
    /// Poolable coin collectible.
    /// Returns to pool when collected instead of being destroyed.
    /// </summary>
    [RequireComponent(typeof(Collider))]
    public class Coin : PoolableObject
    {
        [Header("Coin Settings")]
        [SerializeField] private int _value = 1;
        [SerializeField] private float _rotationSpeed = 90f;
        [SerializeField] private float _bobSpeed = 2f;
        [SerializeField] private float _bobHeight = 0.2f;

        private Vector3 _startPosition;
        private float _bobOffset;

        public int Value => _value;

        public override void OnSpawn()
        {
            _startPosition = transform.position;
            _bobOffset = Random.Range(0f, Mathf.PI * 2f); // Random start phase
        }

        public override void OnDespawn()
        {
            // Reset any state if needed
        }

        private void Update()
        {
            // Rotate
            transform.Rotate(Vector3.up, _rotationSpeed * Time.deltaTime);

            // Bob up and down
            float newY = _startPosition.y + Mathf.Sin((Time.time + _bobOffset) * _bobSpeed) * _bobHeight;
            transform.position = new Vector3(transform.position.x, newY, transform.position.z);
        }

        /// <summary>
        /// Called by PlayerCollisionHandler when collected.
        /// </summary>
        public void Collect()
        {
            ReturnToPool();
        }
    }
}
