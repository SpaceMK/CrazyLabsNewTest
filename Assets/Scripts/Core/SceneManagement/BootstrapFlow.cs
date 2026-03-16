using System;
using Cysharp.Threading.Tasks;
using SledSurfers.Core.Interfaces;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;
using VContainer.Unity;

namespace SledSurfers.Core.SceneManagement
{
    /// <summary>
    /// Bootstrap entry point. Runs once when the game starts.
    /// Responsibility: initialize core systems, then transition to the gameplay scene.
    /// 
    /// Implements IStartable (VContainer) for automatic lifecycle management.
    /// All dependencies injected via constructor - DIP.
    /// </summary>
    public sealed class BootstrapFlow : IStartable
    {
        private readonly ISceneLoader _sceneLoader;
        private readonly IGameStateManager _gameStateManager;
        private readonly IPlayerDataService _playerDataService;
        private readonly GameSettings _settings;

        public BootstrapFlow(
            ISceneLoader sceneLoader,
            IGameStateManager gameStateManager,
            IPlayerDataService playerDataService,
            GameSettings settings)
        {
            _sceneLoader = sceneLoader;
            _gameStateManager = gameStateManager;
            _playerDataService = playerDataService;
            _settings = settings;
        }

        public void Start()
        {
            RunBootstrapAsync().Forget();
        }

        private async UniTaskVoid RunBootstrapAsync()
        {
            Debug.Log("[Bootstrap] Starting bootstrap sequence...");

            try
            {
                // Step 1: Validate / initialize player data
                var playerData = _playerDataService.Load();
                Debug.Log($"[Bootstrap] Player data loaded. Coins: {playerData.Coins}");

                // Step 2: Any additional initialization (analytics, remote config, etc.)
                // Future: await _remoteConfigService.FetchAsync();
                // Future: await _analyticsService.InitializeAsync();

                // Step 3: Transition state
                _gameStateManager.TransitionTo(GameState.Gameplay);

                // Step 4: Load gameplay scene
                var progress = new Progress<float>(p => 
                    Debug.Log($"[Bootstrap] Loading gameplay scene: {p:P0}"));
                
                await _sceneLoader.LoadSceneAsync(_settings.GameplaySceneName, progress);
                
                Debug.Log("[Bootstrap] Gameplay scene loaded successfully.");
            }
            catch (Exception ex)
            {
                Debug.LogError($"[Bootstrap] Failed to complete bootstrap: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}
