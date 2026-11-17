using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public sealed class LoadingController : MonoBehaviour
{
    [SerializeField] private LoadingUI loadingUI;

    private Coroutine loadCoroutine;

    public bool IsLoading =>
        loadCoroutine != null;

    private void Awake()
    {
        if (loadingUI == null)
        {
            loadingUI =
                GetComponent<LoadingUI>();
        }
    }

    public void LoadScene(
        string sceneName,
        string message = "載入中..."
    )
    {
        if (loadCoroutine != null)
            return;

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError(
                "LoadingController 收到空白場景名稱。",
                this
            );

            return;
        }

        if (loadingUI == null)
        {
            Debug.LogError(
                "LoadingController 找不到 LoadingUI。",
                this
            );

            return;
        }

        loadCoroutine = StartCoroutine(
            LoadSceneRoutine(
                sceneName,
                message
            )
        );
    }

    public void RunTask(
    string message,
    Func<bool> task,
    Action<bool> onCompleted = null
)
    {
        if (loadCoroutine != null)
            return;

        if (task == null)
        {
            Debug.LogError(
                "LoadingController 收到空白工作。",
                this
            );

            return;
        }

        if (loadingUI == null)
        {
            Debug.LogError(
                "LoadingController 找不到 LoadingUI。",
                this
            );

            return;
        }

        loadCoroutine = StartCoroutine(
            RunTaskRoutine(
                message,
                task,
                onCompleted
            )
        );
    }

    private IEnumerator LoadSceneRoutine(
        string sceneName,
        string message
    )
    {
        loadingUI.Show(message);

        yield return null;

        AsyncOperation operation;

        try
        {
            operation =
                SceneManager.LoadSceneAsync(
                    sceneName,
                    LoadSceneMode.Single
                );
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);

            loadingUI.Hide();
            loadCoroutine = null;
            yield break;
        }

        if (operation == null)
        {
            Debug.LogError(
                $"無法載入場景：{sceneName}",
                this
            );

            loadingUI.Hide();
            loadCoroutine = null;
            yield break;
        }

        while (!operation.isDone)
        {
            float progress =
                Mathf.Clamp01(
                    operation.progress / 0.9f
                );

            loadingUI.SetProgress(progress);

            yield return null;
        }

        loadingUI.SetProgress(1f);
        loadCoroutine = null;
    }
    private IEnumerator RunTaskRoutine(
        string message,
        Func<bool> task,
        Action<bool> onCompleted
    )
    {
        loadingUI.Show(message);

        yield return null;

        bool succeeded = false;

        try
        {
            succeeded = task.Invoke();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception, this);
        }

        loadingUI.SetProgress(1f);

        yield return null;

        loadingUI.Hide();
        loadCoroutine = null;

        onCompleted?.Invoke(succeeded);
    }
}