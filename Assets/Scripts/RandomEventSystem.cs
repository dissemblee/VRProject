// RandomEventSystem.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RandomEventSystem : MonoBehaviour
{
    [Header("Настройки событий")]
    [SerializeField] private float minTimeBetweenEvents = 30f;
    [SerializeField] private float maxTimeBetweenEvents = 90f;
    [SerializeField] private float safeZoneCheckFrequency = 1f;
    
    [Header("Заглушки событий")]
    [SerializeField] private GameObject deathEffectPrefab;
    [SerializeField] private string cutsceneName = "Event_Cutscene";
    
    [Header("Проверка заказов")]
    [SerializeField] private bool checkForActiveOrders = true;
    
    private Transform player;
    private bool isPlayerAlive = true;
    private List<SafeZone> allSafeZones = new List<SafeZone>();
    private OrderManager orderManager;
    private CarSpawnerAdvanced carSpawner;
    
    private void Start()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            player = playerObj.transform;
        }
        else
        {
            Debug.LogError("Player not found! Make sure your player has 'Player' tag.");
            return;
        }
        
        orderManager = OrderManager.Instance;
        if (orderManager == null)
        {
            orderManager = FindObjectOfType<OrderManager>();
        }
        
        if (orderManager == null)
        {
            Debug.LogWarning("OrderManager not found! Events will trigger without order checking.");
        }
        
        carSpawner = FindObjectOfType<CarSpawnerAdvanced>();
        if (carSpawner == null)
        {
            Debug.LogWarning("CarSpawnerAdvanced not found! Color checking will be disabled.");
        }
        
        FindAllSafeZones();
        
        StartCoroutine(RandomEventRoutine());
    }
    
    private void FindAllSafeZones()
    {
        SafeZone[] zones = FindObjectsOfType<SafeZone>();
        allSafeZones.Clear();
        allSafeZones.AddRange(zones);
        Debug.Log($"Найдено безопасных зон: {allSafeZones.Count}");
    }
    
    private IEnumerator RandomEventRoutine()
    {
        while (isPlayerAlive && player != null)
        {
            float waitTime = Random.Range(minTimeBetweenEvents, maxTimeBetweenEvents);
            yield return new WaitForSeconds(waitTime);
            
            if (CanTriggerEvent())
            {
                TriggerRandomEvent();
            }
            else
            {
                LogEventCancellationReason();
            }
        }
    }
    
    private bool CanTriggerEvent()
    {
        if (IsPlayerInSafeZone())
        {
            return false;
        }
        
        if (checkForActiveOrders && HasActiveOrders())
        {
            return false;
        }
        
        return true;
    }
    
    private bool IsPlayerInSafeZone()
    {
        if (player == null) return false;
        
        foreach (var safeZone in allSafeZones)
        {
            if (safeZone == null) continue;
            
            if (safeZone.IsPointInZone(player.position))
            {
                return true;
            }
        }
        
        return false;
    }
    
    private bool HasActiveOrders()
    {
        if (orderManager == null) return false;
        
        return orderManager.HasActiveOrder();
    }
    
    private void LogEventCancellationReason()
    {
        string reason = "Событие отменено: ";
        
        if (IsPlayerInSafeZone())
        {
            reason += "игрок в безопасной зоне";
        }
        else if (checkForActiveOrders && HasActiveOrders())
        {
            reason += "есть активный заказ";
            
            if (orderManager != null)
            {
                int orderNumber = orderManager.GetCurrentOrderNumber();
                int burgers = orderManager.GetCurrentOrderBurgers();
                if (orderNumber != -1)
                {
                    reason += $" (Заказ #{orderNumber}, {burgers} бургеров)";
                }
            }
        }
        else
        {
            reason += "неизвестная причина";
        }
        
        Debug.Log(reason);
    }
    
    public void TriggerRandomEvent()
    {
        Debug.Log("Запуск случайного события (все условия выполнены)");
        TriggerCutsceneEvent();
    }
    
    private void TriggerCutsceneEvent()
    {
      Debug.Log("Проигрыш");
    }
    
    public void RegisterSafeZone(SafeZone zone)
    {
        if (!allSafeZones.Contains(zone))
        {
            allSafeZones.Add(zone);
        }
    }
    
    public void UnregisterSafeZone(SafeZone zone)
    {
        if (allSafeZones.Contains(zone))
        {
            allSafeZones.Remove(zone);
        }
    }
    
    public string GetEventStatus()
    {
        if (IsPlayerInSafeZone())
            return "Заблокировано: в безопасной зоне";
        
        if (checkForActiveOrders && HasActiveOrders())
            return "Заблокировано: есть активный заказ";
        
        return "Готово к запуску";
    }
    
    public void TestEventTrigger()
    {
        if (CanTriggerEvent())
        {
            TriggerRandomEvent();
        }
        else
        {
            Debug.Log("Тест отменен: " + GetEventStatus());
        }
    }
    
    public bool IsEventAllowed()
    {
        return CanTriggerEvent();
    }
}