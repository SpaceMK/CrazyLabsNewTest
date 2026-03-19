using System;
using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    public class GameStateManager : IGameStateManager
    {
        public GameState CurrentState { get; private set; } = GameState.None;
        public GameStateData CurrentData { get; private set; }

        public event Action<GameState, GameState, GameStateData> OnStateChanged;

        public void TransitionTo(GameState newState, GameStateData data = null)
        {
            if (CurrentState == newState) return;

            var oldState = CurrentState;
            CurrentState = newState;
            CurrentData = data;

            Debug.Log($"[GameState] {oldState} → {newState}");

            OnStateChanged?.Invoke(oldState, newState, data);
        }
    }
}