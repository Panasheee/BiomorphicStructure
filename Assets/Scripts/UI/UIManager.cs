using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

/// <summary>
/// Manages the user interface for the morphology simulator.
/// This class handles all UI panels, controls, and feedback for the user.
/// </summary>
public class UIManager : MonoBehaviour
{
    #region References
    [Header("Main Panels")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private GameObject siteSelectionPanel;
    [SerializeField] private GameObject zoneSelectionPanel;
    [SerializeField] private GameObject morphologyControlPanel;
    [SerializeField] private GameObject scenarioPanel;
    [SerializeField] private GameObject resultsPanel;
    [SerializeField] private GameObject settingsPanel;
    
    [Header("Site Selection Controls")]
    [SerializeField] private Button generateSiteButton;
    [SerializeField] private Slider siteDetailSlider;
    [SerializeField] private Toggle includeBuildingsToggle;
    [SerializeField] private Toggle includeVegetationToggle;
    [SerializeField] private Toggle includeRoadsToggle;
    
    [Header("Zone Selection Controls")]
    [SerializeField] private Slider zoneWidthSlider;
    [SerializeField] private Slider zoneHeightSlider;
    [SerializeField] private Slider zoneDepthSlider;
    [SerializeField] private Button selectRandomZoneButton;
    [SerializeField] private Button confirmZoneButton;
    [SerializeField] private TextMeshProUGUI zoneInfoText;
    
    [Header("Morphology Controls")]
    [SerializeField] private Slider densitySlider;
    [SerializeField] private Slider complexitySlider;
    [SerializeField] private Slider connectivitySlider;
    [SerializeField] private TMP_Dropdown biomorphTypeDropdown;
    [SerializeField] private TMP_Dropdown growthPatternDropdown;
    [SerializeField] private Button generateMorphologyButton;
    [SerializeField] private Button resetMorphologyButton;
    
    [Header("Scenario Controls")]
    [SerializeField] private TMP_Dropdown scenarioDropdown;
    [SerializeField] private Button runScenarioButton;
    [SerializeField] private Button stopScenarioButton;
    [SerializeField] private ScrollRect environmentalFactorsScrollView;
    [SerializeField] private GameObject environmentalFactorPrefab;
    [SerializeField] private Button addFactorButton;
    
    [Header("Results Display")]
    [SerializeField] private TextMeshProUGUI resultsText;
    [SerializeField] private RectTransform graphContainer;
    [SerializeField] private GameObject graphPrefab;
    [SerializeField] private Button exportResultsButton;
    [SerializeField] private TextMeshProUGUI observationsText;
    
    [Header("Progress Indicators")]
    [SerializeField] private Slider generationProgressSlider;
    [SerializeField] private Slider analysisProgressSlider;
    [SerializeField] private TextMeshProUGUI statusText;
    
    [Header("Settings Controls")]
    [SerializeField] private Slider timeStepSlider;
    [SerializeField] private Slider maxIterationsSlider;
    [SerializeField] private Slider convergenceThresholdSlider;
    [SerializeField] private Button applySettingsButton;
    [SerializeField] private Toggle autoRotateToggle;
    [SerializeField] private Slider cameraSpeedSlider;
    
    [Header("Visualization Controls")]
    [SerializeField] private Toggle showStressColorsToggle;
    [SerializeField] private Toggle showForcesToggle;
    [SerializeField] private Slider transparencySlider;
    [SerializeField] private Button screenshotButton;
    [SerializeField] private TMP_Dropdown visualStyleDropdown;
    
    [Header("Navigation")]
    [SerializeField] private Button siteTabButton;
    [SerializeField] private Button zoneTabButton;
    [SerializeField] private Button morphologyTabButton;
    [SerializeField] private Button scenarioTabButton;
    [SerializeField] private Button resultsTabButton;
    [SerializeField] private Button settingsTabButton;
    #endregion

    #region Private Variables
    // Currently selected zone
    private Bounds selectedZone;
    
    // Zone selection gizmo
    private GameObject zoneGizmo;
    
    // Currently active panel
    private GameObject currentPanel;
    
    // Camera control
    private Transform cameraTransform;
    private bool autoRotate = false;
    private float cameraSpeed = 10f;
    
    // List of active environmental factors in scenario builder
    private List<EnvironmentalFactorUI> activeFactors = new List<EnvironmentalFactorUI>();
    
    // List of saved morphologies for scenario comparison
    private Dictionary<string, MorphologyData> savedMorphologies = new Dictionary<string, MorphologyData>();
    
    // List of created graphs
    private List<GameObject> activeGraphs = new List<GameObject>();
    
    // Current simulation state
    private MorphologyManager.SimulationState currentState = MorphologyManager.SimulationState.Idle;
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Find camera
        cameraTransform = Camera.main.transform;
        
        // Create zone gizmo
        CreateZoneGizmo();
        
        // Set current panel to main panel
        currentPanel = mainPanel;
        
        // Initialize UI states
        InitializeUI();
    }
    
