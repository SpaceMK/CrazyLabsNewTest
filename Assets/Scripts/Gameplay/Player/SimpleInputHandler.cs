using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Simple input handler using legacy Input.GetAxis for quick testing.
    /// Swap to InputHandler (New Input System) via DI when InputActions are configured.
    /// LSP - fully interchangeable with InputHandler through IInputHandler.
    /// </summary>
    public sealed class SimpleInputHandler : IInputHandler
    {
        private bool _enabled;

        public float HorizontalInput => _enabled ? Input.GetAxis("Horizontal") : 0f;
        public bool LaunchPressed => _enabled && Input.GetKeyDown(KeyCode.Space);

        public void Enable() => _enabled = true;
        public void Disable() => _enabled = false;
    }
}
