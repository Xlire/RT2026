using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
using UnityEngine.SceneManagement;

public class VegetationGeneratorTest
{
    private GameObject _testObject;
    private GameObject _mapGenerator;
    private VegetationGenerator _vegetationGenerator;


    [SetUp]
    public void SetUp()
    {
        // Creating required objects for the test 
        _testObject = new GameObject("VegetationGeneratorTest");
        _vegetationGenerator = _testObject.AddComponent<VegetationGenerator>();

        _mapGenerator = new GameObject("MapGeneratorMock");
        MapGenerator _mockMapGenerator = _mapGenerator.AddComponent<MapGenerator>();

        // roadWidth needs to be set up for MapGenerator to work inside GenerateVegetation class functions
        _mockMapGenerator.roadWidth = 10;
    }


    // Destructor for created objects
    [TearDown]
    public void Teardown()
    {
        Object.DestroyImmediate(_testObject);
        Object.DestroyImmediate(_mapGenerator);
    }


    //Test to see that LoadPrefabs method doesn't break anything
    [Test]
    public void LoadPrefabsTest()
    {
        // Calling LoadPrefabs to analyze its effects
        _vegetationGenerator.LoadPrefabs();

        // Comfirming the function call didn't break _vegetationGenerator object
        Assert.IsNotNull(_vegetationGenerator, "VegetationGenerator component is null.");
    }


    //Test to see that SetMapSize passes the given size attributes to correct class variables
    [Test]
    public void SetMapSizeChangesAttributes()
    {
        
        // Example parameter values for SetMapSize
        int mapWidth = 100;
        int mapHeight = 100;
        _vegetationGenerator.SetMapSize(mapWidth, mapHeight);

        // Accessing private variables through reflection. Checking that the values have changed
        Assert.AreEqual(100, _vegetationGenerator.GetType().GetField("mapWidth", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_vegetationGenerator));
        Assert.AreEqual(100, _vegetationGenerator.GetType().GetField("mapHeight", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance).GetValue(_vegetationGenerator));
    }


    // Test to see that GenerateVegetation method produces vegetation objects
    [UnityTest]
    public IEnumerator GenerateVegetationSpawnsObjects()
    {   
        // Example parameter values to create a noisemap for GenerateVegetation function
        int mapWidth = 100;
        int mapHeight = 100;
        Vector2 offset = new Vector2(1.0f, 1.0f);
        float heightMultiplier = 3;
        int seed = 1;

        // Create example noisemap with given size
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, 1, 
                                1, 1, 0, 1, 1, 1, offset);

        // Ensuring prefabs are loaded
        _vegetationGenerator.LoadPrefabs();

        // Setting mapsize for the generation to work
        _vegetationGenerator.SetMapSize(mapWidth, mapHeight);
        
        // Calling the function to be tested
        _vegetationGenerator.GenerateVegetation(seed);

        yield return null; // Wait for next frame, allow objects to spawn

        // Testing if the amount of child objects for _testObject has risen. In succesful case it has
        Assert.Greater(_testObject.transform.childCount, 0, "No vegetation was geerated.");
    }


    // Test case for ClearVegetation method
    [UnityTest]
    public IEnumerator ClearVegetationRemovesAllObjects()
    {
        // Example parameter values for a noisemap for GenerateVegetation method
        int mapWidth = 100;
        int mapHeight = 100;
        Vector2 offset = new Vector2(1.0f, 1.0f);
        float heightMultiplier = 3;
        int seed = 1;

        // Create example noisemap with given size
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, 1, 
                                1, 1, 0, 1, 1, 1, offset);

        // Setting up _vegetationGenerator for vegetation generation
        _vegetationGenerator.LoadPrefabs();
        _vegetationGenerator.SetMapSize(mapWidth, mapHeight);
        
        // Generating some vegetation for the cleaning function to remove
        _vegetationGenerator.GenerateVegetation(seed);

        yield return null; // Waiting for next frame, allow objects to spawn

        // Removing all generated objects
        _vegetationGenerator.ClearVegetation();

        yield return null; // Wait for objects to be destroyed

        // _testObject child count should be zero after calling ClearVegetation method
        Assert.AreEqual(0, _testObject.transform.childCount, "Vegetation was not cleared.");
    }
}
