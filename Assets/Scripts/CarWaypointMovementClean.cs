using UnityEngine;
using System.Collections.Generic;

public class CarWaypointMovementClean : MonoBehaviour
{
    private Transform[] _waypoints;
    public float speed = 5f;
    public float rotationSpeed = 180f;
    public float stopDistance = 0.2f;
    public float safeDistance = 3f;
    
    [Header("Order Settings")]
    [SerializeField] private Transform orderWaypoint;
    public float orderStopDuration = 3f;
    public GameObject orderSignal;

    [HideInInspector]
    public int currentWaypoint = 0;

    [Header("Офсет для поворота")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    private Rigidbody rb;
    private bool isForceStopped = false;
    private bool isWaitingForOrder = false;
    private bool isOrderAccepted = false;
    private float stopTimer = 0f;
    private int orderWaypointIndex = -1;
    private int currentOrderNumber = -1;
    
    private bool isBlockedByOtherCar = false;
    private float originalSpeed;
    
    private float pathProgress = 0f;
    private Vector3 previousPosition;
    private float distanceTraveled = 0f;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        originalSpeed = speed;
        
        if (orderSignal != null)
            orderSignal.SetActive(false);
            
        previousPosition = transform.position;
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        _waypoints = newWaypoints;
        currentWaypoint = 0;
        UpdateOrderWaypointIndex();
    }

    public void SetOrderWaypoint(Transform waypoint)
    {
        orderWaypoint = waypoint;
        UpdateOrderWaypointIndex();
    }

    private void UpdateOrderWaypointIndex()
    {
        orderWaypointIndex = -1;
        
        if (orderWaypoint != null && _waypoints != null)
        {
            for (int i = 0; i < _waypoints.Length; i++)
            {
                if (_waypoints[i] == orderWaypoint)
                {
                    orderWaypointIndex = i;
                    break;
                }
            }
        }
    }

    private void FixedUpdate()
    {
        UpdateDistanceTraveled();
        
        if (_waypoints == null || _waypoints.Length == 0 || rb == null) return;
        
        bool wasBlocked = isBlockedByOtherCar;
        isBlockedByOtherCar = CheckSafeDistance();
        
        if (wasBlocked != isBlockedByOtherCar)
        {
            speed = isBlockedByOtherCar ? 0f : originalSpeed;
        }
        
        if (isBlockedByOtherCar)
        {
            BrakeOrStop();
            return;
        }
        
        if (isForceStopped || isWaitingForOrder) return;

        Transform target = _waypoints[currentWaypoint];
        Vector3 dir = target.position - rb.position;
        dir.y = 0;

        if (dir.magnitude > stopDistance)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(rotationOffsetEuler);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

            Vector3 move = dir.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
        else
        {
            if (orderWaypointIndex != -1 && currentWaypoint == orderWaypointIndex)
            {
                StartOrderCreation();
            }
            else
            {
                MoveToNextWaypoint();
            }
        }
    }
    
    private void UpdateDistanceTraveled()
    {
        distanceTraveled += Vector3.Distance(transform.position, previousPosition);
        previousPosition = transform.position;
        pathProgress = distanceTraveled + currentWaypoint * 100f;
    }
    
    private bool CheckSafeDistance()
    {
        #if UNITY_2021_1_OR_NEWER
            var otherCars = FindObjectsByType<CarWaypointMovementClean>(FindObjectsSortMode.None);
        #else
            var otherCars = FindObjectsOfType<CarWaypointMovementClean>();
        #endif
        
        CarWaypointMovementClean closestCar = null;
        float closestDistance = float.MaxValue;
        
        foreach (var car in otherCars)
        {
            if (car == this) continue;
            
            float dist = Vector3.Distance(car.transform.position, transform.position);
            
            if (dist < safeDistance && dist < closestDistance)
            {
                Vector3 toCar = car.transform.position - transform.position;
                Vector3 forward = transform.forward;
                
                float angle = Vector3.Angle(forward, toCar);
                
                if (angle < 45f && toCar.magnitude > 0.1f)
                {
                    Vector3 localPos = transform.InverseTransformPoint(car.transform.position);
                    
                    if (localPos.z > 0 && localPos.z < safeDistance)
                    {
                        if (car.pathProgress > pathProgress)
                        {
                            closestCar = car;
                            closestDistance = dist;
                            
                            Debug.DrawLine(transform.position, car.transform.position, Color.red);
                        }
                    }
                }
            }
        }
        
        if (closestCar != null)
        {
            float distanceToCar = closestDistance;
            
            if (distanceToCar < safeDistance * 0.3f)
            {
                speed = 0f;
            }
            else if (distanceToCar < safeDistance * 0.7f)
            {
                speed = originalSpeed * 0.3f;
            }
            else
            {
                speed = originalSpeed * 0.6f;
            }
            
            return true;
        }
        
        return false;
    }
    
    private void BrakeOrStop()
    {
        if (rb.linearVelocity.magnitude > 0.1f)
        {
            Vector3 brakingForce = -rb.linearVelocity.normalized * (originalSpeed * 3f);
            rb.AddForce(brakingForce * Time.fixedDeltaTime, ForceMode.VelocityChange);
        }
        
        if (orderSignal != null && !orderSignal.activeSelf)
        {
            orderSignal.SetActive(true);
            orderSignal.GetComponent<Renderer>().material.color = Color.red;
        }
    }
    
