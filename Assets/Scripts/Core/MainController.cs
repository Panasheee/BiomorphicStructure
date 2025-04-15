using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core; // Added using directive

/// <summary>
/// Main controller script that initializes and coordinates all components of the morphology simulator.
/// Acts as a central hub for managing the simulation flow and integration of all systems.
/// </summary>
public class MainController : MonoBehaviour
{
    #region References
    [Header("Core Components")]
    [SerializeField] private MorphologyManager morphologyManager;
    [SerializeField] private SiteGenerator siteGenerator;
    [SerializeField] private MorphologyGenerator morphologyGenerator;
    [SerializeField] private ScenarioAnalyzer scenarioAnalyzer;
    [SerializeField] private UIManager uiManager;
    [SerializeField] private VisualizationManager visualizationManager;
    [SerializeField] private AdaptationSystem adaptationSystem;
    [SerializeField] private DataIO dataIO;
    [SerializeField] private DemonstrationController demonstrationController; // Added reference

    [Header("Wellington Lambton Quay Specific")]
    [SerializeField] private TextAsset lambtonQuayGeoData;
    [SerializeField] private Texture2D lambtonQuayHeightmap;
    [SerializeField] private Texture2D lambtonQuayBuildingMap;
    [SerializeField] private GameObject[] lambtonQuayLandmarkPrefabs;
    
    [Header("Default Settings")]
    [SerializeField] private SimulationSettings defaultSettings;
    [SerializeField] private bool loadDefaultSettingsOnStart = true;
    [SerializeField] private bool generateSiteOnStart = false;
    [SerializeField] private bool usePresetZones = true;
    
