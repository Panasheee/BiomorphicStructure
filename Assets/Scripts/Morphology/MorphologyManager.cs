using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BiomorphicSim.Core;

namespace BiomorphicSim.Morphology
{
    /// <summary>
    /// Manages the biomorphic structure simulation, including node creation and connections.
    /// Acts as the central controller for the growth algorithms and morphology adaptation.
    /// </summary>
    public class MorphologyManager : MonoBehaviour
    {
        [Header("Prefabs")]
        [SerializeField] private GameObject nodePrefab;
        [SerializeField] private GameObject connectionPrefab;
        
        [Header("Growth Settings")]
        [SerializeField] private float growthSpeed = 1.0f;
        [SerializeField] private float nodeDistance = 2.0f;
        [SerializeField] private int maxNodes = 500;
        [SerializeField] private float connectionRadius = 3.0f;
        [SerializeField] private int maxConnectionsPerNode = 5;
        
        [Header("Physics Settings")]
        [SerializeField] private float nodeRepulsionForce = 1.0f;
        [SerializeField] private float connectionStrength = 5.0f;
        [SerializeField] private float damping = 0.8f;
        
        // Structure to hold node data
        [System.Serializable]
        public class BiomorphNode
        {
            public GameObject gameObject;
            public List<BiomorphConnection> connections = new List<BiomorphConnection>();
            public Vector3 velocity = Vector3.zero;
            public float mass = 1.0f;
            public float energy = 1.0f;
            public bool isRoot = false;
            
            // Constructor for new nodes
            public BiomorphNode(GameObject nodeObject, bool root = false)
            {
                gameObject = nodeObject;
                isRoot = root;
                
                // Root nodes have more energy
                if (isRoot)
                    energy = 5.0f;
            }
        }
        
        // Structure to hold connection data
        [System.Serializable]
        public class BiomorphConnection
        {
            public GameObject gameObject;
            public BiomorphNode nodeA;
            public BiomorphNode nodeB;
            public float strength;
            public float initialDistance;
            
            // Constructor
            public BiomorphConnection(GameObject connectionObject, BiomorphNode a, BiomorphNode b, float connectionStrength)
            {
                gameObject = connectionObject;
                nodeA = a;
                nodeB = b;
                strength = connectionStrength;
                initialDistance = Vector3.Distance(a.gameObject.transform.position, b.gameObject.transform.position);
            }
            
            // Update the connection's position and scale to match the two nodes
            public void UpdateVisual()
            {
                if (gameObject == null || nodeA == null || nodeB == null)
                    return;
                    
                // Position at midpoint
                Vector3 midPoint = (nodeA.gameObject.transform.position + nodeB.gameObject.transform.position) / 2f;
                gameObject.transform.position = midPoint;
                
                // Scale to match distance
                float distance = Vector3.Distance(nodeA.gameObject.transform.position, nodeB.gameObject.transform.position);
                gameObject.transform.localScale = new Vector3(0.2f, distance / 2f, 0.2f);
                
                // Rotate to look from one node to another
                gameObject.transform.LookAt(nodeB.gameObject.transform);
                gameObject.transform.Rotate(90, 0, 0);
            }
        }
        
        // Internal data
        private List<BiomorphNode> nodes = new List<BiomorphNode>();
        private Transform nodesParent;
        private Transform connectionsParent;
        private bool isGrowing = false;
        private bool isSimulating = false;
        private float growthTimer = 0f;
        private float growthInterval = 1.0f;
        
        // Growth algorithm
        private IGrowthAlgorithm currentGrowthAlgorithm;
        
        // Stored references
        private SiteGenerator siteGenerator;
        
        public void Initialize()
        {
            Debug.Log("Initializing Morphology Manager...");
            
            // Create parent objects for organization
            if (nodesParent == null)
            {
                nodesParent = new GameObject("Nodes").transform;
                nodesParent.SetParent(transform);
            }
            
            if (connectionsParent == null)
            {
                connectionsParent = new GameObject("Connections").transform;
                connectionsParent.SetParent(transform);
            }
            
            // Set up initial growth algorithm
            currentGrowthAlgorithm = new BranchingGrowthAlgorithm();
            
            // Get reference to the site generator
            siteGenerator = FindObjectOfType<SiteGenerator>();
        }
        
        public void StartGrowth()
        {
            if (nodes.Count == 0)
            {
                // Create an initial root node
                CreateInitialRootNode();
            }
            
            isGrowing = true;
            isSimulating = true;
            
            // Start the physics simulation
            StartCoroutine(PhysicsSimulation());
        }
        
        public void PauseGrowth()
        {
            isGrowing = false;
            isSimulating = false;
            
            // Stop coroutines
            StopAllCoroutines();
        }
        
        public void ResetGrowth()
        {
            PauseGrowth();
            
            // Destroy all nodes and connections
            foreach (var node in nodes)
            {
                if (node.gameObject != null)
                    Destroy(node.gameObject);
                
                foreach (var connection in node.connections)
                {
                    if (connection.gameObject != null)
                        Destroy(connection.gameObject);
                }
            }
            
            nodes.Clear();
            
            Debug.Log("Growth reset. All nodes and connections removed.");
        }
        
