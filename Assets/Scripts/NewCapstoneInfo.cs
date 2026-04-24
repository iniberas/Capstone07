using UnityEngine;
using UnityEngine.Video;
using UnityEngine.Networking;
using System.Collections;


public class NewCapstoneInfo : MonoBehaviour {
    [SerializeField] private string mediaId;
    [SerializeField] private CardUI[] cardObjects;
    [SerializeField] private GameObject videoObject;
    [SerializeField] private GameObject videoDecs;
    [SerializeField] private GameObject objectInfo;

    private MediaEntry _data;
    private VideoPlayer _videoPlayer;

    void Start() {
        if (objectInfo != null) {
            objectInfo.SetActive(false);
        }

        _videoPlayer = videoObject.GetComponent<VideoPlayer>();
        StartCoroutine(WaitForData());
    }

    IEnumerator WaitForData() {
        while (MediaManager.Instance == null || !MediaManager.Instance.isLoaded()) yield return null;

        _data = MediaManager.Instance.config.mediaList.Find(x => x.id == mediaId);

        for (int i = 0; i < _data.images.Length; i++) {
            ShowImage(i);
        }
        PlayMainVideo();

        // set tulisan buat video
        videoDecs.GetComponent<TMPro.TextMeshProUGUI>().text = _data.videoDesc;
    }

    public void PlayMainVideo() {
        if (_data == null) return;
        _videoPlayer.url = GetBaseUrl() + "assets/" + mediaId + "/" + _data.mainVideo;
    }

    public void PlayClip() { // lom dipake, gatau mau ditaro ke mana
        if (_data == null) return;
        _videoPlayer.url = GetBaseUrl() + "assets/" + mediaId + "/" + _data.clipVideo;
    }

    public void ShowImage(int index) {
        if (_data == null || index >= _data.images.Length) return;
        ImageEntry imgData = _data.images[index];
        StartCoroutine(LoadImage(index, imgData.fileName, imgData.desc));
    }

    IEnumerator LoadImage(int index, string fileName, string desc) {
    string url = GetBaseUrl() + "assets/" + mediaId + "/" + fileName;
    
    using (UnityWebRequest uwr = UnityWebRequestTexture.GetTexture(url)) {
        yield return uwr.SendWebRequest();

        if (uwr.result == UnityWebRequest.Result.Success) {
            // 1. Ambil Texture-nya
            Texture2D tex = DownloadHandlerTexture.GetContent(uwr);

            // 2. Buat Sprite dari Texture tersebut
            // Rect(0,0, lebar, tinggi) dan Pivot(0.5, 0.5) untuk tengah
            Sprite newSprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));

            cardObjects[index].SetCardInfo(newSprite, desc);
        } else {
            Debug.Log(index);
            Debug.LogError($"Failed to load: {uwr.error}");
        }
    }
}


    string GetBaseUrl() {
        #if UNITY_EDITOR
            return "http://localhost:8080/";
        #else
            string url = Application.absoluteURL;
            if (string.IsNullOrEmpty(url)) return "http://localhost:8080/";
            return url.Substring(0, url.LastIndexOf('/') + 1);
        #endif
    }

    private void OnTriggerEnter(Collider other) {
        objectInfo.SetActive(true);
    }

    private void OnTriggerExit(Collider other) {
        objectInfo.SetActive(false);
    }
}
