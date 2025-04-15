using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Controls how morphology structures adapt to environmental factors and stresses.
/// This system applies adaptive algorithms inspired by biological growth and adaptation.
/// </summary>
public class AdaptationSystem : MonoBehaviour
{
    #region Properties
    [Header("Adaptation Settings")]
    [SerializeField] private float adaptationRate = 0.5f;
    [SerializeField] private float stressThreshold = 0.7f;
    [SerializeField] private float growthThreshold = 0.3f;
    [SerializeField] private int maxNodesPerAdaptation = 10;
    [SerializeField] private float minNodeDistance = 1.0f;
    [SerializeField] private float maxNodeDistance = 5.0f;
    
    [Header("References")]
    [SerializeField] private GameObject nodePrototype;
    [SerializeField] private GameObject connectionPrototype;
    [SerializeField] private Material adaptationMaterial;
    
    // State
    private List<MorphNode> nodes = new List<MorphNode>();
    private List<MorphConnection> connections = new List<MorphConnection>();
    private Dictionary<MorphNode, Vector3> nodeForces = new Dictionary<MorphNode, Vector3>();
    private Dictionary<MorphNode, float> nodeStresses = new Dictionary<MorphNode, float>();
    private MorphologyParameters adaptationParameters;
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the adaptation system with the current morphology
    /// </summary>
    /// <param name="morphNodes">Current nodes in the morphology</param>
    /// <param name="morphConnections">Current connections in the morphology</param>
    /// <param name="parameters">Parameters controlling adaptation</param>
    public void Initialize(List<MorphNode> morphNodes, List<MorphConnection> morphConnections, MorphologyParameters parameters)
    {
        // Store references
        nodes = new List<MorphNode>(morphNodes);
        connections = new List<MorphConnection>(morphConnections);
        adaptationParameters = parameters;
        
        // Clear state
        nodeForces.Clear();
        nodeStresses.Clear();
        
        // Initialize stress maps for each node
        foreach (var node in nodes)
        {
            nodeForces[node] = Vector3.zero;
            nodeStresses[node] = 0f;
        }
        
        Debug.Log($"Adaptation system initialized with {nodes.Count} nodes and {connections.Count} connections");
    }
    
    /// <summary>
    /// Updates the forces acting on each node
    /// </summary>
    /// <param name="forces">Dictionary mapping nodes to forces</param>
    public void UpdateForces(Dictionary<MorphNode, Vector3> forces)
    {
        foreach (var pair in forces)
        {
            if (nodeForces.ContainsKey(pair.Key))
            {
                nodeForces[pair.Key] = pair.Value;
            }
        }
        
        // Update stress levels based on forces
        UpdateStressLevels();
    }
    
    /// <summary>
    /// Performs an adaptation step in response to environmental forces
    /// </summary>
    /// <param name="deltaTime">Time step</param>
    /// <returns>The total amount of adaptation that occurred</returns>
    public float AdaptationStep(float deltaTime)
    {
        // Scale adaptation by time and rate
        float adaptationAmount = deltaTime * adaptationRate;
        
        // Track total adaptation that occurs
        float totalAdaptation = 0f;
        
        // Perform different types of adaptation based on parameters
        switch (adaptationParameters.biomorphType)
        {
            case MorphologyParameters.BiomorphType.Mold:
                totalAdaptation += PerformMoldAdaptation(adaptationAmount);
                break;
                
            case MorphologyParameters.BiomorphType.Bone:
                totalAdaptation += PerformBoneAdaptation(adaptationAmount);
                break;
                
            case MorphologyParameters.BiomorphType.Coral:
                totalAdaptation += PerformCoralAdaptation(adaptationAmount);
                break;
                
            case MorphologyParameters.BiomorphType.Mycelium:
                totalAdaptation += PerformMyceliumAdaptation(adaptationAmount);
                break;
                
            case MorphologyParameters.BiomorphType.Custom:
                totalAdaptation += PerformCustomAdaptation(adaptationAmount);
                break;
        }
        
        return totalAdaptation;
    }
    
