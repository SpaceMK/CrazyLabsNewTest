using SledSurfers.Data.ScriptableObjects;

namespace SledSurfers.Gameplay.Slingshot
{
    /// <summary>
    /// Pure data class holding computed slingshot parameters.
    /// 
    /// Separates configuration/data from behavior (ISP).
    /// SlingshotController owns behavior and reads from this config.
    /// PlayerManager creates and updates this when upgrades change.
    /// 
    /// Immutable after construction — create a new instance to change values.
    /// </summary>
    public sealed class SlingshotConfig
    {
        /// <summary>
        /// Computed launch force = BaseLaunchForce + LaunchForcePerLevel * (level - 1).
        /// </summary>
        public float LaunchForce { get; }

        /// <summary>
        /// Screen pixels of downward drag required for maximum power.
        /// Tune per platform: ~200 for mobile, ~300 for desktop.
        /// </summary>
        public float MaxPullPixels { get; }

        /// <summary>
        /// Maximum launch angle in degrees. Drag fully left/right = this angle.
        /// </summary>
        public float MaxAngleDegrees { get; }

        /// <summary>
        /// Screen pixels of horizontal drag required for maximum angle.
        /// </summary>
        public float MaxAnglePixels { get; }

        /// <summary>
        /// Minimum pull percent required to count as a valid launch (below = cancel).
        /// </summary>
        public float MinPullThreshold { get; }

        /// <summary>
        /// Upward arc component of the launch direction.
        /// </summary>
        public float UpwardArc { get; }

        public SlingshotConfig(
            GameSettings settings,
            int launchPowerLevel,
            float maxPullPixels = 250f,
            float maxAngleDegrees = 35f,
            float maxAnglePixels = 200f,
            float minPullThreshold = 0.1f,
            float upwardArc = 0.3f)
        {
            LaunchForce = settings.BaseLaunchForce
                + settings.LaunchForcePerLevel * (launchPowerLevel - 1);
            MaxPullPixels = maxPullPixels;
            MaxAngleDegrees = maxAngleDegrees;
            MaxAnglePixels = maxAnglePixels;
            MinPullThreshold = minPullThreshold;
            UpwardArc = upwardArc;
        }
    }
}