using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Monitors player speed. If speed stays below threshold for a duration, momentum is lost.
    /// SRP - only monitors, doesn't decide what happens when momentum is lost.
    /// </summary>
    public sealed class MomentumTracker : IMomentumTracker
    {
        private readonly float _minSpeedThreshold;
        private readonly float _gracePeriod;

        private float _timeBelowThreshold;
        private float _maxSpeedReached;
        private bool _isTracking;

        public bool HasMomentum { get; private set; } = true;
        public float MomentumPercent => _maxSpeedReached > 0f
            ? Mathf.Clamp01(_lastSpeed / _maxSpeedReached)
            : 0f;

        private float _lastSpeed;

        public event Action OnMomentumLost;

        /// <param name="minSpeedThreshold">Speed below which the player is considered stalling</param>
        /// <param name="gracePeriod">Seconds allowed below threshold before momentum is lost</param>
        public MomentumTracker(float minSpeedThreshold = 1.5f, float gracePeriod = 1.5f)
        {
            _minSpeedThreshold = minSpeedThreshold;
            _gracePeriod = gracePeriod;
        }

        public void StartTracking()
        {
            _isTracking = true;
            _timeBelowThreshold = 0f;
            _maxSpeedReached = 0f;
            HasMomentum = true;

            Debug.Log("[Momentum] Tracking started.");
        }

        public void StopTracking()
        {
            _isTracking = false;
        }

        public void UpdateSpeed(float currentSpeed)
        {
            if (!_isTracking || !HasMomentum) return;

            _lastSpeed = currentSpeed;

            if (currentSpeed > _maxSpeedReached)
            {
                _maxSpeedReached = currentSpeed;
            }

            if (currentSpeed < _minSpeedThreshold && _maxSpeedReached > _minSpeedThreshold)
            {
                _timeBelowThreshold += Time.deltaTime;

                if (_timeBelowThreshold >= _gracePeriod)
                {
                    HasMomentum = false;
                    Debug.Log("[Momentum] Momentum lost!");
                    OnMomentumLost?.Invoke();
                }
            }
            else
            {
                _timeBelowThreshold = 0f;
            }
        }
    }
}
