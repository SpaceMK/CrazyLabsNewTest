using System.Collections.Generic;
using System.Threading.Tasks;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Upgrades;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Loads upgrade config from ScriptableObject.
    /// </summary>
    public class ScriptableObjectUpgradeConfigProvider : IUpgradeConfigProvider
    {
        private readonly UpgradeConfig _config;

        public IReadOnlyList<UpgradeDefinition> Upgrades => _config.Upgrades;
        public bool IsLoaded => _config != null;

        public ScriptableObjectUpgradeConfigProvider(UpgradeConfig config)
        {
            _config = config;
            Debug.Log($"[UpgradeConfigProvider] Loaded {_config.Upgrades.Count} upgrades from ScriptableObject.");
        }

        public UpgradeDefinition GetUpgrade(string id)
        {
            return _config.GetUpgrade(id);
        }

        public UpgradeDefinition GetUpgrade(UpgradeStatType statType)
        {
            return _config.GetUpgrade(statType);
        }

        public Task LoadAsync()
        {
            // Already loaded via constructor
            return Task.CompletedTask;
        }
    }
}