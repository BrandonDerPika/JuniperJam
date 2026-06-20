using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(LineRenderer))]
public class PlayerController : MonoBehaviour
{
    [Header("Launch Settings")]
    [SerializeField] private float launchPower = 8f;
    [SerializeField] private float maxDragDistance = 3.5f;

    private Rigidbody2D rb;
    private LineRenderer line;

    private bool isAiming;
    private bool hasLockedShot;
    private bool hasLaunchedThisRun;

    private Vector2 aimStartWorld;
    private Vector2 aimCurrentWorld;
    private Vector2 launchVelocity;

    private bool lastPlanningState;

    // Snapshot storage definitions for scene rollbacks
    private Vector2 initialPosition;
    private Vector2 initialVelocity;

    private Vector2 initialAimStart;
    private Vector2 initialLaunchVelocity;
    private bool initialHasLockedShot;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        line = GetComponent<LineRenderer>();

        line.positionCount = 2;
        line.enabled = false;
    }

    private void Start()
    {
        CaptureSnapshot();
    }

    private void CaptureSnapshot()
    {
        initialPosition = rb.position;
        initialVelocity = rb.velocity;

        initialAimStart = aimStartWorld;
        initialLaunchVelocity = launchVelocity;
        initialHasLockedShot = hasLockedShot;
    }

    private void RestoreSnapshot()
    {
        rb.velocity = Vector2.zero;
        rb.position = initialPosition;

        aimStartWorld = initialAimStart;
        launchVelocity = initialLaunchVelocity;
        hasLockedShot = initialHasLockedShot;

        hasLaunchedThisRun = false;

        if (hasLockedShot)
        {
            isAiming = false;
            line.enabled = true;
            Vector2 dragVector = launchVelocity / launchPower;
            DrawLine(dragVector);
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

        bool isPlanning = GameState.Instance.IsPlanning;

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
            UpdateAim();
    }

    private void HandleInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (!isAiming && !hasLockedShot)
            {
                StartAiming();
            }
            else if (isAiming)
            {
                LockAim();
            }
            else if (hasLockedShot)
            {
                ModifyExistingAim();
            }
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

        // Shift vector configuration origin outward to anchor current cursor to the existing vector tip
        aimStartWorld = currentMouse - existingDragVector;
    }

    private void UpdateAim()
    {
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        aimCurrentWorld = new Vector2(mouseWorld.x, mouseWorld.y);

        // Calculate directional vector extending directly toward the system tracking point
        Vector2 dragVector = aimCurrentWorld - aimStartWorld;

        if (dragVector.magnitude > maxDragDistance)
            dragVector = dragVector.normalized * maxDragDistance;

        launchVelocity = dragVector * launchPower;

        DrawLine(dragVector);
    }

    private void DrawLine(Vector2 dragVector)
    {
        line.SetPosition(0, rb.position);
        line.SetPosition(1, rb.position + dragVector);
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
}