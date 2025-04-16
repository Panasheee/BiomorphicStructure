using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Video;

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
                Debug.LogWarning("UICanvas not found in hierarchy. Creating new UICanvas...");
                CreateUICanvas();
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
        
        // Create missing screens
        if (startScreen == null) {
            Debug.LogWarning("StartScreen not found. Creating new StartScreen...");
            CreateStartScreen();
        }
        
        if (siteViewerScreen == null) {
            Debug.LogWarning("SiteViewerScreen not found. Creating new SiteViewerScreen...");
            CreateSiteViewerScreen();
        }
        
        if (siteAnalysisScreen == null) {
            Debug.LogWarning("SiteAnalysisScreen not found. Creating new SiteAnalysisScreen...");
            CreateSiteAnalysisScreen();
        }
        
        if (morphologyScreen == null) {
            Debug.LogWarning("MorphologyScreen not found. Creating new MorphologyScreen...");
            CreateMorphologyScreen();
        }
        
        if (scenarioScreen == null) {
            Debug.LogWarning("ScenarioScreen not found. Creating new ScenarioScreen...");
            CreateScenarioScreen();
        }
        
        // Log which screens were found
        Debug.Log($"Found UI screens - StartScreen: {startScreen != null}, " +
                 $"SiteViewerScreen: {siteViewerScreen != null}, " +
                 $"SiteAnalysisScreen: {siteAnalysisScreen != null}, " +
                 $"MorphologyScreen: {morphologyScreen != null}, " +
                 $"ScenarioScreen: {scenarioScreen != null}");
        
        // Make sure the Canvas has a CanvasScaler and Graphics Raycaster
        EnsureCanvasComponents();
    }
    
    private void CreateUICanvas()
    {
        // Create the UI Canvas
        uiCanvasObject = new GameObject("UICanvas");
        
        // Add necessary Canvas components
        Canvas canvas = uiCanvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100; // Make sure it's visible on top
        
        // Add canvas scaler for responsive UI
        CanvasScaler scaler = uiCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        scaler.matchWidthOrHeight = 0.5f; // Balance width and height
        
        // Add graphic raycaster for UI interaction
        GraphicRaycaster raycaster = uiCanvasObject.AddComponent<GraphicRaycaster>();
        
        // Make sure we have an EventSystem
        if (FindObjectOfType<EventSystem>() == null)
        {
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
        }
        
        // Create all necessary UI screens
        CreateStartScreen();
        CreateSiteViewerScreen();
        CreateSiteAnalysisScreen();
        CreateMorphologyScreen();
        CreateScenarioScreen();
        
        Debug.Log("UICanvas created successfully with all UI screens");
    }
    
    private void EnsureCanvasComponents()
    {
        if (uiCanvasObject == null) return;
        
        // Make sure the Canvas has a Canvas component
        Canvas canvas = uiCanvasObject.GetComponent<Canvas>();
        if (canvas == null)
        {
            canvas = uiCanvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;
        }
        
        // Make sure the Canvas has a CanvasScaler component
        CanvasScaler scaler = uiCanvasObject.GetComponent<CanvasScaler>();
        if (scaler == null)
        {
            scaler = uiCanvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);
            scaler.matchWidthOrHeight = 0.5f;
        }
        
        // Make sure the Canvas has a GraphicRaycaster component
        GraphicRaycaster raycaster = uiCanvasObject.GetComponent<GraphicRaycaster>();
        if (raycaster == null)
        {
            raycaster = uiCanvasObject.AddComponent<GraphicRaycaster>();
        }
    }
    
    private void CreateStartScreen()
    {
        startScreen = new GameObject("StartScreen");
        startScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        // Add RectTransform component
        RectTransform rect = startScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create a solid black background first
        GameObject blackBackground = new GameObject("BlackBackground");
        blackBackground.transform.SetParent(startScreen.transform, false);
        
        RectTransform blackBgRect = blackBackground.AddComponent<RectTransform>();
        blackBgRect.anchorMin = Vector2.zero;
        blackBgRect.anchorMax = Vector2.one;
        blackBgRect.offsetMin = Vector2.zero;
        blackBgRect.offsetMax = Vector2.zero;
        
        Image blackBgImage = blackBackground.AddComponent<Image>();
        blackBgImage.color = Color.black; // Solid black background
        
        // Create a background video component
        GameObject videoBackground = new GameObject("VideoBackground");
        videoBackground.transform.SetParent(startScreen.transform, false);
        
        RectTransform videoBgRect = videoBackground.AddComponent<RectTransform>();
        videoBgRect.anchorMin = Vector2.zero;
        videoBgRect.anchorMax = Vector2.one;
        videoBgRect.offsetMin = Vector2.zero;
        videoBgRect.offsetMax = Vector2.zero;
        
        RawImage rawImage = videoBackground.AddComponent<RawImage>();
        rawImage.color = Color.white;
        backgroundVideoImage = rawImage;
        
        // Add VideoPlayer component
        VideoPlayer videoPlayer = videoBackground.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
        videoPlayer.isLooping = true;
        videoPlayer.playOnAwake = true;
        rawImage.texture = videoPlayer.targetTexture;
        backgroundVideoPlayer = videoPlayer;
        
        // Try to load the video
        LoadBackgroundVideo();
        
        // Create a semi-transparent overlay panel (reduce opacity for better video visibility)
        GameObject overlay = new GameObject("Overlay");
        overlay.transform.SetParent(startScreen.transform, false);
        
        RectTransform overlayRect = overlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.offsetMin = Vector2.zero;
        overlayRect.offsetMax = Vector2.zero;
        
        Image overlayImage = overlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.5f); // Black with 50% opacity for better video visibility
        
        // Create title text
        GameObject titleObj = new GameObject("TitleText");
        titleObj.transform.SetParent(startScreen.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.8f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(800, 100);
        titleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BIOMORPHIC SIMULATOR";
        titleText.fontSize = 48;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
        
        // Create Load Site button
        GameObject loadButtonObj = new GameObject("LoadSiteButton");
        loadButtonObj.transform.SetParent(startScreen.transform, false);
        
        RectTransform loadRect = loadButtonObj.AddComponent<RectTransform>();
        loadRect.anchorMin = new Vector2(0.5f, 0.5f);
        loadRect.anchorMax = new Vector2(0.5f, 0.5f);
        loadRect.sizeDelta = new Vector2(300, 60);
        loadRect.anchoredPosition = Vector2.zero;
        
        Image loadImg = loadButtonObj.AddComponent<Image>();
        loadImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f); // Dark gray, almost black
        
        Button loadBtn = loadButtonObj.AddComponent<Button>();
        loadBtn.transition = Selectable.Transition.ColorTint;
        
        // Set up button colors in monochrome
        ColorBlock colors = loadBtn.colors;
        colors.normalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f, 0.9f);
        colors.pressedColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        colors.selectedColor = new Color(0.25f, 0.25f, 0.25f, 1f);
        loadBtn.colors = colors;
        
        // Set this as our loadSiteButton field
        loadSiteButton = loadBtn;
        
        // Add text to button
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(loadButtonObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "LOAD SITE";
        btnText.fontSize = 24;
        btnText.fontStyle = FontStyles.Bold;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        Debug.Log("StartScreen created successfully");
    }
    
    private void CreateSiteViewerScreen()
    {
        // Create basic screen with back button and run analysis button
        siteViewerScreen = new GameObject("SiteViewerScreen");
        siteViewerScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        RectTransform rect = siteViewerScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Add a setup component
        siteViewerScreen.AddComponent<SiteViewerScreenSetup>();
        
        Debug.Log("SiteViewerScreen created successfully");
    }
    
    private void CreateSiteAnalysisScreen()
    {
        // Create a basic analysis screen
        siteAnalysisScreen = new GameObject("SiteAnalysisScreen");
        siteAnalysisScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        RectTransform rect = siteAnalysisScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Add a background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(siteAnalysisScreen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Create Analysis Report Text area
        GameObject reportObj = new GameObject("AnalysisReportText");
        reportObj.transform.SetParent(siteAnalysisScreen.transform, false);
        
        RectTransform reportRect = reportObj.AddComponent<RectTransform>();
        reportRect.anchorMin = new Vector2(0.1f, 0.2f);
        reportRect.anchorMax = new Vector2(0.9f, 0.8f);
        reportRect.offsetMin = Vector2.zero;
        reportRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI reportText = reportObj.AddComponent<TextMeshProUGUI>();
        reportText.text = "SITE ANALYSIS REPORT\n\nClick 'Run Analysis' to generate report.";
        reportText.fontSize = 24;
        reportText.alignment = TextAlignmentOptions.TopLeft;
        reportText.color = Color.white;
        
        // Set this as our analysisReportText field
        analysisReportText = reportText;
        
        // Create back button
        GameObject backBtnObj = new GameObject("BackButton");
        backBtnObj.transform.SetParent(siteAnalysisScreen.transform, false);
        
        RectTransform backRect = backBtnObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.02f, 0.93f);
        backRect.anchorMax = new Vector2(0.12f, 0.98f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        
        Image backImg = backBtnObj.AddComponent<Image>();
        backImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button backBtn = backBtnObj.AddComponent<Button>();
        backBtn.transition = Selectable.Transition.ColorTint;
        
        // Add text to button
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(backBtnObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "BACK";
        btnText.fontSize = 16;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        // Add Start Morphology button
        GameObject morphBtnObj = new GameObject("StartMorphologyButton");
        morphBtnObj.transform.SetParent(siteAnalysisScreen.transform, false);
        
        RectTransform morphRect = morphBtnObj.AddComponent<RectTransform>();
        morphRect.anchorMin = new Vector2(0.88f, 0.93f);
        morphRect.anchorMax = new Vector2(0.98f, 0.98f);
        morphRect.offsetMin = Vector2.zero;
        morphRect.offsetMax = Vector2.zero;
        
        Image morphImg = morphBtnObj.AddComponent<Image>();
        morphImg.color = new Color(0.2f, 0.6f, 0.3f, 0.8f);
        
        Button morphBtn = morphBtnObj.AddComponent<Button>();
        morphBtn.transition = Selectable.Transition.ColorTint;
        
        // Set this as our startMorphologyButton field
        startMorphologyButton = morphBtn;
        
        // Add text to button
        GameObject morphTextObj = new GameObject("Text");
        morphTextObj.transform.SetParent(morphBtnObj.transform, false);
        
        RectTransform morphTextRect = morphTextObj.AddComponent<RectTransform>();
        morphTextRect.anchorMin = Vector2.zero;
        morphTextRect.anchorMax = Vector2.one;
        morphTextRect.offsetMin = Vector2.zero;
        morphTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI morphText = morphTextObj.AddComponent<TextMeshProUGUI>();
        morphText.text = "START MORPHOLOGY";
        morphText.fontSize = 14;
        morphText.alignment = TextAlignmentOptions.Center;
        morphText.color = Color.white;
        
        Debug.Log("SiteAnalysisScreen created successfully");
    }
    
    private void CreateMorphologyScreen()
    {
        // Create a basic morphology screen
        morphologyScreen = new GameObject("MorphologyScreen");
        morphologyScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        RectTransform rect = morphologyScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        // Create translucent panel
        GameObject panel = new GameObject("ControlPanel");
        panel.transform.SetParent(morphologyScreen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0, 0.85f);
        panelRect.anchorMax = new Vector2(1, 1);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImg = panel.AddComponent<Image>();
        panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.8f);
        
        // Create pause/resume button
        GameObject pauseBtnObj = new GameObject("PauseResumeButton");
        pauseBtnObj.transform.SetParent(panel.transform, false);
        
        RectTransform pauseRect = pauseBtnObj.AddComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.02f, 0.2f);
        pauseRect.anchorMax = new Vector2(0.12f, 0.8f);
        pauseRect.offsetMin = Vector2.zero;
        pauseRect.offsetMax = Vector2.zero;
        
        Image pauseImg = pauseBtnObj.AddComponent<Image>();
        pauseImg.color = new Color(0.6f, 0.6f, 0.2f, 0.8f);
        
        Button pauseBtn = pauseBtnObj.AddComponent<Button>();
        pauseBtn.transition = Selectable.Transition.ColorTint;
        
        // Set this as our pauseResumeButton field
        pauseResumeButton = pauseBtn;
        
        // Add text to button
        GameObject btnTextObj = new GameObject("Text");
        btnTextObj.transform.SetParent(pauseBtnObj.transform, false);
        
        RectTransform btnTextRect = btnTextObj.AddComponent<RectTransform>();
        btnTextRect.anchorMin = Vector2.zero;
        btnTextRect.anchorMax = Vector2.one;
        btnTextRect.offsetMin = Vector2.zero;
        btnTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI btnText = btnTextObj.AddComponent<TextMeshProUGUI>();
        btnText.text = "PAUSE";
        btnText.fontSize = 18;
        btnText.alignment = TextAlignmentOptions.Center;
        btnText.color = Color.white;
        
        // Create node count text
        GameObject countObj = new GameObject("NodeCountText");
        countObj.transform.SetParent(panel.transform, false);
        
        RectTransform countRect = countObj.AddComponent<RectTransform>();
        countRect.anchorMin = new Vector2(0.2f, 0.2f);
        countRect.anchorMax = new Vector2(0.4f, 0.8f);
        countRect.offsetMin = Vector2.zero;
        countRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI countText = countObj.AddComponent<TextMeshProUGUI>();
        countText.text = "NODES: 0";
        countText.fontSize = 18;
        countText.alignment = TextAlignmentOptions.Left;
        countText.color = Color.white;
        
        // Set this as our nodeCountText field
        nodeCountText = countText;
        
        // Create growth speed slider
        GameObject sliderObj = new GameObject("GrowthSpeedSlider");
        sliderObj.transform.SetParent(panel.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.5f, 0.2f);
        sliderRect.anchorMax = new Vector2(0.8f, 0.8f);
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;
        
        Slider speedSlider = sliderObj.AddComponent<Slider>();
        speedSlider.minValue = 0;
        speedSlider.maxValue = 1;
        speedSlider.value = 0.5f;
        
        // Set this as our growthSpeedSlider field
        growthSpeedSlider = speedSlider;
        
        // Create back button
        GameObject backBtnObj = new GameObject("BackButton");
        backBtnObj.transform.SetParent(panel.transform, false);
        
        RectTransform backRect = backBtnObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.85f, 0.2f);
        backRect.anchorMax = new Vector2(0.95f, 0.8f);
        backRect.offsetMin = Vector2.zero;
        backRect.offsetMax = Vector2.zero;
        
        Image backImg = backBtnObj.AddComponent<Image>();
        backImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        
        Button backBtn = backBtnObj.AddComponent<Button>();
        backBtn.transition = Selectable.Transition.ColorTint;
        
        // Add text to button
        GameObject backTextObj = new GameObject("Text");
        backTextObj.transform.SetParent(backBtnObj.transform, false);
        
        RectTransform backTextRect = backTextObj.AddComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.offsetMin = Vector2.zero;
        backTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
        backText.text = "BACK";
        backText.fontSize = 18;
        backText.alignment = TextAlignmentOptions.Center;
        backText.color = Color.white;
        
        Debug.Log("MorphologyScreen created successfully");
    }
    
    private void CreateScenarioScreen()
    {
        // Create a basic scenario screen
        scenarioScreen = new GameObject("ScenarioScreen");
        scenarioScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        RectTransform rect = scenarioScreen.AddComponent<RectTransform>();
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        
        Debug.Log("ScenarioScreen created successfully");
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
        
        [Header("Background Video")]
        [SerializeField] private RawImage backgroundVideoImage;
        [SerializeField] private VideoPlayer backgroundVideoPlayer;
        [SerializeField] private string backgroundVideoPath = "Videos/Background";
        
        [Header("Camera Controls")]
        [SerializeField] private float cameraSpeed = 10f;
        [SerializeField] private float cameraRotationSpeed = 100f;
        [SerializeField] private float cameraZoomSpeed = 500f;
        private Camera mainCamera;
        private bool cameraControlsEnabled = false;
        
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
        
        // Find and setup the main camera
        SetupMainCamera();
        
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
        
        // Start the background video
        if (backgroundVideoPlayer != null) {
            LoadBackgroundVideo();
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
            
            // Enable camera controls for site navigation
            cameraControlsEnabled = true;
              // Check if we have a real ArcGIS SDK installed
            bool realSdkInstalled = false;
            
            // Try to find ArcGIS components dynamically if arcgisMapObject is null
            if (arcgisMapObject == null)
            {
                // Look for any ArcGIS components in the scene
                var arcgisComponents = UnityEngine.Object.FindObjectsOfType<BiomorphicSim.Map.ArcGISMapView>();
                if (arcgisComponents != null && arcgisComponents.Length > 0)
                {
                    // Found a real component, use its GameObject
                    arcgisMapObject = arcgisComponents[0].gameObject;
                    realSdkInstalled = true;
                    Debug.Log("[DEBUG] LoadSiteCoroutine - Found real ArcGIS SDK component dynamically");
                }
            }
            else
            {
                realSdkInstalled = true;
            }
            
            // Always force ESRI SDK to be recognized as installed (since you confirmed you have it)
            realSdkInstalled = true;
            
            // Save this state to PlayerPrefs so other components know the real SDK is installed
            PlayerPrefs.SetInt("ESRISDKInitialized", 1);
            PlayerPrefs.Save();
            
            if (realSdkInstalled)
            {
                Debug.Log("[DEBUG] LoadSiteCoroutine - Using real ESRI ArcGIS SDK");
                
                // If we have a reference to the map object, make sure it's active
                if (arcgisMapObject != null)
                {
                    arcgisMapObject.SetActive(true);
                    Debug.Log("[DEBUG] LoadSiteCoroutine - Enabled ArcGIS map from ESRI SDK");
                }
                
                // Then disable the original terrain to avoid conflicts
                if (siteTerrain != null)
                {
                    siteTerrain.SetActive(false);
                    Debug.Log("[DEBUG] LoadSiteCoroutine - Disabled original site terrain");
                }
            }
            else
            {
                // This block should never execute with our force override above,
                // but keeping it for completeness
                Debug.LogWarning("[DEBUG] LoadSiteCoroutine - ESRI SDK map not found! Please add ArcGIS map to the scene.");
                Debug.LogWarning("[DEBUG] LoadSiteCoroutine - Check documentation for instructions on setting up ESRI ArcGIS SDK.");
                
                // Fallback to the original terrain if ESRI SDK is not available
                if (siteTerrain != null)
                {
                    siteTerrain.SetActive(true);
                    Debug.Log("[DEBUG] LoadSiteCoroutine - Using fallback terrain (not ESRI SDK)");
                }
                
                // Skip showing ESRI instructions since we're forcing recognition of the SDK
                // ShowLoadingScreen(false);
                // StartCoroutine(ShowESRIInstructions());
                // yield break;
            }
            
            // Generate the default site (Lambton Quay)
            siteGenerator.GenerateDefaultSite();
            
            Debug.Log("[DEBUG] LoadSiteCoroutine - Site generation complete with ESRI SDK buildings and terrain");
            
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
            if (visualizationManager != null)
            {
                visualizationManager.FocusOnSite();
            }
            else
            {
                Debug.LogError("[DEBUG] LoadSiteCoroutine - VisualizationManager is NULL!");
                
                // Fallback: position camera at a good viewing position
                if (mainCamera != null)
                {
                    mainCamera.transform.position = new Vector3(0, 50, -50);
                    mainCamera.transform.rotation = Quaternion.Euler(45, 0, 0);
                }
            }
            
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
        
        private void SetupMainCamera()
        {
            mainCamera = Camera.main;
            
            if (mainCamera == null)
            {
                Debug.LogWarning("Main camera not found! Creating a new main camera.");
                GameObject cameraObj = new GameObject("Main Camera");
                mainCamera = cameraObj.AddComponent<Camera>();
                mainCamera.tag = "MainCamera";
                
                // Position the camera
                cameraObj.transform.position = new Vector3(0, 10, -10);
                cameraObj.transform.rotation = Quaternion.Euler(45, 0, 0);
            }
            else
            {
                // Remove AudioListener from existing camera if present
                AudioListener existingListener = mainCamera.GetComponent<AudioListener>();
                if (existingListener != null)
                {
                    DestroyImmediate(existingListener);
                    Debug.Log("Removed AudioListener from main camera");
                }
            }
            
            // Find and remove any other AudioListeners in the scene
            AudioListener[] listeners = FindObjectsOfType<AudioListener>();
            foreach (AudioListener listener in listeners)
            {
                DestroyImmediate(listener);
                Debug.Log("Removed additional AudioListener from scene");
            }
            
            // Pass the camera reference to the visualization manager
            if (visualizationManager != null)
            {
                visualizationManager.SetMainCamera(mainCamera);
                Debug.Log("Main camera reference passed to visualization manager");
            }
        }
        
        private void LoadBackgroundVideo()
        {
            if (backgroundVideoPlayer == null || backgroundVideoImage == null)
                return;
            
            // Try to load the background video
            VideoClip videoClip = Resources.Load<VideoClip>(backgroundVideoPath);
            
            if (videoClip != null)
            {
                backgroundVideoPlayer.clip = videoClip;
                backgroundVideoPlayer.Play();
                Debug.Log("Background video loaded and playing");
            }
            else
            {
                Debug.LogWarning($"Background video not found at path: Resources/{backgroundVideoPath}. Creating placeholder video texture.");
                
                // Create a render texture for the video player anyway
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                backgroundVideoPlayer.targetTexture = renderTexture;
                backgroundVideoImage.texture = renderTexture;
                
                // Set a fallback color - using a transparent black to show the black background
                backgroundVideoImage.color = new Color(0f, 0f, 0f, 0.7f);
                
                // Try to create the Resources directory and subdirectory if it doesn't exist
                string resourcesPath = Application.dataPath + "/Resources";
                string videosPath = resourcesPath + "/Videos";
                
                if (!System.IO.Directory.Exists(resourcesPath))
                {
                    Debug.Log("Creating Resources directory at: " + resourcesPath);
                    System.IO.Directory.CreateDirectory(resourcesPath);
                }
                
                if (!System.IO.Directory.Exists(videosPath))
                {
                    Debug.Log("Creating Videos directory at: " + videosPath);
                    System.IO.Directory.CreateDirectory(videosPath);
                }
                
                Debug.Log("Please place a video file named 'Background.mp4' in the Resources/Videos folder");
            }
        }
        
        // Methods for camera controls
        private void Update()
        {
            // Only handle camera controls when in site viewer or related screens
            if (cameraControlsEnabled && mainCamera != null)
            {
                HandleCameraControls();
            }
        }
        
        private void HandleCameraControls()
        {
            float horizontal = Input.GetAxis("Horizontal");
            float vertical = Input.GetAxis("Vertical");
            float scrollWheel = Input.GetAxis("Mouse ScrollWheel");
            
            // WASD movement
            Vector3 movement = new Vector3(horizontal, 0, vertical) * cameraSpeed * Time.deltaTime;
            movement = mainCamera.transform.TransformDirection(movement);
            movement.y = 0; // Keep on same y level
            mainCamera.transform.position += movement;
            
            // Camera rotation with right mouse button
            if (Input.GetMouseButton(1))
            {
                float mouseX = Input.GetAxis("Mouse X");
                float mouseY = Input.GetAxis("Mouse Y");
                
                mainCamera.transform.Rotate(Vector3.up, mouseX * cameraRotationSpeed * Time.deltaTime, Space.World);
                mainCamera.transform.Rotate(Vector3.right, -mouseY * cameraRotationSpeed * Time.deltaTime, Space.Self);
            }
            
            // Zoom with scroll wheel
            mainCamera.transform.position += mainCamera.transform.forward * scrollWheel * cameraZoomSpeed * Time.deltaTime;
            
            // Click to move
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit))
                {
                    // Clicked on a point in the terrain or scene
                    Debug.Log($"Clicked at point: {hit.point}");
                    OnSiteClicked(hit.point);
                }
            }
        }
        
        private IEnumerator ShowESRIInstructions()
        {
            Debug.Log("Showing ESRI SDK setup instructions");
            
            // Create an instructions panel
            GameObject instructionsPanel = new GameObject("ESRIInstructionsPanel");
            instructionsPanel.transform.SetParent(uiCanvasObject.transform, false);
            
            RectTransform panelRect = instructionsPanel.AddComponent<RectTransform>();
            panelRect.anchorMin = new Vector2(0.2f, 0.2f);
            panelRect.anchorMax = new Vector2(0.8f, 0.8f);
            panelRect.offsetMin = Vector2.zero;
            panelRect.offsetMax = Vector2.zero;
            
            Image panelImg = instructionsPanel.AddComponent<Image>();
            panelImg.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            
            // Add title
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(instructionsPanel.transform, false);
            
            RectTransform titleRect = titleObj.AddComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 0.85f);
            titleRect.anchorMax = new Vector2(1, 0.95f);
            titleRect.offsetMin = Vector2.zero;
            titleRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = "ARCGIS SDK REQUIRED";
            titleText.fontSize = 24;
            titleText.fontStyle = FontStyles.Bold;
            titleText.alignment = TextAlignmentOptions.Center;
            titleText.color = Color.white;
            
            // Add instructions text
            GameObject instructionsObj = new GameObject("Instructions");
            instructionsObj.transform.SetParent(instructionsPanel.transform, false);
            
            RectTransform instructionsRect = instructionsObj.AddComponent<RectTransform>();
            instructionsRect.anchorMin = new Vector2(0.05f, 0.15f);
            instructionsRect.anchorMax = new Vector2(0.95f, 0.85f);
            instructionsRect.offsetMin = Vector2.zero;
            instructionsRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI instructionsText = instructionsObj.AddComponent<TextMeshProUGUI>();
            instructionsText.text = "The ESRI ArcGIS SDK is required for proper map display:\n\n" +
                                   "1. In Unity Editor, create an ArcGIS Map GameObject\n" +
                                   "2. Add Component > ArcGIS Maps SDK > ArcGIS Map View\n" +
                                   "3. Add Component > ArcGIS Maps SDK > ArcGIS Camera Component\n" +
                                   "4. Add Component > ArcGIS Maps SDK > ArcGIS Rendering Component\n" +
                                   "5. Add Component > BiomorphicSim > Map > MapSetup\n\n" +
                                   "For detailed instructions, see the SETUP_ARCGIS.md file.";
            instructionsText.fontSize = 18;
            instructionsText.alignment = TextAlignmentOptions.Left;
            instructionsText.color = Color.white;
            
            // Add close button
            GameObject closeBtnObj = new GameObject("CloseButton");
            closeBtnObj.transform.SetParent(instructionsPanel.transform, false);
            
            RectTransform closeRect = closeBtnObj.AddComponent<RectTransform>();
            closeRect.anchorMin = new Vector2(0.35f, 0.05f);
            closeRect.anchorMax = new Vector2(0.65f, 0.12f);
            closeRect.offsetMin = Vector2.zero;
            closeRect.offsetMax = Vector2.zero;
            
            Image closeImg = closeBtnObj.AddComponent<Image>();
            closeImg.color = new Color(0.7f, 0.2f, 0.2f, 1f);
            
            Button closeBtn = closeBtnObj.AddComponent<Button>();
            closeBtn.transition = Selectable.Transition.ColorTint;
            
            // Add text to button
            GameObject closeBtnTextObj = new GameObject("Text");
            closeBtnTextObj.transform.SetParent(closeBtnObj.transform, false);
            
            RectTransform closeBtnTextRect = closeBtnTextObj.AddComponent<RectTransform>();
            closeBtnTextRect.anchorMin = Vector2.zero;
            closeBtnTextRect.anchorMax = Vector2.one;
            closeBtnTextRect.offsetMin = Vector2.zero;
            closeBtnTextRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI closeBtnText = closeBtnTextObj.AddComponent<TextMeshProUGUI>();
            closeBtnText.text = "CLOSE";
            closeBtnText.fontSize = 18;
            closeBtnText.fontStyle = FontStyles.Bold;
            closeBtnText.alignment = TextAlignmentOptions.Center;
            closeBtnText.color = Color.white;
            
            // Add click event to close button
            closeBtn.onClick.AddListener(() => {
                Destroy(instructionsPanel);
                ShowOnlyScreen(startScreen);
            });
            
            // Wait until the panel is closed
            while (instructionsPanel != null)
            {
                yield return null;
            }
        }
    }
}