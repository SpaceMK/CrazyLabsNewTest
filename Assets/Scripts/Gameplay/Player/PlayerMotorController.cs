using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    public sealed class PlayerMotorController : IPlayerMotor
    {
        private readonly Rigidbody _rigidbody;
        private PlayerMotorConfig _config;
        private float _lastReportedSpeed;

        public Vector3 Velocity => _rigidbody.linearVelocity;
        public float CurrentSpeed => _rigidbody.linearVelocity.magnitude;
        public bool IsMoving => CurrentSpeed > 0.5f;

        public event Action<float> OnSpeedChanged;
        public event Action OnLaunched;
        public event Action OnHalted;

        public PlayerMotorController(Rigidbody rigidbody, PlayerMotorConfig config)
        {
            _rigidbody = rigidbody;
            _config = config;
        }

        public void UpdateConfig(PlayerMotorConfig config)
        {
            _config = config;
        }

        public void Launch(Vector3 force)
        {
            _rigidbody.isKinematic = false;
            _rigidbody.AddForce(force, ForceMode.Impulse);

            Debug.Log($"[PlayerMotorController] Launched with force: {force.magnitude:F1}");
            OnLaunched?.Invoke();
        }

        public void Steer(float horizontalInput, float deltaTime)
        {
            if (Mathf.Abs(horizontalInput) < 0.01f) return;

            Vector3 steerForce = Vector3.right * (horizontalInput * _config.SteeringSpeed);
            _rigidbody.AddForce(steerForce, ForceMode.Acceleration);

            Vector3 vel = _rigidbody.linearVelocity;
            float lateralSpeed = Mathf.Abs(vel.x);
            float maxLateral = _config.SteeringSpeed * 0.8f;

            if (lateralSpeed > maxLateral)
            {
                vel.x = Mathf.Sign(vel.x) * maxLateral;
                _rigidbody.linearVelocity = vel;
            }
        }

        public void ApplyDownhillForce(float deltaTime)
        {
            if (CurrentSpeed >= _config.MaxSpeed) return;

            Vector3 downhillForce = Vector3.forward * _config.DownhillAcceleration;
            _rigidbody.AddForce(downhillForce, ForceMode.Acceleration);
        }

        public void ApplyDrag(float deltaTime)
        {
            Vector3 vel = _rigidbody.linearVelocity;
            float forwardSpeed = vel.z;

            if (forwardSpeed > 0f)
            {
                float dragForce = _config.DragCoefficient * forwardSpeed * forwardSpeed * 0.001f;
                Vector3 drag = -vel.normalized * dragForce;
                _rigidbody.AddForce(drag, ForceMode.Acceleration);
            }

            if (CurrentSpeed > _config.MaxSpeed)
            {
                _rigidbody.linearVelocity = vel.normalized * _config.MaxSpeed;
            }

            float speed = CurrentSpeed;
            if (Mathf.Abs(speed - _lastReportedSpeed) > 0.1f)
            {
                _lastReportedSpeed = speed;
                OnSpeedChanged?.Invoke(speed);
            }
        }

        public void Halt()
        {
            _rigidbody.linearVelocity = Vector3.zero;
            _rigidbody.angularVelocity = Vector3.zero;
            _rigidbody.isKinematic = true;

            _lastReportedSpeed = 0f;
            OnSpeedChanged?.Invoke(0f);
            OnHalted?.Invoke();
        }
    }
}