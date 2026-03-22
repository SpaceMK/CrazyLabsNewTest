using SledSurfers.Data.ScriptableObjects;

namespace SledSurfers.Gameplay.Player
{
    public sealed class PlayerMotorConfig
    {
        public float MaxSpeed { get; }
        public float SteeringSpeed { get; }
        public float DownhillAcceleration { get; }
        public float DragCoefficient { get; }

        public PlayerMotorConfig(GameSettings settings, int maxSpeedLevel, int steeringLevel)
        {
            MaxSpeed = settings.BaseMaxSpeed + settings.MaxSpeedPerLevel * (maxSpeedLevel - 1);
            SteeringSpeed = settings.BaseSteeringSpeed + settings.SteeringSpeedPerLevel * (steeringLevel - 1);
            DownhillAcceleration = settings.DownhillAcceleration;
            DragCoefficient = settings.DragCoefficient;
        }
    }
}