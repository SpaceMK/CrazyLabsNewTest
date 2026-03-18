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
        [SerializeField] private LevelSpawner _levelSpawner;

        protected override void Configure(IContainerBuilder builder)
        {
            builder.Register<InputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();

            builder.RegisterComponent(_playerManager);

            if (_levelSpawner != null)
            {
                builder.RegisterComponent(_levelSpawner);
            }

            builder.RegisterEntryPoint<GameplayFlow>();
        }
    }
}
