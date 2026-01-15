using UnityEngine;

/// <summary>
/// Class for controlling UFO movement in a game.
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UfoMovement : MonoBehaviour
{
    public float speed = 5f; // Speed of the UFO
    public Vector3 direction; // Direction of movement
    private Rigidbody rb;

    private bool isDescending = true; // Flag to control descent
    private float targetY; // Target Y position after descent

    public float oscillationAmplitude = 0.05f; // Amplitude of the up-and-down movement
    public float oscillationFrequency = 2f; // Frequency of the up-and-down movement
    private float oscillationOffset; // Offset for the sine wave        

    private float mapWidth; // Width of the map
    private float mapHeight; // Height of the map
    private const float destroyOffset = 10f; // How many blocks outside map before destroying

    /// <summary>
    /// Initialize the UFO's movement parameters.
    /// </summary>
    private void Start()
    {
        // Assign a random direction for the UFO to move
        direction = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)).normalized;

        // Get the Rigidbody component
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true; // Ensure Rigidbody is kinematic

        // Set the target Y position for descent
        targetY = transform.position.y - 20f;

        // Initialize the oscillation offset
        oscillationOffset = Random.Range(0f, Mathf.PI * 2); // Randomize to avoid synchronized movement

        mapWidth = 100;
        mapHeight = 100;
    }

    /// <summary>
    /// Handle the UFO's movement and oscillation.
    /// </summary>
    private void FixedUpdate()
    {
        if (isDescending)
        {
            // Move the UFO downward until it reaches the target Y position
            speed = 10f;
            Vector3 newPosition = rb.position;
            newPosition.y = Mathf.MoveTowards(newPosition.y, targetY, speed * Time.fixedDeltaTime);

            rb.MovePosition(newPosition);

            // Check if the descent is complete
            if (Mathf.Approximately(newPosition.y, targetY))
            {
                isDescending = false; // Stop descending
                speed = 5f; // Reset speed for horizontal movement
            }
        }
        else
        {
            // Add oscillation to the Y position
            float oscillation = Mathf.Sin(Time.time * oscillationFrequency + oscillationOffset) * oscillationAmplitude;

            // Move the UFO in the assigned direction with oscillation
            Vector3 newPosition = rb.position + direction * speed * Time.fixedDeltaTime;
            newPosition.y += oscillation; // Add the oscillation to the Y position

            rb.MovePosition(newPosition);

            // Check if UFO is outside map boundaries with offset
            CheckBoundaries();
        }

    }

    /// <summary>
    /// Check if the UFO is outside the map boundaries and destroy it if so.
    /// </summary>
    private void CheckBoundaries()
    {
        // Check if the UFO is outside the map boundaries with the destroy offset
        if (rb.position.x < -mapWidth - destroyOffset || rb.position.x > mapWidth + destroyOffset ||
            rb.position.z < -mapHeight - destroyOffset || rb.position.z > mapHeight + destroyOffset)
        {
            // Destroy the UFO game object
            Destroy(gameObject);
        }
    }
}
