namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for handling collision outcomes.
    /// ISP - separated from IPlayerMotor since collision response logic != movement physics.
    /// Consumers subscribe to events without knowing collision detection details.
    /// </summary>
    public interface ICollisionHandler
    {
        event System.Action OnCrash;
        event System.Action<int> OnCoinCollected;
    }
}
