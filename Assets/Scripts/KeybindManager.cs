using UnityEngine;

/// <summary>
/// Struct used for storing keybinds.
/// </summary>
[System.Serializable]
public struct Keybinds
{
    public KeyCode Run;
    public KeyCode Crouch;
    public KeyCode TogglePointCloud;
    public KeyCode ToggleSeenMesh;
    public KeyCode ToggleMouse;
}

/// <summary>
/// Class used for handling keybinds. The basic movement
/// actions, like moving with WASD, as well as jumping,
/// do not utilize this class. They are hard-coded and
/// are printed into the legend by default.
/// </summary>
public class KeybindManager : MonoBehaviour
{
    // Create a KeybindManager singleton.
    public static KeybindManager Instance { get; private set; }

    public Keybinds keybinds;

    /// <summary>
    /// Set up the singleton instance and initialize keybinds.
    /// </summary>
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // INITIALIZE KEYBINDS HERE //
        keybinds = new Keybinds
        {
            Run = KeyCode.LeftShift,
            Crouch = KeyCode.LeftControl,
            TogglePointCloud = KeyCode.U,
            ToggleSeenMesh = KeyCode.Y,
            ToggleMouse = KeyCode.E
        };
    }

}
