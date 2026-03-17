using System;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for slingshot charge-and-release mechanics.
    /// Returns force vector on release - physics applied by IPlayerMotor.
    /// </summary>
    public interface ISlingshot
    {
        bool IsReady { get; }
        float ChargePercent { get; }

        event Action OnChargeStarted;
        event Action<float> OnChargeUpdated;
        event Action<float> OnReleased;

        void StartCharging();
        Vector3 Release();
        void Reset();
    }
}