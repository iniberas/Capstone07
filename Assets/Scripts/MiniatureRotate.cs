using UnityEngine;

public class MiniatureRotate : MonoBehaviour
{
    [Header("Rotation")]
    public float rotationSpeed = 25f;

    [Header("Floating")]
    public float floatHeight = 0.15f;
    public float floatSpeed = 1.5f;

    [Header("Tilt")]
    public float tiltAmount = 6f;
    public float tiltSpeed = 0.8f;

    private Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    void Update()
    {
        // Smooth rotation
        transform.Rotate(Vector3.up * rotationSpeed * Time.deltaTime, Space.Self);

        // Floating up/down
        float floatOffset =
            Mathf.Sin(Time.time * floatSpeed) * floatHeight;

        transform.localPosition =
            startPos + Vector3.up * floatOffset;

        // Subtle tilt
        float tiltX =
            Mathf.Sin(Time.time * tiltSpeed) * tiltAmount;

        float tiltZ =
            Mathf.Cos(Time.time * tiltSpeed * 0.7f) * tiltAmount;

        Quaternion baseRotation =
            Quaternion.Euler(tiltX, transform.localEulerAngles.y, tiltZ);

        transform.rotation = baseRotation;
    }
}