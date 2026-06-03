using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class HallwayTrigger : MonoBehaviour
{
    [SerializeField]
    private List<CapstoneInfo> capstones = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Player"))
        {
            foreach (var capstone in capstones)
            {
                if (capstone != null) {
                    capstone.ActivatePreview();
                }
            }
        }

        if (other.gameObject.layer == LayerMask.NameToLayer("CapstoneInfo"))
        {
            var capstone = other.GetComponent<CapstoneInfo>();
            Debug.Log(other.name);
            if (capstone != null && !capstones.Contains(capstone)) {
                capstones.Add(capstone);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        foreach (var capstone in capstones)
        {
            if (capstone != null)
                capstone.DeactivatePreview();
        }
    }
}