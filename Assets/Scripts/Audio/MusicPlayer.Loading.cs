using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public sealed partial class MusicPlayer
{
    private AudioClip loadedClip;
    private Coroutine playRoutine;

    private void StartTrack(string filePath)
    {
        StopRoutine();

        playRoutine =
            StartCoroutine(
                LoadAndPlay(filePath)
            );
    }

    private IEnumerator LoadAndPlay(
        string filePath
    )
    {
        AudioType audioType =
            GetAudioType(filePath);

        string uri =
            new Uri(
                Path.GetFullPath(filePath)
            ).AbsoluteUri;

        using UnityWebRequest request =
            UnityWebRequestMultimedia.GetAudioClip(
                uri,
                audioType
            );

        DownloadHandlerAudioClip handler =
            request.downloadHandler
                as DownloadHandlerAudioClip;

        if (handler != null)
        {
            handler.streamAudio = true;
        }

        yield return request.SendWebRequest();

        if (
            request.result !=
            UnityWebRequest.Result.Success
        )
        {
            Debug.LogWarning(
                $"無法載入音樂 " +
                $"{Path.GetFileName(filePath)}：" +
                $"{request.error}",
                this
            );

            playRoutine = null;
            yield break;
        }

        AudioClip clip =
            DownloadHandlerAudioClip.GetContent(
                request
            );

        if (clip == null)
        {
            playRoutine = null;
            yield break;
        }

        ReleaseClip();

        loadedClip = clip;
        source.clip = loadedClip;

        currentTrack =
            Path.GetFileName(filePath);

        isPaused = false;

        source.loop =
            audioSystem.Settings.loopCurrentTrack;

        source.Play();

        TrackChanged?.Invoke(currentTrack);
        PauseChanged?.Invoke(false);

        WaitForSecondsRealtime wait =
            new WaitForSecondsRealtime(0.25f);

        while (
            source != null &&
            source.clip == loadedClip
        )
        {
            if (
                !isPaused &&
                !source.isPlaying
            )
            {
                break;
            }

            yield return wait;
        }

        if (
            source != null &&
            source.clip == loadedClip &&
            !isPaused
        )
        {
            playRoutine = null;
            PlayNext();
        }
    }
    private void StopRoutine()
    {
        if (playRoutine == null)
        {
            return;
        }

        StopCoroutine(playRoutine);
        playRoutine = null;
    }

    private void ReleaseClip()
    {
        if (source != null)
        {
            source.Stop();
            source.clip = null;
        }

        if (loadedClip != null)
        {
            Destroy(loadedClip);
            loadedClip = null;
        }
    }
}