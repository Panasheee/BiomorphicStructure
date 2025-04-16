using System.Collections;
using System.Collections.Generic; // Add this import for List<>
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.Video;
using TMPro;
using BiomorphicSim.Core;
using BiomorphicSim.Morphology;
using BiomorphicSim.UI;
using BiomorphicSim.Map;

/// <summary>
/// Single bootstrap script that creates and configures the entire Biomorphic Structure Simulator through code.
/// Add this to an empty GameObject in a new scene to automatically set up the entire system.
/// </summary>
public class BiomorphicSimulatorBootstrap : MonoBehaviour
{
    // Reference to the main camera
    [SerializeField] private Camera mainCamera;
    
    // Prefab references (create these manually and assign them)
    [SerializeField] private GameObject nodePrefab;
    [SerializeField] private GameObject connectionPrefab;
    [SerializeField] private GameObject vectorFieldPrefab;
    [SerializeField] private GameObject zoneSelectionPrefab;
    
    // GameObject hierarchy references (will be created programmatically)
    private GameObject coreObject;
    private GameObject morphologyObject;
    private GameObject uiObject;
    private GameObject visualizationObject;
    private GameObject siteObject;
    
    // Component references (will be created programmatically)
    private MainController mainController;
    private SiteGenerator siteGenerator;
    private MorphologyManager morphologyManager;
    private ScenarioAnalyzer scenarioAnalyzer;
    private VisualizationManager visualizationManager;
    private ModernUIController uiController;
    private SiteInteractionController interactionController;
    
    private void Awake()
    {
        // If no prefabs are assigned, create basic ones
        CreateDefaultPrefabsIfNeeded();
        
        // Create main camera if not assigned
        if (mainCamera == null)
        {
            Debug.Log("Creating main camera");
            GameObject cameraObject = new GameObject("Main Camera");
            mainCamera = cameraObject.AddComponent<Camera>();
            cameraObject.tag = "MainCamera";
            
            // Set up camera for black background
            mainCamera.clearFlags = CameraClearFlags.SolidColor;
            mainCamera.backgroundColor = Color.black;
            
            // Position camera
            mainCamera.transform.position = new Vector3(0, 50, -100);
            mainCamera.transform.LookAt(Vector3.zero);
        }
        
        // Create and set up the entire system
        SetupSystem();
    }
    
    private void CreateDefaultPrefabsIfNeeded()
    {
        // Create node prefab if not assigned
        if (nodePrefab == null)
        {
            Debug.Log("Creating default node prefab");            nodePrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            nodePrefab.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            
            // Create and assign material with a standard shader that exists in all Unity projects
            Material nodeMaterial = new Material(Shader.Find("Standard"));
            if (nodeMaterial == null) {
                // Fallback to diffuse if Standard isn't available
                nodeMaterial = new Material(Shader.Find("Diffuse"));
            }
            if (nodeMaterial != null) {
                nodeMaterial.color = Color.white;
                nodePrefab.GetComponent<Renderer>().material = nodeMaterial;
            }
            
            // Make it a prefab (in runtime, this just detaches it from the scene)
            nodePrefab.SetActive(false);
        }
        
        // Create connection prefab if not assigned
        if (connectionPrefab == null)
        {
            Debug.Log("Creating default connection prefab");            connectionPrefab = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            connectionPrefab.transform.localScale = new Vector3(0.1f, 1.0f, 0.1f);
            
            // Create and assign material with a standard shader that exists in all Unity projects
            Material connectionMaterial = new Material(Shader.Find("Standard"));
            if (connectionMaterial == null) {
                // Fallback to diffuse if Standard isn't available
                connectionMaterial = new Material(Shader.Find("Diffuse"));
            }
            if (connectionMaterial != null) {
                connectionMaterial.color = Color.white;
                connectionPrefab.GetComponent<Renderer>().material = connectionMaterial;
            }
            
            // Make it a prefab
            connectionPrefab.SetActive(false);
        }
        
        // Create vector field prefab if not assigned
        if (vectorFieldPrefab == null)
        {
            Debug.Log("Creating default vector field prefab");
            
            // Create arrow shape
            GameObject shaft = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            shaft.transform.localScale = new Vector3(0.05f, 0.5f, 0.05f);
            
            // Create arrow tip using a cylinder since Cone is not a primitive type
            GameObject tip = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            tip.transform.localScale = new Vector3(0.15f, 0.3f, 0.15f);
            // Make the cylinder taper to look like a cone by scaling only one end
            tip.transform.position = new Vector3(0, 0.4f, 0);
            // Add a tapered look by angling the end faces
            tip.transform.rotation = Quaternion.Euler(0, 0, 0);
              // Create parent object (assign directly to the class field)
            vectorFieldPrefab = new GameObject("VectorArrow");
            shaft.transform.parent = vectorFieldPrefab.transform;
            tip.transform.parent = vectorFieldPrefab.transform;
            
            // Create and assign material with a standard shader that exists in all Unity projects
            Material vectorMaterial = new Material(Shader.Find("Standard"));
            if (vectorMaterial == null) {
                // Fallback to diffuse if Standard isn't available
                vectorMaterial = new Material(Shader.Find("Diffuse"));
            }
            if (vectorMaterial != null) {
                vectorMaterial.color = Color.blue;
                shaft.GetComponent<Renderer>().material = vectorMaterial;
                tip.GetComponent<Renderer>().material = vectorMaterial;
            }
              // Make it a prefab
            vectorFieldPrefab.SetActive(false);
            
            // Make sure the prefab was successfully created
            if (vectorFieldPrefab == null)
            {
                Debug.LogError("vectorFieldPrefab is still null after creation! Check CreateDefaultPrefabsIfNeeded.");
                return;
            }
        }
        
        // Create zone selection prefab if not assigned
        if (zoneSelectionPrefab == null)
        {
            Debug.Log("Creating default zone selection prefab");
            zoneSelectionPrefab = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            zoneSelectionPrefab.transform.localScale = new Vector3(3, 3, 3);
              // Create and assign material with proper fallbacks
            Shader urpShader = Shader.Find("Universal Render Pipeline/Lit");
            if (urpShader == null)
            {
                Debug.LogWarning("URP Shader not found. Falling back to Standard shader.");
                urpShader = Shader.Find("Standard");
                
                if (urpShader == null)
                {
                    Debug.LogWarning("Standard shader not found. Falling back to Diffuse.");
                    urpShader = Shader.Find("Diffuse");
                }
            }
            
            if (urpShader == null)
            {
                Debug.LogError("No suitable shader found. Cannot create zone selection material.");
                return;
            }
            
            Material selectionMaterial = new Material(urpShader);
            selectionMaterial.color = new Color(1, 1, 1, 0.3f);
            
            // These properties are URP-specific, only set them if we're using the URP shader
            if (urpShader.name.Contains("Universal Render Pipeline"))
            {
                selectionMaterial.SetFloat("_Surface", 1); // Set to transparent
                selectionMaterial.SetFloat("_Blend", 0); // Alpha blend
            }
            else
            {
                // For standard shader, enable transparency differently
                selectionMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
                selectionMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
                selectionMaterial.SetInt("_ZWrite", 0);
                selectionMaterial.DisableKeyword("_ALPHATEST_ON");
                selectionMaterial.EnableKeyword("_ALPHABLEND_ON");
                selectionMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
                selectionMaterial.renderQueue = 3000;
            }
            
            zoneSelectionPrefab.GetComponent<Renderer>().material = selectionMaterial;
            
            // Make it a prefab
            zoneSelectionPrefab.SetActive(false);
        }
    }    private void SetupSystem()
    {
        Debug.Log("Setting up Biomorphic Structure Simulator...");
        
        // Create main GameObject hierarchy
        GameObject rootObject = new GameObject("BiomorphicSimulator");
        
        // Create UI components first
        uiObject = new GameObject("UI");
        uiObject.transform.parent = rootObject.transform;
        
        // Initialize UI system
        uiController = uiObject.AddComponent<ModernUIController>();
        
        // Create a single UICanvas for all screens
        GameObject uiCanvasObject = new GameObject("UICanvas");
        uiCanvasObject.transform.SetParent(uiObject.transform, false);
        
        // Add Canvas component
        Canvas uiCanvas = uiCanvasObject.AddComponent<Canvas>();
        uiCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        
        // Add CanvasScaler for proper UI scaling
        CanvasScaler scaler = uiCanvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920, 1080);
        
