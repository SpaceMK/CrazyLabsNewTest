using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Input handler supporting both mouse and touch drag for the slingshot,
    /// plus keyboard/touch steering during the run.
    /// 
    /// Drag mechanic:
    /// - Mouse: left-click hold and drag. Release to launch.
    /// - Touch: finger down and drag. Lift to launch.
    /// 
    /// DragDelta is the screen-space offset from the initial press position.
    /// X axis = horizontal angle, Y axis (negative = pull back) = force.
    /// 
    /// LSP: fully interchangeable with any IInputHandler via DI.
    /// </summary>
    public sealed class SimpleInputHandler : IInputHandler
    {
        private bool _enabled;

        private bool _isDragging;
        private bool _dragStartedThisFrame;
        private bool _dragEndedThisFrame;
        private Vector2 _dragStartScreenPos;
        private Vector2 _currentScreenPos;
        private Vector2 _lastDragDelta;

        // Cache previous frame state to detect edges
        private bool _wasPressing;

        // === Steering ===
        public float HorizontalInput => _enabled ? Input.GetAxis("Horizontal") : 0f;

        // === Drag ===
        public bool DragStarted => _enabled && _dragStartedThisFrame;
        public bool IsDragging => _enabled && _isDragging;
        public bool DragEnded => _enabled && _dragEndedThisFrame;

        /// <summary>
        /// Returns the drag delta while dragging, AND on the release frame
        /// (so the slingshot can read the final pull values before launching).
        /// Resets to zero only on the next frame after release.
        /// </summary>
        public Vector2 DragDelta => (_isDragging || _dragEndedThisFrame) ? _lastDragDelta : Vector2.zero;

        public void Enable() => _enabled = true;

        public void Disable()
        {
            _enabled = false;
            _isDragging = false;
            _dragStartedThisFrame = false;
            _dragEndedThisFrame = false;
            _wasPressing = false;
        }

        /// <summary>
        /// Must be called every frame (from a MonoBehaviour or ITickable).
        /// Reads mouse/touch state and updates drag tracking.
        /// </summary>
        public void Tick()
        {
            _dragStartedThisFrame = false;
            _dragEndedThisFrame = false;

            if (!_enabled) return;

            bool isPressing = GetPointerPressed();
            Vector2 pointerPos = GetPointerPosition();

            // Drag start: wasn't pressing last frame, pressing now
            if (isPressing && !_wasPressing)
            {
                _isDragging = true;
                _dragStartedThisFrame = true;
                _dragStartScreenPos = pointerPos;
                _currentScreenPos = pointerPos;
                _lastDragDelta = Vector2.zero;
            }
            // Drag continue: still pressing
            else if (isPressing && _isDragging)
            {
                _currentScreenPos = pointerPos;
                _lastDragDelta = _currentScreenPos - _dragStartScreenPos;
            }
            // Drag end: was pressing, released now
            // _lastDragDelta intentionally NOT cleared here —
            // it stays valid so DragDelta returns the final pull on the release frame.
            else if (!isPressing && _wasPressing && _isDragging)
            {
                _dragEndedThisFrame = true;
                _isDragging = false;
            }

            _wasPressing = isPressing;
        }

        private bool GetPointerPressed()
        {
            // Touch takes priority over mouse
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).phase != TouchPhase.Ended
                    && Input.GetTouch(0).phase != TouchPhase.Canceled;
            }

            return Input.GetMouseButton(0);
        }

        private Vector2 GetPointerPosition()
        {
            if (Input.touchCount > 0)
            {
                return Input.GetTouch(0).position;
            }

            return (Vector2)Input.mousePosition;
        }
    }
}