using System;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for slingshot drag-and-release mechanics.
    /// 
    /// The slingshot converts a 2D drag gesture into a 3D launch force:
    /// - Drag horizontal axis controls launch angle (left/right).
    /// - Drag vertical axis (pull back) controls launch power.
    /// 
    /// Returns a force vector on release — physics applied by IPlayerMotor.
    /// </summary>
    public interface ISlingshot
    {
        bool IsReady { get; }
        bool IsDragging { get; }
        float PullPercent { get; }
        float LaunchAngle { get; }

        event Action OnDragStarted;
        event Action<float, float> OnDragUpdated; // (pullPercent, angleDegrees)
        event Action<Vector3> OnReleased;

        void StartDrag();
        void UpdateDrag(Vector2 dragDelta);
        Vector3 Release();
        void Cancel();
        void Reset();
    }
}