namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Monitors player speed and determines if momentum is lost.
    /// SRP - only tracks momentum state, doesn't end the run itself.
    /// </summary>
    public interface IMomentumTracker
    {
        bool HasMomentum { get; }
        float MomentumPercent { get; }

        void StartTracking();
        void StopTracking();
        void UpdateSpeed(float currentSpeed);

        event System.Action OnMomentumLost;
    }
}
