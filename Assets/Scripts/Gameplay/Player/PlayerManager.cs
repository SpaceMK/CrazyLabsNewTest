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
    /// Sub-systems communicate via events — no need to route through GameplayFlow.
    /// 
    /// ICoinDespawner is injected via VContainer [Inject] so we avoid
    /// FindFirstObjectByType (which defeated the DI architecture).
    /// 
    /// Ticks the IInputHandler each frame so SimpleInputHandler can track
    /// drag state from mouse/touch. This is necessary because SimpleInputHandler
    /// is a plain C# class (not a MonoBehaviour) and needs a frame-driven update.
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
        private PlayerMotor _motor;
        private SlingshotController _slingshot;
        private MomentumTracker _momentumTracker;

        private ICoinDespawner _coinDespawner;
        private IInputHandler _input;
        private bool _isInitialized;

        // Expose interfaces for external consumers
        public IPlayerMotor Motor => _motor;
        public ICollisionHandler CollisionHandler => _collisionHandler;
        public ISlingshot Slingshot => _slingshot;
        public IMomentumTracker MomentumTracker => _momentumTracker;

        public Rigidbody Rigidbody => _rigidbody;

        public float FinalDistance = 0f;

        [Inject]
        public void Construct(ICoinDespawner coinDespawner)
        {
            _coinDespawner = coinDespawner;
        }

        public void Initialize(GameSettings settings, Data.Models.PlayerData playerData, IInputHandler input)
        {
            _input = input;

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

            if (!_isInitialized)
            {
                _motor = new PlayerMotor(
                    _rigidbody,
                    settings,
                    playerData.MaxSpeedLevel,
                    playerData.SteeringLevel
                );

                _slingshot = new SlingshotController(settings, playerData.LaunchPowerLevel);

                _momentumTracker = new MomentumTracker(_motor);

                _physicsController.Initialize(_motor, input);
                _collisionHandler.Initialize(_coinDespawner);

                _isInitialized = true;

                Debug.Log($"[PlayerManager] Initialized " +
                          $"(Speed Lv{playerData.MaxSpeedLevel}, Steering Lv{playerData.SteeringLevel}, " +
                          $"Launch Lv{playerData.LaunchPowerLevel})");
            }
            else
            {
                _motor.UpdateStats(settings, playerData.MaxSpeedLevel, playerData.SteeringLevel);
                _slingshot.UpdateStats(settings, playerData.LaunchPowerLevel);

                Debug.Log($"[PlayerManager] Updated stats " +
                          $"(Speed Lv{playerData.MaxSpeedLevel}, Steering Lv{playerData.SteeringLevel}, " +
                          $"Launch Lv{playerData.LaunchPowerLevel})");
            }
        }

        private void Start()
        {
            _startingPosition = transform.position;
        }

        private void Update()
        {
            // Tick the input handler so it can track drag state.
            // SimpleInputHandler is a plain C# class that reads Input.*
            // each frame — it needs this manual tick.
            if (_input is SimpleInputHandler simpleInput)
            {
                simpleInput.Tick();
            }
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