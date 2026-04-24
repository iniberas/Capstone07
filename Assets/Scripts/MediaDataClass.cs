using System;
using System.Collections.Generic;

[System.Serializable]
public class ImageEntry {
    public string fileName;
    public string desc;
}

[Serializable]
public class MediaEntry {
    public string id;
    public ImageEntry[] images;
    public string mainVideo;
    public string videoDesc;
    public string clipVideo;
    public string[] desc;
}

[Serializable]
public class MediaConfig {
    public List<MediaEntry> mediaList;
}