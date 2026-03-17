using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay
{
    /// <summary>
    /// Tracks data for the current run: distance, coins, timing.
    /// 
    /// Self-contained - exposes events for UI to subscribe to.
    /// </summary>
    public sealed class RunSession : IRunSession
    {
        private readonly Transform _playerTransform;
        private readonly Vector3 _startPosition;

        public bool IsRunning { get; private set; }
        public float DistanceTraveled { get; private set; }
        public int CoinsCollected { get; private set; }

        public event Action OnRunStarted;
        public event Action OnRunEnded;
        public event Action<float> OnDistanceUpdated;
        public event Action<int> OnCoinsUpdated;

        public RunSession(Transform playerTransform)
        {
            _playerTransform = playerTransform;
            _startPosition = playerTransform.position;
        }

        public void StartRun()
        {
            IsRunning = true;
            DistanceTraveled = 0f;
            CoinsCollected = 0;

            Debug.Log("[RunSession] Run started.");
            OnRunStarted?.Invoke();
        }

        public void EndRun()
        {
            IsRunning = false;

            Debug.Log($"[RunSession] Run ended. Distance: {DistanceTraveled:F1}m, Coins: {CoinsCollected}");
            OnRunEnded?.Invoke();
        }

        public void UpdateDistance(float distance)
        {
            if (!IsRunning) return;

            DistanceTraveled = distance;
            OnDistanceUpdated?.Invoke(distance);
        }

        public void AddCoins(int amount)
        {
            if (!IsRunning) return;

            CoinsCollected += amount;
            OnCoinsUpdated?.Invoke(CoinsCollected);
        }

        /// <summary>
        /// Call this from a MonoBehaviour Update to track distance automatically.
        /// </summary>
        public void Tick()
        {
            if (!IsRunning) return;

            float distance = Vector3.Distance(_startPosition, _playerTransform.position);
            UpdateDistance(distance);
        }
    }
}
