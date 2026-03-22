using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Gameplay.Level
{
    /// <summary>
    /// Interface for providing spawn positions to other managers.
    /// Allows decoupled position sharing between CoinLevelManager and ObstacleLevelManager.
    /// </summary>
    public interface ISpawnPositionProvider
    {
        IReadOnlyList<Vector3> GetOccupiedPositions();
    }
}
