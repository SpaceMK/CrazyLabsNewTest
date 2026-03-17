namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Manages high-level game state transitions.
    /// SRP - only responsible for state, not scene loading or UI.
    /// </summary>
    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        void TransitionTo(GameState newState);
        
        event System.Action<GameState, GameState> OnStateChanged;
    }

    public enum GameState
    {
        Bootstrap,
        MainMenu,
        Gameplay,
        GameOver,
        Upgrade
    }
}
