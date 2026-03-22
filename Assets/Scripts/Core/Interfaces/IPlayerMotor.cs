using System;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    public interface IPlayerMotor
    {
        // === Read-only state ===
        Vector3 Velocity { get; }
        float CurrentSpeed { get; }
        bool IsMoving { get; }

        // === Events ===
        event Action<float> OnSpeedChanged;
        event Action OnLaunched;
        event Action OnHalted;

        // === Commands (controller has authority) ===
        void AlignToSurface(float deltaTime);
        void Launch(Vector3 force);
        void Steer(float horizontalInput, float deltaTime);
        void ApplyDownhillForce(float deltaTime);
        void ApplyDrag(float deltaTime);
        void Halt();
        void UpdateConfig(Gameplay.Player.PlayerMotorConfig config);
    }
}