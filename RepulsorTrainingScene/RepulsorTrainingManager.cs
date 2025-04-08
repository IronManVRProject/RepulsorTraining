using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class RepulsorTrainingManager : MonoBehaviour
{
    [Header("Training Setup")]
    [SerializeField] private GameObject targetPrefab;
    [SerializeField] private Transform[] spawnPoints;
    [SerializeField] private float trainingDuration = 60f; // in seconds
    [SerializeField] private float targetSpawnInterval = 3f;
    [SerializeField] private int maxTargetsAtOnce = 5;
    [SerializeField] private int minimumTargetsToSpawn = 10;
    
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI scoreText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private TextMeshProUGUI targetInfoText;
    [SerializeField] private GameObject trainingCompletePanel;
    [SerializeField] private TextMeshProUGUI finalScoreText;
    [SerializeField] private TextMeshProUGUI accuracyText;
    
    [Header("Audio")]
    [SerializeField] private AudioClip trainingStartSound;
    [SerializeField] private AudioClip trainingEndSound;
    [SerializeField] private AudioClip countdownSound;
    
    [Header("Events")]
    public UnityEvent onTrainingStart;
    public UnityEvent onTrainingComplete;
    
    // Private variables
    private int currentScore = 0;
    private float remainingTime;
    private bool isTrainingActive = false;
    private List<GameObject> activeTargets = new List<GameObject>();
    private int totalTargetsSpawned = 0;
    private int totalTargetsHit = 0;
    private AudioSource audioSource;
    
    private void Awake()
    {
        audioSource = GetComponent<AudioSource>();
        if (audioSource == null)
            audioSource = gameObject.AddComponent<AudioSource>();
            
        // Make sure UI elements are set up
        if (trainingCompletePanel != null)
            trainingCompletePanel.SetActive(false);
            
        UpdateScoreUI();
    }
    
    private void OnEnable()
    {
        // Subscribe to target events
        RepulsorTarget.OnTargetHit += HandleTargetHit;
        RepulsorTarget.OnTargetDestroyed += HandleTargetDestroyed;
    }
    
    private void OnDisable()
    {
        // Unsubscribe from target events
        RepulsorTarget.OnTargetHit -= HandleTargetHit;
        RepulsorTarget.OnTargetDestroyed -= HandleTargetDestroyed;
    }
    
    public void StartTraining()
    {
        if (isTrainingActive)
            return;
            
        // Reset training values
        currentScore = 0;
        remainingTime = trainingDuration;
        totalTargetsSpawned = 0;
        totalTargetsHit = 0;
        
        // Clear any existing targets
        ClearAllTargets();
        
        // Update UI
        UpdateScoreUI();
        UpdateTimerUI();
        
        // Play start sound
        if (audioSource != null && trainingStartSound != null)
        {
            audioSource.clip = trainingStartSound;
            audioSource.Play();
        }
        
        // Invoke start event
        onTrainingStart.Invoke();
        
        // Start spawning targets
        isTrainingActive = true;
        StartCoroutine(SpawnTargetsRoutine());
        StartCoroutine(TrainingTimerRoutine());
        
        // Show initial target info
        UpdateTargetInfoUI();
    }
    
    private IEnumerator TrainingTimerRoutine()
    {
        while (remainingTime > 0 && isTrainingActive)
        {
            // Update timer
            remainingTime -= Time.deltaTime;
            UpdateTimerUI();
            
            // Play countdown sound in final 10 seconds
            if (remainingTime <= 30f && remainingTime > 29.9f && countdownSound != null)
            {
                audioSource.clip = countdownSound;
                audioSource.Play();
            }
            
            yield return null;
        }
        
        // End training when time runs out
        if (isTrainingActive)
            CompleteTraining();
    }
    
    private IEnumerator SpawnTargetsRoutine()
    {
        while (isTrainingActive && (totalTargetsSpawned < minimumTargetsToSpawn || remainingTime > 0))
        {
            // Don't spawn if we already have the maximum number of targets
            if (activeTargets.Count < maxTargetsAtOnce)
            {
                SpawnTarget();
                totalTargetsSpawned++;
                UpdateTargetInfoUI();
            }
            
            yield return new WaitForSeconds(targetSpawnInterval);
        }
    }
    
    private void SpawnTarget()
    {
        if (targetPrefab == null || spawnPoints.Length == 0)
            return;
            
        // Choose a random spawn point
        Transform spawnPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];
        
        // Instantiate the target
        GameObject target = Instantiate(
            targetPrefab, 
            spawnPoint.position, 
            Quaternion.Euler(0, Random.Range(0, 360), 0)
        );
        
        // Add to active targets list
        activeTargets.Add(target);
    }
    
    private void HandleTargetHit(int scoreValue)
    {
        totalTargetsHit++;
        currentScore += scoreValue;
        UpdateScoreUI();
    }
    
    private void HandleTargetDestroyed(int scoreValue)
    {
        currentScore += scoreValue;
        UpdateScoreUI();
        
        // Keep track of active targets
        for (int i = activeTargets.Count - 1; i >= 0; i--)
        {
            if (activeTargets[i] == null)
                activeTargets.RemoveAt(i);
        }
        
        UpdateTargetInfoUI();
    }
    
    private void CompleteTraining()
    {
        isTrainingActive = false;
        
        // Stop all coroutines
        StopAllCoroutines();
        
        // Play end sound
        if (audioSource != null && trainingEndSound != null)
        {
            audioSource.clip = trainingEndSound;
            audioSource.Play();
        }
        
        // Update final UI
        if (trainingCompletePanel != null)
        {
            trainingCompletePanel.SetActive(true);
            
            if (finalScoreText != null)
                finalScoreText.text = "Final Score: " + currentScore;
                
            if (accuracyText != null)
            {
                float accuracy = totalTargetsSpawned > 0 ? 
                    (float)totalTargetsHit / totalTargetsSpawned * 100f : 0;
                accuracyText.text = "Accuracy: " + accuracy.ToString("F1") + "%";
            }
        }
        
        // Invoke complete event
        onTrainingComplete.Invoke();
    }
    
    private void ClearAllTargets()
    {
        foreach (GameObject target in activeTargets)
        {
            if (target != null)
                Destroy(target);
        }
        
        activeTargets.Clear();
    }
    
    private void UpdateScoreUI()
    {
        if (scoreText != null)
            scoreText.text = "Score: " + currentScore;
    }
    
    private void UpdateTimerUI()
    {
        if (timerText != null)
        {
            int minutes = Mathf.FloorToInt(remainingTime / 60f);
            int seconds = Mathf.FloorToInt(remainingTime % 60f);
            timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
        }
    }
    
    private void UpdateTargetInfoUI()
    {
        if (targetInfoText != null)
        {
            targetInfoText.text = "Targets: " + activeTargets.Count + "/" + maxTargetsAtOnce + 
                                 "\nTotal Spawned: " + totalTargetsSpawned;
        }
    }
    
    public void RestartTraining()
    {
        if (trainingCompletePanel != null)
            trainingCompletePanel.SetActive(false);
            
        StartTraining();
    }
    
    public void EndTraining()
    {
        if (isTrainingActive)
            CompleteTraining();
    }
}