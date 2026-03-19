using System;
using SledSurfers.Data.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace SledSurfers.UI.Upgrades
{
    /// <summary>
    /// Individual upgrade item UI.
    /// Displays name, level, cost, and upgrade button.
    /// </summary>
    public class UpgradeItemView : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI _nameText;
        [SerializeField] private TextMeshProUGUI _levelText;
        [SerializeField] private TextMeshProUGUI _valueText;
        [SerializeField] private TextMeshProUGUI _costText;
        [SerializeField] private Button _upgradeButton;
        [SerializeField] private GameObject _maxLevelIndicator;

        private UpgradeStatType _type;

        public event Action<UpgradeStatType> OnUpgradeClicked;

        private void Awake()
        {
            _upgradeButton.onClick.AddListener(HandleUpgradeClicked);
        }

        private void OnDestroy()
        {
            _upgradeButton.onClick.RemoveListener(HandleUpgradeClicked);
        }

        public void SetData(UpgradeItemData data)
        {
            _type = data.Type;

            _nameText.text = data.Name;
            _levelText.text = $"Lv {data.CurrentLevel}/{data.MaxLevel}";

            if (data.IsMaxLevel)
            {
                _valueText.text = $"{data.CurrentValue:F1}";
                _costText.text = "MAX";
                _upgradeButton.interactable = false;

                if (_maxLevelIndicator != null)
                    _maxLevelIndicator.SetActive(true);
            }
            else
            {
                _valueText.text = $"{data.CurrentValue:F1} → {data.NextValue:F1}";
                _costText.text = $"{data.Cost}";
                _upgradeButton.interactable = data.CanAfford;

                if (_maxLevelIndicator != null)
                    _maxLevelIndicator.SetActive(false);
            }
        }

        private void HandleUpgradeClicked()
        {
            OnUpgradeClicked?.Invoke(_type);
        }
    }
}