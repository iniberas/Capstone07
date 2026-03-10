using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using System.Collections.Generic;

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
    public InputAction interactAction;
    
    [Header("References")]
    public Transform playerCamera;

    private CharacterController controller;
    private float xRotation = 0f;
    
    private GameObject currentHoverObject; 

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
        interactAction.Enable();
    }
    
    void OnDisable()
    {
        moveAction.Disable();
        lookAction.Disable();
        lockCursorAction.Disable();
        unlockCursorAction.Disable();
        interactAction.Disable();
    }
    
    void Update()
    {
        HandleCursorLockState();
        HandleMouseLook();
        HandleMovement();
        HandleHoverUI(); 
        HandleInteractionUI();
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
        
        if (currentHoverObject != null && EventSystem.current != null)
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current);
            ExecuteEvents.ExecuteHierarchy(currentHoverObject, pointerData, ExecuteEvents.pointerExitHandler);
            currentHoverObject = null;
        }
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

    void HandleHoverUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked || EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        GameObject hitObject = results.Count > 0 ? results[0].gameObject : null;

        if (hitObject != currentHoverObject)
        {
            if (currentHoverObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(currentHoverObject, pointerData, ExecuteEvents.pointerExitHandler);
            }

            if (hitObject != null)
            {
                ExecuteEvents.ExecuteHierarchy(hitObject, pointerData, ExecuteEvents.pointerEnterHandler);
            }

            currentHoverObject = hitObject;
        }
    }

    void HandleInteractionUI()
    {
        if (Cursor.lockState != CursorLockMode.Locked) return;

        if (interactAction.WasPressedThisFrame())
        {
            InteractUI();
        }
    }
    
    void InteractUI()
    {
        if (EventSystem.current == null) return;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = new Vector2(Screen.width / 2f, Screen.height / 2f)
        };

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerData, results);

        if (results.Count > 0)
        {
            GameObject clickedObject = results[0].gameObject;
            
            ExecuteEvents.ExecuteHierarchy(clickedObject, pointerData, ExecuteEvents.pointerClickHandler);
            
            Debug.Log($"Clicked on: {clickedObject.name}");
        }
    }
}