using UnityEngine;

public class EarthRotation : MonoBehaviour
{
    public float speed = 1f;
    void Update() 
    {
        transform.Rotate(0f, speed * Time.deltaTime, 0f);
    }
}
