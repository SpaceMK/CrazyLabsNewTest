using System;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for player physics/movement.
    /// Single point of contact for all Rigidbody interactions.
    /// Exposes events for other systems to subscribe to.
    /// </summary>
    public interface IPlayerMotor
    {
        Vector3 Velocity { get; }
        float CurrentSpeed { get; }
        bool IsMoving { get; }

        /// <summary>
        /// Fired every physics update with current speed.
        /// Subscribe: Camera, UI, MomentumTracker.
        /// </summary>
        event Action<float> OnSpeedChanged;

        /// <summary>
        /// Fired when player is launched.
        /// </summary>
        event Action OnLaunched;

        /// <summary>
        /// Fired when player is halted.
        /// </summary>
        event Action OnHalted;

        void Launch(Vector3 force);
        void Steer(float horizontalInput, float deltaTime);
        void ApplyDownhillForce(float deltaTime);
        void ApplyDrag(float deltaTime);
        void Halt();
    }
}
