using System.Collections;
using System.Collections.Generic;

using NUnit.Framework;

using PointCloudScanner;

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;


public class PlayerMovementTest
{
    private GameObject _player;
    private PlayerMovement _playerMovement;
    private CharacterController _characterController;

    [UnitySetUp]
    public IEnumerator SetUp()
    {
        // Ensure the Terrain scene is included in the build settings
        if (!Application.CanStreamedLevelBeLoaded("Terrain"))
        {
            Assert.Fail("The Terrain scene is not included in the build settings. Please add it to the build settings.");
        }

        // Disable visualization.
        // The Unity VFX Graph doesn't seem to work in the CI pipeline,
        // as it results in errors and causes the tests to fail.
        PointCloudVisualizer.Disable = true;
        PointCloudManager.DisableCamColor = true;

        // Load the Terrain scene
        SceneManager.LoadScene("Terrain");
        yield return new WaitForSeconds(3f); // Wait for the scene to load

        // Verify that the correct scene is loaded
        Assert.AreEqual("Terrain", SceneManager.GetActiveScene().name, "Failed to load the Terrain scene.");

        // Check that relevant objects and components are present
        _player = GameObject.FindWithTag("Player");
        Assert.IsNotNull(_player, "Player object not found in the scene.");

        var mainCamera = GameObject.FindWithTag("MainCamera");
        Assert.IsNotNull(mainCamera, "MainCamera object not found in the scene.");

        _playerMovement = _player.GetComponent<PlayerMovement>();
        Assert.IsNotNull(_playerMovement, "PlayerMovement component not found on the Player object.");

        _characterController = _player.GetComponent<CharacterController>();
        Assert.IsNotNull(_characterController, "CharacterController component not found on the Player object.");

        yield return null; // Allow one frame for initialization
    }

    [UnityTest]
    public IEnumerator PlayerWalksForward()
    {
        // Set up mock input
        var mockInput = new MockPlayerInput();
        mockInput.SetAxis("Vertical", 1f); // Simulate pressing "W"

        // Inject mock input into PlayerMovement
        _playerMovement.SetInput(mockInput);

        // Record the initial position of the player
        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        // Assert movement
        Assert.Greater(_player.transform.position.z, initialPosition.z);
    }

    [UnityTest]
    public IEnumerator PlayerWalksBackward()
    {
        var mockInput = new MockPlayerInput();
        mockInput.SetAxis("Vertical", -1f); // Simulate pressing "S"

        _playerMovement.SetInput(mockInput);

        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.Less(_player.transform.position.z, initialPosition.z);
    }

    [UnityTest]
    public IEnumerator PlayerWalksLeft()
    {
        var mockInput = new MockPlayerInput();
        mockInput.SetAxis("Horizontal", -1f); // Simulate pressing "A"

        _playerMovement.SetInput(mockInput);

        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.Less(_player.transform.position.x, initialPosition.x);
    }

    [UnityTest]
    public IEnumerator PlayerWalksRight()
    {
        var mockInput = new MockPlayerInput();
        mockInput.SetAxis("Horizontal", 1f); // Simulate pressing "D"

        _playerMovement.SetInput(mockInput);

        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.Greater(_player.transform.position.x, initialPosition.x);
    }

    [UnityTest]
    public IEnumerator PlayerRuns()
    {
        var mockInput = new MockPlayerInput();
        mockInput.SetAxis("Vertical", 1f); // Simulate pressing "W"
        mockInput.PressKey(KeyCode.LeftShift); // Simulate holding "LeftShift" for running

        _playerMovement.SetInput(mockInput);

        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.Greater(_player.transform.position.z, initialPosition.z);
        Assert.Greater(_player.transform.position.z - initialPosition.z, _playerMovement.WalkSpeed, "Player did not run faster than walking speed.");
    }

    [UnityTest]
    public IEnumerator PlayerCrouches()
    {
        var mockInput = new MockPlayerInput();

        // Record the initial height of the CharacterController
        float initialHeight = _characterController.height;

        mockInput.PressKey(KeyCode.LeftControl); // Simulate pressing "LeftControl" for crouching

        _playerMovement.SetInput(mockInput);

        yield return new WaitForSeconds(1f);

        Assert.Less(_characterController.height, initialHeight);
        Assert.IsTrue(Mathf.Abs(_playerMovement.CrouchHeight - _characterController.height) < 0.01f, "Player crouch height is incorrect.");
    }

    [UnityTest]
    public IEnumerator PlayerJumps()
    {
        // Set up mock input
        var mockInput = new MockPlayerInput();
        mockInput.PressButton("Jump"); // Simulate pressing "Space" for jumping

        _playerMovement.SetInput(mockInput);

        Vector3 initialPosition = _player.transform.position;

        yield return new WaitForSeconds(1f);

        Assert.Greater(_player.transform.position.y, initialPosition.y, "Player did not jump.");
    }

    [UnityTest]
    public IEnumerator PlayerRotatesCamera()
    {
        var mockInput = new MockPlayerInput();

        // Record the initial rotation of the camera
        Quaternion initialCameraRotation = _playerMovement.PlayerCamera.transform.localRotation;
        Quaternion initialPlayerRotation = _player.transform.rotation;

        mockInput.SetAxis("Mouse X", 1f); // Simulate moving the mouse horizontally
        mockInput.SetAxis("Mouse Y", -1f); // Simulate moving the mouse vertically

        _playerMovement.SetInput(mockInput);

        yield return new WaitForSeconds(1f); // Allow rotation to occur

        Quaternion finalCameraRotation = _playerMovement.PlayerCamera.transform.localRotation;
        Quaternion finalPlayerRotation = _player.transform.rotation;

        Assert.AreNotEqual(initialCameraRotation, finalCameraRotation, "Camera rotation did not change.");
        Assert.AreNotEqual(initialPlayerRotation, finalPlayerRotation, "Player rotation did not change.");
    }

    [UnityTearDown]
    public IEnumerator TearDown()
    {
        SceneManager.LoadScene("Terrain"); // ensure terrain scene is reloaded to base state
        yield return new WaitForSeconds(3f);
    }
}

// Mock implementation of IPlayerInput for testing
public class MockPlayerInput : IPlayerInput
{
    private Dictionary<string, float> _axisValues = new Dictionary<string, float>();
    private HashSet<KeyCode> _keys = new HashSet<KeyCode>();
    private HashSet<string> _buttons = new HashSet<string>();

    public void SetAxis(string axisName, float value)
    {
        _axisValues[axisName] = value;
    }

    public void PressKey(KeyCode key)
    {
        _keys.Add(key);
    }

    public void ReleaseKey(KeyCode key)
    {
        _keys.Remove(key);
    }

    public void PressButton(string buttonName)
    {
        _buttons.Add(buttonName);
    }

    public void ReleaseButton(string buttonName)
    {
        _buttons.Remove(buttonName);
    }

    public float GetAxis(string axisName)
    {
        return _axisValues.ContainsKey(axisName) ? _axisValues[axisName] : 0f;
    }

    public bool GetKey(KeyCode key)
    {
        return _keys.Contains(key);
    }

    public bool GetButton(string buttonName)
    {
        return _buttons.Contains(buttonName);
    }
}