using SledSurfers.Core.Interfaces;
using SledSurfers.Data.Models;
using UnityEngine;

namespace SledSurfers.Core.Services
{
    /// <summary>
    /// PlayerPrefs-backed persistence for player data.
    /// Implements IPlayerDataService - can be swapped for file/cloud save (OCP).
    /// </summary>
    public sealed class PlayerPrefsDataService : IPlayerDataService
    {
        private const string StorageKey = "SledSurfers_PlayerData";

        public PlayerData Load()
        {
            if (!PlayerPrefs.HasKey(StorageKey))
            {
                return PlayerData.Default;
            }

            var json = PlayerPrefs.GetString(StorageKey);
            
            try
            {
                return JsonUtility.FromJson<PlayerData>(json) ?? PlayerData.Default;
            }
            catch
            {
                Debug.LogWarning("[PlayerData] Corrupted save data, returning default.");
                return PlayerData.Default;
            }
        }

        public void Save(PlayerData data)
        {
            var json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString(StorageKey, json);
            PlayerPrefs.Save();
        }

        public void Reset()
        {
            PlayerPrefs.DeleteKey(StorageKey);
            PlayerPrefs.Save();
        }
    }
}
