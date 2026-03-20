using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for despawning coins on collection.
    /// Decouples PlayerCollisionHandler from CoinLevelManager (SRP + DIP).
    /// </summary>
    public interface ICoinDespawner
    {
        void DespawnCoin(GameObject coinGameObject);
    }
}
