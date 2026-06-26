using System.Collections;
using UnityEngine;

public class Target : MonoBehaviour
{
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private string explosionTriggerName = "Explode"; 

    [Header("Visual Customizations")]
    [SerializeField] private Animator targetAnimator;

    private bool hasExploded = false;

    private void Awake()
    {
        if (targetAnimator == null) targetAnimator = GetComponent<Animator>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag) && !hasExploded)
        {
            hasExploded = true; 
            PlayerAnimator playerAnim = other.GetComponent<PlayerAnimator>();
            if (playerAnim != null)
            {
                playerAnim.TriggerTargetHitVisual();
            }
            if (SFXManager.Instance != null)
                SFXManager.Instance.PlayTargetHit();
            StartCoroutine(ExplosionSequence());
        }
    }

    private IEnumerator ExplosionSequence()
    {
        if (targetAnimator != null)
        {
            targetAnimator.enabled = true;
            targetAnimator.SetTrigger(explosionTriggerName);
            yield return null; 
            float animationLength = targetAnimator.GetCurrentAnimatorStateInfo(0).length;
            yield return new WaitForSeconds(animationLength);
        }
        else
        {
            yield return null;
        }
        if (LevelManager.Instance != null)
        {
            LevelManager.Instance.CompleteCurrentLevel();
        }
    }
}
