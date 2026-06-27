using System.Collections;
using UnityEngine;

public class GameStart : MonoBehaviour
{
    [Header("Music")]
    public AudioSource musicSource;
    public float musicFadeDuration = 1.5f;

    [Header("Screen Fade")]
    public CanvasGroup fadeGroup;
    public float screenFadeDuration = 1f;

    void Start()
    {
        StartCoroutine(BeginGame());
    }

    IEnumerator BeginGame()
    {
        float targetVolume = 1f;

        if (musicSource != null)
        {
            targetVolume = musicSource.volume;
            musicSource.volume = 0f;

            if (!musicSource.isPlaying)
                musicSource.Play();
        }

        float timer = 0f;

        while (timer < Mathf.Max(musicFadeDuration, screenFadeDuration))
        {
            timer += Time.deltaTime;

            if (musicSource != null)
            {
                musicSource.volume = Mathf.Lerp(0f, targetVolume, timer / musicFadeDuration);
            }

            if (fadeGroup != null)
            {
                fadeGroup.alpha = Mathf.Lerp(1f, 0f, timer / screenFadeDuration);
            }

            yield return null;
        }

        if (musicSource != null)
            musicSource.volume = targetVolume;

        if (fadeGroup != null)
        {
            fadeGroup.alpha = 0f;
            Destroy(fadeGroup.gameObject);
        }
    }
}
