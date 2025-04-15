using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BiomorphicSim.Core;

/// <summary>
/// Controls the growth of morphological structures over time.
/// Implements various growth patterns and coordinates the overall growth process.
/// </summary>
public class GrowthSystem : MonoBehaviour
{
    #region Properties
    [Header("Growth Settings")]
    [SerializeField] private float growthRate = 1.0f;
    [SerializeField] private float maxGrowthDistance = 5.0f;
    [SerializeField] private float minGrowthDistance = 1.0f;
    [SerializeField] private int maxNodesPerGrowthCycle = 10;
    [SerializeField] private float connectionProbability = 0.5f;
    [SerializeField] private AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    
    [Header("Constraints")]
    [SerializeField] private bool useGrowthBounds = true;
    [SerializeField] private Bounds growthBounds = new Bounds(Vector3.zero, Vector3.one * 50);
    [SerializeField] private LayerMask obstaclesMask;
    [SerializeField] private float obstacleAvoidanceStrength = 1.0f;
    
    [Header("References")]
    [SerializeField] private MorphologyGenerator morphologyGenerator;
    [SerializeField] private BiotypeBehaviors biotypeBehaviors;
    
    // Internal state
    private List<MorphNode> nodes = new List<MorphNode>();
    private List<MorphConnection> connections = new List<MorphConnection>();
    private MorphologyParameters currentParameters;
    private bool isGrowing = false;
    private float growthProgress = 0f;
    private float totalGrowthTime = 0f;
    private int targetNodeCount = 0;
    private GrowthInfluenceMap influenceMap;
    
    // Growth statistics
    private Dictionary<string, float> growthStats = new Dictionary<string, float>();
    
    // Public progress tracking
    public float GrowthProgress => growthProgress;
    public bool IsGrowing => isGrowing;
    public int CurrentNodeCount => nodes.Count;
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Create influence map
        influenceMap = new GrowthInfluenceMap(growthBounds, 1.0f);
        
        // Find references if not set
        if (morphologyGenerator == null)
        {
            morphologyGenerator = FindFirstObjectByType<MorphologyGenerator>();
        }
        
