using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.Upgrades;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Calculates upgrade costs and applies upgrades to player data.
    /// Uses IUpgradeConfigProvider for configuration.
    /// </summary>
    public sealed class UpgradeService : IUpgradeService
    {
        private readonly IUpgradeConfigProvider _configProvider;

        public UpgradeService(IUpgradeConfigProvider configProvider)
        {
            _configProvider = configProvider;
        }

        public IReadOnlyList<UpgradeDefinition> GetAllUpgrades()
        {
            return _configProvider.Upgrades;
        }

        public UpgradeDefinition GetUpgrade(UpgradeStatType type)
        {
            return _configProvider.GetUpgrade(type);
        }

        public int GetCurrentLevel(UpgradeStatType type, PlayerData data)
        {
            return type switch
            {
                UpgradeStatType.LaunchPower => data.LaunchPowerLevel,
                UpgradeStatType.MaxSpeed => data.MaxSpeedLevel,
                UpgradeStatType.Steering => data.SteeringLevel,
                UpgradeStatType.CoinValue => data.CoinValueLevel,
                _ => 1
            };
        }

        public bool CanAffordUpgrade(UpgradeStatType type, PlayerData data)
        {
            var upgrade = GetUpgrade(type);
            if (upgrade == null) return false;

            int currentLevel = GetCurrentLevel(type, data);

            if (currentLevel >= upgrade.MaxLevel)
                return false;

            return data.Coins >= upgrade.GetCost(currentLevel);
        }

        public PlayerData ApplyUpgrade(UpgradeStatType type, PlayerData data)
        {
            var upgrade = GetUpgrade(type);
            if (upgrade == null)
            {
                Debug.LogWarning($"[UpgradeService] Upgrade not found: {type}");
                return data;
            }

            var updated = data.Clone();
            int currentLevel = GetCurrentLevel(type, updated);
            int cost = upgrade.GetCost(currentLevel);

            if (updated.Coins < cost || currentLevel >= upgrade.MaxLevel)
            {
                Debug.LogWarning($"[UpgradeService] Cannot apply upgrade {type}.");
                return updated;
            }

            updated.Coins -= cost;

            switch (type)
            {
                case UpgradeStatType.LaunchPower:
                    updated.LaunchPowerLevel++;
                    break;
                case UpgradeStatType.MaxSpeed:
                    updated.MaxSpeedLevel++;
                    break;
                case UpgradeStatType.Steering:
                    updated.SteeringLevel++;
                    break;
                case UpgradeStatType.CoinValue:
                    updated.CoinValueLevel++;
                    break;
            }

            Debug.Log($"[UpgradeService] Applied {type} upgrade. New level: {GetCurrentLevel(type, updated)}");
            return updated;
        }

        public int GetUpgradeCost(UpgradeStatType type, int currentLevel)
        {
            var upgrade = GetUpgrade(type);
            return upgrade?.GetCost(currentLevel) ?? 0;
        }

        public float GetUpgradeValue(UpgradeStatType type, int level)
        {
            var upgrade = GetUpgrade(type);
            return upgrade?.GetValue(level) ?? 0f;
        }
    }
}