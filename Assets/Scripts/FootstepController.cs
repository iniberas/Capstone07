using UnityEngine;

public class VRFootstepController : MonoBehaviour
{
    // Harusnya taro di movement aja tapi benerinnya ntar ajah malas :p
    [SerializeField] private Transform vrCamera; 
    [SerializeField] private AudioSource footstepSource; 
    [SerializeField] private AudioClip[] footstepClips;
    [SerializeField] private float stepDistance = 1.0f;
    [SerializeField] private Vector3 audioOffset = new Vector3(0, 0, -0.3f); 
    [SerializeField] private float minPitch = 0.85f;
    [SerializeField] private float maxPitch = 1.15f;
    private Vector3 lastPos;
    private float currentDistance;

    void Start()
    {
        lastPos = GetFlatPosition(vrCamera.position);
        footstepSource.spatialBlend = 1f;
    }

    void Update()
    {
        Vector3 currentPos = GetFlatPosition(vrCamera.position);
        currentDistance += Vector3.Distance(currentPos, lastPos);
        if (currentDistance >= stepDistance)
        {
            PlayFootstep();
            currentDistance = 0;
        }
        lastPos = currentPos;
    }

    private Vector3 GetFlatPosition(Vector3 pos)
    {
        return new Vector3(pos.x, 0, pos.z);
    }

    private void PlayFootstep()
    {
        if (footstepClips.Length == 0) return;
        Vector3 stepPosition = transform.position;
        
        Vector3 backwardDirection = -vrCamera.forward;
        backwardDirection.y = 0;
        
        footstepSource.transform.position = stepPosition + (backwardDirection.normalized * Mathf.Abs(audioOffset.z));
        footstepSource.pitch = Random.Range(minPitch, maxPitch);

        int randomIndex = Random.Range(0, footstepClips.Length);
        AudioClip clipToPlay = footstepClips[randomIndex];
        
        footstepSource.PlayOneShot(clipToPlay);
    }
}