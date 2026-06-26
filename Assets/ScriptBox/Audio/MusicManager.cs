using System.Collections;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [Header("Music Sources")]
    [SerializeField] private AudioSource planningSource;
    [SerializeField] private AudioSource runningSource;

    [Header("Settings")]
    [SerializeField] private float fadeDuration = 1.5f;

    [Range(0f, 1f)]
    [SerializeField] private float maxMusicVolume = 0.4f;

    private bool lastPlanningState;
    private Coroutine fadeRoutine;

    private void Start()
    {
        if (GameState.Instance == null)
            return;

        lastPlanningState = GameState.Instance.IsPlanningOrPaused;

        planningSource.Play();
        runningSource.Play();

        ApplyInstantVolumes(lastPlanningState);
    }

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        bool currentPlanningState = GameState.Instance.IsPlanningOrPaused;

        if (currentPlanningState != lastPlanningState)
        {
            lastPlanningState = currentPlanningState;

            if (fadeRoutine != null)
                StopCoroutine(fadeRoutine);

            fadeRoutine = StartCoroutine(Crossfade(currentPlanningState));
        }
    }

    private void ApplyInstantVolumes(bool toPlanning)
    {
        planningSource.volume = toPlanning ? maxMusicVolume : 0f;
        runningSource.volume = toPlanning ? 0f : maxMusicVolume;
    }

    private IEnumerator Crossfade(bool toPlanning)
    {
        if (!toPlanning)
        {
            runningSource.time = Random.value > 0.5f ? 10f : 0f;
        }

        float timer = 0f;

        float planningStart = planningSource.volume;
        float runningStart = runningSource.volume;

        float planningTarget = toPlanning ? maxMusicVolume : 0f;
        float runningTarget = toPlanning ? 0f : maxMusicVolume;

        while (timer < fadeDuration)
        {
            timer += Time.deltaTime;

            float t = timer / fadeDuration;

            planningSource.volume = Mathf.Lerp(planningStart, planningTarget, t);
            runningSource.volume = Mathf.Lerp(runningStart, runningTarget, t);

            yield return null;
        }

        planningSource.volume = planningTarget;
        runningSource.volume = runningTarget;
    }
}