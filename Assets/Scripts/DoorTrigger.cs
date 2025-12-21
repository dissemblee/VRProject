using UnityEngine;

public class DoorTrigger : MonoBehaviour
{
    public Transform door;
    public float openAngle = 90f;
    public float speed = 2f;

    Quaternion closedRotation;
    Quaternion openRotation;
    bool isOpen;

    void Start()
    {
        closedRotation = door.rotation;
    }

    void Update()
    {
        Quaternion target = isOpen ? openRotation : closedRotation;
        door.rotation = Quaternion.Lerp(
            door.rotation,
            target,
            Time.deltaTime * speed
        );
    }

    void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        // направление от двери к игроку
        Vector3 toPlayer = other.transform.position - door.position;

        // смотрим, с какой стороны игрок
        float dot = Vector3.Dot(door.forward, toPlayer);

         float angle = dot > 0 ? -openAngle : openAngle;

        openRotation = Quaternion.Euler(
            door.eulerAngles + Vector3.up * angle
        );

        isOpen = true;
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            isOpen = false;
    }
}
