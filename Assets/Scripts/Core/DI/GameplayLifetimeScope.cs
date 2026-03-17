using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Services;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerManager _playerFacade;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameplayLifetimeScope...");

            // Per-run session tracking (scoped = fresh each scene load)
            builder.Register<RunSession>(Lifetime.Scoped)
                .As<IRunSession>();


            builder.Register<SimpleInputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();

            // Scene MonoBehaviour references
            builder.RegisterComponent(_playerFacade);

            // Gameplay orchestrator entry point
            builder.RegisterEntryPoint<Gameplay.GameplayFlow>();
        }
    }
}
