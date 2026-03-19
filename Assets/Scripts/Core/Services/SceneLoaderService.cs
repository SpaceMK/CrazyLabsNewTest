using System;
using System.Threading.Tasks;
using SledSurfers.Core.Interfaces;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SledSurfers.Core.SceneManagement
{
    public class SceneLoaderService : ISceneLoader
    {
        public const string BootstrapScene = "Bootstrap";
        public const string MainMenuScene = "MainMenu";
        public const string GameplayScene = "Gameplay";
        public const string UIScene = "UI";

        public event Action<string> OnSceneLoadStarted;
        public event Action<string> OnSceneLoadCompleted;

        public async Task LoadSceneAsync(string sceneName)
        {
            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Loading scene: {sceneName}");

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Scene loaded: {sceneName}");
        }

        public async Task LoadSceneAdditiveAsync(string sceneName)
        {
            OnSceneLoadStarted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Loading scene additive: {sceneName}");

            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            OnSceneLoadCompleted?.Invoke(sceneName);
            Debug.Log($"[SceneLoader] Scene loaded additive: {sceneName}");
        }

        public async Task UnloadSceneAsync(string sceneName)
        {
            Debug.Log($"[SceneLoader] Unloading scene: {sceneName}");

            var operation = SceneManager.UnloadSceneAsync(sceneName);

            if (operation == null)
            {
                Debug.LogWarning($"[SceneLoader] Scene not loaded: {sceneName}");
                return;
            }

            while (!operation.isDone)
            {
                await Task.Yield();
            }

            Debug.Log($"[SceneLoader] Scene unloaded: {sceneName}");
        }

        public async Task LoadGameplayWithUI()
        {
            await LoadSceneAsync(GameplayScene);
            await LoadSceneAdditiveAsync(UIScene);
        }
    }
}