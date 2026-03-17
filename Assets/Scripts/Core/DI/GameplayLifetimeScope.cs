using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Services;
using SledSurfers.Gameplay;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerManager _playerManager;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameplayLifetimeScope...");

          
            builder.Register<RunSession>(Lifetime.Scoped)
                .As<IRunSession>();


            builder.Register<SimpleInputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();

            // Scene MonoBehaviour references
            builder.RegisterComponent(_playerManager);

            // Gameplay orchestrator entry point
            builder.RegisterEntryPoint<Gameplay.GameplayFlow>();
        }
    }
}
