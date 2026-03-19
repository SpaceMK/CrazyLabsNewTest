using System.Collections.Generic;
using SledSurfers.Data.Models;
using SledSurfers.Data.Upgrades;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Handles upgrade logic and cost calculations.
    /// </summary>
    public interface IUpgradeService
    {
        IReadOnlyList<UpgradeDefinition> GetAllUpgrades();
        UpgradeDefinition GetUpgrade(UpgradeStatType type);

        int GetCurrentLevel(UpgradeStatType type, PlayerData data);
        bool CanAffordUpgrade(UpgradeStatType type, PlayerData data);
        PlayerData ApplyUpgrade(UpgradeStatType type, PlayerData data);

        int GetUpgradeCost(UpgradeStatType type, int currentLevel);
        float GetUpgradeValue(UpgradeStatType type, int level);
    }
}