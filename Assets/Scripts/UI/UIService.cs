using System;
using SledSurfers.UI.Interfaces;

namespace SledSurfers.UI.Services
{
    /// <summary>
    /// Service for cross-scene UI communication.
    /// Registered in GameLifetimeScope (singleton).
    /// </summary>
    public class UIService : IUIService
    {
        // Events from UI to Gameplay
        public event Action OnPlayClicked;
        public event Action OnMainMenuClicked;

        // Events from Service to UI Screens
        public event Action OnShowStartMenu;
        public event Action OnShowHUD;
        public event Action<float, int> OnShowGameOver;
        public event Action<int> OnCoinsUpdated;
        public event Action<float> OnDistanceUpdated;
        public event Action OnHideAll;

        public void ShowStartMenu()
        {
            OnShowStartMenu?.Invoke();
        }

        public void ShowHUD()
        {
            OnShowHUD?.Invoke();
        }

        public void ShowGameOver(float distance, int coinsCollected)
        {
            OnShowGameOver?.Invoke(distance, coinsCollected);
        }

        public void UpdateCoins(int coins)
        {
            OnCoinsUpdated?.Invoke(coins);
        }

        public void UpdateDistance(float distance)
        {
            OnDistanceUpdated?.Invoke(distance);
        }

        public void HideAll()
        {
            OnHideAll?.Invoke();
        }

        // Called by UI buttons
        public void TriggerPlay()
        {
            OnPlayClicked?.Invoke();
        }

        public void TriggerMainMenu()
        {
            OnMainMenuClicked?.Invoke();
        }
    }
}
