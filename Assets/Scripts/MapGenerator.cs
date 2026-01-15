using UnityEngine;

/// <summary>
/// Class used for terrain generation.
/// </summary>
public class MapGenerator : MonoBehaviour
{
    public enum DrawMode { NoiseMap, Mesh };
    private DrawMode renderMode = DrawMode.Mesh;

    
    [HideInInspector] public int mapWidth;
    [HideInInspector] public int mapHeight;
    [HideInInspector] public int borderWidth = 30; 
    [HideInInspector] public int roadWidth = 5;
    [HideInInspector] public float heightMultiplier;
    [HideInInspector] public Vector2 offset;

    [HideInInspector] public float noiseScale;
    [HideInInspector] public float UVScale = 500f;
    
    //Seed can be changed in inspector, other values are set in SetDefaults().
    public int seed;
    
    [HideInInspector] public int octaves;
    [HideInInspector] public float persistence;
    [HideInInspector] public float lacunarity;
    [HideInInspector] public bool autoUpdate;

    [HideInInspector] public Material terrainMaterial;
    [HideInInspector] public Material roadMaterial;
    [HideInInspector] public Material mountainMaterial;
    [HideInInspector] public VegetationGenerator vegetationGenerator;

    private float lastWarningTime = -4f;
    private float warningCooldown = 5f;

    /// <summary>
    /// Generate the noise map and display it. 
    /// </summary>
    public void GenerateMap()
    {
        if (Application.isPlaying)
        {
            if (Time.time - lastWarningTime > warningCooldown)
            {
                Debug.LogWarning("Cannot modify terrain while in play mode.");
                lastWarningTime = Time.time; // Update last warning time
            }
            return;
        }

        if (vegetationGenerator != null)
        {
            vegetationGenerator.ClearVegetation();
        }

        int wholeMapWidth = mapWidth + 2 * borderWidth;
        int wholeMapHeight = mapHeight + 2 * borderWidth;

        float[,] noiseMap = 
            Noise.GenerateNoiseMap(wholeMapWidth,
                                   wholeMapHeight,
                                   seed, noiseScale, noiseScale * 2, borderWidth, octaves, persistence, lacunarity, 
                                   new Vector2(offset.x - borderWidth, offset.y - borderWidth));

        // Increase height inside the border area.
        for (int y = 0; y < wholeMapHeight; y++)
        {
            for (int x = 0; x < wholeMapWidth; x++)
            {
                float distX = Mathf.Min(x, wholeMapWidth - x);
                float distY = Mathf.Min(y, wholeMapHeight - y);
                float distance = Mathf.Min(distX, distY);

                // Increase heightMultiplier towards the edges.
                // Use a cubic ease out function to smooth the transition.
                float t = Mathf.Clamp01((borderWidth - distance) / borderWidth);
                float eased = 1 - Mathf.Pow(1 - t, 3);
                float multiplier = Mathf.Lerp(1, 10, eased);

                noiseMap[x, y] *= multiplier;
            }
        }

        bool[,] roadMap = new bool[wholeMapWidth, wholeMapHeight]; // Create a map to mark road positions.
        GenerateRoad(noiseMap, mapWidth, mapHeight, roadMap);

        MapDisplay display = FindAnyObjectByType<MapDisplay>();

        if (renderMode == DrawMode.NoiseMap)
        {
            terrainMaterial = Resources.Load<Material>("Materials/TerrainMaterial");
            display.DrawTexture(TextureGenerator.TextureFromHeightMap(noiseMap));
        } 
        else if (renderMode == DrawMode.Mesh)
        {
            terrainMaterial = Resources.Load<Material>("Materials/Forest_Ground_12/Forest_Ground_12_Albedo");
            roadMaterial = Resources.Load<Material>("Materials/Dry_Ground_10/Dry_Ground_10");
            mountainMaterial = Resources.Load<Material>("Materials/rock_01/rock_01");

            display.DrawMesh(MeshGenerator.GenerateTerrainMesh(noiseMap, heightMultiplier, UVScale), 
                             terrainMaterial, MapDisplay.DrawMeshMode.Terrain);
            display.DrawMesh(MeshGenerator.GenerateRoadMesh(roadMap, noiseMap, wholeMapWidth, wholeMapHeight, heightMultiplier, borderWidth),
                             roadMaterial, MapDisplay.DrawMeshMode.Road);

            if (vegetationGenerator != null)
            {
                vegetationGenerator.SetMapSize(mapWidth, mapHeight);
                vegetationGenerator.GenerateVegetation(seed);
            }
        }

        CreateBoundaries(mapWidth, mapHeight);
    }

