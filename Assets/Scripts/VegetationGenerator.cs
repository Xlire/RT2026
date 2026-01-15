using UnityEngine;
using System.Collections.Generic;
using System;

/// <summary>
/// Class used for generating vegetation in the world.
/// </summary>
[ExecuteAlways] // To also run Awake() in Edit Mode.
public class VegetationGenerator : MonoBehaviour
{
    private GameObject[] _treePrefabs;
    private GameObject[] _rockPrefabs;
    private GameObject[] _grassPlantPrefabs;
    private GameObject[] _mediumPlantPrefabs;
    private GameObject[] _bigPlantPrefabs;

    private List<GameObject> spawnedTrees = new List<GameObject>();
    private List<GameObject> spawnedRocks = new List<GameObject>();
    private List<GameObject> spawnedVegetation = new List<GameObject>();

    private int treeCount;
    private int rockCount;
    private int bigPlantCount;

    private int mapWidth;
    private int mapHeight;

    /// <summary>
    /// Load prefabs from the Resources folder.
    /// </summary>
    private void Awake()
    {
        LoadPrefabs();
    }

    /// <summary>
    /// Sets the map size and recalculates
    /// the vegetation counts.
    /// </summary>
    /// <param name="width">The width of the map.</param>
    /// <param name="height">The height of the map.</param>
    public void SetMapSize(int width, int height)
    {
        mapWidth = width;
        mapHeight = height;
        CalculateCounts();
    }

    /// <summary>
    /// Generates vegetation based on the noise map. Uses
    /// the map width, map height, and seed to determine
    /// positions of trees, rocks, and other vegetation.
    /// </summary>
    /// <param name="seed">The seed used in world generation.</param>
    public void GenerateVegetation(int seed)
    {
        ClearVegetation(); // Clear existing vegetation.

        // Determine vegetation locations based on the world seed.
        System.Random prng = new System.Random(seed);
        List<Vector3> placedPositions = new List<Vector3>();

        GenerateRocks(placedPositions, prng);
        GenerateBigPlants(placedPositions, prng);
        GenerateTreesAndGrass(placedPositions, prng);
    }

    /// <summary>
    /// Generate trees and grass in the world based on a random number
    /// generator with the world seed. Trees are picked randomly
    /// from the treePrefabs array and placed at a random position.
    /// </summary>
    /// <param name="placedPositions">The positions of all placed vegetation.</param>
    /// <param name="prng">The random number generator to use.</param>
    private void GenerateTreesAndGrass(List<Vector3> placedPositions, System.Random prng)
    {
        // Place treeCount trees into the world.
        const int maxTries = 5;
        int tryCount = 0;
        for (int i = 0; i < treeCount; i++)
        {
            const float buff = 2.8f;
            var x = (float)prng.NextDouble() * (mapWidth - buff);
            var z = (float)prng.NextDouble() * (mapHeight - buff);

            float worldX = x - mapWidth / 2f;
            float worldZ = z - mapHeight / 2f;


            // Substract a small value to prevent trees from floating.
            float height = GetTerrainHeight(worldX, worldZ) - (float)0.1;

            Vector3 position = new Vector3(worldX, height, worldZ);

            // Prevent trees from overlapping.
            if (IsOverlapping(position, placedPositions, 3f) || IsOnRoad(x, 3.5f))
            {
                if (tryCount < maxTries)
                {
                    // Try spawning in another location
                    i--;
                    tryCount++;
                }
                continue;
            }
            tryCount = 0;

            GameObject treePrefab = _treePrefabs[prng.Next(0, _treePrefabs.Length)];

            if (treePrefab == null)
            {
                Debug.LogError("Tree prefab not found.");
                return;
            }

            GameObject treeInstance = Instantiate(treePrefab, position, Quaternion.Euler(0, prng.Next(0, 360), 0), transform);
            treeInstance.tag = "Tree"; // Set the tag to Tree.
            spawnedTrees.Add(treeInstance);
            placedPositions.Add(position);
            GenerateGrassPatch(placedPositions, prng, position);
        }
    }

