using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Slingshot
{
    /// <summary>
    /// Visual representation of the slingshot in the scene.
    /// Place this on a GameObject near the player's starting position.
    /// 
    /// Subscribes to ISlingshot events and moves child transforms to show:
    /// - The pull-back position (how far the slingshot band is stretched).
    /// - The aim direction (a line or arrow showing where the sled will launch).
    /// 
    /// Optional: Assign a LineRenderer to draw the slingshot band.
    /// 
    /// This is purely visual — it reads from the slingshot state,
    /// it does not control it. Input flows through IInputHandler → RunSession → ISlingshot.
    /// </summary>
    public class SlingshotVisual : MonoBehaviour
    {
        [Header("References")]
        [Tooltip("The point where the band attaches on the left fork.")]
        [SerializeField] private Transform _leftAnchor;
        [Tooltip("The point where the band attaches on the right fork.")]
        [SerializeField] private Transform _rightAnchor;
        [Tooltip("The handle/pouch the player pulls back. Moves during drag.")]
        [SerializeField] private Transform _pouch;
        [Tooltip("Optional: aim indicator that points in the launch direction.")]
        [SerializeField] private Transform _aimIndicator;

        [Header("Band Visuals")]
        [Tooltip("Optional LineRenderer for drawing the elastic band.")]
        [SerializeField] private LineRenderer _bandRenderer;

        [Header("Tuning")]
        [Tooltip("Maximum world-space distance the pouch moves backward on full pull.")]
        [SerializeField] private float _maxPouchPullDistance = 2f;
        [Tooltip("Maximum world-space distance the pouch moves sideways on full angle.")]
        [SerializeField] private float _maxPouchSideOffset = 1f;

        private ISlingshot _slingshot;
        private Vector3 _pouchRestPosition;
        private bool _isInitialized;

        [Inject]
        public void Construct(PlayerManager playerManager)
        {
            _slingshot = playerManager.Slingshot;
        }

        private void Start()
        {
            if (_slingshot == null)
            {
                Debug.LogWarning("[SlingshotVisual] No slingshot reference. Visual disabled.");
                return;
            }

            if (_pouch != null)
                _pouchRestPosition = _pouch.localPosition;

            _slingshot.OnDragStarted += HandleDragStarted;
            _slingshot.OnDragUpdated += HandleDragUpdated;
            _slingshot.OnReleased += HandleReleased;

            // Start hidden or at rest
            SetPouchAtRest();
            SetAimVisible(false);

            _isInitialized = true;
            Debug.Log("[SlingshotVisual] Initialized.");
        }

        private void OnDestroy()
        {
            if (_slingshot != null)
            {
                _slingshot.OnDragStarted -= HandleDragStarted;
                _slingshot.OnDragUpdated -= HandleDragUpdated;
                _slingshot.OnReleased -= HandleReleased;
            }
        }

        private void HandleDragStarted()
        {
            SetAimVisible(true);
        }

        private void HandleDragUpdated(float pullPercent, float angleDegrees)
        {
            UpdatePouchPosition(pullPercent, angleDegrees);
            UpdateBand();
            UpdateAimIndicator(pullPercent, angleDegrees);
        }

        private void HandleReleased(Vector3 force)
        {
            SetPouchAtRest();
            UpdateBand();
            SetAimVisible(false);
        }

        private void UpdatePouchPosition(float pullPercent, float angleDegrees)
        {
            if (_pouch == null) return;

            // Pull back along local -Z (backward), offset along local X (sideways)
            // Inverted angle: if slingshot angle is positive (aiming right),
            // the pouch moves left (opposite direction, like pulling a real slingshot).
            float pullBack = pullPercent * _maxPouchPullDistance;
            float sideOffset = (angleDegrees / 35f) * _maxPouchSideOffset;

            _pouch.localPosition = _pouchRestPosition
                + Vector3.back * pullBack
                + Vector3.left * sideOffset;
        }

        private void UpdateBand()
        {
            if (_bandRenderer == null || _leftAnchor == null || _rightAnchor == null || _pouch == null)
                return;

            // Band goes: left anchor → pouch → right anchor
            _bandRenderer.positionCount = 3;
            _bandRenderer.SetPosition(0, _leftAnchor.position);
            _bandRenderer.SetPosition(1, _pouch.position);
            _bandRenderer.SetPosition(2, _rightAnchor.position);
        }

        private void UpdateAimIndicator(float pullPercent, float angleDegrees)
        {
            if (_aimIndicator == null) return;

            // Point the aim indicator in the launch direction
            Vector3 aimDir = Quaternion.Euler(0f, angleDegrees, 0f) * Vector3.forward;
            _aimIndicator.rotation = Quaternion.LookRotation(aimDir, Vector3.up);

            // Scale by pull to show power visually
            float scale = Mathf.Lerp(0.2f, 1f, pullPercent);
            _aimIndicator.localScale = new Vector3(scale, scale, scale * 2f);
        }

        private void SetPouchAtRest()
        {
            if (_pouch != null)
                _pouch.localPosition = _pouchRestPosition;
        }

        private void SetAimVisible(bool visible)
        {
            if (_aimIndicator != null)
                _aimIndicator.gameObject.SetActive(visible);
        }

        /// <summary>
        /// Call on player reset to snap visuals back to rest.
        /// </summary>
        public void ResetVisual()
        {
            SetPouchAtRest();
            UpdateBand();
            SetAimVisible(false);
        }
    }
}
