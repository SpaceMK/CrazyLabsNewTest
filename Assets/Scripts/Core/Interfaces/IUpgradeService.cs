using SledSurfers.Data.Models;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Handles upgrade logic and cost calculations.
    /// ISP - separated from IPlayerDataService since upgrade rules != persistence.
    /// </summary>
    public interface IUpgradeService
    {
        bool CanAffordUpgrade(UpgradeType type, PlayerData data);
        PlayerData ApplyUpgrade(UpgradeType type, PlayerData data);
        int GetUpgradeCost(UpgradeType type, int currentLevel);
        float GetUpgradeValue(UpgradeType type, int level);
    }

    public enum UpgradeType
    {
        LaunchPower,
        MaxSpeed,
        SteeringResponsiveness,
        CoinValue
    }
}
