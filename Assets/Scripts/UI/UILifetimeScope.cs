using SledSurfers.Core.DI;
using SledSurfers.UI.Screens;
using UnityEngine;
using VContainer;
using VContainer.Unity;

namespace SledSurfers.UI
{
    /// <summary>
    /// Lifetime scope for UI scene.
    /// Child of GameLifetimeScope - inherits UIService.
    /// </summary>
    public class UILifetimeScope : LifetimeScope
    {
        [SerializeField] private StartMenuScreen _startMenuScreen;
        [SerializeField] private HUDScreen _hudScreen;
        [SerializeField] private GameOverScreen _gameOverScreen;

        protected override void Configure(IContainerBuilder builder)
        {
            Debug.Log("[DI] Configuring UILifetimeScope...");

            if (_startMenuScreen != null)
                builder.RegisterComponent(_startMenuScreen);

            if (_hudScreen != null)
                builder.RegisterComponent(_hudScreen);

            if (_gameOverScreen != null)
                builder.RegisterComponent(_gameOverScreen);
        }
    }
}