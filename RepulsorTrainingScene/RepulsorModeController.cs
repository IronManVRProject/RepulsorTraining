// Enhanced RepulsorModeController.cs with impact particles and comments for all logic

using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.InputSystem;

public class RepulsorModeController : MonoBehaviour
{
    // Enumeration for all firing modes
    public enum RepulsorMode { Single, Burst, Continuous }

    [Header("Repulsor Modes")]
    [SerializeField] private RepulsorMode currentMode = RepulsorMode.Single;

    [Header("Mode Settings")]
    [SerializeField] private float singleShotPower = 10f; // Power for single shot
    [SerializeField] private float burstShotPower = 5f;    // Power per shot in burst mode
    [SerializeField] private int burstCount = 3;           // Number of shots in burst
    [SerializeField] private float burstDelay = 0.1f;      // Delay between burst shots
    [SerializeField] private float continuousShotPower = 3f;  // Power for continuous beam
    [SerializeField] private float continuousDrainRate = 0.05f; // Not currently used but ready for energy drain logic

    [Header("Visual Effects")]
    [SerializeField] private ParticleSystem modeChangeEffect; // Optional effect when mode is switched
    [SerializeField] private ParticleSystem impactEffectPrefab; // Particle effect when beam hits target
    [SerializeField] private Color singleModeColor = new Color(0.3f, 0.8f, 1f); // Blue
    [SerializeField] private Color burstModeColor = new Color(1f, 0.5f, 0f);    // Orange
    [SerializeField] private Color continuousModeColor = new Color(1f, 0f, 0f);  // Red

    [Header("Audio")]
    [SerializeField] private AudioClip modeChangeSound;      // Audio for switching modes
    [SerializeField] private AudioClip singleShotSound;      // Audio for single shot
    [SerializeField] private AudioClip burstShotSound;       // Audio for burst
    [SerializeField] private AudioClip continuousShotSound;  // Audio for continuous beam
    [SerializeField] private AudioSource audioSource;        // AudioSource to play above sounds

    [Header("Input")]
    [SerializeField] private InputActionReference cycleModesAction; // Input to cycle firing modes

    // References
    private RepulsorController repulsorController; // Access to main repulsor controller
    private LineRenderer beamRenderer;             // For modifying beam visuals
    private float originalBeamWidth;               // Cache beam width
    private Color originalBeamColor;               // Cache beam color

    // State management
    private bool firingContinuous = false;         // Track continuous fire status
    private Coroutine burstCoroutine;              // Track burst coroutine
    private Material repulsorMaterial;             // Emissive material for repulsor sphere

    private void Awake()
    {
        repulsorController = GetComponent<RepulsorController>();
        if (audioSource == null) audioSource = GetComponent<AudioSource>();
        Renderer sphereRenderer = transform.GetComponentInChildren<Renderer>();
        if (sphereRenderer != null) repulsorMaterial = sphereRenderer.material;
        UpdateRepulsorVisuals();
    }

    private void OnEnable()
    {
        if (cycleModesAction != null)
            cycleModesAction.action.performed += OnCycleModes;

        if (repulsorController != null)
        {
            repulsorController.OnRepulsorActivated += HandleRepulsorActivated;
            repulsorController.OnRepulsorDeactivated += HandleRepulsorDeactivated;
        }
    }

    private void OnDisable()
    {
        if (cycleModesAction != null)
            cycleModesAction.action.performed -= OnCycleModes;

        if (repulsorController != null)
        {
            repulsorController.OnRepulsorActivated -= HandleRepulsorActivated;
            repulsorController.OnRepulsorDeactivated -= HandleRepulsorDeactivated;
        }

        StopContinuousFiring();
    }

    private void OnCycleModes(InputAction.CallbackContext context)
    {
        CycleRepulsorMode();
    }

    public void CycleRepulsorMode()
    {
        StopContinuousFiring();
        if (burstCoroutine != null) StopCoroutine(burstCoroutine);

        currentMode = (RepulsorMode)(((int)currentMode + 1) % System.Enum.GetValues(typeof(RepulsorMode)).Length);
        UpdateRepulsorVisuals();

        if (modeChangeEffect != null) modeChangeEffect.Play();
        if (audioSource != null && modeChangeSound != null)
        {
            audioSource.clip = modeChangeSound;
            audioSource.Play();
        }
        Debug.Log($"Repulsor mode changed to: {currentMode}");
    }

    private void UpdateRepulsorVisuals()
    {
        if (repulsorMaterial == null) return;

        Color modeColor = currentMode switch
        {
            RepulsorMode.Single => singleModeColor,
            RepulsorMode.Burst => burstModeColor,
            RepulsorMode.Continuous => continuousModeColor,
            _ => singleModeColor
        };

        repulsorMaterial.SetColor("_EmissionColor", modeColor * 2f);
        if (repulsorController != null) repulsorController.SetRepulsorColor(modeColor);
    }