    /// <summary>
    /// Generates a simple road from top to bottom.
    /// </summary>
    /// <param name="noiseMap">The noise map generated for the mesh.</param>
    /// <param name="mapWidth">The width of the world.</param>
    /// <param name="mapHeight">The height of the world.</param>
    /// <param name="roadMap">The road map to generate.</param>
    private void GenerateRoad(float[,] noiseMap, int mapWidth, int mapHeight, bool[,] roadMap)
    {
        int startX = mapWidth / 2 - roadWidth / 2;
        int endX = startX + roadWidth;
        int roadTransitionWidth = Mathf.Max(1, roadWidth);

        for (int x = startX - roadTransitionWidth; x < endX + roadTransitionWidth; x++)
        {
            for (int y = -borderWidth; y < mapHeight + borderWidth; y++)
            {
                // Adjust for extended noiseMap with borders.
                int adjustedX = x + borderWidth;
                int adjustedY = y + borderWidth;

                int distanceFromRoadEdge = Mathf.Min(Mathf.Abs(x - startX), Mathf.Abs(x - endX));

                if (x < startX || x >= endX)
                {
                    float transitionFactor = Mathf.Clamp01((float)distanceFromRoadEdge / roadTransitionWidth);
                    
                    // Use a cubic ease out function to smooth the transition.
                    float easedFactor = 1 - Mathf.Pow(1 - transitionFactor, 3);
                    float blendedHeight = Mathf.Lerp(0.05f * heightMultiplier, noiseMap[adjustedX, adjustedY], easedFactor);

                    noiseMap[adjustedX, adjustedY] = blendedHeight;
                    continue;
                }

                // Flatten the road within the main terrain and border regions.
                noiseMap[adjustedX, adjustedY] = 0.05f * heightMultiplier;
                roadMap[x, adjustedY] = true;
            }
        }        
    }

    /// <summary>
    /// Create boundaries around the map.
    /// </summary>
    /// <param name="width">The width of the map.</param>
    /// <param name="height">The height of the map.</param>
    /// <param name="wallHeight">The height of the walls. Defaults to 100f.</param>
    /// <param name="wallThickness">The thickness of the walls. Defaults to 5f.</param>
    private void CreateBoundaries(int width, int height, float wallHeight = 100f, float wallThickness = 5f)
    {
        float halfWallHeight = wallHeight / 2f;

        CreateWall("NorthWall", new Vector3(0, halfWallHeight, height / 2f + wallThickness / 2f), 
                   new Vector3(width, wallHeight, wallThickness));
    
        CreateWall("SouthWall", new Vector3(0, halfWallHeight, -height / 2f - wallThickness / 2f), 
                   new Vector3(width, wallHeight, wallThickness));
        
        CreateWall("EastWall", new Vector3(width / 2f + wallThickness / 2f, halfWallHeight, 0), 
                   new Vector3(wallThickness, wallHeight, height));
        
        CreateWall("WestWall", new Vector3(-width / 2f - wallThickness / 2f, halfWallHeight, 0), 
                   new Vector3(wallThickness, wallHeight, height));
    }

    /// <summary>
    /// Create a wall with the given name, center and size.
    /// If the wall already exists, update it to match 
    /// the current map state.
    /// </summary>
    /// <param name="name">Name of the wall.</param>
    /// <param name="center">Center of the wall.</param>
    /// <param name="size">Size of the wall.</param>
    private void CreateWall(string name, Vector3 center, Vector3 size)
    {
        Transform existing = transform.Find(name);
        GameObject wall;

        if (existing != null)
        {
            wall = existing.gameObject;
        }
        else
        {
            wall = new GameObject(name);
            wall.transform.parent = transform;
        }

        wall.transform.localPosition = Vector3.zero;

        int layer = LayerMask.NameToLayer("Ignore Raycast");
        wall.layer = layer;

        BoxCollider collider = wall.GetComponent<BoxCollider>();
        if (collider == null) collider = wall.AddComponent<BoxCollider>();

        collider.size = size;
        collider.center = center;
        collider.isTrigger = false;
    }

    /// <summary>
    /// Set the default values for the map generator.
    /// </summary>
    public void SetDefaults()
    {
        renderMode = DrawMode.Mesh;
        mapWidth = 100;
        mapHeight = 100;
        borderWidth = 30;
        roadWidth = 5;
        heightMultiplier = 3;
        noiseScale = 10;
        UVScale = 500;
        octaves = 2;
        persistence = 1f;
        lacunarity = 1;
        offset.x = 0;
        offset.y = 0;
    }

    /// <summary>
    /// Called when the script is loaded or a value is changed.
    /// </summary>
    private void OnValidate()
    {
        renderMode = DrawMode.Mesh;
        mapWidth = Mathf.Max(1, mapWidth);
        mapHeight = Mathf.Max(1, mapHeight);
        heightMultiplier = Mathf.Max(0.001f, heightMultiplier);
        noiseScale = Mathf.Max(0.0001f, noiseScale);
        octaves = Mathf.Max(1, octaves);
        persistence = Mathf.Max(0, persistence);
        lacunarity = Mathf.Max(1, lacunarity);

        if (autoUpdate)
        {
            Invoke(nameof(GenerateMap), 0.1f); // Delay GenerateMap by 0.1 seconds.
        }
    }
}
