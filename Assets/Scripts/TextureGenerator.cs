using UnityEngine;

/// <summary>
/// Class used for generating textures.
/// </summary>
public static class TextureGenerator
{
    /// <summary>
    /// Create a texture from a height map.
    /// </summary>
    /// <param name="colorMap">The required color map.</param>
    /// <param name="width">The width of the texture.</param>
    /// <param name="height">The height of the texture.</param>
    /// <returns>The texture created from the height map.</returns>
    public static Texture2D TextureFromColorMap(Color[] colorMap, int width, int height)
    {
        Texture2D texture = new Texture2D(width, height);

		texture.filterMode = FilterMode.Point;
		texture.wrapMode = TextureWrapMode.Clamp;

		texture.SetPixels(colorMap);
		texture.Apply();

		return texture;
    }

    /// <summary>
    /// Fills colorMap with height-based colors.
    /// </summary>
    /// <param name="heightMap">Pseudo-random height map.</param>
    /// <returns>The texture to be used.</returns>
    public static Texture2D TextureFromHeightMap(float[,] heightMap)
    {
		int width = heightMap.GetLength(0);
		int height = heightMap.GetLength(1);

		Color[] colorMap = new Color[width * height];
		for (int y = 0; y < height; y++) 
        {
			for (int x = 0; x < width; x++) 
            {
				colorMap[y * width + x] = Color.Lerp(Color.black, Color.white, heightMap[x, y]);
			}
		}

		return TextureFromColorMap(colorMap, width, height);
	}
}