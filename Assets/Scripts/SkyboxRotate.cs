using UnityEngine;

public class SkyboxRotate : MonoBehaviour
{
    [SerializeField] private Transform mainCamera;

    [Range(-180f, 180f)]
    [SerializeField] private float verticalAdjustment = 0f;

    void Update()
    {
        if (mainCamera != null)
        {
            Quaternion cameraRotation = mainCamera.rotation;
            Quaternion tilt = Quaternion.Euler(verticalAdjustment, 0, 0);
            transform.rotation = cameraRotation * tilt;
        }
    }
}