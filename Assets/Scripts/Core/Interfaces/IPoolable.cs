namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Interface for objects that can be pooled.
    /// Implement this on MonoBehaviours that will be spawned/despawned frequently.
    /// </summary>
    public interface IPoolable
    {
        /// <summary>
        /// The prefab ID used to identify which pool this object belongs to.
        /// </summary>
        string PoolId { get; }

        /// <summary>
        /// Called when object is retrieved from pool (before activation).
        /// Use to reset state.
        /// </summary>
        void OnSpawn();

        /// <summary>
        /// Called when object is returned to pool (before deactivation).
        /// Use to cleanup.
        /// </summary>
        void OnDespawn();
    }
}
