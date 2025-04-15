using UnityEngine;
using System.Collections;
using System.Collections.Generic;
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
        }
        else
        {
            Debug.LogError("UI Manager reference is missing!");
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