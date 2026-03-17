using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Slingshot;
using UnityEngine;


namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Facade MonoBehaviour that lives on the Player GameObject.
    /// Composes runtime instances of IPlayerMotor, ICollisionHandler, etc.
    /// 
    /// This is the bridge between Unity's scene (Rigidbody, Colliders) and
    /// the DI container. VContainer registers this as a component, and
    /// GameplayFlow accesses sub-systems through it.
    /// 
    /// Facade pattern - single entry point to the player subsystem.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerCollisionHandler))]
    public sealed class PlayerManager : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private PlayerCollisionHandler _collisionHandler;

        private PlayerMotor _motor;
        private SlingshotController _slingshot;
        private MomentumTracker _momentumTracker;

        // Expose interfaces for external consumers (GameplayFlow, UI, etc.)
        public IPlayerMotor Motor => _motor;
        public ICollisionHandler CollisionHandler => _collisionHandler;
        public ISlingshot Slingshot => _slingshot;
        public IMomentumTracker MomentumTracker => _momentumTracker;
        public Rigidbody Rigidbody => _rigidbody;

        /// <summary>
        /// Initialize must be called after Awake, typically by GameplayFlow.
        /// Passes in DI-resolved dependencies that MonoBehaviours can't receive via constructor.
        /// </summary>
        public void Initialize(GameSettings settings, Data.Models.PlayerData playerData)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collisionHandler = GetComponent<PlayerCollisionHandler>();

            // Configure rigidbody defaults
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // Create sub-systems with player's upgrade levels
            _motor = new PlayerMotor(_rigidbody, settings, playerData.MaxSpeedLevel, playerData.SteeringLevel);
            _slingshot = new SlingshotController(settings, playerData.LaunchPowerLevel);
            _momentumTracker = new MomentumTracker();

            Debug.Log("[PlayerFacade] Initialized with player data " +
                      $"(Speed Lv{playerData.MaxSpeedLevel}, Steering Lv{playerData.SteeringLevel}, " +
                      $"Launch Lv{playerData.LaunchPowerLevel})");
        }

        /// <summary>
        /// Reset player to initial position/state for retry.
        /// </summary>
        public void ResetPlayer(Vector3 startPosition)
        {
            _motor.Halt();
            transform.position = startPosition;
            transform.rotation = Quaternion.identity;
            _slingshot.Reset();
            _momentumTracker.StopTracking();

            Debug.Log("[PlayerFacade] Player reset to start position.");
        }
    }
}
