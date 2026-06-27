using UnityEngine;

public class GameStateUI : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator uiAnimator;

    [Header("Animation States")]
    [SerializeField] private string staticStateName = "StaticDefault";
    [SerializeField] private string runningAnimationName = "RunningLoop";

    private void Start()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.OnPhaseChanged += HandlePhaseChanged;
            HandlePhaseChanged(GameState.Instance.CurrentPhase);
        }
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.OnPhaseChanged -= HandlePhaseChanged;
        }
    }

    private void HandlePhaseChanged(GameState.Phase newPhase)
    {
        if (uiAnimator == null) return;

        switch (newPhase)
        {
            case GameState.Phase.Planning:
            case GameState.Phase.Pause:
                uiAnimator.Play(staticStateName, 0, 0f);
                break;

            case GameState.Phase.Running:
                uiAnimator.Play(runningAnimationName, 0, 0f);
                break;
        }
    }
}