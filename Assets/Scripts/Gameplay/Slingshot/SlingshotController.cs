using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;

namespace SledSurfers.Gameplay.Slingshot
{
    public class SlingshotController : ISlingshot
    {
        private GameSettings _settings;
        private int _launchPowerLevel;
        private readonly float _maxChargeTime;

        private float _chargeTime;
        private bool _isCharging;

        public bool IsReady { get; private set; } = true;
        public bool IsCharging => _isCharging;
        public float ChargePercent => _maxChargeTime > 0f ? Mathf.Clamp01(_chargeTime / _maxChargeTime) : 0f;

        public event Action OnChargeStarted;
        public event Action<float> OnChargeUpdated;
        public event Action<Vector3> OnReleased;

        public SlingshotController(GameSettings settings, int launchPowerLevel)
        {
            _settings = settings;
            _launchPowerLevel = launchPowerLevel;
            _maxChargeTime = 1.5f;
        }

        public void UpdateStats(GameSettings settings, int launchPowerLevel)
        {
            _settings = settings;
            _launchPowerLevel = launchPowerLevel;
        }

        public void StartCharging()
        {
            if (!IsReady) return;

            _isCharging = true;
            _chargeTime = 0f;
            IsReady = false;

            Debug.Log("[Slingshot] Charging started.");
            OnChargeStarted?.Invoke();
        }

        public void UpdateCharge(float deltaTime)
        {
            if (!_isCharging) return;

            _chargeTime = Mathf.Min(_chargeTime + deltaTime, _maxChargeTime);
            OnChargeUpdated?.Invoke(ChargePercent);
        }

        public Vector3 Release()
        {
            if (!_isCharging) return Vector3.zero;

            _isCharging = false;

            float launchForce = _settings.BaseLaunchForce
                + _settings.LaunchForcePerLevel * (_launchPowerLevel - 1);

            float finalForce = launchForce * ChargePercent;
            Vector3 launchDirection = (Vector3.forward + Vector3.up * 0.3f).normalized;
            Vector3 force = launchDirection * finalForce;

            Debug.Log($"[Slingshot] Released! Force: {finalForce:F1}, Charge: {ChargePercent:P0}");
            OnReleased?.Invoke(force);

            return force;
        }

        public void Reset()
        {
            _isCharging = false;
            _chargeTime = 0f;
            IsReady = true;
        }
    }
}