using System;

namespace SledSurfers.UI.Interfaces
{
    public interface IUIService
    {
        // Events from UI to Gameplay
        event Action OnPlayClicked;
        event Action OnMainMenuClicked;

        // Methods called by Gameplay to update UI
        void ShowStartMenu();
        void ShowHUD();
        void ShowGameOver(float distance, int coinsCollected);
        void UpdateCoins(int coins);
        void UpdateDistance(float distance);
        void HideAll();
    }
}
