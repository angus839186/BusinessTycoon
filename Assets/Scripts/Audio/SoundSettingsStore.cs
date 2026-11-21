using System.IO;
using System.Text;
using UnityEngine;

public static class SoundSettingsStore
{
    private const string FolderName =
        "Settings";

    private const string FileName =
        "audio.json";

    public static string FilePath =>
        Path.Combine(
            Application.persistentDataPath,
            FolderName,
            FileName
        );

    public static SoundSettings Load()
    {
        if (!File.Exists(FilePath))
        {
            return new SoundSettings();
        }

        try
        {
            string json =
                File.ReadAllText(
                    FilePath,
                    Encoding.UTF8
                );

            SoundSettings settings =
                JsonUtility.FromJson<SoundSettings>(
                    json
                );

            if (settings == null)
            {
                return new SoundSettings();
            }

            settings.musicVolume =
                Mathf.Clamp01(settings.musicVolume);

            settings.sfxVolume =
                Mathf.Clamp01(settings.sfxVolume);

            settings.selectedTrack ??= string.Empty;

            return settings;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(
                $"Cannot load audio settings: " +
                $"{exception.Message}"
            );

            return new SoundSettings();
        }
    }

    public static bool Save(SoundSettings settings)
    {
        if (settings == null)
        {
            return false;
        }

        try
        {
            string folderPath =
                Path.GetDirectoryName(FilePath);

            if (!string.IsNullOrEmpty(folderPath))
            {
                Directory.CreateDirectory(
                    folderPath
                );
            }

            settings.musicVolume =
                Mathf.Clamp01(settings.musicVolume);

            settings.sfxVolume =
                Mathf.Clamp01(settings.sfxVolume);

            string json =
                JsonUtility.ToJson(
                    settings,
                    true
                );

            File.WriteAllText(
                FilePath,
                json,
                Encoding.UTF8
            );

            return true;
        }
        catch (System.Exception exception)
        {
            Debug.LogWarning(
                $"Cannot save audio settings: " +
                $"{exception.Message}"
            );

            return false;
        }
    }
}