using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Creates and manages a modern, sleek UI system for the Biomorphic Structure Simulator.
    /// </summary>
    public class UIManager : MonoBehaviour
    {
        // References to other managers (same as original)
        private BiomorphicSim.Core.MainController mainController;
        private BiomorphicSim.Core.SiteGenerator siteGenerator;
        private BiomorphicSim.Morphology.MorphologyManager morphologyManager;
        private BiomorphicSim.Core.ScenarioAnalyzer scenarioAnalyzer;
        private BiomorphicSim.Core.VisualizationManager visualizationManager;
        
        // UI Components (created programmatically)
        private Canvas mainCanvas;
        private GameObject mainPanel;
        private GameObject siteSelectionPanel;
        private GameObject zoneSelectionPanel;
        private GameObject morphologyControlPanel;
        private GameObject scenarioPanel;
        private GameObject resultsPanel;
        private GameObject settingsPanel;
        
        // UI references for updates
        private TMP_Dropdown siteDropdown;
        private TMP_Dropdown growthAlgorithmDropdown;
        private TMP_Dropdown scenarioDropdown;
        private TextMeshProUGUI nodeCountText;
        private Image scenarioProgressBar;
        private TextMeshProUGUI scenarioStatusText;
        private TextMeshProUGUI resultsText;
        
        // Internal state
        private Coroutine updateUICoroutine;
        
        // Modern UI styling variables
        private Color backgroundColor = Color.black;
        private Color textColor = Color.white;
        private Color accentColor = new Color(1f, 1f, 1f, 0.8f);
        private Color subtleLineColor = new Color(0.2f, 0.2f, 0.2f, 0.1f); // Much more subtle dark gray
        private TMP_FontAsset modernFont; // Assign in inspector or load programmatically
        
        public void Initialize(BiomorphicSim.Core.MainController controller, 
                               BiomorphicSim.Core.SiteGenerator site, 
                               BiomorphicSim.Morphology.MorphologyManager morphology, 
                               BiomorphicSim.Core.ScenarioAnalyzer scenario, 
                               BiomorphicSim.Core.VisualizationManager visualization)
        {
            Debug.Log("Initializing Modern UI Manager...");
            
            // Store references
            mainController = controller;
            siteGenerator = site;
            morphologyManager = morphology;
            scenarioAnalyzer = scenario;
            visualizationManager = visualization;
            
            // Attempt to load a modern font if available
            modernFont = Resources.Load<TMP_FontAsset>("Fonts/Roboto-Regular SDF");
            
            // Create the UI system
            CreateModernUISystem();
            
            // Start UI update coroutine
            updateUICoroutine = StartCoroutine(UpdateUIValues());
        }
        
        private void CreateModernUISystem()
        {
            // Create Canvas - FIXED FOR PROPER INTERACTION
            GameObject canvasObject = new GameObject("Canvas");
            mainCanvas = canvasObject.AddComponent<Canvas>();
            mainCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
            
            // Add Canvas Scaler
            CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            
            // Ensure proper GraphicRaycaster for interaction
            GraphicRaycaster raycaster = canvasObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = true;
            raycaster.blockingObjects = GraphicRaycaster.BlockingObjects.None;
            
            // Create Event System if it doesn't exist
            if (FindObjectOfType<UnityEngine.EventSystems.EventSystem>() == null)
            {
                GameObject eventSystem = new GameObject("EventSystem");
                UnityEngine.EventSystems.EventSystem system = eventSystem.AddComponent<UnityEngine.EventSystems.EventSystem>();
                system.sendNavigationEvents = true;
                system.pixelDragThreshold = 10;
                
                UnityEngine.EventSystems.StandaloneInputModule inputModule = eventSystem.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
                inputModule.forceModuleActive = true;
                inputModule.inputActionsPerSecond = 10;
                inputModule.repeatDelay = 0.5f;
            }
            
            // Create Panels
            CreateModernMainPanel();
            CreateModernSiteSelectionPanel();
            CreateModernZoneSelectionPanel();
            CreateModernMorphologyControlPanel();
            CreateModernScenarioPanel();
            CreateModernResultsPanel();
            CreateModernSettingsPanel();
            
            // Show main panel, hide others
            ShowPanel(mainPanel);
            HidePanel(siteSelectionPanel);
            HidePanel(zoneSelectionPanel);
            HidePanel(morphologyControlPanel);
            HidePanel(scenarioPanel);
            HidePanel(resultsPanel);
            HidePanel(settingsPanel);
        }
        
        #region Modern Panel Creation Methods
        
        private void CreateModernMainPanel()
        {
            mainPanel = CreatePanel("MainPanel", mainCanvas.transform);
              // IMPROVED TITLE APPROACH - with better spacing and readability
            // Create a simple container for the title to control positioning
            GameObject titleContainer = new GameObject("TitleContainer");
            titleContainer.transform.SetParent(mainPanel.transform, false);
            RectTransform titleContainerRect = titleContainer.AddComponent<RectTransform>();
            titleContainerRect.anchorMin = new Vector2(0.5f, 1f);
            titleContainerRect.anchorMax = new Vector2(0.5f, 1f);
            titleContainerRect.pivot = new Vector2(0.5f, 1f);
            titleContainerRect.anchoredPosition = new Vector2(0, -120);
            titleContainerRect.sizeDelta = new Vector2(800, 300); // Taller container for better spacing
            
            // Create a single, clean title text component with improved spacing
            TextMeshProUGUI titleText = titleContainer.AddComponent<TextMeshProUGUI>();            titleText.text = "BIOMORPHIC\n\nSTRUCTURE\n\nSIMULATOR"; // Added extra line breaks for better spacing
            titleText.fontSize = 72;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.enableWordWrapping = false; // Prevent unexpected wrapping
            titleText.overflowMode = TextOverflowModes.Overflow; // Ensure text doesn't get cut off
            titleText.lineSpacing = -20; // Better line spacing than before
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            titleText.enableWordWrapping = false; // Prevent unexpected wrapping
            titleText.overflowMode = TextOverflowModes.Overflow; // Ensure text doesn't get cut off
            titleText.lineSpacing = -20; // Better line spacing than before
            
            // Set modern font if available
            if (modernFont != null)
            {
                titleText.font = modernFont;
            }
            
            // Add letter spacing for more modern look (minimal to avoid rendering issues)
            titleText.characterSpacing = 2;
            
            // Create a container for buttons with proper spacing
            GameObject buttonContainer = new GameObject("ButtonContainer");
            buttonContainer.transform.SetParent(mainPanel.transform, false);
            RectTransform containerRect = buttonContainer.AddComponent<RectTransform>();
            containerRect.sizeDelta = new Vector2(300, 400);
            containerRect.anchoredPosition = new Vector2(0, -100); // Moved down to not overlap with title
            
            // Navigation buttons with consistent spacing and modern look
            float buttonSpacing = 80;
            CreateModernButton(buttonContainer.transform, "GENERATE SITE", new Vector2(0, 150), () => {
                ShowPanel(siteSelectionPanel);
                HidePanel(mainPanel);
            });
            
            CreateModernButton(buttonContainer.transform, "MORPHOLOGY CONTROLS", new Vector2(0, 150 - buttonSpacing), () => {
                ShowPanel(morphologyControlPanel);
                HidePanel(mainPanel);
            });
            
            CreateModernButton(buttonContainer.transform, "RUN SCENARIO", new Vector2(0, 150 - (buttonSpacing * 2)), () => {
                ShowPanel(scenarioPanel);
                HidePanel(mainPanel);
            });
            
            CreateModernButton(buttonContainer.transform, "SETTINGS", new Vector2(0, 150 - (buttonSpacing * 3)), () => {
                ShowPanel(settingsPanel);
                HidePanel(mainPanel);
            });
            
            // Version text in bottom corner with modern styling
            TextMeshProUGUI versionText = CreateModernText(
                mainPanel.transform, 
                "v1.0.0", 
                14, 
                FontStyles.Italic
            );
            RectTransform versionRect = versionText.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(1, 0);
            versionRect.anchorMax = new Vector2(1, 0);
            versionRect.pivot = new Vector2(1, 0);
            versionRect.anchoredPosition = new Vector2(-30, 30);
        }
        
        private void CreateModernSiteSelectionPanel()
        {
            siteSelectionPanel = CreatePanel("SiteSelectionPanel", mainCanvas.transform);
            
            // Add subtle grid and lines
            CreateGridLines(siteSelectionPanel.transform);
            
            // Create back button in top-left
            Button backButton = CreateBackButton(siteSelectionPanel.transform, () => {
                ShowPanel(mainPanel);
                HidePanel(siteSelectionPanel);
            });
            
            // Title with modern typography
            TextMeshProUGUI title = CreateModernText(siteSelectionPanel.transform, "SITE SELECTION", 50, FontStyles.Bold);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Container for dropdown and description
            GameObject dropdownContainer = new GameObject("DropdownContainer");
            dropdownContainer.transform.SetParent(siteSelectionPanel.transform, false);
            RectTransform dropdownContainerRect = dropdownContainer.AddComponent<RectTransform>();
            dropdownContainerRect.sizeDelta = new Vector2(500, 200);
            dropdownContainerRect.anchoredPosition = new Vector2(0, 120);
            
            // Label above dropdown
            TextMeshProUGUI dropdownLabel = CreateModernText(
                dropdownContainer.transform, 
                "SELECT SITE TYPE", 
                24
            );
            RectTransform dropdownLabelRect = dropdownLabel.GetComponent<RectTransform>();
            dropdownLabelRect.anchoredPosition = new Vector2(0, 50);
            
            // Modern styled dropdown
            RectTransform dropdownRect = CreateUIElement<RectTransform>(dropdownContainer.transform, "SiteDropdown");
            dropdownRect.sizeDelta = new Vector2(350, 50);
            dropdownRect.anchoredPosition = new Vector2(0, 0);
            
            siteDropdown = dropdownRect.gameObject.AddComponent<TMP_Dropdown>();
            
            // Style the dropdown
            ConfigureModernDropdown(siteDropdown);
            
            // Populate dropdown
            List<string> options = new List<string>
            {
                "Default Site",
                "Wellington Lambton Quay",
                "Random Terrain",
                "Flat Plane",
                "Mountain Range"
            };
            siteDropdown.ClearOptions();
            siteDropdown.AddOptions(options);
            
            // Action buttons
            Button generateButton = CreateModernButton(siteSelectionPanel.transform, "GENERATE SITE", new Vector2(0, -20), OnGenerateSiteClicked);
            Button customButton = CreateModernButton(siteSelectionPanel.transform, "LOAD CUSTOM SITE", new Vector2(0, -100), OnLoadCustomSiteClicked);
        }
        
        private void CreateModernZoneSelectionPanel()
        {
            zoneSelectionPanel = CreatePanel("ZoneSelectionPanel", mainCanvas.transform);
            
            // Add grid lines
            CreateGridLines(zoneSelectionPanel.transform);
            
            // Back button
            Button backButton = CreateBackButton(zoneSelectionPanel.transform, () => {
                ShowPanel(siteSelectionPanel);
                HidePanel(zoneSelectionPanel);
            });
            
            // Title
            TextMeshProUGUI title = CreateModernText(zoneSelectionPanel.transform, "ZONE SELECTION", 50, FontStyles.Bold);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Instructions with modern typography
            TextMeshProUGUI instructions = CreateModernText(
                zoneSelectionPanel.transform, 
                "SELECT A ZONE ON THE TERRAIN FOR GROWTH", 
                24
            );
            RectTransform instructionsRect = instructions.GetComponent<RectTransform>();
            instructionsRect.anchoredPosition = new Vector2(0, 200);
            
            // Zone selection area with modern styling
            RectTransform zoneRect = CreateUIElement<RectTransform>(zoneSelectionPanel.transform, "ZoneSelectionArea");
            zoneRect.sizeDelta = new Vector2(600, 400);
            zoneRect.anchoredPosition = new Vector2(0, 0);
            
            Image zoneImage = zoneRect.gameObject.AddComponent<Image>();
            zoneImage.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Darker, more opaque for modern look
            
            // Border for zone selection
            CreateBorder(zoneRect.gameObject, subtleLineColor);
            
            // Confirm button
            Button confirmButton = CreateModernButton(zoneSelectionPanel.transform, "CONFIRM ZONE", new Vector2(0, -200), OnConfirmZoneClicked);
        }
        
        private void CreateModernMorphologyControlPanel()
        {
            morphologyControlPanel = CreatePanel("MorphologyControlPanel", mainCanvas.transform);
            
            // Add grid lines
            CreateGridLines(morphologyControlPanel.transform);
            
            // Back button
            Button backButton = CreateBackButton(morphologyControlPanel.transform, () => {
                ShowPanel(mainPanel);
                HidePanel(morphologyControlPanel);
            });
            
            // Title
            TextMeshProUGUI title = CreateModernText(
                morphologyControlPanel.transform, 
                "MORPHOLOGY CONTROLS", 
                50, 
                FontStyles.Bold
            );
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Create two columns for controls
            GameObject leftColumn = new GameObject("LeftColumn");
            leftColumn.transform.SetParent(morphologyControlPanel.transform, false);
            RectTransform leftRect = leftColumn.AddComponent<RectTransform>();
            leftRect.sizeDelta = new Vector2(350, 500);
            leftRect.anchoredPosition = new Vector2(-300, 0);
            
            GameObject rightColumn = new GameObject("RightColumn");
            rightColumn.transform.SetParent(morphologyControlPanel.transform, false);
            RectTransform rightRect = rightColumn.AddComponent<RectTransform>();
            rightRect.sizeDelta = new Vector2(350, 500);
            rightRect.anchoredPosition = new Vector2(300, 0);
            
            // Growth algorithm label & dropdown in left column
            TextMeshProUGUI algorithmLabel = CreateModernText(
                leftColumn.transform, 
                "GROWTH ALGORITHM", 
                24
            );
            RectTransform algorithmLabelRect = algorithmLabel.GetComponent<RectTransform>();
            algorithmLabelRect.anchoredPosition = new Vector2(0, 180);
            
            RectTransform algorithmRect = CreateUIElement<RectTransform>(leftColumn.transform, "AlgorithmDropdown");
            algorithmRect.sizeDelta = new Vector2(300, 50);
            algorithmRect.anchoredPosition = new Vector2(0, 130);
            
            growthAlgorithmDropdown = algorithmRect.gameObject.AddComponent<TMP_Dropdown>();
            ConfigureModernDropdown(growthAlgorithmDropdown);
            
            List<string> options = new List<string>
            {
                "Branching Growth",
                "Random Growth",
                "Space Colonization",
                "L-System Growth"
            };
            growthAlgorithmDropdown.ClearOptions();
            growthAlgorithmDropdown.AddOptions(options);
            growthAlgorithmDropdown.onValueChanged.AddListener(OnGrowthAlgorithmChanged);
            
            // Growth speed slider in left column
            TextMeshProUGUI speedLabel = CreateModernText(
                leftColumn.transform, 
                "GROWTH SPEED", 
                24
            );
            RectTransform speedLabelRect = speedLabel.GetComponent<RectTransform>();
            speedLabelRect.anchoredPosition = new Vector2(0, 60);
            
            RectTransform speedRect = CreateUIElement<RectTransform>(leftColumn.transform, "SpeedSlider");
            speedRect.sizeDelta = new Vector2(300, 30);
            speedRect.anchoredPosition = new Vector2(0, 20);
            
            Slider speedSlider = speedRect.gameObject.AddComponent<Slider>();
            speedSlider.minValue = 0.1f;
            speedSlider.maxValue = 10.0f;
            speedSlider.value = 1.0f;
            speedSlider.onValueChanged.AddListener(OnGrowthSpeedChanged);

            SetupModernSlider(speedSlider, backgroundColor, accentColor);
            
            // Control buttons in right column
            TextMeshProUGUI controlsLabel = CreateModernText(
                rightColumn.transform, 
                "GROWTH CONTROLS", 
                24
            );
            RectTransform controlsLabelRect = controlsLabel.GetComponent<RectTransform>();
            controlsLabelRect.anchoredPosition = new Vector2(0, 180);
            
            CreateModernButton(rightColumn.transform, "START GROWTH", new Vector2(0, 130), OnStartGrowthClicked);
            CreateModernButton(rightColumn.transform, "PAUSE GROWTH", new Vector2(0, 70), OnPauseGrowthClicked);
            CreateModernButton(rightColumn.transform, "RESET GROWTH", new Vector2(0, 10), OnResetGrowthClicked);
            
            // Node count display in right column
            GameObject statsContainer = new GameObject("StatsContainer");
            statsContainer.transform.SetParent(rightColumn.transform, false);
            RectTransform statsRect = statsContainer.AddComponent<RectTransform>();
            statsRect.sizeDelta = new Vector2(300, 100);
            statsRect.anchoredPosition = new Vector2(0, -80);
            
            // Add a background to the stats container
            Image statsBg = statsContainer.AddComponent<Image>();
            statsBg.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            CreateBorder(statsContainer, subtleLineColor);
            
            TextMeshProUGUI statsLabel = CreateModernText(
                statsContainer.transform, 
                "STATISTICS", 
                18
            );
            RectTransform statsLabelRect = statsLabel.GetComponent<RectTransform>();
            statsLabelRect.anchoredPosition = new Vector2(0, 30);
            
            nodeCountText = CreateModernText(statsContainer.transform, "NODES: 0", 24);
            RectTransform nodeCountRect = nodeCountText.GetComponent<RectTransform>();
            nodeCountRect.anchoredPosition = new Vector2(0, -10);
        }
        
        private void CreateModernScenarioPanel()
        {
            scenarioPanel = CreatePanel("ScenarioPanel", mainCanvas.transform);
            
            // Add grid lines
            CreateGridLines(scenarioPanel.transform);
            
            // Back button
            Button backButton = CreateBackButton(scenarioPanel.transform, () => {
                ShowPanel(mainPanel);
                HidePanel(scenarioPanel);
            });
            
            // Title
            TextMeshProUGUI title = CreateModernText(
                scenarioPanel.transform, 
                "ENVIRONMENTAL SCENARIOS", 
                50, 
                FontStyles.Bold
            );
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Create container for controls
            GameObject controlsContainer = new GameObject("ControlsContainer");
            controlsContainer.transform.SetParent(scenarioPanel.transform, false);
            RectTransform controlsRect = controlsContainer.AddComponent<RectTransform>();
            controlsRect.sizeDelta = new Vector2(600, 300);
            controlsRect.anchoredPosition = new Vector2(0, 80);
            
            // Add background to controls
            Image controlsBg = controlsContainer.AddComponent<Image>();
            controlsBg.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            CreateBorder(controlsContainer, subtleLineColor);
            
            // Scenario dropdown with modern styling
            TextMeshProUGUI scenarioLabel = CreateModernText(
                controlsContainer.transform, 
                "SCENARIO TYPE", 
                22
            );
            RectTransform scenarioLabelRect = scenarioLabel.GetComponent<RectTransform>();
            scenarioLabelRect.anchoredPosition = new Vector2(0, 120);
            
            RectTransform scenarioRect = CreateUIElement<RectTransform>(controlsContainer.transform, "ScenarioDropdown");
            scenarioRect.sizeDelta = new Vector2(400, 50);
            scenarioRect.anchoredPosition = new Vector2(0, 70);
            
            scenarioDropdown = scenarioRect.gameObject.AddComponent<TMP_Dropdown>();
            ConfigureModernDropdown(scenarioDropdown);
            
            if (scenarioAnalyzer != null)
            {
                List<string> options = scenarioAnalyzer.GetAvailableScenarios();
                scenarioDropdown.ClearOptions();
                scenarioDropdown.AddOptions(options);
            }
            
            // Intensity slider with modern styling
            TextMeshProUGUI intensityLabel = CreateModernText(
                controlsContainer.transform, 
                "INTENSITY", 
                22
            );
            RectTransform intensityLabelRect = intensityLabel.GetComponent<RectTransform>();
            intensityLabelRect.anchoredPosition = new Vector2(-150, 20);
            
            RectTransform intensityRect = CreateUIElement<RectTransform>(controlsContainer.transform, "IntensitySlider");
            intensityRect.sizeDelta = new Vector2(250, 30);
            intensityRect.anchoredPosition = new Vector2(100, 20);
            
            Slider intensitySlider = intensityRect.gameObject.AddComponent<Slider>();
            intensitySlider.minValue = 0.1f;
            intensitySlider.maxValue = 5.0f;
            intensitySlider.value = 1.0f;
            intensitySlider.onValueChanged.AddListener(OnScenarioIntensityChanged);

            SetupModernSlider(intensitySlider, backgroundColor, accentColor);
            
            // Duration slider with modern styling
            TextMeshProUGUI durationLabel = CreateModernText(
                controlsContainer.transform, 
                "DURATION", 
                22
            );
            RectTransform durationLabelRect = durationLabel.GetComponent<RectTransform>();
            durationLabelRect.anchoredPosition = new Vector2(-150, -30);
            
            RectTransform durationRect = CreateUIElement<RectTransform>(controlsContainer.transform, "DurationSlider");
            durationRect.sizeDelta = new Vector2(250, 30);
            durationRect.anchoredPosition = new Vector2(100, -30);
            
            Slider durationSlider = durationRect.gameObject.AddComponent<Slider>();
            durationSlider.minValue = 1.0f;
            durationSlider.maxValue = 30.0f;
            durationSlider.value = 10.0f;
            durationSlider.onValueChanged.AddListener(OnScenarioDurationChanged);

            SetupModernSlider(durationSlider, backgroundColor, accentColor);
            
            // Run scenario button with modern styling
            Button runButton = CreateModernButton(scenarioPanel.transform, "RUN SCENARIO", new Vector2(0, -80), OnRunScenarioClicked);
            
            // Progress container
            GameObject progressContainer = new GameObject("ProgressContainer");
            progressContainer.transform.SetParent(scenarioPanel.transform, false);
            RectTransform progressContainerRect = progressContainer.AddComponent<RectTransform>();
            progressContainerRect.sizeDelta = new Vector2(600, 100);
            progressContainerRect.anchoredPosition = new Vector2(0, -160);
            
            // Progress label
            TextMeshProUGUI progressLabel = CreateModernText(
                progressContainer.transform, 
                "PROGRESS", 
                18
            );
            RectTransform progressLabelRect = progressLabel.GetComponent<RectTransform>();
            progressLabelRect.anchoredPosition = new Vector2(0, 40);
            
            // Progress bar with modern styling
            RectTransform progressRect = CreateUIElement<RectTransform>(progressContainer.transform, "ProgressBar");
            progressRect.sizeDelta = new Vector2(500, 20);
            progressRect.anchoredPosition = new Vector2(0, 10);
            
            Image progressBgImage = progressRect.gameObject.AddComponent<Image>();
            progressBgImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            
            GameObject fillObj = new GameObject("Fill");
            fillObj.transform.SetParent(progressRect, false);
            RectTransform fillRect = fillObj.AddComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            
            scenarioProgressBar = fillObj.AddComponent<Image>();
            scenarioProgressBar.color = Color.white;
            scenarioProgressBar.type = Image.Type.Filled;
            scenarioProgressBar.fillMethod = Image.FillMethod.Horizontal;
            scenarioProgressBar.fillAmount = 0f;
            
            // Status text with modern styling
            scenarioStatusText = CreateModernText(
                progressContainer.transform, 
                "READY TO RUN SCENARIO", 
                18
            );
            RectTransform statusRect = scenarioStatusText.GetComponent<RectTransform>();
            statusRect.anchoredPosition = new Vector2(0, -20);
        }
        
        private void CreateModernResultsPanel()
        {
            resultsPanel = CreatePanel("ResultsPanel", mainCanvas.transform);
            
            // Add grid lines
            CreateGridLines(resultsPanel.transform);
            
            // Back button
            Button backButton = CreateBackButton(resultsPanel.transform, () => {
                ShowPanel(mainPanel);
                HidePanel(resultsPanel);
            });
            
            // Title
            TextMeshProUGUI title = CreateModernText(
                resultsPanel.transform, 
                "SIMULATION RESULTS", 
                50, 
                FontStyles.Bold
            );
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Results container with modern styling
            GameObject resultsContainer = new GameObject("ResultsContainer");
            resultsContainer.transform.SetParent(resultsPanel.transform, false);
            RectTransform resultsContainerRect = resultsContainer.AddComponent<RectTransform>();
            resultsContainerRect.sizeDelta = new Vector2(700, 400);
            resultsContainerRect.anchoredPosition = new Vector2(0, 50);
            
            // Add background and border
            Image resultsBg = resultsContainer.AddComponent<Image>();
            resultsBg.color = new Color(0.1f, 0.1f, 0.1f, 0.3f);
            CreateBorder(resultsContainer, subtleLineColor);
            
            // Results text with modern styling
            resultsText = CreateModernText(
                resultsContainer.transform, 
                "NO RESULTS AVAILABLE YET", 
                24
            );
            RectTransform resultsTextRect = resultsText.GetComponent<RectTransform>();
            resultsTextRect.sizeDelta = new Vector2(650, 380);
            resultsTextRect.anchoredPosition = new Vector2(0, 0);
            
            // Export button with modern styling
            Button exportButton = CreateModernButton(resultsPanel.transform, "EXPORT RESULTS", new Vector2(0, -200), OnExportResultsClicked);
        }
        
        private void CreateModernSettingsPanel()
        {
            settingsPanel = CreatePanel("SettingsPanel", mainCanvas.transform);
            
            // Add grid lines
            CreateGridLines(settingsPanel.transform);
            
            // Back button
            Button backButton = CreateBackButton(settingsPanel.transform, () => {
                ShowPanel(mainPanel);
                HidePanel(settingsPanel);
            });
            
            // Title
            TextMeshProUGUI title = CreateModernText(
                settingsPanel.transform, 
                "SETTINGS", 
                50, 
                FontStyles.Bold
            );
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 280);
            
            // Settings container with modern styling
            GameObject settingsContainer = new GameObject("SettingsContainer");
            settingsContainer.transform.SetParent(settingsPanel.transform, false);
            RectTransform settingsContainerRect = settingsContainer.AddComponent<RectTransform>();
            settingsContainerRect.sizeDelta = new Vector2(600, 400);
            settingsContainerRect.anchoredPosition = new Vector2(0, 0);
            
            // Add background and border
            Image settingsBg = settingsContainer.AddComponent<Image>();
            settingsBg.color = new Color(0.1f, 0.1f, 0.1f, 0.2f);
            CreateBorder(settingsContainer, subtleLineColor);
            
            // Create two columns
            GameObject leftColumn = new GameObject("LeftColumn");
            leftColumn.transform.SetParent(settingsContainer.transform, false);
            RectTransform leftRect = leftColumn.AddComponent<RectTransform>();
            leftRect.sizeDelta = new Vector2(250, 380);
            leftRect.anchoredPosition = new Vector2(-150, 0);
            
            GameObject rightColumn = new GameObject("RightColumn");
            rightColumn.transform.SetParent(settingsContainer.transform, false);
            RectTransform rightRect = rightColumn.AddComponent<RectTransform>();
            rightRect.sizeDelta = new Vector2(250, 380);
            rightRect.anchoredPosition = new Vector2(150, 0);
            
            // Visualization settings label
            TextMeshProUGUI vizLabel = CreateModernText(
                leftColumn.transform, 
                "VISUALIZATION", 
                24, 
                FontStyles.Bold
            );
            RectTransform vizLabelRect = vizLabel.GetComponent<RectTransform>();
            vizLabelRect.anchoredPosition = new Vector2(0, 160);
            
            // Wireframe toggle with modern styling
            TextMeshProUGUI wireframeLabel = CreateModernText(
                leftColumn.transform, 
                "WIREFRAME MODE", 
                18
            );
            RectTransform wireframeLabelRect = wireframeLabel.GetComponent<RectTransform>();
            wireframeLabelRect.anchoredPosition = new Vector2(-40, 110);
            
            RectTransform wireframeRect = CreateUIElement<RectTransform>(leftColumn.transform, "WireframeToggle");
            wireframeRect.sizeDelta = new Vector2(60, 30);
            wireframeRect.anchoredPosition = new Vector2(80, 110);
            
            Toggle wireframeToggle = wireframeRect.gameObject.AddComponent<Toggle>();
            SetupModernToggle(wireframeToggle);
            wireframeToggle.onValueChanged.AddListener((value) => {
                if (visualizationManager != null)
                    visualizationManager.SetWireframeMode(value);
            });
            
            // Auto-rotate toggle with modern styling
            TextMeshProUGUI rotateLabel = CreateModernText(
                leftColumn.transform, 
                "AUTO-ROTATE", 
                18
            );
            RectTransform rotateLabelRect = rotateLabel.GetComponent<RectTransform>();
            rotateLabelRect.anchoredPosition = new Vector2(-40, 60);
            
            RectTransform rotateRect = CreateUIElement<RectTransform>(leftColumn.transform, "AutoRotateToggle");
            rotateRect.sizeDelta = new Vector2(60, 30);
            rotateRect.anchoredPosition = new Vector2(80, 60);
            
            Toggle rotateToggle = rotateRect.gameObject.AddComponent<Toggle>();
            SetupModernToggle(rotateToggle);
            rotateToggle.onValueChanged.AddListener((value) => {
                if (visualizationManager != null)
                    visualizationManager.SetAutoRotate(value);
            });
            
            // Camera settings label
            TextMeshProUGUI cameraLabel = CreateModernText(
                rightColumn.transform, 
                "CAMERA SETTINGS", 
                24, 
                FontStyles.Bold
            );
            RectTransform cameraLabelRect = cameraLabel.GetComponent<RectTransform>();
            cameraLabelRect.anchoredPosition = new Vector2(0, 160);
            
            // Camera zoom slider with modern styling
            TextMeshProUGUI zoomLabel = CreateModernText(
                rightColumn.transform, 
                "CAMERA ZOOM", 
                18
            );
            RectTransform zoomLabelRect = zoomLabel.GetComponent<RectTransform>();
            zoomLabelRect.anchoredPosition = new Vector2(0, 110);
            
            RectTransform zoomRect = CreateUIElement<RectTransform>(rightColumn.transform, "ZoomSlider");
            zoomRect.sizeDelta = new Vector2(200, 30);
            zoomRect.anchoredPosition = new Vector2(0, 80);
            
            Slider zoomSlider = zoomRect.gameObject.AddComponent<Slider>();
            zoomSlider.minValue = 0.0f;
            zoomSlider.maxValue = 1.0f;
            zoomSlider.value = 0.5f;
            zoomSlider.onValueChanged.AddListener((value) => {
                if (visualizationManager != null)
                    visualizationManager.SetCameraZoom(value);
            });
            SetupModernSlider(zoomSlider, backgroundColor, accentColor);
            
            // Camera speed slider with modern styling
            TextMeshProUGUI camSpeedLabel = CreateModernText(
                rightColumn.transform, 
                "CAMERA SPEED", 
                18
            );
            RectTransform camSpeedLabelRect = camSpeedLabel.GetComponent<RectTransform>();
            camSpeedLabelRect.anchoredPosition = new Vector2(0, 30);
            
            RectTransform camSpeedRect = CreateUIElement<RectTransform>(rightColumn.transform, "CameraSpeedSlider");
            camSpeedRect.sizeDelta = new Vector2(200, 30);
            camSpeedRect.anchoredPosition = new Vector2(0, 0);
            
            Slider camSpeedSlider = camSpeedRect.gameObject.AddComponent<Slider>();
            camSpeedSlider.minValue = 0.0f;
            camSpeedSlider.maxValue = 1.0f;
            camSpeedSlider.value = 0.5f;
            camSpeedSlider.onValueChanged.AddListener((value) => {
                if (visualizationManager != null)
                    visualizationManager.SetCameraSpeed(value);
            });
            SetupModernSlider(camSpeedSlider, backgroundColor, accentColor);
            
            // Screenshot button at the bottom
            Button screenshotButton = CreateModernButton(settingsPanel.transform, "TAKE SCREENSHOT", new Vector2(0, -200), () => {
                if (visualizationManager != null)
                    visualizationManager.TakeScreenshot();
            });
        }
        
        #endregion
        
        #region Modern UI Helper Methods
        
        /// <summary>
        /// Creates a panel with a pure black background.
        /// </summary>
        private GameObject CreatePanel(string name, Transform parent)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent, false);
            
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Create a pure black background with absolutely no transparency
            Image image = panel.AddComponent<Image>();
            image.color = Color.black; // Using Color.black to ensure it's 100% black (#000000)
            // Important: Make sure the background doesn't block clicks to children
            image.raycastTarget = false;
            
            // Don't add an additional Canvas component - this was causing layering issues
            // The panel will use the main canvas for rendering
            
            return panel;
        }
        
        /// <summary>
        /// Creates extremely subtle grid lines for visual structure in the modern UI.
        /// Much more subtle than before to prevent visual interference with text.
        /// </summary>
        private void CreateGridLines(Transform parent)
        {
            // Use extremely subtle lines (almost invisible)
            Color verySubtleLineColor = new Color(0.2f, 0.2f, 0.2f, 0.05f);
            
            // Single horizontal line
            GameObject hLine = new GameObject("HorizontalLine");
            hLine.transform.SetParent(parent, false);
            RectTransform hRect = hLine.AddComponent<RectTransform>();
            hRect.anchorMin = new Vector2(0.1f, 0.5f);
            hRect.anchorMax = new Vector2(0.9f, 0.5f);
            hRect.sizeDelta = new Vector2(0, 1);
            Image hImage = hLine.AddComponent<Image>();
            hImage.color = verySubtleLineColor;
            // Important: Ensure grid lines don't block clicks
            hImage.raycastTarget = false;
            
            // Single vertical line
            GameObject vLine = new GameObject("VerticalLine");
            vLine.transform.SetParent(parent, false);
            RectTransform vRect = vLine.AddComponent<RectTransform>();
            vRect.anchorMin = new Vector2(0.5f, 0.1f);
            vRect.anchorMax = new Vector2(0.5f, 0.9f);
            vRect.sizeDelta = new Vector2(1, 0);
            Image vImage = vLine.AddComponent<Image>();
            vImage.color = verySubtleLineColor;
            // Important: Ensure grid lines don't block clicks
            vImage.raycastTarget = false;
        }
        
        private void CreateBorder(GameObject target, Color borderColor)
        {
            // Create a very subtle border that won't interfere with text
            GameObject border = new GameObject("Border");
            border.transform.SetParent(target.transform, false);
            RectTransform borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = Vector2.zero;
            borderRect.offsetMax = Vector2.zero;
            
            // Use a more direct approach for borders
            // Top border
            GameObject topBorder = new GameObject("TopBorder");
            topBorder.transform.SetParent(border.transform, false);
            RectTransform topRect = topBorder.AddComponent<RectTransform>();
            topRect.anchorMin = new Vector2(0, 1);
            topRect.anchorMax = new Vector2(1, 1);
            topRect.sizeDelta = new Vector2(0, 1);
            topRect.anchoredPosition = Vector2.zero;
            Image topImage = topBorder.AddComponent<Image>();
            topImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f); // Very subtle dark gray
            // Ensure borders don't block clicks
            topImage.raycastTarget = false;
            
            // Bottom border
            GameObject bottomBorder = new GameObject("BottomBorder");
            bottomBorder.transform.SetParent(border.transform, false);
            RectTransform bottomRect = bottomBorder.AddComponent<RectTransform>();
            bottomRect.anchorMin = new Vector2(0, 0);
            bottomRect.anchorMax = new Vector2(1, 0);
            bottomRect.sizeDelta = new Vector2(0, 1);
            bottomRect.anchoredPosition = Vector2.zero;
            Image bottomImage = bottomBorder.AddComponent<Image>();
            bottomImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            // Ensure borders don't block clicks
            bottomImage.raycastTarget = false;
            
            // Left border
            GameObject leftBorder = new GameObject("LeftBorder");
            leftBorder.transform.SetParent(border.transform, false);
            RectTransform leftRect = leftBorder.AddComponent<RectTransform>();
            leftRect.anchorMin = new Vector2(0, 0);
            leftRect.anchorMax = new Vector2(0, 1);
            leftRect.sizeDelta = new Vector2(1, 0);
            leftRect.anchoredPosition = Vector2.zero;
            Image leftImage = leftBorder.AddComponent<Image>();
            leftImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            // Ensure borders don't block clicks
            leftImage.raycastTarget = false;
            
            // Right border
            GameObject rightBorder = new GameObject("RightBorder");
            rightBorder.transform.SetParent(border.transform, false);
            RectTransform rightRect = rightBorder.AddComponent<RectTransform>();
            rightRect.anchorMin = new Vector2(1, 0);
            rightRect.anchorMax = new Vector2(1, 1);
            rightRect.sizeDelta = new Vector2(1, 0);
            rightRect.anchoredPosition = Vector2.zero;
            Image rightImage = rightBorder.AddComponent<Image>();
            rightImage.color = new Color(0.2f, 0.2f, 0.2f, 0.3f);
            // Ensure borders don't block clicks
            rightImage.raycastTarget = false;
        }
        
        private T CreateUIElement<T>(Transform parent, string name) where T : Component
        {
            GameObject element = new GameObject(name);
            element.transform.SetParent(parent, false);
            return element.AddComponent<T>();
        }

        /// <summary>
        /// Creates a TMP text element with modern styling.
        /// </summary>
        private TextMeshProUGUI CreateModernText(Transform parent, string text, int fontSize, FontStyles style = FontStyles.Normal)
        {
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = textObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(800, 200); // Increased height for multi-line text
            
            TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = fontSize;
            tmpText.fontStyle = style;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = textColor;
            // Text should not block clicks to UI elements beneath it
            tmpText.raycastTarget = false;
            
            // Set modern font if available
            if (modernFont != null)
            {
                tmpText.font = modernFont;
            }
            
            // Add letter spacing for more modern look (reduced from previous version)
            tmpText.characterSpacing = 2;
            
            // Optional: For titles, add more letter spacing
            if (fontSize > 30 && style == FontStyles.Bold)
            {
                tmpText.characterSpacing = 4;
            }
            
            return tmpText;
        }
        
        /// <summary>
        /// Creates a modern styled button with robust click handling.
        /// </summary>
        private Button CreateModernButton(Transform parent, string text, Vector2 position, UnityEngine.Events.UnityAction onClick)
        {
            // Create button object with proper naming
            GameObject buttonObject = new GameObject($"Button_{text.Replace(" ", "_")}");
            buttonObject.transform.SetParent(parent, false);
            
            // Ensure proper RectTransform setup
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(250, 50);
            rectTransform.anchoredPosition = position;
            
            // Create background image - use direct black color
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 1f); // Slightly lighter than pure black for visibility
            // Ensure the Image receives clicks
            image.raycastTarget = true;
            
            // Create button with proper setup
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            button.transition = Selectable.Transition.ColorTint; // Ensure visual feedback
            button.navigation = new Navigation { mode = Navigation.Mode.None }; // Disable keyboard navigation to avoid issues

            // Ensure more visible hover/press states for better feedback
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            colors.highlightedColor = new Color(0.25f, 0.25f, 0.25f, 1f); // More noticeable highlight
            colors.pressedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // More noticeable press
            colors.selectedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            colors.disabledColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
            colors.colorMultiplier = 1f;
            colors.fadeDuration = 0.1f; // Faster feedback
            button.colors = colors;

            // Create border with more contrast for visibility
            GameObject border = new GameObject("Border");
            border.transform.SetParent(buttonObject.transform, false);
            RectTransform borderRect = border.AddComponent<RectTransform>();
            borderRect.anchorMin = Vector2.zero;
            borderRect.anchorMax = Vector2.one;
            borderRect.offsetMin = new Vector2(-1, -1);
            borderRect.offsetMax = new Vector2(1, 1);
            Image borderImage = border.AddComponent<Image>();
            borderImage.color = new Color(0.4f, 0.4f, 0.4f, 0.5f); // More visible border
            // Ensure the border doesn't block clicks
            borderImage.raycastTarget = false;

            // Button Text with improved visibility
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.fontSize = 18;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            // Ensure text doesn't block clicks
            tmpText.raycastTarget = false;
            
            // Set modern font if available
            if (modernFont != null)
            {
                tmpText.font = modernFont;
            }
            
            // Add click event with debug logging
            button.onClick.AddListener(() => {
                Debug.Log($"Button clicked: {text}");
                if (onClick != null) onClick.Invoke();
            });
            
            return button;
        }
        
        /// <summary>
        /// Creates a modern back button for navigation.
        /// </summary>
        private Button CreateBackButton(Transform parent, UnityEngine.Events.UnityAction onClick)
        {
            GameObject buttonObject = new GameObject("BackButton");
            buttonObject.transform.SetParent(parent, false);
            
            RectTransform rectTransform = buttonObject.AddComponent<RectTransform>();
            rectTransform.sizeDelta = new Vector2(100, 40);
            rectTransform.anchorMin = new Vector2(0, 1);
            rectTransform.anchorMax = new Vector2(0, 1);
            rectTransform.pivot = new Vector2(0, 1);
            rectTransform.anchoredPosition = new Vector2(20, -20);
            
            // Black background with some transparency
            Image image = buttonObject.AddComponent<Image>();
            image.color = new Color(0f, 0f, 0f, 0.8f);
            
            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;

            // Add subtle hover effects
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0f, 0f, 0f, 0.8f);
            colors.highlightedColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
            colors.pressedColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
            button.colors = colors;

            // Create subtle border
            CreateBorder(buttonObject, subtleLineColor);

            // Button Text
            GameObject textObject = new GameObject("Text");
            textObject.transform.SetParent(buttonObject.transform, false);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmpText = textObject.AddComponent<TextMeshProUGUI>();
            tmpText.text = "BACK";
            tmpText.fontSize = 16;
            tmpText.fontStyle = FontStyles.Bold;
            tmpText.alignment = TextAlignmentOptions.Center;
            tmpText.color = Color.white;
            
            // Set modern font if available
            if (modernFont != null)
            {
                tmpText.font = modernFont;
            }
            
            // Add click event
            button.onClick.AddListener(onClick);
            
            return button;
        }

        /// <summary>
        /// Sets up a slider with modern visuals.
        /// </summary>
        private void SetupModernSlider(Slider slider, Color backgroundColor, Color fillColor)
        {
            // Create background
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(slider.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.1f, 0.1f, 0.1f, 1f); 
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.anchorMin = Vector2.zero;
            bgRect.anchorMax = Vector2.one;
            bgRect.offsetMin = Vector2.zero;
            bgRect.offsetMax = Vector2.zero;
            slider.targetGraphic = bgImage;

            // Create fill area with cleaner look
            GameObject fillArea = new GameObject("Fill Area");
            fillArea.transform.SetParent(slider.transform, false);
            RectTransform fillAreaRect = fillArea.AddComponent<RectTransform>();
            fillAreaRect.anchorMin = new Vector2(0f, 0.4f);
            fillAreaRect.anchorMax = new Vector2(1f, 0.6f);
            fillAreaRect.offsetMin = new Vector2(5f, 0f);
            fillAreaRect.offsetMax = new Vector2(-5f, 0f);

            GameObject fill = new GameObject("Fill");
            fill.transform.SetParent(fillAreaRect, false);
            Image fillImage = fill.AddComponent<Image>();
            fillImage.color = Color.white;
            RectTransform fillRect = fill.GetComponent<RectTransform>();
            fillRect.anchorMin = Vector2.zero;
            fillRect.anchorMax = Vector2.one;
            fillRect.offsetMin = Vector2.zero;
            fillRect.offsetMax = Vector2.zero;
            fillImage.type = Image.Type.Filled;
            fillImage.fillMethod = Image.FillMethod.Horizontal;
            slider.fillRect = fillRect;

            // Modern minimal handle
            GameObject handleObj = new GameObject("Handle");
            handleObj.transform.SetParent(fillAreaRect, false);
            Image handleImage = handleObj.AddComponent<Image>();
            handleImage.color = Color.white;
            RectTransform handleRect = handleObj.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(12f, 20f);
            slider.handleRect = handleRect;

            slider.direction = Slider.Direction.LeftToRight;
            
            // Create subtle border around the slider
            CreateBorder(bg, subtleLineColor);
        }

        /// <summary>
        /// Sets up a toggle with modern styling (switch-like).
        /// </summary>
        private void SetupModernToggle(Toggle toggle)
        {
            // Create background (track)
            GameObject bg = new GameObject("Background");
            bg.transform.SetParent(toggle.transform, false);
            Image bgImage = bg.AddComponent<Image>();
            bgImage.color = new Color(0.15f, 0.15f, 0.15f, 1f);
            RectTransform bgRect = bg.GetComponent<RectTransform>();
            bgRect.sizeDelta = new Vector2(40, 20);
            toggle.targetGraphic = bgImage;
            
            // Round the background corners
            bgImage.maskable = true;

            // Create toggle handle (knob)
            GameObject handle = new GameObject("Handle");
            handle.transform.SetParent(bg.transform, false);
            Image handleImage = handle.AddComponent<Image>();
            handleImage.color = Color.white;
            RectTransform handleRect = handle.GetComponent<RectTransform>();
            handleRect.sizeDelta = new Vector2(16, 16);
            handleRect.anchoredPosition = new Vector2(-10, 0); // Start position
            toggle.graphic = handleImage;
            
            // Handle position animation based on toggle state
            toggle.onValueChanged.AddListener((isOn) => {
                // Update handle position
                handleRect.anchoredPosition = isOn ? new Vector2(10, 0) : new Vector2(-10, 0);
                
                // Update background color when on
                bgImage.color = isOn ? new Color(0.3f, 0.3f, 0.3f, 1f) : new Color(0.15f, 0.15f, 0.15f, 1f);
            });
            
            // Initially off
            toggle.isOn = false;
            
            // Create subtle border
            CreateBorder(bg, subtleLineColor);
        }
        
        /// <summary>
        /// Configures a dropdown with modern styling.
        /// </summary>
        private void ConfigureModernDropdown(TMP_Dropdown dropdown)
        {
            // Style the dropdown
            dropdown.template = CreateModernDropdownTemplate(dropdown);
            
            // Get or add required components
            Image dropdownImage = dropdown.GetComponent<Image>();
            if (dropdownImage == null)
            {
                dropdownImage = dropdown.gameObject.AddComponent<Image>();
            }
            dropdownImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            // Ensure dropdown background can receive clicks
            dropdownImage.raycastTarget = true;
            
            // Create text component
            GameObject textObj = new GameObject("Label");
            textObj.transform.SetParent(dropdown.transform, false);
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = new Vector2(0, 0);
            textRect.anchorMax = new Vector2(1, 1);
            textRect.offsetMin = new Vector2(15, 0);
            textRect.offsetMax = new Vector2(-35, 0);
            
            TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
            text.text = "Select Option";
            text.fontSize = 18;
            text.color = Color.white;
            text.alignment = TextAlignmentOptions.Left;
            // Text should not block clicks
            text.raycastTarget = false;
            dropdown.captionText = text;
            
            // Create arrow indicator
            GameObject arrowObj = new GameObject("Arrow");
            arrowObj.transform.SetParent(dropdown.transform, false);
            RectTransform arrowRect = arrowObj.AddComponent<RectTransform>();
            arrowRect.anchorMin = new Vector2(1, 0.5f);
            arrowRect.anchorMax = new Vector2(1, 0.5f);
            arrowRect.pivot = new Vector2(1, 0.5f);
            arrowRect.anchoredPosition = new Vector2(-15, 0);
            arrowRect.sizeDelta = new Vector2(20, 20);
            
            Image arrowImage = arrowObj.AddComponent<Image>();
            arrowImage.color = Color.white;
            // Arrow should not block clicks
            arrowImage.raycastTarget = false;
            
            // Create subtle border
            CreateBorder(dropdown.gameObject, subtleLineColor);
        }
        
        /// <summary>
        /// Creates a template for the dropdown with modern styling.
        /// </summary>
        private RectTransform CreateModernDropdownTemplate(TMP_Dropdown dropdown)
        {
            // Create template container
            GameObject template = new GameObject("Template");
            template.SetActive(false);
            
            RectTransform templateRect = template.AddComponent<RectTransform>();
            templateRect.sizeDelta = new Vector2(dropdown.GetComponent<RectTransform>().sizeDelta.x, 150);
            
            // Set parent (temporarily)
            template.transform.SetParent(dropdown.transform, false);
            
            // Add required components
            Canvas templateCanvas = template.AddComponent<Canvas>();
            templateCanvas.overrideSorting = true;
            templateCanvas.sortingOrder = 30000;
            
            template.AddComponent<GraphicRaycaster>();
            
            // Add panel background
            Image templateImage = template.AddComponent<Image>();
            templateImage.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            // Create viewport
            GameObject viewport = new GameObject("Viewport");
            viewport.transform.SetParent(template.transform, false);
            
            RectTransform viewportRect = viewport.AddComponent<RectTransform>();
            viewportRect.anchorMin = Vector2.zero;
            viewportRect.anchorMax = Vector2.one;
            viewportRect.sizeDelta = Vector2.zero;
            viewportRect.offsetMin = Vector2.zero;
            viewportRect.offsetMax = Vector2.zero;
            
            // Add mask
            viewport.AddComponent<Mask>().showMaskGraphic = false;
            Image viewportImage = viewport.AddComponent<Image>();
            viewportImage.color = Color.white;
            
            // Create item container
            GameObject content = new GameObject("Content");
            content.transform.SetParent(viewport.transform, false);
            
            RectTransform contentRect = content.AddComponent<RectTransform>();
            contentRect.anchorMin = new Vector2(0f, 1f);
            contentRect.anchorMax = new Vector2(1f, 1f);
            contentRect.pivot = new Vector2(0.5f, 1f);
            contentRect.sizeDelta = new Vector2(0f, 28f);
            contentRect.anchoredPosition = Vector2.zero;
            
            // Add content sizefitter and vertical layout group
            ContentSizeFitter contentSizeFitter = content.AddComponent<ContentSizeFitter>();
            contentSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            
            VerticalLayoutGroup verticalLayoutGroup = content.AddComponent<VerticalLayoutGroup>();
            verticalLayoutGroup.spacing = 0f;
            verticalLayoutGroup.childForceExpandWidth = true;
            verticalLayoutGroup.childForceExpandHeight = false;
            verticalLayoutGroup.childControlWidth = true;
            verticalLayoutGroup.childControlHeight = true;
            
            // Create item template
            GameObject item = new GameObject("Item");
            item.transform.SetParent(content.transform, false);
            
            RectTransform itemRect = item.AddComponent<RectTransform>();
            itemRect.anchorMin = new Vector2(0f, 0.5f);
            itemRect.anchorMax = new Vector2(1f, 0.5f);
            itemRect.sizeDelta = new Vector2(0f, 30f);
            
            // Add toggle for selection
            Toggle itemToggle = item.AddComponent<Toggle>();
            itemToggle.targetGraphic = item.AddComponent<Image>();
            itemToggle.targetGraphic.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            
            // Item background color transitions
            ColorBlock itemColors = itemToggle.colors;
            itemColors.normalColor = new Color(0.1f, 0.1f, 0.1f, 1f);
            itemColors.highlightedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            itemColors.pressedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            itemColors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
            itemToggle.colors = itemColors;
            
            // Item label
            GameObject itemLabel = new GameObject("Item Label");
            itemLabel.transform.SetParent(item.transform, false);
            
            RectTransform itemLabelRect = itemLabel.AddComponent<RectTransform>();
            itemLabelRect.anchorMin = Vector2.zero;
            itemLabelRect.anchorMax = Vector2.one;
            itemLabelRect.offsetMin = new Vector2(15, 0);
            itemLabelRect.offsetMax = new Vector2(-10, 0);
            
            TextMeshProUGUI itemText = itemLabel.AddComponent<TextMeshProUGUI>();
            itemText.text = "Option";
            itemText.fontSize = 16;
            itemText.color = Color.white;
            itemText.alignment = TextAlignmentOptions.Left;
            
            // Set modern font if available
            if (modernFont != null)
            {
                itemText.font = modernFont;
            }
            
            // Set up template
            dropdown.itemText = itemText;
            dropdown.template = templateRect;
            dropdown.captionText = dropdown.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
            
            // Create subtle border around template
            CreateBorder(template, subtleLineColor);
            
            return templateRect;
        }
        
        #endregion
        
        #region UI Event Handlers
        
        // These are the same as in the original UIManager
        private void OnGenerateSiteClicked()
        {
            if (siteGenerator != null && siteDropdown != null)
            {
                string selectedSite = siteDropdown.options[siteDropdown.value].text;
                
                switch (selectedSite)
                {
                    case "Default Site":
                        siteGenerator.GenerateDefaultSite();
                        break;
                        
                    case "Random Terrain":
                        siteGenerator.GenerateRandomSite();
                        break;
                }
                
                // Move to zone selection
                ShowPanel(zoneSelectionPanel);
                HidePanel(siteSelectionPanel);
            }
        }
          private void OnLoadCustomSiteClicked()
        {
            // In Unity, we need to use a special dialog for folder selection
            // This implementation uses StandaloneFileBrowser (for editor and standalone builds)
            // Note: In a real implementation, you would need to import a file browser package
            
            #if UNITY_EDITOR
            // In the Unity Editor, we can use EditorUtility
            string folderPath = UnityEditor.EditorUtility.OpenFolderPanel("Select Folder Containing GIS Data", "", "");
            if (!string.IsNullOrEmpty(folderPath))
            {
                StartCoroutine(ImportGISDataWithProgress(folderPath));
            }
            #else
            // For standalone builds, we would need a third-party file browser
            // For now, we'll use a development folder path for testing
            string folderPath = Application.dataPath + "/GISData";
            if (Directory.Exists(folderPath))
            {
                StartCoroutine(ImportGISDataWithProgress(folderPath));
            }
            else
            {
                Debug.LogError($"GIS data folder not found at: {folderPath}");
                // In a real implementation, you would show an error message to the user
            }
            #endif
        }
        
        private IEnumerator ImportGISDataWithProgress(string folderPath)
        {
            // Create a progress panel to show import progress
            GameObject progressPanel = CreateProgressPanel();
            TextMeshProUGUI progressText = progressPanel.GetComponentInChildren<TextMeshProUGUI>();
            Image progressBar = progressPanel.GetComponentsInChildren<Image>()[1]; // Second image is the progress bar fill
            
            // Show progress panel
            progressPanel.SetActive(true);
            
            // Update progress text
            if (progressText != null)
                progressText.text = "PREPARING TO IMPORT GIS DATA...";
            
            yield return null; // Wait one frame to ensure UI updates
            
            // Start import process
            if (progressText != null)
                progressText.text = "LOADING TERRAIN DATA...";
            if (progressBar != null)
                progressBar.fillAmount = 0.2f;
            
            yield return new WaitForSeconds(0.5f); // Small delay for UI feedback
            
            // Call the import method from SiteGenerator
            if (siteGenerator != null)
            {
                // In a real application, you would implement a callback system
                // for the SiteGenerator to report progress back to the UI
                siteGenerator.ImportSiteData(folderPath);
            }
            
            // Update progress for buildings
            if (progressText != null)
                progressText.text = "PROCESSING BUILDINGS...";
            if (progressBar != null)
                progressBar.fillAmount = 0.6f;
            
            yield return new WaitForSeconds(0.5f); // Small delay for UI feedback
            
            // Update progress for roads
            if (progressText != null)
                progressText.text = "PROCESSING ROADS...";
            if (progressBar != null)
                progressBar.fillAmount = 0.8f;
            
            yield return new WaitForSeconds(0.5f); // Small delay for UI feedback
            
            // Final progress update
            if (progressText != null)
                progressText.text = "IMPORT COMPLETE";
            if (progressBar != null)
                progressBar.fillAmount = 1.0f;
            
            yield return new WaitForSeconds(1.0f); // Show completion message briefly
            
            // Hide progress panel
            progressPanel.SetActive(false);
            
            // Move to zone selection
            ShowPanel(zoneSelectionPanel);
            HidePanel(siteSelectionPanel);
        }
        
        private GameObject CreateProgressPanel()
        {
            // Check if progress panel already exists
            Transform existingPanel = mainCanvas.transform.Find("ProgressPanel");
            if (existingPanel != null)
                return existingPanel.gameObject;
            
            // Create a new progress panel
            GameObject panel = new GameObject("ProgressPanel");
            panel.transform.SetParent(mainCanvas.transform, false);
            
            RectTransform rectTransform = panel.AddComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.3f, 0.4f);
            rectTransform.anchorMax = new Vector2(0.7f, 0.6f);
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            // Add background image
            Image image = panel.AddComponent<Image>();
            image.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add border
            CreateBorder(panel, subtleLineColor);
            
            // Add title
            TextMeshProUGUI title = CreateModernText(panel.transform, "IMPORTING GIS DATA", 30, FontStyles.Bold);
            RectTransform titleRect = title.GetComponent<RectTransform>();
            titleRect.anchoredPosition = new Vector2(0, 40);
            
            // Add progress text
            TextMeshProUGUI progressText = CreateModernText(panel.transform, "PREPARING...", 18);
            RectTransform progressTextRect = progressText.GetComponent<RectTransform>();
            progressTextRect.anchoredPosition = new Vector2(0, 0);
            
            // Add progress bar background
            GameObject progressBg = new GameObject("ProgressBarBg");
            progressBg.transform.SetParent(panel.transform, false);
            RectTransform progressBgRect = progressBg.AddComponent<RectTransform>();
            progressBgRect.sizeDelta = new Vector2(300, 20);
            progressBgRect.anchoredPosition = new Vector2(0, -40);
            Image progressBgImage = progressBg.AddComponent<Image>();
            progressBgImage.color = new Color(0.05f, 0.05f, 0.05f, 1f);
            
            // Add progress bar fill
            GameObject progressFill = new GameObject("ProgressBarFill");
            progressFill.transform.SetParent(progressBg.transform, false);
            RectTransform progressFillRect = progressFill.AddComponent<RectTransform>();
            progressFillRect.anchorMin = new Vector2(0, 0);
            progressFillRect.anchorMax = new Vector2(1, 1);
            progressFillRect.offsetMin = Vector2.zero;
            progressFillRect.offsetMax = Vector2.zero;
            progressFillRect.pivot = new Vector2(0, 0.5f);
            Image progressFillImage = progressFill.AddComponent<Image>();
            progressFillImage.color = Color.white;
            progressFillImage.type = Image.Type.Filled;
            progressFillImage.fillMethod = Image.FillMethod.Horizontal;
            progressFillImage.fillAmount = 0f;
              // Hide the panel initially
            panel.SetActive(false);
            
            return panel;
        }
          private void OnZoneSelected()
        {
            // Process the selected zone...
            ShowPanel(morphologyControlPanel);
            HidePanel(zoneSelectionPanel);
        }
        
        private void OnConfirmZoneClicked()
        {
            // This method is called when the user confirms their zone selection
            // Process the selected zone and move to the morphology control panel
            OnZoneSelected();
        }
        
        private void OnStartGrowthClicked()
        {
            if (mainController != null)
            {
                mainController.StartSimulation();
            }
        }
        
        private void OnPauseGrowthClicked()
        {
            if (mainController != null)
            {
                mainController.PauseSimulation();
            }
        }
        
        private void OnResetGrowthClicked()
        {
            if (mainController != null)
            {
                mainController.ResetSimulation();
            }
        }
        
        private void OnGrowthSpeedChanged(float value)
        {
            if (mainController != null)
            {
                mainController.SetSimulationSpeed(value);
            }
        }
        
        private void OnGrowthAlgorithmChanged(int value)
        {
            if (morphologyManager != null && growthAlgorithmDropdown != null)
            {
                string selectedAlgorithm = growthAlgorithmDropdown.options[value].text;
                
                switch (selectedAlgorithm)
                {
                    case "Branching Growth":
                        morphologyManager.SetGrowthAlgorithm(new BiomorphicSim.Morphology.BranchingGrowthAlgorithm());
                        break;
                }
            }
        }
        
        private void OnRunScenarioClicked()
        {
            if (scenarioAnalyzer != null && scenarioDropdown != null)
            {
                string selectedScenario = scenarioDropdown.options[scenarioDropdown.value].text;
                mainController.RunScenario(selectedScenario);
                
                if (scenarioStatusText != null)
                {
                    scenarioStatusText.text = $"RUNNING: {selectedScenario.ToUpper()}";
                }
            }
        }
        
        private void OnScenarioIntensityChanged(float value)
        {
            // This would update the intensity for the selected scenario
        }
        
        private void OnScenarioDurationChanged(float value)
        {
            // This would update the duration for the selected scenario
        }
        
        private void OnExportResultsClicked()
        {
            // This would export the simulation results
            Debug.Log("Export results clicked - would save results to file");
        }
        
        #endregion
        
        #region Utility Methods
        
        public void ShowPanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(true);
            }
        }
        
        public void HidePanel(GameObject panel)
        {
            if (panel != null)
            {
                panel.SetActive(false);
            }
        }
        
        public void UpdateScenarioProgress(float progress, string scenarioName)
        {
            if (scenarioProgressBar != null)
            {
                scenarioProgressBar.fillAmount = progress;
            }
            
            if (scenarioStatusText != null)
            {
                scenarioStatusText.text = $"RUNNING: {scenarioName.ToUpper()} ({progress:P0})";
            }
        }
        
        public void ScenarioCompleted(string scenarioName)
        {
            if (scenarioProgressBar != null)
            {
                scenarioProgressBar.fillAmount = 1.0f;
            }
            
            if (scenarioStatusText != null)
            {
                scenarioStatusText.text = $"COMPLETED: {scenarioName.ToUpper()}";
            }
            
            // Show results panel
            ShowResultsPanel();
            
            if (resultsText != null && morphologyManager != null)
            {
                int nodeCount = morphologyManager.GetNodeCount();
                resultsText.text = $"SCENARIO: {scenarioName.ToUpper()}\n\nNODES: {nodeCount}\n";
                // Add more detailed results here
            }
        }
        
        public void ShowResultsPanel()
        {
            ShowPanel(resultsPanel);
            HidePanel(mainPanel);
            HidePanel(siteSelectionPanel);
            HidePanel(zoneSelectionPanel);
            HidePanel(morphologyControlPanel);
            HidePanel(scenarioPanel);
            HidePanel(settingsPanel);
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
            {
                StopCoroutine(updateUICoroutine);
            }
        }
        
        #endregion
    }
}