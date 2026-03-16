using System;

namespace SledSurfers.Data.Models
{
    /// <summary>
    /// Serializable player progression data.
    /// Pure data class - no behavior (SRP).
    /// </summary>
    [Serializable]
    public class PlayerData
    {
        public int Coins;
        public int LaunchPowerLevel;
        public int MaxSpeedLevel;
        public int SteeringLevel;
        public int CoinValueLevel;

        public static PlayerData Default => new PlayerData
        {
            Coins = 0,
            LaunchPowerLevel = 1,
            MaxSpeedLevel = 1,
            SteeringLevel = 1,
            CoinValueLevel = 1
        };

        public PlayerData Clone()
        {
            return new PlayerData
            {
                Coins = Coins,
                LaunchPowerLevel = LaunchPowerLevel,
                MaxSpeedLevel = MaxSpeedLevel,
                SteeringLevel = SteeringLevel,
                CoinValueLevel = CoinValueLevel
            };
        }
    }
}