    /// <summary>
    /// Generate rocks in the world based on a random number
    /// generator with the world seed. Rocks are picked randomly
    /// from the rockPrefabs array and placed at a random position.
    /// </summary>
    /// <param name="placedPositions">The positions of all placed vegetation.</param>
    /// <param name="prng">The random number generator to use.</param>
    private void GenerateRocks(List<Vector3> placedPositions, System.Random prng)
    {
        // Place rockCount rocks into the world.
        for (int i = 0; i < rockCount; i++)
        {
            const float buff = 2.8f;
            var x = (float)prng.NextDouble() * (mapWidth - buff);
            var z = (float)prng.NextDouble() * (mapHeight - buff);

            float worldX = x - mapWidth / 2f;
            float worldZ = z - mapHeight / 2f;

            // Substract a small value to prevent rocks from floating.
            float height = GetTerrainHeight(worldX, worldZ) - (float)0.1;

            Vector3 position = new Vector3(worldX, height, worldZ);

            // Prevent rocks from overlapping.
            if (!IsOverlapping(position, placedPositions, 3f) && !IsOnRoad(x, 3))
            {
                GameObject rockPrefab = _rockPrefabs[prng.Next(0, _rockPrefabs.Length)];

                if (rockPrefab == null)
                {
                    Debug.LogError("Tree prefab not found.");
                    return;
                }

                GameObject rockInstance = Instantiate(rockPrefab, position, Quaternion.Euler(0, prng.Next(0, 360), 0), transform);
                rockInstance.tag = "Rock"; // Set the tag to Rock.
                spawnedRocks.Add(rockInstance); // Store the spawned rock.
                placedPositions.Add(position); // Store the position of the rock.
            }
        }
    }

    /// <summary>
    /// Generate bigger plants in the world.
    /// </summary>
    /// <param name="placedPositions">The positions of all placed vegetation.</param>
    /// <param name="prng">The random number generator to use.</param>
    private void GenerateBigPlants(List<Vector3> placedPositions, System.Random prng)
    {
        for (int i = 0; i < bigPlantCount; i++)
        {
            const float buff = 2.8f;
            var x = (float)prng.NextDouble() * (mapWidth - buff);
            var z = (float)prng.NextDouble() * (mapHeight - buff);

            float worldX = x - mapWidth / 2f;
            float worldZ = z - mapHeight / 2f;

            // Substract a small value to prevent trees from floating.
            float height = GetTerrainHeight(worldX, worldZ) - (float)0.1;

            var position = new Vector3(worldX, height, worldZ);

            if (!IsOverlapping(position, placedPositions, 2f) && !IsOnRoad(x, 3))
            {
                GameObject plantPrefab = _bigPlantPrefabs[prng.Next(0, _bigPlantPrefabs.Length)];

                if (plantPrefab == null)
                {
                    Debug.LogError("Tree prefab not found.");
                    return;
                }

                GameObject plant = Instantiate(plantPrefab, position, Quaternion.Euler(0, prng.Next(0, 360), 0), transform);
                plant.tag = "Plant";
                spawnedVegetation.Add(plant);
                placedPositions.Add(position);
            }
        }
    }

