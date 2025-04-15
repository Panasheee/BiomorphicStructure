using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages visualization styles and rendering for the morphology simulator.
/// Controls material properties, visual effects, and camera settings for visualization.
/// </summary>
public class VisualizationManager : MonoBehaviour
{
    #region Properties
    [Header("Visualization Styles")]
    [SerializeField] private VisualizationStyle[] visualizationStyles;
    [SerializeField] private int defaultStyleIndex = 0;
    
    [Header("Visualization Settings")]
    [SerializeField] private float stressColorIntensity = 1.0f;
    [SerializeField] private float adaptationColorIntensity = 1.0f;
    [SerializeField] private float nodeThicknessMultiplier = 1.0f;
    [SerializeField] private float connectionThicknessMultiplier = 1.0f;
    [SerializeField] private float glowIntensity = 1.0f;
    [SerializeField] private float transparencyLevel = 1.0f;
    
    [Header("Camera Settings")]
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float autoRotateSpeed = 10f;
    [SerializeField] private bool autoRotate = false;
    [SerializeField] private float zoomSpeed = 1.0f;
    
    [Header("Force Visualization")]
    [SerializeField] private GameObject forceVisualizerPrefab;
    [SerializeField] private float forceVisualizationScale = 1.0f;
    [SerializeField] private bool showForces = true;
    
    [Header("Rendering Tools")]
    [SerializeField] private Light mainLight;
    [SerializeField] private Transform cameraRig;
    [SerializeField] private Material unlitNodeMaterial;
    [SerializeField] private Material unlitConnectionMaterial;
    
    // Current state
    private VisualizationStyle currentStyle;
    private int currentStyleIndex;
    private List<GameObject> forceVisualizers = new List<GameObject>();    private Dictionary<MorphNode, Material> originalNodeMaterials = new Dictionary<MorphNode, Material>();
    private Dictionary<MorphConnection, Material> originalConnectionMaterials = new Dictionary<MorphConnection, Material>();
    private bool stressColorsEnabled = true;
    
    // Singleton instance
    private static VisualizationManager instance;
    
    #region Public Methods
    /// <summary>
    /// Exports the current camera view as an image
    /// </summary>
    public void ExportCurrentView(string filename = "")
    {
        if (string.IsNullOrEmpty(filename))
        {
            filename = $"Morphology_Capture_{System.DateTime.Now:yyyyMMdd_HHmmss}";
        }
        
        // Ensure the Screenshots directory exists
        string directory = System.IO.Path.Combine(Application.persistentDataPath, "Screenshots");
        if (!System.IO.Directory.Exists(directory))
        {
            System.IO.Directory.CreateDirectory(directory);
        }
        
        string filePath = System.IO.Path.Combine(directory, $"{filename}.png");
        
        // Capture the screenshot
        if (mainCamera != null)
        {
            RenderTexture rt = new RenderTexture(Screen.width, Screen.height, 24);
            mainCamera.targetTexture = rt;
            Texture2D screenShot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            mainCamera.Render();
            RenderTexture.active = rt;
            screenShot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            mainCamera.targetTexture = null;
            RenderTexture.active = null;
            Destroy(rt);
            
            byte[] bytes = screenShot.EncodeToPNG();
            System.IO.File.WriteAllBytes(filePath, bytes);
            
            Debug.Log($"Screenshot saved to: {filePath}");
        }
        else
        {
            Debug.LogError("Cannot export view: Camera reference is missing");
        }
    }
    
    // Property for accessing the singleton
    public static VisualizationManager Instance
    {        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<VisualizationManager>();
                
                if (instance == null)
                {
                    GameObject obj = new GameObject("VisualizationManager");
                    instance = obj.AddComponent<VisualizationManager>();
                }
            }
            
