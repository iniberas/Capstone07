using UnityEngine;
using UnityEngine.InputSystem;

public class DesktopMovement : MonoBehaviour
{
    [Header("Settings")]
    public float gravity = -10f;
    public float walkSpeed = 3f;
    public float lookSensitivity = 0.2f;
    public InputAction moveAction;
    public InputAction lookAction;
    public InputAction lockCursorAction;
    public InputAction unlockCursorAction;
    
    [Header("References")]
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
    }
    void Start()
    {
        UnlockCursor();
    }
    void OnEnable()
    {
        moveAction.Enable();
        lookAction.Enable();
        lockCursorAction.Enable();
        unlockCursorAction.Enable();
    }
    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        lockCursorAction.Disable();
        unlockCursorAction.Disable();
    }
    void Update()
    {
        HandleCursorLockState();
        HandleMouseLook();
        HandleMovement();
        controller.Move(transform.up * gravity * Time.deltaTime);
    }
    void HandleCursorLockState()
    {
        if (lockCursorAction.WasPressedThisFrame())
        {
            LockCursor();
        }
        else if (unlockCursorAction.WasPressedThisFrame())
        {
            UnlockCursor();
        }
    }
    void LockCursor()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
    void UnlockCursor()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
    void HandleMouseLook()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        Vector2 lookInput = lookAction.ReadValue<Vector2>();
        
        float mouseX = lookInput.x * lookSensitivity;
        float mouseY = lookInput.y * lookSensitivity;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -90f, 90f);

        playerCamera.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
        transform.Rotate(Vector3.up * mouseX);
    }
    void HandleMovement()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        Vector3 move = transform.right * moveInput.x + transform.forward * moveInput.y;
        controller.Move(move * walkSpeed * Time.deltaTime);
    }
}