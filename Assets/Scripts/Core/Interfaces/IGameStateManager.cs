using System;

namespace SledSurfers.Core.Interfaces
{
    public enum GameState
    {
        None,
        Loading,
        StartMenu,
        Playing,
        Paused,
        GameOver
    }

    public interface IGameStateManager
    {
        GameState CurrentState { get; }
        GameStateData CurrentData { get; }

        void TransitionTo(GameState newState, GameStateData data = null);

        event Action<GameState, GameState, GameStateData> OnStateChanged;
    }

    public class GameStateData
    {
        public float Distance { get; set; }
        public int CoinsCollected { get; set; }
    }
}