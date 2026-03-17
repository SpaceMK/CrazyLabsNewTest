using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Slingshot;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Facade MonoBehaviour that lives on the Player GameObject.
    /// Composes runtime instances of motor, slingshot, momentum tracker, etc.
    /// 
    /// Sub-systems communicate via events - no need to route through GameplayFlow.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerCollisionHandler))]
    [RequireComponent(typeof(PlayerPhysicsController))]
    public sealed class PlayerManager : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private PlayerCollisionHandler _collisionHandler;
        private PlayerPhysicsController _physicsController;

        private PlayerMotor _motor;
        private SlingshotController _slingshot;
        private MomentumTracker _momentumTracker;

        // Expose interfaces for external consumers
        public IPlayerMotor Motor => _motor;
        public ICollisionHandler CollisionHandler => _collisionHandler;
        public ISlingshot Slingshot => _slingshot;
        public IMomentumTracker MomentumTracker => _momentumTracker;
        public Rigidbody Rigidbody => _rigidbody;

        /// <summary>
        /// Initialize must be called after Awake.
        /// Creates sub-systems and wires up event subscriptions.
        /// </summary>
        public void Initialize(GameSettings settings, Data.Models.PlayerData playerData, IInputHandler input)
        {
            _rigidbody = GetComponent<Rigidbody>();
            _collisionHandler = GetComponent<PlayerCollisionHandler>();
            _physicsController = GetComponent<PlayerPhysicsController>();

            // Configure rigidbody defaults
            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // Create motor first (other systems depend on it)
            _motor = new PlayerMotor(
                _rigidbody,
                settings,
                playerData.MaxSpeedLevel,
                playerData.SteeringLevel
            );

            // Create slingshot
            _slingshot = new SlingshotController(settings, playerData.LaunchPowerLevel);

            // Create momentum tracker - subscribes to motor events internally
            _momentumTracker = new MomentumTracker(_motor);

            // Initialize physics controller - subscribes to motor events internally
            _physicsController.Initialize(_motor, input);

            Debug.Log($"[PlayerManager] Initialized " +
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

            Debug.Log("[PlayerManager] Player reset to start position.");
        }

        private void OnDestroy()
        {
            // Cleanup subscriptions
            (_momentumTracker as MomentumTracker)?.Dispose();
        }
    }
}
