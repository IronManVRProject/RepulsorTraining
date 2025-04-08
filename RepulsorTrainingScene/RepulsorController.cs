using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using System;

public class RepulsorController : MonoBehaviour
{
    [Header("Repulsor Settings")]
    [SerializeField] private GameObject repulsorVisual;
    [SerializeField] private Transform repulsorEmissionPoint;
    [SerializeField] private GameObject repulsorBeamPrefab;
    [SerializeField] private float repulsorPower = 10f;
    [SerializeField] private float maxBeamDistance = 20f;
    [SerializeField] private Color repulsorColor = new Color(0.3f, 0.8f, 1f, 0.8f);
    
    [Header("VR Controller Input")]
    [SerializeField] private InputActionReference activateRepulsorAction;
    
    [Header("Effects")]
    [SerializeField] private ParticleSystem chargeEffect;
    [SerializeField] private AudioSource repulsorSound;
    
    // Events for other components to hook into
    public event Action<GameObject> OnRepulsorActivated;
    public event Action OnRepulsorDeactivated;
    
    // Private variables
    private bool isRepulsorActive = false;
    private LineRenderer beamRenderer;
    private GameObject currentBeam;
    private RaycastHit lastHit;
    private bool didHitLastFrame = false;
    
    private void Awake()
    {
        // Initialize if needed
        if (repulsorEmissionPoint == null)
            repulsorEmissionPoint = transform;
    }
    
    private void OnEnable()
{
    if (activateRepulsorAction != null)
    {
        activateRepulsorAction.action.performed += OnRepulsorPressed;
        activateRepulsorAction.action.canceled += OnRepulsorReleased;
        activateRepulsorAction.action.Enable();
    }
}

    private void OnDisable()
{
    if (activateRepulsorAction != null)
    {
        activateRepulsorAction.action.performed -= OnRepulsorPressed;
        activateRepulsorAction.action.canceled -= OnRepulsorReleased;
        activateRepulsorAction.action.Disable();
    }
}

    
    private void OnRepulsorPressed(InputAction.CallbackContext context)
{
    if (!isRepulsorActive)
        ActivateRepulsor();
}

    private void OnRepulsorReleased(InputAction.CallbackContext context)
{
    if (isRepulsorActive)
        DeactivateRepulsor();
}

    
    private void ActivateRepulsor()
    {
        isRepulsorActive = true;
        
        // Activate visual effects
        if (repulsorVisual != null)
            repulsorVisual.SetActive(true);
            
        // Play particle effect
        if (chargeEffect != null)
            chargeEffect.Play();
            
        // Play sound effect
        if (repulsorSound != null)
            repulsorSound.Play();
            
        // Create beam
        CreateRepulsorBeam();
        
        // Add haptic feedback if available
        SendHapticFeedback();
        
        // Notify subscribers
        OnRepulsorActivated?.Invoke(currentBeam);

        Debug.Log("Repulsor Activated");

    }
    
    private void DeactivateRepulsor()
    {
        isRepulsorActive = false;
        
        // Deactivate visual effects
        if (repulsorVisual != null)
            repulsorVisual.SetActive(false);
            
        // Stop particle effect
        if (chargeEffect != null)
            chargeEffect.Stop();
            
        // Stop sound effect
        if (repulsorSound != null)
            repulsorSound.Stop();
            
        // Destroy beam
        if (currentBeam != null)
            Destroy(currentBeam);
            
        // Notify subscribers
        OnRepulsorDeactivated?.Invoke();

        Debug.Log("Repulsor Deactivated");
    }
    
    private void SendHapticFeedback()
    {
        // Simple stub for haptic feedback
        // For now, we'll just log that feedback would happen
        // You can implement device-specific haptics later when you have the proper setup
        Debug.Log("Repulsor activated - haptic feedback would trigger here");
        
        // In a full implementation, you would use the XR device's haptic capabilities
        // This varies by Unity version and XR plugin
    }
    