    private void Start()
    {
        // Initialize dropdowns
        InitializeDropdowns();
        
        // Set up button listeners
        SetupButtonListeners();
        
        // Set up slider listeners
        SetupSliderListeners();
        
        // Show initial panel
        ShowPanel(siteSelectionPanel);
    }
    
    private void Update()
    {
        // Handle camera rotation if auto-rotate is enabled
        if (autoRotate && cameraTransform != null)
        {
            cameraTransform.RotateAround(Vector3.zero, Vector3.up, cameraSpeed * Time.deltaTime);
        }
        
        // Update progress indicators if needed
        UpdateProgressIndicators();
    }
    #endregion

    #region Initialization Methods
    /// <summary>
    /// Initializes the UI elements to their default states
    /// </summary>
    private void InitializeUI()
    {
        // Hide all panels initially
        if (siteSelectionPanel != null) siteSelectionPanel.SetActive(false);
        if (zoneSelectionPanel != null) zoneSelectionPanel.SetActive(false);
        if (morphologyControlPanel != null) morphologyControlPanel.SetActive(false);
        if (scenarioPanel != null) scenarioPanel.SetActive(false);
        if (resultsPanel != null) resultsPanel.SetActive(false);
        if (settingsPanel != null) settingsPanel.SetActive(false);
        
        // Set initial slider values
        if (siteDetailSlider != null) siteDetailSlider.value = 0.5f;
        if (zoneWidthSlider != null) zoneWidthSlider.value = 20f;
        if (zoneHeightSlider != null) zoneHeightSlider.value = 20f;
        if (zoneDepthSlider != null) zoneDepthSlider.value = 20f;
        if (densitySlider != null) densitySlider.value = 0.5f;
        if (complexitySlider != null) complexitySlider.value = 0.5f;
        if (connectivitySlider != null) connectivitySlider.value = 0.5f;
        
        // Set initial toggle states
        if (includeBuildingsToggle != null) includeBuildingsToggle.isOn = true;
        if (includeVegetationToggle != null) includeVegetationToggle.isOn = true;
        if (includeRoadsToggle != null) includeRoadsToggle.isOn = true;
        if (autoRotateToggle != null) autoRotateToggle.isOn = autoRotate;
        if (showStressColorsToggle != null) showStressColorsToggle.isOn = true;
        if (showForcesToggle != null) showForcesToggle.isOn = true;
        
        // Setup progress sliders
        if (generationProgressSlider != null) 
        {
            generationProgressSlider.value = 0f;
            generationProgressSlider.gameObject.SetActive(false);
        }
        
        if (analysisProgressSlider != null) 
        {
            analysisProgressSlider.value = 0f;
            analysisProgressSlider.gameObject.SetActive(false);
        }
        
        // Clear text displays
        if (statusText != null) statusText.text = "Ready";
        if (zoneInfoText != null) zoneInfoText.text = "No zone selected";
        if (resultsText != null) resultsText.text = "No analysis results";
        if (observationsText != null) observationsText.text = "No observations";
    }
    
