using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Tracks player momentum and fires OnMomentumLost when speed drops to zero.
    /// Subscribes to IPlayerMotor.OnSpeedChanged internally.
    /// 
    /// Implements IDisposable to properly unsubscribe from motor events.
    /// </summary>
    public class MomentumTracker : IMomentumTracker, IDisposable
    {
        private readonly IPlayerMotor _motor;
        private readonly float _stallDuration;

        private float _stoppedAtTime;
        private bool _isStopped;
        private bool _isTracking;

        public bool IsTracking => _isTracking;
        public float CurrentMomentum => _motor.CurrentSpeed;

        public event Action OnMomentumLost;

        public MomentumTracker(IPlayerMotor motor, float stallDuration = 1.5f)
        {
            _motor = motor;
            _stallDuration = stallDuration;

            _motor.OnSpeedChanged += HandleSpeedChanged;
        }

        public void StartTracking()
        {
            _isTracking = true;
            _isStopped = false;
            Debug.Log("[MomentumTracker] Started tracking.");
        }

        public void StopTracking()
        {
            _isTracking = false;
            _isStopped = false;
            Debug.Log("[MomentumTracker] Stopped tracking.");
        }

        private void HandleSpeedChanged(float speed)
        {
            if (!_isTracking) return;

            if (speed <= 0.1f)
            {
                _isStopped = true;
                OnMomentumLost?.Invoke();
                StopTracking();
            }
        }

        public void Dispose()
        {
            _motor.OnSpeedChanged -= HandleSpeedChanged;
        }
    }
}
