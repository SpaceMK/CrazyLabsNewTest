using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Tracks player momentum and fires event when player stalls.
    /// 
    /// Subscribes directly to IPlayerMotor.OnSpeedChanged - no middleman needed.
    /// Self-contained system that manages its own state.
    /// </summary>
    public sealed class MomentumTracker : IMomentumTracker
    {
        private readonly IPlayerMotor _motor;
        private readonly float _stallThreshold;
        private readonly float _stallDuration;

        private float _timeUnderThreshold;
        private bool _isTracking;

        public bool IsTracking => _isTracking;
        public float CurrentMomentum { get; private set; }

        public event Action OnMomentumLost;

        public MomentumTracker(IPlayerMotor motor, float stallThreshold = 2f, float stallDuration = 1.5f)
        {
            _motor = motor;
            _stallThreshold = stallThreshold;
            _stallDuration = stallDuration;

            // Subscribe to motor speed changes
            _motor.OnSpeedChanged += HandleSpeedChanged;
        }

        public void StartTracking()
        {
            _isTracking = true;
            _timeUnderThreshold = 0f;
            Debug.Log("[MomentumTracker] Started tracking.");
        }

        public void StopTracking()
        {
            _isTracking = false;
            _timeUnderThreshold = 0f;
            Debug.Log("[MomentumTracker] Stopped tracking.");
        }

        private void HandleSpeedChanged(float speed)
        {
            if (!_isTracking) return;

            CurrentMomentum = speed;

            if (speed < _stallThreshold)
            {
                // Approximate time since last update (speed events are throttled)
                _timeUnderThreshold += Time.deltaTime;

                if (_timeUnderThreshold >= _stallDuration)
                {
                    Debug.Log($"[MomentumTracker] Momentum lost! Speed: {speed:F1}");
                    OnMomentumLost?.Invoke();
                    StopTracking();
                }
            }
            else
            {
                _timeUnderThreshold = 0f;
            }
        }

        /// <summary>
        /// Call this to unsubscribe from events (cleanup).
        /// </summary>
        public void Dispose()
        {
            _motor.OnSpeedChanged -= HandleSpeedChanged;
        }
    }
}