    private void HandleRepulsorActivated(GameObject beam)
    {
        if (beam == null) return;
        beamRenderer = beam.GetComponent<LineRenderer>();
        if (beamRenderer != null)
        {
            originalBeamWidth = beamRenderer.startWidth;
            originalBeamColor = beamRenderer.startColor;
        }

        switch (currentMode)
        {
            case RepulsorMode.Single:
                FireSingleShot();
                break;
            case RepulsorMode.Burst:
                if (burstCoroutine != null) StopCoroutine(burstCoroutine);
                burstCoroutine = StartCoroutine(FireBurst());
                break;
            case RepulsorMode.Continuous:
                StartContinuousFiring();
                break;
        }
    }

    private void HandleRepulsorDeactivated()
    {
        StopContinuousFiring();
        if (burstCoroutine != null) { StopCoroutine(burstCoroutine); burstCoroutine = null; }
    }

    private void FireSingleShot()
    {
        repulsorController.SetRepulsorPower(singleShotPower);
        if (audioSource != null && singleShotSound != null)
        {
            audioSource.clip = singleShotSound;
            audioSource.Play();
        }
        StartCoroutine(SingleShotPulse());
        TriggerBeamHitWithEffect();
    }

    private IEnumerator SingleShotPulse()
    {
        if (beamRenderer != null)
        {
            beamRenderer.enabled = true;
            yield return new WaitForSeconds(0.05f);
            beamRenderer.enabled = false;
        }
    }

    private IEnumerator FireBurst()
    {
        if (audioSource != null && burstShotSound != null)
        {
            audioSource.clip = burstShotSound;
            audioSource.Play();
        }

        repulsorController.SetRepulsorPower(burstShotPower);

        for (int i = 0; i < burstCount; i++)
        {
            if (beamRenderer != null)
            {
                beamRenderer.enabled = true;
                TriggerBeamHitWithEffect();
                yield return new WaitForSeconds(0.05f);
                beamRenderer.enabled = false;
            }
            yield return new WaitForSeconds(burstDelay);
        }
        burstCoroutine = null;
    }

    private void StartContinuousFiring()
    {
        if (firingContinuous) return;
        firingContinuous = true;
        repulsorController.SetRepulsorPower(continuousShotPower);

        if (audioSource != null && continuousShotSound != null)
        {
            audioSource.clip = continuousShotSound;
            audioSource.loop = true;
            audioSource.Play();
        }
        StartCoroutine(ContinuousBeamEffect());
    }

    private void StopContinuousFiring()
    {
        if (!firingContinuous) return;
        firingContinuous = false;

        if (audioSource != null)
        {
            audioSource.loop = false;
            audioSource.Stop();
        }

        if (beamRenderer != null)
        {
            beamRenderer.startWidth = originalBeamWidth;
            beamRenderer.endWidth = originalBeamWidth * 0.5f;
            beamRenderer.startColor = originalBeamColor;
            beamRenderer.endColor = new Color(originalBeamColor.r, originalBeamColor.g, originalBeamColor.b, 0.2f);
        }
    }

    private IEnumerator ContinuousBeamEffect()
    {
        float time = 0;
        while (firingContinuous && beamRenderer != null)
        {
            time += Time.deltaTime;
            float growth = Mathf.Clamp01(time / 2f);
            float pulseWidth = originalBeamWidth * (1 + 0.2f * Mathf.Sin(time * 15)) * (1 + growth);
            beamRenderer.startWidth = pulseWidth;
            beamRenderer.endWidth = pulseWidth * 0.3f;

            Color intensifiedColor = continuousModeColor * (1.5f + 0.5f * Mathf.Sin(time * 10));
            beamRenderer.startColor = intensifiedColor;
            beamRenderer.endColor = new Color(intensifiedColor.r, intensifiedColor.g, intensifiedColor.b, 0.3f);

            if (Time.frameCount % 3 == 0)
            {
                TriggerBeamHitWithEffect();
            }
            yield return null;
        }
    }

    private void TriggerBeamHitWithEffect()
    {
        // Custom method on RepulsorController to return hit info
        if (repulsorController.TryGetLastHit(out RaycastHit hit))
        {
            // Call the main hit logic
            repulsorController.TriggerBeamHit();

            // Spawn impact particles at hit point
            if (impactEffectPrefab != null)
            {
                ParticleSystem impact = Instantiate(impactEffectPrefab, hit.point, Quaternion.LookRotation(hit.normal));
                impact.Play();
                Destroy(impact.gameObject, 2f);
            }
        }
        else
        {
            // Fallback call in case no info is available
            repulsorController.TriggerBeamHit();
        }
    }
}

