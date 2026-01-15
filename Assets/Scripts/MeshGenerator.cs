using UnityEngine;

/// <summary>
/// Class used for generating a mesh from a height map.
/// </summary>
public static class MeshGenerator
{
    /// <summary>
    /// Generates the terrain mesh from a height map.
    /// </summary>
    /// <param name="heightMap">The height map to use.</param>
    /// <param name="heightMultiplier">The height multiplier to use.</param>
    /// <param name="uvScale">The UV scale to use.</param>
    /// <returns>The mesh created from the height map.</returns>
	public static MeshData GenerateTerrainMesh(float[,] heightMap, float heightMultiplier, float uvScale)
    {
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);
		float topLeftX = (width - 1) / -2f;
		float topLeftZ = (height - 1) / 2f;

		MeshData meshData = new MeshData(width, height);
		int vertexIndex = 0;

		for (int y = 0; y < height; y++)
        {
			for (int x = 0; x < width; x++)
            {
				meshData.vertices[vertexIndex] = new Vector3(topLeftX + x, heightMap[x, y] * heightMultiplier, topLeftZ - y);
				meshData.uvs[vertexIndex] = new Vector2(x / uvScale, y / uvScale);

				if (x < width - 1 && y < height - 1)
                {
					meshData.AddTriangle(vertexIndex, vertexIndex + width + 1, vertexIndex + width);
					meshData.AddTriangle(vertexIndex + width + 1, vertexIndex, vertexIndex + 1);
				}

				vertexIndex++;
			}
		}

		return meshData;
	}

    /// <summary>
    /// Generates a road mesh from a height map and a road map.
    /// </summary>
    /// <param name="roadMap">The road map to use.</param>
    /// <param name="heightMap">The height map to use.</param>
    /// <param name="mapWidth">The width of the map.</param>
    /// <param name="mapHeight">The height of the map.</param>
    /// <param name="heightMultiplier">The height multiplier to use.</param>
    /// <param name="borderWidth">The width of the border area.</param>
    /// <returns>The mesh created from the height map.</returns>
    public static MeshData GenerateRoadMesh(bool[,] roadMap, float[,] heightMap, int mapWidth, int mapHeight, float heightMultiplier, int borderWidth)
    {
        float topLeftX = (mapWidth - 1) / -2f;
        float topLeftZ = (mapHeight - 1) / 2f;

        MeshData meshData = new MeshData(mapWidth, mapHeight);
        int vertexIndex = 0;

        for (int x = 0; x < mapWidth; x++)
        {
            for (int y = 0; y < mapHeight; y++)
            {
                int adjustedX = x + borderWidth;
                int adjustedY = y;

                if (adjustedX >= mapWidth - 1 || adjustedY >= mapHeight - 1) continue;

                if (roadMap[x, y])
                {
                    float xCoord = topLeftX + adjustedX;
                    float yCoord = topLeftZ - adjustedY;

                    // Create vertices for a quad.
                    Vector3 vertex1 = new Vector3(xCoord, heightMap[adjustedX, adjustedY] * heightMultiplier, yCoord);
                    Vector3 vertex2 = new Vector3(xCoord + 1, heightMap[adjustedX + 1, adjustedY] * heightMultiplier, yCoord);
                    Vector3 vertex3 = new Vector3(xCoord + 1, heightMap[adjustedX + 1, adjustedY + 1] * heightMultiplier, yCoord - 1);
                    Vector3 vertex4 = new Vector3(xCoord, heightMap[adjustedX, adjustedY + 1] * heightMultiplier, yCoord - 1);

                    meshData.vertices[vertexIndex] = vertex1;
                    meshData.vertices[vertexIndex + 1] = vertex2;
                    meshData.vertices[vertexIndex + 2] = vertex3;
                    meshData.vertices[vertexIndex + 3] = vertex4;

                    // Create two triangles for the quad.
                    meshData.AddTriangle(vertexIndex, vertexIndex + 1, vertexIndex + 2);
                    meshData.AddTriangle(vertexIndex, vertexIndex + 2, vertexIndex + 3);

                    // Assign UVs.
                    meshData.uvs[vertexIndex] = new Vector2(0, 0);
                    meshData.uvs[vertexIndex + 1] = new Vector2(1, 0);
                    meshData.uvs[vertexIndex + 2] = new Vector2(1, 1);
                    meshData.uvs[vertexIndex + 3] = new Vector2(0, 1);

                    vertexIndex += 4;                  
                }
            }
        }

        return meshData;
    }
}

/// <summary>
/// Class containing mesh information and
/// the possibility of adding to said mesh.
/// </summary>
public class MeshData {
	public Vector3[] vertices;
	public int[] triangles;
	public Vector2[] uvs;

	int triangleIndex;

    /// <summary>
    /// Constructor for the MeshData class.
    /// </summary>
    /// <param name="meshWidth">Width of the mesh.</param>
    /// <param name="meshHeight">Height of the mesh.</param>
	public MeshData(int meshWidth, int meshHeight)
    {
		vertices = new Vector3[meshWidth * meshHeight];
		uvs = new Vector2[meshWidth * meshHeight];
		triangles = new int[(meshWidth - 1) * (meshHeight - 1) * 6];
	}

    /// <summary>
    /// Adds a triangle to the mesh data.
    /// </summary>
    /// <param name="a">Index of the first vertex of the triangle.</param>
    /// <param name="b">Index of the second vertex of the triangle.</param>
    /// <param name="c">Index of the third vertex of the triangle.</param>
	public void AddTriangle(int a, int b, int c)
    {
		triangles[triangleIndex] = a;
		triangles[triangleIndex + 1] = b;
		triangles[triangleIndex + 2] = c;
		triangleIndex += 3;
	}

    /// <summary>
    /// Creates the mesh from given vertices and their indices.
    /// </summary>
    /// <returns>The mesh as a Mesh object.</returns>
	public Mesh CreateMesh()
    {
		Mesh mesh = new Mesh();
		mesh.vertices = vertices;
		mesh.triangles = triangles;
		mesh.uv = uvs;
		mesh.RecalculateNormals();
		return mesh;
	}

}