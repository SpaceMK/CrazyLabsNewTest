using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Handles all player physics: downhill acceleration, steering, drag.
    /// Wraps a Rigidbody but is injected with settings (DIP).
    /// SRP - only physics, no game logic.
    /// </summary>
    public sealed class PlayerMotor : IPlayerMotor
    {
        private readonly Rigidbody _rigidbody;
        private readonly GameSettings _settings;
        private readonly int _maxSpeedLevel;
        private readonly int _steeringLevel;

        private float _maxSpeed;
        private float _steeringSpeed;

        public Vector3 Velocity => _rigidbody.linearVelocity;
        public float CurrentSpeed => _rigidbody.linearVelocity.magnitude;
        public bool IsMoving => CurrentSpeed > 0.5f;

        public PlayerMotor(Rigidbody rigidbody, GameSettings settings, int maxSpeedLevel, int steeringLevel)
        {
            _rigidbody = rigidbody;
            _settings = settings;
            _maxSpeedLevel = maxSpeedLevel;
            _steeringLevel = steeringLevel;

            _maxSpeed = _settings.BaseMaxSpeed + _settings.MaxSpeedPerLevel * (_maxSpeedLevel - 1);
            _steeringSpeed = _settings.BaseSteeringSpeed + _settings.SteeringSpeedPerLevel * (_steeringLevel - 1);
        }

        public void Launch(Vector3 force)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(force, ForceMode.Impulse);
            Debug.Log($"[PlayerMotor] Launched with force: {force.magnitude:F1}");
        }

        public void Steer(float horizontalInput, float deltaTime)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            Vector3 steerForce = Vector3.right * (horizontalInput * _steeringSpeed);
            _rigidbody.AddForce(steerForce, ForceMode.Acceleration);

            // Clamp lateral velocity to prevent excessive sideways drift
            Vector3 vel = _rigidbody.linearVelocity;
            float lateralSpeed = Mathf.Abs(vel.x);
            float maxLateral = _steeringSpeed * 0.8f;

            if (lateralSpeed > maxLateral)
            {
                vel.x = Mathf.Sign(vel.x) * maxLateral;
                _rigidbody.linearVelocity = vel;
            }
        }

        public void ApplyDownhillForce(float deltaTime)
        {
            if (CurrentSpeed >= _maxSpeed) return;

            // Continuous forward + downward force simulating gravity on a slope
            Vector3 downhillForce = Vector3.forward * _settings.DownhillAcceleration;
            _rigidbody.AddForce(downhillForce, ForceMode.Acceleration);
        }

        public void ApplyDrag(float deltaTime)
        {
            // Speed-based drag to create natural deceleration on flat/uphill sections
            Vector3 vel = _rigidbody.linearVelocity;
            float forwardSpeed = vel.z;

            if (forwardSpeed > 0f)
            {
                float dragForce = _settings.DragCoefficient * forwardSpeed * forwardSpeed * 0.001f;
                Vector3 drag = -vel.normalized * dragForce;
                _rigidbody.AddForce(drag, ForceMode.Acceleration);
            }

            // Hard clamp max speed
            if (CurrentSpeed > _maxSpeed)
            {
                _rigidbody.linearVelocity = vel.normalized * _maxSpeed;
            }
        }

        public void Halt()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;
        }
    }
}
