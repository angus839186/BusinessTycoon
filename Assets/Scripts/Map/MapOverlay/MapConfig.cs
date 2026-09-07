using System;

[Serializable]
public sealed class MapConfig
{
    public string mapId;
    public MapLayerConfig[] layers;
}

[Serializable]
public sealed class MapLayerConfig
{
    public string id;
    public string displayName;
    public string path;
    public string category;
    public string geometry;
    public string[] buildTags;
}