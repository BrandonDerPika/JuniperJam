using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameFeelManager : MonoBehaviour
{
    private static GameFeelManager instance;

    // ==============================
    // SCREEN SHAKE
    // ==============================
    public enum ShakeDirection
    {
        Horizontal,
        Vertical,
        Random
    }

    [Header("Screen Shake Settings")]
    [SerializeField] private ShakeDirection shakeDirection = ShakeDirection.Random;

    private Vector3 originalPosition;
    private float shakeTimer;
    private float shakeDuration;
    private float shakeIntensity;



    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        originalPosition = transform.localPosition;
    }

    private void Update()
    {
        HandleScreenShake();
    }

    // ==============================
    // SCREEN SHAKE LOGIC
    // ==============================
    private void HandleScreenShake()
    {
        if (shakeTimer <= 0f)
        {
            transform.localPosition = originalPosition;
            return;
        }

        shakeTimer -= Time.unscaledDeltaTime;

        Vector3 offset = Vector3.zero;

        switch (shakeDirection)
        {
            case ShakeDirection.Horizontal:
                offset = new Vector3(Random.Range(-1f, 1f) * shakeIntensity, 0f, 0f);
                break;

            case ShakeDirection.Vertical:
                offset = new Vector3(0f, Random.Range(-1f, 1f) * shakeIntensity, 0f);
                break;

            case ShakeDirection.Random:
                offset = new Vector3(
                    Random.Range(-1f, 1f) * shakeIntensity,
                    Random.Range(-1f, 1f) * shakeIntensity,
                    0f);
                break;
        }

        transform.localPosition = originalPosition + offset;
    }

    /// <summary>
    /// Triggers a screen shake
    /// </summary>
    public static void Shake(float duration, float intensity)
    {
        if (instance == null) return;

        instance.shakeDuration = duration;
        instance.shakeIntensity = intensity;
        instance.shakeTimer = duration;
        instance.originalPosition = instance.transform.localPosition;
    }

    // ==============================
    // HIT STOP / TIME STOP
    // ==============================
    /// <summary>
    /// Freezes time for a short duration
    /// </summary>
    public static void DoHitStop(float duration)
    {
        if (instance != null)
            instance.StartCoroutine(instance.HitStopCoroutine(duration));
    }

    private IEnumerator HitStopCoroutine(float duration)
    {
        float originalTimeScale = Time.timeScale;
        float originalFixedDelta = Time.fixedDeltaTime;

        Time.timeScale = 0f;
        Time.fixedDeltaTime = 0f;

        yield return new WaitForSecondsRealtime(duration);

        Time.timeScale = originalTimeScale;
        Time.fixedDeltaTime = originalFixedDelta;
    }

    // ==============================
    // COMBO (COMMON GAME FEEL USE)
    // ==============================
    /// <summary>
    /// Shake + Hit Stop together
    /// </summary>
    public static void Impact(
        float hitStopDuration,
        float shakeDuration,
        float shakeIntensity)
    {
        DoHitStop(hitStopDuration);
        Shake(shakeDuration, shakeIntensity);
    }
}
