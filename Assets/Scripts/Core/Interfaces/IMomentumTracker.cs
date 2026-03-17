using System;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Tracks player momentum and detects when player has stalled.
    /// Subscribes to IPlayerMotor.OnSpeedChanged internally.
    /// </summary>
    public interface IMomentumTracker
    {
        bool IsTracking { get; }
        float CurrentMomentum { get; }

        /// <summary>
        /// Fired when player speed drops below threshold for too long.
        /// </summary>
        event Action OnMomentumLost;

        void StartTracking();
        void StopTracking();
    }
}
