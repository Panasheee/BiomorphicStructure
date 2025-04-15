using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core; // Ensure this namespace is used

/// <summary>
/// Core manager class that orchestrates the entire morphology simulation.
/// This is the main entry point and controller for the system.
/// </summary>
public class MorphologyManager : MonoBehaviour
{
    #region Singleton
    public static MorphologyManager Instance { get; private set; }
    
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    #endregion

    #region References
    [Header("System References")]
    [SerializeField] private SiteGenerator siteGenerator;
    [SerializeField] private MorphologyGenerator morphologyGenerator;
    [SerializeField] private ScenarioAnalyzer scenarioAnalyzer;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private AdaptationSystem adaptationSystem; // Add reference field
    
    [Header("Settings")]
    [SerializeField] private SimulationSettings simulationSettings;
    #endregion

    #region State Management
    // Current state of the simulation
    public enum SimulationState
    {
        Idle,
        SiteGeneration,
        ZoneSelection,
        MorphologyGeneration,
        ScenarioAnalysis,
        Paused
    }
    
    public SimulationState CurrentState { get; private set; } = SimulationState.Idle;
    
    // The currently selected zone for morphology generation
    private Bounds selectedZone;
      // Currently active scenario
    private ScenarioData currentScenario;
      // Storage for different morphology generations for comparison
    private Dictionary<string, BiomorphicSim.Core.MorphologyData> savedMorphologies = new Dictionary<string, BiomorphicSim.Core.MorphologyData>();
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets references to other components in the system
    /// </summary>
    // Modified to accept AdaptationSystem
    public void SetReferences(SiteGenerator site, MorphologyGenerator morphology, ScenarioAnalyzer analyzer, UIManager ui, AdaptationSystem adaptSys)
    {
        siteGenerator = site;
        morphologyGenerator = morphology;
        scenarioAnalyzer = analyzer;
        uiManager = ui;
        adaptationSystem = adaptSys; // Assign reference
        
        Debug.Log("MorphologyManager references set");
    }
    
    /// <summary>
    /// Returns the current simulation settings
    /// </summary>
    public SimulationSettings GetCurrentSettings()
    {
        return simulationSettings;
    }
    
    /// <summary>
    /// Initializes the simulation with default settings
    /// </summary>
    public void InitializeSimulation()
    {
        CurrentState = SimulationState.Idle;
        Debug.Log("Morphology Simulator initialized and ready.");
        
        // Initialize sub-systems
        siteGenerator.Initialize();
        morphologyGenerator.Initialize();
        scenarioAnalyzer.Initialize();
        
        // Notify UI
        if (uiManager != null)
        {
            uiManager.UpdateUI(CurrentState);
        }
    }
    
    /// <summary>
    /// Generates the Wellington Lambton Quay site
    /// </summary>
    public void GenerateSite()
    {
        CurrentState = SimulationState.SiteGeneration;
        Debug.Log("Generating Wellington Lambton Quay site...");
        
        siteGenerator.GenerateSite(simulationSettings.siteSettings);
        
        // Once site is generated, move to zone selection
        CurrentState = SimulationState.ZoneSelection;
        
        // Notify UI
        if (uiManager != null)
        {
            uiManager.UpdateUI(CurrentState);
        }
    }
    
    /// <summary>
    /// Sets the selected zone for morphology generation
    /// </summary>
    /// <param name="zone">Bounds of the selected zone</param>
    public void SetSelectedZone(Bounds zone)
    {
        selectedZone = zone;
        Debug.Log($"Zone selected: Center {zone.center}, Size {zone.size}");
        
        // Notify UI
        if (uiManager != null)
        {
            uiManager.UpdateZoneSelection(zone);
        }
    }
      /// <summary>
    /// Starts the morphology generation process
    /// </summary>
    /// <param name="parameters">Parameters controlling the morphology generation</param>
    public void GenerateMorphology(BiomorphicSim.Core.MorphologyParameters parameters)
    {
        if (selectedZone.size == Vector3.zero)
        {
            Debug.LogError("Cannot generate morphology: No zone selected.");
            return;
        }
        
        CurrentState = SimulationState.MorphologyGeneration;
        Debug.Log("Starting morphology generation...");
        
        // Start the generation process
        morphologyGenerator.GenerateMorphology(selectedZone, parameters);
    }    /// <summary>
    /// Called when morphology generation is complete
    /// </summary>
    /// <param name="morphologyData">Data describing the generated morphology</param>
    public void OnMorphologyGenerationComplete(BiomorphicSim.Core.MorphologyData morphologyData)
    {
        Debug.Log("Morphology generation complete.");
        
        // Save the current morphology
        string id = "Morphology_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss");
        savedMorphologies[id] = morphologyData;
        
