using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public bool IsGrabbed { get; private set; }
    public enum IngredientType { BunBottom, Patty, Cheese, BunTop }
    public IngredientType type;
    private Rigidbody rb;
    private Collider col;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        col = GetComponent<Collider>();
    }

    public void SetGrabbed(bool state)
    {
        IsGrabbed = state;

        rb.isKinematic = state;
    }

    public void PlaceOnBurger(Burger burger)
    {
        rb.isKinematic = true;
        col.isTrigger = true;
        // col.enabled = false;

        transform.SetParent(burger.transform, false);

        transform.localPosition = burger.GetNextIngredientPosition();
    }

}
