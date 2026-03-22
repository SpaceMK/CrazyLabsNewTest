using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Slingshot;
using UnityEngine;
using VContainer;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// Facade MonoBehaviour that lives on the Player GameObject.
    /// Composes runtime instances of motor, slingshot, momentum tracker, etc.
    /// 
    /// Responsible for creating config data objects and passing them to controllers.
    /// Controllers own behavior; this class owns composition and lifecycle.
    /// </summary>
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(PlayerCollisionHandler))]
    [RequireComponent(typeof(PlayerPhysicsController))]
    public class PlayerManager : MonoBehaviour
    {
        private Rigidbody _rigidbody;
        private PlayerCollisionHandler _collisionHandler;
        private PlayerPhysicsController _physicsController;
        private Vector3 _startingPosition;
        private PlayerMotorController _motor;
        private SlingshotController _slingshot;
        private MomentumTracker _momentumTracker;

        private IEntityDespawner _entityDespawner;
        private bool _isInitialized;

        // Expose interfaces for external consumers
        public IPlayerMotor Motor => _motor;
        public ICollisionHandler CollisionHandler => _collisionHandler;
        public ISlingshot Slingshot => _slingshot;
        public IMomentumTracker MomentumTracker => _momentumTracker;

        public Rigidbody Rigidbody => _rigidbody;

        public float FinalDistance = 0f;

        [Inject]
        public void Construct(IEntityDespawner entityDespawner)
        {
            _entityDespawner = entityDespawner;
        }

        public void Initialize(GameSettings settings, Data.Models.PlayerData playerData, IInputHandler input)
        {
            if (_rigidbody == null)
                _rigidbody = GetComponent<Rigidbody>();
            if (_collisionHandler == null)
                _collisionHandler = GetComponent<PlayerCollisionHandler>();
            if (_physicsController == null)
                _physicsController = GetComponent<PlayerPhysicsController>();

            _rigidbody.isKinematic = true;
            _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
            _rigidbody.collisionDetectionMode = CollisionDetectionMode.Continuous;
            _rigidbody.constraints = RigidbodyConstraints.FreezeRotation;

            // Build config data objects from settings + upgrade levels
            var motorConfig = new PlayerMotorConfig(settings, playerData.MaxSpeedLevel, playerData.SteeringLevel);
            var slingshotConfig = new SlingshotConfig(settings, playerData.LaunchPowerLevel);

            if (!_isInitialized)
            {
                _motor = new PlayerMotorController(_rigidbody, motorConfig);

                _slingshot = new SlingshotController(slingshotConfig);

                _momentumTracker = new MomentumTracker(_motor);

                _physicsController.Initialize(_motor, input);
                _collisionHandler.Initialize(_entityDespawner);

                _isInitialized = true;

                Debug.Log($"[PlayerManager] Initialized " +
                          $"(Speed Lv{playerData.MaxSpeedLevel}, Steering Lv{playerData.SteeringLevel}, " +
                          $"Launch Lv{playerData.LaunchPowerLevel})");
            }
            else
            {
                // Update configs on existing controllers — no recreation needed
                _motor.UpdateConfig(motorConfig);
                _slingshot.UpdateConfig(slingshotConfig);

                Debug.Log($"[PlayerManager] Updated stats " +
                          $"(Speed Lv{playerData.MaxSpeedLevel}, Steering Lv{playerData.SteeringLevel}, " +
                          $"Launch Lv{playerData.LaunchPowerLevel})");
            }
        }

        private void Start()
        {
            _startingPosition = transform.position;
        }

        void FixedUpdate()
        {
            _motor.AlignToSurface(Time.deltaTime);
        }
        public void CalculateDistance()
        {
            FinalDistance = Vector3.Distance(_startingPosition, transform.position);
        }

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
            _momentumTracker?.Dispose();
        }
    }
}