namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for level element managers (coins, obstacles, etc.).
    /// Allows GameplayFlow to reset level state without knowing concrete manager types.
    /// </summary>
    public interface ILevelManager
    {
        void Reset();
    }
}
