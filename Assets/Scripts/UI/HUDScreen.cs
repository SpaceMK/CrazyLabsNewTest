using SledSurfers.UI.Services;
using TMPro;
using UnityEngine;
using VContainer;

namespace SledSurfers.UI.Screens
{
    /// <summary>
    /// In-game HUD showing coins and distance.
    /// </summary>
    public class HUDScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private TextMeshProUGUI _distanceText;

        private UIService _uiService;

        [Inject]
        public void Construct(UIService uiService)
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

            _uiService.OnShowHUD += Show;
            _uiService.OnShowStartMenu += Hide;
            _uiService.OnShowGameOver += OnGameOver;
            _uiService.OnHideAll += Hide;
            _uiService.OnCoinsUpdated += UpdateCoins;
            _uiService.OnDistanceUpdated += UpdateDistance;
        }

        private void Unsubscribe()
        {
            if (_uiService == null) return;

            _uiService.OnShowHUD -= Show;
            _uiService.OnShowStartMenu -= Hide;
            _uiService.OnShowGameOver -= OnGameOver;
            _uiService.OnHideAll -= Hide;
            _uiService.OnCoinsUpdated -= UpdateCoins;
            _uiService.OnDistanceUpdated -= UpdateDistance;
        }

        public void Show()
        {
            _root.SetActive(true);
            UpdateCoins(0);
            UpdateDistance(0);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void UpdateCoins(int coins)
        {
            _coinsText.text = coins.ToString();
        }

        private void UpdateDistance(float distance)
        {
            _distanceText.text = $"{distance:F0}m";
        }

        private void OnGameOver(float distance, int coins)
        {
            Hide();
        }
    }
}
