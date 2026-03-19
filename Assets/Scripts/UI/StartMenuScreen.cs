using SledSurfers.UI.Services;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SledSurfers.UI.Screens
{
    /// <summary>
    /// Start menu with Play button.
    /// Shown when game loads.
    /// </summary>
    public class StartMenuScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private Button _playButton;

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

            _uiService.OnShowStartMenu += Show;
            _uiService.OnShowHUD += Hide;
            _uiService.OnHideAll += Hide;

            _playButton.onClick.AddListener(OnPlayClicked);
        }

        private void Unsubscribe()
        {
            if (_uiService == null) return;

            _uiService.OnShowStartMenu -= Show;
            _uiService.OnShowHUD -= Hide;
            _uiService.OnHideAll -= Hide;

            _playButton.onClick.RemoveListener(OnPlayClicked);
        }

        public void Show()
        {
            _root.SetActive(true);
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void OnPlayClicked()
        {
            _uiService.TriggerPlay();
        }
    }
}
