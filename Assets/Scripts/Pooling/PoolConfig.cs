using System;
using System.Collections.Generic;
using UnityEngine;

namespace SledSurfers.Core.Pooling
{
    [CreateAssetMenu(fileName = "PoolConfig", menuName = "SledSurfers/Pool Config")]
    public class PoolConfig : ScriptableObject
    {
        [Serializable]
        public class PoolData
        {
            public PoolObjectType Type;
            public GameObject Prefab;
            public int InitialCount;
        }

        public List<PoolData> Pools = new();
    }
}
