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
        ingredient.transform.localScale = new Vector3(9f, 9f, 9f);

        currentBurger.AddIngredient(ingredient);

        ingredient.PlaceOnBurger(currentBurger);
    }

}
