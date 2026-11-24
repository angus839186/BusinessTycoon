using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public sealed partial class MusicPlayer
{
    private static readonly string[] SupportedExtensions =
    {
        ".ogg",
        ".mp3",
        ".wav"
    };

    private readonly List<string> trackPaths = new List<string>();
    private readonly List<string> trackNames = new List<string>();

    public IReadOnlyList<string> TrackNames => trackNames;

    public string BuiltInMusicFolderPath => Path.Combine(Application.streamingAssetsPath, "Audio", "Music");

    public void RefreshTracks()
    {
        trackPaths.Clear();
        trackNames.Clear();

        try
        {
            AddTracksFromFolder(BuiltInMusicFolderPath);
        }
        catch (Exception exception)
        {
            Debug.LogWarning($"讀取音樂資料夾失敗：{exception.Message}", this);
        }

        TracksChanged?.Invoke();
    }



    private void AddTracksFromFolder(string folderPath)
    {
        if (!Directory.Exists(folderPath))
            return;

        string[] files = Directory.GetFiles(folderPath, "*", SearchOption.TopDirectoryOnly);
        Array.Sort(files, StringComparer.OrdinalIgnoreCase);

        foreach (string filePath in files)
        {
            if (!IsSupported(filePath))
                continue;

            trackPaths.Add(filePath);
            trackNames.Add(Path.GetFileName(filePath));
        }
    }

    private static bool IsSupported(string filePath)
    {
        string extension = Path.GetExtension(filePath);

        foreach (string supportedExtension in SupportedExtensions)
        {
            if (string.Equals(extension, supportedExtension, StringComparison.OrdinalIgnoreCase))
                return true;
        }

        return false;
    }

    private static AudioType GetAudioType(string filePath)
    {
        string extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".ogg" => AudioType.OGGVORBIS,
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            _ => AudioType.UNKNOWN
        };
    }
}