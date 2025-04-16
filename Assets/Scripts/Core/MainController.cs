using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;
using BiomorphicSim.Morphology;
using BiomorphicSim.UI;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Main controller for the Biomorphic Structure Simulator.
    /// Acts as the central orchestrator for all simulation systems.
    /// </summary>
    public class MainController : MonoBehaviour
    {    // References to other managers
    [SerializeField] private SiteGenerator siteGenerator;
    [SerializeField] private MorphologyManager morphologyManager;
    [SerializeField] private ScenarioAnalyzer scenarioAnalyzer;
    [SerializeField] private ModernUIController uiManager;
    [SerializeField] private VisualizationManager visualizationManager;

        // Simulation state
        private bool isSimulationRunning = false;
        private float simulationSpeed = 1.0f;

    private void Awake()
    {
        // Ensure we have all required components
        if (siteGenerator == null)
            siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
        
        if (morphologyManager == null)
            morphologyManager = FindFirstObjectByType<MorphologyManager>(FindObjectsInactive.Include);
          if (scenarioAnalyzer == null)
            scenarioAnalyzer = FindFirstObjectByType<ScenarioAnalyzer>(FindObjectsInactive.Include);
          if (uiManager == null)
            uiManager = FindFirstObjectByType<ModernUIController>(FindObjectsInactive.Include);
        
        if (visualizationManager == null)
            visualizationManager = FindFirstObjectByType<VisualizationManager>(FindObjectsInactive.Include);

        // Initialize systems except UI (will be done in Start)
        InitializeAllSystems();
    }
      private void Start()
    {
        // Ensure UI is properly initialized after all components are ready
        if (uiManager != null)
        {
            uiManager.Initialize(this, siteGenerator, morphologyManager, scenarioAnalyzer, visualizationManager);
            Debug.Log("UI Manager initialized in Start() method");
            
            // Load background assets after UI is initialized
            LoadBackgroundAssets();
        }
        else
        {
            Debug.LogError("UI Manager reference is missing!");
        }
    }
    
    /// <summary>
    /// Loads and applies the background assets for the start screen
    /// </summary>
    public void LoadBackgroundAssets()
    {
        // Start the coroutine to load background image or video
        StartCoroutine(LoadBackgroundImage());
    }
    
    /// <summary>
    /// Coroutine to load the background image or video
    /// </summary>
    private IEnumerator LoadBackgroundImage()
    {
        // Wait for the end of the frame to ensure UI is fully loaded
        yield return new WaitForEndOfFrame();
        
        // Path to your background image or video in the Resources folder
        string backgroundPath = "Textures/BiomorphicBackground";
        
        // Try to find the RawImage component for the background
        RawImage backgroundImage = null;
        GameObject backgroundObj = GameObject.Find("AnimatedBackground");
        
        if (backgroundObj != null)
        {
            backgroundImage = backgroundObj.GetComponent<RawImage>();
            Debug.Log("Found background image object");
        }
        else
        {
            Debug.LogWarning("Could not find AnimatedBackground object");
            yield break;
        }
        
        // For image texture
        Texture2D backgroundTexture = Resources.Load<Texture2D>(backgroundPath);
        if (backgroundTexture != null && backgroundImage != null)
        {
            Debug.Log("Loaded background texture, applying to UI");
            backgroundImage.texture = backgroundTexture;
            
            // Disable the video player if we're using a static texture
            VideoPlayer videoPlayer = backgroundObj.GetComponent<VideoPlayer>();
            if (videoPlayer != null)
            {
                videoPlayer.enabled = false;
            }
        }
        else
        {
            Debug.Log("No background texture found, checking for video...");
            
            // For video - try to load a video clip
            VideoClip backgroundVideo = Resources.Load<VideoClip>(backgroundPath);
            if (backgroundVideo != null)
            {
                VideoPlayer videoPlayer = backgroundObj.GetComponent<VideoPlayer>();
                if (videoPlayer != null)
                {
                    Debug.Log("Loaded background video, applying to video player");
                    videoPlayer.clip = backgroundVideo;
                    videoPlayer.Play();
                }
                else
                {
                    Debug.LogWarning("Video player component not found on background object");
                }
            }
            else
            {
                Debug.LogWarning("No background image or video found at path: " + backgroundPath);
            }
        }
    }

    private void InitializeAllSystems()
    {
        Debug.Log("Initializing Biomorphic Structure Simulator...");
        
        // Initialize all required managers
        siteGenerator.Initialize();
        morphologyManager.Initialize();
        scenarioAnalyzer.Initialize();
        visualizationManager.Initialize();
        
        // UI initialization moved to Start() method for proper initialization order
        
        // Generate default site
        StartCoroutine(GenerateDefaultSite());
    }

        private IEnumerator GenerateDefaultSite()
        {
            yield return new WaitForEndOfFrame();
            siteGenerator.GenerateDefaultSite();
            Debug.Log("Default site generated.");
        }

        // Public methods to control the simulation
        public void StartSimulation()
        {
            if (!isSimulationRunning)
            {
                isSimulationRunning = true;
                morphologyManager.StartGrowth();
                Debug.Log("Simulation started.");
            }
        }

        public void PauseSimulation()
        {
            if (isSimulationRunning)
            {
                isSimulationRunning = false;
                morphologyManager.PauseGrowth();
                Debug.Log("Simulation paused.");
            }
        }

        public void ResetSimulation()
        {
            isSimulationRunning = false;
            morphologyManager.ResetGrowth();
            Debug.Log("Simulation reset.");
        }

        public void SetSimulationSpeed(float speed)
        {
            simulationSpeed = Mathf.Clamp(speed, 0.1f, 10.0f);
            Time.timeScale = simulationSpeed;
            Debug.Log($"Simulation speed set to {simulationSpeed}x");
        }

        public void RunScenario(string scenarioName)
        {
            scenarioAnalyzer.RunScenario(scenarioName);
            Debug.Log($"Running scenario: {scenarioName}");
        }

        public bool IsSimulationRunning()
        {
            return isSimulationRunning;
        }

        // Called when application is closing
        private void OnApplicationQuit()
        {
            Debug.Log("Biomorphic Structure Simulator shutting down...");
            
            // Perform any cleanup here
            morphologyManager.Cleanup();
            siteGenerator.Cleanup();
            scenarioAnalyzer.Cleanup();
        }
    }
}