        // Update UI
        if (uiManager != null)
        {
            // Change to UpdateUI method which exists in UIManager
            uiManager.UpdateUI(CurrentState);
            
            // You might want to add additional UI updates here later
            // For now, we're just calling the existing UpdateUI method
        }
    }
    
    /// <summary>
    /// Runs a scenario analysis on the current morphology
    /// </summary>
    /// <param name="scenario">The scenario to analyze</param>
    public void RunScenarioAnalysis(ScenarioData scenario)
    {
        CurrentState = SimulationState.ScenarioAnalysis;
        currentScenario = scenario;
        
        Debug.Log($"Running scenario analysis: {scenario.scenarioName}");
        
        // Make sure we have a morphology to analyze
        if (savedMorphologies.Count == 0)
        {
            Debug.LogError("Cannot run scenario: No morphology has been generated.");
            return;
        }
          // Get the latest morphology if none is specified
        BiomorphicSim.Core.MorphologyData morphologyToAnalyze = null;
        if (scenario.targetMorphologyId != null && savedMorphologies.ContainsKey(scenario.targetMorphologyId))
        {
            morphologyToAnalyze = savedMorphologies[scenario.targetMorphologyId];
        }
        else
        {
            // Get the last generated morphology
            string lastKey = null;
            foreach (var key in savedMorphologies.Keys)
            {
                lastKey = key;
            }
            
            if (lastKey != null)
            {
                morphologyToAnalyze = savedMorphologies[lastKey];
            }
        }
        
        if (morphologyToAnalyze == null)
        {
            Debug.LogError("Cannot run scenario: Failed to find a valid morphology to analyze.");
            return;
        }
        
        // Run the analysis
        scenarioAnalyzer.RunScenario(morphologyToAnalyze, scenario);
    }
      /// <summary>
    /// Called when a scenario analysis is complete
    /// </summary>
    /// <param name="results">Results of the scenario analysis</param>
    public void OnScenarioAnalysisComplete(ScenarioResults results)
    {
        Debug.Log("Scenario analysis complete.");
        
        // Update UI with results
        if (uiManager != null)
        {
            // Change to UpdateUI method since DisplayScenarioResults doesn't exist
            uiManager.UpdateUI(CurrentState);
            
            // Log results for now
            Debug.Log($"Scenario Analysis Results: {results.scenarioId}, Success: {results.adaptationSuccessful}");
        }
        
        // Return to idle state
        CurrentState = SimulationState.Idle;
    }
    
    /// <summary>
    /// Saves the current simulation state
    /// </summary>
    /// <param name="saveName">Name for the saved state</param>
    public void SaveSimulation(string saveName)
    {
        // Implementation for saving the current state to a file
        Debug.Log($"Saving simulation as: {saveName}");
        
        // TODO: Implement data serialization and saving
    }
    
    /// <summary>
    /// Loads a previously saved simulation state
    /// </summary>
    /// <param name="saveName">Name of the saved state to load</param>
    public void LoadSimulation(string saveName)
    {
        // Implementation for loading a saved state from a file
        Debug.Log($"Loading simulation: {saveName}");
        
        // TODO: Implement data loading and deserialization
    }
    
    /// <summary>
    /// Updates the simulation settings
    /// </summary>
    /// <param name="newSettings">New settings to apply</param>
    public void UpdateSettings(SimulationSettings newSettings)
    {
        simulationSettings = newSettings;
        Debug.Log("Simulation settings updated.");
        
        // Apply settings to sub-systems as needed
        siteGenerator.UpdateSettings(newSettings.siteSettings);
        morphologyGenerator.UpdateSettings(newSettings.morphologySettings);
        scenarioAnalyzer.UpdateSettings(newSettings.scenarioSettings);
    }
    #endregion

    #region Private Methods
    private void Start()
    {
        InitializeSimulation();
    }
    
    private void Update()
    {
        // Handle state-specific updates if needed
        switch (CurrentState)
        {            case SimulationState.MorphologyGeneration:
                // Update progress UI if generation is ongoing
                if (uiManager != null && morphologyGenerator != null)
                {
                    // Changed from UpdateGenerationProgress to UpdateUI, which exists in UIManager
                    uiManager.UpdateUI(CurrentState);
                    // You might want to implement a proper progress update method in UIManager later
                }
                break;
                
            case SimulationState.ScenarioAnalysis:
                // Update scenario progress if analysis is ongoing
                if (uiManager != null && scenarioAnalyzer != null)
                {
                    // Changed from UpdateScenarioProgress to UpdateUI, which exists in UIManager
                    uiManager.UpdateUI(CurrentState);
                    // You might want to implement a proper progress update method in UIManager later
                }
                break;
        }
    }
    #endregion
}