    /// <summary>
    /// Initializes dropdown menus with their options
    /// </summary>
    private void InitializeDropdowns()
    {
        // Biomorph type dropdown
        if (biomorphTypeDropdown != null)
        {
            biomorphTypeDropdown.ClearOptions();
            biomorphTypeDropdown.AddOptions(new List<string> 
            {
                "Mold",
                "Bone",
                "Coral",
                "Mycelium",
                "Custom"
            });
        }
        
        // Growth pattern dropdown
        if (growthPatternDropdown != null)
        {
            growthPatternDropdown.ClearOptions();
            growthPatternDropdown.AddOptions(new List<string> 
            {
                "Organic",
                "Directed",
                "Radial",
                "Layered",
                "Adaptive"
            });
        }
        
        // Scenario dropdown
        if (scenarioDropdown != null)
        {
            scenarioDropdown.ClearOptions();
            scenarioDropdown.AddOptions(new List<string> 
            {
                "Wind Resistance",
                "Gravity Adaptation",
                "Temperature Response",
                "Pedestrian Flow",
                "Multi-Factor",
                "Custom"
            });
            
            // Set up listener for changing scenarios
            scenarioDropdown.onValueChanged.AddListener(OnScenarioChanged);
        }
        
        // Visual style dropdown
        if (visualStyleDropdown != null)
        {
            visualStyleDropdown.ClearOptions();
            visualStyleDropdown.AddOptions(new List<string> 
            {
                "Default",
                "Wireframe",
                "X-Ray",
                "Minimal",
                "Colorful"
            });
        }
    }
    
    /// <summary>
    /// Sets up listeners for buttons
    /// </summary>
    private void SetupButtonListeners()
    {
        // Navigation buttons
        if (siteTabButton != null) siteTabButton.onClick.AddListener(() => ShowPanel(siteSelectionPanel));
        if (zoneTabButton != null) zoneTabButton.onClick.AddListener(() => ShowPanel(zoneSelectionPanel));
        if (morphologyTabButton != null) morphologyTabButton.onClick.AddListener(() => ShowPanel(morphologyControlPanel));
        if (scenarioTabButton != null) scenarioTabButton.onClick.AddListener(() => ShowPanel(scenarioPanel));
        if (resultsTabButton != null) resultsTabButton.onClick.AddListener(() => ShowPanel(resultsPanel));
        if (settingsTabButton != null) settingsTabButton.onClick.AddListener(() => ShowPanel(settingsPanel));
        
        // Site generation
        if (generateSiteButton != null) generateSiteButton.onClick.AddListener(OnGenerateSiteClicked);
        
        // Zone selection
        if (selectRandomZoneButton != null) selectRandomZoneButton.onClick.AddListener(OnSelectRandomZoneClicked);
        if (confirmZoneButton != null) confirmZoneButton.onClick.AddListener(OnConfirmZoneClicked);
        
        // Morphology generation
        if (generateMorphologyButton != null) generateMorphologyButton.onClick.AddListener(OnGenerateMorphologyClicked);
        if (resetMorphologyButton != null) resetMorphologyButton.onClick.AddListener(OnResetMorphologyClicked);
        
        // Scenario controls
        if (runScenarioButton != null) runScenarioButton.onClick.AddListener(OnRunScenarioClicked);
        if (stopScenarioButton != null) stopScenarioButton.onClick.AddListener(OnStopScenarioClicked);
        if (addFactorButton != null) addFactorButton.onClick.AddListener(OnAddFactorClicked);
        
        // Results controls
        if (exportResultsButton != null) exportResultsButton.onClick.AddListener(OnExportResultsClicked);
        
        // Settings controls
        if (applySettingsButton != null) applySettingsButton.onClick.AddListener(OnApplySettingsClicked);
        
        // Visualization controls
        if (screenshotButton != null) screenshotButton.onClick.AddListener(OnScreenshotClicked);
    }
    
    /// <summary>
    /// Sets up listeners for sliders
    /// </summary>
    private void SetupSliderListeners()
    {
        // Zone selection sliders
        if (zoneWidthSlider != null) zoneWidthSlider.onValueChanged.AddListener(OnZoneSizeChanged);
        if (zoneHeightSlider != null) zoneHeightSlider.onValueChanged.AddListener(OnZoneSizeChanged);
        if (zoneDepthSlider != null) zoneDepthSlider.onValueChanged.AddListener(OnZoneSizeChanged);
        
        // Camera controls
        if (cameraSpeedSlider != null) cameraSpeedSlider.onValueChanged.AddListener(OnCameraSpeedChanged);
        if (autoRotateToggle != null) autoRotateToggle.onValueChanged.AddListener(OnAutoRotateChanged);
        
        // Visualization controls
        if (transparencySlider != null) transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
        if (showStressColorsToggle != null) showStressColorsToggle.onValueChanged.AddListener(OnShowStressColorsChanged);
        if (showForcesToggle != null) showForcesToggle.onValueChanged.AddListener(OnShowForcesChanged);
    }
    
