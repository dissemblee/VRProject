using UnityEngine;

public class BurgerAssemblyPoint : MonoBehaviour
{
    public Burger currentBurger;

    private void Awake()
    {
        if (currentBurger == null)
        {
            currentBurger = GetComponentInChildren<Burger>();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Ingredient ingredient = other.GetComponent<Ingredient>();
        if (ingredient == null) return;
        if (ingredient.IsGrabbed) return;

        currentBurger.AddIngredient(ingredient);

        ingredient.PlaceOnBurger(currentBurger);
    }

}