            return instance;
        }
    }
    #endregion

    #region Unity Lifecycle
    private void Awake()
    {
        // Ensure singleton behavior
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        
        // Initialize camera reference if not set
        if (mainCamera == null)
        {
            mainCamera = Camera.main;
        }
        
        // Initialize camera rig if not set
        if (cameraRig == null && mainCamera != null)
        {
            GameObject rigObject = new GameObject("CameraRig");
            cameraRig = rigObject.transform;
            mainCamera.transform.parent = cameraRig;
            mainCamera.transform.localPosition = new Vector3(0, 0, -50);
            mainCamera.transform.localRotation = Quaternion.identity;
        }
        
        // Set default style
        if (visualizationStyles != null && visualizationStyles.Length > 0)
        {
            currentStyleIndex = Mathf.Clamp(defaultStyleIndex, 0, visualizationStyles.Length - 1);
            currentStyle = visualizationStyles[currentStyleIndex];
        }
    }
    
    private void Update()
    {
        // Handle auto-rotation if enabled
        if (autoRotate && cameraRig != null)
        {
            cameraRig.Rotate(Vector3.up, autoRotateSpeed * Time.deltaTime);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Applies a visualization style by index
    /// </summary>
    /// <param name="styleIndex">Index of the style to apply</param>
    public void ApplyVisualizationStyle(int styleIndex)
    {
        if (visualizationStyles == null || visualizationStyles.Length == 0)
        {
            Debug.LogWarning("No visualization styles defined");
            return;
        }
        
        // Validate index
        styleIndex = Mathf.Clamp(styleIndex, 0, visualizationStyles.Length - 1);
        
        // Apply the style
        currentStyleIndex = styleIndex;
        currentStyle = visualizationStyles[styleIndex];
        
        ApplyCurrentStyle();
        
        Debug.Log($"Applied visualization style: {currentStyle.styleName}");
    }
    
    /// <summary>
    /// Cycles to the next visualization style
    /// </summary>
    public void NextVisualizationStyle()
    {
        if (visualizationStyles == null || visualizationStyles.Length == 0)
        {
            Debug.LogWarning("No visualization styles defined");
            return;
        }
        
        currentStyleIndex = (currentStyleIndex + 1) % visualizationStyles.Length;
        currentStyle = visualizationStyles[currentStyleIndex];
        
        ApplyCurrentStyle();
        
        Debug.Log($"Applied visualization style: {currentStyle.styleName}");
    }
    
    /// <summary>
    /// Updates the visualization for a set of nodes and connections
    /// </summary>
    /// <param name="nodes">Nodes to visualize</param>
    /// <param name="connections">Connections to visualize</param>
    /// <param name="nodeStresses">Dictionary mapping nodes to stress values (0-1)</param>
    public void UpdateVisualization(List<MorphNode> nodes, List<MorphConnection> connections, Dictionary<MorphNode, float> nodeStresses = null)
    {
        // Store original materials if needed
        if (originalNodeMaterials.Count == 0 && nodes.Count > 0)
        {
            foreach (var node in nodes)
            {
                if (node != null && !originalNodeMaterials.ContainsKey(node))
                {
                    originalNodeMaterials[node] = node.GetMaterial();
                }
            }
        }
        
        if (originalConnectionMaterials.Count == 0 && connections.Count > 0)
        {
            foreach (var connection in connections)
            {
                if (connection != null && !originalConnectionMaterials.ContainsKey(connection))
                {
                    originalConnectionMaterials[connection] = connection.GetMaterial();
                }
            }
        }
        
        // Apply visualization based on current style
        foreach (var node in nodes)
        {
            if (node == null) continue;
            
            // Determine stress level
            float stress = 0f;
            if (nodeStresses != null && nodeStresses.TryGetValue(node, out float nodeStress))
            {
                stress = nodeStress;
            }
            else
            {
                stress = node.Stress;
            }
            
            // Apply visualization
            ApplyNodeVisualization(node, stress);
        }
        
        // Update connections
        foreach (var connection in connections)
        {
            if (connection == null) continue;
            
            // Determine stress based on connected nodes
            float stressA = 0f;
            float stressB = 0f;
            
            if (connection.NodeA != null && nodeStresses != null && nodeStresses.TryGetValue(connection.NodeA, out float nodeAStress))
            {
                stressA = nodeAStress;
            }
            else if (connection.NodeA != null)
            {
                stressA = connection.NodeA.Stress;
            }
            
            if (connection.NodeB != null && nodeStresses != null && nodeStresses.TryGetValue(connection.NodeB, out float nodeBStress))
            {
                stressB = nodeBStress;
            }
            else if (connection.NodeB != null)
            {
                stressB = connection.NodeB.Stress;
            }
            
            float connectionStress = (stressA + stressB) * 0.5f;
            
            // Apply visualization
            ApplyConnectionVisualization(connection, connectionStress);
        }
    }
    
    /// <summary>
    /// Updates the visualization of environmental forces
    /// </summary>
    /// <param name="nodeForces">Dictionary mapping nodes to force vectors</param>
    public void UpdateForceVisualization(Dictionary<MorphNode, Vector3> nodeForces)
    {
        // Clear existing visualizers if not showing forces
        if (!showForces)
        {
            ClearForceVisualizers();
            return;
        }
        
        // Ensure we have enough visualizers
        while (forceVisualizers.Count < nodeForces.Count)
        {
            if (forceVisualizerPrefab != null)
            {
                GameObject visualizer = Instantiate(forceVisualizerPrefab, transform);
                forceVisualizers.Add(visualizer);
            }
            else
            {
                GameObject visualizer = CreateDefaultForceVisualizer();
                forceVisualizers.Add(visualizer);
            }
        }
        
        // Update visualizers
        int index = 0;
        foreach (var pair in nodeForces)
        {
            if (index >= forceVisualizers.Count) break;
            
            MorphNode node = pair.Key;
            Vector3 force = pair.Value;
            
            if (node != null)
            {
                GameObject visualizer = forceVisualizers[index];
                visualizer.transform.position = node.transform.position;
                
                if (force.magnitude > 0.01f)
                {
                    visualizer.transform.rotation = Quaternion.LookRotation(force);
                    
                    // Scale based on force magnitude
                    float scale = Mathf.Clamp(force.magnitude * forceVisualizationScale, 0.1f, 10f);
                    visualizer.transform.localScale = new Vector3(0.2f, 0.2f, scale);
                    
                    // Color based on force magnitude
                    Renderer renderer = visualizer.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        float intensity = Mathf.Clamp01(force.magnitude / 5f);
                        Color forceColor = Color.Lerp(Color.blue, Color.red, intensity);
                        renderer.material.color = forceColor;
                    }
                    
                    visualizer.SetActive(true);
                }
                else
                {
                    visualizer.SetActive(false);
                }
                
                index++;
            }
        }
        
        // Disable unused visualizers
        for (int i = index; i < forceVisualizers.Count; i++)
        {
            forceVisualizers[i].SetActive(false);
        }
    }
    
    /// <summary>
    /// Sets whether to show force visualizers
    /// </summary>
    /// <param name="show">Whether to show forces</param>
    public void SetShowForces(bool show)
    {
        showForces = show;
        
        // Hide all force visualizers if not showing
        if (!showForces)
        {
            foreach (var visualizer in forceVisualizers)
            {
                if (visualizer != null)
                {
                    visualizer.SetActive(false);
                }
            }
        }
    }
    
    /// <summary>
    /// Sets whether to show stress colors
    /// </summary>
    /// <param name="show">Whether to show stress colors</param>
    public void SetShowStressColors(bool show)
    {
        stressColorsEnabled = show;
    }
    
    /// <summary>
    /// Sets the transparency level for visualization
    /// </summary>
    /// <param name="alpha">Alpha transparency (0-1)</param>
    public void SetTransparency(float alpha)
    {
        transparencyLevel = Mathf.Clamp01(alpha);
        
        // Apply to current style
        ApplyCurrentStyle();
    }
    
    /// <summary>
    /// Sets auto-rotation for the camera
    /// </summary>
    /// <param name="rotate">Whether to auto-rotate</param>
    public void SetAutoRotate(bool rotate)
    {
        autoRotate = rotate;
    }
    
    /// <summary>
    /// Sets the auto-rotation speed
    /// </summary>
    /// <param name="speed">Rotation speed</param>
    public void SetAutoRotateSpeed(float speed)
    {
        autoRotateSpeed = speed;
    }
    
    /// <summary>
    /// Repositions the camera to look at the entire morphology
    /// </summary>
    /// <param name="nodes">Nodes to frame</param>
    /// <param name="padding">Extra padding around the view</param>
    public void FrameMorphology(List<MorphNode> nodes, float padding = 1.2f)
    {
        if (nodes == null || nodes.Count == 0 || mainCamera == null) return;
        
        // Calculate bounds
        Bounds bounds = new Bounds();
        bool boundsInitialized = false;
        
        foreach (var node in nodes)
        {
            if (node != null)
            {
                if (!boundsInitialized)
                {
                    bounds = new Bounds(node.transform.position, Vector3.zero);
                    boundsInitialized = true;
                }
                else
                {
                    bounds.Encapsulate(node.transform.position);
                }
            }
        }
        
        // Apply padding
        bounds.Expand(bounds.size * (padding - 1f));
        
        // Calculate camera position
        if (cameraRig != null)
        {
            cameraRig.position = bounds.center;
            
            // Calculate distance needed to view the entire bounds
            float objectSize = bounds.size.magnitude;
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCamera.fieldOfView);
            float distance = objectSize / cameraView;
            
            // Set camera distance
            mainCamera.transform.localPosition = new Vector3(0, 0, -distance * padding);
        }
        else
        {
            // Calculate optimal camera position and distance without a rig
            float objectSize = bounds.size.magnitude;
            float cameraView = 2.0f * Mathf.Tan(0.5f * Mathf.Deg2Rad * mainCamera.fieldOfView);
            float distance = objectSize / cameraView;
            
            mainCamera.transform.position = bounds.center - mainCamera.transform.forward * distance * padding;
            mainCamera.transform.LookAt(bounds.center);
        }
    }
    
    /// <summary>
    /// Takes a screenshot of the current visualization
    /// </summary>
    /// <param name="width">Screenshot width</param>
    /// <param name="height">Screenshot height</param>
    /// <returns>Path to the saved screenshot</returns>
    public string TakeScreenshot(int width = 1920, int height = 1080)
    {
        // Create a render texture
        RenderTexture renderTexture = new RenderTexture(width, height, 24);
        
        // Store current camera target
        RenderTexture currentRT = mainCamera.targetTexture;
        
        // Set camera to render to our texture
        mainCamera.targetTexture = renderTexture;
        
        // Render the camera's view
        mainCamera.Render();
        
        // Create a texture2D and read the render texture
        Texture2D screenshot = new Texture2D(width, height, TextureFormat.RGB24, false);
        
        // Activate the render texture
        RenderTexture.active = renderTexture;
        
        // Read pixels
        screenshot.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        screenshot.Apply();
        
        // Restore previous state
        mainCamera.targetTexture = currentRT;
        RenderTexture.active = null;
        Destroy(renderTexture);
        
        // Save to file
        string fileName = $"Morphology_{System.DateTime.Now:yyyyMMdd_HHmmss}.png";
        string filePath = System.IO.Path.Combine(Application.persistentDataPath, "Screenshots", fileName);
        
        // Ensure directory exists
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(filePath));
        
        // Save texture as PNG
        byte[] bytes = screenshot.EncodeToPNG();
        System.IO.File.WriteAllBytes(filePath, bytes);
        
        Debug.Log($"Screenshot saved: {filePath}");
        
        return filePath;
    }
    
    /// <summary>
    /// Enables a wireframe visualization mode
    /// </summary>
    /// <param name="enable">Whether to enable wireframe mode</param>
    public void SetWireframeMode(bool enable)
    {
        if (enable)
        {
            // Save materials before switching to wireframe
            SaveOriginalMaterials();
              // Find all MorphNodes and apply wireframe material
            MorphNode[] nodes = FindObjectsByType<MorphNode>(FindObjectsSortMode.None);
            foreach (var node in nodes)
            {
                ApplyWireframeMaterial(node.gameObject);
            }
            
            // Find all MorphConnections and apply wireframe material
            MorphConnection[] connections = FindObjectsByType<MorphConnection>(FindObjectsSortMode.None);
            foreach (var connection in connections)
            {
                ApplyWireframeMaterial(connection.gameObject);
            }
        }
        else
        {
            // Restore original materials
            RestoreOriginalMaterials();
        }
    }
    
    /// <summary>
    /// Makes the morphology emit light/glow
    /// </summary>
    /// <param name="intensity">Glow intensity</param>
    public void SetGlowEffect(float intensity)
    {        glowIntensity = Mathf.Max(0, intensity);
        
        // Find all MorphNodes and apply glow
        MorphNode[] nodes = FindObjectsByType<MorphNode>(FindObjectsSortMode.None);
        foreach (var node in nodes)
        {
            ApplyGlowEffect(node.gameObject, intensity);
        }
    }
    
    /// <summary>
    /// Gets the current visualization style name
    /// </summary>
    /// <returns>Name of the current style</returns>
    public string GetCurrentStyleName()
    {
        return currentStyle != null ? currentStyle.styleName : "Default";
    }
    
    /// <summary>
    /// Gets all available visualization style names
    /// </summary>
    /// <returns>Array of style names</returns>
    public string[] GetAllStyleNames()
    {
        if (visualizationStyles == null || visualizationStyles.Length == 0)
        {
            return new string[] { "Default" };
        }
        
        string[] names = new string[visualizationStyles.Length];
        for (int i = 0; i < visualizationStyles.Length; i++)
        {
            names[i] = visualizationStyles[i].styleName;
        }
        
        return names;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Applies the current visualization style settings to relevant components
    /// </summary>
    public void ApplyCurrentStyle() // Changed from private/protected to public
    {
        if (currentStyle == null)
        {
            Debug.LogWarning("Cannot apply style: No current style set");
            return;
        }
        
        // Apply camera settings
        if (mainCamera != null)
        {
            mainCamera.backgroundColor = currentStyle.backgroundColor;
            // Apply post-processing effects if defined in the style
        }
        
        // Apply lighting settings
        if (mainLight != null)
        {
            mainLight.color = currentStyle.lightColor;
            mainLight.intensity = currentStyle.lightIntensity;
        }
        
        // Apply material settings (This might need more complex logic depending on how materials are managed)
        // For now, let's assume we update properties on existing materials or swap them
        // This part needs integration with how nodes/connections get their materials
        
        // Update global settings based on style
        stressColorIntensity = currentStyle.stressColorIntensity;
        adaptationColorIntensity = currentStyle.adaptationColorIntensity;
        nodeThicknessMultiplier = currentStyle.nodeThicknessMultiplier;
        connectionThicknessMultiplier = currentStyle.connectionThicknessMultiplier;
        glowIntensity = currentStyle.glowIntensity;
        transparencyLevel = currentStyle.transparencyLevel;
        
        // Trigger a visual update if necessary (e.g., if morphology already exists)
        // MorphologyManager.Instance?.UpdateMorphologyVisualization(); 
    }
    
    /// <summary>
    /// Applies visualization to a node based on its stress
    /// </summary>
    /// <param name="node">Node to visualize</param>
    /// <param name="stress">Stress level (0-1)</param>
    private void ApplyNodeVisualization(MorphNode node, float stress)
    {
        if (node == null || currentStyle == null) return;
        
        Material nodeMaterial = null;
        
        // Get original material
        if (originalNodeMaterials.TryGetValue(node, out Material original))
        {
            nodeMaterial = new Material(original);
        }
        else
        {
            nodeMaterial = new Material(currentStyle.defaultNodeMaterial);
        }
        
        // Apply style settings
        if (stressColorsEnabled)
        {
            // Interpolate color based on stress
            Color nodeColor = Color.Lerp(
                currentStyle.lowStressColor,
                currentStyle.highStressColor,
                stress * stressColorIntensity
            );
            
            nodeColor.a = transparencyLevel;
            nodeMaterial.color = nodeColor;
        }
        else
        {
            // Use base color with transparency
            Color baseColor = currentStyle.defaultNodeMaterial.color;
            baseColor.a = transparencyLevel;
            nodeMaterial.color = baseColor;
        }
        
        // Set rendering mode based on transparency
        if (transparencyLevel < 0.99f)
        {
            nodeMaterial.SetFloat("_Mode", 3); // Transparent
            nodeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            nodeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            nodeMaterial.SetInt("_ZWrite", 0);
            nodeMaterial.DisableKeyword("_ALPHATEST_ON");
            nodeMaterial.EnableKeyword("_ALPHABLEND_ON");
            nodeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            nodeMaterial.renderQueue = 3000;
        }
        
        // Apply emission if using glow
        if (glowIntensity > 0)
        {
            nodeMaterial.EnableKeyword("_EMISSION");
            nodeMaterial.SetColor("_EmissionColor", nodeMaterial.color * glowIntensity);
        }
        
        // Apply size multiplier
        float scaleFactor = nodeThicknessMultiplier;
        if (currentStyle.scaleByStress)
        {
            scaleFactor *= 1.0f + stress * 0.5f;
        }
        
        node.transform.localScale = Vector3.one * scaleFactor;
        
        // Apply the material
        node.Initialize(nodeMaterial);
    }
    
    /// <summary>
    /// Applies visualization to a connection based on stress
    /// </summary>
    /// <param name="connection">Connection to visualize</param>
    /// <param name="stress">Stress level (0-1)</param>
    private void ApplyConnectionVisualization(MorphConnection connection, float stress)
    {
        if (connection == null || currentStyle == null) return;
        
        Material connectionMaterial = null;
        
        // Get original material
        if (originalConnectionMaterials.TryGetValue(connection, out Material original))
        {
            connectionMaterial = new Material(original);
        }
        else
        {
            connectionMaterial = new Material(currentStyle.defaultConnectionMaterial);
        }
        
        // Apply style settings
        if (stressColorsEnabled)
        {
            // Interpolate color based on stress
            Color connectionColor = Color.Lerp(
                currentStyle.lowStressColor,
                currentStyle.highStressColor,
                stress * stressColorIntensity
            );
            
            connectionColor.a = transparencyLevel;
            connectionMaterial.color = connectionColor;
        }
        else
        {
            // Use base color with transparency
            Color baseColor = currentStyle.defaultConnectionMaterial.color;
            baseColor.a = transparencyLevel;
            connectionMaterial.color = baseColor;
        }
        
        // Set rendering mode based on transparency
        if (transparencyLevel < 0.99f)
        {
            connectionMaterial.SetFloat("_Mode", 3); // Transparent
            connectionMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            connectionMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            connectionMaterial.SetInt("_ZWrite", 0);
            connectionMaterial.DisableKeyword("_ALPHATEST_ON");
            connectionMaterial.EnableKeyword("_ALPHABLEND_ON");
            connectionMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            connectionMaterial.renderQueue = 3000;
        }
        
        // Apply emission if using glow
        if (glowIntensity > 0)
        {
            connectionMaterial.EnableKeyword("_EMISSION");
            connectionMaterial.SetColor("_EmissionColor", connectionMaterial.color * glowIntensity);
        }
        
        // Apply thickness multiplier
        float thickness = connectionThicknessMultiplier;
        if (currentStyle.scaleByStress)
        {
            thickness *= 1.0f - stress * 0.5f; // Thinner when stressed
        }
        
        // Apply the material and update
        connection.Initialize(connection.NodeA, connection.NodeB, connectionMaterial);
        
        // We can't directly access the scale, but during the next UpdateVisual call, 
        // our overridden thickness will be applied
        connection.UpdateVisual();
    }
    
    /// <summary>
    /// Creates a default force visualizer arrow
    /// </summary>
    /// <returns>The created visualizer GameObject</returns>
    private GameObject CreateDefaultForceVisualizer()
    {
        GameObject visualizer = new GameObject("ForceVisualizer");
        visualizer.transform.parent = transform;
        
        // Create a simple arrow mesh        
        GameObject cylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        cylinder.transform.parent = visualizer.transform;
        cylinder.transform.localScale = new Vector3(0.1f, 0.5f, 0.1f);
        cylinder.transform.localPosition = new Vector3(0, 0, 0.5f);
        
        // Use a scaled Cylinder for the arrowhead
        GameObject arrowHead = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        arrowHead.transform.parent = visualizer.transform;
        arrowHead.transform.localScale = new Vector3(0.2f, 0.3f, 0.2f);
        arrowHead.transform.localPosition = new Vector3(0, 0, 1.2f);
        
        // Create material
        Material arrowMaterial = new Material(Shader.Find("Standard"));
        arrowMaterial.color = Color.yellow;
        
        // Apply material
        cylinder.GetComponent<Renderer>().material = arrowMaterial;
        arrowHead.GetComponent<Renderer>().material = arrowMaterial;
        
        return visualizer;
    }
    
    /// <summary>
    /// Optimizes visualization for large morphologies by using LOD and culling
    /// </summary>
    /// <param name="nodeCount">Number of nodes in the morphology</param>
    public void OptimizeForLargeMorphology(int nodeCount)
    {
        // Adjust rendering settings based on morphology size
        if (nodeCount > 1000)
        {
            // For very large morphologies, reduce detail
            nodeThicknessMultiplier = Mathf.Min(nodeThicknessMultiplier, 0.7f);
            connectionThicknessMultiplier = Mathf.Min(connectionThicknessMultiplier, 0.5f);
            
            // Disable advanced effects
            glowIntensity = 0f;
            
            // Update camera settings
            if (mainCamera != null)
            {
                mainCamera.farClipPlane = 1000f;
                mainCamera.allowMSAA = false;
                mainCamera.allowHDR = false;
            }
        }
        else if (nodeCount > 500)
        {
            // For medium morphologies, reduce some detail
            nodeThicknessMultiplier = Mathf.Min(nodeThicknessMultiplier, 0.85f);
            glowIntensity = Mathf.Min(glowIntensity, 0.5f);
        }
        
        // Apply the changes
        ApplyCurrentStyle();
        
        Debug.Log($"Optimized visualization for morphology with {nodeCount} nodes");
    }
    
    /// <summary>
    /// Sets the rendering mode for all morphology elements
    /// </summary>
    /// <param name="mode">Rendering mode (0=solid, 1=transparent, 2=wireframe)</param>
    public void SetRenderingMode(int mode)
    {
        switch (mode)
        {
            case 0: // Solid
                SetTransparency(1.0f);
                SetWireframeMode(false);
                break;
                
            case 1: // Transparent
                SetTransparency(0.6f);
                SetWireframeMode(false);
                break;
                
            case 2: // Wireframe
                SetTransparency(1.0f);
                SetWireframeMode(true);
                break;
                
            default:
                Debug.LogWarning($"Unknown rendering mode: {mode}");
                break;
        }
    }
    
    /// <summary>
    /// Clears all force visualizers
    /// </summary>
    private void ClearForceVisualizers()
    {
        foreach (var visualizer in forceVisualizers)
        {
            if (visualizer != null)
            {
                Destroy(visualizer);
            }
        }
        
        forceVisualizers.Clear();
    }
    
    /// <summary>
    /// Saves the original materials of all nodes and connections
    /// </summary>
    private void SaveOriginalMaterials()
    {
        // Clear existing saved materials
        originalNodeMaterials.Clear();
        originalConnectionMaterials.Clear();
          // Save node materials
        MorphNode[] nodes = FindObjectsByType<MorphNode>(FindObjectsSortMode.None);
        foreach (var node in nodes)
        {
            if (node != null)
            {
                originalNodeMaterials[node] = node.GetMaterial();
            }
        }
          // Save connection materials
        MorphConnection[] connections = FindObjectsByType<MorphConnection>(FindObjectsSortMode.None);
        foreach (var connection in connections)
        {
            if (connection != null)
            {
                originalConnectionMaterials[connection] = connection.GetMaterial();
            }
        }
    }
    
    /// <summary>
    /// Restores the original materials to all nodes and connections
    /// </summary>
    private void RestoreOriginalMaterials()
    {
        // Restore node materials
        foreach (var pair in originalNodeMaterials)
        {
            if (pair.Key != null)
            {
                pair.Key.Initialize(pair.Value);
            }
        }
        
        // Restore connection materials
        foreach (var pair in originalConnectionMaterials)
        {
            if (pair.Key != null && pair.Key.NodeA != null && pair.Key.NodeB != null)
            {
                pair.Key.Initialize(pair.Key.NodeA, pair.Key.NodeB, pair.Value);
            }
        }
    }
    
    /// <summary>
    /// Applies a wireframe material to a GameObject
    /// </summary>
    /// <param name="obj">GameObject to modify</param>
    private void ApplyWireframeMaterial(GameObject obj)
    {
        if (obj == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null)
        {
            // Create wireframe material
            Material wireframeMaterial = new Material(Shader.Find("Standard"));
            wireframeMaterial.color = Color.white;
            wireframeMaterial.SetFloat("_Mode", 1); // Cutout mode
            wireframeMaterial.SetFloat("_Cutoff", 0.5f);
            wireframeMaterial.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.One);
            wireframeMaterial.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.Zero);
            wireframeMaterial.SetInt("_ZWrite", 1);
            wireframeMaterial.EnableKeyword("_ALPHATEST_ON");
            wireframeMaterial.DisableKeyword("_ALPHABLEND_ON");
            wireframeMaterial.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            wireframeMaterial.SetFloat("_Glossiness", 0);
            wireframeMaterial.SetFloat("_Metallic", 0);
            wireframeMaterial.renderQueue = 2450;
            
            // Enable wireframe
            wireframeMaterial.SetFloat("_WireThickness", 800);
            
            renderer.material = wireframeMaterial;
        }
    }
    
    /// <summary>
    /// Applies a glow effect to a GameObject
    /// </summary>
    /// <param name="obj">GameObject to modify</param>
    /// <param name="intensity">Glow intensity</param>
    private void ApplyGlowEffect(GameObject obj, float intensity)
    {
        if (obj == null) return;
        
        Renderer renderer = obj.GetComponent<Renderer>();
        if (renderer != null && renderer.material != null)
        {
            Material material = renderer.material;
            
            // Enable emission
            material.EnableKeyword("_EMISSION");
            
            // Set emission color based on current color and intensity
            material.SetColor("_EmissionColor", material.color * intensity);
        }
    }
    #endregion
}

/// <summary>
/// Defines a visualization style for the morphology simulator
/// </summary>
[System.Serializable]
public class VisualizationStyle
{
    [Header("Basic Information")]
    public string styleName = "Default";
    public string description = "Default visualization style";
    
    [Header("Materials")]
    public Material defaultNodeMaterial;
    public Material defaultConnectionMaterial;
    public Material stressNodeMaterial;
    public Material stressConnectionMaterial;
    
    [Header("Colors")]
    public Color backgroundColor = Color.black;
    public Color lowStressColor = Color.green;
    public Color highStressColor = Color.red;
    public Color ambientLightColor = Color.white;
    
    [Header("Lighting")]
    public float lightIntensity = 1.0f;
    public bool useBloom = false;
    public float bloomIntensity = 1.0f;
    
    [Header("Visual Effects")]
    public bool useTransparency = false;
    public float transparencyLevel = 1.0f;
    public bool useGlow = false;
    public float glowIntensity = 1.0f;
    public bool scaleByStress = true;
    public bool useOutline = false;
    public float outlineWidth = 1.0f;
    
    [Header("Advanced")]
    public bool usePostProcessing = false;
    public bool useShadows = true;
    public bool useReflections = false;
} 
#endregion