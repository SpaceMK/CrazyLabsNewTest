using UnityEngine;
using UnityEditor;

/// <summary>
/// Custom Inspector for DownhillLevelGenerator.
/// This file MUST live inside an Editor folder, e.g. Assets/Scripts/Editor/
/// </summary>
[CustomEditor(typeof(DownhillLevelGenerator))]
public class DownhillLevelGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        GUILayout.Space(12);

        DownhillLevelGenerator gen = (DownhillLevelGenerator)target;

        // ── Generate in Scene ──
        if (GUILayout.Button("Generate in Scene", GUILayout.Height(30)))
        {
            ClearExisting(gen.prefabName);
            GameObject level = gen.GenerateLevel();
            Undo.RegisterCreatedObjectUndo(level, "Generate Downhill Level");
            Selection.activeGameObject = level;
            Debug.Log("[DownhillLevelGenerator] Level generated in scene.");
        }

        // ── Generate & Save as Prefab ──
        if (GUILayout.Button("Generate & Save as Prefab", GUILayout.Height(36)))
        {
            ClearExisting(gen.prefabName);

            GameObject level = gen.GenerateLevel();

            // Ensure target folder exists
            string folderPath = "Assets/" + gen.savePath;
            if (!AssetDatabase.IsValidFolder(folderPath))
            {
                string[] parts = gen.savePath.Split('/');
                string current = "Assets";
                foreach (string part in parts)
                {
                    string next = current + "/" + part;
                    if (!AssetDatabase.IsValidFolder(next))
                        AssetDatabase.CreateFolder(current, part);
                    current = next;
                }
            }

            // Save prefab
            string prefabPath = folderPath + "/" + gen.prefabName + ".prefab";
            GameObject prefab = PrefabUtility.SaveAsPrefabAssetAndConnect(
                level, prefabPath, InteractionMode.UserAction
            );

            Selection.activeGameObject = level;
            EditorGUIUtility.PingObject(prefab);

            Debug.Log($"[DownhillLevelGenerator] Prefab saved to: {prefabPath}");
        }

        GUILayout.Space(4);

        // ── Clear ──
        GUI.color = new Color(1f, 0.7f, 0.7f);
        if (GUILayout.Button("Clear Generated Level"))
        {
            ClearExisting(gen.prefabName);
            Debug.Log("[DownhillLevelGenerator] Cleared.");
        }
        GUI.color = Color.white;
    }

    private void ClearExisting(string name)
    {
        GameObject existing = GameObject.Find(name);
        if (existing != null)
            Undo.DestroyObjectImmediate(existing);
    }
}
