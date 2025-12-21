using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CarSpawnerAdvanced : MonoBehaviour
{
    public GameObject carPrefab;
    public Transform waypointsContainer;
    public float minSpawnTime = 2f;
    public float maxSpawnTime = 5f;
    public int maxCars = 4;

    [Header("Order Settings")]
    public Transform orderWaypoint;

    [Header("Car Colors")]
    public Color[] possibleColors;
    [Range(0f, 1f)]
    public float repeatColorChance = 0.2f;

    private List<GameObject> activeCars = new List<GameObject>();
    private Color lastColor;
    private Color nextCarColor;
    private bool isFirstCar = true;

    private void Start()
    {
        StartCoroutine(SpawnCars());
    }

    private System.Collections.IEnumerator SpawnCars()
    {
        while (true)
        {
            float waitTime = Random.Range(minSpawnTime, maxSpawnTime);
            yield return new WaitForSeconds(waitTime);

            activeCars.RemoveAll(c => c == null);
            if (activeCars.Count >= maxCars) continue;

            if (!isFirstCar && Random.value < repeatColorChance)
            {
                nextCarColor = lastColor;
            }
            else
            {
                nextCarColor = possibleColors[Random.Range(0, possibleColors.Length)];
            }

            GameObject car = Instantiate(carPrefab, transform.position, transform.rotation);

            CarWaypointMovementClean movement = car.GetComponent<CarWaypointMovementClean>();

            if (!isFirstCar && ColorsAreSimilar(nextCarColor, lastColor))
            {
                movement.isSpecialCar = true;
                Debug.Log("Создана особая машина: " + car.name);
            }

            Transform[] waypoints = new Transform[waypointsContainer.childCount];
            for (int i = 0; i < waypointsContainer.childCount; i++)
            {
                waypoints[i] = waypointsContainer.GetChild(i);
            }

            movement.SetWaypoints(waypoints);
            movement.SetOrderWaypoint(orderWaypoint);

            foreach (Transform t in car.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("BodyParts"))
                {
                    MeshRenderer[] renderers = t.GetComponents<MeshRenderer>();
                    foreach (var mr in renderers)
                    {
                        mr.material = new Material(mr.material);
                        mr.material.color = nextCarColor;
                    }
                }
            }

            lastColor = nextCarColor;
            if (isFirstCar) isFirstCar = false;
            
            activeCars.Add(car);
        }
    }

    public bool WillNextCarRepeatColor()
    {
        if (isFirstCar || lastColor == default(Color)) return false;
        
        if (!isFirstCar && Random.value < repeatColorChance)
        {
            return true;
        }
        return false;
    }

    private bool ColorsAreSimilar(Color c1, Color c2, float tolerance = 0.01f)
    {
        return Mathf.Abs(c1.r - c2.r) < tolerance &&
               Mathf.Abs(c1.g - c2.g) < tolerance &&
               Mathf.Abs(c1.b - c2.b) < tolerance;
    }
}