    /// <summary>
    /// Creates a gizmo to visualize the selected zone
    /// </summary>
    private void CreateZoneGizmo()
    {
        // Create parent object
        zoneGizmo = new GameObject("ZoneGizmo");
        
        // Create wireframe for the zone
        GameObject wireframe = new GameObject("Wireframe");
        wireframe.transform.parent = zoneGizmo.transform;
        
        // Add line renderer
        LineRenderer lineRenderer = wireframe.AddComponent<LineRenderer>();
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = 0.1f;
        lineRenderer.endWidth = 0.1f;
        lineRenderer.material = new Material(Shader.Find("Standard"));
        lineRenderer.material.color = new Color(0, 1, 0, 0.7f);
        
        // Set points for a cube (12 lines, 24 points with loops)
        lineRenderer.positionCount = 24;
        
        // Hide initially
        zoneGizmo.SetActive(false);
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Updates the UI based on the current simulation state
    /// </summary>
    /// <param name="state">Current simulation state</param>
    public void UpdateUI(MorphologyManager.SimulationState state)
    {
        currentState = state;
        
        switch (state)
        {
            case MorphologyManager.SimulationState.Idle:
                statusText.text = "Ready";
                generationProgressSlider.gameObject.SetActive(false);
                analysisProgressSlider.gameObject.SetActive(false);
                break;
                
            case MorphologyManager.SimulationState.SiteGeneration:
                statusText.text = "Generating site...";
                generationProgressSlider.gameObject.SetActive(true);
                ShowPanel(siteSelectionPanel);
                break;
                
            case MorphologyManager.SimulationState.ZoneSelection:
                statusText.text = "Select zone for morphology generation";
                generationProgressSlider.gameObject.SetActive(false);
                zoneSelectionPanel.SetActive(true);
                break;
                
            case MorphologyManager.SimulationState.MorphologyGeneration:
                statusText.text = "Generating morphology...";
                generationProgressSlider.gameObject.SetActive(true);
                ShowPanel(morphologyControlPanel);
                break;
                
            case MorphologyManager.SimulationState.ScenarioAnalysis:
                statusText.text = "Running scenario analysis...";
                analysisProgressSlider.gameObject.SetActive(true);
                ShowPanel(scenarioPanel);
                break;
                
            case MorphologyManager.SimulationState.Paused:
                statusText.text = "Paused";
                break;
        }
        
        // Update button states
        UpdateButtonStates(state);
    }
    
    /// <summary>
    /// Updates the zone selection visualization
    /// </summary>
    /// <param name="zone">Selected zone bounds</param>
    public void UpdateZoneSelection(Bounds zone)
    {
        selectedZone = zone;
        
        // Update zone gizmo
        UpdateZoneGizmo(zone);
        
        // Update zone info text
        zoneInfoText.text = $"Zone: Center({zone.center.x:F1}, {zone.center.y:F1}, {zone.center.z:F1}), " +
                            $"Size({zone.size.x:F1}, {zone.size.y:F1}, {zone.size.z:F1})";
        
        // Show the gizmo
        zoneGizmo.SetActive(true);
        
        // Enable morphology generation
        if (morphologyTabButton != null) morphologyTabButton.interactable = true;
    }
    
    /// <summary>
    /// Updates the morphology generation progress
    /// </summary>
    /// <param name="progress">Generation progress (0-1)</param>
    public void UpdateGenerationProgress(float progress)
    {
        if (generationProgressSlider != null)
        {
            generationProgressSlider.value = progress;
            statusText.text = $"Generating morphology... {progress * 100:F0}%";
        }
    }
    
    /// <summary>
    /// Updates the scenario analysis progress
    /// </summary>
    /// <param name="progress">Analysis progress (0-1)</param>
    public void UpdateScenarioProgress(float progress)
    {
        if (analysisProgressSlider != null)
        {
            analysisProgressSlider.value = progress;
            statusText.text = $"Running scenario analysis... {progress * 100:F0}%";
        }
    }
    
    /// <summary>
    /// Called when a morphology has been generated
    /// </summary>
    /// <param name="morphologyData">Generated morphology data</param>
    /// <param name="id">Unique identifier for the morphology</param>
    public void OnMorphologyGenerated(MorphologyData morphologyData, string id)
    {
        // Save the morphology
        savedMorphologies[id] = morphologyData;
        
        // Update UI
        statusText.text = "Morphology generation complete";
        generationProgressSlider.gameObject.SetActive(false);
        
        // Enable scenario analysis
        if (scenarioTabButton != null) scenarioTabButton.interactable = true;
        
        // Log metrics
        Debug.Log($"Morphology generated: {id}");
        Debug.Log($"Node count: {morphologyData.nodePositions.Count}");
        Debug.Log($"Connection count: {morphologyData.connections.Count}");
        
        // Add to the morphology dropdown if it exists
        AddMorphologyToDropdowns(id);
    }

    /// <summary>
    /// Public method to display scenario results (for MorphologyManager)
    /// </summary>
    public void DisplayScenarioResults(ScenarioResults results)
    {
        if (results == null) return;
        DisplayMetrics(results);
        CreateGraphs(results.timeSeriesData);
        DisplayObservations(results.observations);
        ShowPanel(resultsPanel);
    }

    /// <summary>
    /// Sets up preset zones (Placeholder based on error)
    /// </summary>
    public void SetupPresetZones()
    {
        Debug.LogWarning("UIManager.SetupPresetZones() not implemented.");
        // TODO: Implement
    }

    /// <summary>
    /// Shows the comparison panel (Placeholder based on error)
    /// </summary>
    public void ShowComparisonPanel(List<ScenarioResults> resultsList)
    {
        Debug.LogWarning("UIManager.ShowComparisonPanel() not implemented.");
        // TODO: Implement comparison display
        ShowPanel(resultsPanel); // Fallback
    }

    /// <summary>
    /// Shows the lifecycle comparison panel (Placeholder based on error)
    /// </summary>
    public void ShowLifecycleComparisonPanel(List<MorphologyData> lifecycleData)
    {
        Debug.LogWarning("UIManager.ShowLifecycleComparisonPanel() not implemented.");
        // TODO: Implement lifecycle comparison display
        ShowPanel(resultsPanel); // Fallback
    }

    /// <summary>
    /// Sets references to other managers (Placeholder based on error in MainController)
    /// </summary>
    // Overload or adjust based on actual usage in MainController
    public void SetReferences(MorphologyGenerator morphGen, AdaptationSystem adaptSys, GrowthSystem growthSys, ScenarioAnalyzer scenarioAn, EnvironmentManager envMan)
    {
        Debug.LogWarning("UIManager.SetReferences() called with 5 arguments.");
        // TODO: Implement logic to store or use these references.
    }

    #endregion

    #region Private Methods - UI Management
    /// <summary>
    /// Shows the specified panel and hides others
    /// </summary>
    /// <param name="panel">Panel to show</param>
    private void ShowPanel(GameObject panel)
    {
        if (panel == null) return;
        // Hide current panel
        if (currentPanel != null)
        {
            currentPanel.SetActive(false);
        }
        // Show new panel
        panel.SetActive(true);
        currentPanel = panel;
        // Update tab button highlighting
        UpdateTabHighlighting(panel);
    }

    /// <summary>
    /// Updates the highlighting of tab buttons based on the active panel
    /// </summary>
    /// <param name="activePanel">Currently active panel</param>
    private void UpdateTabHighlighting(GameObject activePanel)
    {
        // Reset all buttons
        if (siteTabButton != null) siteTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        if (zoneTabButton != null) zoneTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        if (morphologyTabButton != null) morphologyTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        if (scenarioTabButton != null) scenarioTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        if (resultsTabButton != null) resultsTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        if (settingsTabButton != null) settingsTabButton.GetComponent<Image>().color = new Color(0.7f, 0.7f, 0.7f);
        // Highlight active button
        if (activePanel == siteSelectionPanel && siteTabButton != null)
            siteTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        else if (activePanel == zoneSelectionPanel && zoneTabButton != null)
            zoneTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        else if (activePanel == morphologyControlPanel && morphologyTabButton != null)
            morphologyTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        else if (activePanel == scenarioPanel && scenarioTabButton != null)
            scenarioTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        else if (activePanel == resultsPanel && resultsTabButton != null)
            resultsTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
        else if (activePanel == settingsPanel && settingsTabButton != null)
            settingsTabButton.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.3f);
    }

