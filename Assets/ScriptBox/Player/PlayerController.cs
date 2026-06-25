using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
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

    [Header("Visual Trajectory Settings")]
    [SerializeField] private Sprite trajectorySprite;
    [SerializeField] private int maxTrajectorySprites = 15; 
    [Range(1, 20)] 
    [SerializeField] private int stepInterval = 6;           
    [SerializeField] private Vector3 startScale = Vector3.one;
    [SerializeField] private Vector3 minScale = new Vector3(0.3f, 0.3f, 1f); 
    [Range(0f, 1f)]
    [SerializeField] private float minAlpha = 0.1f;          

    [Header("Cleanliness & Animation Settings")]
    [SerializeField] private float minSpawnDistanceRadius = 0.8f; 
    [SerializeField] private float revealSpeed = 30f; 
    
    [Header("Sorting Layer Settings")]
    [SerializeField] private string sortingLayerName = "Default"; 
    [SerializeField] private int sortingOrder = 10;                

    private Rigidbody2D rb;
    private List<SpriteRenderer> spritePool = new List<SpriteRenderer>();

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

    private PlayerAnimator playerAnimator;
    private Planner planner; 
    
    private float currentAllowedVisibleDots = 0f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerAnimator = GetComponent<PlayerAnimator>();
        planner = FindFirstObjectByType<Planner>(); 

        InitializeSpritePool();
        SetTrajectoryVisible(false);
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

    private void InitializeSpritePool()
    {
        foreach (var sr in spritePool)
        {
            if (sr != null) Destroy(sr.gameObject);
        }
        spritePool.Clear();

        for (int i = 0; i < maxTrajectorySprites; i++)
        {
            GameObject dot = new GameObject($"TrajectoryDot_{i}");
            dot.transform.SetParent(this.transform); 
            dot.transform.localScale = startScale;
            
            SpriteRenderer sr = dot.AddComponent<SpriteRenderer>();
            sr.sprite = trajectorySprite;
            
            sr.sortingLayerName = sortingLayerName;
            sr.sortingOrder = sortingOrder;
            
            dot.SetActive(false);
            spritePool.Add(sr);
        }
    }

    private void SetTrajectoryVisible(bool visible)
    {
        if (!visible)
        {
            foreach (var sr in spritePool)
            {
                if (sr != null) sr.gameObject.SetActive(false);
            }
            currentAllowedVisibleDots = 0f; 
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

        if (playerAnimator != null)
        {
            playerAnimator.ResetVisuals();
        }

        if (hasLockedShot)
        {
            isAiming = false;
            CalculateAndDrawCurvedTrajectory(launchVelocity);
        }
        else
        {
            isAiming = false;
            SetTrajectoryVisible(false);
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
        bool plannerIsBusy = (planner != null && planner.IsDragging);

        if (!plannerIsBusy)
        {
            HandleInput();
        }

        if (isAiming || hasLockedShot)
        {
            currentAllowedVisibleDots += revealSpeed * Time.deltaTime;
            currentAllowedVisibleDots = Mathf.Min(currentAllowedVisibleDots, maxTrajectorySprites);
        }

        if (isAiming && !plannerIsBusy)
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
        currentAllowedVisibleDots = 0f; 
        
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        aimStartWorld = rb.position;
        aimCurrentWorld = new Vector2(mouseWorld.x, mouseWorld.y);
        
        UpdateAim(); 
    }

    private void ModifyExistingAim()
    {
        isAiming = true;
        hasLockedShot = false;

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
        Vector3 mouseWorld = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 currentMousePos = new Vector2(mouseWorld.x, mouseWorld.y);
        float mouseDistanceFromPlayer = Vector2.Distance(currentMousePos, rb.position);
        bool plannerIsBusy = (planner != null && planner.IsDragging);
        if (mouseDistanceFromPlayer < minSpawnDistanceRadius && !plannerIsBusy)
        {
            SetTrajectoryVisible(false);
            return;
        }

        Vector2 currentSimPosition = rb.position;
        Vector2 currentSimVelocity = startVelocity;

        GravityBody bodyComponent = GetComponent<GravityBody>();
        float minEscapeSpeed = bodyComponent != null ? bodyComponent.MinEscapeSpeed : 1.2f;

        int renderedSpriteCount = 0;

        for (int step = 0; step < simulationSteps; step++)
        {
            if (renderedSpriteCount >= maxTrajectorySprites)
                break;

            Vector2 combinedGravityForce = Vector2.zero;

            if (GravitySource.ActiveSources != null)
            {
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
            }

            currentSimVelocity += combinedGravityForce * stepTimeDelta;

            if (currentSimVelocity.sqrMagnitude > maxVelocityMagnitude * maxVelocityMagnitude)
            {
                currentSimVelocity = currentSimVelocity.normalized * maxVelocityMagnitude;
            }

            currentSimPosition += currentSimVelocity * stepTimeDelta;

            if (step % stepInterval == 0)
            {
                float distanceFromPlayer = Vector2.Distance(currentSimPosition, rb.position);
                bool isValidPosition = !float.IsNaN(currentSimPosition.x) && !float.IsNaN(currentSimPosition.y);

                if (isValidPosition && distanceFromPlayer >= minSpawnDistanceRadius)
                {
                    if (renderedSpriteCount < currentAllowedVisibleDots)
                    {
                        SpriteRenderer sr = spritePool[renderedSpriteCount];
                        sr.transform.position = currentSimPosition;
                        
                        float progress = (float)renderedSpriteCount / maxTrajectorySprites;
                        sr.transform.localScale = Vector3.Lerp(startScale, minScale, progress);
                        
                        float alpha = Mathf.Lerp(1f, minAlpha, progress);
                        sr.color = new Color(sr.color.r, sr.color.g, sr.color.b, alpha);

                        sr.gameObject.SetActive(true);
                    }
                    
                    renderedSpriteCount++; 
                }
            }
        }

        for (int i = renderedSpriteCount; i < maxTrajectorySprites; i++)
        {
            if (i >= 0 && i < spritePool.Count && spritePool[i] != null)
                spritePool[i].gameObject.SetActive(false);
        }
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
        CaptureSnapshot();
    }

    private void EnterRunning()
    {
        isAiming = false;
        SetTrajectoryVisible(false);
        hasLaunchedThisRun = false;
    }

    private void ForceRunningRules()
    {
        SetTrajectoryVisible(false);

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
