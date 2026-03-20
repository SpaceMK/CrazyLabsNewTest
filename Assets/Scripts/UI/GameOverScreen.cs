using SledSurfers.UI.Interfaces;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SledSurfers.UI.Screens
{
    /// <summary>
    /// Game over screen with results and play again button.
    /// Depends on IUIService (not concrete UIService) — DIP.
    /// </summary>
    public class GameOverScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _distanceText;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Button _playAgainButton;
        [SerializeField] private Button _mainMenuButton;

        private IUIService _uiService;

        [Inject]
        public void Construct(IUIService uiService)
        {
            _uiService = uiService;
            Subscribe();
        }

        private void OnDestroy()
        {
            Unsubscribe();
        }

        private void Subscribe()
        {
            if (_uiService == null) return;

            _uiService.OnShowGameOver += Show;
            _uiService.OnShowHUD += Hide;
            _uiService.OnShowStartMenu += Hide;

            _playAgainButton.onClick.AddListener(OnPlayAgainClicked);
            _mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }

        private void Unsubscribe()
        {
            if (_uiService == null) return;

            _uiService.OnShowGameOver -= Show;
            _uiService.OnShowHUD -= Hide;
            _uiService.OnShowStartMenu -= Hide;

            _playAgainButton.onClick.RemoveListener(OnPlayAgainClicked);
            _mainMenuButton.onClick.RemoveListener(OnMainMenuClicked);
        }

        public void Show(float distance, int coins)
        {
            _root.SetActive(true);
            _distanceText.text = $"Distance: {distance:F0}m";
            _coinsText.text = $"Coins : {coins}";
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void OnPlayAgainClicked()
        {
            _uiService.TriggerPlay();
        }

        private void OnMainMenuClicked()
        {
            _uiService.TriggerMainMenu();
        }
    }
}
