using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Calculates upgrade costs and applies upgrades to player data.
    /// Depends on GameSettings (injected) for tuning values - DIP.
    /// </summary>
    public sealed class UpgradeService : IUpgradeService
    {
        private readonly GameSettings _settings;

        public UpgradeService(GameSettings settings)
        {
            _settings = settings;
        }

        public bool CanAffordUpgrade(UpgradeType type, PlayerData data)
        {
            int currentLevel = GetLevel(type, data);
            
            if (currentLevel >= _settings.MaxUpgradeLevel)
                return false;

            return data.Coins >= GetUpgradeCost(type, currentLevel);
        }

        public PlayerData ApplyUpgrade(UpgradeType type, PlayerData data)
        {
            var updated = data.Clone();
            int currentLevel = GetLevel(type, updated);
            int cost = GetUpgradeCost(type, currentLevel);

            if (updated.Coins < cost || currentLevel >= _settings.MaxUpgradeLevel)
            {
                Debug.LogWarning($"[Upgrade] Cannot apply upgrade {type}.");
                return updated;
            }

            updated.Coins -= cost;

            switch (type)
            {
                case UpgradeType.LaunchPower:
                    updated.LaunchPowerLevel++;
                    break;
                case UpgradeType.MaxSpeed:
                    updated.MaxSpeedLevel++;
                    break;
                case UpgradeType.SteeringResponsiveness:
                    updated.SteeringLevel++;
                    break;
                case UpgradeType.CoinValue:
                    updated.CoinValueLevel++;
                    break;
            }

            return updated;
        }

        public int GetUpgradeCost(UpgradeType type, int currentLevel)
        {
            return Mathf.RoundToInt(_settings.BaseUpgradeCost * Mathf.Pow(_settings.UpgradeCostMultiplier, currentLevel - 1));
        }

        public float GetUpgradeValue(UpgradeType type, int level)
        {
            return type switch
            {
                UpgradeType.LaunchPower => _settings.BaseLaunchForce + _settings.LaunchForcePerLevel * (level - 1),
                UpgradeType.MaxSpeed => _settings.BaseMaxSpeed + _settings.MaxSpeedPerLevel * (level - 1),
                UpgradeType.SteeringResponsiveness => _settings.BaseSteeringSpeed + _settings.SteeringSpeedPerLevel * (level - 1),
                UpgradeType.CoinValue => _settings.BaseCoinValue + _settings.CoinValuePerLevel * (level - 1),
                _ => 0f
            };
        }

        private int GetLevel(UpgradeType type, PlayerData data)
        {
            return type switch
            {
                UpgradeType.LaunchPower => data.LaunchPowerLevel,
                UpgradeType.MaxSpeed => data.MaxSpeedLevel,
                UpgradeType.SteeringResponsiveness => data.SteeringLevel,
                UpgradeType.CoinValue => data.CoinValueLevel,
                _ => 1
            };
        }
    }
}
