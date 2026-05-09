using System;
using System.Collections.Generic;

[Serializable]
public class MediaEntry 
{
    public string id;
    public string title;
    public string desc;
    public string poster;
    public string preview;
    public string video;
}

[Serializable]
public class MediaConfig 
{
    public List<MediaEntry> mediaList;
}