/// <summary>
/// Holds the overall simulation settings
/// </summary>
[System.Serializable]
public class SimulationSettings
{
    public SiteSettings siteSettings;
    public MorphologySettings morphologySettings;
    public ScenarioSettings scenarioSettings;
}

/// <summary>
/// Settings for site generation
/// </summary>
[System.Serializable]
public class SiteSettings
{
    public Vector3 siteSize = new Vector3(1000, 200, 1000);
    public float detailLevel = 1.0f; // Scale from 0 to 1
    public bool includeBuildings = true;
    public bool includeVegetation = true;
    public bool includeRoads = true;
}

/// <summary>
/// Settings for morphology generation
/// </summary>
[System.Serializable]
public class MorphologySettings
{
    public float nodeMinDistance = 1.0f;
    public float nodeMaxDistance = 5.0f;
    public float connectionProbability = 0.7f;
    public int initialSeedCount = 10;
    public int maxNodes = 1000;
    public float growthSpeed = 1.0f;
    public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
}

/// <summary>
/// Settings for scenario analysis
/// </summary>
[System.Serializable]
public class ScenarioSettings
{
    public float analysisTimeStep = 0.1f;
    public int maxIterations = 1000;
    public float convergenceThreshold = 0.01f;
}

/// <summary>
/// Parameters for controlling morphology generation
/// </summary>
[System.Serializable]
public class MorphologyParameters
{
    public float density = 0.5f;         // 0 to 1, controls node density
    public float complexity = 0.5f;      // 0 to 1, controls branching complexity
    public float connectivity = 0.5f;    // 0 to 1, controls connection density
    public float growthRate = 0.5f;      // 0 to 1, controls growth speed
    public float adaptationRate = 0.5f;  // 0 to 1, controls adaptation speed
    public BiomorphType biomorphType = BiomorphType.Mold;
    public GrowthPattern growthPattern = GrowthPattern.Organic;
    
    public enum BiomorphType
    {
        Standard,
        Mold,
        Bone,
        Coral,
        Mycelium,
        Custom
    }
    
    public enum GrowthPattern
    {
        Organic,
        Directed,
        Radial,
        Layered,
        Adaptive
    }
}

/// <summary>
/// Data structure to hold information about a generated morphology
/// </summary>
[System.Serializable]
public class MorphologyData
{
    public List<Vector3> nodePositions;
    public List<int[]> connections; // Pairs of indices into nodePositions
    public Dictionary<string, float> metrics;
    public MorphologyParameters parameters;
    public System.DateTime generationTime;
}

/// <summary>
/// Data structure for scenario configuration
/// </summary>
[System.Serializable]
public class ScenarioData
{
    public string scenarioName;
    public string description;
    public string targetMorphologyId; // ID of the morphology to analyze
    public Dictionary<string, float> environmentalFactors;
    public float simulationDuration;
    public bool recordHistory;
}

/// <summary>
/// Results from a scenario analysis
/// </summary>
[System.Serializable]
public class ScenarioResults
{
    public string scenarioId;
    public string morphologyId;
    public Dictionary<string, float> metrics;
    public Dictionary<string, List<float>> timeSeriesData;
    public bool adaptationSuccessful;
    public List<string> observations;
}