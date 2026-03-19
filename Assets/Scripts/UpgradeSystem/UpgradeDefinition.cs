using System;
using UnityEngine;

namespace SledSurfers.Data.Upgrades
{
    /// <summary>
    /// Definition for a single upgrade type.
    /// Can be loaded from ScriptableObject or JSON.
    /// </summary>
    [Serializable]
    public class UpgradeDefinition
    {
        public string Id;
        public string DisplayName;
        public string Description;
        public Sprite Icon;

        [Header("Leveling")]
        public int MaxLevel = 10;
        public int BaseCost = 10;
        public float CostMultiplier = 1.5f;

        [Header("Values")]
        public float BaseValue;
        public float ValuePerLevel;

        [Header("Stat Binding")]
        public UpgradeStatType StatType;

        /// <summary>
        /// Calculate cost for a given level.
        /// </summary>
        public int GetCost(int currentLevel)
        {
            return Mathf.RoundToInt(BaseCost * Mathf.Pow(CostMultiplier, currentLevel - 1));
        }

        /// <summary>
        /// Calculate value at a given level.
        /// </summary>
        public float GetValue(int level)
        {
            return BaseValue + ValuePerLevel * (level - 1);
        }
    }

    public enum UpgradeStatType
    {
        LaunchPower,
        MaxSpeed,
        Steering,
        CoinValue
    }
}