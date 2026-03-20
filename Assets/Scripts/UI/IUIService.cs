using System;

namespace SledSurfers.UI.Interfaces
{
    /// <summary>
    /// Full UI service contract.
    /// 
    /// Split into two concerns:
    /// - Gameplay-to-UI: events that gameplay subscribes to (OnPlayClicked, etc.)
    /// - State-to-Screen: events that UI screens subscribe to (OnShowHUD, etc.)
    /// 
    /// Both live on one interface because the UIService is the single mediator
    /// between game state and UI presentation. Screens and gameplay both depend
    /// on this interface, never on the concrete UIService.
    /// </summary>
    public interface IUIService
    {
        // === Events from UI to Gameplay ===
        event Action OnPlayClicked;
        event Action OnMainMenuClicked;
        event Action OnUpgradesClicked;
        event Action OnCloseUpgradesClicked;

        // === Events from GameState to UI Screens ===
        event Action OnShowStartMenu;
        event Action OnShowHUD;
        event Action<float, int> OnShowGameOver;
        event Action OnShowUpgrades;

        // === Runtime data updates ===
        event Action<int> OnCoinsUpdated;
        event Action<float> OnDistanceUpdated;

        void UpdateCoins(int coins);
        void UpdateDistance(float distance);

        // === Trigger methods (called by UI button handlers) ===
        void TriggerPlay();
        void TriggerMainMenu();
        void TriggerUpgrades();
        void TriggerCloseUpgrades();
    }
}
