using SledSurfers.Core.Interfaces;
using SledSurfers.Core.SceneManagement;
using SledSurfers.Core.Services;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    /// <summary>
    /// Root lifetime scope that lives in the Bootstrap scene.
    /// Registers project-wide singletons. Child scopes (Gameplay, etc.) inherit these.
    /// 
    /// VContainer hierarchy:
    ///   RootLifetimeScope (Bootstrap - DontDestroyOnLoad)
    ///     └── GameplayLifetimeScope (Gameplay scene)
    /// </summary>
    public sealed class RootLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings _gameSettings;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring RootLifetimeScope...");

            // ScriptableObject instance (shared config)
            builder.RegisterInstance(_gameSettings);

            // Core services - singleton lifetime, persist across scenes
            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoader>();

            builder.Register<GameStateManager>(Lifetime.Singleton)
                .As<IGameStateManager>();

            builder.Register<PlayerPrefsDataService>(Lifetime.Singleton)
                .As<IPlayerDataService>();

            builder.Register<UpgradeService>(Lifetime.Singleton)
                .As<IUpgradeService>();

            // Entry point - kicks off the bootstrap flow
            builder.RegisterEntryPoint<BootstrapFlow>();
        }
    }
}
