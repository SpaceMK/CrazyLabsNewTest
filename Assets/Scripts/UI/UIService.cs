using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.UI.Interfaces;
using UnityEngine;

namespace SledSurfers.UI.Services
{
    /// <summary>
    /// Mediates between GameState transitions and UI screen visibility.
    /// Implements the full IUIService contract so all consumers (screens and gameplay)
    /// depend on the interface, not this concrete class.
    /// </summary>
    public class UIService : IUIService, IDisposable
    {
        private readonly IGameStateManager _gameState;

        // Events from UI to Gameplay
        public event Action OnPlayClicked;
        public event Action OnMainMenuClicked;
        public event Action OnUpgradesClicked;
        public event Action OnCloseUpgradesClicked;

        // Events to UI Screens (driven by GameState)
        public event Action OnShowStartMenu;
        public event Action OnShowHUD;
        public event Action<float, int> OnShowGameOver;
        public event Action OnShowUpgrades;
        public event Action<int> OnCoinsUpdated;
        public event Action<float> OnDistanceUpdated;

        public UIService(IGameStateManager gameState)
        {
            _gameState = gameState;
            _gameState.OnStateChanged += HandleStateChanged;
        }

        public void Dispose()
        {
            _gameState.OnStateChanged -= HandleStateChanged;
        }

        private void HandleStateChanged(GameState oldState, GameState newState, GameStateData data)
        {
            Debug.Log($"[UIService] State changed: {oldState} → {newState}");

            switch (newState)
            {
                case GameState.StartMenu:
                    OnShowStartMenu?.Invoke();
                    break;

                case GameState.Playing:
                    OnShowHUD?.Invoke();
                    break;

                case GameState.GameOver:
                    float distance = data?.Distance ?? 0f;
                    int coins = data?.CoinsCollected ?? 0;
                    OnShowGameOver?.Invoke(distance, coins);
                    break;

                case GameState.Upgrades:
                    OnShowUpgrades?.Invoke();
                    break;
            }
        }

        public void UpdateCoins(int coins)
        {
            OnCoinsUpdated?.Invoke(coins);
        }

        public void UpdateDistance(float distance)
        {
            OnDistanceUpdated?.Invoke(distance);
        }

        public void TriggerPlay()
        {
            OnPlayClicked?.Invoke();
        }

        public void TriggerMainMenu()
        {
            OnMainMenuClicked?.Invoke();
        }

        public void TriggerUpgrades()
        {
            OnUpgradesClicked?.Invoke();
        }

        public void TriggerCloseUpgrades()
        {
            OnCloseUpgradesClicked?.Invoke();
        }
    }
}
