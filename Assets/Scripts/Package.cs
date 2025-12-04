using UnityEngine;
using System.Collections.Generic;

public class Package : MonoBehaviour
{
    [Header("Package Settings")]
    public int maxCapacity = 1;
    private int burgerCount = 0;
    
    [Header("Visual Settings")]
    public GameObject packageFullIndicator;
    public Material fullMaterial;
    public Material emptyMaterial;
    public Renderer packageRenderer;
    
    [Header("Sound Settings")]
    public AudioClip collectSound;
    public AudioClip fullSound;
    public AudioSource audioSource;
    
    [Header("Physics Settings")]
    public bool useGravity = true;
    
    [Header("Order Settings")]
    public bool useGlobalCapacity = true;
    
    public bool IsGrabbed { get; private set; }
    private Rigidbody rb;
    private bool isFull = false;
    private Collider col;
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        
        if (rb != null)
        {
            rb.useGravity = useGravity;
            rb.isKinematic = false;
        }
    }

    void Start()
    {
        if (packageRenderer == null) packageRenderer = GetComponent<Renderer>();
            
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
            
        if (emptyMaterial != null && packageRenderer != null) packageRenderer.material = emptyMaterial;
            
        if (useGlobalCapacity && OrderManager.Instance != null) maxCapacity = OrderManager.Instance.currentOrderBurgers;
            
        UpdateVisuals();
    }

    void OnCollisionEnter(Collision collision)
    {
        GameObject other = collision.gameObject;
        
        if (IsGrabbed) return;
        
        if (other.CompareTag("Burger")) CollectBurger(other);
    }

    public void DeliverToPoint(Transform deliveryPoint)
    {
        IsGrabbed = false;
        
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.useGravity = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
        
        if (col != null) col.enabled = false;
        
        transform.SetParent(deliveryPoint);
    }

    public void SetGrabbed(bool state)
    {
        IsGrabbed = state;
        
        if (rb != null)
        {
            if (state)
            {
                rb.isKinematic = true;
                rb.useGravity = false;
            }
            else
            {
                rb.isKinematic = false;
                rb.useGravity = true;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }
        }
    }

    void CollectBurger(GameObject burger)
    {
        if (isFull)
        {
            
            if (audioSource != null && fullSound != null) audioSource.PlayOneShot(fullSound);

            return;
        }

        burgerCount++;
        
        Destroy(burger);
        
        if (audioSource != null && collectSound != null) audioSource.PlayOneShot(collectSound);

        if (burgerCount >= maxCapacity) isFull = true;

        UpdateVisuals();
    }

    public void SetMaxCapacity(int newCapacity)
    {
        maxCapacity = newCapacity;

        if (burgerCount >= maxCapacity)
        {
            isFull = true;
        }
        else
        {
            isFull = false;
        }
        
        UpdateVisuals();
    }

    void UpdateVisuals()
    {
        if (packageFullIndicator != null) packageFullIndicator.SetActive(isFull);
            
        if (isFull && fullMaterial != null && packageRenderer != null)
        {
            packageRenderer.material = fullMaterial;
        }
        else if (!isFull && emptyMaterial != null && packageRenderer != null)
        {
            packageRenderer.material = emptyMaterial;
        }
    }

    public bool IsFull()
    {
        return isFull;
    }

    public int GetCurrentCount()
    {
        return burgerCount;
    }

    public void ClearPackage()
    {
        burgerCount = 0;
        isFull = false;
        
        UpdateVisuals();
    }
}
