using UnityEngine;

public class CarWaypointMovementClean : MonoBehaviour
{
    private Transform[] waypoints;
    public float speed = 5f;
    public float rotationSpeed = 180f;
    public float stopDistance = 0.2f;
    public float safeDistance = 1.5f;

    [HideInInspector]
    public int currentWaypoint = 0;

    private Rigidbody rb;

    [Header("Офсет для поворота, чтобы перед машины совпадал с визуалом")]
    public Vector3 rotationOffsetEuler = Vector3.zero;

    private void Awake()
    {
        rb = GetComponentInChildren<Rigidbody>();
        if (rb == null)
        {
            Debug.LogError("Rigidbody не найден в дочерних объектах машины!");
        }
    }

    public void SetWaypoints(Transform[] newWaypoints)
    {
        waypoints = newWaypoints;
        currentWaypoint = 0;
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0 || rb == null) return;

        foreach (var car in Object.FindObjectsByType<CarWaypointMovementClean>(FindObjectsSortMode.None))
        {
            if (car == this) continue;
            float dist = Vector3.Distance(car.transform.position, rb.position);
            if (dist < safeDistance && car.currentWaypoint <= currentWaypoint)
            {
                return;
            }
        }

        Transform target = waypoints[currentWaypoint];
        Vector3 dir = target.position - rb.position;
        dir.y = 0;

        if (dir.magnitude > stopDistance)
        {
            Quaternion targetRot = Quaternion.LookRotation(dir) * Quaternion.Euler(rotationOffsetEuler);
            rb.MoveRotation(Quaternion.RotateTowards(rb.rotation, targetRot, rotationSpeed * Time.fixedDeltaTime));

            Vector3 move = dir.normalized * speed * Time.fixedDeltaTime;
            rb.MovePosition(rb.position + move);
        }
        else
        {
            currentWaypoint++;
            if (currentWaypoint >= waypoints.Length)
            {
                currentWaypoint = waypoints.Length - 1;
                // Машина достигла последнего Waypoint и остановилась
                // Здесь можно добавить взаимодействие с игроком
            }
        }
    }
}