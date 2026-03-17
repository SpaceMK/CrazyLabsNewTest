using System;
using Cysharp.Threading.Tasks;
using SledSurfers.Core.Interfaces;
using UnityEngine.SceneManagement;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Concrete scene loader using Unity's SceneManager.
    /// Implements ISceneLoader - can be swapped for addressable-based loader later (OCP).
    /// </summary>
    public sealed class SceneLoaderService : ISceneLoader
    {
        public async UniTask LoadSceneAsync(string sceneName, IProgress<float> progress = null)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            
            if (operation == null)
            {
                throw new InvalidOperationException($"Failed to start loading scene: {sceneName}");
            }

            operation.allowSceneActivation = false;

            while (operation.progress < 0.9f)
            {
                progress?.Report(operation.progress / 0.9f);
                await UniTask.Yield();
            }

            progress?.Report(1f);
            operation.allowSceneActivation = true;

            await UniTask.WaitUntil(() => operation.isDone);
        }

        public async UniTask UnloadSceneAsync(string sceneName)
        {
            var operation = SceneManager.UnloadSceneAsync(sceneName);
            
            if (operation != null)
            {
                await UniTask.WaitUntil(() => operation.isDone);
            }
        }
    }
}
