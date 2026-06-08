using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using DG.Tweening;

[System.Serializable]
public class CapstoneResponse
{
    public string status;
    public CapstoneData data;
}

[System.Serializable]
public class CapstoneData
{
    public int id;
    public string title;
    public string desc;
    public string poster;
    public string video;
    public string preview;
    public int total_likes;
}

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
    [SerializeField] private Image likeIconImage;

    [Header("Trigger Things")]
    [SerializeField] private float closedY = -2f;
    [SerializeField] private float openedY = 0f;
    [SerializeField] private GameObject objectInfo;
    [SerializeField] private GameObject objectBoard;

    [Header("Media Control Things")]
    [SerializeField] private GameObject pauseButton;
    [SerializeField] private GameObject resumeButton;

    private CapstoneData _data;
    private RenderTexture uniqueRenderTexture;
    private bool isPlayerClose = false;
    private bool isLiked = false;
    private string PlayerPrefsLikeKey => $"isLiked_{mediaId}";

    private bool hallwayActive;
    private bool dataLoaded;
    private bool dataLoading;
    private bool userPaused;
    private double pausedTime;

    void Start()
    {
        uniqueRenderTexture = new RenderTexture(1920, 1080, 16);

        Material[] objectMaterials = screenRenderer.materials;
        objectMaterials[1].SetTexture("_BaseMap", uniqueRenderTexture);
        objectMaterials[1].EnableKeyword("_EMISSION");
        objectMaterials[1].SetTexture("_EmissionMap", uniqueRenderTexture);

        boardAndInfo.SetActive(false);
        userPaused = false;
        UpdatePlaybackButtons();

        objectBoard.transform.localPosition = new Vector3(0, -2, -3);
        objectInfo.transform.localScale = Vector3.zero;

        previewPlayer.playOnAwake = false;
        fullPlayer.playOnAwake = false;

        isLiked = PlayerPrefs.GetInt(PlayerPrefsLikeKey, 0) == 1;
        UpdateLikeUI();

        // StartCoroutine(FetchDataFromAPI());
    }

    private void UpdateLikeUI()
    {
        if (likeIconImage != null)
        {
            likeIconImage.color = isLiked ? Color.red : Color.black;
        }
    }

    IEnumerator FetchDataFromAPI()
    {
        string url = GetBaseUrl() + "api/capstones/" + mediaId;

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                dataLoading = false;
                Debug.LogError($"[API Error] Failed to fetch data for ID {mediaId}: " + request.error);
            }
            else
            {
                string jsonResponse = request.downloadHandler.text;
                CapstoneResponse response = JsonUtility.FromJson<CapstoneResponse>(jsonResponse);

                if (response != null && response.status == "success" && response.data != null)
                {
                    _data = response.data;
                    SetupMedia();
                    dataLoaded = true;
                    dataLoading = false;
                }
                else
                {
                    dataLoading = false;
                    Debug.LogError($"Media data not found or JSON parsing failed for ID: {mediaId}");
                }
            }
        }
    }

    void SetupMedia()
    {
        titleTMP.text = _data.title;
        descTMP.text = _data.desc;

        string baseUrl = GetBaseUrl();

        if (!string.IsNullOrEmpty(_data.preview))
            previewPlayer.url = baseUrl + _data.preview;

        if (!string.IsNullOrEmpty(_data.video))
            fullPlayer.url = baseUrl + _data.video;

        if (!string.IsNullOrEmpty(_data.poster))
        {
            string posterUrl = baseUrl + _data.poster;
            Debug.Log("Loading poster from: " + posterUrl);
            StartCoroutine(LoadImageFromURL(posterUrl));
        }

        previewPlayer.isLooping = true;
        fullPlayer.isLooping = true;

        if (hallwayActive)
            StartCoroutine(ManageVideoLoadingPriority());
    }

    IEnumerator ManageVideoLoadingPriority()
    {
        if (!string.IsNullOrEmpty(previewPlayer.url))
        {
            previewPlayer.Prepare();
            while (!previewPlayer.isPrepared && !isPlayerClose)
                yield return null;
        }

        if (!isPlayerClose)
        {
            if (!hallwayActive)
                yield break;
            SetPreviewMode();
            if (!string.IsNullOrEmpty(fullPlayer.url)) fullPlayer.Prepare();
        }
        else
        {
            if (!string.IsNullOrEmpty(fullPlayer.url))
            {
                fullPlayer.Prepare();
                while (!fullPlayer.isPrepared)
                    yield return null;
            }

            SetFullVideoMode();
            if (!string.IsNullOrEmpty(previewPlayer.url)) previewPlayer.Prepare();
        }
    }

    IEnumerator LoadImageFromURL(string imageUrl)
    {
        using (UnityWebRequest request = UnityWebRequestTexture.GetTexture(imageUrl))
        {
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Failed to load image from {imageUrl}: {request.error}");
            }
            else
            {
                Texture2D texture = DownloadHandlerTexture.GetContent(request);
                posterImage.texture = texture;
            }
        }
    }

    public void SetPreviewMode()
    {
        if (_data == null || string.IsNullOrEmpty(previewPlayer.url)) return;

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

        if (!hallwayActive)
            yield break;

        if (!isPlayerClose)
        {
            previewPlayer.targetTexture = uniqueRenderTexture;
            previewPlayer.Play();
        }
    }

    public void SetFullVideoMode()
    {
        if (_data == null || string.IsNullOrEmpty(fullPlayer.url))
            return;

        previewPlayer.Pause();
        previewPlayer.targetTexture = null;

        boardAndInfo.SetActive(true);

        if (fullPlayer.isPrepared)
        {
            fullPlayer.targetTexture = uniqueRenderTexture;

            if (!userPaused) {
                fullPlayer.Play();
            } else {
                StartCoroutine(PauseNextFrame());
            }

            UpdatePlaybackButtons();
        }
        else
        {
            StartCoroutine(WaitAndSetFullVideo());
        }
    }

    // anu ini helper gaje banget plis tolonnggggg
    IEnumerator PauseNextFrame() {
        float originalVolume = fullPlayer.GetDirectAudioVolume(0);
        fullPlayer.SetDirectAudioVolume(0, 0f);
        fullPlayer.time = pausedTime;
        fullPlayer.Play();
        // 2 frame supaya aman kali
        yield return null;
        yield return null;
        fullPlayer.SetDirectAudioVolume(0, originalVolume);
        fullPlayer.Pause();
    }

    IEnumerator WaitAndSetFullVideo()
    {
        if (!fullPlayer.isPrepared)
            fullPlayer.Prepare();

        while (!fullPlayer.isPrepared)
            yield return null;

        if (!hallwayActive)
            yield break;

        if (isPlayerClose)
        {
            fullPlayer.targetTexture = uniqueRenderTexture;

            if (!userPaused)
                fullPlayer.Play();

            UpdatePlaybackButtons();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        isPlayerClose = true;

        objectBoard.transform.DOLocalMoveY(openedY, 0.5f).SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill();
        objectInfo.transform.DOScale(Vector3.one, 0.5f).SetEase(Ease.OutBack);

        if (_data != null) SetFullVideoMode();
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.layer != LayerMask.NameToLayer("Player")) return;

        objectBoard.transform.DOLocalMoveY(closedY, 0.5f).SetEase(Ease.InOutCubic);
        objectInfo.transform.DOKill();
        objectInfo.transform.DOScale(Vector3.zero, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                isPlayerClose = false;

                if (hallwayActive && _data != null)
                    SetPreviewMode();
            });
    }

    public void ActivatePreview()
    {
        if (hallwayActive)
            return;

        hallwayActive = true;

        if (!dataLoaded && !dataLoading)
        {
            dataLoading = true;
            StartCoroutine(FetchDataFromAPI());
        }
        else if (dataLoaded)
        {
            StartCoroutine(ManageVideoLoadingPriority());
        }
    }

    public void DeactivatePreview()
    {
        hallwayActive = false;

        previewPlayer.Pause();
        previewPlayer.targetTexture = null;

        fullPlayer.Pause();
        fullPlayer.targetTexture = null;

        boardAndInfo.SetActive(false);
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

    public void ToggleLike()
    {
        string guestId = GuestSessionManager.GetGuestId();
        StartCoroutine(SendLikeRequest(guestId));
    }

    IEnumerator SendLikeRequest(string guestId)
    {
        string url = GetBaseUrl() + $"api/capstones/{mediaId}/like";
        string jsonData = $"{{\"guest_id\": \"{guestId}\"}}";

        using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(jsonData);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                Debug.Log("Like status toggled successfully: " + request.downloadHandler.text);

                isLiked = !isLiked;

                PlayerPrefs.SetInt(PlayerPrefsLikeKey, isLiked ? 1 : 0);
                PlayerPrefs.Save();

                UpdateLikeUI();
            }
            else
            {
                Debug.LogError("Error toggling like: " + request.error);
            }
        }
    }

    void OnDestroy()
    {
        if (uniqueRenderTexture != null)
            uniqueRenderTexture.Release();
    }

    public void PauseVideo()
    {
        pausedTime = fullPlayer.time;
        fullPlayer.Pause();
        userPaused = true;
        UpdatePlaybackButtons();
    }

    public void ResumeVideo()
    {
        fullPlayer.Play();
        userPaused = false;
        UpdatePlaybackButtons();
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

    private void UpdatePlaybackButtons() {
        resumeButton.SetActive(userPaused);
        pauseButton.SetActive(!userPaused);
    }
}