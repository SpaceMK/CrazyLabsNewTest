using System;
using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.Upgrades;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Handles upgrade logic and cost calculations.
    /// 
    /// Uses dictionary-based dispatch instead of switch statements so that
    /// adding a new UpgradeStatType only requires adding entries to the
    /// dictionaries — no method bodies need to change (OCP).
    /// 
    /// Trade-off: Slightly more indirection than a switch, but scales cleanly
    /// and eliminates the risk of forgetting a case when new types are added.
    /// </summary>
    public sealed class UpgradeService : IUpgradeService
    {
        private readonly IUpgradeConfigProvider _configProvider;

        /// <summary>
        /// Maps each stat type to a getter that reads the current level from PlayerData.
        /// </summary>
        private static readonly Dictionary<UpgradeStatType, Func<PlayerData, int>> LevelGetters = new()
        {
            { UpgradeStatType.LaunchPower, d => d.LaunchPowerLevel },
            { UpgradeStatType.MaxSpeed,    d => d.MaxSpeedLevel },
            { UpgradeStatType.Steering,    d => d.SteeringLevel },
            { UpgradeStatType.CoinValue,   d => d.CoinValueLevel },
        };

        /// <summary>
        /// Maps each stat type to a mutator that increments the level on PlayerData.
        /// </summary>
        private static readonly Dictionary<UpgradeStatType, Action<PlayerData>> LevelSetters = new()
        {
            { UpgradeStatType.LaunchPower, d => d.LaunchPowerLevel++ },
            { UpgradeStatType.MaxSpeed,    d => d.MaxSpeedLevel++ },
            { UpgradeStatType.Steering,    d => d.SteeringLevel++ },
            { UpgradeStatType.CoinValue,   d => d.CoinValueLevel++ },
        };

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
            return LevelGetters.TryGetValue(type, out var getter) ? getter(data) : 1;
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

            if (LevelSetters.TryGetValue(type, out var setter))
            {
                setter(updated);
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
