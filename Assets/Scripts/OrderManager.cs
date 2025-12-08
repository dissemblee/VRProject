using UnityEngine;
using System.Collections.Generic;

public class OrderManager : MonoBehaviour
{
    public static OrderManager Instance { get; private set; }
    public System.Action<int> OnOrderReadyForPickup;
    public System.Action<int> OnOrderAccepted;
    
    [Header("Order Settings")]
    public int minBurgersPerOrder = 1;
    public int maxBurgersPerOrder = 4;
    
    [Header("Current Orders")]
    public List<ActiveOrder> activeOrders = new List<ActiveOrder>();
    public List<PendingOrder> pendingOrders = new List<PendingOrder>();
    
    public System.Action<int> OnOrderCompleted;
    
    [System.Serializable]
    public class ActiveOrder
    {
        public int orderNumber;
        public int burgerCount;
        public string source;
        public float timeCreated;
    }
    
    [System.Serializable]
    public class PendingOrder
    {
        public int orderNumber;
        public string carName;
        public int burgerCount;
        public float timeArrived;
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
    
    public void CreateNewOrder(string source = "Unknown")
    {
        CreatePendingOrder(source);
    }
    
    public int CreatePendingOrder(string carName)
    {
        if (pendingOrders.Count > 0 && pendingOrders.Exists(p => p.carName == carName))
        {
            return -1;
        }
        
        orderCounter++;
        int burgerCount = Random.Range(minBurgersPerOrder, maxBurgersPerOrder + 1);
        
        PendingOrder pendingOrder = new PendingOrder
        {
            orderNumber = orderCounter,
            carName = carName,
            burgerCount = burgerCount,
            timeArrived = Time.time
        };
        
        pendingOrders.Add(pendingOrder);
        
        OnOrderReadyForPickup?.Invoke(orderCounter);
        
        return orderCounter;
    }
    
    public void AcceptPendingOrder(int orderNumber)
    {
        PendingOrder pendingOrder = pendingOrders.Find(o => o.orderNumber == orderNumber);
        if (pendingOrder != null)
        {
            ActiveOrder newOrder = new ActiveOrder
            {
                orderNumber = pendingOrder.orderNumber,
                burgerCount = pendingOrder.burgerCount,
                source = pendingOrder.carName,
                timeCreated = Time.time
            };
            
            activeOrders.Add(newOrder);
            UpdateAllPackagesCapacity(pendingOrder.burgerCount);
            pendingOrders.Remove(pendingOrder);
            OnOrderAccepted?.Invoke(orderNumber);
            
        }
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
    
    public void CompleteOrder(int orderNumber)
    {
        ActiveOrder order = activeOrders.Find(o => o.orderNumber == orderNumber);
        if (order != null)
        {
            activeOrders.Remove(order);
            OnOrderCompleted?.Invoke(orderNumber);
        }
    }
    
    public bool HasActiveOrder()
    {
        return activeOrders.Count > 0;
    }
    
    public bool HasPendingOrderForCar(string carName)
    {
        return pendingOrders.Exists(p => p.carName == carName);
    }
    
    public bool HasPendingOrders()
    {
        return pendingOrders.Count > 0;
    }
    
    public int GetCurrentOrderNumber()
    {
        if (activeOrders.Count > 0)
            return activeOrders[0].orderNumber;
        
        return -1;
    }
    
    public int GetCurrentOrderBurgers()
    {
        if (activeOrders.Count > 0)
            return activeOrders[0].burgerCount;
        
        return 0;
    }
    
    public List<PendingOrder> GetPendingOrders()
    {
        return pendingOrders;
    }
    
    public bool HasOrderForCar(string carName)
    {
        return activeOrders.Exists(o => o.source == carName) || 
               pendingOrders.Exists(p => p.carName == carName);
    }
}
