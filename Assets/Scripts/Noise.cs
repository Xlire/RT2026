using UnityEngine;

/// <summary>
/// Class for generating noise maps.
/// </summary>
public class Noise
{
    /// <summary>
    /// Generate a noise map using Perlin noise.
    /// </summary>
    /// <param name="mapWidth">The width of the noise map.</param>
    /// <param name="mapHeight">The height of the noise map.</param>
    /// <param name="seed">The seed used for noise generation.</param>
    /// <param name="scale">The scale of the noise map.</param>
    /// <param name="borderScale">The scale of the border area.</param>
    /// <param name="borderSize">The size of the border area.</param>
    /// <param name="octaves">The number of octaves used for noise generation.</param>
    /// <param name="persistence">The persistence of the noise map.</param>
    /// <param name="lacunarity">The lacunarity of the noise map.</param>
    /// <param name="offset">The offset of the noise map.</param>
    /// <returns>The generated noise map. Returned values between [0, 1].</returns>
    public static float[,] GenerateNoiseMap(int mapWidth,  int mapHeight,     int seed, 
                                            float scale,   float borderScale, int borderSize,
                                            int octaves,   float persistence, float lacunarity, Vector2 offset)
    {
        float[,] noiseMap = new float[mapWidth, mapHeight];

        // Introduce pseudorandom offsets to break symmetry.
        System.Random prng = new System.Random(seed);
        Vector2[] octaveOffsets = new Vector2[octaves];

        for (int i = 0; i < octaves; i++)
        {
            float offsetX = prng.Next(-100000, 100000) + offset.x;
			float offsetY = prng.Next(-100000, 100000) + offset.y;
			octaveOffsets[i] = new Vector2(offsetX, offsetY);
        }

        // Ensure minimum scale.
        scale = Mathf.Max(scale, 0.0001f);

        // For centering the noise map.
        float halfWidth = mapWidth / 2f;
        float halfHeight = mapHeight / 2f;

        float maxNoiseHeight = float.MinValue; // Set as low as possible.
        float minNoiseHeight = float.MaxValue; // Set as high as possible.

        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                float noiseHeight = 0f;
                float amplitude = 1;
                float frequency = 1;

                float distX = Mathf.Min(x, mapWidth - x - 1);
                float distY = Mathf.Min(y, mapHeight - y - 1);
                float distance = Mathf.Min(distX, distY);

                float currentScale = (distance < borderSize) ? borderScale : scale;

                for (int i = 0; i < octaves; i++)
                {
                    float sampleX = (x - halfWidth) / (currentScale + 0.1f) * frequency + octaveOffsets[i].x;
                    float sampleY = (y - halfHeight) / (currentScale + 0.1f) * frequency + octaveOffsets[i].y;

                    float noiseValue = Mathf.PerlinNoise(sampleX, sampleY);
                    noiseHeight += noiseValue * amplitude;

                    amplitude *= persistence;
                    frequency *= lacunarity;
                }

                // Track the min and max noise height.
                if (noiseHeight > maxNoiseHeight) maxNoiseHeight = noiseHeight;
                else if (noiseHeight < minNoiseHeight) minNoiseHeight = noiseHeight;

                noiseMap[x, y] = noiseHeight;
            }
        }

        // Normalize the noise map.
        for (int y = 0; y < mapHeight; y++)
        {
            for (int x = 0; x < mapWidth; x++)
            {
                noiseMap[x, y] = Mathf.InverseLerp(minNoiseHeight, maxNoiseHeight, noiseMap[x, y]);
            }
        }

        return noiseMap;
    }
}
