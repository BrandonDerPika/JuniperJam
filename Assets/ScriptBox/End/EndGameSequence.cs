using System.Collections;
using UnityEngine;

public class EndGameSequence : MonoBehaviour
{
    [Header("Timing")]
    public float delayBeforeStart = 3f;

    [Header("Wave Object (EXISTING IN SCENE)")]
    public GameObject waveObject;

    [Header("Movement")]
    public float waveSpeed = 5f;
    public float moveDistance = 10f;

    [Header("SFX")]
    public AudioSource sfxSource;
    public AudioClip endClip;

    [Header("Infinite Scroll")]
    public InfiniteScroll infiniteScroll;

    private bool hasRun;

    void Start()
    {
        StartCoroutine(Run());
    }

    IEnumerator Run()
    {
        if (hasRun) yield break;
        hasRun = true;

        yield return new WaitForSeconds(delayBeforeStart);

        if (sfxSource != null && endClip != null)
        {
            sfxSource.PlayOneShot(endClip);
        }

        if (waveObject == null) yield break;

        float travelled = 0f;

        while (travelled < moveDistance)
        {
            float step = waveSpeed * Time.deltaTime;

            waveObject.transform.position += Vector3.right * step;
            travelled += step;

            yield return null;
        }

        if (infiniteScroll != null)
        {
            infiniteScroll.isEnabled = true;
        }
    }
}