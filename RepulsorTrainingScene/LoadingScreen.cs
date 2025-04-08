using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class LoadingScreen : MonoBehaviour
{
    [Header("Loading Screen UI")]
    [SerializeField] private GameObject loadingScreenPanel;
    [SerializeField] private Slider progressBar;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private TextMeshProUGUI tipText;
    
    [Header("Loading Tips")]
    [SerializeField] private string[] loadingTips;
    
    [Header("Iron Man Theme")]
    [SerializeField] private Color ironManBlue = new Color(0.3f, 0.8f, 1f);
    [SerializeField] private Image logoImage;
    [SerializeField] private AudioSource loadingSound;
    
    // Static instance to access from anywhere
    private static LoadingScreen _instance;
    public static LoadingScreen Instance { get { return _instance; } }
    
    private void Awake()
    {
        // Singleton pattern to ensure only one loading screen exists
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Hide panel initially
        loadingScreenPanel.SetActive(false);
    }
    
    // Call this method to load a scene with loading screen
    public void LoadScene(string sceneName)
    {
        StartCoroutine(LoadSceneAsync(sceneName));
    }
    
    private IEnumerator LoadSceneAsync(string sceneName)
    {
        // Show loading screen
        loadingScreenPanel.SetActive(true);
        
        // Select random tip
        if (loadingTips != null && loadingTips.Length > 0)
        {
            tipText.text = loadingTips[Random.Range(0, loadingTips.Length)];
        }
        
        // Play loading sound
        if (loadingSound != null)
        {
            loadingSound.Play();
        }
        
        // Start loading the scene
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        
        // Don't let the scene activate until we allow it
        asyncLoad.allowSceneActivation = false;
        
        float progress = 0f;
        
        // While the scene loads
        while (!asyncLoad.isDone)
        {
            // Progress is reported as a value between 0 and 0.9
            // The last 0.1 is for when the scene is activated
            progress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
            
            // Update UI
            progressBar.value = progress;
            progressText.text = $"{(progress * 100):0}%";
            
            // If loading is almost done
            if (asyncLoad.progress >= 0.9f)
            {
                // Wait a bit so users can read the tip
                yield return new WaitForSeconds(1.5f);
                
                // Allow activation
                asyncLoad.allowSceneActivation = true;
            }
            
            yield return null;
        }
        
        // Hide loading screen after scene is loaded
        loadingScreenPanel.SetActive(false);
    }
    
    // Helper method for starting a new training session
    public void StartTraining(string trainingScene)
    {
        LoadScene(trainingScene);
    }

    public void SimulateLoadingScreen()
{
    StartCoroutine(FakeLoad());
}

private IEnumerator FakeLoad()
{
    loadingScreenPanel.SetActive(true);

    float duration = 3f; // Total fake loading time
    float time = 0;

    while (time < duration)
    {
        time += Time.deltaTime;
        float progress = Mathf.Clamp01(time / duration);

        if (progressBar != null)
            progressBar.value = progress;

        if (progressText != null)
            progressText.text = $"{(progress * 100):0}%";

        yield return null;
    }

    yield return new WaitForSeconds(0.5f); // Small delay for UX

    loadingScreenPanel.SetActive(false); // Hide the screen
}

// This will run the simulated loading automatically when you press Play.
void Start()
{
    SimulateLoadingScreen();
}


}