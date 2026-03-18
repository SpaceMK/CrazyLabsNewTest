using System;
using UnityEngine;

namespace SledSurfers.Data.ScriptableObjects
{
    /// <summary>
    /// Configuration for level spawning.
    /// </summary>
    [CreateAssetMenu(fileName = "SpawnConfig", menuName = "SledSurfers/Spawn Config")]
    public class SpawnConfig : ScriptableObject
    {
        [Header("Prefabs")]
        public GameObject CoinPrefab;
        public GameObject ObstaclePrefab;

        [Header("Pool Settings")]
        [Range(5, 50)]
        public int PrewarmCoins = 20;
        [Range(5, 50)]
        public int PrewarmObstacles = 15;

        [Header("Lane Settings")]
        public float LaneWidth = 3f;
        [Range(1, 5)]
        public int LaneCount = 3;

        [Header("Initial Spawn (Before Launch)")]
        public bool SpawnOnStart = true;
        public float InitialSpawnDistance = 100f;
        public float InitialSpawnInterval = 5f;
        public float InitialSpawnStartZ = 10f;

        [Header("Runtime Spawn (During Gameplay)")]
        public bool SpawnDuringGameplay = true;
        public float SpawnAheadDistance = 50f;
        public float SpawnInterval = 5f;
        [Range(0f, 1f)]
        public float ObstacleChance = 0.5f;

        [Header("Coin Settings")]
        public int CoinValue = 1;
        public float CoinHeight = 1f;

        [Header("Obstacle Settings")]
        public float ObstacleHeight = 0f;
    }
}