        // Add GraphicRaycaster for UI interaction
        uiCanvasObject.AddComponent<GraphicRaycaster>();
          // UI screens will be created later in SetupUI
        
        // Create Core components
        coreObject = new GameObject("Core");
        coreObject.transform.parent = rootObject.transform;
        
        mainController = coreObject.AddComponent<MainController>();
        scenarioAnalyzer = coreObject.AddComponent<ScenarioAnalyzer>();
        interactionController = coreObject.AddComponent<SiteInteractionController>();
          // Create Site components
        siteObject = new GameObject("Site");
        siteObject.transform.parent = rootObject.transform;
        
        GameObject terrainObject = new GameObject("Terrain");
        terrainObject.transform.parent = siteObject.transform;
        // Add a mesh to the terrain
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainObject.AddComponent<MeshRenderer>();
        // Create a simple plane mesh for the terrain
        meshFilter.mesh = CreateTerrainMesh(100, 100);
        // Add the TerrainMaterialFixer to automatically create a proper material
        TerrainMaterialFixer materialFixer = terrainObject.AddComponent<BiomorphicSim.Map.TerrainMaterialFixer>();
        
        GameObject buildingsObject = new GameObject("Buildings");
        buildingsObject.transform.parent = siteObject.transform;
        
        GameObject roadsObject = new GameObject("Roads");
        roadsObject.transform.parent = siteObject.transform;
        
        siteGenerator = siteObject.AddComponent<SiteGenerator>();
        
        // Create Morphology components
        morphologyObject = new GameObject("Morphology");
        morphologyObject.transform.parent = rootObject.transform;
        
        GameObject nodesObject = new GameObject("Nodes");
        nodesObject.transform.parent = morphologyObject.transform;
        
        GameObject connectionsObject = new GameObject("Connections");
        connectionsObject.transform.parent = morphologyObject.transform;
        
        morphologyManager = morphologyObject.AddComponent<MorphologyManager>();
        
        // Create Visualization components
        visualizationObject = new GameObject("Visualization");
        visualizationObject.transform.parent = rootObject.transform;
        
        GameObject overlaysObject = new GameObject("Overlays");
        overlaysObject.transform.parent = visualizationObject.transform;
        
        visualizationManager = visualizationObject.AddComponent<VisualizationManager>();
        
        // Create UI components
        SetupUI(rootObject);
        
        // Configure component references
        ConfigureComponents();
        
