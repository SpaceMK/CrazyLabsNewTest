using SledSurfers.Core.Interfaces;
using SledSurfers.Core.Pooling;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// Plain C# class wrapping a pooled GameObject.
    /// NOT a MonoBehaviour.
    /// </summary>
    public class PoolingObject : IPoolingObject
    {
        public GameObject GameObject { get; }
        public PoolObjectType Type { get; }

        public PoolingObject(PoolObjectType type, GameObject gameObject)
        {
            Type = type;
            GameObject = gameObject;
        }
    }
}
