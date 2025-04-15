using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core;

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
    /// Runs a demonstration of the system
    /// </summary>
    /// <param name="demoType">Type of demo to run</param>
    public void RunDemonstration(string demoType)
    {
        StartCoroutine(DemonstrationCoroutine(demoType));
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
    }
    
    /// <summary>
    /// Sets up event listeners for components
    /// </summary>
    private void SetupEventListeners()
    {
        // Example: morphologyManager.OnSiteGenerated += HandleSiteGenerated;
    }
    
    /// <summary>
    /// Coroutine that runs a demonstration of the system
    /// </summary>
    /// <param name="demoType">Type of demo to run</param>
    private IEnumerator DemonstrationCoroutine(string demoType)
    {
        Debug.Log($"Starting demonstration: {demoType}");
        
        switch (demoType)
        {
            case "GrowthAndAdaptation":
                yield return StartCoroutine(GrowthAndAdaptationDemo());
                break;
                
            case "WindAdaptation":
                yield return StartCoroutine(WindAdaptationDemo());
                break;
                
            case "BioTypeComparison":
                yield return StartCoroutine(BioTypeComparisonDemo());
                break;
                
            case "FullLifecycle":
                yield return StartCoroutine(FullLifecycleDemo());
                break;
                
            default:
                Debug.LogWarning($"Unknown demo type: {demoType}");
                break;
        }
        
        Debug.Log("Demonstration completed");
    }
    
    /// <summary>
    /// Demonstration of growth and adaptation
    /// </summary>
    private IEnumerator GrowthAndAdaptationDemo()
    {
        if (siteGenerator.GetSiteBounds().size == Vector3.zero)
        {
            GenerateLambtonQuaySite();
            yield return new WaitForSeconds(1f);
        }
          if (zonePositions.Length > 0)
        {
            Bounds zone = new Bounds(zonePositions[0], zoneSizes[0]);
            morphologyManager.SetSelectedZone(zone);
            yield return new WaitForSeconds(0.5f);
        }
        
        BiomorphicSim.Core.MorphologyParameters parameters = new BiomorphicSim.Core.MorphologyParameters
        {
            density = 0.6f,
            complexity = 0.7f,
            connectivity = 0.5f,
            biomorphType = BiomorphicSim.Core.MorphologyParameters.BiomorphType.Mold,
            growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Adaptive
        };
        
        morphologyManager.GenerateMorphology(parameters);
        
        while (morphologyGenerator.GenerationProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        ScenarioData scenario = new ScenarioData
        {
            scenarioName = "Adaptive Growth Demo",
            description = "Demonstrates adaptation to changing environmental forces",
            simulationDuration = 15f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 0.8f },
                { "Gravity", 0.5f },
                { "Temperature", 0.4f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }
        
        if (uiManager != null)
        {
            ScenarioResults results = scenarioAnalyzer.GetResults();
            if (results != null) 
            {
                // Use DisplayScenarioResults in place of missing ShowResultsPanel
                uiManager.DisplayScenarioResults(results);
            }
            else
            {
                Debug.LogWarning("No results available to show in GrowthAndAdaptationDemo.");
            }
        }
    }
    
    /// <summary>
    /// Demonstration of wind adaptation
    /// </summary>
    private IEnumerator WindAdaptationDemo()
    {
        if (siteGenerator.GetSiteBounds().size == Vector3.zero)
        {
            GenerateLambtonQuaySite();
            yield return new WaitForSeconds(1f);
        }
          if (zonePositions.Length > 1)
        {
            Bounds zone = new Bounds(zonePositions[1], zoneSizes[1]);
            morphologyManager.SetSelectedZone(zone);
            yield return new WaitForSeconds(0.5f);
        }
        
        BiomorphicSim.Core.MorphologyParameters parameters = new BiomorphicSim.Core.MorphologyParameters
        {
            density = 0.4f,
            complexity = 0.6f,
            connectivity = 0.6f,
            biomorphType = BiomorphicSim.Core.MorphologyParameters.BiomorphType.Bone,
            growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Directed
        };
        
        morphologyManager.GenerateMorphology(parameters);
        
        while (morphologyGenerator.GenerationProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1f);
        
        ScenarioData scenario = new ScenarioData
        {
            scenarioName = "Wind Adaptation",
            description = "Tests how structure adapts to strong wind forces",
            simulationDuration = 12f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 1.0f },
                { "Gravity", 0.3f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }
        
        if (uiManager != null)
        {
            ScenarioResults results = scenarioAnalyzer.GetResults();
            if (results != null)
            {
                uiManager.DisplayScenarioResults(results);
            }
            else
            {
                Debug.LogWarning("No results available to show in WindAdaptationDemo.");
            }
        }
    }
    
    /// <summary>
    /// Demonstration comparing different bio-types
    /// </summary>
    private IEnumerator BioTypeComparisonDemo()
    {        if (siteGenerator.GetSiteBounds().size == Vector3.zero)
        {
            GenerateLambtonQuaySite();
            yield return new WaitForSeconds(1f);
        }
        
        BiomorphicSim.Core.MorphologyParameters.BiomorphType[] types = new BiomorphicSim.Core.MorphologyParameters.BiomorphType[]
        {
            BiomorphicSim.Core.MorphologyParameters.BiomorphType.Mold,
            BiomorphicSim.Core.MorphologyParameters.BiomorphType.Bone,
            BiomorphicSim.Core.MorphologyParameters.BiomorphType.Coral,
            BiomorphicSim.Core.MorphologyParameters.BiomorphType.Mycelium
        };
        
        ScenarioData commonScenario = new ScenarioData
        {
            scenarioName = "Biotype Comparison",
            description = "Comparing how different biotypes respond to the same conditions",
            simulationDuration = 10f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 0.7f },
                { "Gravity", 0.6f },
                { "Temperature", 0.3f },
                { "PedestrianFlow", 0.5f }
            }
        };
        
        List<ScenarioResults> comparisonResults = new List<ScenarioResults>();
        
        for (int i = 0; i < types.Length; i++)
        {
            if (zonePositions.Length > 0)
            {                Bounds zone = new Bounds(zonePositions[0], zoneSizes[0]);
                morphologyManager.SetSelectedZone(zone);
                yield return new WaitForSeconds(0.5f);
            }
            
            BiomorphicSim.Core.MorphologyParameters parameters = new BiomorphicSim.Core.MorphologyParameters
            {
                density = 0.5f,
                complexity = 0.5f,
                connectivity = 0.5f,
                biomorphType = types[i],
                growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Organic
            };
            
            morphologyManager.GenerateMorphology(parameters);
            
            while (morphologyGenerator.GenerationProgress < 1f)
            {
                yield return null;
            }
            
            yield return new WaitForSeconds(1f);
            
            commonScenario.scenarioName = $"Biotype Comparison - {types[i]}";
            morphologyManager.RunScenarioAnalysis(commonScenario);
            
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }

            ScenarioResults currentResult = scenarioAnalyzer.GetResults();
            if (currentResult != null) 
            {
                comparisonResults.Add(currentResult);
            }
            
            string screenshotPath = visualizationManager.TakeScreenshot();
            Debug.Log($"Screenshot saved: {screenshotPath}");
            
            if (uiManager != null && currentResult != null)
            {
                uiManager.DisplayScenarioResults(currentResult);
                yield return new WaitForSeconds(3f);
            }
            
            if (dataIO != null)
            {
                string saveName = $"BioComparison_{types[i]}";
                
                MorphologyData morphologyData = morphologyGenerator.ExportMorphologyData();
                if (morphologyData != null)
                {
                    dataIO.SaveMorphology(morphologyData, saveName);
                }
                
                ScenarioResults results = scenarioAnalyzer.GetResults();
                if (results != null)
                {
                    dataIO.SaveResults(results, saveName);
                }
            }
        }
        
        if (uiManager != null)
        {
            uiManager.ShowComparisonPanel(comparisonResults.Count > 0 ? comparisonResults : null);
        }
    }
    
    /// <summary>
    /// Demonstration of the full lifecycle from site to morphology to adaptation
    /// </summary>
    private IEnumerator FullLifecycleDemo()
    {
        GenerateLambtonQuaySite();
        yield return new WaitForSeconds(2f);
        
        if (zonePositions.Length > 2)
        {
            Bounds zone = new Bounds(zonePositions[2], zoneSizes[2]);
            morphologyManager.SetSelectedZone(zone);
        }
        else
        {
            Vector3 siteCenter = siteGenerator.GetSiteBounds().center;
            Vector3 zoneSize = new Vector3(30, 30, 30);
            Bounds zone = new Bounds(siteCenter, zoneSize);
            morphologyManager.SetSelectedZone(zone);
        }
          yield return new WaitForSeconds(1f);
        
        BiomorphicSim.Core.MorphologyParameters parameters = new BiomorphicSim.Core.MorphologyParameters
        {
            density = 0.7f,
            complexity = 0.8f,
            connectivity = 0.6f,
            biomorphType = BiomorphicSim.Core.MorphologyParameters.BiomorphType.Custom,
            growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Adaptive
        };
        
        morphologyManager.GenerateMorphology(parameters);
        
        while (morphologyGenerator.GenerationProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(2f);
        
        // Scenario 1: Initial growth
        ScenarioData scenario1 = new ScenarioData
        {
            scenarioName = "Initial Growth",
            description = "Initial growth phase with minimal environmental forces",
            simulationDuration = 8f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Gravity", 0.4f },
                { "SiteAttraction", 0.3f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario1);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Scenario 2: Wind stress
        ScenarioData scenario2 = new ScenarioData
        {
            scenarioName = "Wind Stress Adaptation",
            description = "Adaptation to increasing wind forces",
            simulationDuration = 10f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 0.8f },
                { "Gravity", 0.4f },
                { "SiteAttraction", 0.2f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario2);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Scenario 3: Pedestrian interaction
        ScenarioData scenario3 = new ScenarioData
        {
            scenarioName = "Pedestrian Interaction",
            description = "Adaptation to pedestrian flow patterns",
            simulationDuration = 12f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 0.3f },
                { "Gravity", 0.4f },
                { "PedestrianFlow", 0.9f },
                { "SiteAttraction", 0.2f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario3);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }
        
        yield return new WaitForSeconds(1.5f);
        
        // Scenario 4: Extreme weather
        ScenarioData scenario4 = new ScenarioData
        {
            scenarioName = "Extreme Weather",
            description = "Testing resilience to extreme weather conditions",
            simulationDuration = 15f,
            recordHistory = true,
            environmentalFactors = new Dictionary<string, float>
            {
                { "Wind", 1.0f },
                { "Gravity", 0.7f },
                { "Temperature", 0.9f },
                { "SiteAttraction", 0.5f }
            }
        };
        
        morphologyManager.RunScenarioAnalysis(scenario4);
        
        while (scenarioAnalyzer.AnalysisProgress < 1f)
        {
            yield return null;
        }

        var lifecycleData = new List<MorphologyData>();

        if (uiManager != null)
        {
            uiManager.ShowLifecycleComparisonPanel(lifecycleData.Count > 0 ? lifecycleData : null);
        }
        
        if (dataIO != null)
        {
            string saveName = "FullLifecycleDemo";
            
            MorphologyData morphologyData = morphologyGenerator.ExportMorphologyData();
            if (morphologyData != null)
            {
                dataIO.SaveMorphology(morphologyData, saveName);
            }
            
            ScenarioResults results = scenarioAnalyzer.GetResults();
            if (results != null)
            {
                dataIO.SaveResults(results, saveName);
            }
            
            visualizationManager.ExportCurrentView(saveName);
        }
    }
    #endregion
}
