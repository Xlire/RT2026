using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Class for managing a game's scoring.
/// </summary>
public class ScoreManager : MonoBehaviour
{
    public static ScoreManager Instance { get; private set; } // Singleton instance

    public Text scoreText;

    private int score = 0;

    /// <summary>
    /// Make a singleton.
    /// </summary>
    void Awake()
    {
        Instance = this;
    }

    /// <summary>
    /// Set the score text to the current score.
    /// </summary>
    void Start()
    {
        scoreText.text = "Score: " + score.ToString();
    }

    /// <summary>
    /// Add to the score of the game.
    /// </summary>
    public void AddScore()
    {
        score++;
        scoreText.text = "Score: " + score.ToString();
    }
}
