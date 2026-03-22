using System;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Player;
using SledSurfers.UI.Interfaces;
using UnityEngine;

namespace SledSurfers.Gameplay
{
    /// <summary>
    /// Manages a single gameplay run: drag-to-launch → run → end.
    /// 
    /// Extracted from GameplayFlow to satisfy SRP.
    /// GameplayFlow handles menu navigation and session lifecycle.
    /// RunSession handles the active gameplay loop and scoring.
    /// 
    /// Slingshot flow:
    /// 1. WaitingToLaunch: player touches/clicks the screen → StartDrag
    /// 2. Dragging: player holds and drags → UpdateDrag with screen delta each frame
    /// 3. Player releases → Release computes force vector → Motor.Launch
    /// 4. Running: normal gameplay until crash or momentum lost.
    /// </summary>
    public sealed class RunSession : IDisposable
    {
        private readonly IInputHandler _input;
        private readonly IUIService _uiService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;

        private PlayerData _playerData;
        private int _coinsCollected;

        public enum Phase
        {
            Inactive,
            WaitingToLaunch,
            Dragging,
            Running,
            Ended
        }

        public Phase CurrentPhase { get; private set; } = Phase.Inactive;

        /// <summary>
        /// Fired when the run ends (crash, momentum lost, etc.).
        /// Payload: (distance, coinsCollected).
        /// </summary>
        public event Action<float, int> OnRunEnded;

        public RunSession(
            IInputHandler input,
            IUIService uiService,
            GameSettings settings,
            PlayerManager player)
        {
            _input = input;
            _uiService = uiService;
            _settings = settings;
            _player = player;
        }

        /// <summary>
        /// Begin a new run. Subscribes to player events and enables input.
        /// </summary>
        public void Begin(PlayerData playerData)
        {
            _playerData = playerData;
            _coinsCollected = 0;

            _player.CollisionHandler.OnCrash += HandleCrash;
            _player.CollisionHandler.OnCoinCollected += HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost += HandleMomentumLost;

            _input.Enable();
            CurrentPhase = Phase.WaitingToLaunch;

            Debug.Log("[RunSession] Ready to launch. Drag to aim.");
        }

        /// <summary>
        /// Called every frame by GameplayFlow.Tick().
        /// </summary>
        public void Tick()
        {
            switch (CurrentPhase)
            {
                case Phase.WaitingToLaunch:
                    TickWaitingToLaunch();
                    break;
                case Phase.Dragging:
                    TickDragging();
                    break;
                case Phase.Running:
                    TickRunning();
                    break;
            }
        }

        public void Dispose()
        {
            UnsubscribePlayerEvents();
        }

        private void TickWaitingToLaunch()
        {
            if (_input.DragStarted)
            {
                _player.Slingshot.StartDrag();
                CurrentPhase = Phase.Dragging;
                Debug.Log("[RunSession] Dragging slingshot...");
            }
        }

        private void TickDragging()
        {
            // Always feed the current drag delta — including on the release frame,
            // so the slingshot has the final pull values before Release() is called.
            Vector2 delta = _input.DragDelta;
            _player.Slingshot.UpdateDrag(delta);

            if (_input.DragEnded)
            {
                Debug.Log($"[RunSession] Release drag delta: {delta}, Pull: {_player.Slingshot.PullPercent:P0}, Angle: {_player.Slingshot.LaunchAngle:F1}°");

                Vector3 force = _player.Slingshot.Release();

                if (force.sqrMagnitude < 0.01f)
                {
                    _player.Slingshot.Reset();
                    CurrentPhase = Phase.WaitingToLaunch;
                    Debug.Log("[RunSession] Pull too weak, back to waiting.");
                    return;
                }

                _player.Motor.Launch(force);
                _player.MomentumTracker.StartTracking();

                CurrentPhase = Phase.Running;
                Debug.Log("[RunSession] Launched!");
            }
        }

        private void TickRunning()
        {
            _player.CalculateDistance();
            _uiService.UpdateDistance(_player.FinalDistance);
        }

        private void HandleCrash()
        {
            if (CurrentPhase != Phase.Running) return;

            Debug.Log("[RunSession] Crashed!");
            EndRun();
        }

        private void HandleMomentumLost()
        {
            if (CurrentPhase != Phase.Running) return;

            Debug.Log("[RunSession] Momentum lost!");
            EndRun();
        }

        private void HandleCoinCollected(int amount)
        {
            if (CurrentPhase != Phase.Running) return;

            int value = _settings.BaseCoinValue
                + _settings.CoinValuePerLevel * (_playerData.CoinValueLevel - 1);
            _coinsCollected += amount * value;

            _uiService.UpdateCoins(_coinsCollected);
        }

        private void EndRun()
        {
            CurrentPhase = Phase.Ended;
            _input.Disable();
            _player.Motor.Halt();
            _player.MomentumTracker.StopTracking();

            _player.CalculateDistance();
            float distance = _player.FinalDistance;

            UnsubscribePlayerEvents();

            OnRunEnded?.Invoke(distance, _coinsCollected);

            Debug.Log($"[RunSession] Run ended. Distance: {distance:F1}m, Coins: {_coinsCollected}");
        }

        private void UnsubscribePlayerEvents()
        {
            _player.CollisionHandler.OnCrash -= HandleCrash;
            _player.CollisionHandler.OnCoinCollected -= HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost -= HandleMomentumLost;
        }
    }
}