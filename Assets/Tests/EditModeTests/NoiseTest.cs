using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class NoiseTest
{
    // A Test to check that GenerateNoiseMap returns a valid array
    [Test]
    public void NoiseMapIsNotNull()
    {
        Noise noise = new Noise();
        int mapWidth = 10;
        int mapHeight = 10;
        int seed = 1;
        float scale = 1;
        float borderScale = scale * 2;
        int borderSize = 30;
        int octaves = 1;
        float persistence = 1;
        float lacunarity = 1;
        Vector2 offset = new Vector2(1.0f, 1.0f);
        
        // Create example noiseMap
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, seed, 
                                scale, borderScale, borderSize, octaves, persistence, lacunarity, offset);
        
        Assert.IsNotNull(noiseMap, "GenerateNoiseMap should not return a null noiseMap");
    }

    // Test to see that generated noiseMap is correct size
    [Test]
    public void NoiseMapCorrectSize()
    {
        //Set example size for map
        int mapWidth = 10;
        int mapHeight = 10;
        Vector2 offset = new Vector2(1.0f, 1.0f);

        // Create example noisemap with given size
        float[,] noiseMap = Noise.GenerateNoiseMap(mapWidth, mapHeight, 1, 
                                1, 1, 0, 1, 1, 1, offset);
        int expectedSize = mapHeight * mapWidth;
        int actualSize = noiseMap.Length;

        // Compare actual size of the created noiseMap to theoretical one
        Assert.AreEqual(expectedSize, actualSize, "Wrong size of NoiseMap");
    }

    // Test to check that seed value affects the end result
    [Test]
    public void NoiseMapSeedAffectsResult()
    {
        int seed1 = 1;
        int seed2 = 2;
        Vector2 offset = new Vector2(1.0f, 1.0f);
        // create two similar noiseMaps but with different seed values
        float[,] noiseMap1 = Noise.GenerateNoiseMap(10, 10, seed1, 
                                1, 1, 0, 1, 1, 1, offset);
        float[,] noiseMap2 = Noise.GenerateNoiseMap(10, 10, seed2, 
                                1, 1, 0, 1, 1, 1, offset);
        
        // The noiseMaps with different seed parameters should not be equal
        Assert.AreNotEqual(noiseMap1, noiseMap2, "Seed value doesn't change result");
    }
}
