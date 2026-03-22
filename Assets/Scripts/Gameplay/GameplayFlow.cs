using System.Collections.Generic;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Gameplay.Player;
using SledSurfers.UI.Interfaces;
using UnityEngine;
using VContainer.Unity;

namespace SledSurfers.Gameplay
{
    public sealed class GameplayFlow : IStartable, ITickable
    {
        private readonly IInputHandler _input;
        private readonly IGameStateManager _gameState;
        private readonly IPlayerDataService _playerDataService;
        private readonly IUIService _uiService;
        private readonly GameSettings _settings;
        private readonly PlayerManager _player;
        private readonly IReadOnlyList<ILevelManager> _levelManagers;

        private PlayerData _playerData;
        private Vector3 _startPosition;
        private RunSession _currentRun;

        private enum FlowPhase
        {
            StartMenu,
            Upgrades,
            Playing,
            Ended
        }

        private FlowPhase _currentPhase = FlowPhase.StartMenu;

        public GameplayFlow(
            IInputHandler input,
            IGameStateManager gameState,
            IPlayerDataService playerDataService,
            IUIService uiService,
            GameSettings settings,
            PlayerManager player,
            IReadOnlyList<ILevelManager> levelManagers)
        {
            _input = input;
            _gameState = gameState;
            _playerDataService = playerDataService;
            _uiService = uiService;
            _settings = settings;
            _player = player;
            _levelManagers = levelManagers;
        }

        public void Start()
        {
            _playerData = _playerDataService.Load();
            _startPosition = _player.transform.position;

            _player.Initialize(_settings, _playerData, _input);

            ResetAllLevels();

            _uiService.OnPlayClicked += HandlePlayClicked;
            _uiService.OnMainMenuClicked += HandleMainMenu;
            _uiService.OnUpgradesClicked += HandleUpgradesClicked;
            _uiService.OnCloseUpgradesClicked += HandleCloseUpgrades;

            _input.Disable();
            _currentPhase = FlowPhase.StartMenu;
            _gameState.TransitionTo(GameState.StartMenu);

            Debug.Log("[GameplayFlow] Ready. Showing start menu.");
        }

        public void Tick()
        {
            // Tick input first — reads raw mouse/touch state and updates
            // DragDelta, DragStarted, DragEnded for the current frame.
            // This must run before RunSession.Tick() consumes those values.
            _input.Tick();

            if (_currentPhase == FlowPhase.Playing)
            {
                _currentRun?.Tick();
            }
        }

        // === Navigation Handlers ===

        private void HandlePlayClicked()
        {
            if (_currentPhase == FlowPhase.StartMenu)
            {
                StartGame();
            }
            else if (_currentPhase == FlowPhase.Ended)
            {
                Retry();
            }
        }

        private void HandleUpgradesClicked()
        {
            if (_currentPhase == FlowPhase.StartMenu)
            {
                _currentPhase = FlowPhase.Upgrades;
                _gameState.TransitionTo(GameState.Upgrades);
                Debug.Log("[GameplayFlow] Showing upgrades.");
            }
        }

        private void HandleCloseUpgrades()
        {
            if (_currentPhase == FlowPhase.Upgrades)
            {
                _playerData = _playerDataService.Load();
                _player.Initialize(_settings, _playerData, _input);

                _currentPhase = FlowPhase.StartMenu;
                _gameState.TransitionTo(GameState.StartMenu);
                Debug.Log("[GameplayFlow] Closed upgrades, back to start menu.");
            }
        }

        private void HandleMainMenu()
        {
            DisposeCurrentRun();

            _player.ResetPlayer(_startPosition);
            ResetAllLevels();

            _currentPhase = FlowPhase.StartMenu;
            _input.Disable();
            _gameState.TransitionTo(GameState.StartMenu);

            Debug.Log("[GameplayFlow] Returned to main menu.");
        }

        // === Session Lifecycle ===

        private void StartGame()
        {
            _playerData = _playerDataService.Load();
            _player.Initialize(_settings, _playerData, _input);

            _currentRun = new RunSession(_input, _uiService, _settings, _player);
            _currentRun.OnRunEnded += HandleRunEnded;
            _currentRun.Begin(_playerData);

            _currentPhase = FlowPhase.Playing;
            _gameState.TransitionTo(GameState.Playing);

            Debug.Log("[GameplayFlow] Game started.");
        }

        private void HandleRunEnded(float distance, int coinsCollected)
        {
            _currentPhase = FlowPhase.Ended;

            _playerData.Coins += coinsCollected;
            _playerDataService.Save(_playerData);

            _gameState.TransitionTo(GameState.GameOver, new GameStateData
            {
                Distance = distance,
                CoinsCollected = coinsCollected
            });

            Debug.Log($"[GameplayFlow] Run ended. Distance: {distance:F1}m, Coins: {coinsCollected}");
        }

        private void Retry()
        {
            DisposeCurrentRun();

            _playerData = _playerDataService.Load();
            _player.ResetPlayer(_startPosition);
            ResetAllLevels();

            _currentRun = new RunSession(_input, _uiService, _settings, _player);
            _currentRun.OnRunEnded += HandleRunEnded;
            _currentRun.Begin(_playerData);

            _currentPhase = FlowPhase.Playing;
            _gameState.TransitionTo(GameState.Playing);

            Debug.Log("[GameplayFlow] Retry - new run started.");
        }

        // === Helpers ===

        private void ResetAllLevels()
        {
            foreach (var manager in _levelManagers)
            {
                manager.Reset();
            }
        }

        private void DisposeCurrentRun()
        {
            if (_currentRun != null)
            {
                _currentRun.OnRunEnded -= HandleRunEnded;
                _currentRun.Dispose();
                _currentRun = null;
            }
        }
    }
}