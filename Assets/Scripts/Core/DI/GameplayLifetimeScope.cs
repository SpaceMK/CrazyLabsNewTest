using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay;
using SledSurfers.Gameplay.Level;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{

    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private LevelPoolProvider _levelPoolProvider;
        [SerializeField] private ObstacleLevelManager _obstacleLevelManager;
        [SerializeField] private CoinLevelManager _coinLevelManager;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameplayLifetimeScope...");

            builder.Register<SimpleInputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();

            builder.RegisterComponent(_playerManager);

            if (_levelPoolProvider != null)
                builder.RegisterComponent(_levelPoolProvider);

           
            if (_obstacleLevelManager != null)
            {
                builder.RegisterComponent(_obstacleLevelManager)
                    .As<ISpawnPositionProvider>()
                    .As<ILevelManager>();
            }

          
            if (_coinLevelManager != null)
            {
                builder.RegisterComponent(_coinLevelManager)
                    .As<ICoinDespawner>()
                    .As<ILevelManager>();
            }

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
