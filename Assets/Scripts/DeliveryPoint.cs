using UnityEngine;
using System.Collections;

public class DeliveryPoint : MonoBehaviour
{
    [Header("Delivery Settings")]
    public GameObject highlightObject;
    public GameObject completeEffect;
    public Vector3 packagePosition = new Vector3(0, 0.5f, 0);
    public float deliveryDelay = 1f;
    
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
        if (package != null) ProcessDelivery(package);
    }
    
    void ProcessDelivery(Package package)
    {
        isProcessing = true;
        currentPackage = package;
        
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
        
        package.transform.SetParent(transform);
        
        package.transform.localPosition = packagePosition;
        package.transform.localRotation = Quaternion.identity;
        
        StartCoroutine(DeliveryCheckCoroutine());
    }
    
    IEnumerator DeliveryCheckCoroutine()
    {
        yield return new WaitForSeconds(0.5f);
        
        CheckDelivery();
    }
    
    void CheckDelivery()
    {
        if (currentPackage == null)
        {
            isProcessing = false;
            return;
        }
        
        if (OrderManager.Instance == null || !OrderManager.Instance.hasActiveOrder)
        {
            ResetPackage();
            return;
        }
        
        int burgersInPackage = currentPackage.GetCurrentCount();
        int requiredBurgers = OrderManager.Instance.currentOrderBurgers;
        
        if (currentPackage.IsFull() && burgersInPackage == requiredBurgers)
        {
            StartCoroutine(CompleteDeliveryCoroutine());
        }
        else
        {
            FailedDelivery();
        }
    }
    
    IEnumerator CompleteDeliveryCoroutine()
    {
        if (audioSource != null && deliverySound != null) audioSource.PlayOneShot(deliverySound);
        
        if (completeEffect != null)
        {
            GameObject effect = Instantiate(completeEffect, 
                transform.position + Vector3.up * 0.5f, 
                Quaternion.identity);
            Destroy(effect, 2f);
        }
        
        OrderManager.Instance.CompleteCurrentOrder(Time.time);
        
        yield return new WaitForSeconds(deliveryDelay);
        
        if (currentPackage != null) Destroy(currentPackage.gameObject);
        
        isProcessing = false;
        currentPackage = null;
    }
    
    void FailedDelivery()
    {
        if (audioSource != null && wrongDeliverySound != null) audioSource.PlayOneShot(wrongDeliverySound);
        
        StartCoroutine(ReturnPackageCoroutine());
    }
    
    IEnumerator ReturnPackageCoroutine()
    {
        yield return new WaitForSeconds(1f);
        
        ResetPackage();
    }
    
    void ResetPackage()
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
    
    void OnDrawGizmos()
    {
        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Gizmos.DrawWireCube(transform.position + packagePosition, new Vector3(0.5f, 0.1f, 0.5f));
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            Gizmos.color = new Color(1, 0.5f, 0, 0.3f);
            Gizmos.DrawCube(transform.position, col.bounds.size);
        }
    }
}