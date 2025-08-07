using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ASyncLoader : Singleton<ASyncLoader>
{
    [Header("Menu screens")]
    [SerializeField] private GameObject[] objectsToHide;
    [SerializeField] private GameObject loadingScreen;

    [Header("Loading Progress Slider (Not required)")]
    [SerializeField] private Slider loadingProgressSlider;
    [SerializeField] private TextMeshProUGUI loadingProgressText;

    public void LoadScene(string targetSceneName)
    {
        foreach (GameObject obj in objectsToHide)
        {
            obj.SetActive(false);
        }

        loadingScreen.SetActive(true);
        StartCoroutine(LoadSceneASync(targetSceneName));
    }

    IEnumerator LoadSceneASync(string targetSceneName)
    {
        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(targetSceneName);

        while (!asyncOperation.isDone)
        {
            float progressValue = Mathf.Clamp01(asyncOperation.progress / 0.9f);
            if (loadingProgressSlider != null)
            {
                loadingProgressSlider.value = progressValue;
            }
            if (loadingProgressText != null)
            {
                loadingProgressText.text = $"Loading...{Mathf.RoundToInt(progressValue * 100)}%";
            }

            yield return null;
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        StopAllCoroutines();
    }
}