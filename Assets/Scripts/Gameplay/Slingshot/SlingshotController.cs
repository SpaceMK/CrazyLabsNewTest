using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;

namespace SledSurfers.Gameplay.Slingshot
{
    /// <summary>
    /// Slingshot that charges over time and releases the player downhill.
    /// Depends on GameSettings for force values (DIP via constructor injection).
    /// </summary>
    public sealed class SlingshotController : ISlingshot
    {
        private readonly GameSettings _settings;
        private readonly int _launchPowerLevel;

        private float _chargeTime;
        private float _maxChargeTime;
        private bool _isCharging;

        public bool IsReady { get; private set; } = true;
        public float ChargePercent => _maxChargeTime > 0f ? Mathf.Clamp01(_chargeTime / _maxChargeTime) : 0f;

        public event Action OnChargeStarted;
        public event Action<float> OnChargeUpdated;
        public event Action<float> OnReleased;

        public SlingshotController(GameSettings settings, int launchPowerLevel)
        {
            _settings = settings;
            _launchPowerLevel = launchPowerLevel;
            _maxChargeTime = 1.5f;
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

        /// <summary>
        /// Call from MonoBehaviour Update to advance the charge.
        /// </summary>
        public void UpdateCharge(float deltaTime)
        {
            if (!_isCharging) return;

            _chargeTime = Mathf.Min(_chargeTime + deltaTime, _maxChargeTime);
            OnChargeUpdated?.Invoke(ChargePercent);
        }

        public void Release(Rigidbody target)
        {
            if (!_isCharging) return;

            _isCharging = false;

            float launchForce = _settings.BaseLaunchForce
                + _settings.LaunchForcePerLevel * (_launchPowerLevel - 1);

            float finalForce = launchForce * ChargePercent;

            // Launch forward and slightly upward for the arc feel
            Vector3 launchDirection = (Vector3.forward + Vector3.up * 0.3f).normalized;
            target.AddForce(launchDirection * finalForce, ForceMode.Impulse);

            Debug.Log($"[Slingshot] Released! Force: {finalForce:F1}, Charge: {ChargePercent:P0}");
            OnReleased?.Invoke(finalForce);
        }

        public void Reset()
        {
            _isCharging = false;
            _chargeTime = 0f;
            IsReady = true;
        }
    }
}
