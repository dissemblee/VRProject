using UnityEngine;

public class Grabable : MonoBehaviour
{
    public Camera mainCamera;
    public float moveSpeed = 15f;
    private Ingredient ingredient;
    private Package package;
    private Rigidbody rb;
    private bool isGrabbed = false;

    private void Awake()
    {
        ingredient = GetComponent<Ingredient>();
        package = GetComponent<Package>();
        rb = GetComponent<Rigidbody>();

        if (mainCamera == null) mainCamera = Camera.main;
    }

    private void Update()
    {
        if (mainCamera == null) return;

        HandleMouseInput();

        if (isGrabbed) FollowMouse();
    }

    private void HandleMouseInput()
    {
        if (Input.GetMouseButtonDown(0)) TryPickUp();

        if (Input.GetMouseButtonUp(0) && isGrabbed) Drop();
    }

    private void TryPickUp()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 3f))
        {
            if (hit.transform == transform)
            {
                isGrabbed = true;
                
                if (ingredient != null)
                    ingredient.SetGrabbed(true);
                if (package != null)
                    package.SetGrabbed(true);
            }
        }
    }

    private void FollowMouse()
    {
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, 10f))
        {
            Vector3 targetPosition = hit.point + Vector3.up * 0.1f;
            
            if (rb != null && rb.isKinematic)
            {
                transform.position = Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed);
            }
            else
            {
                rb.MovePosition(Vector3.Lerp(transform.position, targetPosition, Time.deltaTime * moveSpeed));
            }
        }
    }

    private void Drop()
    {
        isGrabbed = false;

        if (ingredient != null)
            ingredient.SetGrabbed(false);
        if (package != null)
            package.SetGrabbed(false);
    }
}