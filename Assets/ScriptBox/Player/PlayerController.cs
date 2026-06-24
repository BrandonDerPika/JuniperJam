using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float launchPower = 8f;
    [SerializeField] private float maxDragDistance = 3.5f;

    [Header("Trajectory Settings")]
    [SerializeField] private int simulationSteps = 180;
    [SerializeField] private float stepTimeDelta = 0.02f;
    [SerializeField] private float maxForcePerSource = 100f;
    [SerializeField] private float maxVelocityMagnitude = 50f;

    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isAiming;
    private bool hasLockedShot;
    private bool hasLaunchedThisRun;

    private Vector2 aimStartWorld;
    private Vector2 aimCurrentWorld;
    private Vector2 launchVelocity;

    private bool lastPlanningState;
    private Vector2 initialPosition;
    private Vector2 initialVelocity;

    private Vector2 initialAimStart;
    private Vector2 initialLaunchVelocity;
    private bool initialHasLockedShot;

    // Reference to our new animator script
    private PlayerAnimator playerAnimator;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();
        playerAnimator = GetComponent<PlayerAnimator>();

        line.positionCount = 0;
        line.enabled = false;
    }

    private void Start()
    {
        CaptureSnapshot();
        if (GameState.Instance != null)
        {
            GameState.Instance.OnCheckpointSnapped += UpdateCheckpointSnapshot;
        }
    }

    private void OnDestroy()
    {
        if (GameState.Instance != null)
        {
            GameState.Instance.OnCheckpointSnapped -= UpdateCheckpointSnapshot;
        }
    }

    private void CaptureSnapshot()
    {
        initialPosition = rb.position;
        initialVelocity = rb.velocity;

        initialAimStart = aimStartWorld;
        initialLaunchVelocity = launchVelocity;
        initialHasLockedShot = hasLockedShot;
    }

    private void UpdateCheckpointSnapshot()
    {
        initialPosition = rb.position;
        initialVelocity = rb.velocity;
        
        hasLockedShot = false;
        initialHasLockedShot = false;
        launchVelocity = Vector2.zero;
        initialLaunchVelocity = Vector2.zero;
    }

    private void RestoreSnapshot()
    {
        rb.velocity = Vector2.zero;
        rb.position = initialPosition;

        aimStartWorld = initialAimStart;
        launchVelocity = initialLaunchVelocity;
        hasLockedShot = initialHasLockedShot;

        hasLaunchedThisRun = false;

        // Tell the animator to clear any active flashes/hit states
        if (playerAnimator != null)
        {
            playerAnimator.ResetVisuals();
        }

        if (hasLockedShot)
        {
            isAiming = false;
            line.enabled = true;
            CalculateAndDrawCurvedTrajectory(launchVelocity);
        }
        else
        {
            isAiming = false;
            line.enabled = false;
        }
    }

    private void Update()
    {
        if (GameState.Instance == null)
            return;

        bool isPlanning = GameState.Instance.IsPlanningOrPaused;

        if (isPlanning != lastPlanningState)
        {
            lastPlanningState = isPlanning;

            if (isPlanning)
                EnterPlanning();
            else
                EnterRunning();
        }

        if (isPlanning)
            HandlePlanning();
        else
            ForceRunningRules();
    }

    private void EnterPlanning()
    {
        RestoreSnapshot();
    }

    private void HandlePlanning()
    {
        HandleInput();

        if (isAiming)
        {
            UpdateAim();
        }
        else if (hasLockedShot)
        {
            CalculateAndDrawCurvedTrajectory(launchVelocity);
        }
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;

            if (!hasLockedShot)
            {
                StartAiming();
            }
            else
            {
                ModifyExistingAim();
            }
        }

        if (Input.GetMouseButtonUp(0) && isAiming)
        {
            LockAim();
        }
    }

    private void StartAiming()
    {
        isAiming = true;
        hasLockedShot = false;
        aimStartWorld = rb.position;
        line.enabled = true;
    }

    private void ModifyExistingAim()
    {
        isAiming = true;
        hasLockedShot = false;
        line.enabled = true;

        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 currentMouse = new Vector2(mouseWorld.x, mouseWorld.y);
        Vector2 existingDragVector = launchVelocity / launchPower;

        aimStartWorld = currentMouse - existingDragVector;
    }

    private void UpdateAim()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        aimCurrentWorld = new Vector2(mouseWorld.x, mouseWorld.y);

        Vector2 dragVector = aimCurrentWorld - aimStartWorld;

        if (dragVector.magnitude > maxDragDistance)
            dragVector = dragVector.normalized * maxDragDistance;

        launchVelocity = dragVector * launchPower;

        CalculateAndDrawCurvedTrajectory(launchVelocity);
    }

    private void CalculateAndDrawCurvedTrajectory(Vector2 startVelocity)
    {
        line.enabled = true;

        List<Vector3> simulationPoints = new List<Vector3>();
        Vector2 currentSimPosition = rb.position;
        Vector2 currentSimVelocity = startVelocity;

        simulationPoints.Add(currentSimPosition);

        GravityBody bodyComponent = GetComponent<GravityBody>();
        float minEscapeSpeed = bodyComponent != null ? bodyComponent.MinEscapeSpeed : 1.2f;

        for (int step = 0; step < simulationSteps; step++)
        {
            Vector2 combinedGravityForce = Vector2.zero;

            for (int i = 0; i < GravitySource.ActiveSources.Count; i++)
            {
                GravitySource source = GravitySource.ActiveSources[i];
                if (source == null || !source.isActiveAndEnabled)
                    continue;

                Vector2 force = GetSimulatedForceOn(source, currentSimPosition, currentSimVelocity, minEscapeSpeed);

                if (float.IsNaN(force.x) || float.IsNaN(force.y) || float.IsInfinity(force.x) || float.IsInfinity(force.y))
                    continue;

                if (force.sqrMagnitude > maxForcePerSource * maxForcePerSource)
                    force = force.normalized * maxForcePerSource;

                combinedGravityForce += force;
            }

            currentSimVelocity += combinedGravityForce * stepTimeDelta;

            if (currentSimVelocity.sqrMagnitude > maxVelocityMagnitude * maxVelocityMagnitude)
            {
                currentSimVelocity = currentSimVelocity.normalized * maxVelocityMagnitude;
            }

            currentSimPosition += currentSimVelocity * stepTimeDelta;
            simulationPoints.Add(currentSimPosition);
        }

        line.positionCount = simulationPoints.Count;
        line.SetPositions(simulationPoints.ToArray());
    }

    private Vector2 GetSimulatedForceOn(GravitySource source, Vector2 position, Vector2 velocity, float minEscapeSpeed)
    {
        Vector2 sourcePos = source.transform.position;
        Vector2 toSource = sourcePos - position;
        float distance = toSource.magnitude;

        if (distance > source.InfluenceRadius || distance <= Mathf.Epsilon)
            return Vector2.zero;

        Vector2 inwardDir = toSource.normalized; 

        float range01 = Mathf.Clamp01(distance / source.InfluenceRadius);
        float rangeFalloff = 1f - range01;
        float inverseSoftened = 1f / (1f + source.Softening * distance * distance);

        float radialStrength = source.GravityStrength * rangeFalloff * rangeFalloff * inverseSoftened;
        Vector2 force = inwardDir * radialStrength;

        if (source.TangentialAssist > 0f && distance <= source.TangentialAssistRadius)
        {
            Vector2 tangent = new Vector2(-inwardDir.y, inwardDir.x);

            if (Vector2.Dot(velocity, tangent) < 0f)
                tangent = -tangent;

            float assist01 = Mathf.Clamp01(1f - (distance / source.TangentialAssistRadius));
            float assistStrength = source.GravityStrength * source.TangentialAssist * assist01 * assist01;

            force += tangent * assistStrength;
        }

        if (distance < source.EscapeAssistRadius && velocity.magnitude < minEscapeSpeed)
        {
            float speed01 = Mathf.Clamp01(velocity.magnitude / Mathf.Max(minEscapeSpeed, 0.01f));
            float escapeBoost = (1f - speed01) * source.EscapeAssistStrength;
            force += -inwardDir * escapeBoost;
        }

        return force;
    }

    private void LockAim()
    {
        isAiming = false;
        hasLockedShot = true;
        line.enabled = true;

        CaptureSnapshot();
    }

    private void EnterRunning()
    {
        isAiming = false;
        line.enabled = false;
        hasLaunchedThisRun = false;
    }

    private void ForceRunningRules()
    {
        line.enabled = false;

        if (hasLaunchedThisRun)
            return;

        if (!hasLockedShot)
            return;

        rb.velocity = launchVelocity;
        hasLaunchedThisRun = true;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (GameState.Instance != null && GameState.Instance.IsPlanningOrPaused)
            return;

        if (playerAnimator != null) playerAnimator.TriggerHitVisual();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GameState.Instance != null && GameState.Instance.IsPlanningOrPaused)
            return;

        if (playerAnimator != null) playerAnimator.TriggerHitVisual();
    }
}