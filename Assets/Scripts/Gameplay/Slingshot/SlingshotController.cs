using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;

namespace SledSurfers.Gameplay.Slingshot
{
    /// <summary>
    /// Drag-based slingshot that converts a 2D screen drag into a 3D launch force.
    /// 
    /// Drag mapping:
    /// - Drag Y (pull down on screen = pull slingshot back) → launch power.
    ///   Pulling down in screen space means the player is "pulling back" the slingshot.
    ///   The further they pull, the more force (clamped to maxPullPixels).
    /// 
    /// - Drag X (horizontal offset) → launch angle.
    ///   Dragging left aims the launch slightly right, dragging right aims left
    ///   (inverted, like a real slingshot — you pull opposite to where you aim).
    /// 
    /// The final force vector combines:
    ///   forward (always) + up (slight arc) + right (from angle).
    /// Magnitude is scaled by pull percent and the player's launch power level.
    /// 
    /// Trade-off: Using screen-space pixels for drag distance means the feel varies
    /// slightly across screen resolutions. This is mitigated by normalizing against
    /// maxPullPixels which can be tuned per platform, or switched to viewport-relative
    /// units in the future.
    /// </summary>
    public class SlingshotController : ISlingshot
    {
        private GameSettings _settings;
        private int _launchPowerLevel;

        private bool _isDragging;
        private Vector2 _currentDragDelta;

        /// <summary>
        /// Screen pixels of downward drag required for maximum power.
        /// Tune per platform: ~200 for mobile, ~300 for desktop.
        /// </summary>
        private readonly float _maxPullPixels;

        /// <summary>
        /// Maximum launch angle in degrees. Drag fully left/right = this angle.
        /// </summary>
        private readonly float _maxAngleDegrees;

        /// <summary>
        /// Screen pixels of horizontal drag required for maximum angle.
        /// </summary>
        private readonly float _maxAnglePixels;

        public bool IsReady { get; private set; } = true;
        public bool IsDragging => _isDragging;
        public float PullPercent { get; private set; }
        public float LaunchAngle { get; private set; }

        public event Action OnDragStarted;
        public event Action<float, float> OnDragUpdated;
        public event Action<Vector3> OnReleased;

        public SlingshotController(
            GameSettings settings,
            int launchPowerLevel,
            float maxPullPixels = 250f,
            float maxAngleDegrees = 35f,
            float maxAnglePixels = 200f)
        {
            _settings = settings;
            _launchPowerLevel = launchPowerLevel;
            _maxPullPixels = maxPullPixels;
            _maxAngleDegrees = maxAngleDegrees;
            _maxAnglePixels = maxAnglePixels;
        }

        public void UpdateStats(GameSettings settings, int launchPowerLevel)
        {
            _settings = settings;
            _launchPowerLevel = launchPowerLevel;
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

        /// <summary>
        /// Called every frame while dragging with the screen-space delta
        /// from the drag start position to the current pointer position.
        /// </summary>
        public void UpdateDrag(Vector2 dragDelta)
        {
            if (!_isDragging) return;

            _currentDragDelta = dragDelta;

            // Pull power: negative Y = pulling down on screen = pulling back the slingshot.
            // Clamp to [0, 1].
            float rawPull = -dragDelta.y;
            PullPercent = Mathf.Clamp01(rawPull / _maxPullPixels);

            // Angle: horizontal drag. Inverted — drag left to aim right (slingshot feel).
            // Clamp to [-maxAngle, +maxAngle].
            float rawAngle = -dragDelta.x;
            LaunchAngle = Mathf.Clamp(
                (rawAngle / _maxAnglePixels) * _maxAngleDegrees,
                -_maxAngleDegrees,
                _maxAngleDegrees
            );

            OnDragUpdated?.Invoke(PullPercent, LaunchAngle);
        }

        public Vector3 Release()
        {
            if (!_isDragging) return Vector3.zero;

            _isDragging = false;

            // If the player barely pulled, treat as a cancel
            if (PullPercent < 0.1f)
            {
                Debug.Log("[Slingshot] Released with minimal pull — cancelled.");
                Reset();
                return Vector3.zero;
            }

            // Calculate force magnitude from pull + upgrade level
            float baseForceMagnitude = _settings.BaseLaunchForce
                + _settings.LaunchForcePerLevel * (_launchPowerLevel - 1);
            float finalMagnitude = baseForceMagnitude * PullPercent;

            // Build direction: forward + slight up arc + angle
            Vector3 direction = Quaternion.Euler(0f, LaunchAngle, 0f)
                * (Vector3.forward + Vector3.up * 0.3f).normalized;

            Vector3 force = direction * finalMagnitude;

            Debug.Log($"[Slingshot] Released! Power: {PullPercent:P0}, Angle: {LaunchAngle:F1}°, Force: {finalMagnitude:F1}");
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