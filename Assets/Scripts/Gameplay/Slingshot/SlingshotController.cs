using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Slingshot
{
    /// <summary>
    /// Controller that owns all slingshot drag-and-release behavior.
    /// Reads tuning values from SlingshotConfig (pure data).
    /// 
    /// SRP: only slingshot mechanics, no config computation, no input reading.
    /// </summary>
    public class SlingshotController : ISlingshot
    {
        private SlingshotConfig _config;

        private bool _isDragging;
        private Vector2 _currentDragDelta;

        public bool IsReady { get; private set; } = true;
        public bool IsDragging => _isDragging;
        public float PullPercent { get; private set; }
        public float LaunchAngle { get; private set; }

        public event Action OnDragStarted;
        public event Action<float, float> OnDragUpdated;
        public event Action<Vector3> OnReleased;

        public SlingshotController(SlingshotConfig config)
        {
            _config = config;
        }

        public void UpdateConfig(SlingshotConfig config)
        {
            _config = config;
        }

        public void StartDrag()
        {
            if (!IsReady) return;

            _isDragging = true;
            _currentDragDelta = Vector2.zero;
            PullPercent = 0f;
            LaunchAngle = 0f;
            IsReady = false;

            Debug.Log("[Slingshot] Drag started.");
            OnDragStarted?.Invoke();
        }

        public void UpdateDrag(Vector2 dragDelta)
        {
            if (!_isDragging) return;

            _currentDragDelta = dragDelta;

            float rawPull = -dragDelta.y;
            PullPercent = Mathf.Clamp01(rawPull / _config.MaxPullPixels);

            float rawAngle = -dragDelta.x;
            LaunchAngle = Mathf.Clamp(
                (rawAngle / _config.MaxAnglePixels) * _config.MaxAngleDegrees,
                -_config.MaxAngleDegrees,
                _config.MaxAngleDegrees
            );

            OnDragUpdated?.Invoke(PullPercent, LaunchAngle);
        }

        public Vector3 Release()
        {
            if (!_isDragging) return Vector3.zero;

            _isDragging = false;

            if (PullPercent < _config.MinPullThreshold)
            {
                Debug.Log("[Slingshot] Released with minimal pull — cancelled.");
                Reset();
                return Vector3.zero;
            }

            float finalMagnitude = _config.LaunchForce * PullPercent;

            Vector3 direction = Quaternion.Euler(0f, LaunchAngle, 0f)
                * (Vector3.forward + Vector3.up * _config.UpwardArc).normalized;

            Vector3 force = direction * finalMagnitude;

            Debug.Log($"[Slingshot] Released! Power: {PullPercent:P0}, Angle: {LaunchAngle:F1}, Force: {finalMagnitude:F1}");
            OnReleased?.Invoke(force);

            return force;
        }

        public void Cancel()
        {
            if (!_isDragging) return;

            _isDragging = false;
            PullPercent = 0f;
            LaunchAngle = 0f;
            IsReady = true;

            Debug.Log("[Slingshot] Drag cancelled.");
        }

        public void Reset()
        {
            _isDragging = false;
            _currentDragDelta = Vector2.zero;
            PullPercent = 0f;
            LaunchAngle = 0f;
            IsReady = true;
        }
    }
}