using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Level;
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
    /// </summary>
    public sealed class GameplayFlow : IStartable, ITickable
    {
        private readonly IInputHandler _input;
        private readonly IGameStateManager _gameState;
        private readonly IPlayerDataService _playerDataService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;
        private readonly CoinLevelManager _coinLevelManager;
       //private readonly ObstacleLevelManager _obstacleLevelManager;

        private PlayerData _playerData;
        private Vector3 _startPosition;
        private int _coinsCollected;
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
            PlayerManager player,
            CoinLevelManager coinLevelManager)
           
        {
            _input = input;
            _gameState = gameState;
            _playerDataService = playerDataService;
            _settings = settings;
            _player = player;
            _coinLevelManager = coinLevelManager;
           
        }

        public void Start()
        {
            _playerData = _playerDataService.Load();
            _startPosition = _player.transform.position;

            // Initialize player
            _player.Initialize(_settings, _playerData, _input);

            // Subscribe to events
            _player.CollisionHandler.OnCrash += HandleCrash;
            _player.CollisionHandler.OnCoinCollected += HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost += HandleMomentumLost;

            // Enable input
            _input.Enable();
            _currentPhase = RunPhase.WaitingToLaunch;

            Debug.Log("[GameplayFlow] Ready. Press Space/Tap to launch.");
        }

        public void Tick()
        {
            // Debug restart
            if (UnityEngine.Input.GetKeyDown(KeyCode.R))
            {
                Retry();
                return;
            }

            switch (_currentPhase)
            {
                case RunPhase.WaitingToLaunch:
                    TickWaitingToLaunch();
                    break;

                case RunPhase.Charging:
                    TickCharging();
                    break;

                case RunPhase.Running:
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

            if (_input.LaunchReleased)
            {
                Vector3 force = _player.Slingshot.Release();
                _player.Motor.Launch(force);
                _player.MomentumTracker.StartTracking();

                _coinsCollected = 0;
                _currentPhase = RunPhase.Running;
                Debug.Log("[GameplayFlow] Launched!");
            }
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
            _coinsCollected += amount * value;

            Debug.Log($"[GameplayFlow] Coin collected! Total: {_coinsCollected}");
        }

        private void EndRun()
        {
            _currentPhase = RunPhase.Ended;
            _input.Disable();
            _player.Motor.Halt();
            _player.MomentumTracker.StopTracking();

            // Calculate distance
            float distance = _player.FinalDistance;

            // Save coins
            _playerData.Coins += _coinsCollected;
            _playerDataService.Save(_playerData);

            _gameState.TransitionTo(GameState.GameOver);

            Debug.Log($"[GameplayFlow] Run ended. Distance: {distance:F1}m, Coins: {_coinsCollected}");
        }

        /// <summary>
        /// Call to restart the run.
        /// </summary>
        public void Retry()
        {
            // Reset player
            _player.ResetPlayer(_startPosition);

            // Reset level objects
            _coinLevelManager.Reset();
            //_obstacleLevelManager.Reset();

            // Reset state
            _coinsCollected = 0;
            _currentPhase = RunPhase.WaitingToLaunch;
            _input.Enable();

            Debug.Log("[GameplayFlow] Retry - ready to launch.");
        }
    }
}
