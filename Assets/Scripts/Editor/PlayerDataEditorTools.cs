using UnityEditor;
using UnityEngine;

namespace SledSurfers.Editor
{
    public static class PlayerDataEditorTools
    {
        [MenuItem("SledSurfers/Reset Player Data")]
        public static void ResetPlayerData()
        {
            PlayerPrefs.DeleteKey("SledSurfers_PlayerData");
            PlayerPrefs.Save();
            Debug.Log("[Editor] Player data reset successfully.");
        }

        [MenuItem("SledSurfers/Log Player Data")]
        public static void LogPlayerData()
        {
            if (PlayerPrefs.HasKey("SledSurfers_PlayerData"))
            {
                var json = PlayerPrefs.GetString("SledSurfers_PlayerData");
                Debug.Log($"[Editor] Player Data: {json}");
            }
            else
            {
                Debug.Log("[Editor] No saved player data found.");
            }
        }
    }
}
