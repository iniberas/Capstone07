using UnityEngine;
using DG.Tweening;

public class TriggerArea : MonoBehaviour
{
    [SerializeField] private float closedY = -2f;
    [SerializeField] private float openedY = 0f;
    [SerializeField] private GameObject objectInfo;
    [SerializeField] private GameObject objectBoard;
    [SerializeField] private CapstoneInfo capstoneInfo;

    private void Start() 
    {
        objectBoard.transform.localPosition = new Vector3(0, -2, -3);
        objectInfo.transform.localScale = Vector3.zero;
    }

    private void OnTriggerEnter(Collider other) 
    {
        objectBoard.transform.DOLocalMoveY(openedY, 0.5f)
            .SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill(); 
        objectInfo.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);
        capstoneInfo.SetFullVideoMode();
    }

    private void OnTriggerExit(Collider other) 
    {
        objectBoard.transform.DOLocalMoveY(closedY, 0.5f)
            .SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill();
        objectInfo.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() => {
                capstoneInfo.SetPreviewMode();
            });
    }
}