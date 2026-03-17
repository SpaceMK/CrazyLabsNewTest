using SledSurfers.Core.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay.Player
{
    /// <summary>
    /// MonoBehaviour that drives player physics each FixedUpdate.
    /// 
    /// Self-contained - subscribes to Motor.OnLaunched to know when to start,
    /// and Motor.OnHalted to know when to stop.
    /// 
    /// GameplayFlow doesn't need to call this every frame.
    /// </summary>
    public sealed class PlayerPhysicsController : MonoBehaviour
    {
        private IPlayerMotor _motor;
        private IInputHandler _input;
        private bool _isRunning;

        public void Initialize(IPlayerMotor motor, IInputHandler input)
        {
            _motor = motor;
            _input = input;

            // Subscribe to motor events
            _motor.OnLaunched += HandleLaunched;
            _motor.OnHalted += HandleHalted;
        }

        private void FixedUpdate()
        {
            if (!_isRunning || _motor == null) return;

            float dt = Time.fixedDeltaTime;

            _motor.ApplyDownhillForce(dt);
            _motor.ApplyDrag(dt);
            _motor.Steer(_input.HorizontalInput, dt);
        }

        private void HandleLaunched()
        {
            _isRunning = true;
            Debug.Log("[PlayerPhysicsController] Physics started.");
        }

        private void HandleHalted()
        {
            _isRunning = false;
            Debug.Log("[PlayerPhysicsController] Physics stopped.");
        }

        private void OnDestroy()
        {
            if (_motor != null)
            {
                _motor.OnLaunched -= HandleLaunched;
                _motor.OnHalted -= HandleHalted;
            }
        }
    }
}
