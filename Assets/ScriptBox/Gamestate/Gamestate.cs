using System;
using UnityEngine;

public class GameState : MonoBehaviour
{
    public static GameState Instance;

    public enum Phase
    {
        Planning,
        Running
    }

    [Header("State")]
    [SerializeField] private Phase currentPhase = Phase.Planning;

    public Phase CurrentPhase => currentPhase;
    public bool IsRunning => currentPhase == Phase.Running;
    public bool IsPlanning => currentPhase == Phase.Planning;

    public event Action<Phase> OnPhaseChanged;
    public event Action OnPlanningStarted;
    public event Action OnRunningStarted;

    private void Awake()
    {
        // Enforce instance isolation on initialization
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
        SetPhase(currentPhase == Phase.Planning
            ? Phase.Running
            : Phase.Planning);
    }

    public void SetPhase(Phase newPhase)
    {
        if (currentPhase == newPhase)
            return;

        currentPhase = newPhase;
        ApplyPhase(currentPhase);
    }

    private void ApplyPhase(Phase phase)
    {
        // Dispatch state changes to downstream listeners
        OnPhaseChanged?.Invoke(phase);

        switch (phase)
        {
            case Phase.Planning:
                GravitySystem.SetGravityEnabled(false);
                Debug.Log("Resetting...");

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