    private void CreateRepulsorBeam()
    {
        // If a beam prefab is provided, instantiate it
        if (repulsorBeamPrefab != null)
        {
            currentBeam = Instantiate(repulsorBeamPrefab, repulsorEmissionPoint.position, repulsorEmissionPoint.rotation);
            currentBeam.transform.parent = repulsorEmissionPoint;
            
            // If the beam has a LineRenderer, configure it
            beamRenderer = currentBeam.GetComponent<LineRenderer>();
            if (beamRenderer != null)
            {
                beamRenderer.startColor = repulsorColor;
                beamRenderer.endColor = new Color(repulsorColor.r, repulsorColor.g, repulsorColor.b, 0.2f);
            }
        }
        else
        {
            // Create a basic line renderer if no prefab is available
            currentBeam = new GameObject("RepulsorBeam");
            currentBeam.transform.parent = repulsorEmissionPoint;
            currentBeam.transform.localPosition = Vector3.zero;
            
            beamRenderer = currentBeam.AddComponent<LineRenderer>();
            beamRenderer.positionCount = 2;
            beamRenderer.startWidth = 0.05f;
            beamRenderer.endWidth = 0.01f;
            beamRenderer.material = new Material(Shader.Find("Universal Render Pipeline/Particles/Unlit"));
            beamRenderer.startColor = repulsorColor;
            beamRenderer.endColor = new Color(repulsorColor.r, repulsorColor.g, repulsorColor.b, 0.2f);
        }
        
        // Start updating the beam positions
        StartCoroutine(UpdateBeam());
    }
    
    private IEnumerator UpdateBeam()
    {
        while (isRepulsorActive && beamRenderer != null)
        {
            UpdateBeamPositions();
            yield return null;
        }
    }
    
    private void UpdateBeamPositions()
    {
        RaycastHit hit;
        Vector3 endPosition;
        
        // Cast a ray to determine where the beam hits
        bool didHit = Physics.Raycast(repulsorEmissionPoint.position, repulsorEmissionPoint.forward, out hit, maxBeamDistance);
        
        if (didHit)
        {
            endPosition = hit.point;
            lastHit = hit;
            didHitLastFrame = true;
            UpdateLastHit(hit);
            // Process the hit
            ProcessHit(hit);
        }
        else
        {
            // If nothing is hit, the beam extends to the maximum distance
            endPosition = repulsorEmissionPoint.position + (repulsorEmissionPoint.forward * maxBeamDistance);
            didHitLastFrame = false;
        }
        
        // Update beam positions
        if (beamRenderer != null)
        {
            beamRenderer.SetPosition(0, repulsorEmissionPoint.position);
            beamRenderer.SetPosition(1, endPosition);
        }
    }
    
    private void ProcessHit(RaycastHit hit)
    {
        // Apply force to rigidbodies
        if (hit.rigidbody != null)
        {
            hit.rigidbody.AddForceAtPosition(
                repulsorEmissionPoint.forward * repulsorPower,
                hit.point,
                ForceMode.Impulse
            );
        }
        
        // For targets, call a method they can implement
        IRepulsorTarget target = hit.collider.GetComponent<IRepulsorTarget>();
        if (target != null)
        {
            target.OnHitByRepulsor(repulsorPower, hit.point, repulsorEmissionPoint.forward);
        }
    }
    
    // New methods to support different firing modes
    public void SetRepulsorPower(float power)
    {
        repulsorPower = power;
    }
    
    public void SetRepulsorColor(Color color)
    {
        repulsorColor = color;
        
        // Update beam color if it exists
        if (beamRenderer != null)
        {
            beamRenderer.startColor = color;
            beamRenderer.endColor = new Color(color.r, color.g, color.b, 0.2f);
        }
        
        // Update material if possible
        if (repulsorVisual != null)
        {
            Renderer renderer = repulsorVisual.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                renderer.material.SetColor("_EmissionColor", color * 2f);
            }
        }
    }
    
    // Store the most recent hit result
        private RaycastHit lastHitInfo;
        private bool hasValidHit = false;

     // Called internally when a beam hits something. Stores hit info.
        private void UpdateLastHit(RaycastHit hit)
        {
            lastHitInfo = hit;
            hasValidHit = true;
        }
        // Public method to get the last beam hit, if available.
        public bool TryGetLastHit(out RaycastHit hit)
        {
            hit = lastHitInfo;
            return hasValidHit;
        }
    
    // Method to manually trigger a beam hit (for burst mode)
    public void TriggerBeamHit()
    {
        if (didHitLastFrame)
        {
            ProcessHit(lastHit);
        }
    }
    
    // Get the current beam GameObject
    public GameObject GetCurrentBeam()
    {
        return currentBeam;
    }
}