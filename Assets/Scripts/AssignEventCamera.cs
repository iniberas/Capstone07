using UnityEngine;

[RequireComponent(typeof(Canvas))]
public class AssignEventCamera : MonoBehaviour
{
    void Start()
    {
        Canvas canvas = GetComponent<Canvas>();

        if (Camera.main != null)
        {
            canvas.worldCamera = Camera.main;
        }
        else
        {
            Debug.LogWarning("gaada main camera helpppppp");
        }
    }
}