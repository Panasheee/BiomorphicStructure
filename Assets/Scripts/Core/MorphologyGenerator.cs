using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Handles the generation of biomorphic structures based on biological growth patterns.
    /// </summary>
    public class MorphologyGenerator : MonoBehaviour
    {
        #region References
        [Header("Prefabs")]
        [SerializeField] private GameObject nodePrototype;
        [SerializeField] private GameObject connectionPrototype;
        
        [Header("Materials")]
        [SerializeField] private Material nodeMaterial;
        [SerializeField] private Material connectionMaterial;
        [SerializeField] private Material growthMaterial; // Example material

        [Header("Settings")]
        [SerializeField] private MorphologySettings settings; // Use consolidated type

        [Header("Runtime References")]
        [SerializeField] private UIManager uiManager; // Add reference if needed for progress

        // Internal State
        private List<MorphNode> nodes = new List<MorphNode>();
        private List<MorphConnection> connections = new List<MorphConnection>();
        private Bounds growthZone;
        private MorphologyParameters currentParameters; // Use consolidated type
        private bool isGenerating = false;
        private float generationProgress = 0f;
        private Coroutine activeGrowthCoroutine;
        private GameObject morphologyContainer; // Parent object for generated elements

        // Mesh Generation
        private MeshFilter combinedMeshFilter;
        private MeshRenderer combinedMeshRenderer;
        #endregion

        #region Properties
        public float GenerationProgress => generationProgress;
        public bool IsGenerating => isGenerating;
        #endregion

        #region Unity Methods        private void Awake()
        {
            InitializeMorphologyContainer();
            
            // Only find UIManager if needed
            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>();
        }
        
        private void InitializeMorphologyContainer()
        {
            // Initialize container
            morphologyContainer = new GameObject("MorphologyContainer");
            morphologyContainer.transform.SetParent(this.transform);

            // Initialize mesh components
            combinedMeshFilter = morphologyContainer.AddComponent<MeshFilter>();
            combinedMeshRenderer = morphologyContainer.AddComponent<MeshRenderer>();
            combinedMeshRenderer.material = growthMaterial;
        }

        private void Update()
        {
            // Optional: Update visual feedback during generation
            if (isGenerating && uiManager != null)
            {
                 // Assuming UIManager has a method like UpdateGenerationProgress
                 uiManager.UpdateGenerationProgress(generationProgress); 
            }
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Initializes the generator.
        /// </summary>
        public void Initialize()
        {
            ClearMorphology();
            Debug.Log("Morphology Generator Initialized.");
        }

        /// <summary>
        /// Updates the generator's settings.
        /// </summary>
        public void UpdateSettings(MorphologySettings newSettings)
        {
            settings = newSettings;
            Debug.Log("MorphologyGenerator settings updated.");
            // Apply settings changes if needed (e.g., update materials, scales)
            if (nodePrototype != null) nodePrototype.transform.localScale = Vector3.one * settings.nodeScale;
             if (connectionPrototype != null)
             {
                 // Assuming connection prototype has a way to set scale/radius
                 // connectionPrototype.GetComponent<SomeConnectionScript>()?.SetScale(settings.connectionScale);
             }
             if (combinedMeshRenderer != null)
             {
                 // Potentially update material based on settings
             }
        }

        /// <summary>
        /// Starts the morphology generation process within the specified zone.
        /// </summary>
        public void GenerateMorphology(Bounds zone, MorphologyParameters parameters)
        {
            if (isGenerating)
            {
                Debug.LogWarning("Generation already in progress.");
                return;
            }

            ClearMorphology();
            growthZone = zone;
            currentParameters = parameters;

            Debug.Log($"Starting morphology generation in zone: {zone.center} with parameters: {parameters.biomorphType}");

            // Start the coroutine
            activeGrowthCoroutine = StartCoroutine(GrowMorphologyCoroutine(parameters));
        }

        /// <summary>
        /// Stops the ongoing generation process.
        /// </summary>
        public void StopGeneration()
        {
            if (activeGrowthCoroutine != null)
            {
                StopCoroutine(activeGrowthCoroutine);
                activeGrowthCoroutine = null;
            }
            isGenerating = false;
            Debug.Log("Morphology generation stopped.");
            // Potentially finalize or clean up partial generation
        }

        /// <summary>
        /// Clears the currently generated morphology.
        /// </summary>
        public void ClearMorphology()
        {
            StopGeneration(); // Ensure any ongoing process is stopped

            // Destroy existing nodes and connections
            foreach (var connection in connections)
            {
                if (connection != null) Destroy(connection.gameObject);
            }
            connections.Clear();

            foreach (var node in nodes)
            {
                if (node != null) Destroy(node.gameObject);
            }
            nodes.Clear();

            // Clear combined mesh if used
            if (combinedMeshFilter != null && combinedMeshFilter.mesh != null)
            {
                Destroy(combinedMeshFilter.mesh);
                combinedMeshFilter.mesh = null;
            }

            generationProgress = 0f;
            Debug.Log("Existing morphology cleared.");
        }

        /// <summary>
        /// Creates a single node at the specified position.
        /// </summary>
        public MorphNode CreateNode(Vector3 position)
        {
            if (nodePrototype == null)
            {
                Debug.LogError("Node prototype is not set!");
                return null;
            }
            GameObject nodeObject = Instantiate(nodePrototype, position, Quaternion.identity, morphologyContainer.transform);
            nodeObject.name = $"Node_{nodes.Count}";
            MorphNode newNode = nodeObject.GetComponent<MorphNode>();
            if (newNode == null)
            {
                newNode = nodeObject.AddComponent<MorphNode>(); // Add component if missing
            }
            newNode.Initialize($"Node_{nodes.Count}"); // Initialize with node ID
            nodes.Add(newNode);
            return newNode;
        }

        /// <summary>
        /// Creates a connection between two nodes.
        /// </summary>
        public MorphConnection CreateConnection(MorphNode nodeA, MorphNode nodeB)
        {
             if (connectionPrototype == null)
            {
                Debug.LogError("Connection prototype is not set!");
                return null;
            }
             if (nodeA == null || nodeB == null)
             {
                 Debug.LogError("Cannot create connection: One or both nodes are null.");
                 return null;
             }

            // Avoid duplicate connections
            if (nodeA.IsConnectedTo(nodeB)) return null;

            GameObject connectionObject = Instantiate(connectionPrototype, morphologyContainer.transform);
            connectionObject.name = $"Connection_{connections.Count}";
            MorphConnection newConnection = connectionObject.GetComponent<MorphConnection>();
             if (newConnection == null)
            {
                 newConnection = connectionObject.AddComponent<MorphConnection>();
            }
            // Use connectionMaterial for the connection
            // Use Material parameter instead of string
            newConnection.Initialize(nodeA, nodeB, connectionMaterial); 
            connections.Add(newConnection);
            nodeA.AddConnection(newConnection);
            nodeB.AddConnection(newConnection);
            return newConnection;
        }

        /// <summary>
        /// Removes a specific connection.
        /// </summary>
        public void RemoveConnection(MorphConnection connectionToRemove)
        {
            if (connectionToRemove == null || !connections.Contains(connectionToRemove))
                return;

            if (connectionToRemove.NodeA != null)
                connectionToRemove.NodeA.RemoveConnection(connectionToRemove);
            if (connectionToRemove.NodeB != null)
                connectionToRemove.NodeB.RemoveConnection(connectionToRemove);

            connections.Remove(connectionToRemove);
            Destroy(connectionToRemove.gameObject);
        }

        /// <summary>
        /// Placeholder for triggering growth calculation (might be internal or called by GrowthSystem).
        /// </summary>
        public void CalculateGrowth(GrowthInfluenceMap influenceMap, LayerMask layerMask)
        {
            if (currentParameters == null)
            {
                Debug.LogWarning("Cannot calculate growth: MorphologyParameters not set.");
                return;
            }

            IGrowthAlgorithm growthAlgorithm = CreateGrowthAlgorithm(currentParameters);
            if (growthAlgorithm != null)
            {
                // Calculate potential growth points
                GrowthResult result = growthAlgorithm.CalculateGrowth(nodes, connections, growthZone, currentParameters, influenceMap, layerMask);
                
                // Process the result (e.g., add node if valid)
                if (result.isValid)
                {
                    MorphNode newNode = CreateNode(result.position);
                    if (result.parentNode != null)
                    {
                        CreateConnection(result.parentNode, newNode);
                    }
                    // Optionally create more connections based on proximity/parameters
                    AddConnections(newNode, currentParameters.connectivity); 
                }
                UpdateMeshes(); // Update visualization after potential change
            }
            else
            {
                Debug.LogError("Failed to create growth algorithm.");
            }
        }

        /// <summary>
        /// Imports the morphology data (e.g., recreating nodes/connections)
        /// </summary>
        public void ImportMorphologyData(MorphologyData data)
        {
            ClearMorphology();
            // Example: Rebuild morphology using data.nodePositions and data.connections.
            Debug.Log("Morphology data imported.");
        }

        public MorphologyData ExportMorphologyData()
        {
            MorphologyData data = new MorphologyData
            {
                morphologyId = $"Morph_{System.DateTime.Now:yyyyMMdd_HHmmss}",
                nodePositions = nodes.Select(n => n.transform.position).ToList(),
                connections = new List<int[]>(),
                metrics = CalculateMetrics(),
                parametersUsed = currentParameters,
                generationTime = System.DateTime.Now
            };

            // Map nodes to an index for connection data
            Dictionary<MorphNode, int> nodeIndexMap = nodes
                .Select((node, index) => new { node, index })
                .ToDictionary(pair => pair.node, pair => pair.index);

            foreach (var connection in connections)
            {
                if (connection.NodeA != null && connection.NodeB != null &&
                    nodeIndexMap.ContainsKey(connection.NodeA) && nodeIndexMap.ContainsKey(connection.NodeB))
                {
                    data.connections.Add(new int[] { nodeIndexMap[connection.NodeA], nodeIndexMap[connection.NodeB] });
                }
            }
            
            Debug.Log($"Exported MorphologyData with {data.nodePositions.Count} nodes and {data.connections.Count} connections.");
            return data;
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// Coroutine that handles the step-by-step growth process.
        /// </summary>
        private IEnumerator GrowMorphologyCoroutine(MorphologyParameters parameters)
        {
            isGenerating = true;
            generationProgress = 0f;
            
            IGrowthAlgorithm growthAlgorithm = CreateGrowthAlgorithm(parameters);
            if (growthAlgorithm == null)
            {
                 Debug.LogError("Failed to create growth algorithm. Aborting generation.");
                 isGenerating = false;
                 yield break;
            }

            PlaceSeedNodes(parameters);
            UpdateMeshes(); // Initial mesh update
            yield return null; // Wait a frame

            // Calculate target node count based on density and volume/settings
            int targetNodeCount = Mathf.Clamp(Mathf.RoundToInt(settings.maxNodes * parameters.density), settings.initialSeedCount, settings.maxNodes);

            float startTime = Time.time;
            float maxDuration = 60f; // Add a safety break time

            // Main growth loop
            while (nodes.Count < targetNodeCount && isGenerating && (Time.time - startTime) < maxDuration)
            {
                // --- Growth Step Calculation ---
                int stepsPerFrame = Mathf.Max(1, Mathf.RoundToInt(parameters.growthRate * 10)); // Adjust multiplier as needed
                int nodesAddedThisFrame = 0;
                for(int i = 0; i < stepsPerFrame && nodes.Count < targetNodeCount; i++)
                {
                    GrowthResult result = growthAlgorithm.CalculateGrowth(nodes, connections, growthZone, parameters, null, default); // Pass influence map & mask if used
                    if (result.isValid)
                    {
                        MorphNode newNode = CreateNode(result.position);
                        if (result.parentNode != null)
                        {
                            CreateConnection(result.parentNode, newNode);
                        }
                        AddConnections(newNode, parameters.connectivity); // Add more connections potentially
                        nodesAddedThisFrame++;
                    }
                }

                if(nodesAddedThisFrame > 0)
                {
                    UpdateMeshes(); // Update mesh only if something changed
                }

                generationProgress = (float)nodes.Count / targetNodeCount;
                
                // Update UI progress (if needed)
                if (uiManager != null)
                {
                    uiManager.UpdateGenerationProgress(generationProgress);
                }

                yield return null; // Wait for the next frame
            }
            
            // Finalization
            if (isGenerating) // Check if loop finished naturally or was stopped
            {
                 Debug.Log($"Growth loop finished. Nodes: {nodes.Count}/{targetNodeCount}");
                 generationProgress = 1f;
                 // Post-processing steps
                 AddConnections(parameters.connectivity); // Final connection pass
                 OptimizeMorphology(); // Optional optimization step
                 UpdateMeshes(); // Final mesh update
            }
            else {
                 Debug.Log($"Growth loop interrupted. Nodes: {nodes.Count}");
            }

            MorphologyData finalData = ExportMorphologyData();
            isGenerating = false;
            
            // Notify Manager - Use the singleton reference to avoid ambiguity
            MorphologyManager manager = MorphologyManager.Instance;
            if (manager != null)
            {
                manager.OnMorphologyGenerationComplete(finalData);
            }
            else {
                 Debug.LogError("MorphologyManager instance not found to report completion.");
            }

            activeGrowthCoroutine = null;
        }
        
        /// <summary>
        /// Places the initial seed nodes for growth.
        /// </summary>
        private void PlaceSeedNodes(MorphologyParameters parameters)
        {
            int seedCount = Mathf.Max(1, settings.initialSeedCount); // Ensure at least one seed
            
            for (int i = 0; i < seedCount; i++)
            {
                // Place seeds randomly within the zone (consider different strategies)
                Vector3 randomPosition = new Vector3(
                    Random.Range(growthZone.min.x, growthZone.max.x),
                    Random.Range(growthZone.min.y, growthZone.max.y),
                    Random.Range(growthZone.min.z, growthZone.max.z)
                );
                
                // Ensure position is within bounds (might be redundant if Random.Range is inclusive)
                if (growthZone.Contains(randomPosition))
                {
                    CreateNode(randomPosition);
                }
                else {
                    // Fallback or retry if random position was somehow outside
                    CreateNode(growthZone.center); 
                    Debug.LogWarning("Generated seed position was outside bounds, placed at center instead.");
                }
            }
            Debug.Log($"Placed {nodes.Count} seed nodes.");
        }

        /// <summary>
        /// Adds additional connections between nearby nodes based on probability.
        /// Can be called per node or as a global pass.
        /// </summary>
        private void AddConnections(MorphNode newNode, float probability)
        {
             if (newNode == null || probability <= 0) return;

             float maxDistSq = settings.nodeMaxDistance * settings.nodeMaxDistance * 1.5f; // Increase search radius slightly

             // Simple O(N) check against all other nodes (optimize for large N)
             foreach (var otherNode in nodes)
             {
                 if (otherNode == newNode || newNode.IsConnectedTo(otherNode)) continue;

                 float distSq = (newNode.transform.position - otherNode.transform.position).sqrMagnitude;
                 if (distSq < maxDistSq && distSq > (settings.nodeMinDistance * settings.nodeMinDistance * 0.8f)) // Check max and min distance
                 {
                     // Probability check - higher chance for closer nodes?
                     float connectionChance = probability * (1f - Mathf.Sqrt(distSq) / (settings.nodeMaxDistance * 1.2f));
                     if (Random.value < connectionChance)
                     {
                         CreateConnection(newNode, otherNode);
                     }
                 }
             }
        }
          /// <summary>
        /// Global pass to add connections based on probability.
        /// </summary>
        private void AddConnections(float probability)
        {
            if (probability <= 0 || nodes.Count < 2) return;
            
            int added = 0;
            float maxDistSq = settings.nodeMaxDistance * settings.nodeMaxDistance;
            float minDistSq = settings.nodeMinDistance * settings.nodeMinDistance;

            // Use spatial partitioning for larger node sets
            if (nodes.Count > 100)
            {
                // Create a simple grid-based spatial partition
                Dictionary<Vector3Int, List<MorphNode>> grid = new Dictionary<Vector3Int, List<MorphNode>>();
                float cellSize = settings.nodeMaxDistance;
                
                // Add nodes to grid
                foreach (var node in nodes)
                {
                    Vector3Int cell = new Vector3Int(
                        Mathf.FloorToInt(node.transform.position.x / cellSize),
                        Mathf.FloorToInt(node.transform.position.y / cellSize),
                        Mathf.FloorToInt(node.transform.position.z / cellSize)
                    );
                    
                    if (!grid.ContainsKey(cell))
                        grid[cell] = new List<MorphNode>();
                    
                    grid[cell].Add(node);
                }
                
                // Connect only within neighboring cells
                foreach (var node in nodes)
                {
                    Vector3Int cell = new Vector3Int(
                        Mathf.FloorToInt(node.transform.position.x / cellSize),
                        Mathf.FloorToInt(node.transform.position.y / cellSize),
                        Mathf.FloorToInt(node.transform.position.z / cellSize)
                    );
                    
                    // Get neighboring cells
                    for (int x = -1; x <= 1; x++)
                    {
                        for (int y = -1; y <= 1; y++)
                        {
                            for (int z = -1; z <= 1; z++)
                            {
                                Vector3Int neighborCell = new Vector3Int(cell.x + x, cell.y + y, cell.z + z);
                                if (!grid.ContainsKey(neighborCell)) continue;
                                
                                foreach (var otherNode in grid[neighborCell])
                                {
                                    if (node == otherNode || node.IsConnectedTo(otherNode)) continue;
                                    
                                    float distSq = (node.transform.position - otherNode.transform.position).sqrMagnitude;
                                    if (distSq >= minDistSq && distSq <= maxDistSq && Random.value < probability)
                                    {
                                        CreateConnection(node, otherNode);
                                        added++;
                                    }
                                }
                            }
                        }
                    }
                }
            }
            else
            {
                // Original O(N²) approach for smaller sets
                for (int i = 0; i < nodes.Count; i++)
                {
                    for (int j = i + 1; j < nodes.Count; j++)
                    {
                        MorphNode nodeA = nodes[i];
                        MorphNode nodeB = nodes[j];

                        if (nodeA.IsConnectedTo(nodeB)) continue;

                        float distSq = (nodeA.transform.position - nodeB.transform.position).sqrMagnitude;
                        if (distSq >= minDistSq && distSq <= maxDistSq)
                        {
                            // Simple probability check
                            if (Random.value < probability)
                            {
                                CreateConnection(nodeA, nodeB);
                                added++;
                            }
                        }
                    }
                }
            }
            
            if (added > 0)
            {
                Debug.Log($"Added {added} connections in global pass.");
            }
        }

        /// <summary>
        /// Updates the visual representation (individual objects or combined mesh).
        /// </summary>
        private void UpdateMeshes()
        {
            // Individual GameObjects for nodes/connections are updated through their own components
            
            // For combined mesh, you would update it here if needed
            if (combinedMeshFilter != null)
            {
                 // Placeholder for mesh generation logic
                 // You'd generate a mesh based on node positions and connections
            }
        }

        /// <summary>
        /// Optional step to optimize the morphology (e.g., remove redundant nodes, smooth connections).
        /// </summary>
        private void OptimizeMorphology()
        {
            Debug.Log("Running morphology optimization...");
            // Example: Remove unconnected nodes
            List<MorphNode> nodesToRemove = nodes.Where(n => n.Connections.Count == 0 && nodes.Count > settings.initialSeedCount).ToList();
            if(nodesToRemove.Count > 0)
            {
                 Debug.Log($"Removing {nodesToRemove.Count} unconnected nodes.");
                 foreach(var node in nodesToRemove)
                 {
                     nodes.Remove(node);
                     Destroy(node.gameObject);
                 }
            }
        }

        /// <summary>
        /// Calculates various metrics about the generated morphology.
        /// </summary>
        private Dictionary<string, float> CalculateMetrics()
        {
            Dictionary<string, float> metrics = new Dictionary<string, float>();
            metrics["NodeCount"] = nodes.Count;
            metrics["ConnectionCount"] = connections.Count;
            metrics["Density"] = nodes.Count / Mathf.Max(1f, growthZone.size.x * growthZone.size.y * growthZone.size.z); // Volumetric density

            // Add more metrics as needed
            if (nodes.Count > 0)
            {
                Bounds actualBounds = new Bounds(nodes[0].transform.position, Vector3.zero);
                foreach(var node in nodes) { actualBounds.Encapsulate(node.transform.position); }
                metrics["BoundingVolume"] = actualBounds.size.x * actualBounds.size.y * actualBounds.size.z;

                float totalConnectionLength = 0;
                foreach(var conn in connections) 
                { 
                    // Avoid direct property access to prevent ambiguity
                    float length = Vector3.Distance(conn.NodeA.transform.position, conn.NodeB.transform.position);
                    totalConnectionLength += length;
                }
                metrics["AverageConnectionLength"] = connections.Count > 0 ? totalConnectionLength / connections.Count : 0;
            } else {
                 metrics["BoundingVolume"] = 0;
                 metrics["AverageConnectionLength"] = 0;
            }

            return metrics;
        }        /// <summary>
        /// Creates the appropriate growth algorithm based on parameters.
        /// </summary>
        private IGrowthAlgorithm CreateGrowthAlgorithm(MorphologyParameters parameters)
        {
            // Try to use BiotypeBehaviors if available
            BiotypeBehaviors behaviors = GetComponent<BiotypeBehaviors>();
            if (behaviors != null)
            {
                return behaviors.GetGrowthAlgorithm(parameters);
            }

            // Fallback to direct instantiation using dictionary pattern instead of switch
            return GetGrowthAlgorithmByType(parameters.biomorphType);
        }
        
        /// <summary>
        /// Factory method to get growth algorithm by type
        /// </summary>
        private IGrowthAlgorithm GetGrowthAlgorithmByType(MorphologyParameters.BiomorphType type)
        {
            // Using a dictionary approach for cleaner code and better performance
            var algorithmMap = new Dictionary<MorphologyParameters.BiomorphType, IGrowthAlgorithm>
            {
                { MorphologyParameters.BiomorphType.Mold, new MoldGrowthAlgorithm() },
                { MorphologyParameters.BiomorphType.Bone, new BoneGrowthAlgorithm() },
                { MorphologyParameters.BiomorphType.Coral, new CoralGrowthAlgorithm() },
                { MorphologyParameters.BiomorphType.Mycelium, new MyceliumGrowthAlgorithm() },
                { MorphologyParameters.BiomorphType.Custom, new CustomGrowthAlgorithm() }
            };

            if (algorithmMap.TryGetValue(type, out var algorithm))
            {
                return algorithm;
            }
            
            return new MoldGrowthAlgorithm(); // Default fallback
        }
        #endregion
    }
}