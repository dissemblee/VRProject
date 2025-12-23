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
    
    [Header("Sound Settings")]
    public AudioClip acceptOrderSound;
    public AudioClip hoverSound;
    public AudioClip errorSound;
    private AudioSource audioSource;
    
    [Header("VR Settings")]
    public bool useVRControls = false;
    public string vrInteractButton = "XRI_Right_TriggerButton"; // Для XR Interaction Toolkit
    public float vrInteractionDistance = 2f;
    private Transform vrPlayer;
    
    private bool playerInRange = false;
    private List<OrderManager.PendingOrder> availableOrders = new List<OrderManager.PendingOrder>();
    private float lastUpdateTime = 0f;
    private const float updateInterval = 0.5f;
    private bool canInteract = true;
    private float interactCooldown = 0.5f;
    private float lastInteractTime = 0f;
    
    void Start()
    {
        // Настройка AudioSource
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
        
        audioSource.spatialBlend = 0.8f; // 3D звук
        audioSource.maxDistance = 10f;
        
        SetActive(highlightObject, false);
        SetActive(interactionUI, false);
        SetActive(orderInfoUI, false);
        
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderReadyForPickup += OnOrderReadyForPickup;
            OrderManager.Instance.OnOrderAccepted += OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
        }
        
        // Поиск VR игрока
        FindVRPlayer();
    }
    
    void FindVRPlayer()
    {
        if (useVRControls)
        {
            // Поиск VR камеры или контроллера
            GameObject vrCamera = GameObject.Find("XR Origin")?.transform?.Find("Camera")?.gameObject;
            if (vrCamera == null)
                vrCamera = GameObject.Find("CenterEyeAnchor");
            if (vrCamera == null)
                vrCamera = GameObject.Find("Main Camera");
            
            if (vrCamera != null)
            {
                vrPlayer = vrCamera.transform;
            }
            
            // Если не нашли, проверяем наличие XR компонентов
            if (vrPlayer == null)
            {
                UnityEngine.XR.XRDisplaySubsystem display = GetComponent<UnityEngine.XR.XRDisplaySubsystem>();
                if (display != null && display.running)
                {
                    useVRControls = true;
                    vrPlayer = Camera.main?.transform;
                }
                else
                {
                    useVRControls = false;
                    Debug.Log("VR не обнаружен, переключаемся на клавиатуру");
                }
            }
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
        // Проверка VR расстояния
        if (useVRControls && vrPlayer != null)
        {
            float distance = Vector3.Distance(transform.position, vrPlayer.position);
            bool wasInRange = playerInRange;
            playerInRange = distance <= vrInteractionDistance;
            
            // Проиграть звук при приближении
            if (!wasInRange && playerInRange && hoverSound != null)
            {
                PlaySound(hoverSound, 0.3f);
            }
        }
        
        if (Time.time - lastUpdateTime > updateInterval && playerInRange)
        {
            UpdateAvailableOrders();
            lastUpdateTime = Time.time;
        }
        
        // Проверка взаимодействия
        if (playerInRange && canInteract && Time.time - lastInteractTime > interactCooldown)
        {
            if (useVRControls)
            {
                CheckVRInteraction();
            }
            else if (Input.GetKeyDown(interactionKey))
            {
                AcceptNextOrder();
            }
        }
    }
    
    void CheckVRInteraction()
    {
        bool vrInput = false;
        
        // Проверка различных систем ввода VR
        if (!string.IsNullOrEmpty(vrInteractButton))
        {
            // Для XR Interaction Toolkit
            vrInput = Input.GetButtonDown(vrInteractButton);
            
            // Альтернативные контролы
            if (!vrInput)
            {
                vrInput = Input.GetAxis("XRI_Right_Trigger") > 0.5f ||
                         Input.GetKeyDown(KeyCode.JoystickButton0) || // A на Oculus
                         Input.GetKeyDown(KeyCode.JoystickButton1) || // B на Oculus
                         Input.GetKeyDown(KeyCode.JoystickButton2) || // X на Oculus
                         Input.GetKeyDown(KeyCode.JoystickButton3);   // Y на Oculus
            }
        }
        
        if (vrInput)
        {
            // Проверка, смотрит ли игрок на объект (опционально)
            if (vrPlayer != null)
            {
                Vector3 direction = (transform.position - vrPlayer.position).normalized;
                float dot = Vector3.Dot(vrPlayer.forward, direction);
                
                if (dot > 0.7f) // Смотрит ли в сторону объекта
                {
                    AcceptNextOrder();
                }
                else
                {
                    // Проиграть звук ошибки
                    if (errorSound != null)
                    {
                        PlaySound(errorSound, 0.2f);
                    }
                }
            }
            else
            {
                AcceptNextOrder();
            }
        }
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (!useVRControls && other.CompareTag("Player"))
        {
            playerInRange = true;
            SetActive(highlightObject, true);
            
            UpdateAvailableOrders();
            
            SetActive(interactionUI, true);
            
            // Проиграть звук при приближении
            if (hoverSound != null)
            {
                PlaySound(hoverSound, 0.3f);
            }
        }
    }
    
    void OnTriggerExit(Collider other)
    {
        if (!useVRControls && other.CompareTag("Player"))
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
        if (!canInteract || Time.time - lastInteractTime < interactCooldown)
            return;
        
        if (OrderManager.Instance != null && availableOrders.Count > 0)
        {
            OrderManager.PendingOrder order = availableOrders[0];
            
            OrderManager.Instance.AcceptPendingOrder(order.orderNumber);
            
            // Проиграть звук принятия заказа
            if (acceptOrderSound != null)
            {
                PlaySound(acceptOrderSound, 0.5f);
            }
            
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
            
            // КД на взаимодействие
            lastInteractTime = Time.time;
            canInteract = false;
            Invoke("ResetInteract", interactCooldown);
        }
        else
        {
            // Проиграть звук ошибки
            if (errorSound != null)
            {
                PlaySound(errorSound, 0.2f);
            }
        }
    }
    
    void ResetInteract()
    {
        canInteract = true;
    }
    
    void ShowOrderInfo()
    {
        if (orderInfoUI != null && availableOrders.Count > 0)
        {
            orderInfoUI.SetActive(true);
            
            if (orderInfoText != null)
            {
                OrderManager.PendingOrder order = availableOrders[0];
                string controlText = useVRControls ? "ТРИГГЕР" : interactionKey.ToString();
                orderInfoText.text = $"Заказ #{order.orderNumber}\n{order.burgerCount} бургеров\nНажмите {controlText}";
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
    
    void PlaySound(AudioClip clip, float volume = 1f)
    {
        if (clip != null && audioSource != null)
        {
            audioSource.PlayOneShot(clip, volume);
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
            
            string controlText = useVRControls ? "ТРИГГЕР" : $"[{interactionKey}]";
            orderInfo.AppendLine($"\n{controlText} - Принять первый заказ");
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
            else if (line.Contains("[E]") || line.Contains("ТРИГГЕР"))
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
            
            string hintText = useVRControls ? "ТРИГГЕР - ПРИНЯТЬ ЗАКАЗ" : $"[{interactionKey}] - ПРИНЯТЬ ЗАКАЗ";
            GUI.Label(hintRect, hintText, hintStyle);
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
    
    // Для VR можно добавить визуальную обратную связь
    void OnDrawGizmos()
    {
        if (useVRControls)
        {
            Gizmos.color = playerInRange ? Color.green : Color.yellow;
            Gizmos.DrawWireSphere(transform.position, vrInteractionDistance);
        }
    }
}