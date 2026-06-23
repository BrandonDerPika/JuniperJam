using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public enum Phase
    {
        Planning,
        Running,
        Pause
    }

    [Header("State")]
    [SerializeField] private Phase currentPhase = Phase.Planning;

    public Phase CurrentPhase => currentPhase;
    public bool IsRunning => currentPhase == Phase.Running;
    public bool IsPlanning => currentPhase == Phase.Planning;
    public bool IsPaused => currentPhase == Phase.Pause;
    
    public bool IsPlanningOrPaused => currentPhase == Phase.Planning || currentPhase == Phase.Pause;

    public event Action<Phase> OnPhaseChanged;
    public event Action OnPlanningStarted;
    public event Action OnRunningStarted;
    public event Action OnCheckpointSnapped;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        ApplyPhase(currentPhase);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            TogglePhase();
        }
    }

    public void TogglePhase()
    {
        if (currentPhase == Phase.Running)
        {
            // If hit a checkpoint before go back to Pause state
            SetPhase(Checkpoint.HasReachedCheckpoint ? Phase.Pause : Phase.Planning);
        }
        else
        {
            SetPhase(Phase.Running);
        }
    }

    public void SetPhase(Phase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        currentPhase = newPhase;
        ApplyPhase(currentPhase);
    }

    public void TriggerCheckpoint()
    {
        OnCheckpointSnapped?.Invoke();
    }

    private void ApplyPhase(Phase phase)
    {
        OnPhaseChanged?.Invoke(phase);

        switch (phase)
        {
            case Phase.Planning:
            case Phase.Pause:
                GravitySystem.SetGravityEnabled(false);
                Debug.Log($"Resetting to {phase}...");
                OnPlanningStarted?.Invoke();
                break;

            case Phase.Running:
                GravitySystem.SetGravityEnabled(true);
                Debug.Log("Running...");
                OnRunningStarted?.Invoke();
                break;
        }
    }
}
