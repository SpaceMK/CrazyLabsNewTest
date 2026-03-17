using System;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Handles player collision events.
    /// </summary>
    public interface ICollisionHandler
    {
        event Action OnCrash;
        event Action<int> OnCoinCollected;
    }
}
