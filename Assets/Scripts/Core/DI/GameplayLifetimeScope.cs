using SledSurfers.Core.Interfaces;
using SledSurfers.Gameplay;
using SledSurfers.Gameplay.Level;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    /// <summary>
    /// Lifetime scope for Gameplay scene.
    /// Child of GameLifetimeScope — inherits PoolManager, UIService, GameStateManager, etc.
    /// 
    /// Registration order matters: ObstacleLevelManager BEFORE CoinLevelManager
    /// ensures obstacles spawn first, then coins avoid their positions.
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerManager _playerManager;
        [SerializeField] private LevelPoolProvider _levelPoolProvider;
        [SerializeField] private ObstacleLevelManager _obstacleLevelManager;
        [SerializeField] private CoinLevelManager _coinLevelManager;

        protected override void Configure(IContainerBuilder builder)
        {
           
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
                    .As<IEntityDespawner>()
                    .As<ILevelManager>();
            }

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}