    /// <summary>
    /// Updates button interactability based on simulation state
    /// </summary>
    /// <param name="state">Current simulation state</param>
    private void UpdateButtonStates(MorphologyManager.SimulationState state)
    {
        // Initially disable progression buttons
        if (zoneTabButton != null) zoneTabButton.interactable = false;
        if (morphologyTabButton != null) morphologyTabButton.interactable = false;
        if (scenarioTabButton != null) scenarioTabButton.interactable = false;
        if (resultsTabButton != null) resultsTabButton.interactable = false;
        
        // Enable buttons based on state
        switch (state)
        {
            case MorphologyManager.SimulationState.ZoneSelection:
                if (zoneTabButton != null) zoneTabButton.interactable = true;
                break;
                
            case MorphologyManager.SimulationState.MorphologyGeneration:
            case MorphologyManager.SimulationState.ScenarioAnalysis:
                if (zoneTabButton != null) zoneTabButton.interactable = true;
                if (morphologyTabButton != null) morphologyTabButton.interactable = true;
                break;
                
            case MorphologyManager.SimulationState.Idle:
                if (MorphologyManager.Instance != null)
                {
                    SiteGenerator siteGenerator = FindObjectOfType<SiteGenerator>();
                    if (siteGenerator != null && siteGenerator.GetSiteBounds().size != Vector3.zero)
                    {
                        if (zoneTabButton != null) zoneTabButton.interactable = true;
                    }
                    if (selectedZone.size != Vector3.zero)
                    {
                        if (morphologyTabButton != null) morphologyTabButton.interactable = true;
                    }
                    if (savedMorphologies.Count > 0)
                    {
                        if (scenarioTabButton != null) scenarioTabButton.interactable = true;
                    }
                    ScenarioAnalyzer scenarioAnalyzer = FindObjectOfType<ScenarioAnalyzer>();
                    if (scenarioAnalyzer != null && scenarioAnalyzer.GetResults() != null)
                    {
                        if (resultsTabButton != null) resultsTabButton.interactable = true;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Updates progress indicators based on current state
    /// </summary>
    private void UpdateProgressIndicators()
    {
        if (currentState == MorphologyManager.SimulationState.MorphologyGeneration)
        {
            MorphologyGenerator generator = FindObjectOfType<MorphologyGenerator>();
            if (generator != null)
            {
                UpdateGenerationProgress(generator.GenerationProgress);
            }
        }
        else if (currentState == MorphologyManager.SimulationState.ScenarioAnalysis)
        {
            ScenarioAnalyzer analyzer = FindObjectOfType<ScenarioAnalyzer>();
            if (analyzer != null)
            {
                UpdateScenarioProgress(analyzer.AnalysisProgress);
            }
        }
    }

    /// <summary>
    /// Updates the zone gizmo to match the selected zone
    /// </summary>
    /// <param name="zone">Zone bounds</param>
    private void UpdateZoneGizmo(Bounds zone)
    {
        zoneGizmo.transform.position = zone.center;
        LineRenderer lineRenderer = zoneGizmo.GetComponentInChildren<LineRenderer>();
        if (lineRenderer != null)
        {
            Vector3 extents = zone.extents;
            Vector3[] corners = new Vector3[8];
            corners[0] = new Vector3(-extents.x, -extents.y, -extents.z) + zone.center;
            corners[1] = new Vector3(extents.x, -extents.y, -extents.z) + zone.center;
            corners[2] = new Vector3(extents.x, -extents.y, extents.z) + zone.center;
            corners[3] = new Vector3(-extents.x, -extents.y, extents.z) + zone.center;
            corners[4] = new Vector3(-extents.x, extents.y, -extents.z) + zone.center;
            corners[5] = new Vector3(extents.x, extents.y, -extents.z) + zone.center;
            corners[6] = new Vector3(extents.x, extents.y, extents.z) + zone.center;
            corners[7] = new Vector3(-extents.x, extents.y, extents.z) + zone.center;
            int index = 0;
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[4]);
            lineRenderer.SetPosition(index++, corners[5]);
            lineRenderer.SetPosition(index++, corners[5]);
            lineRenderer.SetPosition(index++, corners[6]);
            lineRenderer.SetPosition(index++, corners[6]);
            lineRenderer.SetPosition(index++, corners[7]);
            lineRenderer.SetPosition(index++, corners[7]);
            lineRenderer.SetPosition(index++, corners[4]);
            lineRenderer.SetPosition(index++, corners[0]);
            lineRenderer.SetPosition(index++, corners[4]);
            lineRenderer.SetPosition(index++, corners[1]);
            lineRenderer.SetPosition(index++, corners[5]);
            lineRenderer.SetPosition(index++, corners[2]);
            lineRenderer.SetPosition(index++, corners[6]);
            lineRenderer.SetPosition(index++, corners[3]);
            lineRenderer.SetPosition(index++, corners[7]);
        }
    }

    /// <summary>
    /// Adds a newly generated morphology to the selection dropdowns
    /// </summary>
    /// <param name="id">Morphology ID</param>
    private void AddMorphologyToDropdowns(string id)
    {
        // Implementation would depend on your UI design
    }

    /// <summary>
    /// Clears all graphs from the results panel
    /// </summary>
    private void ClearGraphs()
    {
        foreach (var graph in activeGraphs)
        {
            if (graph != null)
            {
                Destroy(graph);
            }
        }
        
        activeGraphs.Clear();
    }

    /// <summary>
    /// Displays metrics in the results panel
    /// </summary>
    /// <param name="results">Analysis results</param>
    private void DisplayMetrics(ScenarioResults results)
    {
        if (resultsText == null || results == null) return;
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("Scenario Analysis Results:");
        sb.AppendLine("------------------------");
        sb.AppendLine($"Scenario: {results.scenarioId}");
        sb.AppendLine($"Adaptation Successful: {(results.adaptationSuccessful ? "Yes" : "No")}");
        sb.AppendLine();
        
        sb.AppendLine("Metrics:");
        
        foreach (var metric in results.metrics)
        {
            sb.AppendLine($"{metric.Key}: {metric.Value:F3}");
        }
        
        resultsText.text = sb.ToString();
    }

    /// <summary>
    /// Creates graphs for time series data
    /// </summary>
    /// <param name="timeSeriesData">Time series data from analysis</param>
    private void CreateGraphs(Dictionary<string, List<float>> timeSeriesData)
    {
        if (graphContainer == null || graphPrefab == null || timeSeriesData == null) return;
        
        // Limit to the most important metrics for display
        string[] keysToDisplay = new string[]
        {
            "AverageStress",
            "MaxDisplacement",
            "AverageConnectivity"
        };
        
        int graphCount = 0;
        
        foreach (string key in keysToDisplay)
        {
            if (timeSeriesData.ContainsKey(key))
            {
                // Create graph object
                GameObject graphObject = Instantiate(graphPrefab, graphContainer);
                graphObject.name = $"Graph_{key}";
                
                // Position the graph
                RectTransform graphRect = graphObject.GetComponent<RectTransform>();
                if (graphRect != null)
                {
                    float graphHeight = 150f;
                    float spacing = 20f;
                    graphRect.anchoredPosition = new Vector2(0, -(graphHeight + spacing) * graphCount);
                }
                
                // Set up the graph
                LineGraph graph = graphObject.GetComponent<LineGraph>();
                if (graph != null)
                {
                    graph.SetTitle(key);
                    graph.SetData(timeSeriesData[key].ToArray());
                }
                
                activeGraphs.Add(graphObject);
                graphCount++;
            }
        }
    }
    
    /// <summary>
    /// Displays observations in the results panel
    /// </summary>
    /// <param name="observations">List of observations</param>
    private void DisplayObservations(List<string> observations)
    {
        if (observationsText == null || observations == null) return;
        
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        
        sb.AppendLine("Observations:");
        sb.AppendLine("-------------");
        
        foreach (string observation in observations)
        {
            sb.AppendLine($"• {observation}");
        }
        
        observationsText.text = sb.ToString();
    }
    #endregion
}