using UnityEngine;

public class Billboard : MonoBehaviour
{
    [SerializeField] private GameObject target;

    void Update()
    {
        if (target == null) return;

        Vector3 direction = target.transform.position - transform.position;
        direction.y = 0f;

        if (direction != Vector3.zero)
        {
            Quaternion rotation = Quaternion.LookRotation(direction);
            transform.rotation = rotation * Quaternion.Euler(0f, 180f, 0f);
        }
    }
}