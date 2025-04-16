using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
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
        [SerializeField] private GameObject arcgisMapObject; // Reference to the ArcGIS Map GameObject
        [SerializeField] private GameObject siteTerrain; // Reference to the original site terrain with magenta material
        
        [Header("UI Screens")]
        [SerializeField] private GameObject startScreen;
        [SerializeField] private GameObject siteViewerScreen;
        [SerializeField] private GameObject siteAnalysisScreen;
        [SerializeField] private GameObject morphologyScreen;
        [SerializeField] private GameObject scenarioScreen;
        
        // Reference to the parent UICanvas that contains all screens
        private GameObject uiCanvasObject;
        
        // Flag to prevent duplicate initialization
        private bool uiInitialized = false;
        
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
        
        // Find the UICanvas in the hierarchy
        FindUICanvas();
        
        Debug.Log("ModernUIController initialized with all references");
    }    
    /// <summary>
    /// Finds the UICanvas in the hierarchy and assigns references to all UI screens
    /// </summary>
    private void FindUICanvas()
    {
        // First look for UICanvas as a child of this GameObject
        uiCanvasObject = transform.Find("UICanvas")?.gameObject;
        
        if (uiCanvasObject == null)
        {
            // If not found, search in the entire scene
            uiCanvasObject = GameObject.Find("UICanvas");
            
            if (uiCanvasObject == null)
            {
                Debug.LogError("UICanvas not found in hierarchy. UI screens will not function correctly.");
                return;
            }
        }
        
        Debug.Log($"Found UICanvas: {uiCanvasObject.name}");
        
        // Now find all the UI screens as children of the UICanvas
        startScreen = uiCanvasObject.transform.Find("StartScreen")?.gameObject;
        siteViewerScreen = uiCanvasObject.transform.Find("SiteViewerScreen")?.gameObject;
        siteAnalysisScreen = uiCanvasObject.transform.Find("SiteAnalysisScreen")?.gameObject;
        morphologyScreen = uiCanvasObject.transform.Find("MorphologyScreen")?.gameObject;
        scenarioScreen = uiCanvasObject.transform.Find("ScenarioScreen")?.gameObject;
        
        // Log which screens were found
        Debug.Log($"Found UI screens - StartScreen: {startScreen != null}, " +
                 $"SiteViewerScreen: {siteViewerScreen != null}, " +
                 $"SiteAnalysisScreen: {siteAnalysisScreen != null}, " +
                 $"MorphologyScreen: {morphologyScreen != null}, " +
                 $"ScenarioScreen: {scenarioScreen != null}");
    }
          [Header("UI Components")]
        [SerializeField] private Button loadSiteButton;
        [SerializeField] private Button runAnalysisButton;
        [SerializeField] private Button startMorphologyButton;
        [SerializeField] private Button runScenarioButton;
        [SerializeField] private Button resetButton;
        [SerializeField] private Button backButton;
        
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
        private GameObject loadingScreen;
      private void Start()
    {
        // Only initialize once to prevent duplicate initialization
        if (uiInitialized) {
            Debug.Log("UI already initialized, skipping duplicate initialization");
            return;
        }
        
        // Make sure we have references to all UI components
        FindUICanvas();
        
        // Log that we've found (or not found) screens
        Debug.Log($"[STARTUP] UI screens status - StartScreen: {startScreen != null}, " +
                 $"SiteViewerScreen: {siteViewerScreen != null}, " +
                 $"SiteAnalysisScreen: {siteAnalysisScreen != null}");
        
        // Set up UI events
        SetupUIEvents();
        
        // Start with only the start screen active if it exists
        if (startScreen != null) {
            ShowOnlyScreen(startScreen);
            Debug.Log("Successfully showed start screen");
        } else {
            Debug.LogError("[STARTUP] Cannot show StartScreen - it was not found in the UICanvas!");
        }
        
        // Mark as initialized
        uiInitialized = true;
        
        // Start UI update coroutine
            updateUICoroutine = StartCoroutine(UpdateUIValues());
        }
        private void SetupUIEvents()
        {
            // Start screen
            if (loadSiteButton != null) 
            {
                // Clear existing listeners to avoid duplicates
                loadSiteButton.onClick.RemoveAllListeners();
                loadSiteButton.onClick.AddListener(OnLoadSiteClicked);
                
                // Add an EventTrigger component for more reliable detection
                EventTrigger eventTrigger = loadSiteButton.gameObject.GetComponent<EventTrigger>();
                if (eventTrigger == null)
                    eventTrigger = loadSiteButton.gameObject.AddComponent<EventTrigger>();
                    
                // Clear existing triggers
                eventTrigger.triggers.Clear();
                
                // Add pointer click event
                EventTrigger.Entry clickEntry = new EventTrigger.Entry();
                clickEntry.eventID = EventTriggerType.PointerClick;
                clickEntry.callback.AddListener((data) => { OnLoadSiteClicked(); });
                eventTrigger.triggers.Add(clickEntry);
                
                // Configure button navigation for improved focus handling
                ConfigureButtonNavigation(loadSiteButton);
            }
            
            // Site viewer screen
            if (runAnalysisButton != null) {
                runAnalysisButton.onClick.RemoveAllListeners();
                runAnalysisButton.onClick.AddListener(OnRunAnalysisClicked);
                ConfigureButtonNavigation(runAnalysisButton);
                
                // Add EventTrigger for reliability
                AddEventTrigger(runAnalysisButton.gameObject, EventTriggerType.PointerClick, 
                               (data) => { OnRunAnalysisClicked(); });
            }
            
            // Analysis screen
            if (windOverlayToggle != null)
                windOverlayToggle.onValueChanged.AddListener(OnWindOverlayToggled);
            
            if (sunExposureToggle != null)
                sunExposureToggle.onValueChanged.AddListener(OnSunExposureToggled);
                
            if (pedestrianOverlayToggle != null)
                pedestrianOverlayToggle.onValueChanged.AddListener(OnPedestrianOverlayToggled);
                
            if (startMorphologyButton != null) {
                startMorphologyButton.onClick.RemoveAllListeners();
                startMorphologyButton.onClick.AddListener(OnStartMorphologyClicked);
                ConfigureButtonNavigation(startMorphologyButton);
                
                // Add EventTrigger for reliability
                AddEventTrigger(startMorphologyButton.gameObject, EventTriggerType.PointerClick, 
                               (data) => { OnStartMorphologyClicked(); });
            }
            
            // Morphology screen
            if (pauseResumeButton != null) {
                pauseResumeButton.onClick.RemoveAllListeners();
                pauseResumeButton.onClick.AddListener(OnPauseResumeClicked);
                ConfigureButtonNavigation(pauseResumeButton);
                
                // Add EventTrigger for reliability
                AddEventTrigger(pauseResumeButton.gameObject, EventTriggerType.PointerClick, 
                               (data) => { OnPauseResumeClicked(); });
            }
            
            if (growthSpeedSlider != null)
                growthSpeedSlider.onValueChanged.AddListener(OnGrowthSpeedChanged);
                
            if (runScenarioButton != null) {
                runScenarioButton.onClick.RemoveAllListeners();
                runScenarioButton.onClick.AddListener(OnRunScenarioClicked);
                ConfigureButtonNavigation(runScenarioButton);
                
                // Add EventTrigger for reliability
                AddEventTrigger(runScenarioButton.gameObject, EventTriggerType.PointerClick, 
                               (data) => { OnRunScenarioClicked(); });
            }
            
            // Scenario screen
            if (scenarioDropdown != null) {
                scenarioDropdown.onValueChanged.AddListener(OnScenarioSelected);
                
                // Populate dropdown if needed
                if (scenarioDropdown.options.Count == 0) {
                    scenarioDropdown.ClearOptions();
                    scenarioDropdown.AddOptions(new List<string> { 
                        "Wind Scenario", 
                        "Light Scenario",
                        "High Wind",
                        "Low Sunlight",
                        "Weekend Crowds"
                    });
                }
            }
            
            // Common buttons
            if (resetButton != null)
                resetButton.onClick.AddListener(OnResetClicked);
                
            // Back button
            if (backButton != null) {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(OnBackClicked);
                ConfigureButtonNavigation(backButton);
                
                // Add EventTrigger for reliability
                AddEventTrigger(backButton.gameObject, EventTriggerType.PointerClick, 
                               (data) => { OnBackClicked(); });
            }
                
            Debug.Log("UI events connected successfully");
        }
          /// <summary>
        /// Helper method to configure button navigation for better focus handling
        /// </summary>
        private void ConfigureButtonNavigation(Button button)
        {
            if (button == null) return;
            
            // Set explicit navigation mode
            Navigation navigation = button.navigation;
            navigation.mode = Navigation.Mode.Explicit;
            button.navigation = navigation;
            
            // Make sure the button is interactable
            button.interactable = true;
            
            // Make sure the colors are set up correctly to give visual feedback
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 1f);
            colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            colors.colorMultiplier = 1.5f;
            colors.fadeDuration = 0.1f;
            button.colors = colors;
            
            // Ensure the button is in the correct hierarchy for raycasting
            Canvas canvas = button.GetComponentInParent<Canvas>();
            if (canvas != null)
            {
                // Make sure the canvas has a raycaster
                GraphicRaycaster raycaster = canvas.GetComponent<GraphicRaycaster>();
                if (raycaster == null)
                {
                    raycaster = canvas.gameObject.AddComponent<GraphicRaycaster>();
                    Debug.Log($"Added GraphicRaycaster to canvas for button: {button.name}");
                }
                
                // Configure the raycaster for better button interaction
                raycaster.ignoreReversedGraphics = false;
                raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            }
            else
            {
                Debug.LogWarning($"Button {button.name} is not under a Canvas, interaction might not work properly");
            }
        }
        
        /// <summary>
        /// Helper method to add event triggers to UI elements
        /// </summary>
        private void AddEventTrigger(GameObject targetObject, EventTriggerType triggerType, UnityEngine.Events.UnityAction<BaseEventData> action)
        {
            EventTrigger eventTrigger = targetObject.GetComponent<EventTrigger>();
            if (eventTrigger == null)
                eventTrigger = targetObject.AddComponent<EventTrigger>();
                
            // Create a new entry
            EventTrigger.Entry entry = new EventTrigger.Entry();
            entry.eventID = triggerType;
            entry.callback.AddListener(action);
            
            // Add the entry to the trigger list
            eventTrigger.triggers.Add(entry);
        }
        
        #region Button Handlers
          private void OnLoadSiteClicked()
        {
            Debug.Log("BiomorphicSim: Loading Lambton Quay site...");
            
            // Show a loading screen
            ShowLoadingScreen(true, "Loading site...");
            
            // Use coroutine to allow loading screen to appear before processing
            StartCoroutine(LoadSiteCoroutine());
        }
          private IEnumerator LoadSiteCoroutine()
        {
            Debug.Log("[DEBUG] LoadSiteCoroutine - Starting site generation");
            
            // Wait for a frame to let the loading screen render
            yield return null;
            
            // Handle the terrain visibility
            if (siteTerrain != null)
            {
                // Disable the original magenta terrain
                siteTerrain.SetActive(false);
                Debug.Log("[DEBUG] LoadSiteCoroutine - Disabled original site terrain");
            }
            
            // Enable the ArcGIS map if available
            if (arcgisMapObject != null)
            {
                arcgisMapObject.SetActive(true);
                Debug.Log("[DEBUG] LoadSiteCoroutine - Enabled ArcGIS map");
            }
            else
            {
                Debug.LogWarning("[DEBUG] LoadSiteCoroutine - ArcGIS map reference not set. Using original terrain.");
                
                // If ArcGIS map is not available, re-enable the original terrain
                if (siteTerrain != null)
                {
                    siteTerrain.SetActive(true);
                }
            }
            
            // Generate the default site (Lambton Quay)
            siteGenerator.GenerateDefaultSite();
            
            Debug.Log("[DEBUG] LoadSiteCoroutine - Site generation complete");
            
            // Wait a moment for the site to finish generating and for the ArcGIS map to load
            yield return new WaitForSeconds(1.0f);
            
            // Hide loading screen
            Debug.Log("[DEBUG] LoadSiteCoroutine - Hiding loading screen");
            ShowLoadingScreen(false);
            
            // Check if site viewer screen exists
            if (siteViewerScreen == null) 
            {
                Debug.LogError("[DEBUG] LoadSiteCoroutine - siteViewerScreen is NULL! Cannot transition.");
                yield break;
            }
            
            // Switch to site viewer screen
            Debug.Log("[DEBUG] LoadSiteCoroutine - Showing site viewer screen");
            ShowOnlyScreen(siteViewerScreen);
            
            // Set camera to focus on the site
            visualizationManager.FocusOnSite();
            
            Debug.Log("[DEBUG] LoadSiteCoroutine - Site loading sequence complete");
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
        
        private void OnBackClicked()
        {
            Debug.Log("Back button clicked");
            
            // Determine which screen is active and return to the appropriate previous screen
            if (siteViewerScreen != null && siteViewerScreen.activeSelf)
            {
                // From site viewer back to start screen
                ShowOnlyScreen(startScreen);
            }
            else if (siteAnalysisScreen != null && siteAnalysisScreen.activeSelf)
            {
                // From analysis screen back to site viewer
                ShowOnlyScreen(siteViewerScreen);
            }
            else if (morphologyScreen != null && morphologyScreen.activeSelf)
            {
                // From morphology screen back to site analysis
                ShowOnlyScreen(siteAnalysisScreen);
            }
            else if (scenarioScreen != null && scenarioScreen.activeSelf)
            {
                // From scenario screen back to morphology
                ShowOnlyScreen(morphologyScreen);
            }
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
            Debug.Log("Starting site analysis process...");
            
            // Show loading indicator if available
            ShowLoadingScreen(true, "Running analysis...");
            
            // For now, just populate a sample report
            if (analysisReportText != null)
            {
                string report = "SITE ANALYSIS REPORT\n\n";
                report += "Wind: Moderate from the north\n";
                report += "Sun Exposure: High on east side\n";
                report += "Pedestrian Activity: High along Lambton Quay\n";
                report += "Slope Analysis: Steep on western hill\n";
                report += "Analysis Time: " + System.DateTime.Now.ToString("HH:mm:ss") + "\n";
                
                analysisReportText.text = report;
                
                // Hide loading screen when analysis completes
                ShowLoadingScreen(false);
                
                Debug.Log("Site analysis completed successfully.");
            }
        }
        
        #endregion
        
        // Public accessors for UI screens
        public GameObject GetStartScreen() { return startScreen; }
        public GameObject GetSiteViewerScreen() { return siteViewerScreen; }
        public GameObject GetSiteAnalysisScreen() { return siteAnalysisScreen; }
        public GameObject GetMorphologyScreen() { return morphologyScreen; }
        public GameObject GetScenarioScreen() { return scenarioScreen; }
        
        // Public accessor for visualization manager
        public BiomorphicSim.Core.VisualizationManager GetVisualizationManager() { return visualizationManager; }
        
        #region UI Utilities
        
        public void ShowOnlyScreen(GameObject screenToShow)
        {
            Debug.Log($"[DEBUG] ShowOnlyScreen - Transitioning to {(screenToShow != null ? screenToShow.name : "NULL")}");
            
            // Check if any screens are null before trying to hide them
            if (startScreen == null) Debug.LogWarning("[DEBUG] ShowOnlyScreen - startScreen is null");
            if (siteViewerScreen == null) Debug.LogWarning("[DEBUG] ShowOnlyScreen - siteViewerScreen is null");
            if (siteAnalysisScreen == null) Debug.LogWarning("[DEBUG] ShowOnlyScreen - siteAnalysisScreen is null");
            if (morphologyScreen == null) Debug.LogWarning("[DEBUG] ShowOnlyScreen - morphologyScreen is null");
            if (scenarioScreen == null) Debug.LogWarning("[DEBUG] ShowOnlyScreen - scenarioScreen is null");
            
            // Hide all screens
            if (startScreen != null) startScreen.SetActive(false);
            if (siteViewerScreen != null) siteViewerScreen.SetActive(false);
            if (siteAnalysisScreen != null) siteAnalysisScreen.SetActive(false);
            if (morphologyScreen != null) morphologyScreen.SetActive(false);
            if (scenarioScreen != null) scenarioScreen.SetActive(false);
            
            // Show the requested screen
            if (screenToShow != null) {
                screenToShow.SetActive(true);
                Debug.Log($"[DEBUG] ShowOnlyScreen - {screenToShow.name} activated");
            }
            else {
                Debug.LogError("[DEBUG] ShowOnlyScreen - Cannot show null screen");
            }
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
        
    private void CreateLoadingScreen()
    {
        // Create loading screen if it doesn't exist
        if (loadingScreen == null)
        {
            // Make sure we have a reference to the UICanvas
            if (uiCanvasObject == null)
            {
                FindUICanvas();
                if (uiCanvasObject == null)
                {
                    Debug.LogError("Cannot create LoadingScreen: UICanvas not found.");
                    return;
                }
            }
            
            loadingScreen = new GameObject("LoadingScreen");
            loadingScreen.transform.SetParent(uiCanvasObject.transform, false);
            
            // Add RectTransform component (needed for UI positioning)
            RectTransform rectTransform = loadingScreen.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Set sorting order higher to ensure it appears on top
            Canvas parentCanvas = uiCanvasObject.GetComponent<Canvas>();
            if (parentCanvas != null)
            {
                parentCanvas.sortingOrder = 10; // Make sure it's on top of other UI
            }
                
                // Add background panel
                GameObject panel = new GameObject("Panel");
                panel.transform.SetParent(loadingScreen.transform, false);
                
                RectTransform panelRect = panel.AddComponent<RectTransform>();
                panelRect.anchorMin = Vector2.zero;
                panelRect.anchorMax = Vector2.one;
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                Image panelImage = panel.AddComponent<Image>();
                panelImage.color = new Color(0, 0, 0, 0.8f); // Semi-transparent black
                
                // Add loading text
                GameObject textObj = new GameObject("LoadingText");
                textObj.transform.SetParent(panel.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = new Vector2(0.5f, 0.5f);
                textRect.anchorMax = new Vector2(0.5f, 0.5f);
                textRect.sizeDelta = new Vector2(500, 100);
                textRect.anchoredPosition = Vector2.zero;
                
                TextMeshProUGUI loadingText = textObj.AddComponent<TextMeshProUGUI>();
                loadingText.text = "LOADING...";
                loadingText.fontSize = 48;
                loadingText.fontStyle = FontStyles.Bold;
                loadingText.alignment = TextAlignmentOptions.Center;
                loadingText.color = Color.white;
                
                // Create a spinning loading icon
                GameObject spinnerObj = new GameObject("LoadingSpinner");
                spinnerObj.transform.SetParent(panel.transform, false);
                
                RectTransform spinnerRect = spinnerObj.AddComponent<RectTransform>();
                spinnerRect.anchorMin = new Vector2(0.5f, 0.5f);
                spinnerRect.anchorMax = new Vector2(0.5f, 0.5f);
                spinnerRect.sizeDelta = new Vector2(100, 100);
                spinnerRect.anchoredPosition = new Vector2(0, -80);
                
                Image spinnerImage = spinnerObj.AddComponent<Image>();
                spinnerImage.color = Color.white;
                
                // Add a rotation animation component to the spinner
                LoadingSpinner spinner = spinnerObj.AddComponent<LoadingSpinner>();
                spinner.rotationSpeed = 300f; // degrees per second
                
                // Initially hide the loading screen
                loadingScreen.SetActive(false);
            }
        }
        
        public void ShowLoadingScreen(bool show, string message = "LOADING...")
        {
            // Create the loading screen if it doesn't exist
            if (loadingScreen == null)
            {
                CreateLoadingScreen();
            }
            
            // Set the loading message
            TextMeshProUGUI loadingText = loadingScreen.GetComponentInChildren<TextMeshProUGUI>();
            if (loadingText != null)
            {
                loadingText.text = message;
            }
            
            // Show or hide the loading screen
            loadingScreen.SetActive(show);
        }
    }
}