using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer.Unity;

namespace SledSurfers.Gameplay
{
    /// <summary>
    /// Orchestrates the gameplay loop:
    ///   1. Wait for launch input
    ///   2. Charge slingshot
    ///   3. Release → player travels downhill
    ///   4. Player steers left/right
    ///   5. Run ends on crash or momentum loss
    ///   
    /// SRP - only coordinates sub-systems, doesn't implement physics/input/collision itself.
    /// DIP - depends entirely on interfaces, resolved via VContainer.
    /// 
    /// Implements VContainer's ITickable for Update loop and IStartable for initialization.
    /// </summary>
    public sealed class GameplayFlow : IStartable, ITickable, IFixedTickable
    {
        private readonly IInputHandler _input;
        private readonly IRunSession _runSession;
        private readonly IGameStateManager _gameState;
        private readonly IPlayerDataService _playerDataService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;

        private RunPhase _currentPhase = RunPhase.WaitingToLaunch;
        private Vector3 _startPosition;
        private PlayerData _playerData;

        private enum RunPhase
        {
            WaitingToLaunch,
            Charging,
            Running,
            Ended
        }

        public GameplayFlow(
            IInputHandler input,
            IRunSession runSession,
            IGameStateManager gameState,
            IPlayerDataService playerDataService,
            GameSettings settings,
            PlayerManager player)
        {
            _input = input;
            _runSession = runSession;
            _gameState = gameState;
            _playerDataService = playerDataService;
            _settings = settings;
            _player = player;
        }

        public void Start()
        {
            _playerData = _playerDataService.Load();
            _startPosition = _player.transform.position;

            // Initialize player sub-systems with current upgrade levels
            _player.Initialize(_settings, _playerData);

            // Subscribe to run-ending events
            _player.CollisionHandler.OnCrash += HandleCrash;
            _player.CollisionHandler.OnCoinCollected += HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost += HandleMomentumLost;

            // Enable input
            _input.Enable();
            _currentPhase = RunPhase.WaitingToLaunch;

            Debug.Log("[GameplayFlow] Ready. Press Space/Tap to start charging slingshot.");
        }

        /// <summary>
        /// Called every frame by VContainer. Handles input and phase transitions.
        /// </summary>
        public void Tick()
        {
            switch (_currentPhase)
            {
                case RunPhase.WaitingToLaunch:
                    TickWaitingToLaunch();
                    break;

                case RunPhase.Charging:
                    TickCharging();
                    break;

                case RunPhase.Running:
                    TickRunning();
                    break;

                case RunPhase.Ended:
                    // Waiting for retry input or UI interaction
                    break;
            }
        }

        /// <summary>
        /// Called every fixed update by VContainer. Handles physics.
        /// </summary>
        public void FixedTick()
        {
            if (_currentPhase != RunPhase.Running) return;

            float dt = Time.fixedDeltaTime;

            // Apply continuous physics forces
            _player.Motor.ApplyDownhillForce(dt);
            _player.Motor.ApplyDrag(dt);
            _player.Motor.Steer(_input.HorizontalInput, dt);

            // Track distance
            float distance = Vector3.Distance(_startPosition, _player.transform.position);
            _runSession.UpdateDistance(distance);

            // Monitor momentum
            _player.MomentumTracker.UpdateSpeed(_player.Motor.CurrentSpeed);
        }

        // --- Phase Handlers ---

        private void TickWaitingToLaunch()
        {
            if (_input.LaunchPressed)
            {
                _player.Slingshot.StartCharging();
                _currentPhase = RunPhase.Charging;
                Debug.Log("[GameplayFlow] Slingshot charging...");
            }
        }

        private void TickCharging()
        {
            // Access the concrete type to call UpdateCharge (not on the interface
            // because it's a frame-tick concern, not a consumer concern)
            if (_player.Slingshot is Slingshot.SlingshotController slingshot)
            {
                slingshot.UpdateCharge(Time.deltaTime);
            }

            // Release on button release or after max charge
            if (_input.LaunchPressed || _player.Slingshot.ChargePercent >= 1f)
            {
                _player.Slingshot.Release(_player.Rigidbody);
                _runSession.StartRun();
                _player.MomentumTracker.StartTracking();
                _currentPhase = RunPhase.Running;
                Debug.Log("[GameplayFlow] Launched! Run started.");
            }
        }

        private void TickRunning()
        {
            // Running phase logic handled in FixedTick for physics
            // This tick is available for UI updates, effects, etc.
        }

        // --- Event Handlers ---

        private void HandleCrash()
        {
            if (_currentPhase != RunPhase.Running) return;

            Debug.Log("[GameplayFlow] Player crashed!");
            EndRun();
        }

        private void HandleMomentumLost()
        {
            if (_currentPhase != RunPhase.Running) return;

            Debug.Log("[GameplayFlow] Player lost momentum!");
            EndRun();
        }

        private void HandleCoinCollected(int amount)
        {
            if (_currentPhase != RunPhase.Running) return;

            int value = _settings.BaseCoinValue + _settings.CoinValuePerLevel * (_playerData.CoinValueLevel - 1);
            _runSession.AddCoins(amount * value);
        }

        private void EndRun()
        {
            _currentPhase = RunPhase.Ended;
            _input.Disable();
            _player.Motor.Halt();
            _player.MomentumTracker.StopTracking();
            _runSession.EndRun();

            // Persist coins earned this run
            _playerData.Coins += _runSession.CoinsCollected;
            _playerDataService.Save(_playerData);

            _gameState.TransitionTo(GameState.GameOver);

            Debug.Log($"[GameplayFlow] Run ended. Coins earned: {_runSession.CoinsCollected}, " +
                      $"Distance: {_runSession.DistanceTraveled:F1}m, " +
                      $"Total coins: {_playerData.Coins}");
        }
    }
}
