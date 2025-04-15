using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BiomorphicSim.Core; // Add this line to use the correct types

/// <summary>
/// Handles the analysis of how morphological structures respond to different scenarios.
/// This class simulates environmental conditions and evaluates how the structure adapts.
/// </summary>
public class ScenarioAnalyzer : MonoBehaviour
{
    #region References
    [Header("Simulation Settings")]
    [SerializeField] private ScenarioSettings settings;
    
    [Header("Visualization")]
    [SerializeField] private Material standardMaterial;
    [SerializeField] private Material stressMaterial;
    [SerializeField] private Material adaptationMaterial;
    [SerializeField] private GameObject forceVisualizerPrefab;
    #endregion
    
    #region State
    // The morphology being analyzed
    private MorphologyData currentMorphology;
    private List<MorphNode> morphNodes = new List<MorphNode>();
    private List<MorphConnection> morphConnections = new List<MorphConnection>();
    
    // The current scenario
    private ScenarioData currentScenario;
    
    // Simulation state
    private bool isAnalyzing = false;
    private float analysisProgress = 0f;
    private Coroutine activeAnalysisCoroutine;
    
    // Analysis container
    private GameObject analysisContainer;
    
    // Results accumulation
    private ScenarioResults results;
    private Dictionary<string, List<float>> timeSeriesData = new Dictionary<string, List<float>>();
    
    // External force visualization
    private List<GameObject> forceVisualizers = new List<GameObject>();
    
    // Flag to track if a node has moved far enough to count as adaptation
    private bool significantAdaptationDetected = false;
    
    // Public progress tracker for UI
    public float AnalysisProgress => analysisProgress;
    
    // Used to track the current scenario
    public ScenarioData CurrentScenario => currentScenario;
    #endregion
    
    #region Public Methods
    /// <summary>
    /// Returns the current active scenario data
    /// </summary>
    public ScenarioData GetCurrentScenario()
    {
        return currentScenario;
    }
    /// <summary>
    /// Initializes the scenario analyzer
    /// </summary>
    public void Initialize()
    {
        // Create container if it doesn't exist
        if (analysisContainer == null)
        {
            analysisContainer = new GameObject("AnalysisContainer");
            analysisContainer.transform.parent = transform;
        }
        
        // Initialize simulation settings if not set
        if (settings == null)
        {
            settings = new ScenarioSettings
            {
                analysisTimeStep = 0.1f,
                maxIterations = 1000,
                convergenceThreshold = 0.01f
            };
        }
    }
    
    /// <summary>
    /// Updates the analyzer settings
    /// </summary>
    /// <param name="newSettings">New settings to apply</param>
    public void UpdateSettings(ScenarioSettings newSettings)
    {
        settings = newSettings;
    }
    
    /// <summary>
    /// Runs a scenario analysis on the specified morphology
    /// </summary>
    /// <param name="morphologyData">The morphology to analyze</param>
    /// <param name="scenario">The scenario to run</param>
    public void RunScenario(MorphologyData morphologyData, ScenarioData scenario)
    {
        // Stop any existing analysis
        if (isAnalyzing)
        {
            StopAnalysis();
        }
        
        // Save inputs
        currentMorphology = morphologyData;
        currentScenario = scenario;
        
        // Clear existing analysis
        ClearAnalysis();
        
        // Initialize results
        results = new ScenarioResults
        {
            scenarioId = scenario.scenarioName,
            morphologyId = Time.time.ToString(), // Use timestamp as ID if none specified
            metrics = new Dictionary<string, float>(),
            timeSeriesData = new Dictionary<string, List<float>>(),
            observations = new List<string>(),
            adaptationSuccessful = false
        };
        
        // Start analysis coroutine
        activeAnalysisCoroutine = StartCoroutine(AnalyzeScenarioCoroutine());
    }
    
    /// <summary>
    /// Stops the current analysis
    /// </summary>
    public void StopAnalysis()
    {
        if (activeAnalysisCoroutine != null)
        {
            StopCoroutine(activeAnalysisCoroutine);
            activeAnalysisCoroutine = null;
        }
        
        isAnalyzing = false;
        analysisProgress = 0f;
    }
    
