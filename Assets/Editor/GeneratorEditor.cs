using UnityEngine;
using UnityEditor;

/// <summary>
/// This class is used for editing the inspector of
/// the MapGenerator class.
/// </summary>
[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    /// <summary>
    /// Called when the inspector is drawn.
    /// </summary>
    public override void OnInspectorGUI()
    {
        MapGenerator mapGenerator = (MapGenerator)target;
        
        // Loads the default parts of the inspector.
        DrawDefaultInspector();
        
        // Add a button for displaying the noise map.
        if (GUILayout.Button("Generate Map"))
        {
            mapGenerator.GenerateMap();
        }

        // Add a button for setting the default values.
        if (GUILayout.Button("Set Defaults"))
        {
            mapGenerator.SetDefaults();
            mapGenerator.GenerateMap();
        }
    }
}

[CustomEditor(typeof(VegetationGenerator))]
public class VegetationGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        VegetationGenerator vegetationGenerator = (VegetationGenerator)target;
        
        DrawDefaultInspector();
        
        if (GUILayout.Button("Reload Assets"))
        {
            vegetationGenerator.LoadPrefabs();
        }
    }
}
