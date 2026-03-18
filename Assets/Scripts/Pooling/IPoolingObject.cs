using SledSurfers.Core.Pooling;
using UnityEngine;

namespace SledSurfers.Core.Interfaces
{
    public interface IPoolingObject
    {
        GameObject GameObject { get; }
        PoolObjectType Type { get; }
    }
}
