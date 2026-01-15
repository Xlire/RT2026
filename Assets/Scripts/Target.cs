using UnityEngine;

/// <summary>
/// Class for handling target behavior in shooter games.
/// </summary>
public class Target : MonoBehaviour
{
    /// <summary>
    /// Kill the target when it is hit.
    /// </summary>
    public void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore();
        }
        // Destroy the target when it is hit
        Destroy(gameObject);
    }
    
}