        public void SetGrowthAlgorithm(IGrowthAlgorithm algorithm)
        {
            currentGrowthAlgorithm = algorithm;
            Debug.Log($"Growth algorithm set to {algorithm.GetType().Name}");
        }
        
        public void SetGrowthSpeed(float speed)
        {
            growthSpeed = Mathf.Clamp(speed, 0.1f, 10.0f);
            growthInterval = 1.0f / growthSpeed;
            Debug.Log($"Growth speed set to {growthSpeed}x");
        }
        
        private void Update()
        {
            if (isGrowing && nodes.Count < maxNodes)
            {
                // Increment growth timer
                growthTimer += Time.deltaTime;
                
                // Check if it's time for new growth
                if (growthTimer >= growthInterval)
                {
                    growthTimer = 0f;
                    
                    // Execute growth step
                    ExecuteGrowthStep();
                }
            }
        }
        
        private void CreateInitialRootNode()
        {
            // Use site generator to get a point on the terrain if available
            Vector3 position;
            if (siteGenerator != null)
            {
                position = siteGenerator.GetRandomPointOnTerrain();
            }
            else
            {
                position = new Vector3(0, 0, 0);
            }
            
            // Create the root node
            GameObject rootNodeObject = Instantiate(nodePrefab, position, Quaternion.identity, nodesParent);
            rootNodeObject.name = "RootNode_0";
            
            // Make it slightly larger to indicate it's a root
            rootNodeObject.transform.localScale *= 1.5f;
            
            // Create data structure
            BiomorphNode rootNode = new BiomorphNode(rootNodeObject, true);
            nodes.Add(rootNode);
            
            Debug.Log("Created initial root node at " + position);
        }
        
        private void ExecuteGrowthStep()
        {
            if (currentGrowthAlgorithm == null || nodes.Count == 0)
                return;
                
            // Use the current growth algorithm to determine where to grow next
            GrowthPoint growthPoint = currentGrowthAlgorithm.DetermineNextGrowthPoint(nodes);
            
            if (growthPoint.parentNode != null)
            {
                // Create the new node
                GameObject newNodeObject = Instantiate(nodePrefab, growthPoint.position, Quaternion.identity, nodesParent);
                newNodeObject.name = "Node_" + nodes.Count;
                
                // Create data structure
                BiomorphNode newNode = new BiomorphNode(newNodeObject);
                nodes.Add(newNode);
                
                // Connect to parent
                CreateConnection(growthPoint.parentNode, newNode);
                
                // Find nearby nodes for additional connections
                FindAndConnectNearbyNodes(newNode);
            }
        }
        
        private void CreateConnection(BiomorphNode nodeA, BiomorphNode nodeB)
        {
            // Check if already connected
            foreach (var conn in nodeA.connections)
            {
                if (conn.nodeA == nodeB || conn.nodeB == nodeB)
                    return;
            }
            
            // Instantiate connection prefab
            GameObject connectionObject = Instantiate(connectionPrefab, Vector3.zero, Quaternion.identity, connectionsParent);
            connectionObject.name = "Connection_" + nodeA.gameObject.name + "_" + nodeB.gameObject.name;
            
            // Create connection data structure
            BiomorphConnection connection = new BiomorphConnection(connectionObject, nodeA, nodeB, connectionStrength);
            
            // Add connection to both nodes
            nodeA.connections.Add(connection);
            nodeB.connections.Add(connection);
            
            // Update visuals
            connection.UpdateVisual();
        }
        
        private void FindAndConnectNearbyNodes(BiomorphNode node)
        {
            if (node.connections.Count >= maxConnectionsPerNode)
                return;
                
            Vector3 position = node.gameObject.transform.position;
            
            foreach (var otherNode in nodes)
            {
                // Skip if it's the same node
                if (otherNode == node)
                    continue;
                    
                // Skip if max connections reached
                if (node.connections.Count >= maxConnectionsPerNode)
                    break;
                    
                // Skip if other node has max connections
                if (otherNode.connections.Count >= maxConnectionsPerNode)
                    continue;
                    
                // Calculate distance
                float distance = Vector3.Distance(position, otherNode.gameObject.transform.position);
                
                // If close enough and not already connected
                if (distance <= connectionRadius)
                {
                    bool alreadyConnected = false;
                    
                    foreach (var conn in node.connections)
                    {
                        if (conn.nodeA == otherNode || conn.nodeB == otherNode)
                        {
                            alreadyConnected = true;
                            break;
                        }
                    }
                    
                    if (!alreadyConnected)
                    {
                        CreateConnection(node, otherNode);
                    }
                }
            }
        }
        
        private IEnumerator PhysicsSimulation()
        {
            while (isSimulating)
            {
                SimulatePhysicsStep();
                yield return null;
            }
        }
        
