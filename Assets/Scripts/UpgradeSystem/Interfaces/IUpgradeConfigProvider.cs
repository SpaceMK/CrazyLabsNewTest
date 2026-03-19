using System.Collections.Generic;
using System.Threading.Tasks;
using SledSurfers.Data.Upgrades;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Interface for loading upgrade configurations.
    /// Allows swapping between ScriptableObject and remote JSON.
    /// </summary>
    public interface IUpgradeConfigProvider
    {
        IReadOnlyList<UpgradeDefinition> Upgrades { get; }
        UpgradeDefinition GetUpgrade(string id);
        UpgradeDefinition GetUpgrade(UpgradeStatType statType);
        Task LoadAsync();
        bool IsLoaded { get; }
    }
}