        Debug.Log("Biomorphic Structure Simulator setup complete!");
    }    private void SetupUI(GameObject parent)    {
        // Always create a new EventSystem to ensure UI interactions work
        // First destroy any existing EventSystem to avoid conflicts
        EventSystem[] existingSystems = FindObjectsOfType<EventSystem>();
        foreach (EventSystem es in existingSystems)
        {
            DestroyImmediate(es.gameObject);
        }
        
        // Create a new EventSystem
        GameObject eventSystem = new GameObject("EventSystem");
        eventSystem.transform.parent = parent.transform;
        eventSystem.AddComponent<EventSystem>();
        eventSystem.AddComponent<StandaloneInputModule>();
        
        Debug.Log("Created new EventSystem for UI interaction");
        
        // Get the existing UICanvas that was created in SetupSystem
        GameObject uiCanvasObject = uiObject.transform.Find("UICanvas")?.gameObject;
        
        if (uiCanvasObject == null)
        {
            Debug.LogError("Could not find UICanvas! Unable to create UI screens.");
            return;
        }
        
        // Create UI Screens
        CreateStartScreen(uiCanvasObject);
        CreateSiteViewerScreen(uiCanvasObject);
        CreateSiteAnalysisScreen(uiCanvasObject);
        CreateMorphologyScreen(uiCanvasObject);
        CreateScenarioScreen(uiCanvasObject);
    }
    
    /// <summary>
    /// Creates the start screen UI with animated background video
    /// </summary>
    private void CreateStartScreen(GameObject uiCanvasObject)
    {
        GameObject startScreen = new GameObject("StartScreen");
        startScreen.transform.SetParent(uiCanvasObject.transform, false);
        
        // Add RectTransform component (needed for UI positioning)
        RectTransform rectTransform = startScreen.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(startScreen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = Color.black;
        
        // Add the animated background
        GameObject backgroundObj = new GameObject("AnimatedBackground");
        backgroundObj.transform.SetParent(panel.transform, false);
        
        RectTransform bgRect = backgroundObj.AddComponent<RectTransform>();
        bgRect.anchorMin = Vector2.zero;
        bgRect.anchorMax = Vector2.one;
        bgRect.offsetMin = Vector2.zero;
        bgRect.offsetMax = Vector2.zero;
        
        // Add RawImage component for displaying the video
        RawImage backgroundImage = backgroundObj.AddComponent<RawImage>();
        backgroundImage.color = Color.white;
          // Set up video player with looping enabled
        VideoPlayer videoPlayer = backgroundObj.AddComponent<VideoPlayer>();
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.targetTexture = new RenderTexture(1920, 1080, 24);
        videoPlayer.isLooping = true; // Ensure looping is explicitly enabled
        videoPlayer.playOnAwake = true;
        videoPlayer.waitForFirstFrame = true;
        videoPlayer.playbackSpeed = 1.0f;
        videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // No audio needed for background
        
        // Look for a background video in Resources folder
        string[] videoNames = { "BackgroundLoop", "BiomorphicBackground", "CityBackground" };
        foreach (string videoName in videoNames)
        {
            VideoClip clip = Resources.Load<VideoClip>(videoName);
            if (clip != null)
            {
                videoPlayer.clip = clip;
                Debug.Log($"BiomorphicSim: Using background video: {videoName}");
                break;
            }
        }
        
        // If no video was found, look in the streaming assets folder
        if (videoPlayer.clip == null)
        {
            string[] extensions = { ".mp4", ".webm", ".mov" };
            foreach (string ext in extensions)
            {
                string videoPath = System.IO.Path.Combine(Application.streamingAssetsPath, "BackgroundVideo" + ext);
                if (System.IO.File.Exists(videoPath))
                {
                    videoPlayer.url = videoPath;
                    Debug.Log($"BiomorphicSim: Using background video from streaming assets: {videoPath}");
                    break;
                }
            }
        }
        
        // If still no video was found, log a warning
        if (videoPlayer.clip == null && string.IsNullOrEmpty(videoPlayer.url))
        {
            Debug.LogWarning("BiomorphicSim: No background video found. Please add a video file to Resources or StreamingAssets folder.");
        }
        
        // Assign the video's render texture to the RawImage
        backgroundImage.texture = videoPlayer.targetTexture;
        
        // Add title
        GameObject titleObj = new GameObject("Title");
        titleObj.transform.SetParent(panel.transform, false);
        
        RectTransform titleRect = titleObj.AddComponent<RectTransform>();
        titleRect.anchorMin = new Vector2(0.5f, 0.7f);
        titleRect.anchorMax = new Vector2(0.5f, 0.9f);
        titleRect.sizeDelta = new Vector2(800, 200);
        titleRect.anchoredPosition = Vector2.zero;
          TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
        titleText.text = "BIOMORPHIC STRUCTURE\n\nSIMULATOR";
        titleText.fontSize = 72;
        titleText.fontStyle = FontStyles.Bold;
        titleText.alignment = TextAlignmentOptions.Center;
        titleText.color = Color.white;
          // Add load site button
        GameObject buttonObj = new GameObject("LoadSiteButton");        buttonObj.transform.SetParent(panel.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.5f, 0.3f);
        buttonRect.anchorMax = new Vector2(0.5f, 0.4f);
        buttonRect.sizeDelta = new Vector2(300, 80);
        buttonRect.anchoredPosition = Vector2.zero;
        
        // Add image component
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = Color.white; // Pure white
        // Make the button a pill shape with rounded corners
        buttonImage.sprite = CreateRoundedRectSprite(300, 80, 40); // 40px corner radius for pill shape
        
        Button loadSiteButton = buttonObj.AddComponent<Button>();
        loadSiteButton.targetGraphic = buttonImage;
        
        // Configure button navigation to improve focus handling
        Navigation navigation = loadSiteButton.navigation;
        navigation.mode = Navigation.Mode.Explicit;
        loadSiteButton.navigation = navigation;
          // Add direct onClick handler to the button instead of using EventTrigger
        loadSiteButton.onClick.AddListener(() => {
            Debug.Log("Load Site button clicked directly");
            CallMethod(uiController, "OnLoadSiteClicked");
        });
        
        // Add button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = buttonTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "LOAD SITE";
        buttonText.fontSize = 24;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.black; // Black text on white/transparent button
    }
      private void CreateSiteViewerScreen(GameObject uiCanvasObject)
    {
        GameObject screen = new GameObject("SiteViewerScreen");
        screen.transform.SetParent(uiCanvasObject.transform, false);
        screen.SetActive(false); // Start inactive
        
        // Add RectTransform component
        RectTransform rectTransform = screen.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(screen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f); // Semi-transparent black
        
        // Add run analysis button
        GameObject buttonObj = new GameObject("RunAnalysisButton");
        buttonObj.transform.SetParent(panel.transform, false);
        
        RectTransform buttonRect = buttonObj.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0.9f, 0.9f);
        buttonRect.anchorMax = new Vector2(0.98f, 0.98f);
        buttonRect.anchoredPosition = Vector2.zero;
        
        Image buttonImage = buttonObj.AddComponent<Image>();
        buttonImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        Button runAnalysisButton = buttonObj.AddComponent<Button>();
        runAnalysisButton.targetGraphic = buttonImage;
        
        // Add button text
        GameObject buttonTextObj = new GameObject("Text");
        buttonTextObj.transform.SetParent(buttonObj.transform, false);
        
        RectTransform textRect = buttonTextObj.AddComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI buttonText = buttonTextObj.AddComponent<TextMeshProUGUI>();
        buttonText.text = "RUN ANALYSIS";
        buttonText.fontSize = 16;
        buttonText.fontStyle = FontStyles.Bold;
        buttonText.alignment = TextAlignmentOptions.Center;
        buttonText.color = Color.white;
        
        // Add back button
        GameObject backObj = new GameObject("BackButton");
        backObj.transform.SetParent(panel.transform, false);
        
        RectTransform backRect = backObj.AddComponent<RectTransform>();
        backRect.anchorMin = new Vector2(0.02f, 0.95f);
        backRect.anchorMax = new Vector2(0.12f, 0.98f);
        backRect.anchoredPosition = Vector2.zero;
        
        Image backImage = backObj.AddComponent<Image>();
        backImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        Button backButton = backObj.AddComponent<Button>();
        backButton.targetGraphic = backImage;
        
        // Add button text
        GameObject backTextObj = new GameObject("Text");
        backTextObj.transform.SetParent(backObj.transform, false);
        
        RectTransform backTextRect = backTextObj.AddComponent<RectTransform>();
        backTextRect.anchorMin = Vector2.zero;
        backTextRect.anchorMax = Vector2.one;
        backTextRect.offsetMin = Vector2.zero;
        backTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI backText = backTextObj.AddComponent<TextMeshProUGUI>();
        backText.text = "BACK";
        backText.fontSize = 14;
        backText.alignment = TextAlignmentOptions.Center;
        backText.color = Color.white;
    }
      private void CreateSiteAnalysisScreen(GameObject uiCanvasObject)
    {
        GameObject screen = new GameObject("SiteAnalysisScreen");
        screen.transform.SetParent(uiCanvasObject.transform, false);
        screen.SetActive(false); // Start inactive
        
        // Add RectTransform component
        RectTransform rectTransform = screen.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(screen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Add overlay toggles container
        GameObject togglesObj = new GameObject("OverlayToggles");
        togglesObj.transform.SetParent(panel.transform, false);
        
        RectTransform togglesRect = togglesObj.AddComponent<RectTransform>();
        togglesRect.anchorMin = new Vector2(0.02f, 0.7f);
        togglesRect.anchorMax = new Vector2(0.2f, 0.9f);
        togglesRect.anchoredPosition = Vector2.zero;
        
        // Add wind overlay toggle
        CreateToggle(togglesObj, "WindOverlayToggle", "Wind Overlay", 0);
        
        // Add sun exposure toggle
        CreateToggle(togglesObj, "SunExposureToggle", "Sun Exposure", 1);
        
        // Add pedestrian toggle
        CreateToggle(togglesObj, "PedestrianToggle", "Pedestrian Activity", 2);
        
        // Add report panel
        GameObject reportObj = new GameObject("ReportPanel");
        reportObj.transform.SetParent(panel.transform, false);
        
        RectTransform reportRect = reportObj.AddComponent<RectTransform>();
        reportRect.anchorMin = new Vector2(0.75f, 0.6f);
        reportRect.anchorMax = new Vector2(0.98f, 0.95f);
        reportRect.anchoredPosition = Vector2.zero;
        
        Image reportImage = reportObj.AddComponent<Image>();
        reportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.7f);
        
        // Add report title
        GameObject reportTitleObj = new GameObject("ReportTitle");
        reportTitleObj.transform.SetParent(reportObj.transform, false);
        
        RectTransform reportTitleRect = reportTitleObj.AddComponent<RectTransform>();
        reportTitleRect.anchorMin = new Vector2(0, 0.9f);
        reportTitleRect.anchorMax = new Vector2(1, 1);
        reportTitleRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI reportTitle = reportTitleObj.AddComponent<TextMeshProUGUI>();
        reportTitle.text = "REPORT SUMMARY";
        reportTitle.fontSize = 18;
        reportTitle.fontStyle = FontStyles.Bold;
        reportTitle.alignment = TextAlignmentOptions.Center;
        reportTitle.color = Color.white;
        
        // Add report content
        GameObject reportContentObj = new GameObject("ReportContent");
        reportContentObj.transform.SetParent(reportObj.transform, false);
        
        RectTransform reportContentRect = reportContentObj.AddComponent<RectTransform>();
        reportContentRect.anchorMin = new Vector2(0.05f, 0.05f);
        reportContentRect.anchorMax = new Vector2(0.95f, 0.85f);
        reportContentRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI reportContent = reportContentObj.AddComponent<TextMeshProUGUI>();
        reportContent.text = "Analysis data will appear here.";
        reportContent.fontSize = 14;
        reportContent.alignment = TextAlignmentOptions.Left;
        reportContent.color = Color.white;
        
        // Add Start Morphology button
        GameObject morphButtonObj = new GameObject("StartMorphologyButton");
        morphButtonObj.transform.SetParent(panel.transform, false);
        
        RectTransform morphButtonRect = morphButtonObj.AddComponent<RectTransform>();
        morphButtonRect.anchorMin = new Vector2(0.4f, 0.1f);
        morphButtonRect.anchorMax = new Vector2(0.6f, 0.2f);
        morphButtonRect.anchoredPosition = Vector2.zero;
        
        Image morphButtonImage = morphButtonObj.AddComponent<Image>();
        morphButtonImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        Button morphButton = morphButtonObj.AddComponent<Button>();
        morphButton.targetGraphic = morphButtonImage;
        
        // Add button text
        GameObject morphTextObj = new GameObject("Text");
        morphTextObj.transform.SetParent(morphButtonObj.transform, false);
        
        RectTransform morphTextRect = morphTextObj.AddComponent<RectTransform>();
        morphTextRect.anchorMin = Vector2.zero;
        morphTextRect.anchorMax = Vector2.one;
        morphTextRect.offsetMin = Vector2.zero;
        morphTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI morphText = morphTextObj.AddComponent<TextMeshProUGUI>();
        morphText.text = "START MORPHOLOGY";
        morphText.fontSize = 18;
        morphText.fontStyle = FontStyles.Bold;
        morphText.alignment = TextAlignmentOptions.Center;
        morphText.color = Color.white;
    }
      private void CreateMorphologyScreen(GameObject uiCanvasObject)
    {
        GameObject screen = new GameObject("MorphologyScreen");
        screen.transform.SetParent(uiCanvasObject.transform, false);
        screen.SetActive(false); // Start inactive
        
        // Add RectTransform component
        RectTransform rectTransform = screen.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(screen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Add growth speed slider
        GameObject sliderObj = new GameObject("GrowthSpeedSlider");
        sliderObj.transform.SetParent(panel.transform, false);
        
        RectTransform sliderRect = sliderObj.AddComponent<RectTransform>();
        sliderRect.anchorMin = new Vector2(0.05f, 0.9f);
        sliderRect.anchorMax = new Vector2(0.25f, 0.95f);
        sliderRect.anchoredPosition = Vector2.zero;
        
        Slider speedSlider = sliderObj.AddComponent<Slider>();
        speedSlider.minValue = 0.1f;
        speedSlider.maxValue = 10f;
        speedSlider.value = 1f;
        
        // Add slider background
        GameObject sliderBg = new GameObject("Background");
        sliderBg.transform.SetParent(sliderObj.transform, false);
        
        RectTransform sliderBgRect = sliderBg.AddComponent<RectTransform>();
        sliderBgRect.anchorMin = Vector2.zero;
        sliderBgRect.anchorMax = Vector2.one;
        sliderBgRect.offsetMin = Vector2.zero;
        sliderBgRect.offsetMax = Vector2.zero;
        
        Image sliderBgImage = sliderBg.AddComponent<Image>();
        sliderBgImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        // Add slider fill area
        GameObject fillAreaObj = new GameObject("Fill Area");
        fillAreaObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform fillAreaRect = fillAreaObj.AddComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0, 0.5f);
        fillAreaRect.anchorMax = new Vector2(1, 0.5f);
        fillAreaRect.offsetMin = new Vector2(5, -5);
        fillAreaRect.offsetMax = new Vector2(-5, 5);
        
        // Add slider fill
        GameObject fillObj = new GameObject("Fill");
        fillObj.transform.SetParent(fillAreaObj.transform, false);
        
        RectTransform fillRect = fillObj.AddComponent<RectTransform>();
        fillRect.anchorMin = new Vector2(0, 0);
        fillRect.anchorMax = new Vector2(0, 1);
        fillRect.pivot = new Vector2(0, 0.5f);
        fillRect.sizeDelta = new Vector2(10, 0);
        
        Image fillImage = fillObj.AddComponent<Image>();
        fillImage.color = Color.white;
        
        speedSlider.fillRect = fillRect;
        
        // Add slider handle
        GameObject handleObj = new GameObject("Handle");
        handleObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform handleRect = handleObj.AddComponent<RectTransform>();
        handleRect.anchorMin = new Vector2(0, 0.5f);
        handleRect.anchorMax = new Vector2(0, 0.5f);
        handleRect.sizeDelta = new Vector2(20, 20);
        
        Image handleImage = handleObj.AddComponent<Image>();
        handleImage.color = Color.white;
        
        speedSlider.handleRect = handleRect;
        
        // Add slider label
        GameObject sliderLabelObj = new GameObject("Label");
        sliderLabelObj.transform.SetParent(sliderObj.transform, false);
        
        RectTransform sliderLabelRect = sliderLabelObj.AddComponent<RectTransform>();
        sliderLabelRect.anchorMin = new Vector2(0, 1);
        sliderLabelRect.anchorMax = new Vector2(1, 1.5f);
        sliderLabelRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI sliderLabel = sliderLabelObj.AddComponent<TextMeshProUGUI>();
        sliderLabel.text = "GROWTH SPEED";
        sliderLabel.fontSize = 14;
        sliderLabel.alignment = TextAlignmentOptions.Center;
        sliderLabel.color = Color.white;
        
        // Add pause/resume button
        GameObject pauseObj = new GameObject("PauseResumeButton");
        pauseObj.transform.SetParent(panel.transform, false);
        
        RectTransform pauseRect = pauseObj.AddComponent<RectTransform>();
        pauseRect.anchorMin = new Vector2(0.05f, 0.8f);
        pauseRect.anchorMax = new Vector2(0.15f, 0.85f);
        pauseRect.anchoredPosition = Vector2.zero;
        
        Image pauseImage = pauseObj.AddComponent<Image>();
        pauseImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        Button pauseButton = pauseObj.AddComponent<Button>();
        pauseButton.targetGraphic = pauseImage;
        
        // Add button text
        GameObject pauseTextObj = new GameObject("Text");
        pauseTextObj.transform.SetParent(pauseObj.transform, false);
        
        RectTransform pauseTextRect = pauseTextObj.AddComponent<RectTransform>();
        pauseTextRect.anchorMin = Vector2.zero;
        pauseTextRect.anchorMax = Vector2.one;
        pauseTextRect.offsetMin = Vector2.zero;
        pauseTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI pauseText = pauseTextObj.AddComponent<TextMeshProUGUI>();
        pauseText.text = "PAUSE";
        pauseText.fontSize = 14;
        pauseText.alignment = TextAlignmentOptions.Center;
        pauseText.color = Color.white;
        
        // Add node count display
        GameObject nodeCountObj = new GameObject("NodeCountDisplay");
        nodeCountObj.transform.SetParent(panel.transform, false);
        
        RectTransform nodeCountRect = nodeCountObj.AddComponent<RectTransform>();
        nodeCountRect.anchorMin = new Vector2(0.05f, 0.7f);
        nodeCountRect.anchorMax = new Vector2(0.2f, 0.75f);
        nodeCountRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI nodeCountText = nodeCountObj.AddComponent<TextMeshProUGUI>();
        nodeCountText.text = "NODES: 0";
        nodeCountText.fontSize = 16;
        nodeCountText.alignment = TextAlignmentOptions.Left;
        nodeCountText.color = Color.white;
        
        // Add Run Scenario button
        GameObject scenarioObj = new GameObject("RunScenarioButton");
        scenarioObj.transform.SetParent(panel.transform, false);
        
        RectTransform scenarioRect = scenarioObj.AddComponent<RectTransform>();
        scenarioRect.anchorMin = new Vector2(0.85f, 0.9f);
        scenarioRect.anchorMax = new Vector2(0.98f, 0.95f);
        scenarioRect.anchoredPosition = Vector2.zero;
        
        Image scenarioImage = scenarioObj.AddComponent<Image>();
        scenarioImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        Button scenarioButton = scenarioObj.AddComponent<Button>();
        scenarioButton.targetGraphic = scenarioImage;
        
        // Add button text
        GameObject scenarioTextObj = new GameObject("Text");
        scenarioTextObj.transform.SetParent(scenarioObj.transform, false);
        
        RectTransform scenarioTextRect = scenarioTextObj.AddComponent<RectTransform>();
        scenarioTextRect.anchorMin = Vector2.zero;
        scenarioTextRect.anchorMax = Vector2.one;
        scenarioTextRect.offsetMin = Vector2.zero;
        scenarioTextRect.offsetMax = Vector2.zero;
        
        TextMeshProUGUI scenarioText = scenarioTextObj.AddComponent<TextMeshProUGUI>();
        scenarioText.text = "RUN SCENARIO";
        scenarioText.fontSize = 14;
        scenarioText.alignment = TextAlignmentOptions.Center;
        scenarioText.color = Color.white;
    }
      private void CreateScenarioScreen(GameObject uiCanvasObject)
    {
        GameObject screen = new GameObject("ScenarioScreen");
        screen.transform.SetParent(uiCanvasObject.transform, false);
        screen.SetActive(false); // Start inactive
        
        // Add RectTransform component
        RectTransform rectTransform = screen.AddComponent<RectTransform>();
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
        
        // Add background panel
        GameObject panel = new GameObject("Panel");
        panel.transform.SetParent(screen.transform, false);
        
        RectTransform panelRect = panel.AddComponent<RectTransform>();
        panelRect.anchorMin = Vector2.zero;
        panelRect.anchorMax = Vector2.one;
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;
        
        Image panelImage = panel.AddComponent<Image>();
        panelImage.color = new Color(0, 0, 0, 0.5f);
        
        // Add scenario dropdown
        GameObject dropdownObj = new GameObject("ScenarioDropdown");
        dropdownObj.transform.SetParent(panel.transform, false);
        
        RectTransform dropdownRect = dropdownObj.AddComponent<RectTransform>();
        dropdownRect.anchorMin = new Vector2(0.7f, 0.9f);
        dropdownRect.anchorMax = new Vector2(0.95f, 0.95f);
        dropdownRect.anchoredPosition = Vector2.zero;
        
        Image dropdownImage = dropdownObj.AddComponent<Image>();
        dropdownImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        TMP_Dropdown scenarioDropdown = dropdownObj.AddComponent<TMP_Dropdown>();
        scenarioDropdown.captionText = CreateDropdownText(dropdownObj);
        
        // Add dropdown label
        GameObject dropdownLabelObj = new GameObject("Label");
        dropdownLabelObj.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform dropdownLabelRect = dropdownLabelObj.AddComponent<RectTransform>();
        dropdownLabelRect.anchorMin = new Vector2(0, 1);
        dropdownLabelRect.anchorMax = new Vector2(1, 1.3f);
        dropdownLabelRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI dropdownLabel = dropdownLabelObj.AddComponent<TextMeshProUGUI>();
        dropdownLabel.text = "SELECT SCENARIO";
        dropdownLabel.fontSize = 14;
        dropdownLabel.alignment = TextAlignmentOptions.Center;
        dropdownLabel.color = Color.white;
        
        // Add scenario description
        GameObject descObj = new GameObject("ScenarioDescription");
        descObj.transform.SetParent(panel.transform, false);
        
        RectTransform descRect = descObj.AddComponent<RectTransform>();
        descRect.anchorMin = new Vector2(0.7f, 0.7f);
        descRect.anchorMax = new Vector2(0.95f, 0.85f);
        descRect.anchoredPosition = Vector2.zero;
        
        Image descImage = descObj.AddComponent<Image>();
        descImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        // Add description text
        GameObject descTextObj = new GameObject("Text");
        descTextObj.transform.SetParent(descObj.transform, false);
        
        RectTransform descTextRect = descTextObj.AddComponent<RectTransform>();
        descTextRect.anchorMin = new Vector2(0.05f, 0.05f);
        descTextRect.anchorMax = new Vector2(0.95f, 0.95f);
        descTextRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI descText = descTextObj.AddComponent<TextMeshProUGUI>();
        descText.text = "Scenario description will appear here.";
        descText.fontSize = 14;
        descText.alignment = TextAlignmentOptions.Left;
        descText.color = Color.white;
    }
    
    private void CreateToggle(GameObject parent, string name, string label, int position)
    {
        GameObject toggleObj = new GameObject(name);
        toggleObj.transform.SetParent(parent.transform, false);
        
        RectTransform toggleRect = toggleObj.AddComponent<RectTransform>();
        float height = 1f / 4f; // Divide the container into 4 parts
        toggleRect.anchorMin = new Vector2(0, 1f - (position + 1) * height);
        toggleRect.anchorMax = new Vector2(1, 1f - position * height);
        toggleRect.anchoredPosition = Vector2.zero;
        
        Toggle toggle = toggleObj.AddComponent<Toggle>();
        
        // Add background
        GameObject bgObj = new GameObject("Background");
        bgObj.transform.SetParent(toggleObj.transform, false);
        
        RectTransform bgRect = bgObj.AddComponent<RectTransform>();
        bgRect.anchorMin = new Vector2(0, 0.25f);
        bgRect.anchorMax = new Vector2(0.2f, 0.75f);
        bgRect.anchoredPosition = Vector2.zero;
        
        Image bgImage = bgObj.AddComponent<Image>();
        bgImage.color = new Color(0.1f, 0.1f, 0.1f);
        
        toggle.targetGraphic = bgImage;
        
        // Add checkmark
        GameObject checkObj = new GameObject("Checkmark");
        checkObj.transform.SetParent(bgObj.transform, false);
        
        RectTransform checkRect = checkObj.AddComponent<RectTransform>();
        checkRect.anchorMin = new Vector2(0.2f, 0.2f);
        checkRect.anchorMax = new Vector2(0.8f, 0.8f);
        checkRect.anchoredPosition = Vector2.zero;
        
        Image checkImage = checkObj.AddComponent<Image>();
        checkImage.color = Color.white;
        
        toggle.graphic = checkImage;
        
        // Add label
        GameObject labelObj = new GameObject("Label");
        labelObj.transform.SetParent(toggleObj.transform, false);
        
        RectTransform labelRect = labelObj.AddComponent<RectTransform>();
        labelRect.anchorMin = new Vector2(0.25f, 0);
        labelRect.anchorMax = new Vector2(1, 1);
        labelRect.anchoredPosition = Vector2.zero;
        
        TextMeshProUGUI labelText = labelObj.AddComponent<TextMeshProUGUI>();
        labelText.text = label;
        labelText.fontSize = 14;
        labelText.alignment = TextAlignmentOptions.Left;
        labelText.color = Color.white;
    }
    
    private TextMeshProUGUI CreateDropdownText(GameObject dropdownObj)
    {
        GameObject textObj = new GameObject("Text");
        textObj.transform.SetParent(dropdownObj.transform, false);
        
        RectTransform textRect = textObj.AddComponent<RectTransform>();
        textRect.anchorMin = new Vector2(0, 0);
        textRect.anchorMax = new Vector2(1, 1);
        textRect.offsetMin = new Vector2(10, 0);
        textRect.offsetMax = new Vector2(-10, 0);
        
        TextMeshProUGUI text = textObj.AddComponent<TextMeshProUGUI>();
        text.text = "Select Scenario";
        text.fontSize = 14;
        text.alignment = TextAlignmentOptions.Left;
        text.color = Color.white;
        
        return text;
    }
    
    private void ConfigureComponents()
    {
        Debug.Log("Configuring component references...");
        
        // Configure MainController references
        if (mainController != null)
        {
            // Use reflection or find fields and assign them since we can't directly access serialized fields
            AssignFieldValue(mainController, "siteGenerator", siteGenerator);
            AssignFieldValue(mainController, "morphologyManager", morphologyManager);
            AssignFieldValue(mainController, "scenarioAnalyzer", scenarioAnalyzer);
            AssignFieldValue(mainController, "uiManager", uiController);
            AssignFieldValue(mainController, "visualizationManager", visualizationManager);
        }
        
        // Configure MorphologyManager references
        if (morphologyManager != null)
        {
            AssignFieldValue(morphologyManager, "nodePrefab", nodePrefab);
            AssignFieldValue(morphologyManager, "connectionPrefab", connectionPrefab);
            AssignFieldValue(morphologyManager, "growthSpeed", 1.0f);
            AssignFieldValue(morphologyManager, "nodeDistance", 2.0f);
            AssignFieldValue(morphologyManager, "maxNodes", 500);
            AssignFieldValue(morphologyManager, "connectionRadius", 3.0f);
        }
        
        // Configure ScenarioAnalyzer references
        if (scenarioAnalyzer != null)
        {
            AssignFieldValue(scenarioAnalyzer, "windVectorPrefab", vectorFieldPrefab);
            AssignFieldValue(scenarioAnalyzer, "uiManager", uiController);
        }
        
        // Configure VisualizationManager references
        if (visualizationManager != null)
        {
            AssignFieldValue(visualizationManager, "mainCamera", mainCamera);
            AssignFieldValue(visualizationManager, "vectorFieldPrefab", vectorFieldPrefab);
        }
        
        // Configure SiteInteractionController references
        if (interactionController != null)
        {
            AssignFieldValue(interactionController, "mainCamera", mainCamera);
            AssignFieldValue(interactionController, "visualizationManager", visualizationManager);
            AssignFieldValue(interactionController, "uiController", uiController);
            AssignFieldValue(interactionController, "zoneSelectionIndicatorPrefab", zoneSelectionPrefab);
        }
        
        // Configure UI controller references
        if (uiController != null)
        {
            AssignFieldValue(uiController, "mainController", mainController);
            AssignFieldValue(uiController, "siteGenerator", siteGenerator);
            AssignFieldValue(uiController, "morphologyManager", morphologyManager);
            AssignFieldValue(uiController, "scenarioAnalyzer", scenarioAnalyzer);
            AssignFieldValue(uiController, "visualizationManager", visualizationManager);
            
            // Find and assign UI references
            AssignUIReferences();
        }
    }
      private void AssignUIReferences()
    {
        if (uiController == null) return;
        
        // Find the UICanvas under uiObject
        Transform uiCanvasTransform = uiObject.transform.Find("UICanvas");
        if (uiCanvasTransform == null)
        {
            Debug.LogError("UICanvas not found under uiObject");
            return;
        }
        
        // Now find all UI screens as children of the UICanvas
        Transform startScreen = uiCanvasTransform.Find("StartScreen");
        Transform siteViewerScreen = uiCanvasTransform.Find("SiteViewerScreen");
        Transform siteAnalysisScreen = uiCanvasTransform.Find("SiteAnalysisScreen");
        Transform morphologyScreen = uiCanvasTransform.Find("MorphologyScreen");
        Transform scenarioScreen = uiCanvasTransform.Find("ScenarioScreen");
        
        AssignFieldValue(uiController, "startScreen", startScreen?.gameObject);
        AssignFieldValue(uiController, "siteViewerScreen", siteViewerScreen?.gameObject);
        AssignFieldValue(uiController, "siteAnalysisScreen", siteAnalysisScreen?.gameObject);
        AssignFieldValue(uiController, "morphologyScreen", morphologyScreen?.gameObject);
        AssignFieldValue(uiController, "scenarioScreen", scenarioScreen?.gameObject);
        
        // Find buttons
        Button loadSiteButton = FindComponentInChildren<Button>(startScreen, "LoadSiteButton");
        Button runAnalysisButton = FindComponentInChildren<Button>(siteViewerScreen, "RunAnalysisButton");
        Button startMorphologyButton = FindComponentInChildren<Button>(siteAnalysisScreen, "StartMorphologyButton");
        Button runScenarioButton = FindComponentInChildren<Button>(morphologyScreen, "RunScenarioButton");
        Button pauseResumeButton = FindComponentInChildren<Button>(morphologyScreen, "PauseResumeButton");
        
        AssignFieldValue(uiController, "loadSiteButton", loadSiteButton);
        AssignFieldValue(uiController, "runAnalysisButton", runAnalysisButton);
        AssignFieldValue(uiController, "startMorphologyButton", startMorphologyButton);
        AssignFieldValue(uiController, "runScenarioButton", runScenarioButton);
        AssignFieldValue(uiController, "pauseResumeButton", pauseResumeButton);
        
        // Find toggles
        Toggle windOverlayToggle = FindComponentInChildren<Toggle>(siteAnalysisScreen, "WindOverlayToggle");
        Toggle sunExposureToggle = FindComponentInChildren<Toggle>(siteAnalysisScreen, "SunExposureToggle");
        Toggle pedestrianOverlayToggle = FindComponentInChildren<Toggle>(siteAnalysisScreen, "PedestrianToggle");
        
        AssignFieldValue(uiController, "windOverlayToggle", windOverlayToggle);
        AssignFieldValue(uiController, "sunExposureToggle", sunExposureToggle);
        AssignFieldValue(uiController, "pedestrianOverlayToggle", pedestrianOverlayToggle);
        
        // Find other UI elements
        TextMeshProUGUI analysisReportText = FindComponentInChildren<TextMeshProUGUI>(siteAnalysisScreen, "ReportContent");
        TextMeshProUGUI nodeCountText = FindComponentInChildren<TextMeshProUGUI>(morphologyScreen, "NodeCountDisplay");
        Slider growthSpeedSlider = FindComponentInChildren<Slider>(morphologyScreen, "GrowthSpeedSlider");
        TMP_Dropdown scenarioDropdown = FindComponentInChildren<TMP_Dropdown>(scenarioScreen, "ScenarioDropdown");
        
        AssignFieldValue(uiController, "analysisReportText", analysisReportText);
        AssignFieldValue(uiController, "nodeCountText", nodeCountText);
        AssignFieldValue(uiController, "growthSpeedSlider", growthSpeedSlider);
        AssignFieldValue(uiController, "scenarioDropdown", scenarioDropdown);
        
        // Wire up events
        ConnectUIEvents();
    }    private void ConnectUIEvents()
    {
        Debug.Log("Connecting UI events...");
        
        // Find UICanvas first
        Transform uiCanvasTransform = uiObject.transform.Find("UICanvas");
        if (uiCanvasTransform == null)
        {
            Debug.LogError("UICanvas not found under uiObject");
            return;
        }
          // Find UI Components and directly connect events with proper debug logging
        Transform startScreenTransform = uiCanvasTransform.Find("StartScreen");
        if (startScreenTransform != null)
        {
            // First find the Panel under StartScreen
            Transform panelTransform = startScreenTransform.Find("Panel");
            if (panelTransform != null)
            {
                // Then find the button under Panel
                Button loadSiteButton = panelTransform.Find("LoadSiteButton")?.GetComponent<Button>();
                if (loadSiteButton != null)
                {
                    // Clear existing listeners first to avoid duplicates
                    loadSiteButton.onClick.RemoveAllListeners();
                    loadSiteButton.onClick.AddListener(() => {
                        Debug.Log("Load Site button clicked from ConnectUIEvents!");
                        if (uiController != null)
                        {
                            CallMethod(uiController, "OnLoadSiteClicked");
                        }
                        else
                        {
                            Debug.LogError("uiController is null in ConnectUIEvents!");
                        }
                    });
                    Debug.Log("Load Site button events connected successfully");
                }
                else
                {
                    Debug.LogError("LoadSiteButton not found under Panel in StartScreen or missing Button component!");
                    // Debug the hierarchy to help diagnose
                    foreach (Transform child in panelTransform)
                    {
                        Debug.Log($"Found child under Panel: {child.name}");
                    }
                }
            }
            else
            {
                Debug.LogError("Panel not found under StartScreen!");
            }
        }
        else
        {
            Debug.LogError("StartScreen not found in UICanvas!");
        }
        
        // Connect other UI button events in a similar way with proper null checks
        Button runAnalysisButton = FindComponentInChildren<Button>(uiCanvasTransform.Find("SiteViewerScreen"), "RunAnalysisButton");
        if (runAnalysisButton != null)
        {
            runAnalysisButton.onClick.RemoveAllListeners();
            runAnalysisButton.onClick.AddListener(() => {
                Debug.Log("Run Analysis button clicked!");
                if (uiController != null) CallMethod(uiController, "OnRunAnalysisClicked");
            });
            Debug.Log("Run Analysis button events connected successfully");
        }
        
        Button startMorphologyButton = FindComponentInChildren<Button>(uiCanvasTransform.Find("SiteAnalysisScreen"), "StartMorphologyButton");
        if (startMorphologyButton != null)
        {
            startMorphologyButton.onClick.RemoveAllListeners();
            startMorphologyButton.onClick.AddListener(() => {
                Debug.Log("Start Morphology button clicked!");
                if (uiController != null) CallMethod(uiController, "OnStartMorphologyClicked");
            });
            Debug.Log("Start Morphology button events connected successfully");
        }
        
        Button pauseResumeButton = FindComponentInChildren<Button>(uiCanvasTransform.Find("MorphologyScreen"), "PauseResumeButton");
        if (pauseResumeButton != null)
        {
            pauseResumeButton.onClick.RemoveAllListeners();
            pauseResumeButton.onClick.AddListener(() => {
                Debug.Log("Pause/Resume button clicked!");
                if (uiController != null) CallMethod(uiController, "OnPauseResumeClicked");
            });
            Debug.Log("Pause/Resume button events connected successfully");
        }
        
        Button runScenarioButton = FindComponentInChildren<Button>(uiCanvasTransform.Find("MorphologyScreen"), "RunScenarioButton");
        if (runScenarioButton != null)
        {
            runScenarioButton.onClick.RemoveAllListeners();
            runScenarioButton.onClick.AddListener(() => {
                Debug.Log("Run Scenario button clicked!");
                if (uiController != null) CallMethod(uiController, "OnRunScenarioClicked");
            });
            Debug.Log("Run Scenario button events connected successfully");
        }
        
        // Connect toggle events
        Toggle windOverlayToggle = FindComponentInChildren<Toggle>(uiCanvasTransform.Find("SiteAnalysisScreen"), "WindOverlayToggle");
        if (windOverlayToggle != null)
        {
            windOverlayToggle.onValueChanged.RemoveAllListeners();
            windOverlayToggle.onValueChanged.AddListener((value) => {
                Debug.Log($"Wind Overlay toggled: {value}");
                if (uiController != null) CallMethod(uiController, "OnWindOverlayToggled", value);
            });
            Debug.Log("Wind Overlay toggle events connected successfully");
        }
        
        Toggle sunExposureToggle = FindComponentInChildren<Toggle>(uiCanvasTransform.Find("SiteAnalysisScreen"), "SunExposureToggle");
        if (sunExposureToggle != null)
        {
            sunExposureToggle.onValueChanged.RemoveAllListeners();
            sunExposureToggle.onValueChanged.AddListener((value) => {
                Debug.Log($"Sun Exposure toggled: {value}");
                if (uiController != null) CallMethod(uiController, "OnSunExposureToggled", value);
            });
            Debug.Log("Sun Exposure toggle events connected successfully");
        }
        
        Toggle pedestrianOverlayToggle = FindComponentInChildren<Toggle>(uiCanvasTransform.Find("SiteAnalysisScreen"), "PedestrianToggle");
        if (pedestrianOverlayToggle != null)
        {
            pedestrianOverlayToggle.onValueChanged.RemoveAllListeners();
            pedestrianOverlayToggle.onValueChanged.AddListener((value) => {
                Debug.Log($"Pedestrian Overlay toggled: {value}");
                if (uiController != null) CallMethod(uiController, "OnPedestrianOverlayToggled", value);
            });
            Debug.Log("Pedestrian Overlay toggle events connected successfully");
        }
        
        // Connect slider events
        Slider growthSpeedSlider = FindComponentInChildren<Slider>(uiCanvasTransform.Find("MorphologyScreen"), "GrowthSpeedSlider");
        if (growthSpeedSlider != null)
        {
            growthSpeedSlider.onValueChanged.RemoveAllListeners();
            growthSpeedSlider.onValueChanged.AddListener((value) => {
                Debug.Log($"Growth Speed changed to: {value}");
                if (uiController != null) CallMethod(uiController, "OnGrowthSpeedChanged", value);
            });
            Debug.Log("Growth Speed slider events connected successfully");
        }
        
        // Connect dropdown events
        TMP_Dropdown scenarioDropdown = FindComponentInChildren<TMP_Dropdown>(uiCanvasTransform.Find("ScenarioScreen"), "ScenarioDropdown");
        if (scenarioDropdown != null)
        {
            // Populate dropdown options
            scenarioDropdown.ClearOptions();
            scenarioDropdown.AddOptions(new List<string> { 
                "Wind Scenario", 
                "Light Scenario",
                "High Wind",
                "Low Sunlight",
                "Weekend Crowds"
            });
            
            scenarioDropdown.onValueChanged.RemoveAllListeners();
            scenarioDropdown.onValueChanged.AddListener((index) => {
                Debug.Log($"Scenario selected: {index}");
                if (uiController != null) CallMethod(uiController, "OnScenarioSelected", index);
            });
            Debug.Log("Scenario dropdown events connected successfully");
        }
        
        Debug.Log("All UI events connected successfully.");
    }
    
    #region Reflection Utility Methods
    
    private void AssignFieldValue(object target, string fieldName, object value)
    {
        if (target == null || string.IsNullOrEmpty(fieldName) || value == null)
            return;
            
        System.Reflection.FieldInfo field = target.GetType().GetField(
            fieldName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance
        );
        
        if (field != null)
        {
            try
            {
                field.SetValue(target, value);
                Debug.Log($"Assigned {fieldName} on {target.GetType().Name}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error assigning {fieldName} on {target.GetType().Name}: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Field {fieldName} not found on {target.GetType().Name}");
        }
    }
    
    private void CallMethod(object target, string methodName, params object[] parameters)
    {
        if (target == null || string.IsNullOrEmpty(methodName))
            return;
            
        System.Reflection.MethodInfo method = target.GetType().GetMethod(
            methodName, 
            System.Reflection.BindingFlags.Public | 
            System.Reflection.BindingFlags.NonPublic | 
            System.Reflection.BindingFlags.Instance
        );
        
        if (method != null)
        {
            try
            {
                method.Invoke(target, parameters);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error calling {methodName} on {target.GetType().Name}: {ex.Message}");
            }
        }
        else
        {
            Debug.LogWarning($"Method {methodName} not found on {target.GetType().Name}");
        }
    }
    
    private T FindComponentInChildren<T>(Transform parent, string childName) where T : Component
    {
        if (parent == null)
            return null;
            
        Transform child = parent.Find(childName);
        if (child != null)
        {
            return child.GetComponent<T>();
        }
        
        return null;
    }
    
    #endregion
    
    // Helper method to create a rounded rectangle sprite for UI elements
    private Sprite CreateRoundedRectSprite(int width, int height, int cornerRadius)
    {
        // Create a new texture with transparent background
        Texture2D texture = new Texture2D(width, height);
        Color[] colors = new Color[width * height];
        
        // Fill with transparent pixels by default
        for (int i = 0; i < colors.Length; i++)
        {
            colors[i] = Color.clear;
        }
        
        // Fill with white pixels where we want the rounded rectangle to appear
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Check if this pixel is within the rounded rectangle
                if (IsInsideRoundedRect(x, y, width, height, cornerRadius))
                {
                    colors[y * width + x] = Color.white;
                }
            }
        }
        
        texture.SetPixels(colors);
        texture.Apply();
        
        // Create a sprite from the texture
        return Sprite.Create(texture, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    // Helper method to determine if a pixel is inside a rounded rectangle
    private bool IsInsideRoundedRect(int x, int y, int width, int height, int radius)
    {
        // Check if we're in the central region (not in corners)
        if (x >= radius && x < width - radius || y >= radius && y < height - radius)
            return true;
            
        // Check if we're in one of the corners
        int cornerX = x < radius ? x : width - x - 1;
        int cornerY = y < radius ? y : height - y - 1;
        
        if (cornerX < radius && cornerY < radius)
        {
            // Calculate distance from corner center
            float distance = Mathf.Sqrt((radius - cornerX) * (radius - cornerX) + 
                                        (radius - cornerY) * (radius - cornerY));
            return distance <= radius;
        }
        
        return false;
    }
    
    /// <summary>
    /// Creates a flat terrain mesh with the specified dimensions
    /// </summary>
    /// <param name="width">Width of the terrain in world units</param>
    /// <param name="length">Length of the terrain in world units</param>
    /// <returns>A new mesh for the terrain</returns>
    private Mesh CreateTerrainMesh(float width, float length)
    {
        Mesh mesh = new Mesh();
        
        // Create vertices (simple flat plane)
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-width/2, 0, -length/2);
        vertices[1] = new Vector3(width/2, 0, -length/2);
        vertices[2] = new Vector3(-width/2, 0, length/2);
        vertices[3] = new Vector3(width/2, 0, length/2);
        
        // Create triangles
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;
        
        // Create UVs
        Vector2[] uv = new Vector2[4];
        uv[0] = new Vector2(0, 0);
        uv[1] = new Vector2(1, 0);
        uv[2] = new Vector2(0, 1);
        uv[3] = new Vector2(1, 1);
        
        // Assign values to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uv;
        
        // Recalculate normals and bounds
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        
        return mesh;
    }
}