using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class MediaManager : MonoBehaviour {
    public static MediaManager Instance;
    public MediaConfig config;
    private bool _isLoaded = false;

void Awake() {
    if (Instance == null) {
        Instance = this;
        DontDestroyOnLoad(gameObject); // ????
    } else {
        Destroy(gameObject); // Hapus kalau sudah ada instance lain
    }
}

    IEnumerator Start() {
        string url = GetBaseUrl() + "assets/media_config.json";
        using (UnityWebRequest uwr = UnityWebRequest.Get(url)) {
            yield return uwr.SendWebRequest();
            if (uwr.result == UnityWebRequest.Result.Success) {
                config = JsonUtility.FromJson<MediaConfig>(uwr.downloadHandler.text);
                _isLoaded = true;
                Debug.Log("Config loaded!");
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

    public bool isLoaded() {
        return _isLoaded;
    }
}