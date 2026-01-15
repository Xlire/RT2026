using System.Collections;
using System.Collections.Generic;

using UnityEngine;

/// <summary>
/// Class for spawning UFOs in a game.
/// </summary>
public class UfoSpawner : MonoBehaviour
{
    public GameObject ufoPrefab; // Reference to the UFO prefab
    public int maxUFOs = 100; // Maximum number of UFOs to spawn
    public float waitTime = 5f; // Wait time between spawns

    private List<GameObject> activeUFOs = new List<GameObject>();
    private Coroutine spawnCoroutine;

    /// <summary>
    /// Asynchronously spawn UFOs at random positions within a defined area.
    /// </summary>
    private void Start()
    {
        spawnCoroutine = StartCoroutine(SpawnUFOs());
    }

    /// <summary>
    /// Coroutine to spawn UFOs at random positions at regular intervals.
    /// </summary>
    /// <returns>IEnumerator for coroutine.</returns>
    private IEnumerator SpawnUFOs()
    {
        while (true) // Continuous spawning
        {
            // Clean up destroyed UFOs from the list
            activeUFOs.RemoveAll(ufo => ufo == null);

            SpawnUFO();

            yield return new WaitForSeconds(waitTime);
        }
    }

    /// <summary>
    /// Spawn a single UFO at a random position.
    /// </summary>
    private void SpawnUFO()
    {
        if (ufoPrefab == null)
        {
            Debug.LogError("MapGenerator or UFO prefab reference is missing!");
            return;
        }

        // Use mapWidth and mapHeight to define the spawn area
        const int spawnAreaWidth = 100;
        const int spawnAreaHeight = 100;
        float spawnX = Random.Range(-spawnAreaWidth / 2f, spawnAreaWidth / 2f);
        float spawnZ = Random.Range(-spawnAreaHeight / 2f, spawnAreaHeight / 2f);
        float spawnY = Random.Range(26f, 32f); // Spawn UFOs to a random height above the terrain

        Vector3 spawnPosition = new Vector3(spawnX, spawnY, spawnZ);
        GameObject newUFO = Instantiate(ufoPrefab, spawnPosition, Quaternion.identity);
        activeUFOs.Add(newUFO);
    }

    /// <summary>
    /// Clean up the coroutine when the object is destroyed.
    /// </summary>
    private void OnDestroy()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
        }
    }
}