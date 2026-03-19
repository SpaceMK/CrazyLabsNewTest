using SledSurfers.Core.Interfaces;
using SledSurfers.Core.SceneManagement;
using SledSurfers.Core.Services;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.UI.Interfaces;
using SledSurfers.UI.Services;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    public class GameLifetimeScope : LifetimeScope
    {
        [SerializeField] private GameSettings _gameSettings;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameLifetimeScope...");

            builder.RegisterInstance(_gameSettings);

            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoader>();

            builder.Register<GameStateManager>(Lifetime.Singleton)
                .As<IGameStateManager>();

            builder.Register<PlayerPrefsDataService>(Lifetime.Singleton)
                .As<IPlayerDataService>();

            builder.Register<UpgradeService>(Lifetime.Singleton)
                .As<IUpgradeService>();

            builder.Register<PoolManager>(Lifetime.Singleton)
                .As<IPoolManager>();

            // UI Service - singleton for cross-scene communication
            builder.Register<UIService>(Lifetime.Singleton)
                .As<IUIService>()
                .AsSelf();

            builder.RegisterEntryPoint<BootstrapFlow>();
        }
    }
}
