using UnityEngine;

namespace SledSurfers.Gameplay.Camera
{
    /// <summary>
    /// Third-person follow camera mimicking Sled Surfer style.
    /// Positioned behind and above the player, looking down at the slope.
    /// Smooth follow with slight lag for natural feel.
    /// </summary>
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Target")]
        [SerializeField] private Transform _target;

        [Header("Position Settings")]
        [Tooltip("Height above the player")]
        [SerializeField] private float _height = 8f;

        [Tooltip("Distance behind the player")]
        [SerializeField] private float _distance = 5f;

        [Tooltip("How far ahead of the player to look")]
        [SerializeField] private float _lookAheadDistance = 10f;

        [Header("Smoothing")]
        [Tooltip("How quickly the camera follows position (lower = smoother)")]
        [SerializeField] private float _followSpeed = 8f;

        [Tooltip("How quickly the camera rotates to follow (lower = smoother)")]
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
        private Vector3 _currentVelocity;
        private float _smoothedHorizontalOffset;
        private float _currentSpeed;
        private bool _isInitialized;

        private void Start()
        {
            if (_target == null)
            {
                Debug.LogWarning("[PlayerCameraController] No target assigned. Searching for PlayerManager...");
                var player = FindFirstObjectByType<Player.PlayerManager>();
                if (player != null)
                {
                    _target = player.transform;
                }
            }

            if (_target != null)
            {
                // Snap to initial position
                transform.position = CalculateDesiredPosition(0f);
                transform.rotation = CalculateDesiredRotation();
                _isInitialized = true;
            }
        }

        private void LateUpdate()
        {
            if (_target == null || !_isInitialized) return;

            UpdateCameraPosition();
            UpdateCameraRotation();
        }

        /// <summary>
        /// Call this from PlayerManager or GameplayFlow to update speed for dynamic camera.
        /// </summary>
        public void UpdateSpeed(float speed)
        {
            _currentSpeed = speed;
        }

        /// <summary>
        /// Set the target to follow (call from GameplayFlow or DI setup).
        /// </summary>
        public void SetTarget(Transform target)
        {
            _target = target;

            if (_target != null && !_isInitialized)
            {
                transform.position = CalculateDesiredPosition(0f);
                transform.rotation = CalculateDesiredRotation();
                _isInitialized = true;
            }
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
            // Smooth horizontal offset based on player's X position
            float targetHorizontalOffset = _target.position.x * _horizontalFollow;
            _smoothedHorizontalOffset = Mathf.Lerp(
                _smoothedHorizontalOffset,
                targetHorizontalOffset,
                _horizontalSmoothSpeed * Time.deltaTime
            );

            Vector3 desiredPosition = CalculateDesiredPosition(_smoothedHorizontalOffset);

            // Smooth follow using SmoothDamp for natural feel
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

            // Smooth rotation
            transform.rotation = Quaternion.Slerp(
                transform.rotation,
                desiredRotation,
                _rotationSpeed * Time.deltaTime
            );
        }

        private Vector3 CalculateDesiredPosition(float horizontalOffset)
        {
            // Calculate speed factor (0 to 1)
            float speedFactor = Mathf.Clamp01(_currentSpeed / _maxSpeedReference);

            // Dynamic height and distance based on speed
            float dynamicHeight = _height + (_speedHeightBonus * speedFactor);
            float dynamicDistance = _distance + (_speedDistanceBonus * speedFactor);

            // Position behind and above the player
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

            // Calculate speed factor for dynamic pitch
            float speedFactor = Mathf.Clamp01(_currentSpeed / _maxSpeedReference);
            float dynamicPitch = _basePitch + (_speedPitchBonus * speedFactor);

            // Look at a point ahead of the player
            Vector3 lookAtPoint = _target.position + Vector3.forward * _lookAheadDistance;

            // Calculate direction and apply pitch
            Vector3 direction = lookAtPoint - transform.position;
            Quaternion lookRotation = Quaternion.LookRotation(direction);

            // Override pitch to maintain consistent downward angle
            Vector3 euler = lookRotation.eulerAngles;
            euler.x = dynamicPitch;
            euler.z = 0f; // No roll

            return Quaternion.Euler(euler);
        }

#if UNITY_EDITOR
        private void OnDrawGizmosSelected()
        {
            if (_target == null) return;

            // Draw camera position
            Vector3 desiredPos = CalculateDesiredPosition(0f);
            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(desiredPos, 0.5f);
            Gizmos.DrawLine(_target.position, desiredPos);

            // Draw look-at point
            Vector3 lookAt = _target.position + Vector3.forward * _lookAheadDistance;
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(lookAt, 0.3f);
            Gizmos.DrawLine(desiredPos, lookAt);
        }
#endif
    }
}