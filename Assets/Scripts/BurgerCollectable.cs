using UnityEngine;

public class BurgerCollectable : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"Burger touched: {other.name} | Tag: {other.tag}");
        
        if (other.CompareTag("Package"))
        {
            Debug.Log("Burger touched a Package!");
        }
    }
}