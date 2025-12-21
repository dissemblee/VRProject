using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Text;

public class OrderPoint : MonoBehaviour
{
    [Header("Order Point Settings")]
    public GameObject highlightObject;
    public KeyCode interactionKey = KeyCode.E;
    
    [Header("UI Settings")]
    public GameObject interactionUI;
    public GameObject orderInfoUI;
    public Text orderInfoText;
    
    private bool playerInRange = false;
    private List<OrderManager.PendingOrder> availableOrders = new List<OrderManager.PendingOrder>();
    private float lastUpdateTime = 0f;
    private const float updateInterval = 0.5f;
    void Start()
    {
        SetActive(highlightObject, false);
        SetActive(interactionUI, false);
        SetActive(orderInfoUI, false);
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderReadyForPickup += OnOrderReadyForPickup;
            OrderManager.Instance.OnOrderAccepted += OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
        }
    }
    
    void OnDestroy()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderReadyForPickup -= OnOrderReadyForPickup;
            OrderManager.Instance.OnOrderAccepted -= OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        }
    }
    
    void Update()
    {
        if (Time.time - lastUpdateTime > updateInterval && playerInRange)
        {
            UpdateAvailableOrders();
            lastUpdateTime = Time.time;
        }
        
        if (playerInRange && Input.GetKeyDown(interactionKey)) 
        {
            AcceptNextOrder();
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            SetActive(highlightObject, true);
            
            UpdateAvailableOrders();
            
            SetActive(interactionUI, true);
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            SetActive(highlightObject, false);
            SetActive(interactionUI, false);
            SetActive(orderInfoUI, false);
        }
    }
    
    void OnOrderReadyForPickup(int orderNumber)
    {
        UpdateAvailableOrders();
    }
    
    void OnOrderAccepted(int orderNumber)
    {
        UpdateAvailableOrders();
    }
    
    void OnOrderCompleted(int orderNumber)
    {
        UpdateAvailableOrders();
    }
    
    void UpdateAvailableOrders()
    {
        if (OrderManager.Instance != null)
        {
            availableOrders = OrderManager.Instance.GetPendingOrders();
            
            if (playerInRange)
            {
                if (availableOrders.Count > 0)
                {
                    ShowOrderInfo();
                }
                else if (orderInfoUI != null)
                {
                    orderInfoUI.SetActive(false);
                }
            }
        }
    }
    
    void AcceptNextOrder()
    {
        if (OrderManager.Instance != null && availableOrders.Count > 0)
        {
            OrderManager.PendingOrder order = availableOrders[0];
            
            OrderManager.Instance.AcceptPendingOrder(order.orderNumber);
            
            if (orderInfoText != null)
            {
                if (order.isSpecialCar)
                {
                    orderInfoText.text = $"⚠️ ОСОБЫЙ заказ #{order.orderNumber} принят!";
                }
                else
                {
                    orderInfoText.text = $"Заказ #{order.orderNumber} принят!";
                }
                SetActive(orderInfoUI, true);
                Invoke("HideOrderInfo", 2f);
            }
            
            UpdateAvailableOrders();
        }
    }
    
    void ShowOrderInfo()
    {
        if (orderInfoUI != null && availableOrders.Count > 0)
        {
            orderInfoUI.SetActive(true);
            
            if (orderInfoText != null)
            {
                OrderManager.PendingOrder order = availableOrders[0];
                orderInfoText.text = $"Заказ #{order.orderNumber}\n{order.burgerCount} бургеров\nНажмите {interactionKey}";
            }
        }
    }
    
    void HideOrderInfo()
    {
        if (orderInfoUI != null)
        {
            orderInfoUI.SetActive(false);
        }
    }
    
    void SetActive(GameObject obj, bool state)
    {
        if (obj != null) 
        {
            obj.SetActive(state);
        }
    }
    
    void OnGUI()
    {
        if (!playerInRange || Camera.main == null) return;
        
        Vector3 worldPos = transform.position + Vector3.up * 2.5f;
        
        Vector3 screenPos = Camera.main.WorldToScreenPoint(worldPos);
        
        if (screenPos.z <= 0) return;
        
        StringBuilder orderInfo = new StringBuilder();
        
        if (availableOrders.Count > 0)
        {
            orderInfo.AppendLine("🚗 ОЖИДАЮТ ПОДТВЕРЖДЕНИЯ:");
            orderInfo.AppendLine("═══════════════════════");
            
            foreach (var order in availableOrders)
            {
                orderInfo.AppendLine($"📋 Заказ #{order.orderNumber}");
                orderInfo.AppendLine($"🚗 Машина: {order.carName}");
                orderInfo.AppendLine($"🍔 Бургеров: {order.burgerCount}");
                orderInfo.AppendLine($"⏱ В ожидании: {Mathf.RoundToInt(Time.time - order.timeArrived)}с");
                orderInfo.AppendLine("─────────────────────");
            }
            
            orderInfo.AppendLine($"\n[E] - Принять первый заказ");
        }
        
        if (OrderManager.Instance != null && OrderManager.Instance.activeOrders.Count > 0)
        {
            orderInfo.AppendLine("\n✅ АКТИВНЫЕ ЗАКАЗЫ:");
            orderInfo.AppendLine("═══════════════════════");
            
            foreach (var order in OrderManager.Instance.activeOrders)
            {
                orderInfo.AppendLine($"📋 Заказ #{order.orderNumber}");
                orderInfo.AppendLine($"🚗 От: {order.source}");
                orderInfo.AppendLine($"🍔 Бургеров: {order.burgerCount}");
                orderInfo.AppendLine($"⏱ Активен: {Mathf.RoundToInt(Time.time - order.timeCreated)}с");
                orderInfo.AppendLine("─────────────────────");
            }
        }
        
        if (availableOrders.Count == 0 && 
            (OrderManager.Instance == null || OrderManager.Instance.activeOrders.Count == 0))
        {
            orderInfo.AppendLine("⏳ НЕТ ЗАКАЗОВ");
            orderInfo.AppendLine("Ожидание клиентов...");
        }
        
        string text = orderInfo.ToString();
        string[] lines = text.Split('\n');
        int lineCount = lines.Length;
        
        float lineHeight = 22f;
        float padding = 20f;
        float width = 350f;
        float height = lineCount * lineHeight + padding;

        Rect rect = new Rect(
            screenPos.x - width / 2,
            Screen.height - screenPos.y - height - 30,
            width,
            height
        );
        
        GUIStyle boxStyle = new GUIStyle(GUI.skin.box);
        boxStyle.normal.background = MakeTex(2, 2, new Color(0f, 0f, 0f, 0.85f));
        boxStyle.border = new RectOffset(10, 10, 10, 10);
        
        GUIStyle labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.fontSize = 13;
        labelStyle.normal.textColor = Color.white;
        labelStyle.richText = true;
        labelStyle.alignment = TextAnchor.UpperLeft;
        labelStyle.wordWrap = true;
        
        GUIStyle headerStyle = new GUIStyle(labelStyle);
        headerStyle.fontSize = 14;
        headerStyle.fontStyle = FontStyle.Bold;
        headerStyle.normal.textColor = Color.yellow;
        
        GUIStyle keyStyle = new GUIStyle(labelStyle);
        keyStyle.fontStyle = FontStyle.Bold;
        keyStyle.normal.textColor = new Color(1f, 0.8f, 0f);
        
        GUI.Box(rect, "", boxStyle);
        
        Rect textRect = new Rect(rect.x + 10, rect.y + 10, rect.width - 20, rect.height - 20);
        
        float y = textRect.y;
        foreach (string line in lines)
        {
            GUIStyle currentStyle = labelStyle;
            
            if (line.Contains("ОЖИДАЮТ ПОДТВЕРЖДЕНИЯ") || line.Contains("АКТИВНЫЕ ЗАКАЗЫ"))
            {
                currentStyle = headerStyle;
            }
            else if (line.Contains("Заказ #") || line.Contains("Машина:") || 
                     line.Contains("Бургеров:") || line.Contains("От:"))
            {
                currentStyle = keyStyle;
            }
            else if (line.Contains("[E]"))
            {
                currentStyle = headerStyle;
            }
            else if (line.StartsWith("════") || line.StartsWith("────"))
            {
                currentStyle = labelStyle;
                currentStyle.normal.textColor = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            }
            
            GUI.Label(new Rect(textRect.x, y, textRect.width, lineHeight), line, currentStyle);
            y += lineHeight;
        }
        
        if (availableOrders.Count > 0)
        {
            Rect hintRect = new Rect(
                screenPos.x - 100,
                rect.y + rect.height + 5,
                200,
                30
            );
            
            GUIStyle hintStyle = new GUIStyle(GUI.skin.label);
            hintStyle.fontSize = 14;
            hintStyle.fontStyle = FontStyle.Bold;
            hintStyle.normal.textColor = Color.green;
            hintStyle.alignment = TextAnchor.MiddleCenter;
            
            GUI.Label(hintRect, $"[{interactionKey}] - ПРИНЯТЬ ЗАКАЗ", hintStyle);
        }
    }
    
    private Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];
        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;
        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();
        return result;
    }
}