    // Preset zones for the Lambton Quay site
    [Header("Preset Zones")]
    [SerializeField] private Vector3[] zonePositions;
    [SerializeField] private Vector3[] zoneSizes;
    [SerializeField] private string[] zoneNames;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Verify and initialize all required components
        InitializeComponents();
    }
    
    private void Start()
    {
        // Apply default settings
        if (loadDefaultSettingsOnStart && defaultSettings != null)
        {
            morphologyManager.UpdateSettings(defaultSettings);
        }
        
        // Generate site if requested
        if (generateSiteOnStart)
        {
            GenerateLambtonQuaySite();
        }
        
        // Set up event listeners
        SetupEventListeners();
        
        Debug.Log("Morphology Simulator initialized");
    }
    
    private void OnDestroy()
    {
        // Clean up event listeners if needed
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Generates the Wellington Lambton Quay site
    /// </summary>
    public void GenerateLambtonQuaySite()
    {
        if (siteGenerator == null)
        {
            Debug.LogError("Cannot generate site: SiteGenerator not found");
            return;
        }
        
        // Create site settings
        SiteSettings siteSettings = new SiteSettings
        {
            siteSize = new Vector3(1000, 200, 1000),
            detailLevel = 1.0f,
            includeBuildings = true,
            includeVegetation = true,
            includeRoads = true
        };
        
        // Apply custom data if available
        if (lambtonQuayHeightmap != null)
        {
            siteGenerator.SetHeightmapTexture(lambtonQuayHeightmap);
        }
        
        if (lambtonQuayBuildingMap != null)
        {
            siteGenerator.SetBuildingMapTexture(lambtonQuayBuildingMap);
        }
        
        if (lambtonQuayGeoData != null)
        {
            siteGenerator.LoadGeoData(lambtonQuayGeoData);
        }
        
        // Generate the site
        morphologyManager.GenerateSite();
        
        // Set up preset zones if enabled
        if (usePresetZones && zonePositions != null && zoneSizes != null && 
            zonePositions.Length > 0 && zonePositions.Length == zoneSizes.Length)
        {
            // Set up zone selection UI
            if (uiManager != null)
            {
                uiManager.SetupPresetZones();
            }
        }
        
        Debug.Log("Wellington Lambton Quay site generated");
    }
    
    /// <summary>
    /// Loads a saved simulation
    /// </summary>
    /// <param name="fileName">Name of the save file</param>
    public void LoadSimulation(string fileName)
    {
        if (dataIO == null)
        {
            Debug.LogError("Cannot load simulation: DataIO not found");
            return;
        }
        
        // Load settings
        SimulationSettings settings = dataIO.LoadSettings(fileName);
        if (settings != null)
        {
            morphologyManager.UpdateSettings(settings);
        }
        
        // Load morphology data
        BiomorphicSim.Core.MorphologyData morphologyData = dataIO.LoadMorphology(fileName);
        morphologyGenerator.ImportMorphologyData(morphologyData);
        morphologyManager.OnMorphologyGenerationComplete(morphologyData);
        
        Debug.Log($"Simulation loaded from {fileName}");
    }
    
    /// <summary>
    /// Saves the current simulation
    /// </summary>
    /// <param name="fileName">Name for the save file</param>
    public void SaveSimulation(string fileName)
    {
        if (dataIO == null)
        {
            Debug.LogError("Cannot save simulation: DataIO not found");
            return;
        }
          // Save settings
        SimulationSettings currentSettings = morphologyManager.GetCurrentSettings();
        dataIO.SaveSettings(currentSettings, fileName);
        
        // Export morphology data if available
        BiomorphicSim.Core.MorphologyData morphologyData = morphologyGenerator.ExportMorphologyData();
        if (morphologyData != null)
        {
            dataIO.SaveMorphology(morphologyData, fileName);
        }
        
        // Save active scenario if available
        ScenarioData scenarioData = scenarioAnalyzer.GetCurrentScenario();
        if (scenarioData != null)
        {
            dataIO.SaveScenario(scenarioData, fileName);
        }
        
        Debug.Log($"Simulation saved as {fileName}");
    }
    
    /// <summary>
    /// Runs a demonstration of the system by delegating to DemonstrationController
    /// </summary>
    /// <param name="demoType">Type of demo to run</param>
    public void RunDemonstration(string demoType)
    {
        if (demonstrationController != null)
        {
            demonstrationController.RunDemonstration(demoType);
        }
        else
        {
            Debug.LogError("DemonstrationController reference not set in MainController. Cannot run demonstration.");
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes all required components
    /// </summary>
    private void InitializeComponents()
    {
        if (morphologyManager == null)
        {
            morphologyManager = FindFirstObjectByType<MorphologyManager>();
            if (morphologyManager == null)
            {
                GameObject obj = new GameObject("MorphologyManager");
                morphologyManager = obj.AddComponent<MorphologyManager>();
            }
        }
        
        if (siteGenerator == null)
        {
            siteGenerator = FindFirstObjectByType<SiteGenerator>();
            if (siteGenerator == null && morphologyManager != null)
            {
                GameObject obj = new GameObject("SiteGenerator");
                siteGenerator = obj.AddComponent<SiteGenerator>();
                obj.transform.parent = morphologyManager.transform;
            }
        }
        
        if (morphologyGenerator == null)
        {
            morphologyGenerator = FindFirstObjectByType<MorphologyGenerator>();
            if (morphologyGenerator == null && morphologyManager != null)
            {
                GameObject obj = new GameObject("MorphologyGenerator");
                morphologyGenerator = obj.AddComponent<MorphologyGenerator>();
                obj.transform.parent = morphologyManager.transform;
            }
        }
        
        if (scenarioAnalyzer == null)
        {
            scenarioAnalyzer = FindFirstObjectByType<ScenarioAnalyzer>();
            if (scenarioAnalyzer == null && morphologyManager != null)
            {
                GameObject obj = new GameObject("ScenarioAnalyzer");
                scenarioAnalyzer = obj.AddComponent<ScenarioAnalyzer>();
                obj.transform.parent = morphologyManager.transform;
            }
        }
        
        if (uiManager == null)
        {
            uiManager = FindFirstObjectByType<UIManager>();
        }
        
        if (visualizationManager == null)
        {
            visualizationManager = FindFirstObjectByType<VisualizationManager>();
            if (visualizationManager == null)
            {
                GameObject obj = new GameObject("VisualizationManager");
                visualizationManager = obj.AddComponent<VisualizationManager>();
            }
        }
        
        if (adaptationSystem == null)
        {
            adaptationSystem = FindFirstObjectByType<AdaptationSystem>();
            if (adaptationSystem == null && morphologyManager != null)
            {
                GameObject obj = new GameObject("AdaptationSystem");
                adaptationSystem = obj.AddComponent<AdaptationSystem>();
                obj.transform.parent = morphologyManager.transform;
            }
        }
        
        if (dataIO == null)
        {
            dataIO = FindFirstObjectByType<DataIO>();
            if (dataIO == null)
            {
                GameObject obj = new GameObject("DataIO");
                dataIO = obj.AddComponent<DataIO>();
            }
        }

        // Find or create DemonstrationController
        if (demonstrationController == null)
        {
            demonstrationController = FindFirstObjectByType<DemonstrationController>();
            if (demonstrationController == null)
            {
                GameObject obj = new GameObject("DemonstrationController");
                demonstrationController = obj.AddComponent<DemonstrationController>();
                // Optionally parent it
                // obj.transform.parent = this.transform; 
            }
            // Pass necessary references to DemonstrationController
            // This assumes DemonstrationController has public setters or uses [SerializeField]
            // demonstrationController.SetupReferences(this, morphologyManager, siteGenerator, ...); 
            // OR rely on DemonstrationController finding its own references via FindObjectOfType or [SerializeField]
        }

        if (morphologyManager != null)
        {
            morphologyManager.SetReferences(
                siteGenerator,
                morphologyGenerator,
                scenarioAnalyzer,
                uiManager,
                adaptationSystem
            );
        }
        
        // Ensure DemonstrationController has its references set up
        // (Could be done here or within DemonstrationController's Awake/Start)
        // Consider adding a public Initialize method to DemonstrationController if needed
    }
    
    /// <summary>
    /// Sets up event listeners for components
    /// </summary>
    private void SetupEventListeners()
    {
        // Example: morphologyManager.OnSiteGenerated += HandleSiteGenerated;
    }
    #endregion
}