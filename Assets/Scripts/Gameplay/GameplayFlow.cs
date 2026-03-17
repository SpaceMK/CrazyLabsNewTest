using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Camera;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer.Unity;

namespace SledSurfers.Gameplay
{
    /// <summary>
    /// Slim orchestrator that ONLY manages phase transitions.
    /// 
    /// Responsibilities:
    ///   - WaitingToLaunch → Charging → Running → Ended
    ///   - Delegates all actual work to specialized systems
    ///   
    /// Does NOT:
    ///   - Handle physics (PlayerPhysicsController does this)
    ///   - Track distance (RunSession does this)
    ///   - Feed camera (Camera subscribes to Motor directly)
    ///   - Track momentum (MomentumTracker subscribes to Motor directly)
    /// </summary>
    public sealed class GameplayFlow : IStartable, ITickable
    {
        private readonly IInputHandler _input;
        private readonly IGameStateManager _gameState;
        private readonly IPlayerDataService _playerDataService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;
       

        private RunSession _runSession;
        private PlayerData _playerData;
        private Vector3 _startPosition;
        private RunPhase _currentPhase = RunPhase.WaitingToLaunch;

        private enum RunPhase
        {
            WaitingToLaunch,
            Charging,
            Running,
            Ended
        }

        public GameplayFlow(
            IInputHandler input,
            IGameStateManager gameState,
            IPlayerDataService playerDataService,
            GameSettings settings,
            PlayerManager player
           )
        {
            _input = input;
            _gameState = gameState;
            _playerDataService = playerDataService;
            _settings = settings;
            _player = player;
        }

        public void Start()
        {
            _playerData = _playerDataService.Load();
            _startPosition = _player.transform.position;

            // Initialize player (creates motor, slingshot, momentum tracker)
            _player.Initialize(_settings, _playerData, _input);

            

            // Create run session
            _runSession = new RunSession(_player.transform);

            // Subscribe to run-ending events
            _player.CollisionHandler.OnCrash += HandleCrash;
            _player.CollisionHandler.OnCoinCollected += HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost += HandleMomentumLost;

            // Enable input
            _input.Enable();
            _currentPhase = RunPhase.WaitingToLaunch;

            Debug.Log("[GameplayFlow] Ready. Press Space/Tap to launch.");
        }

        /// <summary>
        /// Only handles input and phase transitions - no physics here.
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
                    break;
            }
        }

        private void TickWaitingToLaunch()
        {
            if (_input.LaunchPressed)
            {
                _player.Slingshot.StartCharging();
                _currentPhase = RunPhase.Charging;
                Debug.Log("[GameplayFlow] Charging...");
            }
        }

        private void TickCharging()
        {
            _player.Slingshot.UpdateCharge(Time.deltaTime);

            // Release on button release or max charge
            if (_input.LaunchReleased || _player.Slingshot.ChargePercent >= 1f)
            {
                // Get force from slingshot, apply via motor
                Vector3 force = _player.Slingshot.Release();
                _player.Motor.Launch(force);

                // Start tracking
                _runSession.StartRun();
                _player.MomentumTracker.StartTracking();

                _currentPhase = RunPhase.Running;
                Debug.Log("[GameplayFlow] Launched!");
            }
        }

        private void TickRunning()
        {
            // Distance tracking
            _runSession.Tick();

            // Physics handled by PlayerPhysicsController (subscribes to Motor.OnLaunched)
            // Camera handled by PlayerCameraController (subscribes to Motor.OnSpeedChanged)
            // Momentum handled by MomentumTracker (subscribes to Motor.OnSpeedChanged)
        }

        private void HandleCrash()
        {
            if (_currentPhase != RunPhase.Running) return;

            Debug.Log("[GameplayFlow] Crashed!");
            EndRun();
        }

        private void HandleMomentumLost()
        {
            if (_currentPhase != RunPhase.Running) return;

            Debug.Log("[GameplayFlow] Momentum lost!");
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

            // Persist coins
            _playerData.Coins += _runSession.CoinsCollected;
            _playerDataService.Save(_playerData);

            _gameState.TransitionTo(GameState.GameOver);

            Debug.Log($"[GameplayFlow] Run ended. Distance: {_runSession.DistanceTraveled:F1}m, " +
                      $"Coins: {_runSession.CoinsCollected}");
        }
    }
}
