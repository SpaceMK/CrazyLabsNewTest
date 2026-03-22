using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for player input.
    /// Supports both keyboard/mouse and touch input.
    /// 
    /// Drag properties are used by the slingshot system:
    /// - DragStarted: true on the frame the player begins dragging
    /// - IsDragging: true while the player holds the drag
    /// - DragEnded: true on the frame the player releases
    /// - DragDelta: screen-space offset from drag start to current position
    /// 
    /// HorizontalInput is used for steering during the run.
    /// </summary>
    public interface IInputHandler
    {
        // === Steering (during run) ===
        float HorizontalInput { get; }

        // === Drag (for slingshot) ===
        bool DragStarted { get; }
        bool IsDragging { get; }
        bool DragEnded { get; }
        Vector2 DragDelta { get; }

        void Enable();
        void Disable();
        void Tick();
    }
}