    /// <summary>
    /// Generate a grass patch.
    /// </summary>
    /// <param name="placedPositions">The positions of all placed vegetation.</param>
    /// <param name="prng">The random number generator to use.</param>
    /// <param name="midPos">The middle point of the grass patch.</param>
    private void GenerateGrassPatch(List<Vector3> placedPositions, System.Random prng, Vector3 midPos)
    {
        const int grassCount = 70;
        const int patchRadius = 7;
        const int maxTries = 5;
        int tryCount = 0;
        // Spawn grass
        for (int i = 0; i < grassCount; i++)
        {
            (float x, float z) = RandomInsideCircle(prng, patchRadius);
            float worldX = midPos.x + x;
            float worldZ = midPos.z + z;
            // Substract a small value to prevent plants from floating.
            float worldY = GetTerrainHeight(worldX, worldZ) - (float)0.1;
            var position = new Vector3(worldX, worldY, worldZ);

            // Prevent plants from overlapping.
            // Plants are smaller than trees and rocks, so we can use a smaller distance.
            if (IsOverlapping(position, placedPositions, 0.2f) || IsOnRoad(worldX + (mapWidth / 2), 2f) || !IsWithinMapBounds(position, 0.7f))
            {
                if (tryCount < maxTries)
                {
                    // Try spawning in another location.
                    i--;
                    tryCount++;
                }
                continue;
            }
            tryCount = 0;

            GameObject grassPrefab = _grassPlantPrefabs[prng.Next(0, _grassPlantPrefabs.Length)];

            if (grassPrefab == null)
            {
                Debug.LogError("Grass prefab not found.");
                return;
            }

            GameObject grass = Instantiate(grassPrefab, position, Quaternion.Euler(0, prng.Next(0, 360), 0), transform);
            grass.tag = "Plant";
            spawnedVegetation.Add(grass);
        }

        tryCount = 0;
        const int maxMediumPlants = 3;
        int mediumPlantCount = prng.Next(0, maxMediumPlants);

        // Spawn rarer plants.
        for (int i = 0; i < mediumPlantCount; i++)
        {
            (float x, float z) = RandomInsideCircle(prng, patchRadius);
            float worldX = midPos.x + x;
            float worldZ = midPos.z + z;
            // Substract a small value to prevent plants from floating.
            float worldY = GetTerrainHeight(worldX, worldZ) - (float)0.1;
            var position = new Vector3(worldX, worldY, worldZ);

            // Prevent plants from overlapping.
            // Plants are smaller than trees and rocks, so we can use a smaller distance.
            if (IsOverlapping(position, placedPositions, 0.3f) || IsOnRoad(worldX + (mapWidth / 2), 2f) || !IsWithinMapBounds(position, 0.7f))
            {
                if (tryCount < maxTries)
                {
                    // Try spawning in another location
                    i--;
                    tryCount++;
                }
                continue;
            }
            tryCount = 0;

            GameObject plantPrefab = _mediumPlantPrefabs[prng.Next(0, _mediumPlantPrefabs.Length)];

            if (plantPrefab == null)
            {
                Debug.LogError("Plant prefab not found.");
                return;
            }

            GameObject plant = Instantiate(plantPrefab, position, Quaternion.Euler(0, prng.Next(0, 360), 0), transform);
            plant.tag = "Plant";
            spawnedVegetation.Add(plant);
        }
    }


    /// <summary>
    /// Loads all prefabs from the Resources folder.
    /// </summary>
    public void LoadPrefabs()
    {
        _treePrefabs = Resources.LoadAll<GameObject>("Prefabs/Trees");
        _rockPrefabs = Resources.LoadAll<GameObject>("Prefabs/Rocks");
        _grassPlantPrefabs = Resources.LoadAll<GameObject>("Prefabs/Plants/Grass");
        _mediumPlantPrefabs = Resources.LoadAll<GameObject>("Prefabs/Plants/Medium");
        _bigPlantPrefabs = Resources.LoadAll<GameObject>("Prefabs/Plants/Big");

        if (_treePrefabs.Length == 0) Debug.LogError("No tree prefabs found.");
        if (_rockPrefabs.Length == 0) Debug.LogError("No rock prefabs found.");
        if (_grassPlantPrefabs.Length == 0) Debug.LogError("No plant prefabs found.");
    }

