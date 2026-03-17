using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for player movement physics.
    /// SRP - only handles forces, velocity, steering. Doesn't know about game state or collisions.
    /// DIP - gameplay flow depends on this, not on Rigidbody directly.
    /// </summary>
    public interface IPlayerMotor
    {
        Vector3 Velocity { get; }
        float CurrentSpeed { get; }
        bool IsMoving { get; }

        void Launch(Vector3 force);
        void Steer(float horizontalInput, float deltaTime);
        void ApplyDownhillForce(float deltaTime);
        void ApplyDrag(float deltaTime);
        void Halt();
    }
}
