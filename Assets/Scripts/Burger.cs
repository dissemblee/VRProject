using UnityEngine;
using System.Collections.Generic;

public class Burger : MonoBehaviour
{
    public List<Ingredient> ingredients = new List<Ingredient>();
    public Vector3 ingredientOffset = new Vector3(0, 0.02f, 0);
    public GameObject assembledBurgerPrefab;
    public Vector3 GetNextIngredientPosition()
    {
        return new Vector3(0, ingredients.Count * ingredientOffset.y, 0);
    }

    public void AddIngredient(Ingredient ing)
    {
        if (ingredients.Exists(i => i.type == ing.type)) return;

        ingredients.Add(ing);
        
        if (IsComplete())
        {
            AssembleBurger();
        }
    }
    public bool IsComplete()
    {
        return ingredients.Count >= 4;
    }

    private void AssembleBurger()
    {
        Vector3 pos = transform.position;
        Quaternion rot = transform.rotation;

        foreach (var ing in ingredients)
        {
            if (ing != null)
            {
                Destroy(ing.gameObject);
            }
        }
        ingredients.Clear();

        if (assembledBurgerPrefab != null)
        {
            GameObject finalBurger = Instantiate(assembledBurgerPrefab, pos, rot);
            finalBurger.transform.SetParent(transform.parent);
        }
    }
}
