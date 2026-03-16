using SledSurfers.Core.Interfaces;
using SledSurfers.Core.SceneManagement;
using SledSurfers.Core.Services;
using SledSurfers.Data.ScriptableObjects;
using UnityEngine;
using VContainer;
using VContainer.Unity;

public class RootLifetimeScope : LifetimeScope
{
    [SerializeField] private GameSettings gameSettings;

    protected override void Configure(IContainerBuilder builder)
    { 
        builder.RegisterInstance(gameSettings);
        builder.Register<SceneLoaderService>(Lifetime.Singleton).As<ISceneLoader>();
        builder.Register<GameStateManager>(Lifetime.Singleton).As<IGameStateManager>();
        builder.Register<PlayerPrefsDataService>(Lifetime.Singleton).As<IPlayerDataService>();
        builder.Register<UpgradeService>(Lifetime.Singleton).As<IUpgradeService>();
        builder.RegisterEntryPoint<BootstrapFlow>();
    }
}