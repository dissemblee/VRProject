using UnityEngine;

public class OrderPoint : MonoBehaviour
{
    [Header("Order Point Settings")]
    public GameObject highlightObject;
    public float interactionDistance = 2f;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Settings")]
    public GameObject interactionUI;
    public GameObject orderInfoUI;
    
    private bool playerInRange = false;
    private Camera playerCamera;
    
    void Start()
    {
        if (highlightObject != null) highlightObject.SetActive(false);
            
        if (interactionUI != null) interactionUI.SetActive(false);
            
        if (orderInfoUI != null) orderInfoUI.SetActive(false);
    }
    
    void Update()
    {
        if (playerInRange && Input.GetKeyDown(interactionKey)) AcceptOrder();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerCamera = other.GetComponentInChildren<Camera>();
            
            if (highlightObject != null) highlightObject.SetActive(true);
                
            if (interactionUI != null) interactionUI.SetActive(true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            
            if (highlightObject != null) highlightObject.SetActive(false);
                
            if (interactionUI != null) interactionUI.SetActive(false);
                
            if (orderInfoUI != null) orderInfoUI.SetActive(false);
        }
    }
    
    void AcceptOrder()
    {
        if (OrderManager.Instance != null)
        {
            if (!OrderManager.Instance.hasActiveOrder)
            {
                OrderManager.Instance.CreateNewOrder();
                ShowOrderInfo();
            }
            else
            {
                Debug.Log("Уже есть активный заказ!");
            }
        }
    }
    
    void ShowOrderInfo()
    {
        if (orderInfoUI != null)
        {
            orderInfoUI.SetActive(true);
            
            Debug.Log("Заказ принят через кассовый аппарат!");
        }
    }
    
    void OnGUI()
    {
        if (playerInRange)
        {
            Vector3 screenPos = playerCamera.WorldToScreenPoint(transform.position);
            GUI.skin.label.fontSize = 14;
            GUI.Label(new Rect(screenPos.x - 50, Screen.height - screenPos.y - 50, 100, 30), 
                     $"Нажмите {interactionKey} для заказа");
        }
    }
}
