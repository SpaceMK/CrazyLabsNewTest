using SledSurfers.Data.Upgrades;

namespace SledSurfers.UI.Upgrades
{
    /// <summary>
    /// Data for displaying an upgrade item in the UI.
    /// </summary>
    public class UpgradeItemData
    {
        public UpgradeStatType Type { get; set; }
        public string Name { get; set; }
        public int CurrentLevel { get; set; }
        public int MaxLevel { get; set; }
        public int Cost { get; set; }
        public float CurrentValue { get; set; }
        public float NextValue { get; set; }
        public bool CanAfford { get; set; }
        public bool IsMaxLevel { get; set; }
    }
}