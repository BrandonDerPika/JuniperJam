using UnityEngine;

public class SFXManager : MonoBehaviour
{
    public static SFXManager Instance;

    [Header("Once")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Loop")]
    [SerializeField] private AudioSource loopSource;

    [Header("Sound Effects")]
    [SerializeField] private AudioClip targetHitSFX;
    [SerializeField] private AudioClip checkpointSFX;
    [SerializeField] private AudioClip playerDeathSFX;
    [SerializeField] private AudioClip dragSFX;
    [SerializeField] private AudioClip placePlanetSFX;
    [SerializeField] private AudioClip gravityFieldEnterSFX;
    [SerializeField] private AudioClip buttonClick1SFX;
    [SerializeField] private AudioClip buttonClick2SFX;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

// once
    public void PlayTargetHit()
    {
        if (targetHitSFX != null)
            sfxSource.PlayOneShot(targetHitSFX);
    }

    public void PlayCheckpoint()
    {
        if (checkpointSFX != null)
            sfxSource.PlayOneShot(checkpointSFX);
    }

    public void PlayPlayerDeath()
    {
        if (playerDeathSFX != null)
            sfxSource.PlayOneShot(playerDeathSFX);
    }

    public void PlayPlacePlanet()
    {
        if (placePlanetSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(placePlanetSFX);
    }

    public void PlayGravityFieldEnter()
    {
        if (gravityFieldEnterSFX != null && sfxSource != null)
            sfxSource.PlayOneShot(gravityFieldEnterSFX);
    }
    public void PlayButtonClick1()
    {
        if (buttonClick1SFX != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClick1SFX);
    }
    public void PlayButtonClick2()
    {
        if (buttonClick2SFX != null && sfxSource != null)
            sfxSource.PlayOneShot(buttonClick2SFX);
    }

// looped
    public void StartDrag()
    {
        if (dragSFX == null || loopSource == null) return;

        loopSource.clip = dragSFX;
        loopSource.loop = true;
        loopSource.Play();
    }
    public void StopDrag()
    {
        if (loopSource == null) return;

        loopSource.Stop();
        loopSource.clip = null;
    }
}
