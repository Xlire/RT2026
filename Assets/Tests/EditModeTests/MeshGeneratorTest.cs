using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

public class MeshGeneratorTest
{

    // Test to see that GenearateTerrainMesh or GenerateRoadMesh won't return null
    [Test]
    public void GenerateMeshOfCorrectType()
    {
        // Parameters for MeshGenerator functions
        Vector2 offset = new Vector2(1.0f, 1.0f);
        float[,] noiseMap = Noise.GenerateNoiseMap(10, 10, 1, 
                                1, 1, 0, 1, 1, 1, offset);
        // Example terrainMeshData
        MeshData terrainMeshData = MeshGenerator.GenerateTerrainMesh(noiseMap, 1, 500);

        // dummy roadMap for GenerateRoadMesh function
        bool[,] roadMap = new bool[10, 10];
        // Example roadMeshData
        MeshData roadMeshData = MeshGenerator.GenerateRoadMesh(roadMap, noiseMap, 10, 10, 500, 0);

        // Tests that retuned objects are not null
        Assert.IsNotNull(terrainMeshData, "GenerateTerrainMesh function should not return null");
        Assert.IsNotNull(roadMeshData, "GenerateRoadMesh function should not return null");

        // Tests that returned objects are of type MeshData
        Assert.IsInstanceOf<MeshData>(terrainMeshData, "Returned object from GenerateTerrainMesh should by of type MeshData");
        Assert.IsInstanceOf<MeshData>(roadMeshData, "Returned object from GenerateRoadMesh should by of type MeshData");
    }

    // Test for AddTriangle function
    [Test]
    public void AddTriangleChangesValues()
    {   
        // Creating dummy MeshData for testing of AddTriangle method
        MeshData md = new MeshData(10, 10);
        Debug.Log(md.triangles.Length);
        // Test the correct starting size of the triangles array
        Assert.AreEqual(md.triangles.Length, 486, "With 10, 10 size mesh the amount of starting triangles should be 486");

        // Adding values to the first three indexes of triangles array
        md.AddTriangle(1, 2, 3);

        // Checking that the values changed
        Assert.AreEqual(md.triangles[0], 1, "AddTriangel should change the values of first 3 indexes to ones given");
        Assert.AreEqual(md.triangles[1], 2, "AddTriangel should change the values of first 3 indexes to ones given");
        Assert.AreEqual(md.triangles[2], 3, "AddTriangel should change the values of first 3 indexes to ones given");
    }

    //Test for CreateMesh return type
    [Test]
    public void CreateMeshReturnsMesh()
    {
        // Create dummy MeshData to be tested
        MeshData md = new MeshData(10, 10);
        
        // Calling CreateMesh-function and saving the return value
        var m = md.CreateMesh();

        // Return values should be of type Mesh"
        Assert.IsInstanceOf<Mesh>(m, "CreateMesh should return Mesh type data");
    }

}
