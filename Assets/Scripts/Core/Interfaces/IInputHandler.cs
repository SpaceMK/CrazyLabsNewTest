namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for player input.
    /// DIP - gameplay code depends on this abstraction, not on UnityEngine.Input directly.
    /// Enables testability and input remapping.
    /// </summary>
    public interface IInputHandler
    {
        /// <summary>
        /// Horizontal steering value, normalized to [-1, 1].
        /// </summary>
        float HorizontalInput { get; }
        
        /// <summary>
        /// True during the frame the launch/action button is pressed.
        /// </summary>
        bool LaunchPressed { get; }
        
        void Enable();
        void Disable();
    }
}
