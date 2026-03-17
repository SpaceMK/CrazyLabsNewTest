using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay.Player;
using UnityEngine;

namespace SledSurfers.Gameplay.Camera
{
    /// <summary>
    /// Third-person follow camera mimicking Sled Surfer style.
    /// 
    /// Self-contained - subscribes directly to IPlayerMotor.OnSpeedChanged.
    /// No need for GameplayFlow to feed it data.
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private PlayerManager _playerManager;

        [Header("Position Settings")]
        [Tooltip("Height above the player")]
        [SerializeField] private float _height = 8f;

        [Tooltip("Distance behind the player")]
        [SerializeField] private float _distance = 5f;

        [Tooltip("How far ahead of the player to look")]
        [SerializeField] private float _lookAheadDistance = 10f;

        [Header("Smoothing")]
        [Tooltip("How quickly the camera follows position")]
        [SerializeField] private float _followSpeed = 8f;

        [Tooltip("How quickly the camera rotates to follow")]
        [SerializeField] private float _rotationSpeed = 5f;

        [Header("Dynamic Adjustments")]
        [Tooltip("Extra height added at max speed")]
        [SerializeField] private float _speedHeightBonus = 2f;

        [Tooltip("Extra distance added at max speed")]
        [SerializeField] private float _speedDistanceBonus = 2f;

        [Tooltip("Speed value considered 'max' for dynamic adjustments")]
        [SerializeField] private float _maxSpeedReference = 30f;

        [Header("Tilt Settings")]
        [Tooltip("Base downward angle in degrees")]
        [SerializeField] private float _basePitch = 25f;

        [Tooltip("Additional pitch when going fast")]
        [SerializeField] private float _speedPitchBonus = 5f;

        [Header("Horizontal Follow")]
        [Tooltip("How much the camera follows horizontal movement (0 = none, 1 = full)")]
        [SerializeField][Range(0f, 1f)] private float _horizontalFollow = 0.3f;

        [Tooltip("Smoothing for horizontal movement")]
        [SerializeField] private float _horizontalSmoothSpeed = 4f;

        // Runtime state
        private Transform _target;
        private IPlayerMotor _motor;
        private Vector3 _currentVelocity;
        private float _smoothedHorizontalOffset;
        private float _currentSpeed;
        private bool _isInitialized;

        /// <summary>
        /// Initialize with player reference. Call after PlayerManager.Initialize().
        /// </summary>
        public void Start()
        {
            
            _target = _playerManager.transform;
            _motor = _playerManager.Motor;

            // Subscribe to motor speed changes - self-contained, no middleman
            _motor.OnSpeedChanged += HandleSpeedChanged;
            _motor.OnHalted += HandleHalted;

            // Snap to initial position
            transform.position = CalculateDesiredPosition(0f);
            transform.rotation = CalculateDesiredRotation();
            _isInitialized = true;

            Debug.Log("[PlayerCameraController] Initialized and subscribed to motor events.");
        }

    
        private void LateUpdate()
        {
            if (_target == null || !_isInitialized) return;

            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        private void OnDestroy()
        {
            // Unsubscribe from events
            if (_motor != null)
            {
                _motor.OnSpeedChanged -= HandleSpeedChanged;
                _motor.OnHalted -= HandleHalted;
            }
        }

        private void HandleSpeedChanged(float speed)
        {
            _currentSpeed = speed;
        }

        private void HandleHalted()
        {
            _currentSpeed = 0f;
        }

        /// <summary>
        /// Instantly snap camera to target position (use on respawn/reset).
        /// </summary>
        public void SnapToTarget()
        {
            if (_target == null) return;

            _smoothedHorizontalOffset = 0f;
            _currentSpeed = 0f;
            transform.position = CalculateDesiredPosition(0f);
            transform.rotation = CalculateDesiredRotation();
        }

        private void UpdateCameraPosition()
        {
            float targetHorizontalOffset = _target.position.x * _horizontalFollow;
            _smoothedHorizontalOffset = Mathf.Lerp(
                _smoothedHorizontalOffset,
                targetHorizontalOffset,
                _horizontalSmoothSpeed * Time.deltaTime
            );

            Vector3 desiredPosition = CalculateDesiredPosition(_smoothedHorizontalOffset);

            transform.position = Vector3.SmoothDamp(
                transform.position,
                desiredPosition,
                ref _currentVelocity,
                1f / _followSpeed
            );
        }

        private void UpdateCameraRotation()
        {
            Quaternion desiredRotation = CalculateDesiredRotation();

            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        private Vector3 CalculateDesiredPosition(float horizontalOffset)
        {
            float speedFactor = Mathf.Clamp01(_currentSpeed / _maxSpeedReference);

            float dynamicHeight = _height + (_speedHeightBonus * speedFactor);
            float dynamicDistance = _distance + (_speedDistanceBonus * speedFactor);

            Vector3 targetPos = _target.position;

            return new Vector3(
                targetPos.x * _horizontalFollow + horizontalOffset * (1f - _horizontalFollow),
                targetPos.y + dynamicHeight,
                targetPos.z - dynamicDistance
            );
        }

        private Quaternion CalculateDesiredRotation()
        {
            if (_target == null) return transform.rotation;

            float speedFactor = Mathf.Clamp01(_currentSpeed / _maxSpeedReference);
            float dynamicPitch = _basePitch + (_speedPitchBonus * speedFactor);

            Vector3 lookAtPoint = _target.position + Vector3.forward * _lookAheadDistance;
            Vector3 direction = lookAtPoint - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            Vector3 euler = lookRotation.eulerAngles;
            euler.x = dynamicPitch;
            euler.z = 0f;

            return Quaternion.Euler(euler);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            Transform target = _target;
            if (target == null && _playerManager != null)
                target = _playerManager.transform;
            if (target == null) return;

            Vector3 desiredPos = new Vector3(
                target.position.x * _horizontalFollow,
                target.position.y + _height,
                target.position.z - _distance
            );

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(desiredPos, 0.5f);
            Gizmos.DrawLine(target.position, desiredPos);

            Vector3 lookAt = target.position + Vector3.forward * _lookAheadDistance;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lookAt, 0.3f);
            Gizmos.DrawLine(desiredPos, lookAt);
        }
#endif
    }
}