    /// <summary>
    /// Clears the current analysis state
    /// </summary>
    public void ClearAnalysis()
    {
        // Destroy all nodes and connections
        foreach (var node in morphNodes)
        {
            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }
        
        foreach (var connection in morphConnections)
        {
            if (connection != null && connection.gameObject != null)
            {
                Destroy(connection.gameObject);
            }
        }
        
        morphNodes.Clear();
        morphConnections.Clear();
        
        // Clear force visualizers
        foreach (var visualizer in forceVisualizers)
        {
            if (visualizer != null)
            {
                Destroy(visualizer);
            }
        }
        
        forceVisualizers.Clear();
        
        // Clear container
        foreach (Transform child in analysisContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Reset time series data
        timeSeriesData.Clear();
        
        // Reset adaptation flag
        significantAdaptationDetected = false;
    }
    
    /// <summary>
    /// Gets the current analysis results
    /// </summary>
    public ScenarioResults GetResults()
    {
        return results;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Coroutine that runs the scenario analysis step by step
    /// </summary>
    private IEnumerator AnalyzeScenarioCoroutine()
    {
        isAnalyzing = true;
        analysisProgress = 0f;
        
        Debug.Log($"Starting scenario analysis: {currentScenario.scenarioName}");
        
        // Recreate morphology from data
        RecreateMorphology();
        
        // Initialize time series for metrics we want to track
        InitializeTimeSeriesData();
        
        // Create force visualizers for environmental factors
        CreateForceVisualizers();
        
        // Initial metrics
        CalculateAndStoreMetrics(0);
        
        // Run simulation for specified duration
        float simulationTime = 0f;
        int iteration = 0;
        float maxDisplacement = 0f;
        
        while (simulationTime < currentScenario.simulationDuration && iteration < settings.maxIterations)
        {
            // Calculate environmental forces for this step
            Dictionary<MorphNode, Vector3> nodeForces = CalculateEnvironmentalForces(simulationTime);
            
            // Apply forces to nodes and update state
            maxDisplacement = ApplyForcesToMorphology(nodeForces, settings.analysisTimeStep);
            
            // Check if we've achieved significant adaptation
            if (maxDisplacement > 0.5f)
            {
                significantAdaptationDetected = true;
            }
            
            // Calculate metrics for this time step
            CalculateAndStoreMetrics(simulationTime);
            
            // Update visualization
            UpdateMorphologyVisualization();
            
            // Update progress
            simulationTime += settings.analysisTimeStep;
            iteration++;
            analysisProgress = simulationTime / currentScenario.simulationDuration;
            
            // Check for convergence (if changes become very small)
            if (maxDisplacement < settings.convergenceThreshold && iteration > 10)
            {
                Debug.Log("Simulation converged early");
                results.observations.Add("Simulation converged to stable state at t=" + simulationTime.ToString("F1"));
                break;
            }
            
            // Yield to prevent freezing
            if (iteration % 10 == 0)
            {
                yield return null;
            }
        }
        
        // Complete analysis and prepare results
        FinalizeResults(simulationTime);
        
        // Final update to visualization
        UpdateMorphologyVisualization();
        
        // Analysis complete
        isAnalyzing = false;
        analysisProgress = 1f;
        
        Debug.Log("Scenario analysis complete");
        
        // Notify manager
        MorphologyManager.Instance.OnScenarioAnalysisComplete(results);
        
        activeAnalysisCoroutine = null;
    }
    
    /// <summary>
    /// Recreates the morphology from data for analysis
    /// </summary>
    private void RecreateMorphology()
    {
        if (currentMorphology == null || currentMorphology.nodePositions == null)
        {
            Debug.LogError("Cannot recreate morphology: Data is null or invalid");
            return;
        }
        
        // Create container for this analysis
        GameObject morphologyObject = new GameObject("AnalysisMorphology");
        morphologyObject.transform.parent = analysisContainer.transform;
        
        // Create nodes
        for (int i = 0; i < currentMorphology.nodePositions.Count; i++)
        {
            Vector3 position = currentMorphology.nodePositions[i];
            
            // Create node object
            GameObject nodeObject = new GameObject($"Node_{i}");
            nodeObject.transform.parent = morphologyObject.transform;
            nodeObject.transform.position = position;
            
            // Add MorphNode component
            MorphNode node = nodeObject.AddComponent<MorphNode>();
            node.Initialize(standardMaterial);
            
            morphNodes.Add(node);
        }
        
        // Create connections
        for (int i = 0; i < currentMorphology.connections.Count; i++)
        {
            int[] connectionIndices = currentMorphology.connections[i];
            
            if (connectionIndices.Length == 2)
            {
                int nodeAIndex = connectionIndices[0];
                int nodeBIndex = connectionIndices[1];
                
                if (nodeAIndex >= 0 && nodeAIndex < morphNodes.Count && 
                    nodeBIndex >= 0 && nodeBIndex < morphNodes.Count)
                {
                    // Create connection object
                    GameObject connectionObject = new GameObject($"Connection_{i}");
                    connectionObject.transform.parent = morphologyObject.transform;
                    
                    // Add MorphConnection component
                    MorphConnection connection = connectionObject.AddComponent<MorphConnection>();
                    connection.Initialize(morphNodes[nodeAIndex], morphNodes[nodeBIndex], standardMaterial);
                    
                    morphConnections.Add(connection);
                    
                    // Register with nodes
                    morphNodes[nodeAIndex].AddConnection(connection);
                    morphNodes[nodeBIndex].AddConnection(connection);
                }
            }
        }
        
        Debug.Log($"Recreated morphology with {morphNodes.Count} nodes and {morphConnections.Count} connections");
    }
    
    /// <summary>
    /// Initializes time series data for tracking metrics over time
    /// </summary>
    private void InitializeTimeSeriesData()
    {
        timeSeriesData.Clear();
        
        // Add standard metrics to track
        timeSeriesData.Add("AverageStress", new List<float>());
        timeSeriesData.Add("MaxStress", new List<float>());
        timeSeriesData.Add("TotalDisplacement", new List<float>());
        timeSeriesData.Add("AverageConnectivity", new List<float>());
        timeSeriesData.Add("MaxDisplacement", new List<float>());
        
        // Add scenario-specific metrics
        foreach (var factor in currentScenario.environmentalFactors.Keys)
        {
            timeSeriesData.Add($"Factor_{factor}", new List<float>());
        }
    }
    
    /// <summary>
    /// Creates visualizers for the environmental forces in the scenario
    /// </summary>
    private void CreateForceVisualizers()
    {
        if (forceVisualizerPrefab == null) return;
        
        // Clear existing visualizers
        foreach (var visualizer in forceVisualizers)
        {
            if (visualizer != null)
            {
                Destroy(visualizer);
            }
        }
        
        forceVisualizers.Clear();
        
        // Calculate center of morphology
        Vector3 center = Vector3.zero;
        foreach (var node in morphNodes)
        {
            center += node.transform.position;
        }
        center /= morphNodes.Count;
        
        // Create visualizers for each force
        foreach (var factor in currentScenario.environmentalFactors)
        {
            GameObject visualizer = Instantiate(forceVisualizerPrefab, center, Quaternion.identity, analysisContainer.transform);
            visualizer.name = $"Force_{factor.Key}";
            
            // Set color based on factor
            SetVisualizerProperties(visualizer, factor.Key, factor.Value);
            
            forceVisualizers.Add(visualizer);
        }
    }
    
    /// <summary>
    /// Sets the properties of a force visualizer
    /// </summary>
    private void SetVisualizerProperties(GameObject visualizer, string factorName, float factorValue)
    {
        // This would be implemented based on the specific visualizer prefab
        // For example, it might set color, scale, or direction based on the factor
        
        switch (factorName)
        {
            case "Wind":
                // Set color to blue, point in wind direction
                break;
                
            case "Gravity":
                // Set color to yellow, point downward
                break;
                
            case "Temperature":
                // Set color based on hot (red) or cold (blue)
                break;
                
            case "PedestrianFlow":
                // Set color to green, point in flow direction
                break;
                
            default:
                // Default visualization
                break;
        }
    }
    
    /// <summary>
    /// Calculates the environmental forces acting on each node
    /// </summary>
    private Dictionary<MorphNode, Vector3> CalculateEnvironmentalForces(float time)
    {
        Dictionary<MorphNode, Vector3> nodeForces = new Dictionary<MorphNode, Vector3>();
        
        foreach (var node in morphNodes)
        {
            Vector3 totalForce = Vector3.zero;
            
            // Apply different environmental factors
            foreach (var factor in currentScenario.environmentalFactors)
            {
                Vector3 force = CalculateForceFromFactor(node, factor.Key, factor.Value, time);
                totalForce += force;
            }
            
            // Store total force for this node
            nodeForces[node] = totalForce;
        }
        
        return nodeForces;
    }
    
    /// <summary>
    /// Calculates force on a node from a specific environmental factor
    /// </summary>
    private Vector3 CalculateForceFromFactor(MorphNode node, string factorName, float factorValue, float time)
    {
        Vector3 force = Vector3.zero;
        
        switch (factorName)
        {
            case "Wind":
                // Wind force in horizontal direction
                float windAngle = time * 0.1f; // Wind changes direction slowly
                Vector3 windDirection = new Vector3(Mathf.Cos(windAngle), 0, Mathf.Sin(windAngle)).normalized;
                force = windDirection * factorValue;
                
                // Exposed nodes get more wind force
                int connectionCount = node.ConnectionCount;
                float exposureFactor = Mathf.Max(0.5f, 1.0f - connectionCount * 0.1f);
                force *= exposureFactor;
                break;
                
            case "Gravity":
                // Gravity force downward
                force = Vector3.down * factorValue;
                
                // Nodes with more connections resist gravity better
                float supportFactor = Mathf.Max(0.2f, 1.0f - node.ConnectionCount * 0.15f);
                force *= supportFactor;
                break;
                
            case "Temperature":
                // Temperature causes expansion/contraction
                // Positive value = heat (expansion), negative = cold (contraction)
                force = (node.transform.position - GetMorphologyCenter()) * factorValue * 0.05f;
                break;
                
            case "PedestrianFlow":
                // Pedestrian flow along a specific path
                Vector3 pedestrianDirection = new Vector3(1, 0, 0.5f).normalized;
                
                // Check if node is near the flow path
                float distanceToPath = Vector3.Cross(pedestrianDirection, node.transform.position - Vector3.zero).magnitude;
                if (distanceToPath < 5f)
                {
                    force = pedestrianDirection * factorValue * (1f - distanceToPath / 5f);
                }
                break;
                
            case "SunlightExposure":
                // Sunlight from above and changing angle
                float sunAngle = time * 0.2f; // Sun moves across the sky
                Vector3 sunDirection = new Vector3(Mathf.Cos(sunAngle), 0.5f + 0.5f * Mathf.Sin(time * 0.1f), Mathf.Sin(sunAngle)).normalized;
                force = sunDirection * factorValue * 0.2f;
                break;
                
            case "SiteAttraction":
                // Attraction to the center of the site
                Vector3 toCenter = GetMorphologyCenter() - node.transform.position;
                float distanceFactor = Mathf.Clamp01(toCenter.magnitude / 20f);
                force = toCenter.normalized * factorValue * distanceFactor;
                break;
                
            default:
                Debug.LogWarning($"Unknown environmental factor: {factorName}");
                break;
        }
        
        return force;
    }
    
    /// <summary>
    /// Gets the center point of the morphology
    /// </summary>
    private Vector3 GetMorphologyCenter()
    {
        if (morphNodes.Count == 0) return Vector3.zero;
        
        Vector3 center = Vector3.zero;
        foreach (var node in morphNodes)
        {
            center += node.transform.position;
        }
        return center / morphNodes.Count;
    }
    
    /// <summary>
    /// Applies forces to the morphology and updates its state
    /// </summary>
    private float ApplyForcesToMorphology(Dictionary<MorphNode, Vector3> nodeForces, float deltaTime)
    {
        float maxDisplacement = 0f;
        
        // Apply forces to each node
        foreach (var node in morphNodes)
        {
            if (nodeForces.TryGetValue(node, out Vector3 force))
            {
                // Calculate displacement based on force, connections, and time
                float forceMagnitude = force.magnitude;
                
                // Nodes with more connections are more resistant to movement
                float resistanceFactor = 1.0f / (1.0f + node.ConnectionCount * 0.2f);
                
                // Calculate displacement direction and magnitude
                Vector3 displacement = force.normalized * forceMagnitude * resistanceFactor * deltaTime;
                
                // Track maximum displacement for convergence checking
                float displacementMagnitude = displacement.magnitude;
                maxDisplacement = Mathf.Max(maxDisplacement, displacementMagnitude);
                
                // Move the node
                node.transform.position += displacement;
                
                // Update node attributes
                node.UpdateAttributes(deltaTime, force);
            }
        }
        
        // Update connection attributes
        foreach (var connection in morphConnections)
        {
            Vector3 averageForce = Vector3.zero;
            
            if (nodeForces.TryGetValue(connection.NodeA, out Vector3 forceA) &&
                nodeForces.TryGetValue(connection.NodeB, out Vector3 forceB))
            {
                averageForce = (forceA + forceB) * 0.5f;
            }
            
            connection.UpdateAttributes(deltaTime, averageForce);
        }
        
        return maxDisplacement;
    }
    
    /// <summary>
    /// Calculates metrics and stores them in time series data
    /// </summary>
    private void CalculateAndStoreMetrics(float time)
    {
        // Calculate stress metrics
        float totalStress = 0f;
        float maxStress = 0f;
        
        foreach (var node in morphNodes)
        {
            totalStress += node.Stress;
            maxStress = Mathf.Max(maxStress, node.Stress);
        }
        
        float averageStress = totalStress / morphNodes.Count;
        
        // Calculate displacement metrics
        float totalDisplacement = 0f;
        float maxDisplacement = 0f;
        
        if (currentMorphology != null && currentMorphology.nodePositions != null)
        {
            for (int i = 0; i < morphNodes.Count && i < currentMorphology.nodePositions.Count; i++)
            {
                Vector3 originalPosition = currentMorphology.nodePositions[i];
                Vector3 currentPosition = morphNodes[i].transform.position;
                
                float displacement = Vector3.Distance(originalPosition, currentPosition);
                totalDisplacement += displacement;
                maxDisplacement = Mathf.Max(maxDisplacement, displacement);
            }
        }
        
        // Calculate connectivity metrics
        float totalConnections = 0f;
        foreach (var node in morphNodes)
        {
            totalConnections += node.ConnectionCount;
        }
        float averageConnectivity = totalConnections / morphNodes.Count;
        
        // Store metrics in time series
        if (timeSeriesData.ContainsKey("AverageStress"))
            timeSeriesData["AverageStress"].Add(averageStress);
            
        if (timeSeriesData.ContainsKey("MaxStress"))
            timeSeriesData["MaxStress"].Add(maxStress);
            
        if (timeSeriesData.ContainsKey("TotalDisplacement"))
            timeSeriesData["TotalDisplacement"].Add(totalDisplacement);
            
        if (timeSeriesData.ContainsKey("MaxDisplacement"))
            timeSeriesData["MaxDisplacement"].Add(maxDisplacement);
            
        if (timeSeriesData.ContainsKey("AverageConnectivity"))
            timeSeriesData["AverageConnectivity"].Add(averageConnectivity);
            
        // Store environmental factor values
        foreach (var factor in currentScenario.environmentalFactors)
        {
            string key = $"Factor_{factor.Key}";
            if (timeSeriesData.ContainsKey(key))
                timeSeriesData[key].Add(factor.Value);
        }
    }
    
    /// <summary>
    /// Updates the visual appearance of the morphology based on analysis state
    /// </summary>
    private void UpdateMorphologyVisualization()
    {
        // Update node visualization based on stress
        foreach (var node in morphNodes)
        {
            // Color nodes based on stress
            if (node.Stress > 0.7f)
            {
                // High stress - use stress material
                node.Initialize(stressMaterial);
            }
            else if (Vector3.Distance(node.transform.position, currentMorphology.nodePositions[morphNodes.IndexOf(node)]) > 1.0f)
            {
                // Significant movement - use adaptation material
                node.Initialize(adaptationMaterial);
            }
            else
            {
                // Normal stress - use standard material
                node.Initialize(standardMaterial);
            }
            
            node.UpdateVisual();
        }
        
        // Update connection visualization
        foreach (var connection in morphConnections)
        {
            // Color connections based on stress
            if (connection.Stress > 0.7f)
            {
                // High stress - use stress material
                connection.Initialize(connection.NodeA, connection.NodeB, stressMaterial);
            }
            else
            {
                // Normal stress - use standard material
                connection.Initialize(connection.NodeA, connection.NodeB, standardMaterial);
            }
            
            connection.UpdateVisual();
        }
    }
    
    /// <summary>
    /// Finalizes analysis results
    /// </summary>
    private void FinalizeResults(float simulationTime)
    {
        // Calculate final metrics
        CalculateFinalMetrics();
        
        // Copy time series data to results
        results.timeSeriesData = new Dictionary<string, List<float>>(timeSeriesData);
        
        // Determine if adaptation was successful
        results.adaptationSuccessful = significantAdaptationDetected && GetFinalStressLevel() < 0.5f;
        
        // Add observations
        AddObservations();
        
        Debug.Log($"Analysis completed. Adaptation successful: {results.adaptationSuccessful}");
    }
    
    /// <summary>
    /// Calculates final metrics for the results
    /// </summary>
    private void CalculateFinalMetrics()
    {
        // Calculate final stress
        float totalFinalStress = 0f;
        foreach (var node in morphNodes)
        {
            totalFinalStress += node.Stress;
        }
        float averageFinalStress = totalFinalStress / morphNodes.Count;
        
        // Calculate total displacement
        float totalDisplacement = 0f;
        for (int i = 0; i < morphNodes.Count && i < currentMorphology.nodePositions.Count; i++)
        {
            Vector3 originalPosition = currentMorphology.nodePositions[i];
            Vector3 finalPosition = morphNodes[i].transform.position;
            
            totalDisplacement += Vector3.Distance(originalPosition, finalPosition);
        }
        
        // Calculate structural changes
        float originalVolume = CalculateMorphologyVolume(currentMorphology.nodePositions);
        float finalVolume = CalculateMorphologyVolume(GetCurrentNodePositions());
        
        float volumeChange = (finalVolume - originalVolume) / originalVolume;
        
        // Store final metrics
        results.metrics["FinalAverageStress"] = averageFinalStress;
        results.metrics["TotalDisplacement"] = totalDisplacement;
        results.metrics["AverageDisplacement"] = totalDisplacement / morphNodes.Count;
        results.metrics["VolumeChange"] = volumeChange;
        results.metrics["AdaptationIndex"] = CalculateAdaptationIndex();
    }
    
    /// <summary>
    /// Calculates the volume of a morphology from node positions
    /// </summary>
    private float CalculateMorphologyVolume(List<Vector3> positions)
    {
        if (positions.Count < 2) return 0f;
        
        // Calculate the bounding box
        Vector3 min = positions[0];
        Vector3 max = positions[0];
        
        foreach (var pos in positions)
        {
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }
        
        Vector3 size = max - min;
        return size.x * size.y * size.z;
    }
    
    /// <summary>
    /// Gets current node positions
    /// </summary>
    private List<Vector3> GetCurrentNodePositions()
    {
        List<Vector3> positions = new List<Vector3>();
        foreach (var node in morphNodes)
        {
            positions.Add(node.transform.position);
        }
        return positions;
    }
    
    /// <summary>
    /// Calculates an adaptation index measuring how well the structure adapted
    /// </summary>
    private float CalculateAdaptationIndex()
    {
        // Higher is better (0 to 1)
        
        // Calculate average stress reduction
        float initialStress = timeSeriesData["AverageStress"].Count > 0 ? timeSeriesData["AverageStress"][0] : 0;
        float finalStress = GetFinalStressLevel();
        float stressReduction = Mathf.Max(0, (initialStress - finalStress) / initialStress);
        
        // Calculate movement efficiency (displacement vs stress)
        float totalDisplacement = results.metrics["TotalDisplacement"];
        float displacementEfficiency = Mathf.Min(1, totalDisplacement / (morphNodes.Count * 2));
        
        // Calculate structural integrity
        float structuralIntegrity = 1.0f;
        foreach (var connection in morphConnections)
        {
            if (connection.Stress > 0.7f)
            {
                structuralIntegrity -= 0.05f;
            }
        }
        structuralIntegrity = Mathf.Max(0, structuralIntegrity);
        
        // Combine factors
        return (stressReduction * 0.4f + displacementEfficiency * 0.3f + structuralIntegrity * 0.3f);
    }
    
    /// <summary>
    /// Gets the final stress level of the morphology
    /// </summary>
    private float GetFinalStressLevel()
    {
        float totalStress = 0f;
        foreach (var node in morphNodes)
        {
            totalStress += node.Stress;
        }
        return morphNodes.Count > 0 ? totalStress / morphNodes.Count : 0f;
    }
    
    /// <summary>
    /// Adds observations about the adaptation behavior
    /// </summary>
    private void AddObservations()
    {
        // Add general observations
        results.observations.Add($"Structure showed {(significantAdaptationDetected ? "significant" : "minimal")} adaptation movement.");
        
        // Stress observations
        float finalStress = GetFinalStressLevel();
        if (finalStress < 0.3f)
        {
            results.observations.Add("Structure successfully distributed stress and reached a stable state.");
        }
        else if (finalStress < 0.6f)
        {
            results.observations.Add("Structure adapted partially but maintains some stress points.");
        }
        else
        {
            results.observations.Add("Structure failed to adequately adapt to environmental forces, with high remaining stress.");
        }
        
        // Movement observations
        float totalDisplacement = results.metrics["TotalDisplacement"];
        float avgDisplacement = results.metrics["AverageDisplacement"];
        
        if (avgDisplacement < 0.5f)
        {
            results.observations.Add("Structure showed minimal displacement, maintaining its original form.");
        }
        else if (avgDisplacement < 2f)
        {
            results.observations.Add("Structure showed moderate adaptation through controlled displacement.");
        }
        else
        {
            results.observations.Add("Structure underwent significant morphological change through displacement.");
        }
        
        // Volume change observations
        float volumeChange = results.metrics["VolumeChange"];
        if (Mathf.Abs(volumeChange) < 0.1f)
        {
            results.observations.Add("Structure maintained its overall volume during adaptation.");
        }
        else if (volumeChange > 0)
        {
            results.observations.Add($"Structure expanded by approximately {(volumeChange * 100).ToString("F0")}% during adaptation.");
        }
        else
        {
            results.observations.Add($"Structure contracted by approximately {(Mathf.Abs(volumeChange) * 100).ToString("F0")}% during adaptation.");
        }
        
        // Add scenario-specific observations
        foreach (var factor in currentScenario.environmentalFactors)
        {
            switch (factor.Key)
            {
                case "Wind":
                    if (factor.Value > 0.7f)
                    {
                        results.observations.Add("Structure showed aerodynamic adaptation to strong wind forces.");
                    }
                    break;
                    
                case "Gravity":
                    if (factor.Value > 0.7f)
                    {
                        results.observations.Add("Structure developed load-bearing adaptations in response to gravity.");
                    }
                    break;
                    
                case "Temperature":
                    if (Mathf.Abs(factor.Value) > 0.7f)
                    {
                        results.observations.Add($"Structure adapted to {(factor.Value > 0 ? "expansion" : "contraction")} from temperature changes.");
                    }
                    break;
            }
        }
    }
    #endregion
}