        private void SimulatePhysicsStep()
        {
            // Apply forces and update positions for all nodes
            foreach (var node in nodes)
            {
                if (node.isRoot)
                    continue; // Skip root nodes, keep them fixed
                    
                Vector3 totalForce = Vector3.zero;
                
                // Force from connections (spring forces)
                foreach (var connection in node.connections)
                {
                    BiomorphNode otherNode = connection.nodeA == node ? connection.nodeB : connection.nodeA;
                    
                    Vector3 direction = otherNode.gameObject.transform.position - node.gameObject.transform.position;
                    float distance = direction.magnitude;
                    direction.Normalize();
                    
                    // Spring force: F = k * (distance - rest_length)
                    float springForce = connection.strength * (distance - connection.initialDistance);
                    totalForce += direction * springForce;
                }
                
                // Repulsion forces from other nodes
                foreach (var otherNode in nodes)
                {
                    if (otherNode == node)
                        continue;
                        
                    Vector3 direction = node.gameObject.transform.position - otherNode.gameObject.transform.position;
                    float distance = direction.magnitude;
                    
                    if (distance < nodeDistance * 2)
                    {
                        // Avoid division by zero
                        if (distance < 0.1f)
                            distance = 0.1f;
                            
                        direction.Normalize();
                        
                        // Repulsion force: F = k / r²
                        float repulsionForce = nodeRepulsionForce / (distance * distance);
                        totalForce += direction * repulsionForce;
                    }
                }
                
                // Apply gravity
                totalForce += Physics.gravity * node.mass;
                
                // Update velocity using Verlet integration
                node.velocity = node.velocity * damping + totalForce * Time.deltaTime;
                
                // Update position
                node.gameObject.transform.position += node.velocity * Time.deltaTime;
            }
            
            // Update connection visuals
            foreach (var node in nodes)
            {
                foreach (var connection in node.connections)
                {
                    connection.UpdateVisual();
                }
            }
        }
        
        public void ApplyExternalForce(Vector3 center, float radius, Vector3 force)
        {
            foreach (var node in nodes)
            {
                float distance = Vector3.Distance(node.gameObject.transform.position, center);
                
                if (distance <= radius)
                {
                    // Calculate force based on distance (stronger at center)
                    float forceMagnitude = 1f - (distance / radius);
                    
                    // Apply force to node velocity
                    node.velocity += force * forceMagnitude;
                }
            }
        }
        
        public int GetNodeCount()
        {
            return nodes.Count;
        }
        
        public void Cleanup()
        {
            PauseGrowth();
        }
    }
    
    // Interface for growth algorithms
    public interface IGrowthAlgorithm
    {
        GrowthPoint DetermineNextGrowthPoint(List<MorphologyManager.BiomorphNode> currentNodes);
    }
    
    // Data structure for growth points
    public struct GrowthPoint
    {
        public Vector3 position;
        public MorphologyManager.BiomorphNode parentNode;
    }
    
    // Basic branching growth algorithm implementation
    public class BranchingGrowthAlgorithm : IGrowthAlgorithm
    {
        public GrowthPoint DetermineNextGrowthPoint(List<MorphologyManager.BiomorphNode> currentNodes)
        {
            GrowthPoint result = new GrowthPoint();
            
            // Select a random node with energy
            List<MorphologyManager.BiomorphNode> eligibleNodes = new List<MorphologyManager.BiomorphNode>();
            
            foreach (var node in currentNodes)
            {
                if (node.energy > 0 && node.connections.Count < 5)
                {
                    eligibleNodes.Add(node);
                }
            }
            
            if (eligibleNodes.Count > 0)
            {
                // Pick a random eligible node
                int randomIndex = Random.Range(0, eligibleNodes.Count);
                MorphologyManager.BiomorphNode selectedNode = eligibleNodes[randomIndex];
                
                // Determine growth direction
                Vector3 growthDirection = Random.onUnitSphere;
                
                // If it has connections, prefer growing in the average direction
                if (selectedNode.connections.Count > 0)
                {
                    Vector3 averageDirection = Vector3.zero;
                    
                    foreach (var connection in selectedNode.connections)
                    {
                        MorphologyManager.BiomorphNode otherNode = connection.nodeA == selectedNode ? connection.nodeB : connection.nodeA;
                        Vector3 direction = otherNode.gameObject.transform.position - selectedNode.gameObject.transform.position;
                        averageDirection += direction.normalized;
                    }
                    
                    if (averageDirection.magnitude > 0)
                    {
                        averageDirection.Normalize();
                        
                        // Mix random direction with average direction
                        growthDirection = Vector3.Lerp(growthDirection, averageDirection, 0.5f);
                        growthDirection.Normalize();
                    }
                }
                
                // Calculate new position
                Vector3 nodePosition = selectedNode.gameObject.transform.position;
                float growthDistance = Random.Range(1.5f, 2.5f);
                Vector3 newPosition = nodePosition + growthDirection * growthDistance;
                
                // Decrease energy of the parent node
                selectedNode.energy -= 0.2f;
                
                // Set result
                result.position = newPosition;
                result.parentNode = selectedNode;
            }
            
            return result;
        }
    }
}