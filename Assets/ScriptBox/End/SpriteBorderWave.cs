using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class SpriteBorderWave : MonoBehaviour
{
    [System.Serializable]
    public struct UniqueHitTarget
    {
        public string objectIdentifier;
        public Sprite uniqueHitSprite;
        public string animatorTriggerName;
    }

    [Header("Wave Settings")]
    public float expansionSpeed = 8f;
    public float maxRadius = 12f;
    public Color waveColor = Color.white;

    [Header("On Hit Effects")]
    public string ignoreTag = "IgnoreWave";
    public List<UniqueHitTarget> uniqueHitTargets = new List<UniqueHitTarget>();
    public string defaultAnimatorTrigger = "Explode";
    public Color fallbackDarkenColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    [Header("Explosion Visuals")]
    [SerializeField] private GameObject explosionPrefab;
    [SerializeField] private float explosionScaleMultiplier = 1f;

    private SpriteRenderer spriteRenderer;
    private float currentRadius = 0f;
    private bool isRunning = false;
    
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private List<SpriteRenderer> targetRenderers = new List<SpriteRenderer>();

    // Used to calculate how the local scale maps to the actual world radius
    private float baseSpriteRadius = 0.5f; 

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void InitializeCustom(float speed, float radius, Color col, 
    List<UniqueHitTarget> customTargets, string defaultTrigger, Color darkenColor, string customIgnoreTag)
    {
        expansionSpeed = speed;
        maxRadius = radius;
        waveColor = col;
        uniqueHitTargets = new List<UniqueHitTarget>(customTargets);
        defaultAnimatorTrigger = defaultTrigger;
        fallbackDarkenColor = darkenColor;
        ignoreTag = customIgnoreTag;

        // Fetch global settings from LevelManager if available
        SpriteBorderWave settingsSource = LevelManager.Instance != null ? LevelManager.Instance.GetComponent<SpriteBorderWave>() : null;
        if (settingsSource != null)
        {
            explosionPrefab = settingsSource.explosionPrefab;
            explosionScaleMultiplier = settingsSource.explosionScaleMultiplier;
        }

        spriteRenderer.color = waveColor;
        
        // Calculate the unscaled, base radius of the sprite (assumes a circular sprite)
        if (spriteRenderer.sprite != null)
        {
            baseSpriteRadius = spriteRenderer.sprite.bounds.extents.x;
        }

        // Reset scale to 0 at the start
        transform.localScale = Vector3.zero;

        targetRenderers.AddRange(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None));
        
        currentRadius = 0f;
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        // 1. Expand the radius over time
        currentRadius += expansionSpeed * Time.deltaTime;

        // 2. Scale the sprite transform to visually match the calculated radius
        if (baseSpriteRadius > 0)
        {
            float localScaleValue = currentRadius / baseSpriteRadius;
            transform.localScale = new Vector3(localScaleValue, localScaleValue, 1f);
        }

        // 3. Process game logic collisions
        CheckForWaveImpacts();

        // 4. Fade out the sprite color as it approaches max radius
        float alpha = Mathf.Clamp01(1f - (currentRadius / maxRadius));
        spriteRenderer.color = new Color(waveColor.r, waveColor.g, waveColor.b, alpha);

        // 5. Lifespan check
        if (currentRadius >= maxRadius)
        {
            Destroy(gameObject);
        }
    }

    private void CheckForWaveImpacts()
    {
        for (int i = targetRenderers.Count - 1; i >= 0; i--)
        {
            SpriteRenderer renderer = targetRenderers[i];
            
            if (renderer == null || !renderer.gameObject.activeInHierarchy) continue;
            if (hitObjects.Contains(renderer.gameObject)) continue;
            if (renderer.gameObject == gameObject || renderer.transform == transform.parent) continue;
            if (!string.IsNullOrEmpty(ignoreTag) && renderer.gameObject.CompareTag(ignoreTag)) continue;

            float distance = Vector2.Distance(transform.position, renderer.transform.position);

            if (currentRadius >= distance)
            {
                hitObjects.Add(renderer.gameObject);
                TriggerObjectImpact(renderer);
            }
        }
    }

    private void TriggerObjectImpact(SpriteRenderer targetRenderer)
    {
        GameObject targetGo = targetRenderer.gameObject;
        string objectName = targetGo.name;
        bool foundUniqueMatch = false;
        string triggerToPlay = null;
        Sprite spriteToSwap = null;

        if (SFXManager.Instance != null)
        {
            SFXManager.Instance.PlayRandomExplosion();
        }

        if (explosionPrefab != null)
        {
            GameObject explosionInstance = Instantiate(explosionPrefab, targetGo.transform.position, Quaternion.identity);
            explosionInstance.transform.localScale = targetGo.transform.localScale * explosionScaleMultiplier;
            StartCoroutine(DestroyAfterAnimation(explosionInstance));
        }

        foreach (UniqueHitTarget targetRule in uniqueHitTargets)
        {
            if (!string.IsNullOrEmpty(targetRule.objectIdentifier) && objectName.Contains(targetRule.objectIdentifier))
            {
                triggerToPlay = !string.IsNullOrEmpty(targetRule.animatorTriggerName) ? targetRule.animatorTriggerName : defaultAnimatorTrigger;
                spriteToSwap = targetRule.uniqueHitSprite;
                foundUniqueMatch = true;
                break;
            }
        }

        Animator animator = targetGo.GetComponent<Animator>();
        if (animator == null)
        {
            animator = targetGo.GetComponentInParent<Animator>();
        }

        if (animator != null)
        {
            string finalTrigger = !string.IsNullOrEmpty(triggerToPlay) ? triggerToPlay : defaultAnimatorTrigger;
            animator.SetTrigger(finalTrigger);
        }
        else
        {
            if (foundUniqueMatch && spriteToSwap != null)
            {
                targetRenderer.sprite = spriteToSwap;
            }
            else if (!foundUniqueMatch)
            {
                targetRenderer.color = fallbackDarkenColor;
            }
        }
    }

    private IEnumerator DestroyAfterAnimation(GameObject clone)
    {
        if (clone == null) yield break;

        Animator animator = clone.GetComponent<Animator>();
        if (animator == null) animator = clone.GetComponentInChildren<Animator>();

        if (animator != null && animator.runtimeAnimatorController != null)
        {
            yield return null;
            AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
            yield return new WaitForSeconds(stateInfo.length);
        }

        if (clone != null)
        {
            Destroy(clone);
        }
    }
}