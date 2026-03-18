using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Pooling
{
    /// <summary>
    /// Base class for all poolable objects.
    /// Inherit from this instead of MonoBehaviour for objects that need pooling.
    /// 
    /// Handles the IPoolable interface and provides common functionality.
    /// </summary>
    public abstract class PoolableObject : MonoBehaviour, IPoolable
    {
        [SerializeField] private string _poolId;

        /// <summary>
        /// Reference to the pool manager for self-despawning.
        /// Set by PoolManager when object is created.
        /// </summary>
        public IPoolManager PoolManager { get; set; }

        public string PoolId => string.IsNullOrEmpty(_poolId) ? gameObject.name.Replace("(Clone)", "").Trim() : _poolId;

        /// <summary>
        /// Called when object is retrieved from pool.
        /// Override to reset your object's state.
        /// </summary>
        public virtual void OnSpawn()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Called when object is returned to pool.
        /// Override to cleanup your object.
        /// </summary>
        public virtual void OnDespawn()
        {
            // Override in derived classes
        }

        /// <summary>
        /// Return this object to its pool.
        /// Call this instead of Destroy() or SetActive(false).
        /// </summary>
        protected void ReturnToPool()
        {
            PoolManager?.Despawn(this);
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            if (string.IsNullOrEmpty(_poolId))
            {
                _poolId = gameObject.name;
            }
        }
#endif
    }
}
