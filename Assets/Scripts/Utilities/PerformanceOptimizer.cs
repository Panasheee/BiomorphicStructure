using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BiomorphicSim.Utilities;

/// <summary>
/// Manages system performance and optimizes resource usage.
/// Automatically adjusts detail levels, LODs, and processing to maintain target frame rate.
/// </summary>
public class PerformanceOptimizer : MonoBehaviour
{
    #region Properties
    [Header("Performance Targets")]
    [SerializeField] private int targetFrameRate = 60;
    [SerializeField] private float minAcceptableFrameRate = 30f;
    [SerializeField] private bool enableDynamicOptimization = true;
    [SerializeField] private float optimizationInterval = 2.0f;
    
    [Header("Morphology Optimization")]
    [SerializeField] private bool enableNodeCulling = true;
    [SerializeField] private bool enableLOD = true;
    [SerializeField] private float nodeLODDistance = 50f;
    [SerializeField] private float connectionLODDistance = 70f;
    [SerializeField] private int maxVisibleNodes = 2000;
    [SerializeField] private int maxVisibleConnections = 4000;
    
    [Header("Graphics Quality")]
    [SerializeField] private bool enableDynamicQuality = true;
    [SerializeField] private bool enableDynamicResolution = true;
    [SerializeField] private float minResolutionScale = 0.7f;
    [SerializeField] private float maxResolutionScale = 1.0f;
    [SerializeField] private float targetGPUUsage = 0.8f;
    
    [Header("Physics Optimization")]
    [SerializeField] private bool enablePhysicsOptimization = true;
    [SerializeField] private int maxPhysicsIterations = 6;
    [SerializeField] private bool useAdaptiveTimeStep = true;
    [SerializeField] private float minPhysicsTimeStep = 0.01f;
    [SerializeField] private float maxPhysicsTimeStep = 0.02f;
    
    [Header("Monitoring")]
    [SerializeField] private bool showPerformanceStats = true;
    [SerializeField] private bool logPerformanceWarnings = true;
    [SerializeField] private float memoryWarningThreshold = 0.8f;
    
    // Performance metrics
    private float averageFrameTime = 0f;
    private float averageFrameRate = 0f;
    private float currentFrameTime = 0f;
    private float currentGPUTime = 0f;
    private float currentCPUTime = 0f;
    private float memoryUsage = 0f;
    
    // Smoothing
    private List<float> frameTimeSamples = new List<float>();
    private int maxSamples = 60;
    
    // Optimization state
    private QualityLevel currentQualityLevel = QualityLevel.High;
    private float currentResolutionScale = 1.0f;
    private int currentRenderDistance = 100;
    private float nodeSizeMultiplier = 1.0f;
    private bool isOptimizing = false;
    
    // Frame time history
    private Queue<float> frameTimeHistory = new Queue<float>();
    private int frameHistoryLength = 120; // 2 seconds at 60fps
    
    // Singleton instance
    private static PerformanceOptimizer instance;
    public static PerformanceOptimizer Instance => instance;
    
    // Properties for external access
    public QualityLevel CurrentQuality => currentQualityLevel;
    public float CurrentFrameRate => averageFrameRate;
    public float MemoryUsage => memoryUsage;
    #endregion

    #region Enums
    /// <summary>
    /// Quality levels for optimization
    /// </summary>
    public enum QualityLevel
    {
        Low,
        Medium,
        High,
        Ultra
    }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Singleton setup
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Initialize metrics
        InitializeMetrics();
        
        // Set initial quality settings
        ApplyQualitySettings(currentQualityLevel);
        
