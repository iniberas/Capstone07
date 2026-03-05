using UnityEngine;

public class CapstoneInfo : MonoBehaviour
{
    public GameObject objectInfo;

    void Start()
    {
        if (objectInfo != null)
        {
            objectInfo.SetActive(false);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        objectInfo.SetActive(true);
    }

    void OnTriggerExit(Collider other)
    {
        objectInfo.SetActive(false);
    }
}
