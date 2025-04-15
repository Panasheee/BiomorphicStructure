using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;
using BiomorphicSim.Core;

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

    
    public void SetupPresetZones() { Debug.Log("Placeholder: SetupZones called"); }
    public void UpdateZoneSelection(Bounds zone) { Debug.Log($"Placeholder: UpdateZoneSelection called with zone: {zone}"); }
    public void UpdateUI(MorphologyManager.SimulationState state)
{
    Debug.Log($"Placeholder: UpdateUI called with state: {state}");
    // TODO: Update UI based on state
}

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
    #endregion

    #region Event Handlers - Buttons
    // Placeholder methods to resolve CS0103 errors
    private void OnGenerateSiteClicked()
    {
        Debug.LogWarning("OnGenerateSiteClicked not fully implemented.");
        // TODO: Call appropriate method in SiteManager or MorphologyManager
    }

    private void OnSelectRandomZoneClicked()
    {
        Debug.LogWarning("OnSelectRandomZoneClicked not fully implemented.");
        // TODO: Call appropriate method in ZoneSelectionManager or MorphologyManager
    }

    private void OnConfirmZoneClicked()
    {
        Debug.LogWarning("OnConfirmZoneClicked not fully implemented.");
        // TODO: Call appropriate method in MorphologyManager to confirm zone
        ShowPanel(morphologyControlPanel); // Example navigation
    }

    private void OnGenerateMorphologyClicked()
    {
        Debug.LogWarning("OnGenerateMorphologyClicked not fully implemented.");
        // TODO: Gather parameters from UI and call MorphologyManager.GenerateMorphology
        MorphologyParameters parameters = GetCurrentMorphologyParameters();
        MorphologyManager.Instance.GenerateMorphology(selectedZone, parameters);
    }

    private void OnResetMorphologyClicked()
    {
        Debug.LogWarning("OnResetMorphologyClicked not fully implemented.");
        // TODO: Call MorphologyManager.ResetMorphology
        MorphologyManager.Instance.ResetMorphology();
    }

    private void OnRunScenarioClicked()
    {
        Debug.LogWarning("OnRunScenarioClicked not fully implemented.");
        // TODO: Gather scenario settings and call ScenarioManager.RunScenario
        ScenarioParameters scenarioParams = GetCurrentScenarioParameters();
        ScenarioManager.Instance.RunScenario(scenarioParams);
    }

    private void OnStopScenarioClicked()
    {
        Debug.LogWarning("OnStopScenarioClicked not fully implemented.");
        // TODO: Call ScenarioManager.StopScenario
        ScenarioManager.Instance.StopScenario();
    }

    private void OnAddFactorClicked()
    {
        Debug.LogWarning("OnAddFactorClicked not fully implemented.");
        // TODO: Implement adding new environmental factor UI element
        AddEnvironmentalFactorUI();
    }

    private void OnExportResultsClicked()
    {
        Debug.LogWarning("OnExportResultsClicked not fully implemented.");
        // TODO: Call ResultsManager or DataIO to export results
        ResultsManager.Instance.ExportResults();
    }

    private void OnApplySettingsClicked()
    {
        Debug.LogWarning("OnApplySettingsClicked not fully implemented.");
        // TODO: Apply settings from the settings panel
        ApplySimulationSettings();
    }

    private void OnScreenshotClicked()
    {
        Debug.LogWarning("OnScreenshotClicked not fully implemented.");
        // TODO: Call a utility method to capture screenshot
        ScreenCaptureUtility.CaptureScreenshot();
    }
    #endregion

    #region Event Handlers - Sliders & Toggles
    // Placeholder methods to resolve CS0103 errors
    private void OnZoneSizeChanged(float value) // Assuming single handler for all 3 sliders
    {
        Debug.LogWarning("OnZoneSizeChanged not fully implemented.");
        // TODO: Update selectedZone bounds based on slider values
        UpdateSelectedZoneFromSliders();
    }

    private void OnCameraSpeedChanged(float value)
    {
        Debug.LogWarning("OnCameraSpeedChanged not fully implemented.");
        cameraSpeed = value;
        // TODO: Update camera controller speed if separate component exists
    }

    private void OnAutoRotateChanged(bool value)
    {
        Debug.LogWarning("OnAutoRotateChanged not fully implemented.");
        autoRotate = value;
    }

    private void OnTransparencyChanged(float value)
    {
        Debug.LogWarning("OnTransparencyChanged not fully implemented.");
        // TODO: Call VisualizationManager to set transparency
        VisualizationManager.Instance.SetGlobalTransparency(value);
    }

    private void OnShowStressColorsChanged(bool value)
    {
        Debug.LogWarning("OnShowStressColorsChanged not fully implemented.");
        // TODO: Call VisualizationManager to toggle stress colors
        VisualizationManager.Instance.SetStressColorVisibility(value);
    }

    private void OnShowForcesChanged(bool value)
    {
        Debug.LogWarning("OnShowForcesChanged not fully implemented.");
        // TODO: Call VisualizationManager to toggle force vectors
        VisualizationManager.Instance.SetForceVectorVisibility(value);
    }
    #endregion

    #region Event Handlers - Dropdowns
    // Placeholder method to resolve CS0103 error
    private void OnScenarioChanged(int index)
    {
        Debug.LogWarning("OnScenarioChanged not fully implemented.");
        // TODO: Update scenario panel based on selected scenario preset
        string selectedScenario = scenarioDropdown.options[index].text;
        UpdateScenarioPanelForPreset(selectedScenario);
    }
    #endregion

    #region Helper Methods for Event Handlers (Placeholders)

    private MorphologyParameters GetCurrentMorphologyParameters()
    {
        // TODO: Read values from densitySlider, complexitySlider, etc.
        Debug.LogWarning("GetCurrentMorphologyParameters reading default values.");
        return new MorphologyParameters {
            biomorphType = (MorphologyParameters.BiomorphType)biomorphTypeDropdown.value,
            density = densitySlider.value,
            complexity = complexitySlider.value,
            connectivity = connectivitySlider.value
            // Add other parameters
        };
    }

    private ScenarioParameters GetCurrentScenarioParameters()
    {
        // TODO: Read values from scenario panel controls
        Debug.LogWarning("GetCurrentScenarioParameters reading default values.");
        return new ScenarioParameters(); // Replace with actual parameters
    }

    private void AddEnvironmentalFactorUI()
    {
        // TODO: Instantiate environmentalFactorPrefab and add to scroll view
        Debug.LogWarning("AddEnvironmentalFactorUI not implemented.");
        if (environmentalFactorPrefab != null && environmentalFactorsScrollView != null)
        {
            // GameObject factorGO = Instantiate(environmentalFactorPrefab, environmentalFactorsScrollView.content);
            // EnvironmentalFactorUI factorUI = factorGO.GetComponent<EnvironmentalFactorUI>();
            // if (factorUI != null)
            // {
            //     activeFactors.Add(factorUI);
            //     // Initialize factorUI
            // }
        }
    }

    private void ApplySimulationSettings()
    {
        // TODO: Read values from timeStepSlider, maxIterationsSlider, etc. and apply them
        Debug.LogWarning("ApplySimulationSettings not implemented.");
        // Example: SimulationSettings.TimeStep = timeStepSlider.value;
    }

    private void UpdateSelectedZoneFromSliders()
    {
        // TODO: Read slider values and update selectedZone & gizmo
        Debug.LogWarning("UpdateSelectedZoneFromSliders not implemented.");
        if (zoneWidthSlider != null && zoneHeightSlider != null && zoneDepthSlider != null)
        {
            // Vector3 newSize = new Vector3(zoneWidthSlider.value, zoneHeightSlider.value, zoneDepthSlider.value);
            // selectedZone.size = newSize;
            // UpdateZoneSelection(selectedZone); // Update gizmo and text
        }
    }

     private void UpdateScenarioPanelForPreset(string scenarioName)
    {
        // TODO: Load preset settings into the scenario panel UI elements
        Debug.LogWarning($"UpdateScenarioPanelForPreset for '{scenarioName}' not implemented.");
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
    /// <param name=\"timeSeriesData\">Time series data from analysis</param>
    private void CreateGraphs(Dictionary<string, List<float>> timeSeriesData)
    {
        if (graphContainer == null || graphPrefab == null || timeSeriesData == null) return;

        ClearGraphs(); // Clear previous graphs

        // Limit to the most important metrics for display
        string[] keysToDisplay = new string[]
        {
            "AverageStress",
            "MaxDisplacement",
            "AverageConnectivity"
            // Add other relevant keys as needed
        };

        int graphCount = 0;

        foreach (string key in keysToDisplay)
        {
            if (timeSeriesData.ContainsKey(key))
            {
                // Create graph object
                GameObject graphObject = Instantiate(graphPrefab, graphContainer);
                // Fix interpolated string literal: remove extra escapes
                graphObject.name = $"Graph_{key}";

                // Position the graph
                RectTransform graphRect = graphObject.GetComponent<RectTransform>();
                if (graphRect != null)
                {
                    float graphHeight = 150f; // Adjust as needed
                    float spacing = 20f; // Adjust as needed
                    graphRect.anchoredPosition = new Vector2(0, -(graphHeight + spacing) * graphCount);
                }

                // Set up the graph
                LineGraph graph = graphObject.GetComponent<LineGraph>(); // CS0246 here
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
    #endregion
} // End of UIManager class

// Placeholder for LineGraph to resolve CS0246. Replace with actual implementation or using directive.
public class LineGraph : MonoBehaviour
{
    public void SetTitle(string title) { Debug.Log($"Graph Title Set: {title}"); }
    public void SetData(float[] data) { Debug.Log($"Graph Data Set: {data.Length} points"); }
}

 
public static class ScreenCaptureUtility
{
    public static void CaptureScreenshot() { Debug.Log("Placeholder: CaptureScreenshot called"); }
}

// Placeholder structures/classes if not defined elsewhere
[System.Serializable]
public class ScenarioParameters { /* Add fields as needed */ }


