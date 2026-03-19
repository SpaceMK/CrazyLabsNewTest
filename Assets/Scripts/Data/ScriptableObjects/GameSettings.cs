using UnityEngine;

namespace SledSurfers.Data.ScriptableObjects
{
    /// <summary>
    /// Central configuration for gameplay tuning.
    /// ScriptableObject allows designers to tweak values without touching code.
    /// </summary>
    [CreateAssetMenu(fileName = "GameSettings", menuName = "SledSurfers/Game Settings")]
    public class GameSettings : ScriptableObject
    {
        [Header("Scene Names")]
        public string BootstrapSceneName = "Bootstrap";
        public string GameplaySceneName = "Gameplay";
        public string UISceneName = "UI";

        [Header("Launch Settings")]
        public float BaseLaunchForce = 15f;
        public float LaunchForcePerLevel = 3f;

        [Header("Movement Settings")]
        public float BaseMaxSpeed = 20f;
        public float MaxSpeedPerLevel = 2f;
        public float BaseSteeringSpeed = 8f;
        public float SteeringSpeedPerLevel = 1.5f;
        public float DownhillAcceleration = 5f;
        public float DragCoefficient = 0.5f;

        [Header("Collectibles")]
        public int BaseCoinValue = 1;
        public int CoinValuePerLevel = 1;

        [Header("Upgrades")]
        public int BaseUpgradeCost = 10;
        public float UpgradeCostMultiplier = 1.5f;
        public int MaxUpgradeLevel = 10;
    }
}
