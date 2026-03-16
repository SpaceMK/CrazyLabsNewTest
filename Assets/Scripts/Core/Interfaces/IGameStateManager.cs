namespace SledSurfers.Core.Interfaces
{
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
