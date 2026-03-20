using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.Upgrades;
using SledSurfers.UI.Services;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

namespace SledSurfers.UI.Upgrades
{
    public class UpgradeScreen : MonoBehaviour
    {
        [SerializeField] private GameObject _root;
        [SerializeField] private TextMeshProUGUI _coinsText;
        [SerializeField] private Button _closeButton;
        [SerializeField] private Transform _upgradesContainer;
        [SerializeField] private UpgradeItemView _upgradeItemPrefab;

        private UIService _uiService;
        private IUpgradeService _upgradeService;
        private IPlayerDataService _playerDataService;
        private PlayerData _playerData;

        private readonly List<UpgradeItemView> _upgradeItems = new();

        [Inject]
        public void Construct(
            UIService uiService,
            IUpgradeService upgradeService,
            IPlayerDataService playerDataService)
        {
            _uiService = uiService;
            _upgradeService = upgradeService;
            _playerDataService = playerDataService;

            Subscribe();
            CreateUpgradeItems();
        }

        private void Subscribe()
        {
            _uiService.OnShowUpgrades += Show;
            _uiService.OnShowHUD += Hide;
            _uiService.OnShowStartMenu += Hide;

            _closeButton.onClick.AddListener(OnCloseClicked);
        }

        private void CreateUpgradeItems()
        {
            var upgrades = _upgradeService.GetAllUpgrades();

            foreach (var upgrade in upgrades)
            {
                var item = Instantiate(_upgradeItemPrefab, _upgradesContainer);
                item.OnUpgradeClicked += HandleUpgrade;
                _upgradeItems.Add(item);
            }

            Debug.Log($"[UpgradeScreen] Created {_upgradeItems.Count} upgrade items.");
        }

       

        public void Show()
        {
            _playerData = _playerDataService.Load();
            _root.SetActive(true);
            RefreshUI();
        }

        public void Hide()
        {
            _root.SetActive(false);
        }

        private void RefreshUI()
        {
            _coinsText.text = _playerData.Coins.ToString();

            var upgrades = _upgradeService.GetAllUpgrades();

            for (int i = 0; i < upgrades.Count && i < _upgradeItems.Count; i++)
            {
                var upgrade = upgrades[i];
                var item = _upgradeItems[i];
                int currentLevel = _upgradeService.GetCurrentLevel(upgrade.StatType, _playerData);
                bool isMaxLevel = currentLevel >= upgrade.MaxLevel;

                var data = new UpgradeItemData
                {
                    Type = upgrade.StatType,
                    Name = upgrade.DisplayName,
                    CurrentLevel = currentLevel,
                    MaxLevel = upgrade.MaxLevel,
                    Cost = isMaxLevel ? 0 : upgrade.GetCost(currentLevel),
                    CurrentValue = upgrade.GetValue(currentLevel),
                    NextValue = isMaxLevel ? 0 : upgrade.GetValue(currentLevel + 1),
                    CanAfford = _upgradeService.CanAffordUpgrade(upgrade.StatType, _playerData),
                    IsMaxLevel = isMaxLevel
                };

                item.SetData(data);
            }
        }

        private void HandleUpgrade(UpgradeStatType type)
        {
            if (!_upgradeService.CanAffordUpgrade(type, _playerData))
            {
                Debug.Log($"[UpgradeScreen] Cannot afford {type} upgrade.");
                return;
            }

            _playerData = _upgradeService.ApplyUpgrade(type, _playerData);
            _playerDataService.Save(_playerData);

            Debug.Log($"[UpgradeScreen] Upgraded {type}.");

            RefreshUI();
        }

        private void OnCloseClicked()
        {
            _uiService.TriggerCloseUpgrades();
        }
    }
}