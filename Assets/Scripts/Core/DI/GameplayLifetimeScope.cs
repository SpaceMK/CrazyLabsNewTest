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
        [SerializeField] private CoinLevelManager _coinLevelManager;
        [SerializeField] private LevelPoolProvider _levelPoolProvider;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameplayLifetimeScope...");

            builder.Register<InputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();
            builder.RegisterComponent(_playerManager);
           

            if (_levelPoolProvider != null)
            {
                builder.RegisterComponent(_levelPoolProvider);
            }


            builder.RegisterComponent(_coinLevelManager);
            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}