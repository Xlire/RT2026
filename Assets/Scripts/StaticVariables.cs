using UnityEngine;

/// <summary>
/// This script is responsible for storing static variables.
/// </summary>
class StaticVariables : MonoBehaviour
{
    public static string starttime = System.DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss"); // The time the simulation started
    public static string identifier = "simforscan"; // The identifier for the simulation
}