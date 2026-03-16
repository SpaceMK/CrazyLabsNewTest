namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Tracks data for a single run session (coins collected, distance, etc.).
    /// SRP - only tracks run metrics, doesn't handle persistence.
    /// </summary>
    public interface IRunSession
    {
        int CoinsCollected { get; }
        float DistanceTraveled { get; }
        bool IsRunActive { get; }
        
        void StartRun();
        void EndRun();
        void AddCoins(int amount);
        void UpdateDistance(float distance);
        
        event System.Action OnRunStarted;
        event System.Action<RunResult> OnRunEnded;
    }

    public struct RunResult
    {
        public int CoinsCollected;
        public float DistanceTraveled;
    }
}
