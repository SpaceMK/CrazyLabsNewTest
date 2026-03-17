using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for the slingshot launch mechanic.
    /// SRP - only handles launch force calculation and execution.
    /// </summary>
    public interface ISlingshot
    {
        bool IsReady { get; }
        float ChargePercent { get; }

        void StartCharging();
        void Release(Rigidbody target);
        void Reset();

        event System.Action OnChargeStarted;
        event System.Action<float> OnChargeUpdated;
        event System.Action<float> OnReleased;
    }
}
