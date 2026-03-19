using System;

namespace SledSurfers.UI.Interfaces
{
    public interface IUIService
    {
        // Events from UI to Gameplay
        event Action OnPlayClicked;
        event Action OnMainMenuClicked;
        event Action OnPauseClicked;
        event Action OnResumeClicked;

        // Runtime data updates
        void UpdateCoins(int coins);
        void UpdateDistance(float distance);
    }
}