using UnityEngine;
using System.Collections;

public class Grabable : MonoBehaviour
{
    [Header("VR Settings")]
    public Transform leftController;
    public Transform rightController;
    public float grabDistance = 0.2f;
    public float throwForce = 1.5f;
    
    [Header("Input Settings")]
    public string grabButton = "Fire1";
    public string throwButton = "Fire2";
    
    [Header("Legacy Camera Support")]
    public Camera mainCamera;
    
    [Header("Visual Feedback")]
    public Material highlightMaterial;
    private Material originalMaterial;
    private Renderer objectRenderer;
    
    private Rigidbody rb;
    private bool isGrabbed = false;
    private Transform grabbingController;
    private Vector3 previousPosition;
    private Quaternion previousRotation;
    
    private Ingredient ingredient;
    private Package package;
    
    private static Transform staticLeftController;
    private static Transform staticRightController;
    private static bool controllersFound = false;
    
    private enum ControllerHand { None, Left, Right }
    private ControllerHand currentHand = ControllerHand.None;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        ingredient = GetComponent<Ingredient>();
        package = GetComponent<Package>();
        objectRenderer = GetComponent<Renderer>();
        
        if (objectRenderer != null)
        {
            originalMaterial = objectRenderer.material;
        }
        
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
    }

    private void Start()
    {
        FindAndAssignControllers();
    }

    private void Update()
    {
        if ((leftController == null || rightController == null) && !controllersFound)
        {
            FindAndAssignControllers();
        }
        
        HandleMixedInput();
        
        if (isGrabbed)
        {
            if (grabbingController != null)
            {
                FollowController();
                CheckForVRRelease();
            }
            else
            {
                FollowMouse();
                CheckForMouseRelease();
            }
        }
        
        UpdateHighlight();
    }

    private void HandleMixedInput()
    {
        if (!isGrabbed)
        {
            CheckControllerForGrab(leftController, ControllerHand.Left);
            CheckControllerForGrab(rightController, ControllerHand.Right);
            
            if (Input.GetMouseButtonDown(0))
            {
                TryMousePickUp();
            }
        }
    }

    private void TryMousePickUp()
    {
        if (mainCamera == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.transform == transform)
            {
                Grab(null);
            }
        }
    }

    private void FollowMouse()
    {
        if (mainCamera == null) return;
        
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Vector3 targetPosition = hit.point + Vector3.up * 0.1f;
            
            if (rb != null)
            {
                rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f));
            }
            else
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * 15f);
            }
        }
    }

    private void CheckForMouseRelease()
    {
        if (Input.GetMouseButtonUp(0))
        {
            Release();
        }
    }

    private void FindAndAssignControllers()
    {
        if (controllersFound)
        {
            if (leftController == null) leftController = staticLeftController;
            if (rightController == null) rightController = staticRightController;
            return;
        }
        
        GameObject leftControllerObj = GameObject.FindGameObjectWithTag("LeftController");
        GameObject rightControllerObj = GameObject.FindGameObjectWithTag("RightController");
        
        if (leftControllerObj != null)
        {
            leftController = leftControllerObj.transform;
            staticLeftController = leftController;
        }
        
        if (rightControllerObj != null)
        {
            rightController = rightControllerObj.transform;
            staticRightController = rightController;
        }
        
        if (leftController == null)
        {
            GameObject left = GameObject.Find("LeftHand Controller") ?? 
                            GameObject.Find("Left Controller") ?? 
                            GameObject.Find("Controller (left)") ??
                            GameObject.Find("LeftHand") ??
                            GameObject.Find("LeftHandAnchor");
            if (left != null)
            {
                leftController = left.transform;
                staticLeftController = leftController;
            }
        }
        
        if (rightController == null)
        {
            GameObject right = GameObject.Find("RightHand Controller") ?? 
                             GameObject.Find("Right Controller") ?? 
                             GameObject.Find("Controller (right)") ??
                             GameObject.Find("RightHand") ??
                             GameObject.Find("RightHandAnchor");
            if (right != null)
            {
                rightController = right.transform;
                staticRightController = rightController;
            }
        }
        
        controllersFound = (staticLeftController != null || staticRightController != null);
    }

    private void CheckControllerForGrab(Transform controller, ControllerHand hand)
    {
        if (controller == null) return;
        
        float distance = Vector3.Distance(transform.position, controller.position);
        
        bool isButtonPressed = CheckGrabButtonPressed(hand);
        
        if (isButtonPressed && distance <= grabDistance)
        {
            Grab(controller);
            currentHand = hand;
        }
    }

    private bool CheckGrabButtonPressed(ControllerHand hand)
    {
        switch (hand)
        {
            case ControllerHand.Left:
                return Input.GetKeyDown(KeyCode.JoystickButton0) ||
                       Input.GetKeyDown(KeyCode.JoystickButton2) ||
                       Input.GetAxis("LeftTrigger") > 0.5f ||      
                       Input.GetButtonDown("LeftGrab") ||          
                       Input.GetMouseButtonDown(0);                
            
            case ControllerHand.Right:
                return Input.GetKeyDown(KeyCode.JoystickButton1) ||
                       Input.GetKeyDown(KeyCode.JoystickButton3) ||
                       Input.GetAxis("RightTrigger") > 0.5f ||     
                       Input.GetButtonDown("RightGrab") ||         
                       Input.GetMouseButtonDown(1);                
            
            default:
                return false;
        }
    }

    private bool CheckGrabButtonReleased(ControllerHand hand)
    {
        switch (hand)
        {
            case ControllerHand.Left:
                return Input.GetKeyUp(KeyCode.JoystickButton0) ||
                       Input.GetKeyUp(KeyCode.JoystickButton2) ||
                       Input.GetAxis("LeftTrigger") < 0.1f ||
                       Input.GetButtonUp("LeftGrab") ||
                       Input.GetMouseButtonUp(0);
            
            case ControllerHand.Right:
                return Input.GetKeyUp(KeyCode.JoystickButton1) ||
                       Input.GetKeyUp(KeyCode.JoystickButton3) ||
                       Input.GetAxis("RightTrigger") < 0.1f ||
                       Input.GetButtonUp("RightGrab") ||
                       Input.GetMouseButtonUp(1);
            
            default:
                return true;
        }
    }

    private void UpdateHighlight()
    {
        if (objectRenderer == null || highlightMaterial == null || originalMaterial == null)
            return;
            
        if (isGrabbed)
        {
            objectRenderer.material = highlightMaterial;
            return;
        }
        
        bool isHighlighted = false;
        
        if (leftController != null)
        {
            float distance = Vector3.Distance(transform.position, leftController.position);
            if (distance <= grabDistance * 1.5f)
            {
                isHighlighted = true;
            }
        }
        
        if (rightController != null && !isHighlighted)
        {
            float distance = Vector3.Distance(transform.position, rightController.position);
            if (distance <= grabDistance * 1.5f)
            {
                isHighlighted = true;
            }
        }
        
        objectRenderer.material = isHighlighted ? highlightMaterial : originalMaterial;
    }

    private void Grab(Transform controller)
    {
        isGrabbed = true;
        grabbingController = controller;
        previousPosition = transform.position;
        previousRotation = transform.rotation;
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (ingredient != null)
            ingredient.SetGrabbed(true);
        if (package != null)
            package.SetGrabbed(true);
        
        PlayGrabSound();
        
        Debug.Log($"Object grabbed by {(controller == null ? "Mouse" : "VR Controller")}");
    }

    private void FollowController()
    {
        if (grabbingController == null) return;
        
        transform.position = Vector3.Lerp(transform.position, 
                                         grabbingController.position, 
                                         Time.deltaTime * 20f);

        transform.rotation = Quaternion.Slerp(transform.rotation, 
                                            grabbingController.rotation, 
                                            Time.deltaTime * 15f);
    }

    private void CheckForVRRelease()
    {
        if (grabbingController == null) return;
        
        bool shouldRelease = CheckGrabButtonReleased(currentHand);
        
        bool shouldThrow = false;
        switch (currentHand)
        {
            case ControllerHand.Left:
                shouldThrow = Input.GetKeyDown(KeyCode.JoystickButton4) || // L1
                             Input.GetButtonDown("LeftThrow");
                break;
            case ControllerHand.Right:
                shouldThrow = Input.GetKeyDown(KeyCode.JoystickButton5) || // R1
                             Input.GetButtonDown("RightThrow");
                break;
        }
        
        if (shouldRelease)
        {
            Release();
        }
        else if (shouldThrow)
        {
            Throw();
        }
    }

    private void Release()
    {
        isGrabbed = false;
        
        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            
            // Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
            // rb.linearVelocity = velocity * 0.5f;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (objectRenderer != null && originalMaterial != null)
        {
            objectRenderer.material = originalMaterial;
        }
        
        if (ingredient != null)
            ingredient.SetGrabbed(false);
        if (package != null)
            package.SetGrabbed(false);
        
        grabbingController = null;
        currentHand = ControllerHand.None;
    }

    private void Throw()
    {
        if (grabbingController == null || rb == null) return;
        
        Vector3 velocity = (transform.position - previousPosition) / Time.deltaTime;
        
        Release();
        
        rb.AddForce(velocity * throwForce, ForceMode.Impulse);
        
        rb.AddTorque(Random.insideUnitSphere * throwForce * 0.3f, ForceMode.Impulse);
    }

    private void PlayGrabSound()
    {
        AudioSource audioSource = GetComponent<AudioSource>();
        if (audioSource != null && audioSource.clip != null)
        {
            audioSource.Play();
        }
    }

    public void SetMainCamera(Camera camera)
    {
        mainCamera = camera;
    }
    
    public void SetLeftController(Transform controller)
    {
        leftController = controller;
        if (controller != null) staticLeftController = controller;
    }
    
    public void SetRightController(Transform controller)
    {
        rightController = controller;
        if (controller != null) staticRightController = controller;
    }
    
    public static void SetGlobalControllers(Transform left, Transform right)
    {
        staticLeftController = left;
        staticRightController = right;
        controllersFound = (left != null || right != null);
        
        Grabable[] allGrabables = FindObjectsOfType<Grabable>();
        foreach (var grabable in allGrabables)
        {
            if (left != null) grabable.leftController = left;
            if (right != null) grabable.rightController = right;
        }
    }
}