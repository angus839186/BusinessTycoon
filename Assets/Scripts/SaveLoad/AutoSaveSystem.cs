using System;
using System.Collections;
using UnityEngine;

public sealed class AutoSaveSystem : MonoBehaviour
{
    [SerializeField] private GameSaveSystem saveSystem;
    [SerializeField] private GameRules gameRules;
    [SerializeField] private bool logAutoSave = true;

    private Coroutine autoSaveCoroutine;

    public event Action AutoSaved;
    public event Action<string> AutoSaveFailed;

    private void Awake()
    {
        if (saveSystem == null)
            saveSystem = FindFirstObjectByType<GameSaveSystem>();

        if (gameRules == null)
            gameRules = FindFirstObjectByType<GameRules>();
    }

    private void Start()
    {
        if (saveSystem == null)
        {
            Debug.LogError(
                "AutoSaveSystem 找不到 GameSaveSystem。",
                this
            );

            return;
        }

        if (gameRules == null)
        {
            Debug.LogError(
                "AutoSaveSystem 找不到 GameRules。",
                this
            );

            return;
        }

        if (!gameRules.AutoSaveEnabled)
            return;

        autoSaveCoroutine =
            StartCoroutine(AutoSaveLoop());
    }

    private void OnDisable()
    {
        if (autoSaveCoroutine == null)
            return;

        StopCoroutine(autoSaveCoroutine);
        autoSaveCoroutine = null;
    }

    private IEnumerator AutoSaveLoop()
    {
        while (true)
        {
            float waitSeconds =
                gameRules.AutoSaveIntervalMinutes * 60f;

            yield return new WaitForSecondsRealtime(
                waitSeconds
            );

            SaveAutoSlot();
        }
    }

    private void SaveAutoSlot()
    {
        string previousSlotId =
            saveSystem.ActiveSlotId;

        try
        {
            if (!saveSystem.SetActiveSlot(
                    SaveSlots.AutoSaveId
                ))
            {
                NotifyFailure(
                    "無法選擇自動存檔槽。"
                );

                return;
            }

            saveSystem.SaveGame();

            if (logAutoSave)
            {
                Debug.Log(
                    $"Auto save completed: " +
                    $"{saveSystem.SavePath}",
                    this
                );
            }

            AutoSaved?.Invoke();
        }
        catch (Exception exception)
        {
            NotifyFailure(exception.Message);
        }
        finally
        {
            saveSystem.SetActiveSlot(
                previousSlotId
            );
        }
    }

    private void NotifyFailure(string message)
    {
        Debug.LogError(
            $"Auto save failed: {message}",
            this
        );

        AutoSaveFailed?.Invoke(message);
    }
}