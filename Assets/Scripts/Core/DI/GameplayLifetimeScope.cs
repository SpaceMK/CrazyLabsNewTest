using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Services;
using SledSurfers.Gameplay.Player;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.Core.DI
{
    /// <summary>
    /// Gameplay scene lifetime scope. Child of RootLifetimeScope.
    /// Registers per-run services that get disposed when leaving the gameplay scene.
    /// 
    /// Inherits from parent scope:
    ///   ISceneLoader, IGameStateManager, IPlayerDataService, IUpgradeService, GameSettings
    /// 
    /// Scene setup:
    ///   - Player GameObject must have PlayerFacade + PlayerCollisionHandler + Rigidbody + Collider
    ///   - Assign PlayerFacade reference in the inspector
    /// </summary>
    public sealed class GameplayLifetimeScope : LifetimeScope
    {
        [SerializeField] private PlayerManager _playerFacade;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring GameplayLifetimeScope...");

            // Per-run session tracking (scoped = fresh each scene load)
            builder.Register<RunSession>(Lifetime.Scoped)
                .As<IRunSession>();

            // Input - using simple legacy input for quick iteration
            // Swap to InputHandler when Input Actions asset is configured
            builder.Register<SimpleInputHandler>(Lifetime.Scoped)
                .As<IInputHandler>();

            // Scene MonoBehaviour references
            builder.RegisterComponent(_playerFacade);

            // Gameplay orchestrator entry point
            builder.RegisterEntryPoint<Gameplay.GameplayFlow>();
        }
    }
}
