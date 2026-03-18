using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Interface for the pool manager service.
    /// Allows spawning and despawning pooled objects.
    /// </summary>
    public interface IPoolManager
    {
        /// <summary>
        /// Get an object from the pool.
        /// </summary>
        T Spawn<T>(T prefab, Vector3 position, Quaternion rotation) where T : Component, IPoolable;

        /// <summary>
        /// Return an object to the pool.
        /// </summary>
        void Despawn<T>(T instance) where T : Component, IPoolable;

        /// <summary>
        /// Pre-warm a pool with instances.
        /// </summary>
        void Prewarm<T>(T prefab, int count) where T : Component, IPoolable;

        /// <summary>
        /// Clear all pools and destroy pooled objects.
        /// </summary>
        void ClearAll();
    }
}
