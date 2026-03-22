using SledSurfers.Core.Interfaces;
using UnityEngine;
using UnityEngine.InputSystem;

namespace SledSurfers.Gameplay.Player
{
    public sealed class InputHandler : IInputHandler
    {
        private readonly PlayerInputActions _actions;
        private bool _enabled;

        private bool _isDragging;
        private bool _dragStartedThisFrame;
        private bool _dragEndedThisFrame;
        private Vector2 _dragStartScreenPos;
        private Vector2 _lastDragDelta;
        private bool _wasPressed;

        // === Steering ===
        public float HorizontalInput => _enabled ? _actions.Gameplay.Steer.ReadValue<float>() : 0f;

        // === Drag ===
        public bool DragStarted => _enabled && _dragStartedThisFrame;
        public bool IsDragging => _enabled && _isDragging;
        public bool DragEnded => _enabled && _dragEndedThisFrame;
        public Vector2 DragDelta => (_isDragging || _dragEndedThisFrame) ? _lastDragDelta : Vector2.zero;

        public InputHandler()
        {
            _actions = new PlayerInputActions();
        }

        public void Enable()
        {
            _actions.Gameplay.Enable();
            _enabled = true;
        }

        public void Disable()
        {
            _actions.Gameplay.Disable();
            _enabled = false;
            _isDragging = false;
            _dragStartedThisFrame = false;
            _dragEndedThisFrame = false;
            _wasPressed = false;
        }

        public void Tick()
        {
            _dragStartedThisFrame = false;
            _dragEndedThisFrame = false;

            if (!_enabled) return;

            bool isPressed = Mouse.current?.leftButton.isPressed == true
                          || (Touchscreen.current?.primaryTouch.press.isPressed == true);

            Vector2 pointerPos = GetPointerPosition();

            if (isPressed && !_wasPressed)
            {
                _isDragging = true;
                _dragStartedThisFrame = true;
                _dragStartScreenPos = pointerPos;
                _lastDragDelta = Vector2.zero;
            }
            else if (isPressed && _isDragging)
            {
                _lastDragDelta = pointerPos - _dragStartScreenPos;
            }
            else if (!isPressed && _wasPressed && _isDragging)
            {
                _dragEndedThisFrame = true;
                _isDragging = false;
            }

            _wasPressed = isPressed;
        }

        private Vector2 GetPointerPosition()
        {
            if (Touchscreen.current?.primaryTouch.press.isPressed == true)
            {
                return Touchscreen.current.primaryTouch.position.ReadValue();
            }

            if (Mouse.current != null)
            {
                return Mouse.current.position.ReadValue();
            }

            return Vector2.zero;
        }
    }
}