    /// <summary>
    /// Gets the height of the terrain at a given position 
    /// by raycasting down from a high point.
    /// </summary>
    /// <param name="x">The x coordinate of the world point.</param>
    /// <param name="z">The z coordinate of the world point.</param>
    /// <returns>The y coordinate at which to place the tree. If 
    ///          nothing was hit with the raycast, return -999f.</returns>
    private float GetTerrainHeight(float x, float z)
    {
        RaycastHit hit;
        Ray ray = new Ray(new Vector3(x, 1000, z), Vector3.down);

        int terrainLayer = LayerMask.GetMask("Terrain");

        // Cast and detect a hit on the terrain layer.
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, terrainLayer))
        {
            return hit.point.y;
        }

        Debug.LogWarning($"No terrain hit detected at {x}, {z}.");
        return 0;
    }

    /// <summary>
    /// Calculates the number of trees, rocks, and
    /// vegetation to be placed in the world.
    /// </summary>
    private void CalculateCounts()
    {
        if (mapWidth == 0 || mapHeight == 0)
        {
            Debug.LogError("Map is not yet initialized. Could not calculate vegetation counts.");
            return;
        }

        int mapSize = mapWidth * mapHeight;

        treeCount = mapSize / 80;
        rockCount = mapSize / 500;
        bigPlantCount = mapSize / 800;
    }

    /// <summary>
    /// Prevents objects from overlapping by checking distance.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="placedPositions">The list of already placed positions.</param>
    /// <param name="minDistance">The minimum distance to check.</param>
    /// <returns>True if the position is overlapping with other objects, false otherwise.</returns>
    private bool IsOverlapping(Vector3 position, List<Vector3> placedPositions, float minDistance)
    {
        foreach (Vector3 placed in placedPositions)
        {
            if (Vector3.Distance(position, placed) < minDistance)
                return true;
        }

        // Check physics-based overlap using a sphere
        Collider[] colliders = Physics.OverlapSphere(position, minDistance);
        foreach (Collider collider in colliders)
        {
            if (collider.gameObject.CompareTag("Tree") || collider.gameObject.CompareTag("Rock")
                || collider.gameObject.CompareTag("Plant")) // Check for all vegetation types
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Checks if the given position is within map bounds.
    /// </summary>
    /// <param name="position">The position to check.</param>
    /// <param name="dist">The distance from the edge of the map.</param>
    /// <returns>True if the position is within map bounds, false otherwise.</returns>
    private bool IsWithinMapBounds(Vector3 position, float dist = 0f)
    {
        var mg = FindAnyObjectByType<MapGenerator>();
        var max_x = mapWidth / 2 - dist;
        var max_z = mapHeight / 2 - dist;
        return position.x > -max_x && position.x < max_x &&
               position.z > -max_z && position.z < max_z;
    }

    /// <summary>
    /// Checks if a position is on the road.
    /// </summary>
    /// <param name="x">The x coordinate of the position.</param>
    /// <param name="buffer">The buffer distance from the road.</param>
    /// <returns>True if the position is on the road, false otherwise.</returns>
    private bool IsOnRoad(float x, float buffer = 1f)
    {
        var mapGenerator = FindAnyObjectByType<MapGenerator>();

        // Calculate road boundaries.
        int roadStart = mapWidth / 2 - mapGenerator.roadWidth / 2;
        int roadEnd = mapWidth / 2 + mapGenerator.roadWidth / 2;

        return x >= roadStart - buffer && x <= roadEnd + buffer;
    }

    /// <summary>
    /// Destroys all vegetation under VegetationGenerator to prevent duplicates.
    /// </summary>
    public void ClearVegetation()
    {
        if (transform.childCount == 0) return;

        Debug.Log($"Clearing all vegetation. Found {transform.childCount} objects.");

        List<GameObject> objectsToDestroy = new List<GameObject>();

        // Collect the children of the VegetationGenerator to a list
        //  to prevent modifying the collection while iterating.
        // Doing this any other way resulted in an error.
        foreach (Transform child in transform)
        {
            objectsToDestroy.Add(child.gameObject);
        }

        spawnedTrees.Clear(); // Clear the list of spawned trees.
        spawnedRocks.Clear(); // Clear the list of spawned rocks.
        spawnedVegetation.Clear(); // Clear the list of spawned vegetation.

        // Destroy all collected objects.
        foreach (GameObject obj in objectsToDestroy)
        {
            DestroyImmediate(obj);
        }

        Debug.Log($"Vegetation cleared. Remaining objects: {transform.childCount}.");
    }

    /// <summary>
    /// Returns a random point inside a circle with given radius.
    /// </summary>
    /// <param name="rng">The random number generator to use.</param>
    /// <param name="radius">The radius of the circle.</param>
    /// <returns>A random point inside the circle.</returns>
    public static (float x, float y) RandomInsideCircle(System.Random rng, float radius = 1.0f)
    {
        double theta = rng.NextDouble() * 2 * Math.PI;

        double r = radius * Math.Sqrt(rng.NextDouble());

        // Polar to cartesian
        float x = (float)(r * Math.Cos(theta));
        float y = (float)(r * Math.Sin(theta));

        return (x, y);
    }
}