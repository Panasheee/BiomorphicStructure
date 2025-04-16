using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Simplified, modern UI system implementing the mockup design for the biomorphic simulator.
    /// Focuses on a minimalist, step-by-step flow from site loading to morphology generation.
    /// </summary>
    public class ModernUIController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private BiomorphicSim.Core.MainController mainController;
        [SerializeField] private BiomorphicSim.Core.SiteGenerator siteGenerator;
        [SerializeField] private BiomorphicSim.Morphology.MorphologyManager morphologyManager;
        [SerializeField] private BiomorphicSim.Core.ScenarioAnalyzer scenarioAnalyzer;
        [SerializeField] private BiomorphicSim.Core.VisualizationManager visualizationManager;
        
        // New Initialize method to receive references from MainController
        public void Initialize(BiomorphicSim.Core.MainController controller, 
                             BiomorphicSim.Core.SiteGenerator generator,
                             BiomorphicSim.Morphology.MorphologyManager morphology,
                             BiomorphicSim.Core.ScenarioAnalyzer analyzer,
                             BiomorphicSim.Core.VisualizationManager visualization)
        {
            mainController = controller;
            siteGenerator = generator;
            morphologyManager = morphology;
            scenarioAnalyzer = analyzer;
            visualizationManager = visualization;
            
            Debug.Log("ModernUIController initialized with all references");
        }
        
        [Header("UI Screens")]
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject siteViewerScreen;
        [SerializeField] private GameObject siteAnalysisScreen;
        [SerializeField] private GameObject morphologyScreen;
        [SerializeField] private GameObject scenarioScreen;
        
        [Header("UI Components")]
        [SerializeField] private Button loadSiteButton;
        [SerializeField] private Button runAnalysisButton;
        [SerializeField] private Button startMorphologyButton;
        [SerializeField] private Button runScenarioButton;
        [SerializeField] private Button resetButton;
        
        [Header("Analysis Overlays")]
        [SerializeField] private Toggle windOverlayToggle;
        [SerializeField] private Toggle sunExposureToggle;
        [SerializeField] private Toggle pedestrianOverlayToggle;
        
        [Header("Analysis Report")]
        [SerializeField] private TextMeshProUGUI analysisReportText;
        
        [Header("Morphology Controls")]
        [SerializeField] private Slider growthSpeedSlider;
        [SerializeField] private Button pauseResumeButton;
        [SerializeField] private TextMeshProUGUI nodeCountText;
        
        [Header("Scenario Selection")]
        [SerializeField] private TMP_Dropdown scenarioDropdown;
        
        // Tracking state
        private bool isMorphologyRunning = false;
        private bool isSiteZoneSelected = false;
        private Vector3 selectedZonePosition;
        private Coroutine updateUICoroutine;
        
        private void Start()
        {
            // Set up UI events
            SetupUIEvents();
            
            // Start with only the start screen active
            ShowOnlyScreen(startScreen);
            
            // Start UI update coroutine
            updateUICoroutine = StartCoroutine(UpdateUIValues());
        }
        
        private void SetupUIEvents()
        {
            // Start screen
            if (loadSiteButton != null)
                loadSiteButton.onClick.AddListener(OnLoadSiteClicked);
            
            // Site viewer screen
            if (runAnalysisButton != null)
                runAnalysisButton.onClick.AddListener(OnRunAnalysisClicked);
            
            // Analysis screen
            if (windOverlayToggle != null)
                windOverlayToggle.onValueChanged.AddListener(OnWindOverlayToggled);
            
            if (sunExposureToggle != null)
                sunExposureToggle.onValueChanged.AddListener(OnSunExposureToggled);
            
            if (pedestrianOverlayToggle != null)
                pedestrianOverlayToggle.onValueChanged.AddListener(OnPedestrianOverlayToggled);
            
            if (startMorphologyButton != null)
                startMorphologyButton.onClick.AddListener(OnStartMorphologyClicked);
            
            // Morphology screen
            if (growthSpeedSlider != null)
                growthSpeedSlider.onValueChanged.AddListener(OnGrowthSpeedChanged);
            
            if (pauseResumeButton != null)
                pauseResumeButton.onClick.AddListener(OnPauseResumeClicked);
            
            if (runScenarioButton != null)
                runScenarioButton.onClick.AddListener(OnRunScenarioClicked);
            
            // Scenario screen
            if (scenarioDropdown != null)
            {
                // Populate with available scenarios
                PopulateScenarioDropdown();
                scenarioDropdown.onValueChanged.AddListener(OnScenarioSelected);
            }
            
            // Common buttons
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
        }
        
        #region Button Handlers
        
        private void OnLoadSiteClicked()
        {
            Debug.Log("Loading Lambton Quay site...");
            
            // Generate the default site (Lambton Quay)
            siteGenerator.GenerateDefaultSite();
            
            // Switch to site viewer screen
            ShowOnlyScreen(siteViewerScreen);
            
            // Set camera to focus on the site
            visualizationManager.FocusOnSite();
        }
        
        private void OnRunAnalysisClicked()
        {
            Debug.Log("Running site analysis...");
            
            // Run environmental analysis
            RunSiteAnalysis();
            
            // Switch to analysis screen
            ShowOnlyScreen(siteAnalysisScreen);
        }
        
        private void OnStartMorphologyClicked()
        {
            Debug.Log("Starting morphology generation...");
            
            // If zone not selected, prompt user to select one
            if (!isSiteZoneSelected)
            {
                Debug.Log("Please select a zone first");
                // In a real app, we'd show a prompt or highlight the interaction
                return;
            }
            
            // Set morphology starting position to selected zone
            morphologyManager.SetStartPosition(selectedZonePosition);
            
            // Start the morphology growth
            mainController.StartSimulation();
            isMorphologyRunning = true;
            
            // Switch to morphology screen
            ShowOnlyScreen(morphologyScreen);
            
            // Update button text
            if (pauseResumeButton != null)
                pauseResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = "PAUSE";
        }
        
        private void OnPauseResumeClicked()
        {
            if (isMorphologyRunning)
            {
                // Pause the simulation
                mainController.PauseSimulation();
                isMorphologyRunning = false;
                
                // Update button text
                if (pauseResumeButton != null)
                    pauseResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = "RESUME";
            }
            else
            {
                // Resume the simulation
                mainController.StartSimulation();
                isMorphologyRunning = true;
                
                // Update button text
                if (pauseResumeButton != null)
                    pauseResumeButton.GetComponentInChildren<TextMeshProUGUI>().text = "PAUSE";
            }
        }
        
        private void OnRunScenarioClicked()
        {
            if (scenarioDropdown != null && scenarioDropdown.options.Count > 0)
            {
                string selectedScenario = scenarioDropdown.options[scenarioDropdown.value].text;
                Debug.Log($"Running scenario: {selectedScenario}");
                
                // Run the selected scenario
                mainController.RunScenario(selectedScenario);
                
                // Switch to scenario screen
                ShowOnlyScreen(scenarioScreen);
            }
        }
        
        private void OnResetClicked()
        {
            Debug.Log("Resetting simulation...");
            
            // Reset the simulation
            mainController.ResetSimulation();
            isMorphologyRunning = false;
            isSiteZoneSelected = false;
            
            // Switch to start screen
            ShowOnlyScreen(startScreen);
        }
        
        #endregion
        
        #region Toggle Handlers
        
        private void OnWindOverlayToggled(bool isOn)
        {
            // Toggle wind overlay
            if (visualizationManager != null)
                visualizationManager.ToggleWindOverlay(isOn);
        }
        
        private void OnSunExposureToggled(bool isOn)
        {
            // Toggle sun exposure overlay
            if (visualizationManager != null)
                visualizationManager.ToggleSunExposureOverlay(isOn);
        }
        
        private void OnPedestrianOverlayToggled(bool isOn)
        {
            // Toggle pedestrian overlay
            if (visualizationManager != null)
                visualizationManager.TogglePedestrianOverlay(isOn);
        }
        
        #endregion
        
        #region Dropdown Handlers
        
        private void PopulateScenarioDropdown()
        {
            if (scenarioDropdown != null && scenarioAnalyzer != null)
            {
                // Get available scenarios from the analyzer
                List<string> scenarios = scenarioAnalyzer.GetAvailableScenarios();
                
                // Add custom scenarios
                scenarios.Add("High Wind");
                scenarios.Add("Low Sunlight");
                scenarios.Add("Weekend Crowds");
                
                // Populate dropdown
                scenarioDropdown.ClearOptions();
                scenarioDropdown.AddOptions(scenarios);
            }
        }
        
        private void OnScenarioSelected(int index)
        {
            // Handle scenario selection
            if (scenarioDropdown != null && index < scenarioDropdown.options.Count)
            {
                string selectedScenario = scenarioDropdown.options[index].text;
                Debug.Log($"Selected scenario: {selectedScenario}");
                
                // Update UI or preview based on selected scenario
                UpdateScenarioDescription(selectedScenario);
            }
        }
        
        private void UpdateScenarioDescription(string scenarioName)
        {
            // Update description based on scenario
            TextMeshProUGUI descText = scenarioScreen.GetComponentInChildren<TextMeshProUGUI>();
            if (descText != null)
            {
                switch (scenarioName)
                {
                    case "High Wind":
                        descText.text = "Simulates strong wind conditions from the north.";
                        break;
                    case "Low Sunlight":
                        descText.text = "Simulates winter sunlight conditions with lower intensity.";
                        break;
                    case "Weekend Crowds":
                        descText.text = "Simulates increased pedestrian activity during weekends.";
                        break;
                    default:
                        descText.text = $"Scenario: {scenarioName}";
                        break;
                }
            }
        }
        
        #endregion
        
        #region Slider Handlers
        
        private void OnGrowthSpeedChanged(float value)
        {
            // Update growth speed
            if (mainController != null)
                mainController.SetSimulationSpeed(value);
        }
        
        #endregion
        
        #region Site Interaction
        
        /// <summary>
        /// Called when the user clicks on the terrain to select a zone
        /// </summary>
        public void OnSiteClicked(Vector3 position)
        {
            // Set the selected zone position
            selectedZonePosition = position;
            isSiteZoneSelected = true;
            
            // Visualize the selected zone
            VisualizeSelectedZone(position);
            
            Debug.Log($"Selected zone at position: {position}");
        }
        
        private void VisualizeSelectedZone(Vector3 position)
        {
            // Create or update a visual indicator for the selected zone
            GameObject zoneIndicator = GameObject.Find("SelectedZoneIndicator");
            if (zoneIndicator == null)
            {
                zoneIndicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                zoneIndicator.name = "SelectedZoneIndicator";
                
                // Create a material with highlight color
                Material highlightMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                highlightMaterial.color = new Color(1f, 1f, 1f, 0.5f);
                zoneIndicator.GetComponent<Renderer>().material = highlightMaterial;
            }
            
            // Position and scale the indicator
            zoneIndicator.transform.position = position;
            zoneIndicator.transform.localScale = Vector3.one * 5f;
        }
        
        #endregion
        
        #region Analysis
        
        private void RunSiteAnalysis()
        {
            // Run analysis on the site
            // This would call into your actual analysis system
            
            // For now, just populate a sample report
            if (analysisReportText != null)
            {
                string report = "SITE ANALYSIS REPORT\n\n";
                report += "Wind: Moderate from the north\n";
                report += "Sun Exposure: High on east side\n";
                report += "Pedestrian Activity: High along Lambton Quay\n";
                report += "Slope Analysis: Steep on western hill\n";
                
                analysisReportText.text = report;
            }
        }
        
        #endregion
        
        #region UI Utilities
        
        private void ShowOnlyScreen(GameObject screenToShow)
        {
            // Hide all screens
            if (startScreen != null) startScreen.SetActive(false);
            if (siteViewerScreen != null) siteViewerScreen.SetActive(false);
            if (siteAnalysisScreen != null) siteAnalysisScreen.SetActive(false);
            if (morphologyScreen != null) morphologyScreen.SetActive(false);
            if (scenarioScreen != null) scenarioScreen.SetActive(false);
            
            // Show the requested screen
            if (screenToShow != null)
                screenToShow.SetActive(true);
        }
        
        private IEnumerator UpdateUIValues()
        {
            while (true)
            {
                // Update node count
                if (nodeCountText != null && morphologyManager != null)
                {
                    int count = morphologyManager.GetNodeCount();
                    nodeCountText.text = $"NODES: {count}";
                }
                
                yield return new WaitForSeconds(0.5f);
            }
        }
        
        private void OnDestroy()
        {
            if (updateUICoroutine != null)
                StopCoroutine(updateUICoroutine);
        }
        
        #endregion
    }
}