using SledSurfers.UI.Interfaces;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SledSurfers.UI.Screens
{
    /// <summary>
    /// Start menu with Play and Upgrades buttons.
    /// Depends on IUIService (not concrete UIService) — DIP.
    /// </summary>
    public class StartMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _playButton;
        [SerializeField] private Button _upgradesButton;

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

            _uiService.OnShowStartMenu += Show;
            _uiService.OnShowHUD += Hide;
            _uiService.OnShowUpgrades += Hide;
            _uiService.OnShowGameOver += OnGameOver;

            _playButton.onClick.AddListener(OnPlayClicked);
            _upgradesButton.onClick.AddListener(OnUpgradesClicked);
        }

        private void Unsubscribe()
        {
            if (_uiService == null) return;

            _uiService.OnShowStartMenu -= Show;
            _uiService.OnShowHUD -= Hide;
            _uiService.OnShowUpgrades -= Hide;
            _uiService.OnShowGameOver -= OnGameOver;

            _playButton.onClick.RemoveListener(OnPlayClicked);
            _upgradesButton.onClick.RemoveListener(OnUpgradesClicked);
        }

        public void Show()
        {
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void OnGameOver(float distance, int coins)
        {
            Hide();
        }

        private void OnPlayClicked()
        {
            _uiService.TriggerPlay();
        }

        private void OnUpgradesClicked()
        {
            _uiService.TriggerUpgrades();
        }
    }
}
