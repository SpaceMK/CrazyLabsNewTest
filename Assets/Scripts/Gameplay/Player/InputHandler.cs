using SledSurfers.Core.Interfaces;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Concrete input handler using Unity Input System.
    /// DIP - gameplay depends on IInputHandler, this is the concrete wired by DI.
    /// Can be swapped for AI/replay input without touching gameplay code.
    /// </summary>
    public sealed class InputHandler : IInputHandler
    {
        private readonly PlayerInputActions _actions;
        private bool _enabled;

        public float HorizontalInput => _enabled ? _actions.Gameplay.Steer.ReadValue<float>() : 0f;
        public bool LaunchPressed => _enabled && _actions.Gameplay.Launch.WasPressedThisFrame();
        public bool LaunchReleased => _enabled && _actions.Gameplay.Launch.WasReleasedThisFrame();

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
        }
    }
}
