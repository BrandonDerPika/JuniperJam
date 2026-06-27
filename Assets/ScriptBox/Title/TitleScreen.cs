using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class TitleScreen : MonoBehaviour
{
    [Header("Scene Settings")]
    public string sceneToLoad = "GameScene";

    [Header("Audio")]
    public AudioSource musicSource;
    public float musicFadeDuration = 1.5f;

    [Header("Screen Fade")]
    public CanvasGroup fadeGroup;
    public float screenFadeDuration = 1f;

    private bool isLoading = false;

    void Update()
    {
        if (isLoading) return;

        if (Input.GetMouseButtonDown(0) ||
            (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began))
        {
            StartCoroutine(Transition());
        }
    }

    IEnumerator Transition()
    {
        isLoading = true;

        float startVolume = musicSource != null ? musicSource.volume : 0f;
        float duration = Mathf.Max(musicFadeDuration, screenFadeDuration);
        float t = 0f;

        while (t < duration)
        {
            t += Time.deltaTime;

            if (musicSource != null)
            {
                float musicProgress = Mathf.Clamp01(t / musicFadeDuration);
                musicSource.volume = Mathf.Lerp(startVolume, 0f, musicProgress);
            }

            if (fadeGroup != null)
            {
                float fadeProgress = Mathf.Clamp01(t / screenFadeDuration);
                fadeGroup.alpha = Mathf.Lerp(0f, 1f, fadeProgress);
            }

            yield return null;
        }

        if (musicSource != null)
            musicSource.volume = 0f;

        if (fadeGroup != null)
            fadeGroup.alpha = 1f;

        SceneManager.LoadScene(sceneToLoad);
    }
}
