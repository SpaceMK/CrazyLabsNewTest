namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for player input.
    /// Allows swapping between real input, AI input, or replay input.
    /// </summary>
    public interface IInputHandler
    {
        float HorizontalInput { get; }
        bool LaunchPressed { get; }
        bool LaunchReleased { get; }

        void Enable();
        void Disable();
    }
}