        // Set target frame rate
        Application.targetFrameRate = targetFrameRate;
    }
    
    private void Start()
    {
        // Initial optimization
        StartCoroutine(OptimizationRoutine());
    }
    
    private void Update()
    {
        // Calculate frame time
        currentFrameTime = Time.unscaledDeltaTime;
        frameTimeSamples.Add(currentFrameTime);
        
        // Keep sample list within max size
        if (frameTimeSamples.Count > maxSamples)
        {
            frameTimeSamples.RemoveAt(0);
        }
        
        // Update rolling frame time history
        frameTimeHistory.Enqueue(currentFrameTime);
        if (frameTimeHistory.Count > frameHistoryLength)
        {
            frameTimeHistory.Dequeue();
        }
        
        // Calculate averages
        averageFrameTime = frameTimeSamples.Average();
        averageFrameRate = 1.0f / averageFrameTime;
        
        // Update memory usage
        UpdateMemoryUsage();
        
        // Check for performance issues
        if (enableDynamicOptimization && !isOptimizing)
        {
            CheckPerformance();
        }
    }
    
    private void OnDestroy()
    {
    }
    
    private void OnGUI()
    {
        if (showPerformanceStats)
        {
            GUI.skin.box.fontSize = 16;
            GUI.skin.box.alignment = TextAnchor.UpperLeft;
            
            string stats = $"FPS: {averageFrameRate:0.0}\n" +
                         $"Frame Time: {averageFrameTime * 1000:0.0} ms\n" +
                         $"CPU: {currentCPUTime * 1000:0.0} ms\n" +
                         $"GPU: {currentGPUTime * 1000:0.0} ms\n" +
                         $"Memory: {memoryUsage * 100:0.0}%\n" +
                         $"Quality: {currentQualityLevel}\n" +
                         $"Resolution: {currentResolutionScale:P0}";
            
            GUI.Box(new Rect(10, 10, 220, 160), stats);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the target quality level
    /// </summary>
    /// <param name="quality">Desired quality level</param>
    public void SetQualityLevel(QualityLevel quality)
    {
        currentQualityLevel = quality;
        ApplyQualitySettings(quality);
    }
    
    /// <summary>
    /// Gets recommended morphology generation settings based on system performance
    /// </summary>
    /// <returns>Optimized parameters for morphology generation</returns>
    public MorphologyParameters GetOptimizedMorphologyParameters(MorphologyParameters baseParameters)
    {
        MorphologyParameters optimizedParams = baseParameters;
        
        // Adjust based on system performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        
        // Scale density with performance
        float densityScale = Mathf.Lerp(0.5f, 1.0f, performanceFactor);
        optimizedParams.density = Mathf.Clamp01(baseParameters.density * densityScale);
        
        // Scale complexity with performance
        float complexityScale = Mathf.Lerp(0.6f, 1.0f, performanceFactor);
        optimizedParams.complexity = Mathf.Clamp01(baseParameters.complexity * complexityScale);
        
        // Adjust connectivity
        float connectivityScale = Mathf.Lerp(0.8f, 1.0f, performanceFactor);
        optimizedParams.connectivity = Mathf.Clamp01(baseParameters.connectivity * connectivityScale);
        
        return optimizedParams;
    }
    
    /// <summary>
    /// Checks if the system can handle a morphology of the specified size
    /// </summary>
    /// <param name="nodeCount">Number of nodes</param>
    /// <param name="connectionCount">Number of connections</param>
    /// <returns>True if the system can handle it</returns>
    public bool CanHandleMorphologySize(int nodeCount, int connectionCount)
    {
        // Base estimate on current performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        
        // Adjust max counts based on performance
        int adjustedMaxNodes = Mathf.RoundToInt(maxVisibleNodes * performanceFactor);
        int adjustedMaxConnections = Mathf.RoundToInt(maxVisibleConnections * performanceFactor);
        
        return (nodeCount <= adjustedMaxNodes && connectionCount <= adjustedMaxConnections);
    }
    
    /// <summary>
    /// Determines if a node should be culled based on distance and system load
    /// </summary>
    /// <param name="node">The node to check</param>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>True if the node should be culled</returns>
    public bool ShouldCullNode(MorphNode node, Vector3 cameraPosition)
    {
        if (!enableNodeCulling) return false;
        
        // Distance-based culling
        float distance = Vector3.Distance(node.transform.position, cameraPosition);
        return distance > nodeLODDistance * currentRenderDistance;
    }
    
    /// <summary>
    /// Determines if a connection should be culled based on distance and system load
    /// </summary>
    /// <param name="connection">The connection to check</param>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>True if the connection should be culled</returns>
    public bool ShouldCullConnection(MorphConnection connection, Vector3 cameraPosition)
    {
        if (!enableNodeCulling) return false;
        
        // Get midpoint of connection
        Vector3 midpoint = Vector3.zero;
        if (connection.NodeA != null && connection.NodeB != null)
        {
            midpoint = (connection.NodeA.transform.position + connection.NodeB.transform.position) * 0.5f;
        }
        else if (connection.NodeA != null)
        {
            midpoint = connection.NodeA.transform.position;
        }
        else if (connection.NodeB != null)
        {
            midpoint = connection.NodeB.transform.position;
        }
        
        // Distance-based culling
        float distance = Vector3.Distance(midpoint, cameraPosition);
        return distance > connectionLODDistance * currentRenderDistance;
    }
    
    /// <summary>
    /// Gets the appropriate LOD level for a node based on distance
    /// </summary>
    /// <param name="node">The node</param>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>LOD level (0 = highest detail, 2 = lowest)</returns>
    public int GetNodeLODLevel(MorphNode node, Vector3 cameraPosition)
    {
        if (!enableLOD) return 0;
        
        float distance = Vector3.Distance(node.transform.position, cameraPosition);
        
        if (distance < nodeLODDistance * 0.5f)
            return 0;
        else if (distance < nodeLODDistance)
            return 1;
        else
            return 2;
    }
    
    /// <summary>
    /// Gets the node size multiplier based on current performance
    /// </summary>
    /// <returns>Node size multiplier (0.5 - 1.0)</returns>
    public float GetNodeSizeMultiplier()
    {
        return nodeSizeMultiplier;
    }
    
    /// <summary>
    /// Gets the optimal mesh complexity for generators
    /// </summary>
    /// <returns>Mesh complexity level (4-24)</returns>
    public int GetOptimalMeshComplexity()
    {
        // Scale with performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        
        // Map to range from 4 segments (low) to 24 segments (high)
        return Mathf.RoundToInt(Mathf.Lerp(4, 24, performanceFactor));
    }
    
    /// <summary>
    /// Forces an immediate optimization pass
    /// </summary>
    public void ForceOptimization()
    {
        StartCoroutine(OptimizeNow());
    }
    
    /// <summary>
    /// Returns a recommended physics time step based on current performance
    /// </summary>
    /// <returns>Recommended physics time step</returns>
    public float GetRecommendedPhysicsTimeStep()
    {
        if (!useAdaptiveTimeStep) return maxPhysicsTimeStep;
        
        // Scale with performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        return Mathf.Lerp(maxPhysicsTimeStep, minPhysicsTimeStep, performanceFactor);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes performance metrics
    /// </summary>
    private void InitializeMetrics()
    {
        // Initialize with some reasonable defaults
        averageFrameTime = 1.0f / 60.0f;
        averageFrameRate = 60.0f;
        
        // Pre-fill sample list
        for (int i = 0; i < maxSamples; i++)
        {
            frameTimeSamples.Add(averageFrameTime);
        }
        
        // Initialize frame history
        for (int i = 0; i < frameHistoryLength; i++)
        {
            frameTimeHistory.Enqueue(averageFrameTime);
        }
    }
    
    /// <summary>
    /// Applies quality settings for a specified quality level
    /// </summary>
    /// <param name="quality">Quality level to apply</param>
    private void ApplyQualitySettings(QualityLevel quality)
    {
        switch (quality)
        {
            case QualityLevel.Low:
                QualitySettings.SetQualityLevel(0, true);
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = ShadowQuality.Disable;
                QualitySettings.antiAliasing = 0;
                QualitySettings.globalTextureMipmapLimit = 1;
                currentRenderDistance = 70;
                nodeSizeMultiplier = 0.7f;
                break;
                
            case QualityLevel.Medium:
                QualitySettings.SetQualityLevel(1, true);
                QualitySettings.vSyncCount = 0;
                QualitySettings.shadows = ShadowQuality.HardOnly;
                QualitySettings.antiAliasing = 0;
                QualitySettings.globalTextureMipmapLimit = 0;
                currentRenderDistance = 85;
                nodeSizeMultiplier = 0.85f;
                break;
                
            case QualityLevel.High:
                QualitySettings.SetQualityLevel(2, true);
                QualitySettings.vSyncCount = 1;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 4;
                QualitySettings.globalTextureMipmapLimit = 0;
                currentRenderDistance = 100;
                nodeSizeMultiplier = 1.0f;
                break;
                
            case QualityLevel.Ultra:
                QualitySettings.SetQualityLevel(3, true);
                QualitySettings.vSyncCount = 1;
                QualitySettings.shadows = ShadowQuality.All;
                QualitySettings.antiAliasing = 8;
                QualitySettings.globalTextureMipmapLimit = 0;
                currentRenderDistance = 120;
                nodeSizeMultiplier = 1.0f;
                break;
        }
        
        // Apply physics settings
        if (enablePhysicsOptimization)
        {
            // Adjust physics iterations based on quality
            switch (quality)
            {
                case QualityLevel.Low:
                    Physics.defaultSolverIterations = 2;
                    break;
                case QualityLevel.Medium:
                    Physics.defaultSolverIterations = 4;
                    break;
                case QualityLevel.High:
                    Physics.defaultSolverIterations = 6;
                    break;
                case QualityLevel.Ultra:
                    Physics.defaultSolverIterations = 8;
                    break;
            }
        }
        
#if UNITY_2019_1_OR_NEWER
        // Apply dynamic resolution if enabled
        if (enableDynamicResolution)
        {
            switch (quality)
            {
                case QualityLevel.Low:
                    currentResolutionScale = minResolutionScale;
                    break;
                case QualityLevel.Medium:
                    currentResolutionScale = Mathf.Lerp(minResolutionScale, maxResolutionScale, 0.33f);
                    break;
                case QualityLevel.High:
                    currentResolutionScale = Mathf.Lerp(minResolutionScale, maxResolutionScale, 0.67f);
                    break;
                case QualityLevel.Ultra:
                    currentResolutionScale = maxResolutionScale;
                    break;
            }
            
            // Apply resolution scale
            if (DynamicResolutionHandler.instance != null)
            {
                DynamicResolutionHandler.SetDynamicResScaler(
                    () => currentResolutionScale,
                    DynamicResScalePolicyType.ReturnsPercentage);
            }
        }
#endif
    }
    
    /// <summary>
    /// Updates memory usage statistics
    /// </summary>
    private void UpdateMemoryUsage()
    {
        // Calculate approximate memory usage (0-1 range)
        long totalMemory = SystemInfo.systemMemorySize * 1024 * 1024;
        long usedMemory = System.GC.GetTotalMemory(false);
        
        memoryUsage = (float)usedMemory / totalMemory;
        
        // Log warning if memory usage is high
        if (logPerformanceWarnings && memoryUsage > memoryWarningThreshold)
        {
            Debug.LogWarning($"High memory usage: {memoryUsage:P0} of system memory");
        }
    }
    
    /// <summary>
    /// Checks for performance issues and triggers optimization if needed
    /// </summary>
    private void CheckPerformance()
    {
        // Check frame rate
        if (averageFrameRate < minAcceptableFrameRate)
        {
            // Significant performance issue - optimize now
            StartCoroutine(OptimizeNow());
            return;
        }
        
        // Check for sudden changes in frame rate
        float frameRateStdDev = CalculateFrameRateStandardDeviation();
        if (frameRateStdDev > 10f) // More than 10 FPS deviation
        {
            StartCoroutine(OptimizeNow());
            return;
        }
        
        // Check memory usage
        if (memoryUsage > memoryWarningThreshold)
        {
            StartCoroutine(OptimizeNow());
            return;
        }
    }
    
    /// <summary>
    /// Calculates the standard deviation of recent frame rates
    /// </summary>
    /// <returns>Standard deviation of frame rates</returns>
    private float CalculateFrameRateStandardDeviation()
    {
        if (frameTimeSamples.Count < 2) return 0f;
        
        // Convert frame times to frame rates
        List<float> frameRates = frameTimeSamples.Select(ft => 1.0f / ft).ToList();
        
        // Calculate mean
        float mean = frameRates.Average();
        
        // Calculate sum of squared differences
        float sumSqDiff = frameRates.Sum(rate => (rate - mean) * (rate - mean));
        
        // Calculate variance and standard deviation
        float variance = sumSqDiff / (frameRates.Count - 1);
        return Mathf.Sqrt(variance);
    }
    
    /// <summary>
    /// Performs an immediate optimization pass
    /// </summary>
    private IEnumerator OptimizeNow()
    {
        if (isOptimizing) yield break;
        
        isOptimizing = true;
        
        // Wait one frame to get stable measurements
        yield return null;
        
        QualityLevel newQuality = currentQualityLevel;
        
        // Determine new quality level based on frame rate
        if (averageFrameRate < minAcceptableFrameRate * 0.5f)
        {
            // Severe performance issues - drop to lowest quality
            newQuality = QualityLevel.Low;
        }
        else if (averageFrameRate < minAcceptableFrameRate * 0.8f)
        {
            // Significant performance issues - drop one level
            newQuality = (QualityLevel)Mathf.Max(0, (int)currentQualityLevel - 1);
        }
        else if (averageFrameRate > targetFrameRate * 1.2f && currentQualityLevel < QualityLevel.Ultra)
        {
            // Performance is good - try increasing quality
            newQuality = (QualityLevel)Mathf.Min((int)QualityLevel.Ultra, (int)currentQualityLevel + 1);
        }
        
        // Apply new quality if changed
        if (newQuality != currentQualityLevel)
        {
            Debug.Log($"Adjusting quality from {currentQualityLevel} to {newQuality} (FPS: {averageFrameRate:0.0})");
            SetQualityLevel(newQuality);
        }
        
        // Adjust resolution scale if enabled
#if UNITY_2019_1_OR_NEWER
        if (enableDynamicResolution)
        {
            // Calculate new resolution scale based on GPU time
            float gpuLoad = currentGPUTime / (1.0f / targetFrameRate);
            float targetScale = currentResolutionScale;
            
            if (gpuLoad > targetGPUUsage * 1.2f)
            {
                // GPU is overloaded - reduce resolution
                targetScale = Mathf.Max(minResolutionScale, currentResolutionScale - 0.1f);
            }
            else if (gpuLoad < targetGPUUsage * 0.8f && currentResolutionScale < maxResolutionScale)
            {
                // GPU has headroom - increase resolution
                targetScale = Mathf.Min(maxResolutionScale, currentResolutionScale + 0.05f);
            }
            
            // Apply new scale if significantly different
            if (Mathf.Abs(targetScale - currentResolutionScale) > 0.03f)
            {
                currentResolutionScale = targetScale;
                Debug.Log($"Adjusting resolution scale to {currentResolutionScale:P0} (GPU load: {gpuLoad:P0})");
            }
        }
#endif
        
        // Wait a bit before allowing further optimization
        isOptimizing = false;
    }
    
    /// <summary>
    /// Continuous optimization routine that runs at intervals
    /// </summary>
    private IEnumerator OptimizationRoutine()
    {
        while (enableDynamicOptimization)
        {
            // Wait for the specified interval
            yield return new WaitForSeconds(optimizationInterval);
            
            // Only optimize if we're not already doing so
            if (!isOptimizing)
            {
                yield return StartCoroutine(OptimizeNow());
            }
        }
    }
    #endregion

    #region Optimization Utilities
    /// <summary>
    /// Batch-processes a list of nodes to determine which should be culled
    /// </summary>
    /// <param name="nodes">List of all nodes</param>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>List of visible nodes</returns>
    public List<MorphNode> GetVisibleNodes(List<MorphNode> nodes, Vector3 cameraPosition)
    {
        if (!enableNodeCulling || nodes == null)
            return nodes;
        
        // Calculate optimal visible node count based on performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        int optimalNodeCount = Mathf.RoundToInt(Mathf.Lerp(maxVisibleNodes * 0.5f, maxVisibleNodes, performanceFactor));
        
        // If we have fewer nodes than the optimal count, show all
        if (nodes.Count <= optimalNodeCount)
            return nodes;
        
        // Sort nodes by distance to camera
        List<MorphNode> sortedNodes = nodes
            .OrderBy(n => Vector3.Distance(n.transform.position, cameraPosition))
            .ToList();
        
        // Return the closest N nodes
        return sortedNodes.Take(optimalNodeCount).ToList();
    }
    
    /// <summary>
    /// Batch-processes a list of connections to determine which should be culled
    /// </summary>
    /// <param name="connections">List of all connections</param>
    /// <param name="cameraPosition">Current camera position</param>
    /// <returns>List of visible connections</returns>
    public List<MorphConnection> GetVisibleConnections(List<MorphConnection> connections, Vector3 cameraPosition)
    {
        if (!enableNodeCulling || connections == null)
            return connections;
        
        // Calculate optimal visible connection count based on performance
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        int optimalConnectionCount = Mathf.RoundToInt(Mathf.Lerp(maxVisibleConnections * 0.5f, maxVisibleConnections, performanceFactor));
        
        // If we have fewer connections than the optimal count, show all
        if (connections.Count <= optimalConnectionCount)
            return connections;
        
        // Sort connections by distance to camera
        List<MorphConnection> sortedConnections = connections
            .OrderBy(c => {
                Vector3 midpoint = (c.NodeA.transform.position + c.NodeB.transform.position) * 0.5f;
                return Vector3.Distance(midpoint, cameraPosition);
            })
            .ToList();
        
        // Return the closest N connections
        return sortedConnections.Take(optimalConnectionCount).ToList();
    }
    
    /// <summary>
    /// Creates an optimized mesh for the given nodes and connections
    /// </summary>
    /// <param name="nodes">Nodes to include</param>
    /// <param name="connections">Connections to include</param>
    /// <param name="biomorphType">Type of biomorph for mesh generation</param>
    /// <returns>Optimized mesh</returns>
    public Mesh CreateOptimizedMesh(List<MorphNode> nodes, List<int[]> connections, MorphologyParameters.BiomorphType biomorphType)
    {
        // Performance-based reduction in mesh complexity
        float performanceFactor = Mathf.Clamp01(averageFrameRate / targetFrameRate);
        
        // Get appropriate quality level
        int quality;
        if (performanceFactor < 0.3f)
            quality = 0; // Low
        else if (performanceFactor < 0.6f)
            quality = 1; // Medium
        else
            quality = 2; // High
        
        // Parameters for different quality levels
        int[] segmentCounts = { 6, 12, 24 };
        float[] smoothingFactors = { 0.5f, 0.7f, 0.9f };
        
        int segments = segmentCounts[quality];
        float smoothing = smoothingFactors[quality];
        
        // Call the mesh generator with optimized parameters
        Mesh mesh = MeshGenerator.GenerateMorphologyMesh(
            nodes.Select(n => n.transform.position).ToList(),
            connections,
            biomorphType,
            segments,
            smoothing
        );
        
        return mesh;
    }
    #endregion
}