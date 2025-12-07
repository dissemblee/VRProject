using UnityEngine;

public class DeliveryPoint : MonoBehaviour
{
    [Header("Delivery Settings")]
    public GameObject highlightObject;
    public GameObject completeEffect;
    public Vector3 packagePosition = new Vector3(0, 0.5f, 0);
    
    [Header("Sound Settings")]
    public AudioClip deliverySound;
    public AudioClip wrongDeliverySound;
    public AudioSource audioSource;
    
    private Package currentPackage = null;
    private bool isProcessing = false;
    
    void Start()
    {
        if (highlightObject != null)
            highlightObject.SetActive(true);
            
        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();
    }
    
    void OnTriggerEnter(Collider other)
    {
        if (isProcessing) return;
        
        Package package = other.GetComponent<Package>();
        if (package != null)
        {
            ProcessDelivery(package);
        }
    }
    
    void ProcessDelivery(Package package)
    {
        if (OrderManager.Instance == null || OrderManager.Instance.activeOrders.Count == 0)
            return;
        
        isProcessing = true;
        currentPackage = package;
        
        DisablePackagePhysics(package);
        AttachPackage(package);
        
        CheckDelivery();
    }
    
    void DisablePackagePhysics(Package package)
    {
        Grabable grabable = package.GetComponent<Grabable>();
        if (grabable != null) grabable.enabled = false;
        
        package.SetGrabbed(false);
        
        Rigidbody packageRb = package.GetComponent<Rigidbody>();
        if (packageRb != null)
        {
            packageRb.isKinematic = true;
            packageRb.linearVelocity = Vector3.zero;
            packageRb.angularVelocity = Vector3.zero;
        }
        
        Collider packageCol = package.GetComponent<Collider>();
        if (packageCol != null) packageCol.enabled = false;
    }
    
    void AttachPackage(Package package)
    {
        package.transform.SetParent(transform);
        package.transform.localPosition = packagePosition;
        package.transform.localRotation = Quaternion.identity;
    }
    
    void CheckDelivery()
    {
        if (currentPackage == null || OrderManager.Instance.activeOrders.Count == 0)
        {
            isProcessing = false;
            return;
        }
        
        int burgersInPackage = currentPackage.GetCurrentCount();
        
        foreach (var order in OrderManager.Instance.activeOrders)
        {
            if (currentPackage.IsFull() && burgersInPackage == order.burgerCount)
            {
                CompleteDelivery(order.orderNumber);
                return;
            }
        }
        
        FailedDelivery();
    }
    
    void CompleteDelivery(int orderNumber)
    {
        if (audioSource != null && deliverySound != null)
            audioSource.PlayOneShot(deliverySound);
        
        if (completeEffect != null)
        {
            GameObject effect = Instantiate(completeEffect, 
                transform.position + Vector3.up * 0.5f, 
                Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        OrderManager.Instance.CompleteOrder(orderNumber);
        
        if (currentPackage != null)
            Destroy(currentPackage.gameObject);
        
        isProcessing = false;
        currentPackage = null;
    }
    
    void FailedDelivery()
    {
        if (audioSource != null && wrongDeliverySound != null)
            audioSource.PlayOneShot(wrongDeliverySound);
        
        ReturnPackage();
    }
    
    void ReturnPackage()
    {
        if (currentPackage != null)
        {
            currentPackage.transform.SetParent(null);
            
            Grabable grabable = currentPackage.GetComponent<Grabable>();
            if (grabable != null) grabable.enabled = true;
            
            Rigidbody packageRb = currentPackage.GetComponent<Rigidbody>();
            if (packageRb != null)
            {
                packageRb.isKinematic = false;
                packageRb.useGravity = true;
            }
            
            Collider packageCol = currentPackage.GetComponent<Collider>();
            if (packageCol != null) packageCol.enabled = true;
            
            packageRb.AddForce(Vector3.up * 3f + Random.insideUnitSphere * 2f, ForceMode.Impulse);
        }
        
        isProcessing = false;
        currentPackage = null;
    }
}