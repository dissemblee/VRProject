using UnityEngine;
using System.Collections.Generic;

public class IngredientSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    public List<GameObject> ingredientPrefabs;
    public int maxIngredients = 9;
    public float spawnInterval = 180f;
    public Vector3 spawnAreaCenter;
    public Vector3 spawnAreaSize = new Vector3(5f, 0f, 5f);

    [Header("Camera Reference")]
    public Camera mainCamera;

    [Header("Rotation Settings")]
    public bool randomRotation = true;
    public Vector3 minRotation = new Vector3(0, 0, 0);
    public Vector3 maxRotation = new Vector3(0, 360, 0);

    private List<GameObject> spawnedIngredients = new List<GameObject>();
    private float spawnTimer = 0f;

    void Start()
    {
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
            if (mainCamera == null) Debug.LogError("No main camera found in scene!");
        }

        for (int i = 0; i < Mathf.Min(3, maxIngredients); i++)
        {
            SpawnIngredient();
        }
        
        spawnTimer = spawnInterval;
    }

    void Update()
    {
        spawnTimer += Time.deltaTime;
        
        if (spawnTimer >= spawnInterval)
        {
            SpawnIngredient();
            spawnTimer = 0f;
        }
        
        CleanupDestroyedIngredients();
    }

    void SpawnIngredient()
    {
        if (spawnedIngredients.Count >= maxIngredients) return;

        if (ingredientPrefabs == null || ingredientPrefabs.Count == 0) return;

        GameObject prefabToSpawn = ingredientPrefabs[Random.Range(0, ingredientPrefabs.Count)];
        
        Vector3 randomOffset = new Vector3(
            Random.Range(-spawnAreaSize.x / 2f, spawnAreaSize.x / 2f),
            Random.Range(-spawnAreaSize.y / 2f, spawnAreaSize.y / 2f),
            Random.Range(-spawnAreaSize.z / 2f, spawnAreaSize.z / 2f)
        );
        
        Vector3 spawnPosition = transform.position + spawnAreaCenter + randomOffset;
        
        Quaternion spawnRotation = Quaternion.identity;
        if (randomRotation)
        {
            Vector3 randomRot = new Vector3(
                Random.Range(minRotation.x, maxRotation.x),
                Random.Range(minRotation.y, maxRotation.y),
                Random.Range(minRotation.z, maxRotation.z)
            );
            spawnRotation = Quaternion.Euler(randomRot);
        }
        
        GameObject spawnedIngredient = Instantiate(prefabToSpawn, spawnPosition, spawnRotation);
        
        Grabable grabable = spawnedIngredient.GetComponent<Grabable>();
        if (grabable != null)
        {
            grabable.mainCamera = mainCamera;
        }

        spawnedIngredients.Add(spawnedIngredient);
    }

    void CleanupDestroyedIngredients()
    {
        for (int i = spawnedIngredients.Count - 1; i >= 0; i--)
        {
            if (spawnedIngredients[i] == null) spawnedIngredients.RemoveAt(i);
        }
    }

    public bool ForceSpawnIngredient()
    {
        if (spawnedIngredients.Count < maxIngredients)
        {
            SpawnIngredient();
            return true;
        }
        return false;
    }

    public bool ForceSpawnSpecificIngredient(GameObject ingredientPrefab)
    {
        if (spawnedIngredients.Count >= maxIngredients) return false;
        
        List<GameObject> tempList = new List<GameObject>(ingredientPrefabs);
        
        ingredientPrefabs = new List<GameObject> { ingredientPrefab };
        
        SpawnIngredient();
        
        ingredientPrefabs = tempList;
        
        return true;
    }

    public int GetCurrentIngredientCount()
    {
        return spawnedIngredients.Count;
    }

    public void ResetSpawner()
    {
        foreach (GameObject ingredient in spawnedIngredients)
        {
            if (ingredient != null)
                Destroy(ingredient);
        }
        spawnedIngredients.Clear();
        spawnTimer = 0f;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawWireCube(transform.position + spawnAreaCenter, spawnAreaSize);
        Gizmos.color = new Color(0, 1, 0, 0.3f);
        Gizmos.DrawCube(transform.position + spawnAreaCenter, spawnAreaSize);
    }
}
