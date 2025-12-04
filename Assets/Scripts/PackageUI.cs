using UnityEngine;
using UnityEngine.UI;

public class PackageUI : MonoBehaviour
{
    public Package package;
    public Text capacityText;
    public Image fillBar;
    public GameObject fullIndicator;

    void Update()
    {
        if (package == null) return;
        
        if (capacityText != null) capacityText.text = $"{package.GetCurrentCount()}/{package.maxCapacity}";
        
        if (fillBar != null) fillBar.fillAmount = (float)package.GetCurrentCount() / package.maxCapacity;
        
        if (fullIndicator != null) fullIndicator.SetActive(package.IsFull());
    }
}