    /// <summary>
    /// Gets the current list of nodes after adaptation
    /// </summary>
    public List<MorphNode> GetNodes()
    {
        return nodes;
    }
    
    /// <summary>
    /// Gets the current list of connections after adaptation
    /// </summary>
    public List<MorphConnection> GetConnections()
    {
        return connections;
    }
    
    /// <summary>
    /// Gets the stress level of a specific node
    /// </summary>
    public float GetNodeStress(MorphNode node)
    {
        if (nodeStresses.ContainsKey(node))
        {
            return nodeStresses[node];
        }
        return 0f;
    }
    
    /// <summary>
    /// Gets the average stress level across all nodes
    /// </summary>
    public float GetAverageStress()
    {
        if (nodes.Count == 0) return 0f;
        
        float totalStress = 0f;
        foreach (var node in nodes)
        {
            totalStress += GetNodeStress(node);
        }
        
        return totalStress / nodes.Count;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Updates stress levels for all nodes based on current forces
    /// </summary>
    private void UpdateStressLevels()
    {
        foreach (var node in nodes)
        {
            if (nodeForces.ContainsKey(node))
            {
                // Calculate stress based on force magnitude and connection count
                float forceMagnitude = nodeForces[node].magnitude;
                float connectionFactor = 1.0f / (1.0f + node.ConnectionCount * 0.2f);
                
                // More connections = less stress from the same force
                float stress = forceMagnitude * connectionFactor;
                
                // Smooth stress change
                if (nodeStresses.ContainsKey(node))
                {
                    nodeStresses[node] = Mathf.Lerp(nodeStresses[node], stress, 0.2f);
                }
                else
                {
                    nodeStresses[node] = stress;
                }
                
                // Update node stress
                node.Stress = nodeStresses[node];
            }
        }
    }
    
    /// <summary>
    /// Performs adaptation for mold-like structures
    /// </summary>
    private float PerformMoldAdaptation(float adaptationAmount)
    {
        float totalAdaptation = 0f;
        
        // Identify high-stress nodes that need adaptation
        List<MorphNode> highStressNodes = nodes
            .Where(n => nodeStresses.ContainsKey(n) && nodeStresses[n] > stressThreshold)
            .OrderByDescending(n => nodeStresses[n])
            .ToList();
        
        // For mold, focus on creating new pathways around high-stress areas
        int adaptationsPerformed = 0;
        foreach (var node in highStressNodes)
        {
            if (adaptationsPerformed >= maxNodesPerAdaptation) break;
            
            // Get the force direction
            Vector3 forceDir = Vector3.zero;
            if (nodeForces.ContainsKey(node))
            {
                forceDir = nodeForces[node].normalized;
            }
            
            // Create new nodes to distribute stress
            if (Random.value < adaptationAmount * nodeStresses[node])
            {
                // Create a new node in the opposite direction of the force
                Vector3 growthDir = -forceDir + Random.insideUnitSphere * 0.3f;
                growthDir.Normalize();
                
                // Determine distance based on connection count
                float distance = Mathf.Lerp(minNodeDistance, maxNodeDistance, 
                    1.0f / (1.0f + node.ConnectionCount * 0.3f));
                
                // Create a new position
                Vector3 newPos = node.transform.position + growthDir * distance;
                
                // Check if too close to an existing node
                bool tooClose = false;
                foreach (var existingNode in nodes)
                {
                    if (Vector3.Distance(newPos, existingNode.transform.position) < minNodeDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    // Create the new node
                    MorphNode newNode = CreateNode(newPos);
                    
                    // Create connection to source node
                    CreateConnection(node, newNode);
                    
                    // Create some additional connections to nearby nodes
                    ConnectToNearbyNodes(newNode, adaptationParameters.connectivity);
                    
                    // Count as adaptation
                    totalAdaptation += 1.0f;
                    adaptationsPerformed++;
                }
            }
        }
        
        // Also reinforce existing connections under stress
        foreach (var connection in connections.ToList())
        {
            // Check if both nodes are under stress
            float nodeAStress = GetNodeStress(connection.NodeA);
            float nodeBStress = GetNodeStress(connection.NodeB);
            
            // If connection is under stress, strengthen it
            if (nodeAStress > growthThreshold && nodeBStress > growthThreshold)
            {
                connection.Strength = Mathf.Min(1.0f, connection.Strength + adaptationAmount * 0.5f);
                totalAdaptation += 0.1f;
            }
        }
        
        return totalAdaptation;
    }
    
    /// <summary>
    /// Performs adaptation for bone-like structures
    /// </summary>
    private float PerformBoneAdaptation(float adaptationAmount)
    {
        float totalAdaptation = 0f;
        
        // Bone-like structures focus on thickening connections in high-stress areas
        foreach (var connection in connections)
        {
            // Calculate average stress on the connection
            float stressA = GetNodeStress(connection.NodeA);
            float stressB = GetNodeStress(connection.NodeB);
            float avgStress = (stressA + stressB) * 0.5f;
            
            // Reinforce connections under stress
            if (avgStress > growthThreshold)
            {
                // Increase connection strength (would normally affect thickness)
                float strengthIncrease = adaptationAmount * avgStress;
                connection.Strength = Mathf.Min(1.0f, connection.Strength + strengthIncrease);
                
                totalAdaptation += strengthIncrease;
            }
            else if (avgStress < growthThreshold * 0.3f)
            {
                // Weaken underused connections
                float strengthDecrease = adaptationAmount * 0.1f;
                connection.Strength = Mathf.Max(0.1f, connection.Strength - strengthDecrease);
            }
        }
        
        // Also add cross-connections for triangulation in high-stress areas
        List<MorphNode> highStressNodes = nodes
            .Where(n => nodeStresses.ContainsKey(n) && nodeStresses[n] > stressThreshold * 0.7f)
            .ToList();
        
        int triangulationsAdded = 0;
        foreach (var node in highStressNodes)
        {
            if (triangulationsAdded >= maxNodesPerAdaptation / 2) break;
            
            // Get connected nodes
            List<MorphNode> connectedNodes = node.GetConnectedNodes();
            
            // Check pairs of connected nodes to see if they should be triangulated
            for (int i = 0; i < connectedNodes.Count; i++)
            {
                for (int j = i + 1; j < connectedNodes.Count; j++)
                {
                    // Check if these two nodes are already connected
                    bool alreadyConnected = false;
                    foreach (var conn in connections)
                    {
                        if ((conn.NodeA == connectedNodes[i] && conn.NodeB == connectedNodes[j]) ||
                            (conn.NodeA == connectedNodes[j] && conn.NodeB == connectedNodes[i]))
                        {
                            alreadyConnected = true;
                            break;
                        }
                    }
                    
                    // If not connected and under stress, add a triangulation connection
                    if (!alreadyConnected && 
                        GetNodeStress(connectedNodes[i]) > growthThreshold &&
                        GetNodeStress(connectedNodes[j]) > growthThreshold)
                    {
                        if (Random.value < adaptationAmount * adaptationParameters.connectivity)
                        {
                            CreateConnection(connectedNodes[i], connectedNodes[j]);
                            triangulationsAdded++;
                            totalAdaptation += 0.5f;
                        }
                    }
                }
            }
        }
        
        return totalAdaptation;
    }
    
    /// <summary>
    /// Performs adaptation for coral-like structures
    /// </summary>
    private float PerformCoralAdaptation(float adaptationAmount)
    {
        float totalAdaptation = 0f;
        
        // Coral tends to grow upward and spread outward, adapting to currents
        
        // Identify potential growth nodes - prefer tips (nodes with only 1 connection)
        List<MorphNode> growthNodes = nodes
            .Where(n => n.ConnectionCount <= 1 || 
                  (nodeStresses.ContainsKey(n) && nodeStresses[n] > growthThreshold))
            .OrderByDescending(n => n.transform.position.y) // Favor upward growth
            .Take(maxNodesPerAdaptation * 2)
            .ToList();
        
        // Shuffle the list for variety
        ShuffleList(growthNodes);
        
        // Grow from selected nodes
        int adaptationsPerformed = 0;
        foreach (var node in growthNodes)
        {
            if (adaptationsPerformed >= maxNodesPerAdaptation) break;
            
            if (Random.value < adaptationAmount * (1.0f + (node.ConnectionCount <= 1 ? 0.5f : 0f)))
            {
                // Determine growth direction with bias upward
                Vector3 growthDir = Vector3.up * 0.5f + Random.insideUnitSphere * 0.5f;
                
                // If the node is under stress, grow away from the force
                if (nodeForces.ContainsKey(node) && nodeForces[node].magnitude > 0.1f)
                {
                    Vector3 forceDir = nodeForces[node].normalized;
                    growthDir = (growthDir - forceDir * 0.5f).normalized;
                }
                
                // Determine distance based on complexity
                float distance = Mathf.Lerp(minNodeDistance, maxNodeDistance, adaptationParameters.complexity);
                
                // Create a new position
                Vector3 newPos = node.transform.position + growthDir * distance;
                
                // Check if too close to an existing node
                bool tooClose = false;
                foreach (var existingNode in nodes)
                {
                    if (Vector3.Distance(newPos, existingNode.transform.position) < minNodeDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    // Create the new node
                    MorphNode newNode = CreateNode(newPos);
                    
                    // Create connection to source node
                    CreateConnection(node, newNode);
                    
                    // Sometimes create plate-like structures at tips
                    if (node.ConnectionCount <= 1 && Random.value < 0.3f)
                    {
                        // Add additional nodes in a plate formation
                        int plateNodes = Random.Range(2, 5);
                        Vector3 plateNormal = Vector3.up;
                        
                        for (int i = 0; i < plateNodes; i++)
                        {
                            // Create points around the new node in a plane
                            float angle = Random.Range(0, Mathf.PI * 2);
                            float plateRadius = Random.Range(minNodeDistance, maxNodeDistance * 0.7f);
                            
                            Vector3 plateDir = (Quaternion.AngleAxis(angle * Mathf.Rad2Deg, plateNormal) * Vector3.right).normalized;
                            Vector3 platePos = newNode.transform.position + plateDir * plateRadius;
                            
                            MorphNode plateNode = CreateNode(platePos);
                            CreateConnection(newNode, plateNode);
                            
                            // Connect plate nodes to form a structure
                            if (i > 0)
                            {
                                CreateConnection(nodes[nodes.Count - 2], plateNode);
                            }
                        }
                        
                        // Connect the last and first plate nodes to close the loop
                        if (plateNodes > 2)
                        {
                            CreateConnection(nodes[nodes.Count - plateNodes], nodes[nodes.Count - 1]);
                        }
                        
                        totalAdaptation += plateNodes;
                    }
                    else
                    {
                        // Connect to nearby existing nodes 
                        ConnectToNearbyNodes(newNode, adaptationParameters.connectivity * 0.5f);
                    }
                    
                    totalAdaptation += 1.0f;
                    adaptationsPerformed++;
                }
            }
        }
        
        return totalAdaptation;
    }
    
    /// <summary>
    /// Performs adaptation for mycelium-like structures
    /// </summary>
    private float PerformMyceliumAdaptation(float adaptationAmount)
    {
        float totalAdaptation = 0f;
        
        // Mycelium adapts by creating extensive branching networks,
        // focusing on exploration and resource gathering
        
        // Identify potential growth nodes - prefer tips but also allow some internal branching
        List<MorphNode> growthNodes = nodes
            .Where(n => n.ConnectionCount <= 2 || 
                  (nodeStresses.ContainsKey(n) && nodeStresses[n] > growthThreshold))
            .OrderByDescending(n => n.ConnectionCount <= 2 ? 1 : 0) // Favor tips
            .Take(maxNodesPerAdaptation * 3)
            .ToList();
        
        // Shuffle the list for variety
        ShuffleList(growthNodes);
        
        // Grow from selected nodes
        int adaptationsPerformed = 0;
        foreach (var node in growthNodes)
        {
            if (adaptationsPerformed >= maxNodesPerAdaptation) break;
            
            // Higher growth probability for tips
            float growthProb = adaptationAmount * (1.0f + (node.ConnectionCount <= 1 ? 0.8f : 0));
            
            if (Random.value < growthProb)
            {
                // Determine direction with some randomness
                Vector3 growthDir = Random.onUnitSphere;
                
                // If the node is under stress, grow in a direction influenced by the force
                if (nodeForces.ContainsKey(node) && nodeForces[node].magnitude > 0.1f)
                {
                    Vector3 forceDir = nodeForces[node].normalized;
                    
                    // For mycelium, sometimes grow toward resources (force)
                    if (Random.value < 0.3f)
                    {
                        growthDir = Vector3.Lerp(growthDir, forceDir, 0.7f).normalized;
                    }
                    else
                    {
                        growthDir = Vector3.Lerp(growthDir, -forceDir, 0.4f).normalized;
                    }
                }
                
                // Determine distance - mycelium has more varied branch lengths
                float distance = Mathf.Lerp(minNodeDistance, maxNodeDistance, 
                    Random.value * adaptationParameters.complexity);
                
                // Create a new position
                Vector3 newPos = node.transform.position + growthDir * distance;
                
                // Check if too close to an existing node
                bool tooClose = false;
                foreach (var existingNode in nodes)
                {
                    if (Vector3.Distance(newPos, existingNode.transform.position) < minNodeDistance * 0.8f)
                    {
                        tooClose = true;
                        break;
                    }
                }
                
                if (!tooClose)
                {
                    // Create the new node
                    MorphNode newNode = CreateNode(newPos);
                    
                    // Create connection to source node
                    CreateConnection(node, newNode);
                    
                    // Mycelium has thinner, more numerous connections
                    ConnectToNearbyNodes(newNode, adaptationParameters.connectivity * 0.7f);
                    
                    // Sometimes add multiple branches from a single point
                    if (Random.value < adaptationParameters.complexity * 0.4f)
                    {
                        int branches = Random.Range(1, 3);
                        for (int i = 0; i < branches; i++)
                        {
                            Vector3 branchDir = (growthDir + Random.insideUnitSphere * 0.6f).normalized;
                            float branchDistance = distance * Random.Range(0.5f, 0.9f);
                            Vector3 branchPos = node.transform.position + branchDir * branchDistance;
                            
                            // Check if too close to other nodes
                            bool branchTooClose = false;
                            foreach (var existingNode in nodes)
                            {
                                if (Vector3.Distance(branchPos, existingNode.transform.position) < minNodeDistance * 0.8f)
                                {
                                    branchTooClose = true;
                                    break;
                                }
                            }
                            
                            if (!branchTooClose)
                            {
                                MorphNode branchNode = CreateNode(branchPos);
                                CreateConnection(node, branchNode);
                                totalAdaptation += 0.5f;
                            }
                        }
                    }
                    
                    totalAdaptation += 1.0f;
                    adaptationsPerformed++;
                }
            }
        }
        
        // Mycelium also creates long-distance connections occasionally
        if (Random.value < adaptationAmount * adaptationParameters.connectivity && nodes.Count > 10)
        {
            int longConnections = Mathf.Min(3, maxNodesPerAdaptation - adaptationsPerformed);
            for (int i = 0; i < longConnections; i++)
            {
                // Select two random nodes that aren't already connected
                MorphNode nodeA = nodes[Random.Range(0, nodes.Count)];
                MorphNode nodeB = null;
                
                // Find a node that's not too close and not already connected
                int attempts = 0;
                while (nodeB == null && attempts < 10)
                {
                    MorphNode candidate = nodes[Random.Range(0, nodes.Count)];
                    float distance = Vector3.Distance(nodeA.transform.position, candidate.transform.position);
                    
                    // Check if already connected
                    bool alreadyConnected = false;
                    foreach (var conn in connections)
                    {
                        if ((conn.NodeA == nodeA && conn.NodeB == candidate) ||
                            (conn.NodeA == candidate && conn.NodeB == nodeA))
                        {
                            alreadyConnected = true;
                            break;
                        }
                    }
                    
                    if (!alreadyConnected && distance > minNodeDistance * 3 && distance < maxNodeDistance * 3)
                    {
                        nodeB = candidate;
                    }
                    
                    attempts++;
                }
                
                if (nodeB != null)
                {
                    CreateConnection(nodeA, nodeB);
                    totalAdaptation += 0.3f;
                }
            }
        }
        
        return totalAdaptation;
    }
    
    /// <summary>
    /// Performs adaptation for custom structures
    /// </summary>
    private float PerformCustomAdaptation(float adaptationAmount)
    {
        // Custom adaptation combines aspects of the other types
        float moldAdaptation = PerformMoldAdaptation(adaptationAmount * 0.4f);
        float boneAdaptation = PerformBoneAdaptation(adaptationAmount * 0.3f);
        float coralAdaptation = PerformCoralAdaptation(adaptationAmount * 0.2f);
        float myceliumAdaptation = PerformMyceliumAdaptation(adaptationAmount * 0.1f);
        
        return moldAdaptation + boneAdaptation + coralAdaptation + myceliumAdaptation;
    }
    
    /// <summary>
    /// Creates a new node at the specified position
    /// </summary>
    private MorphNode CreateNode(Vector3 position)
    {
        // Create game object
        GameObject nodeObj = Instantiate(nodePrototype, position, Quaternion.identity);
        nodeObj.name = $"Node_{nodes.Count}_Adapted";
        nodeObj.transform.parent = transform;
        
        // Add component
        MorphNode node = nodeObj.GetComponent<MorphNode>();
        if (node == null)
        {
            node = nodeObj.AddComponent<MorphNode>();
        }
        
        // Initialize
        node.Initialize(adaptationMaterial);
        
        // Add to lists
        nodes.Add(node);
        nodeForces[node] = Vector3.zero;
        nodeStresses[node] = 0f;
        
        return node;
    }
    
    /// <summary>
    /// Creates a connection between two nodes
    /// </summary>
    private MorphConnection CreateConnection(MorphNode nodeA, MorphNode nodeB)
    {
        // Check if connection already exists
        foreach (var conn in connections)
        {
            if ((conn.NodeA == nodeA && conn.NodeB == nodeB) ||
                (conn.NodeA == nodeB && conn.NodeB == nodeA))
            {
                return conn;
            }
        }
        
        // Create game object
        GameObject connectionObj = Instantiate(connectionPrototype);
        connectionObj.name = $"Connection_{connections.Count}_Adapted";
        connectionObj.transform.parent = transform;
        
        // Add component
        MorphConnection connection = connectionObj.GetComponent<MorphConnection>();
        if (connection == null)
        {
            connection = connectionObj.AddComponent<MorphConnection>();
        }
        
        // Initialize
        connection.Initialize(nodeA, nodeB, adaptationMaterial);
        
        // Add to list and register with nodes
        connections.Add(connection);
        nodeA.AddConnection(connection);
        nodeB.AddConnection(connection);
        
        return connection;
    }
    
    /// <summary>
    /// Connects a node to nearby existing nodes
    /// </summary>
    private void ConnectToNearbyNodes(MorphNode node, float connectionProbability)
    {
        // Find nearby nodes
        List<MorphNode> nearbyNodes = new List<MorphNode>();
        
        foreach (var otherNode in nodes)
        {
            if (otherNode == node) continue;
            
            float distance = Vector3.Distance(node.transform.position, otherNode.transform.position);
            
            // Check if within range
            if (distance >= minNodeDistance && distance <= maxNodeDistance)
            {
                nearbyNodes.Add(otherNode);
            }
        }
        
        // Connect to some nearby nodes based on connection probability
        foreach (var nearbyNode in nearbyNodes)
        {
            if (Random.value < connectionProbability)
            {
                CreateConnection(node, nearbyNode);
            }
        }
    }
    
    /// <summary>
    /// Shuffles a list
    /// </summary>
    private void ShuffleList<T>(List<T> list)
    {
        int n = list.Count;
        while (n > 1)
        {
            n--;
            int k = Random.Range(0, n + 1);
            T value = list[k];
            list[k] = list[n];
            list[n] = value;
        }
    }
    #endregion
}