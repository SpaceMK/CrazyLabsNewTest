using UnityEngine;

/// <summary>
/// Generates a level with repeating flat → downhill slope sections,
/// walled off on both sides.
/// 
/// SETUP:
///   1. Put this file in Assets/Scripts/
///   2. Put DownhillLevelGeneratorEditor.cs in Assets/Scripts/Editor/
///   3. Attach this to any GameObject
///   4. Use the Inspector buttons to generate and save as prefab
/// </summary>
public class DownhillLevelGenerator : MonoBehaviour
{
    [Header("Section Dimensions")]
    [Tooltip("Width of the track (X axis)")]
    public float trackWidth = 10f;

    [Tooltip("Length of each flat segment (Z axis)")]
    public float flatLength = 15f;

    [Tooltip("Length of each slope segment (Z axis)")]
    public float slopeLength = 20f;

    [Tooltip("Height drop per slope section (Y axis)")]
    public float slopeDropHeight = 5f;

    [Tooltip("Number of flat+slope sections")]
    public int sectionCount = 4;

    [Header("Wall Settings")]
    [Tooltip("Height of the side walls")]
    public float wallHeight = 3f;

    [Tooltip("Thickness of the side walls")]
    public float wallThickness = 0.5f;

    [Header("Materials (optional)")]
    public Material groundMaterial;
    public Material wallMaterial;

    [Header("Prefab Settings")]
    [Tooltip("Folder inside Assets to save the prefab (no trailing slash)")]
    public string savePath = "Prefabs";

    [Tooltip("Name for the saved prefab")]
    public string prefabName = "DownhillLevel";

    /// <summary>
    /// Generates the level geometry and returns the root GameObject.
    /// </summary>
    public GameObject GenerateLevel()
    {
        GameObject levelRoot = new GameObject(prefabName);

        float currentZ = 0f;
        float currentY = 0f;

        for (int i = 0; i < sectionCount; i++)
        {
            CreateFlat(levelRoot.transform, currentZ, currentY, i);
            CreateWall(levelRoot.transform, currentZ, currentY, flatLength, 0f, -1, i, "Flat");
            CreateWall(levelRoot.transform, currentZ, currentY, flatLength, 0f,  1, i, "Flat");

            currentZ += flatLength;

            CreateSlope(levelRoot.transform, currentZ, currentY, i);
            CreateWall(levelRoot.transform, currentZ, currentY, slopeLength, slopeDropHeight, -1, i, "Slope");
            CreateWall(levelRoot.transform, currentZ, currentY, slopeLength, slopeDropHeight,  1, i, "Slope");

            currentZ += slopeLength;
            currentY -= slopeDropHeight;
        }

        // Landing at the end
        CreateFlat(levelRoot.transform, currentZ, currentY, sectionCount, "Landing");
        CreateWall(levelRoot.transform, currentZ, currentY, flatLength, 0f, -1, sectionCount, "Landing");
        CreateWall(levelRoot.transform, currentZ, currentY, flatLength, 0f,  1, sectionCount, "Landing");

        return levelRoot;
    }

    private void CreateFlat(Transform parent, float startZ, float startY, int index, string label = "Flat")
    {
        GameObject flat = GameObject.CreatePrimitive(PrimitiveType.Cube);
        flat.name = $"Section {index} - {label}";
        flat.transform.parent = parent;
        flat.transform.localScale = new Vector3(trackWidth, 0.5f, flatLength);
        flat.transform.position = new Vector3(0f, startY - 0.25f, startZ + flatLength / 2f);

        if (groundMaterial != null)
            flat.GetComponent<Renderer>().material = groundMaterial;
    }

    private void CreateSlope(Transform parent, float startZ, float startY, int index)
    {
        GameObject slope = GameObject.CreatePrimitive(PrimitiveType.Cube);
        slope.name = $"Section {index} - Slope";
        slope.transform.parent = parent;

        float hypotenuse = Mathf.Sqrt(slopeLength * slopeLength + slopeDropHeight * slopeDropHeight);
        float angle = Mathf.Atan2(slopeDropHeight, slopeLength) * Mathf.Rad2Deg;

        slope.transform.localScale = new Vector3(trackWidth, 0.5f, hypotenuse);
        slope.transform.position = new Vector3(0f, startY - slopeDropHeight / 2f - 0.25f, startZ + slopeLength / 2f);
        slope.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

        if (groundMaterial != null)
            slope.GetComponent<Renderer>().material = groundMaterial;
    }

    private void CreateWall(Transform parent, float startZ, float startY,
                            float segLength, float dropHeight, int side,
                            int index, string label)
    {
        GameObject wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = $"Section {index} - {label} Wall {(side == -1 ? "L" : "R")}";
        wall.transform.parent = parent;

        float hypotenuse = Mathf.Sqrt(segLength * segLength + dropHeight * dropHeight);
        float angle = Mathf.Atan2(dropHeight, segLength) * Mathf.Rad2Deg;

        wall.transform.localScale = new Vector3(wallThickness, wallHeight, hypotenuse);

        float xPos = side * (trackWidth / 2f + wallThickness / 2f);
        float midZ = startZ + segLength / 2f;
        float midY = startY - dropHeight / 2f + wallHeight / 2f;

        wall.transform.position = new Vector3(xPos, midY - 0.25f, midZ);
        wall.transform.rotation = Quaternion.Euler(angle, 0f, 0f);

        if (wallMaterial != null)
            wall.GetComponent<Renderer>().material = wallMaterial;
    }
}