    void Update()
    {
        if (isForceStopped && isWaitingForOrder)
        {
            if (isOrderAccepted)
            {
                stopTimer -= Time.deltaTime;
                
                if (stopTimer <= 0f)
                {
                    CompleteOrderStop();
                }
            }
            else if (currentOrderNumber != -1 && OrderManager.Instance != null)
            {
                var activeOrder = OrderManager.Instance.activeOrders.Find(o => o.orderNumber == currentOrderNumber);
                if (activeOrder != null)
                {
                    isOrderAccepted = true;
                    stopTimer = orderStopDuration;
                    
                    if (orderSignal != null)
                    {
                        orderSignal.GetComponent<Renderer>().material.color = Color.blue;
                    }
                }
            }
        }
        
        if (isWaitingForOrder && currentOrderNumber != -1 && OrderManager.Instance != null)
        {
            CheckIfOrderCompleted();
        }
        
        if (isBlockedByOtherCar && !isForceStopped && !isWaitingForOrder)
        {
            if (orderSignal != null && !orderSignal.activeSelf)
            {
                orderSignal.SetActive(true);
                orderSignal.GetComponent<Renderer>().material.color = Color.yellow;
            }
        }
        else if (!isBlockedByOtherCar && !isForceStopped && !isWaitingForOrder)
        {
            if (orderSignal != null && orderSignal.activeSelf)
            {
                orderSignal.SetActive(false);
            }
        }
    }
    
    void StartOrderCreation()
    {
        if (OrderManager.Instance != null && 
            OrderManager.Instance.HasOrderForCar(gameObject.name))
        {

            var pendingOrder = OrderManager.Instance.GetPendingOrders().Find(o => o.carName == gameObject.name);
            if (pendingOrder != null)
            {
                currentOrderNumber = pendingOrder.orderNumber;
            }
            else
            {
                var activeOrder = OrderManager.Instance.activeOrders.Find(o => o.source == gameObject.name);
                if (activeOrder != null)
                {
                    currentOrderNumber = activeOrder.orderNumber;
                    isOrderAccepted = true;
                    stopTimer = orderStopDuration;
                }
            }
            
            isWaitingForOrder = true;
            isForceStopped = true;
            
            OrderManager.Instance.OnOrderAccepted += OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
            
            if (orderSignal != null)
            {
                orderSignal.SetActive(true);
                orderSignal.GetComponent<Renderer>().material.color = isOrderAccepted ? Color.blue : Color.yellow;
            }
            return;
        }
        
        if (orderSignal != null)
        {
            orderSignal.SetActive(true);
            orderSignal.GetComponent<Renderer>().material.color = Color.yellow;
        }
        
        if (OrderManager.Instance != null)
        {
            currentOrderNumber = OrderManager.Instance.CreatePendingOrder(gameObject.name);
            
            if (currentOrderNumber != -1)
            {
                isWaitingForOrder = true;
                isForceStopped = true;
                
                OrderManager.Instance.OnOrderAccepted += OnOrderAccepted;
                OrderManager.Instance.OnOrderCompleted += OnOrderCompleted;
            }
        }
    }

    void OnOrderAccepted(int orderNumber)
    {
        if (currentOrderNumber == orderNumber)
        {
            isOrderAccepted = true;
            stopTimer = orderStopDuration;
            
            if (orderSignal != null)
            {
                orderSignal.GetComponent<Renderer>().material.color = Color.blue;
            }
        }
    }
    
    void OnOrderCompleted(int orderNumber)
    {
        if (currentOrderNumber == orderNumber && isWaitingForOrder)
        {
            ContinueMovement();
        }
    }
    
    void CheckIfOrderCompleted()
    {
        if (currentOrderNumber != -1)
        {
            bool isPending = OrderManager.Instance.GetPendingOrders().Exists(o => o.orderNumber == currentOrderNumber);
            bool isActive = OrderManager.Instance.activeOrders.Exists(o => o.orderNumber == currentOrderNumber);
            
            if (!isPending && !isActive)
            {
                ContinueMovement();
            }
        }
    }
    
    void CompleteOrderStop()
    {
        isForceStopped = false;
        
        if (orderSignal != null)
        {
            orderSignal.GetComponent<Renderer>().material.color = Color.green;
        }
    }
    
    void ContinueMovement()
    {
        isForceStopped = false;
        isWaitingForOrder = false;
        isOrderAccepted = false;
        currentOrderNumber = -1;
        speed = originalSpeed;
        
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderAccepted -= OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        }
        
        if (orderSignal != null)
            orderSignal.SetActive(false);
            
        MoveToNextWaypoint();
    }
    
    public void MoveToNextWaypoint()
    {
        currentWaypoint++;
        if (currentWaypoint >= _waypoints.Length)
        {
            Destroy(gameObject);
            return;
        }
    }
    
    void OnDestroy()
    {
        if (OrderManager.Instance != null)
        {
            OrderManager.Instance.OnOrderAccepted -= OnOrderAccepted;
            OrderManager.Instance.OnOrderCompleted -= OnOrderCompleted;
        }
    }
}
