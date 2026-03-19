using System;
using System.Threading.Tasks;

namespace SledSurfers.Core.Interfaces
{
    public interface ISceneLoader
    {
        event Action<string> OnSceneLoadStarted;
        event Action<string> OnSceneLoadCompleted;

        Task LoadSceneAsync(string sceneName);
        Task LoadSceneAdditiveAsync(string sceneName);
        Task UnloadSceneAsync(string sceneName);
        Task LoadGameplayWithUI();
    }
}