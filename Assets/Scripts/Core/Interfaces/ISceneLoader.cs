using Cysharp.Threading.Tasks;
using System;

namespace SledSurfers.Core.Interfaces
{
    public interface ISceneLoader
    {
        UniTask LoadSceneAsync(string sceneName, IProgress<float> progress = null);
        UniTask UnloadSceneAsync(string sceneName);
    }
}
