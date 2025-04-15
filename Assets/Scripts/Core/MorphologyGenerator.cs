using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using BiomorphicSim.Core; // Ensure this namespace is used

namespace BiomorphicSim.Core // Keep namespace if originally present, otherwise adjust
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
        // private MarchingCubes meshGenerator; // Assuming MarchingCubes implementation exists/will be added

        #endregion

        #region Properties
        public float GenerationProgress => generationProgress;
        public bool IsGenerating => isGenerating;
        #endregion

        #region Unity Methods
        private void Awake()
        {
            // Initialize container
            morphologyContainer = new GameObject("MorphologyContainer");
            morphologyContainer.transform.SetParent(this.transform);

            // Initialize mesh components if generating a single combined mesh
            combinedMeshFilter = morphologyContainer.AddComponent<MeshFilter>();
            combinedMeshRenderer = morphologyContainer.AddComponent<MeshRenderer>();
            combinedMeshRenderer.material = growthMaterial; // Assign a default material

            // meshGenerator = new MarchingCubes(...); // Initialize Marching Cubes if used

            if (uiManager == null)
                uiManager = FindFirstObjectByType<UIManager>(); // Example: Find UIManager if not set
        }

        private void Update()
        {
            // Optional: Update visual feedback during generation
            if (isGenerating && uiManager != null)
            {
                 // Assuming UIManager has a method like UpdateGenerationProgress
                 // uiManager.UpdateGenerationProgress(generationProgress); 
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
            newNode.Initialize($"Node_{nodes.Count}"); // Assuming MorphNode has Initialize
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
            // Use connectionMaterial or derive from settings/nodes
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
            // float volume = growthZone.size.x * growthZone.size.y * growthZone.size.z; // Less reliable for complex zones
            int targetNodeCount = Mathf.Clamp(Mathf.RoundToInt(settings.maxNodes * parameters.density), settings.initialSeedCount, settings.maxNodes);

            float startTime = Time.time;
            float maxDuration = 60f; // Add a safety break time

            // Main growth loop
            while (nodes.Count < targetNodeCount && isGenerating && (Time.time - startTime) < maxDuration)
            {
                // --- Growth Step Calculation ---
                // Option 1: Simple loop calling CalculateGrowth multiple times per frame
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

                // Option 2: Algorithm's GrowStep (if it modifies nodes/connections directly)
                // growthAlgorithm.GrowStep(nodes, connections, growthZone, parameters); 

                if(nodesAddedThisFrame > 0)
                {
                    UpdateMeshes(); // Update mesh only if something changed
                }

                generationProgress = (float)nodes.Count / targetNodeCount;
                
                // Update UI progress (consider doing this less frequently)
                if (uiManager != null)
                {
                    // uiManager.UpdateGenerationProgress(generationProgress);
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
            
            // Notify Manager
            if (MorphologyManager.Instance != null)
            {
                MorphologyManager.Instance.OnMorphologyGenerationComplete(finalData);
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
            Debug.Log($"Running global connection pass (Probability: {probability})...");
            int added = 0;
            float maxDistSq = settings.nodeMaxDistance * settings.nodeMaxDistance;
            float minDistSq = settings.nodeMinDistance * settings.nodeMinDistance;

            // O(N^2) - potentially slow for large N
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
             Debug.Log($"Added {added} connections in global pass.");
        }


        /// <summary>
        /// Updates the visual representation (individual objects or combined mesh).
        /// </summary>
        private void UpdateMeshes()
        {
            // Option 1: If using individual GameObjects for nodes/connections
            // Their visuals might update automatically or via MorphNode/MorphConnection scripts.

            // Option 2: If generating a combined mesh (e.g., Marching Cubes or metaballs)
            // This is where you'd call the mesh generation logic.
            // Example placeholder:
            if (combinedMeshFilter != null /* && meshGenerator != null */)
            {
                 // List<Vector3> points = nodes.Select(n => n.transform.position).ToList();
                 // float isoLevel = 0.5f; // Example
                 // Mesh newMesh = meshGenerator.GenerateMesh(points, isoLevel); 
                 // combinedMeshFilter.mesh = newMesh; // Assign the generated mesh
                 // Debug.Log("Combined mesh updated (Placeholder).");
            }
             else
             {
                 // Fallback or handle case where mesh components aren't ready
             }
        }

        /// <summary>
        /// Optional step to optimize the morphology (e.g., remove redundant nodes, smooth connections).
        /// </summary>
        private void OptimizeMorphology()
        {
            Debug.Log("Running morphology optimization (Placeholder)...");
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

            // Example: Merge very close nodes (more complex)

            // Example: Smooth connection paths (requires geometry manipulation)
        }

        /// <summary>
        /// Exports the current state of the morphology into a data structure.
        /// </summary>
        private MorphologyData ExportMorphologyData()
        {
            MorphologyData data = new MorphologyData
            {
                morphologyId = $"Morph_{System.DateTime.Now:yyyyMMdd_HHmmss}",
                nodePositions = nodes.Select(n => n.transform.position).ToList(),
                connections = new List<int[]>(),
                metrics = CalculateMetrics(),
                parametersUsed = currentParameters, // Store the parameters used
                generationTime = System.DateTime.Now
            };

            // Create connection index pairs
            Dictionary<MorphNode, int> nodeIndexMap = nodes.Select((node, index) => new { node, index })
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

        /// <summary>
        /// Calculates various metrics about the generated morphology.
        /// </summary>
        private Dictionary<string, float> CalculateMetrics()
        {
            Dictionary<string, float> metrics = new Dictionary<string, float>();
            metrics["NodeCount"] = nodes.Count;
            metrics["ConnectionCount"] = connections.Count;
            metrics["Density"] = nodes.Count / Mathf.Max(1f, growthZone.size.x * growthZone.size.y * growthZone.size.z); // Volumetric density

            // Add more metrics: Bounding box volume, surface area (if mesh exists), average connection length, etc.
            if (nodes.Count > 0)
            {
                Bounds actualBounds = new Bounds(nodes[0].transform.position, Vector3.zero);
                foreach(var node in nodes) { actualBounds.Encapsulate(node.transform.position); }
                metrics["BoundingVolume"] = actualBounds.size.x * actualBounds.size.y * actualBounds.size.z;

                float totalLength = 0;
                 foreach(var conn in connections) { totalLength += conn.Length; }
                 metrics["AverageConnectionLength"] = connections.Count > 0 ? totalLength / connections.Count : 0;
            } else {
                 metrics["BoundingVolume"] = 0;
                 metrics["AverageConnectionLength"] = 0;
            }


            return metrics;
        }

        /// <summary>
        /// Creates the appropriate growth algorithm based on parameters.
        /// </summary>
        private IGrowthAlgorithm CreateGrowthAlgorithm(MorphologyParameters parameters)
        {
            // Use BiotypeBehaviors if available (preferred)
            BiotypeBehaviors behaviors = GetComponent<BiotypeBehaviors>(); // Or find it if it's elsewhere
            if (behaviors != null)
            {
                return behaviors.GetGrowthAlgorithm(parameters);
            }

            // Fallback to potentially missing or simplified direct instantiation (using detailed names)
            Debug.LogWarning("BiotypeBehaviors component not found. Falling back to direct algorithm instantiation. Ensure BiotypeBehaviors.cs exists and defines the detailed algorithms.");
            switch (parameters.biomorphType)
            {
                case MorphologyParameters.BiomorphType.Mold:
                    // Use the detailed version defined in BiotypeBehaviors.cs (assuming it exists globally or is accessible)
                    // Need to decide how parameters are passed if BiotypeBehaviors isn't used.
                    // return new DetailedMoldGrowthAlgorithm(/* constructor parameters if needed */); 
                    return new MoldGrowthAlgorithm(); // Simple fallback
                case MorphologyParameters.BiomorphType.Bone:
                    // return new DetailedBoneGrowthAlgorithm(/* constructor parameters if needed */);
                     return new BoneGrowthAlgorithm(); // Simple fallback
                case MorphologyParameters.BiomorphType.Coral:
                    // return new DetailedCoralGrowthAlgorithm(/* constructor parameters if needed */);
                     return new CoralGrowthAlgorithm(); // Simple fallback
                case MorphologyParameters.BiomorphType.Mycelium:
                    // return new DetailedMyceliumGrowthAlgorithm(/* constructor parameters if needed */);
                     return new MyceliumGrowthAlgorithm(); // Simple fallback
                case MorphologyParameters.BiomorphType.Custom:
                    // return new DetailedCustomGrowthAlgorithm(/* constructor parameters if needed */);
                     return new CustomGrowthAlgorithm(); // Simple fallback
                // Handle newly added BiomorphTypes if needed
                case MorphologyParameters.BiomorphType.Organic:
                case MorphologyParameters.BiomorphType.Crystalline:
                case MorphologyParameters.BiomorphType.Fungal:
                case MorphologyParameters.BiomorphType.Hybrid:
                     Debug.LogWarning($"Fallback algorithm not defined for {parameters.biomorphType}. Using Mold.");
                     return new MoldGrowthAlgorithm(); // Default fallback
                default:
                    Debug.LogWarning($"Unknown biomorph type: {parameters.biomorphType}. Using Mold algorithm.");
                    return new MoldGrowthAlgorithm(); // Default fallback
            }
        }
        #endregion
    }

    // Removed GrowthResult, GrowthInfluenceMap, MorphologyData, MorphologyParameters, MorphologySettings definitions from here.
    // Removed IGrowthAlgorithm interface and basic implementations (MoldGrowthAlgorithm, etc.) from here.
} // End of namespace BiomorphicSim.Core