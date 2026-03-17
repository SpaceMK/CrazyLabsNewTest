using System;
using Cysharp.Threading.Tasks;

namespace SledSurfers.Core.Interfaces
{
    /// <summary>
    /// Abstraction for scene loading operations.
    /// Follows ISP - only exposes what consumers need.
    /// </summary>
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, IProgress<float> progress = null);
        UniTask UnloadSceneAsync(string sceneName);
    }
}
