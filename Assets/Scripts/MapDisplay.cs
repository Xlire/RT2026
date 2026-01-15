using UnityEngine;

/// <summary>
/// Class for handling the display of the map.
/// </summary>
public class MapDisplay : MonoBehaviour
{
    public enum DrawMeshMode { Terrain, Road, Mountain };
    public Renderer textureRenderer;
    public MeshFilter meshFilterTerrain;
    public MeshCollider meshColliderTerrain;
    public MeshRenderer meshRendererTerrain;
    public MeshFilter meshFilterMountain;
    public MeshRenderer meshRendererMountain;
    public MeshFilter meshFilterRoad;
    public MeshCollider meshColliderRoad;
    public MeshRenderer meshRendererRoad;

    /// <summary>
    /// Draw the given texture.
    /// </summary>
    /// <param name="texture">The texture to draw.</param>
    public void DrawTexture(Texture2D texture)
    {
        // Send the texture to the renderer.
        textureRenderer.sharedMaterial.mainTexture = texture;
        textureRenderer.transform.localScale = new Vector3(texture.width, 1, texture.height);
    }

    /// <summary>
    /// Create a mesh and draw its texture.
    /// </summary>
    /// <param name="meshData">The mesh data to use.</param>
    /// <param name="material">The material to use.</param>
    /// <param name="mode">The mode to use for drawing the mesh (Terrain/Road/Mountain).</param>
    public void DrawMesh(MeshData meshData, Material material, DrawMeshMode mode)
    {
        Mesh mesh = meshData.CreateMesh();
        if (mode == DrawMeshMode.Terrain)
        {
            meshFilterTerrain.sharedMesh = mesh;
            meshColliderTerrain.sharedMesh = mesh;
            meshRendererTerrain.sharedMaterial = material;
        }
        else if (mode == DrawMeshMode.Road)
        {
            meshFilterRoad.sharedMesh = mesh;
            meshColliderRoad.sharedMesh = mesh;
            meshRendererRoad.sharedMaterial = material;
        }
        else if (mode == DrawMeshMode.Mountain)
        {
            meshFilterMountain.sharedMesh = mesh;
            meshRendererMountain.sharedMaterial = material;
        }
    }
}
