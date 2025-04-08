using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class RepulsorTarget : MonoBehaviour, IRepulsorTarget
{
    [Header("Target Settings")]
    [SerializeField] private int scoreValue = 10;
    [SerializeField] private float healthPoints = 100f;
    [SerializeField] private bool isDestructible = true;
    
    [Header("Visual Effects")]
    [SerializeField] private GameObject hitEffectPrefab;
    [SerializeField] private GameObject destructionEffectPrefab;
    [SerializeField] private Material hitMaterial;
    [SerializeField] private float hitFlashDuration = 0.2f;
    
    [Header("Audio")]
    [SerializeField] private AudioClip hitSound;
    [SerializeField] private AudioClip destructionSound;
    
    // Private variables
    private Material originalMaterial;
    private Renderer targetRenderer;
    private AudioSource audioSource;
    private bool isDestroyed = false;
    private Transform playerTransform;
    
    // Events
    public delegate void TargetHitEvent(int scoreValue);
    public static event TargetHitEvent OnTargetHit;
    
    public delegate void TargetDestroyedEvent(int scoreValue);
    public static event TargetDestroyedEvent OnTargetDestroyed;
    
    private void Awake()
    {
        targetRenderer = GetComponent<Renderer>();
        if (targetRenderer != null)
            originalMaterial = targetRenderer.material;
            
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null && (hitSound != null || destructionSound != null))
            audioSource = gameObject.AddComponent<AudioSource>();
    }

    private void Start()
{
    GameObject player = GameObject.Find("XR Origin (XR Rig)"); // This matches your exact object name
    if (player != null)
    {
        playerTransform = player.transform;
        FacePlayer();
    }
}

    private void FacePlayer()
{
    if (playerTransform == null) return;

    Vector3 lookDirection = playerTransform.position - transform.position;
    lookDirection.y = 0f; // Prevent up/down tilt

    if (lookDirection != Vector3.zero)
        transform.rotation = Quaternion.LookRotation(lookDirection);
}


    
    public void OnHitByRepulsor(float power, Vector3 hitPoint, Vector3 direction)
    {
        if (isDestroyed)
            return;
            
        // Apply damage based on repulsor power
        if (isDestructible)
            healthPoints -= power;
            
        // Visual feedback
        StartCoroutine(FlashHitEffect());
        
        // Spawn hit effect at the hit point
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                hitEffectPrefab, 
                hitPoint, 
                Quaternion.LookRotation(direction * -1f)
            );
            Destroy(effect, 2f); // Clean up effect after 2 seconds
        }
        
        // Play hit sound
        if (audioSource != null && hitSound != null)
        {
            audioSource.clip = hitSound;
            audioSource.Play();
        }
        
        // Notify of hit for scoring
        if (OnTargetHit != null)
            OnTargetHit(scoreValue);
            
        // Check if target is destroyed
        if (isDestructible && healthPoints <= 0 && !isDestroyed)
        {
            DestroyTarget();
        }
    }
    
    private void DestroyTarget()
    {
        isDestroyed = true;
        
        // Spawn destruction effect
        if (destructionEffectPrefab != null)
        {
            GameObject effect = Instantiate(
                destructionEffectPrefab, 
                transform.position, 
                transform.rotation
            );
            Destroy(effect, 3f); // Clean up effect after 3 seconds
        }
        
        // Play destruction sound
        if (audioSource != null && destructionSound != null)
        {
            audioSource.clip = destructionSound;
            audioSource.Play();
        }
        
        // Notify of destruction for scoring
        if (OnTargetDestroyed != null)
            OnTargetDestroyed(scoreValue * 2); // Double points for destruction
            
        // Disable the renderer to make it invisible
        if (targetRenderer != null)
            targetRenderer.enabled = false;
            
        // Disable any colliders
        Collider[] colliders = GetComponents<Collider>();
        foreach (Collider col in colliders)
            col.enabled = false;
            
        // Destroy the GameObject after the sound plays (if any)
        float destroyDelay = (audioSource != null && destructionSound != null) ? 
            destructionSound.length : 0.5f;
        StartCoroutine(DestroyAfterDelay(destroyDelay));

    }
    
    private IEnumerator FlashHitEffect()
    {
        if (targetRenderer != null && hitMaterial != null)
        {
            // Change to hit material
            targetRenderer.material = hitMaterial;
            
            // Wait for flash duration
            yield return new WaitForSeconds(hitFlashDuration);
            
            // Change back to original material
            targetRenderer.material = originalMaterial;
        }
    }

    private IEnumerator DestroyAfterDelay(float delay)
{
    yield return new WaitForSeconds(delay);

    // After delay, disable visuals and colliders
    if (targetRenderer != null)
        targetRenderer.enabled = false;

    Collider[] colliders = GetComponents<Collider>();
    foreach (Collider col in colliders)
        col.enabled = false;

    Destroy(gameObject);
}

}
