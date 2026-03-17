using System;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Tracks data for the current run.
    /// Subscribes to player events internally.
    /// </summary>
    public interface IRunSession
    {
        bool IsRunning { get; }
        float DistanceTraveled { get; }
        int CoinsCollected { get; }

        event Action OnRunStarted;
        event Action OnRunEnded;
        event Action<float> OnDistanceUpdated;
        event Action<int> OnCoinsUpdated;

        void StartRun();
        void EndRun();
        void AddCoins(int amount);
        void UpdateDistance(float distance);
    }
}
