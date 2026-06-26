using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class BorderWave : MonoBehaviour
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
    public float thickness = 0.25f;
    public int segments = 60;
    public Color waveColor = Color.white;

    [Header("On Hit Effects")]
    public List<UniqueHitTarget> uniqueHitTargets = new List<UniqueHitTarget>();
    public string defaultAnimatorTrigger = "Explode";
    public Color fallbackDarkenColor = new Color(0.2f, 0.2f, 0.2f, 1f);

    private LineRenderer lineRenderer;
    private float currentRadius = 0f;
    private bool isRunning = false;
    
    private HashSet<GameObject> hitObjects = new HashSet<GameObject>();
    private List<SpriteRenderer> targetRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = 0; 
    }

    public void InitializeCustom(float speed, float radius, float thick, int segs, Color col, 
                                 List<UniqueHitTarget> customTargets, string defaultTrigger, Color darkenColor)
    {
        expansionSpeed = speed;
        maxRadius = radius;
        thickness = thick;
        segments = segs;
        waveColor = col;
        uniqueHitTargets = new List<UniqueHitTarget>(customTargets);
        defaultAnimatorTrigger = defaultTrigger;
        fallbackDarkenColor = darkenColor;

        lineRenderer.positionCount = segments + 1;
        lineRenderer.useWorldSpace = false;
        lineRenderer.startWidth = thickness;
        lineRenderer.endWidth = thickness;
        
        Shader spriteShader = Shader.Find("Sprites/Default");
        if (spriteShader != null)
        {
            lineRenderer.material = new Material(spriteShader);
        }
        
        lineRenderer.startColor = waveColor;
        lineRenderer.endColor = waveColor;
        
        targetRenderers.AddRange(Object.FindObjectsByType<SpriteRenderer>(FindObjectsSortMode.None));
        
        currentRadius = 0f;
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        currentRadius += expansionSpeed * Time.deltaTime;
        DrawCircle(currentRadius);

        CheckForWaveImpacts();

        float alpha = Mathf.Clamp01(1f - (currentRadius / maxRadius));
        Color fadedColor = new Color(waveColor.r, waveColor.g, waveColor.b, alpha);
        lineRenderer.startColor = fadedColor;
        lineRenderer.endColor = fadedColor;

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

    private void DrawCircle(float radius)
    {
        float angle = 0f;
        for (int i = 0; i <= segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float y = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;

            lineRenderer.SetPosition(i, new Vector3(x, y, 0f));
            angle += (360f / segments);
        }
    }
}
