using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LoadingScreenUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private Image backgroundImage;
    [SerializeField] private GameObject jarvisAnimationObject;
    
    [Header("Visual Settings")]
    [SerializeField] private Color ironManBlue = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Color ironManOrange = new Color(1f, 0.5f, 0f);
    [SerializeField] private Sprite[] backgroundImages;
    
    [Header("Animation")]
    [SerializeField] private float pulsateSpeed = 1.5f;
    [SerializeField] private float pulsateAmount = 0.2f;
    [SerializeField] private float circleRotationSpeed = 30f;
    [SerializeField] private RectTransform[] rotatingElements;
    [SerializeField] private Image[] glowElements;
    
    [Header("J.A.R.V.I.S. Voice")]
    [SerializeField] private AudioClip[] jarvisClips;
    [SerializeField] private AudioSource voiceSource;
    
    // References to LoadingScreen controller
    private LoadingScreen loadingScreen;
    
    // Animation state
    private float animationTime;
    
    private void Awake()
    {
        loadingScreen = GetComponent<LoadingScreen>();
        
        // Hide canvas initially
        if (canvasGroup != null)
            canvasGroup.alpha = 0f;
            
        // Set initial colors
        UpdateColors();
    }
    
    private void OnEnable()
    {
        // Subscribe to loading events if there are any
    }
    
    private void OnDisable()
    {
        // Unsubscribe from loading events
    }
    
    private void Update()
    {
        // Animate elements
        animationTime += Time.deltaTime;
        AnimateElements();
    }
    
    // Show the loading screen with a fade effect
    public void Show(string sceneName)
    {
        // Select random background
        if (backgroundImages != null && backgroundImages.Length > 0 && backgroundImage != null)
        {
            backgroundImage.sprite = backgroundImages[Random.Range(0, backgroundImages.Length)];
        }
        
        // Set title
        if (titleText != null)
        {
            titleText.text = "Loading " + sceneName;
        }
        
        // Play JARVIS voice
        PlayRandomJarvisClip();
        
        // Start fade in
        StartCoroutine(FadeIn());
    }
    
    // Hide the loading screen with a fade effect
    public void Hide()
    {
        StartCoroutine(FadeOut());
    }
    
    // Update progress display
    public void UpdateProgress(float progress)
    {
        if (progressBar != null)
            progressBar.value = progress;
            
        if (progressText != null)
            progressText.text = Mathf.FloorToInt(progress * 100) + "%";
    }
    
    // Set the tip text
    public void SetTip(string tip)
    {
        if (tipText != null)
            tipText.text = tip;
    }
    
    // Play a random JARVIS voice clip
    private void PlayRandomJarvisClip()
    {
        if (voiceSource != null && jarvisClips != null && jarvisClips.Length > 0)
        {
            voiceSource.clip = jarvisClips[Random.Range(0, jarvisClips.Length)];
            voiceSource.Play();
        }
    }
    
    // Animate UI elements for Iron Man tech feel
    private void AnimateElements()
    {
        // Rotate circular elements
        if (rotatingElements != null)
        {
            foreach (RectTransform element in rotatingElements)
            {
                if (element != null)
                    element.Rotate(0, 0, circleRotationSpeed * Time.deltaTime);
            }
        }
        
        // Pulsate glow elements
        if (glowElements != null)
        {
            float pulse = 1f + pulsateAmount * Mathf.Sin(animationTime * pulsateSpeed);
            
            foreach (Image element in glowElements)
            {
                if (element != null)
                {
                    Color color = element.color;
                    color.a = pulse * 0.7f; // Base alpha is 0.7
                    element.color = color;
                }
            }
        }
        
        // Animate JARVIS visualization if present
        if (jarvisAnimationObject != null)
        {
            // Custom JARVIS audio visualization 
            // (This would be expanded in a real implementation)
            float audioLevel = voiceSource != null && voiceSource.isPlaying ? 
                Random.Range(0.2f, 1f) : 0.2f;
                
            jarvisAnimationObject.transform.localScale = Vector3.one * (1f + audioLevel * 0.3f);
        }
    }
    
    // Update colors on UI elements for consistent theme
    private void UpdateColors()
    {
        // Apply colors to various elements that should match the Iron Man theme
        if (progressBar != null)
        {
            Image fillImage = progressBar.fillRect.GetComponent<Image>();
            if (fillImage != null)
                fillImage.color = ironManBlue;
        }
        
        if (progressText != null)
            progressText.color = ironManBlue;
            
        if (titleText != null)
            titleText.color = ironManOrange;
    }
    
    // Fade in the loading screen
    private IEnumerator FadeIn()
    {
        float time = 0;
        float duration = 0.5f;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(0, 1, time / duration);
                yield return null;
            }
            
            canvasGroup.alpha = 1;
        }
    }
    
    // Fade out the loading screen
    private IEnumerator FadeOut()
    {
        float time = 0;
        float duration = 0.5f;
        
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1;
            
            while (time < duration)
            {
                time += Time.deltaTime;
                canvasGroup.alpha = Mathf.Lerp(1, 0, time / duration);
                yield return null;
            }
            
            canvasGroup.alpha = 0;
        }
    }
}
