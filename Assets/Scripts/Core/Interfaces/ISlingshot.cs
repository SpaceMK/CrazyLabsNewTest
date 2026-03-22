using System;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Behavior contract for slingshot drag-and-release mechanics.
    /// 
    /// Converts a 2D drag gesture into a 3D launch force:
    /// - Drag horizontal axis controls launch angle (left/right).
    /// - Drag vertical axis (pull back) controls launch power.
    /// 
    /// Read-only state is exposed for UI/visual subscribers.
    /// All mutating methods are commands — the controller decides force computation.
    /// Configuration is handled separately by SlingshotConfig (pure data).
    /// </summary>
    public interface ISlingshot
    {
        // === Read-only state ===
        bool IsReady { get; }
        bool IsDragging { get; }
        float PullPercent { get; }
        float LaunchAngle { get; }

        // === Events ===
        event Action OnDragStarted;
        event Action<float, float> OnDragUpdated; // (pullPercent, angleDegrees)
        event Action<Vector3> OnReleased;

        // === Commands ===
        void StartDrag();
        void UpdateDrag(Vector2 dragDelta);
        Vector3 Release();
        void Cancel();
        void Reset();

        // === Configuration ===
        void UpdateConfig(Gameplay.Slingshot.SlingshotConfig config);
    }
}