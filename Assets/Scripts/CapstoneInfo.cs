using UnityEngine;
using System.Collections;

public class CapstoneInfo : MonoBehaviour
{
    [SerializeField] private GameObject objectInfo;

    private void Start() {
        if (objectInfo != null) {
            objectInfo.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other) {
        objectInfo.SetActive(true);
    }

    private void OnTriggerExit(Collider other) {
        objectInfo.SetActive(false);
    }
}
