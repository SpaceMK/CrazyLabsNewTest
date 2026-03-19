using SledSurfers.Core.Interfaces;
using SledSurfers.Core.SceneManagement;
using SledSurfers.Core.Services;
using SledSurfers.Data.ScriptableObjects;
using SledSurfers.Data.Upgrades;
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
        [SerializeField] private UpgradeConfig _upgradeConfig;

        protected override void Awake()
        {
            DontDestroyOnLoad(gameObject);
            base.Awake();
        }

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameLifetimeScope...");

            builder.RegisterInstance(_gameSettings);
            builder.RegisterInstance(_upgradeConfig);

            builder.Register<SceneLoaderService>(Lifetime.Singleton)
                .As<ISceneLoader>();

            builder.Register<GameStateManager>(Lifetime.Singleton)
                .As<IGameStateManager>();

            builder.Register<PlayerPrefsDataService>(Lifetime.Singleton)
                .As<IPlayerDataService>();

            builder.Register<ScriptableObjectUpgradeConfigProvider>(Lifetime.Singleton)
                .As<IUpgradeConfigProvider>();

            // Upgrade Service - uses config provider
            builder.Register<UpgradeService>(Lifetime.Singleton)
                .As<IUpgradeService>();

            builder.Register<PoolManager>(Lifetime.Singleton)
                .As<IPoolManager>();

            builder.Register<UIService>(Lifetime.Singleton)
                .As<IUIService>()
                .AsSelf();

            builder.RegisterEntryPoint<BootstrapFlow>();
        }
    }
}