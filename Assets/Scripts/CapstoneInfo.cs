using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

public class CapstoneInfo : MonoBehaviour
{
    [SerializeField] private string mediaId;

    [Header("Video Players")]
    [SerializeField] private VideoPlayer previewPlayer;
    [SerializeField] private VideoPlayer fullPlayer;

    [Header("UI & Materials")]
    [SerializeField] private Renderer screenRenderer;
    [SerializeField] private RawImage posterImage;
    [SerializeField] private GameObject boardAndInfo;
    [SerializeField] private TextMeshProUGUI titleTMP;
    [SerializeField] private TextMeshProUGUI descTMP;

    [Header("Trigger Things")]
    [SerializeField] private float closedY = -2f;
    [SerializeField] private float openedY = 0f;
    [SerializeField] private GameObject objectInfo;
    [SerializeField] private GameObject objectBoard;

    [Header("Media Control Things")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeButton;
    private MediaEntry _data;
    private RenderTexture uniqueRenderTexture;

    private bool isPlayerClose = false;

    void Start()
    {
        uniqueRenderTexture = new RenderTexture(1920, 1080, 16);

        Material[] objectMaterials = screenRenderer.materials;
        objectMaterials[1].SetTexture("_BaseMap", uniqueRenderTexture);
        objectMaterials[1].EnableKeyword("_EMISSION");
        objectMaterials[1].SetTexture("_EmissionMap", uniqueRenderTexture);



        boardAndInfo.SetActive(false);
        resumeButton.SetActive(false);

        objectBoard.transform.localPosition = new Vector3(0, -2, -3);
        objectInfo.transform.localScale = Vector3.zero;

        previewPlayer.playOnAwake = false;
        fullPlayer.playOnAwake = false;

        StartCoroutine(WaitForData());
    }

    IEnumerator WaitForData()
    {
        while (MediaManager.Instance == null || !MediaManager.Instance.isLoaded()) yield return null;

        _data = MediaManager.Instance.config.mediaList.Find(x => x.id == mediaId);

        if (_data != null)
        {
            titleTMP.text = _data.title;
            descTMP.text = _data.desc;

            string baseUrl = GetBaseUrl();
            previewPlayer.url = baseUrl + "assets/" + mediaId + "/" + _data.preview;
            fullPlayer.url = baseUrl + "assets/" + mediaId + "/" + _data.video;
            
            string posterUrl = baseUrl + "assets/" + mediaId + "/" + _data.poster;
            Debug.Log(posterUrl);
            StartCoroutine(LoadImageFromURL(posterUrl));

            previewPlayer.isLooping = true;
            fullPlayer.isLooping = true;

            StartCoroutine(ManageVideoLoadingPriority());
        }
        else
        {
            Debug.LogError($"Media data not found for ID: {mediaId}");
        }
    }

    IEnumerator ManageVideoLoadingPriority()
    {
        previewPlayer.Prepare();

        while (!previewPlayer.isPrepared && !isPlayerClose)
            yield return null;

        if (!isPlayerClose)
        {
            SetPreviewMode();
            fullPlayer.Prepare();
        }
        else
        {
            fullPlayer.Prepare();
            while (!fullPlayer.isPrepared)
                yield return null;

            SetFullVideoMode();
            previewPlayer.Prepare();
        }
    }

    IEnumerator LoadImageFromURL(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("HELPPPPPPPPPPPPP");
            }
            else {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                posterImage.texture = texture;
            }
        }
    }

    public void SetPreviewMode()
    {
        if (_data == null) return;

        fullPlayer.Pause();
        fullPlayer.targetTexture = null;
        boardAndInfo.SetActive(false);

        if (previewPlayer.isPrepared)
        {
            previewPlayer.targetTexture = uniqueRenderTexture;
            previewPlayer.Play();
        }
        else
        {
            StartCoroutine(WaitAndSetPreviewVideo());
        }
    }

    IEnumerator WaitAndSetPreviewVideo()
    {
        if (!previewPlayer.isPrepared)
            previewPlayer.Prepare();

        while (!previewPlayer.isPrepared)
            yield return null;

        if (!isPlayerClose)
        {
            previewPlayer.targetTexture = uniqueRenderTexture;
            previewPlayer.Play();
        }
    }

    public void SetFullVideoMode()
    {
        if (_data == null) return;

        previewPlayer.Pause();
        previewPlayer.targetTexture = null;
        boardAndInfo.SetActive(true);

        if (fullPlayer.isPrepared)
        {
            fullPlayer.targetTexture = uniqueRenderTexture;
            fullPlayer.Play();
        }
        else
        {
            StartCoroutine(WaitAndSetFullVideo());
        }
    }

    IEnumerator WaitAndSetFullVideo()
    {
        if (!fullPlayer.isPrepared)
            fullPlayer.Prepare();

        while (!fullPlayer.isPrepared)
            yield return null;

        if (isPlayerClose)
        {
            fullPlayer.targetTexture = uniqueRenderTexture;
            fullPlayer.Play();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isPlayerClose = true;

        objectBoard.transform.DOLocalMoveY(openedY, 0.5f).SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill();
        objectInfo.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (_data != null) SetFullVideoMode();
    }

    private void OnTriggerExit(Collider other)
    {
        objectBoard.transform.DOLocalMoveY(closedY, 0.5f).SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill();
        objectInfo.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                isPlayerClose = false;
                if (_data != null) SetPreviewMode();
            });
    }

    string GetBaseUrl()
    {
#if UNITY_EDITOR
        return "http://localhost:8080/";
#else
        string url = Application.absoluteURL;
        if (string.IsNullOrEmpty(url)) return "http://localhost:8080/";
        return url.Substring(0, url.LastIndexOf('/') + 1);
#endif
    }

    void OnDestroy()
    {
        if (uniqueRenderTexture != null)
            uniqueRenderTexture.Release();
    }

    public void PauseVideo()
    {
        fullPlayer.Pause();
        resumeButton.SetActive(true);
        pauseButton.SetActive(false);
    }

    public void ResumeVideo()
    {
        fullPlayer.Play();
        resumeButton.SetActive(false);
        pauseButton.SetActive(true);
    }

    public void FastForward5s()
    {
        double newTime = fullPlayer.time + 5f;
        fullPlayer.time = newTime < fullPlayer.length ? newTime : fullPlayer.length;
    }

    public void FastBackward5s()
    {
        double newTime = fullPlayer.time - 5f;
        fullPlayer.time = newTime > 0 ? newTime : 0;
    }
}