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

    public Color[] possibleColors;
    [Range(0f, 1f)]
    public float repeatColorChance = 0.2f;

    private List<GameObject> activeCars = new List<GameObject>();
    private Color lastColor;

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

            GameObject car = Instantiate(carPrefab, transform.position, transform.rotation);

            Transform[] waypoints = new Transform[waypointsContainer.childCount];
            for (int i = 0; i < waypointsContainer.childCount; i++)
            {
                waypoints[i] = waypointsContainer.GetChild(i);
            }

            CarWaypointMovementClean movement = car.GetComponent<CarWaypointMovementClean>();
            movement.SetWaypoints(waypoints);

            Color carColor;
            if (Random.value < repeatColorChance)
            {
                carColor = lastColor;
            }
            else
            {
                carColor = possibleColors[Random.Range(0, possibleColors.Length)];
            }
            lastColor = carColor;

            foreach (Transform t in car.GetComponentsInChildren<Transform>())
            {
                if (t.CompareTag("BodyParts"))
                {
                    MeshRenderer[] renderers = t.GetComponents<MeshRenderer>();
                    foreach (var mr in renderers)
                    {
                        mr.material = new Material(mr.material);
                        mr.material.color = carColor;
                    }
                }
            }

            activeCars.Add(car);
        }
    }
}
