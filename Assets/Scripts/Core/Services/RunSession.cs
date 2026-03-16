using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Tracks a single gameplay run's metrics.
    /// SRP - only tracks data, doesn't control gameplay flow.
    /// </summary>
    public sealed class RunSession : IRunSession
    {
        public int CoinsCollected { get; private set; }
        public float DistanceTraveled { get; private set; }
        public bool IsRunActive { get; private set; }

        public event Action OnRunStarted;
        public event Action<RunResult> OnRunEnded;

        public void StartRun()
        {
            CoinsCollected = 0;
            DistanceTraveled = 0f;
            IsRunActive = true;
            
            Debug.Log("[RunSession] Run started.");
            OnRunStarted?.Invoke();
        }

        public void EndRun()
        {
            if (!IsRunActive) return;

            IsRunActive = false;
            
            var result = new RunResult
            {
                CoinsCollected = CoinsCollected,
                DistanceTraveled = DistanceTraveled
            };

            Debug.Log($"[RunSession] Run ended. Coins: {CoinsCollected}, Distance: {DistanceTraveled:F1}");
            OnRunEnded?.Invoke(result);
        }

        public void AddCoins(int amount)
        {
            if (!IsRunActive) return;
            CoinsCollected += amount;
        }

        public void UpdateDistance(float distance)
        {
            if (!IsRunActive) return;
            DistanceTraveled = distance;
        }
    }
}
