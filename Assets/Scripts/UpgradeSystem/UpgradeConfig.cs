using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Data.Upgrades
{
    /// <summary>
    /// ScriptableObject containing all upgrade definitions.
    /// Can be replaced with JSON loader in the future.
    /// </summary>
    [CreateAssetMenu(fileName = "UpgradeConfig", menuName = "SledSurfers/Upgrade Config")]
    public class UpgradeConfig : ScriptableObject
    {
        [SerializeField] private List<UpgradeDefinition> _upgrades = new();

        public IReadOnlyList<UpgradeDefinition> Upgrades => _upgrades;

        public UpgradeDefinition GetUpgrade(string id)
        {
            return _upgrades.Find(u => u.Id == id);
        }

        public UpgradeDefinition GetUpgrade(UpgradeStatType statType)
        {
            return _upgrades.Find(u => u.StatType == statType);
        }

#if UNITY_EDITOR
        /// <summary>
        /// Create default upgrades for quick setup.
        /// </summary>
        [ContextMenu("Create Default Upgrades")]
        private void CreateDefaults()
        {
            _upgrades = new List<UpgradeDefinition>
            {
                new UpgradeDefinition
                {
                    Id = "launch_power",
                    DisplayName = "Launch Power",
                    Description = "Increases initial launch force",
                    StatType = UpgradeStatType.LaunchPower,
                    MaxLevel = 10,
                    BaseCost = 10,
                    CostMultiplier = 1.5f,
                    BaseValue = 15f,
                    ValuePerLevel = 3f
                },
                new UpgradeDefinition
                {
                    Id = "max_speed",
                    DisplayName = "Max Speed",
                    Description = "Increases maximum speed",
                    StatType = UpgradeStatType.MaxSpeed,
                    MaxLevel = 10,
                    BaseCost = 10,
                    CostMultiplier = 1.5f,
                    BaseValue = 20f,
                    ValuePerLevel = 2f
                },
                new UpgradeDefinition
                {
                    Id = "steering",
                    DisplayName = "Steering",
                    Description = "Increases steering responsiveness",
                    StatType = UpgradeStatType.Steering,
                    MaxLevel = 10,
                    BaseCost = 10,
                    CostMultiplier = 1.5f,
                    BaseValue = 8f,
                    ValuePerLevel = 1.5f
                },
                new UpgradeDefinition
                {
                    Id = "coin_value",
                    DisplayName = "Coin Value",
                    Description = "Increases value of collected coins",
                    StatType = UpgradeStatType.CoinValue,
                    MaxLevel = 10,
                    BaseCost = 10,
                    CostMultiplier = 1.5f,
                    BaseValue = 1f,
                    ValuePerLevel = 1f
                }
            };

            UnityEditor.EditorUtility.SetDirty(this);
        }
#endif
    }
}