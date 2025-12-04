using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }
    
    [Header("Order Settings")]
    public int minBurgersPerOrder = 1;
    public int maxBurgersPerOrder = 4;
    public int timeBetweenOrders = 30;
    
    [Header("Current Order")]
    public int currentOrderBurgers = 1;
    public bool hasActiveOrder = false;
    public float orderTimer = 0f;
    
    [Header("Order History")]
    public List<Order> orderHistory = new List<Order>();
    
    [System.Serializable]
    public class Order
    {
        public int orderNumber;
        public int burgerCount;
        public bool isCompleted;
        public float timeTaken;
    }
    
    private int orderCounter = 0;
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    void Start()
    {
        CreateNewOrder();
    }
    
    void Update()
    {
        if (!hasActiveOrder)
        {
            orderTimer += Time.deltaTime;
            if (orderTimer >= timeBetweenOrders)
            {
                CreateNewOrder();
                orderTimer = 0f;
            }
        }
    }
    
    public void CreateNewOrder()
    {
        orderCounter++;
        currentOrderBurgers = Random.Range(minBurgersPerOrder, maxBurgersPerOrder + 1);
        hasActiveOrder = true;
        
        Order newOrder = new Order
        {
            orderNumber = orderCounter,
            burgerCount = currentOrderBurgers,
            isCompleted = false,
            timeTaken = 0f
        };
        
        orderHistory.Add(newOrder);
        
        UpdateAllPackagesCapacity(currentOrderBurgers);
    }
    
    void UpdateAllPackagesCapacity(int newCapacity)
    {
        #if UNITY_2021_1_OR_NEWER
            Package[] allPackages = FindObjectsByType<Package>(FindObjectsSortMode.None);
        #else
            Package[] allPackages = FindObjectsOfType<Package>();
        #endif
        
        foreach (Package package in allPackages)
        {
            package.SetMaxCapacity(newCapacity);
        }
    }
    
    public void CompleteOrder(int orderNumber, float completionTime)
    {
        Order order = orderHistory.Find(o => o.orderNumber == orderNumber);
        if (order != null)
        {
            order.isCompleted = true;
            order.timeTaken = completionTime;
            hasActiveOrder = false;
            
            orderTimer = 0f;
        }
    }
    
    public void CompleteCurrentOrder(float completionTime)
    {
        if (hasActiveOrder)
        {
            CompleteOrder(orderCounter, completionTime);
        }
    }
    
    public int GetCurrentOrderNumber()
    {
        return orderCounter;
    }
    
    void OnGUI()
    {
        GUI.skin.label.fontSize = 20;
        
        if (hasActiveOrder)
        {
            GUI.Label(new Rect(10, 10, 300, 50), $"📋 ЗАКАЗ #{orderCounter}");
            GUI.Label(new Rect(10, 40, 300, 50), $"🍔 Бургеров: {currentOrderBurgers}");
            GUI.Label(new Rect(10, 70, 300, 50), $"⏱️ Таймер: {orderTimer:F0}/{timeBetweenOrders}с");
        }
        else
        {
            GUI.Label(new Rect(10, 10, 400, 50), "⏳ ОЖИДАНИЕ НОВОГО ЗАКАЗА...");
            GUI.Label(new Rect(10, 40, 400, 50), $"Следующий через: {timeBetweenOrders - orderTimer:F0}с");
        }
        
        int completedOrders = orderHistory.FindAll(o => o.isCompleted).Count;
        GUI.Label(new Rect(Screen.width - 250, 10, 240, 50), $"📊 Завершено: {completedOrders}/{orderHistory.Count}");
    }
    
    public int GetActiveOrderCount()
    {
        return orderHistory.FindAll(o => !o.isCompleted).Count;
    }
}