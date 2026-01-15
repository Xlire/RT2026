using System.Collections.Generic;
using UnityEngine;
using PointCloudScanner;
using UnityEngine.EventSystems;
using System.Collections;

/// <summary>
/// Interface for player input handling.
/// Used to abstract player input handling for testing purposes.
/// </summary>
public interface IPlayerInput
{
    float GetAxis(string axisName);
    bool GetKey(KeyCode key);
    bool GetButton(string buttonName);
}

/// <summary>
/// Default implementation of IPlayerInput that uses Unity's Input class.
/// </summary>
public class PlayerMovementInput : IPlayerInput
{
    public float GetAxis(string axisName) => Input.GetAxis(axisName);
    public bool GetKey(KeyCode key) => Input.GetKey(key);
    public bool GetButton(string buttonName) => Input.GetButton(buttonName);
}

/// <summary>
/// Player movement script that handles player movement, jumping, crouching, and camera rotation.
/// </summary>
[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    public Camera PlayerCamera;

    [Header("Movement Settings")]
    [Range(1f, 10f)] public float WalkSpeed = 3f;
    [Range(1f, 10f)] public float RunSpeed = 6f;
    [HideInInspector] public float JumpPower = 7f;
    [HideInInspector] public float Gravity = 20f;
    [HideInInspector] public float LookSpeed = 2f;
    [HideInInspector] public float LookXLimit = 90f;
    [HideInInspector] public float DefaultHeight = 2f;
    [HideInInspector] public float CrouchHeight = 1f;
    [Range(1f, 10f)] public float CrouchSpeed = 3f;
    [HideInInspector] public float CrouchTransitionSpeed = 15f; // Speed of crouch height transition

    private Vector3 _moveDirection = Vector3.zero;
    private float _rotationX = 0;
    private CharacterController _characterController;
    private IPlayerInput _input;

    private readonly bool _canMove = true;
    private bool _canLook = true;
    private bool _isCurrentlyRunning = false; // Tracks running status
    private float _targetHeight; // Target height for crouching/standing
    private EventSystem _eventSystem;

    private InputSystem_Actions _inputActions;

    private Keybinds _keybinds;

    /// <summary>
    /// Called before the first frame update. Does various initializations.
    /// </summary>
    void Start()
    {
        _characterController = GetComponent<CharacterController>();
        _input = new PlayerMovementInput();
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Set initial height
        _targetHeight = DefaultHeight;

        // Initialize input system
        PointCloudManager scanner = FindFirstObjectByType<PointCloudManager>();
        if (_inputActions == null) // the object needs to be created earlier if keybinds need listeners
        {
            _inputActions = new InputSystem_Actions();
        }
        _inputActions.Player.Scan.performed += ctx => scanner.ScanObject();
        _inputActions.Enable();

        _keybinds = KeybindManager.Instance.keybinds;

        if (PlayerCamera == null)
        {
            Debug.LogWarning("Player camera not set, defaulting to main camera");
            PlayerCamera = Camera.main;
        }

        // Ignore tree colliders
        GameObject[] trees = GameObject.FindGameObjectsWithTag("Tree");
        foreach (GameObject tree in trees)
        {
            Collider treeCollider = tree.GetComponent<Collider>();
            if (treeCollider != null && treeCollider is MeshCollider)
            {
                Physics.IgnoreCollision(treeCollider, _characterController);
            }
        }
        // Wait for the SeenMesh GameObject to be created
        StartCoroutine(WaitForSeenMeshInitialization());

        var eventSystemObj = GameObject.Find("EventSystem");
        if (eventSystemObj != null)
            _eventSystem = eventSystemObj.GetComponent<EventSystem>();
    }

    /// <summary>
    /// Wait for the SeenMesh GameObject to be created before ignoring collisions
    /// </summary>
    private IEnumerator WaitForSeenMeshInitialization()
    {
        GameObject seenMesh;

        // Retry until the SeenMesh GameObject is created
        while ((seenMesh = GameObject.FindWithTag("ARMesh")) == null)
        {
            yield return null; // Wait for the next frame
        }

        // Ignore collisions with the SeenMesh
        Collider seenMeshCollider = seenMesh.GetComponent<Collider>();
        if (seenMeshCollider != null)
        {
            Physics.IgnoreCollision(seenMeshCollider, _characterController);
        }
    }

    /// <summary>
    /// Set the player input.
    /// Used to mock input for automated testing.
    /// </summary>
    public void SetInput(IPlayerInput input)
    {
        _input = input;
    }

    /// <summary>
    /// Update is called once per frame. Handles movement.
    /// </summary>
    private void Update()
    {
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        Vector3 right = transform.TransformDirection(Vector3.right);

        // Determine if the player is grounded
        bool isGrounded = _characterController.isGrounded;

        // Crouch logic (press and hold)
        if (_input.GetKey(_keybinds.Crouch) && _canMove)
        {
            _targetHeight = CrouchHeight;
        }
        else
        {
            _targetHeight = DefaultHeight;
        }

        // Smoothly adjust height
        _characterController.height = Mathf.Lerp(_characterController.height, _targetHeight, CrouchTransitionSpeed * Time.deltaTime);

        // Adjust speed while crouching
        bool isCrouching = Mathf.Abs(_characterController.height - CrouchHeight) < 0.1f;
        if (isCrouching)
        {
            _isCurrentlyRunning = false; // Cannot run while crouching
        }

        // Determine movement speed
        if (_input.GetKey(_keybinds.Run) && isGrounded && !isCrouching)
        {
            _isCurrentlyRunning = true; // Start running if grounded and shift is held
        }
        else if (isGrounded)
        {
            _isCurrentlyRunning = false; // Stop running if grounded and shift is not held
        }

        float currentSpeed = isCrouching ? CrouchSpeed : (_isCurrentlyRunning ? RunSpeed : WalkSpeed);
        float curSpeedX = _canMove ? currentSpeed * _input.GetAxis("Vertical") : 0;
        float curSpeedY = _canMove ? currentSpeed * _input.GetAxis("Horizontal") : 0;

        // Preserve Y-axis movement
        float movementDirectionY = _moveDirection.y;

        if (isGrounded)
        {
            // Update movement direction when grounded
            _moveDirection = (forward * curSpeedX) + (right * curSpeedY);
        }
        else
        {
            // Maintain momentum while airborne
            Vector3 horizontalVelocity = (forward * curSpeedX) + (right * curSpeedY);
            Vector3 inputVelocity = (forward * curSpeedX) + (right * curSpeedY);

            // Add input to current horizontal velocity
            if (inputVelocity != Vector3.zero)
            {
                horizontalVelocity = inputVelocity;
            }

            _moveDirection = horizontalVelocity;
        }

        // Apply Y-axis movement
        _moveDirection.y = movementDirectionY;

        // Jump logic
        if (_input.GetButton("Jump") && _canMove && isGrounded)
        {
            _moveDirection.y = JumpPower;
        }

        // Apply gravity if not grounded
        if (!isGrounded)
        {
            _moveDirection.y -= Gravity * Time.deltaTime;
        }
        // Move the character
        _characterController.Move(_moveDirection * Time.deltaTime);

        // Handle camera rotation
        if (_canMove && _canLook)
        {
            _rotationX += -_input.GetAxis("Mouse Y") * LookSpeed;
            _rotationX = Mathf.Clamp(_rotationX, -LookXLimit, LookXLimit);
            PlayerCamera.transform.localRotation = Quaternion.Euler(_rotationX, 0, 0);
            transform.rotation *= Quaternion.Euler(0, _input.GetAxis("Mouse X") * LookSpeed, 0);
        }

        // Toggle mouse lock state
        // Unlocking the mouse allows interacting with the AR game canvas.
        if (Input.GetKeyDown(_keybinds.ToggleMouse))
        {
            if (_canLook)
            {
                Cursor.lockState = CursorLockMode.Confined;
                Cursor.visible = true;
                _canLook = false;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                _canLook = true;
            }
        }

        // Simulate a press in the middle of the screen
        SimulateMiddlePress();
    }

    /// <summary>
    /// Simulates a press in the middle of the screen
    /// </summary>
    private void SimulateMiddlePress()
    {
        if (_eventSystem == null)
            return;

        const int leftButton = 0;
        bool lbDown = Input.GetMouseButtonDown(leftButton);
        bool lbUp = Input.GetMouseButtonUp(leftButton);

        if (!lbDown && !lbUp)
            return;

        // Find object(s) in the middle of the screen
        var screenCenter = new Vector2(Screen.width / 2, Screen.height / 2);
        var pointerData = new PointerEventData(_eventSystem) { position = screenCenter };
        var results = new List<RaycastResult>();
        _eventSystem.RaycastAll(pointerData, results);

        foreach (var result in results)
        {
            // Execute a pointer event on the UI object
            if (lbDown)
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerDownHandler);
            else
                ExecuteEvents.Execute(result.gameObject, pointerData, ExecuteEvents.pointerUpHandler);
        }
    }

    /// <summary>
    /// Enable input actions when the script is enabled
    /// </summary>
    private void OnEnable()
    {
        if (_inputActions == null)
        {
            _inputActions = new InputSystem_Actions(); // Initialize if not already done
        }
        _inputActions.Enable();
        _inputActions.UI.Exit.performed += OnExitPerformed;
    }

    /// <summary>
    /// Disable input actions when the script is disabled
    /// </summary>
    private void OnDisable()
    {
        if (_inputActions != null)
        {
            _inputActions.UI.Exit.performed -= OnExitPerformed;
            _inputActions.Disable();
        }
    }

    /// <summary>
    /// Called when the exit action is performed
    /// </summary>
    /// <param name="context">The input action context</param>
    private void OnExitPerformed(UnityEngine.InputSystem.InputAction.CallbackContext context)
    {
        Application.Quit();
    }
}
