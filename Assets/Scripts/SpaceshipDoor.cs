using System;
using UnityEditor;
using UnityEngine;
using DG.Tweening;

public class SpaceshipDoor : MonoBehaviour
{
    // hardcoded <3 <3
    private float closedY = 0f;
    private float openedY = -7.5f;

    [SerializeField] private GameObject doorMesh;
    [SerializeField] private GameObject doorClosing;
    [SerializeField] private AudioClip audioClip;
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;
    private AudioSource audioSource;

    private void Start() {
        Vector3 pos = doorClosing.transform.localPosition;
        pos.y = 0;
        doorClosing.transform.localPosition = pos;
        audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void OnTriggerEnter(Collider other) {
        doorMesh.transform.DOLocalMoveY(openedY, 0.5f)
            .SetEase(Ease.InOutCubic);
        doorClosing.transform.DOLocalMoveY(1.5f, 0.7f)
            .SetEase(Ease.InOutCubic);
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(audioClip);
    }

    private void OnTriggerExit(Collider other) {
        doorMesh.transform.DOLocalMoveY(closedY, 0.5f)
            .SetEase(Ease.InOutCubic);
        doorClosing.transform.DOLocalMoveY(0, 0.2f)
            .SetEase(Ease.InOutCubic);
        audioSource.pitch = UnityEngine.Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(audioClip);
    }
}
