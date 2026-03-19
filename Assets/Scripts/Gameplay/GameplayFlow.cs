using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Level;
using SledSurfers.Gameplay.Player;
using SledSurfers.UI.Interfaces;
using UnityEngine;
using VContainer.Unity;

namespace SledSurfers.Gameplay
{
    /// <summary>
    /// Orchestrates the gameplay loop.
    /// Decoupled from UI - communicates only via IGameStateManager and IUIService events.
    /// </summary>
    public sealed class GameplayFlow : IStartable, ITickable
    {
        private readonly IInputHandler _input;
        private readonly IGameStateManager _gameState;
        private readonly IPlayerDataService _playerDataService;
        private readonly IUIService _uiService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;
        private readonly CoinLevelManager _coinLevelManager;

        private PlayerData _playerData;
        private Vector3 _startPosition;
        private int _coinsCollected;
        private RunPhase _currentPhase = RunPhase.StartMenu;

        private enum RunPhase
        {
            StartMenu,
            Upgrades,
            WaitingToLaunch,
            Charging,
            Running,
            Ended
        }

        public GameplayFlow(
            IInputHandler input,
            IGameStateManager gameState,
            IPlayerDataService playerDataService,
            IUIService uiService,
            GameSettings settings,
            PlayerManager player,
            CoinLevelManager coinLevelManager)
        {
            _input = input;
            _gameState = gameState;
            _playerDataService = playerDataService;
            _uiService = uiService;
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

            // Subscribe to player events
            _player.CollisionHandler.OnCrash += HandleCrash;
            _player.CollisionHandler.OnCoinCollected += HandleCoinCollected;
            _player.MomentumTracker.OnMomentumLost += HandleMomentumLost;

            // Subscribe to UI events
            _uiService.OnPlayClicked += HandlePlayClicked;
            _uiService.OnMainMenuClicked += HandleMainMenu;
            _uiService.OnUpgradesClicked += HandleUpgradesClicked;
            _uiService.OnCloseUpgradesClicked += HandleCloseUpgrades;

            // Start with input disabled
            _input.Disable();
            _currentPhase = RunPhase.StartMenu;

            // Transition to StartMenu state
            _gameState.TransitionTo(GameState.StartMenu);

            Debug.Log("[GameplayFlow] Ready. Showing start menu.");
        }

        public void Tick()
        {
            Debug.Log($"[GameplayFlow] Current phase: {_currentPhase}");
            switch (_currentPhase)
            {
                case RunPhase.StartMenu:
                case RunPhase.Upgrades:
                    break;

                case RunPhase.Ended:
                   
                    break;

                case RunPhase.WaitingToLaunch:
                    TickWaitingToLaunch();
                    break;

                case RunPhase.Charging:
                    TickCharging();
                    break;

                case RunPhase.Running:
                    TickRunning();
                    break;
            }
        }

        private void HandlePlayClicked()
        {
            if (_currentPhase == RunPhase.StartMenu)
            {
                StartGame();
            }
            else if (_currentPhase == RunPhase.Ended)
            {
                Retry();
            }
        }

        private void HandleUpgradesClicked()
        {
            if (_currentPhase == RunPhase.StartMenu)
            {
                _currentPhase = RunPhase.Upgrades;
                _gameState.TransitionTo(GameState.Upgrades);
                Debug.Log("[GameplayFlow] Showing upgrades.");
            }
        }

        private void HandleCloseUpgrades()
        {
            if (_currentPhase == RunPhase.Upgrades)
            {
                // Reload player data in case upgrades were applied
                _playerData = _playerDataService.Load();

                // Reinitialize player with new stats
                _player.Initialize(_settings, _playerData, _input);

                _currentPhase = RunPhase.StartMenu;
                _gameState.TransitionTo(GameState.StartMenu);
                Debug.Log("[GameplayFlow] Closed upgrades, back to start menu.");
            }
        }

        private void StartGame()
        {
            // Reload player data to get latest upgrades
            _playerData = _playerDataService.Load();
            _player.Initialize(_settings, _playerData, _input);

            _input.Enable();
            _coinsCollected = 0;
            _currentPhase = RunPhase.WaitingToLaunch;

            _gameState.TransitionTo(GameState.Playing);

            Debug.Log("[GameplayFlow] Game started. Press to launch.");
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

                _currentPhase = RunPhase.Running;
                Debug.Log("[GameplayFlow] Launched!");
            }
        }

        private void TickRunning()
        {
            _player.CalculateDistance();
            _uiService.UpdateDistance(_player.FinalDistance);
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

            _uiService.UpdateCoins(_coinsCollected);
            Debug.Log($"[GameplayFlow] Coin collected! Total: {_coinsCollected}");
        }

        private void EndRun()
        {
            _currentPhase = RunPhase.Ended;
            _input.Disable();
            _player.Motor.Halt();
            _player.MomentumTracker.StopTracking();

            _player.CalculateDistance();
            float distance = _player.FinalDistance;

            _playerData.Coins += _coinsCollected;
            _playerDataService.Save(_playerData);

            _gameState.TransitionTo(GameState.GameOver, new GameStateData
            {
                Distance = distance,
                CoinsCollected = _coinsCollected
            });

            Debug.Log($"[GameplayFlow] Run ended. Distance: {distance:F1}m, Coins: {_coinsCollected}");
        }

        private void HandleMainMenu()
        {
            Debug.Log("[GameplayFlow] Main menu requested.");
        }

        private void Retry()
        {
            // Reload player data to get latest coin count
            _playerData = _playerDataService.Load();

            _player.ResetPlayer(_startPosition);
            _coinLevelManager.Reset();

            _coinsCollected = 0;
            _currentPhase = RunPhase.WaitingToLaunch;
            _input.Enable();

            _gameState.TransitionTo(GameState.Playing);

            Debug.Log("[GameplayFlow] Retry - ready to launch.");
        }
    }
}