        if (biotypeBehaviors == null)
        {
            biotypeBehaviors = FindFirstObjectByType<BiotypeBehaviors>();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the growth system with nodes and connections
    /// </summary>
    /// <param name="initialNodes">Starting nodes</param>
    /// <param name="initialConnections">Starting connections</param>
    /// <param name="parameters">Growth parameters</param>
    public void Initialize(List<MorphNode> initialNodes, List<MorphConnection> initialConnections, MorphologyParameters parameters)
    {
        // Store references
        nodes = new List<MorphNode>(initialNodes);
        connections = new List<MorphConnection>(initialConnections);
        currentParameters = parameters;
        
        // Reset state
        isGrowing = false;
        growthProgress = 0f;
        totalGrowthTime = 0f;
        
        // Calculate target node count based on density
        float volume = growthBounds.size.x * growthBounds.size.y * growthBounds.size.z;
        targetNodeCount = Mathf.RoundToInt(1000 * parameters.density); // Scale by volume
        targetNodeCount = Mathf.Clamp(targetNodeCount, 50, 2000);
        
        // Initialize stats
        growthStats.Clear();
        growthStats["StartTime"] = Time.time;
        growthStats["InitialNodeCount"] = nodes.Count;
        
        // Reset influence map
        influenceMap.Reset();
        
        // Add initial nodes to influence map
        foreach (var node in nodes)
        {
            Vector3 position = node.transform.position;
            influenceMap.AddInfluencePoint(position, 1.0f);
        }
        
        Debug.Log($"Growth system initialized with {nodes.Count} nodes, target: {targetNodeCount}");
    }
    
    /// <summary>
    /// Starts the growth process
    /// </summary>
    /// <param name="duration">Duration of growth in seconds</param>
    /// <returns>Coroutine for tracking</returns>
    public Coroutine StartGrowth(float duration = 10f)
    {
        if (isGrowing)
        {
            Debug.LogWarning("Growth already in progress");
            return null;
        }
        
        return StartCoroutine(GrowCoroutine(duration));
    }
    
    /// <summary>
    /// Stops the current growth process
    /// </summary>
    public void StopGrowth()
    {
        isGrowing = false;
    }
    
    /// <summary>
    /// Grows the structure by one step
    /// </summary>
    /// <returns>Number of new nodes created</returns>
    public int GrowStep()
    {
        if (nodes.Count >= targetNodeCount) return 0;
        
        int nodesAtStart = nodes.Count;
        int newNodesCreated = 0;
        
        // Select a growth algorithm based on biomorph type
        IGrowthAlgorithm growthAlgorithm = GetGrowthAlgorithm();
        
        // Grow up to maximum nodes per cycle
        int nodesToGrow = Mathf.Min(maxNodesPerGrowthCycle, targetNodeCount - nodes.Count);
        
        for (int i = 0; i < nodesToGrow; i++)
        {
            // Use algorithm to calculate growth
            GrowthResult result = growthAlgorithm.CalculateGrowth(
                nodes, 
                connections, 
                growthBounds, 
                currentParameters,
                influenceMap,
                obstaclesMask
            );
            
            // Check if growth is valid
            if (result.isValid)
            {
                // Create new node
                MorphNode newNode = CreateNode(result.position);
                
                // Create connection to parent node
                if (result.parentNode != null)
                {
                    CreateConnection(result.parentNode, newNode);
                }
                
                // Create additional connections based on connectivity parameter
                CreateAdditionalConnections(newNode, currentParameters.connectivity);
                
                // Update influence map
                influenceMap.AddInfluencePoint(result.position, 1.0f);
                
                newNodesCreated++;
            }
        }
        
        // Apply any global adjustments to the system
        ApplyGlobalAdjustments();
        
        return newNodesCreated;
    }
    
    /// <summary>
    /// Gets the current growth statistics
    /// </summary>
    /// <returns>Dictionary of growth statistics</returns>
    public Dictionary<string, float> GetGrowthStatistics()
    {
        // Update stats
        growthStats["CurrentNodeCount"] = nodes.Count;
        growthStats["GrowthProgress"] = growthProgress;
        growthStats["GrowthTime"] = totalGrowthTime;
        growthStats["GrowthRate"] = nodes.Count / Mathf.Max(0.1f, totalGrowthTime);
        growthStats["ConnectionDensity"] = connections.Count / Mathf.Max(1, nodes.Count);
        
        return new Dictionary<string, float>(growthStats);
    }
    
    /// <summary>
    /// Sets the growth bounds
    /// </summary>
    /// <param name="bounds">New growth bounds</param>
    public void SetGrowthBounds(Bounds bounds)
    {
        growthBounds = bounds;
        useGrowthBounds = true;
        
        // Reset influence map with new bounds
        influenceMap = new GrowthInfluenceMap(bounds, 1.0f);
        
        // Re-add nodes to influence map
        foreach (var node in nodes)
        {
            Vector3 position = node.transform.position;
            influenceMap.AddInfluencePoint(position, 1.0f);
        }
    }
    
    /// <summary>
    /// Updates growth parameters
    /// </summary>
    /// <param name="parameters">New growth parameters</param>
    public void UpdateParameters(MorphologyParameters parameters)
    {
        currentParameters = parameters;
        
        // Recalculate target node count
        float volume = growthBounds.size.x * growthBounds.size.y * growthBounds.size.z;
        targetNodeCount = Mathf.RoundToInt(1000 * parameters.density); // Scale by volume
        targetNodeCount = Mathf.Clamp(targetNodeCount, 50, 2000);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Coroutine for growing the structure over time
    /// </summary>
    private IEnumerator GrowCoroutine(float duration)
    {
        isGrowing = true;
        growthProgress = 0f;
        float startTime = Time.time;
        
        while (isGrowing && nodes.Count < targetNodeCount && Time.time - startTime < duration)
        {
            // Grow one step
            int newNodes = GrowStep();
            
            // Update progress
            totalGrowthTime = Time.time - startTime;
            growthProgress = Mathf.Clamp01((float)nodes.Count / targetNodeCount);
            
            // Update structure visualization
            if (morphologyGenerator != null)
            {
                morphologyGenerator.UpdateMeshes();
            }
            
            // Log progress every 10% increase
            if (Mathf.FloorToInt(growthProgress * 10) > Mathf.FloorToInt((growthProgress - (float)newNodes / targetNodeCount) * 10))
            {
                Debug.Log($"Growth progress: {growthProgress:P0} ({nodes.Count}/{targetNodeCount} nodes)");
            }
            
            // Yield to avoid freezing
            yield return null;
        }
        
        // Final update
        totalGrowthTime = Time.time - startTime;
        growthProgress = Mathf.Clamp01((float)nodes.Count / targetNodeCount);
        
        // Update statistics
        growthStats["EndTime"] = Time.time;
        growthStats["TotalGrowthTime"] = totalGrowthTime;
        growthStats["FinalNodeCount"] = nodes.Count;
        
        isGrowing = false;
        
        Debug.Log($"Growth completed: {nodes.Count} nodes created in {totalGrowthTime:F2} seconds");
    }
    
    /// <summary>
    /// Creates a new node at the specified position
    /// </summary>
    private MorphNode CreateNode(Vector3 position)
    {
        // Delegate node creation to the morphology generator
        if (morphologyGenerator != null)
        {
            return morphologyGenerator.CreateNode(position);
        }
        
        Debug.LogError("Cannot create node: MorphologyGenerator is null");
        return null;
    }
    
    /// <summary>
    /// Creates a connection between two nodes
    /// </summary>
    private MorphConnection CreateConnection(MorphNode nodeA, MorphNode nodeB)
    {
        // Delegate connection creation to the morphology generator
        if (morphologyGenerator != null)
        {
            return morphologyGenerator.CreateConnection(nodeA, nodeB);
        }
        
        Debug.LogError("Cannot create connection: MorphologyGenerator is null");
        return null;
    }
    
    /// <summary>
    /// Creates additional connections from a node to nearby nodes
    /// </summary>
    private void CreateAdditionalConnections(MorphNode node, float connectionProbability)
    {
        if (node == null) return;
        
        // Find nearby nodes
        List<MorphNode> nearbyNodes = new List<MorphNode>();
        
        foreach (var existingNode in nodes)
        {
            if (existingNode == node) continue;
            
            float distance = Vector3.Distance(node.transform.position, existingNode.transform.position);
            
            // Check if within connection range
            if (distance >= minGrowthDistance && distance <= maxGrowthDistance)
            {
                nearbyNodes.Add(existingNode);
            }
        }
        
        // Connect to some nearby nodes based on probability
        foreach (var nearbyNode in nearbyNodes)
        {
            if (Random.value < connectionProbability)
            {
                CreateConnection(node, nearbyNode);
            }
        }
    }
    
    /// <summary>
    /// Applies global adjustments to all nodes and connections
    /// </summary>
    private void ApplyGlobalAdjustments()
    {
        // Apply any global forces or adjustments to the entire structure
        // This is useful for global effects like gravity or field forces
        
        // Example: Apply a slight downward force to simulate gravity
        if (currentParameters.biomorphType != MorphologyParameters.BiomorphType.Bone &&
            currentParameters.biomorphType != MorphologyParameters.BiomorphType.Coral)
        {
            // These types would resist gravity more
            return;
        }
        
        foreach (var node in nodes)
        {
            // Apply gravity with small chance
            if (Random.value < 0.05f)
            {
                Vector3 position = node.transform.position;
                position.y = Mathf.Max(growthBounds.min.y, position.y - 0.1f);
                node.transform.position = position;
            }
        }
    }
    
    /// <summary>
    /// Gets the appropriate growth algorithm based on current parameters
    /// </summary>
    private IGrowthAlgorithm GetGrowthAlgorithm()
    {
        // Use the biotype behaviors if available
        if (biotypeBehaviors != null)
        {
            return biotypeBehaviors.GetGrowthAlgorithm(currentParameters);
        }
        
        // Fallback to default algorithms
        switch (currentParameters.biomorphType)
        {
            case MorphologyParameters.BiomorphType.Mold:
                return new MoldGrowthAlgorithm();
                
            case MorphologyParameters.BiomorphType.Bone:
                return new BoneGrowthAlgorithm();
                
            case MorphologyParameters.BiomorphType.Coral:
                return new CoralGrowthAlgorithm();
                
            case MorphologyParameters.BiomorphType.Mycelium:
                return new MyceliumGrowthAlgorithm();
                
            case MorphologyParameters.BiomorphType.Custom:
                return new CustomGrowthAlgorithm();
                
            default:
                return new MoldGrowthAlgorithm();
        }
    }
    #endregion
}

/// <summary>
/// Result of a growth calculation
/// </summary>

/// <summary>
/// Influence map for guiding growth patterns
/// </summary>
public class GrowthInfluenceMap
{
    private Bounds bounds;
    private float cellSize;
    private Dictionary<Vector3Int, float> influenceGrid = new Dictionary<Vector3Int, float>();
    
    public GrowthInfluenceMap(Bounds mapBounds, float mapCellSize)
    {
        bounds = mapBounds;
        cellSize = mapCellSize;
    }
    
    public void Reset()
    {
        influenceGrid.Clear();
    }
    
    public void AddInfluencePoint(Vector3 position, float strength)
    {
        Vector3Int cell = GetCell(position);
        
        // Add or update influence
        if (influenceGrid.ContainsKey(cell))
        {
            influenceGrid[cell] += strength;
        }
        else
        {
            influenceGrid[cell] = strength;
        }
    }
    
    public float GetInfluenceAt(Vector3 position)
    {
        Vector3Int cell = GetCell(position);
        
        if (influenceGrid.TryGetValue(cell, out float value))
        {
            return value;
        }
        
        return 0f;
    }
    
    public Vector3 GetGradientAt(Vector3 position)
    {
        Vector3 gradient = Vector3.zero;
        Vector3Int cell = GetCell(position);
        
        // Check neighboring cells in all 6 directions
        Vector3Int[] neighbors = new Vector3Int[]
        {
            new Vector3Int(cell.x + 1, cell.y, cell.z),
            new Vector3Int(cell.x - 1, cell.y, cell.z),
            new Vector3Int(cell.x, cell.y + 1, cell.z),
            new Vector3Int(cell.x, cell.y - 1, cell.z),
            new Vector3Int(cell.x, cell.y, cell.z + 1),
            new Vector3Int(cell.x, cell.y, cell.z - 1)
        };
        
        float centerValue = GetInfluenceAt(position);
        
        for (int i = 0; i < neighbors.Length; i++)
        {
            Vector3Int neighbor = neighbors[i];
            
            // Get influence at this neighbor
            float neighborValue = 0f;
            if (influenceGrid.TryGetValue(neighbor, out float value))
            {
                neighborValue = value;
            }
            
            // Calculate direction vector
            Vector3 direction = Vector3.zero;
            switch (i)
            {
                case 0: direction = Vector3.right; break;
                case 1: direction = Vector3.left; break;
                case 2: direction = Vector3.up; break;
                case 3: direction = Vector3.down; break;
                case 4: direction = Vector3.forward; break;
                case 5: direction = Vector3.back; break;
            }
            
            // Add to gradient based on value difference
            gradient += direction * (neighborValue - centerValue);
        }
        
        // Normalize if not zero
        if (gradient.magnitude > 0.001f)
        {
            gradient.Normalize();
        }
        
        return gradient;
    }
    
    public Vector3Int GetCell(Vector3 position)
    {
        // Convert world position to cell coordinates
        int x = Mathf.FloorToInt((position.x - bounds.min.x) / cellSize);
        int y = Mathf.FloorToInt((position.y - bounds.min.y) / cellSize);
        int z = Mathf.FloorToInt((position.z - bounds.min.z) / cellSize);
        
        return new Vector3Int(x, y, z);
    }
}