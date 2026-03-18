using SledSurfers.Core.Pooling;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    public interface IPoolManager
    {
        void InitializePool(PoolObjectType type, GameObject prefab, int initialCount, Transform parent);
        IPoolingObject Get(PoolObjectType type);
        void Return(IPoolingObject poolObject);
        void Clear();
    }
}
