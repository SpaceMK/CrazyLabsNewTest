using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    
    public sealed class GameStateManager : IGameStateManager
    {
        public GameState CurrentState { get; private set; } = GameState.Bootstrap;
        
        public event Action<GameState, GameState> OnStateChanged;

        public void TransitionTo(GameState newState)
        {
            if (CurrentState == newState)
            {
                Debug.LogWarning($"[GameState] Already in state {newState}, ignoring transition.");
                return;
            }

            var previousState = CurrentState;
            
            if (!IsValidTransition(previousState, newState))
            {
                Debug.LogError($"[GameState] Invalid transition: {previousState} -> {newState}");
                return;
            }

            Debug.Log($"[GameState] {previousState} -> {newState}");
            CurrentState = newState;
            OnStateChanged?.Invoke(previousState, newState);
        }

        private bool IsValidTransition(GameState from, GameState to)
        {
            return (from, to) switch
            {
                (GameState.Bootstrap, GameState.Gameplay) => true,
                (GameState.Bootstrap, GameState.MainMenu) => true,
                (GameState.MainMenu, GameState.Gameplay) => true,
                (GameState.Gameplay, GameState.GameOver) => true,
                (GameState.GameOver, GameState.Upgrade) => true,
                (GameState.GameOver, GameState.Gameplay) => true,
                (GameState.Upgrade, GameState.Gameplay) => true,
                (GameState.Upgrade, GameState.MainMenu) => true,
                _ => false
            };
        }
    }
}
