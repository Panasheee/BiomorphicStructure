using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Represents the result of a growth calculation.
/// </summary>
public struct GrowthResult
{
    public bool isValid;           // Whether the growth attempt added a node.
    public Vector3 position;       // Position of the new node.
    public MorphNode parentNode;   // The node from which growth occurred (if applicable).
    public float quality;          // A quality score (set to 1.0f as a placeholder).
}

/// <summary>
/// Handles the generation of biomorphic structures based on biological growth patterns.
/// This is the core system that creates the complex mesh-like structures inspired by
/// biological forms like molds, fungi, and cellular networks.
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
    [SerializeField] private Material growthMaterial;
    
    [Header("Generation Settings")]
    [SerializeField] private MorphologySettings settings;
    #endregion

    #region State
    // Container for the morphology
    private GameObject morphologyContainer;
    
    // Lists to track nodes and connections
    private List<MorphNode> nodes = new List<MorphNode>();
    private List<MorphConnection> connections = new List<MorphConnection>();
    
    // Growth state
    private bool isGenerating = false;
    private float generationProgress = 0f;
    
    // Progress tracking for UI
    public float GenerationProgress => generationProgress;
    
    // The active growth coroutine
    private Coroutine activeGrowthCoroutine;
    
    // Zone where the morphology is growing
    private Bounds growthZone;
    
    // Current generation parameters
    private MorphologyParameters currentParameters;
    #endregion

    #region Public Methods
    public void Initialize()
    {
        if (morphologyContainer == null)
        {
            morphologyContainer = new GameObject("MorphologyContainer");
            morphologyContainer.transform.parent = transform;
        }
        ResetMorphology();
    }
    
    public void UpdateSettings(MorphologySettings newSettings)
    {
        settings = newSettings;
    }
    
    public void GenerateMorphology(Bounds zone, MorphologyParameters parameters)
    {
        if (isGenerating)
        {
            StopGeneration();
        }
        
        growthZone = zone;
        currentParameters = parameters;
        ResetMorphology();
        
        activeGrowthCoroutine = StartCoroutine(GrowMorphologyCoroutine(parameters));
    }
    
    public void StopGeneration()
    {
        if (activeGrowthCoroutine != null)
        {
            StopCoroutine(activeGrowthCoroutine);
            activeGrowthCoroutine = null;
        }
        
        isGenerating = false;
        generationProgress = 0f;
    }
    
    public void ResetMorphology()
    {
        StopGeneration();
        
        foreach (var node in nodes)
        {
            if (node != null && node.gameObject != null)
            {
                Destroy(node.gameObject);
            }
        }
        
        foreach (var connection in connections)
        {
            if (connection != null && connection.gameObject != null)
            {
                Destroy(connection.gameObject);
            }
        }
        
        nodes.Clear();
        connections.Clear();
        
        foreach (Transform child in morphologyContainer.transform)
        {
            Destroy(child.gameObject);
        }
    }
    
    public MorphologyData ExportMorphologyData()
    {
        MorphologyData data = new MorphologyData();
        data.nodePositions = new List<Vector3>();
        foreach (var node in nodes)
        {
            data.nodePositions.Add(node.transform.position);
        }
        
        data.connections = new List<int[]>();
        foreach (var connection in connections)
        {
            int nodeAIndex = nodes.IndexOf(connection.NodeA);
            int nodeBIndex = nodes.IndexOf(connection.NodeB);
            if (nodeAIndex >= 0 && nodeBIndex >= 0)
            {
                data.connections.Add(new int[] { nodeAIndex, nodeBIndex });
            }
        }
        
        data.metrics = new Dictionary<string, float>();
        data.metrics["NodeCount"] = nodes.Count;
        data.metrics["ConnectionCount"] = connections.Count;
        data.metrics["Density"] = CalculateDensity();
        data.metrics["AverageConnectivity"] = CalculateAverageConnectivity();
        data.metrics["Volume"] = CalculateVolume();
        data.parameters = currentParameters;
        data.generationTime = System.DateTime.Now;
        
        return data;
    }
    
    public void ImportMorphologyData(MorphologyData data)
    {
        ResetMorphology();
        
        for (int i = 0; i < data.nodePositions.Count; i++)
        {
            CreateNode(data.nodePositions[i]);
        }
        
        foreach (var connectionIndices in data.connections)
        {
            if (connectionIndices.Length == 2)
            {
                int indexA = connectionIndices[0];
                int indexB = connectionIndices[1];
                if (indexA >= 0 && indexA < nodes.Count && indexB >= 0 && indexB < nodes.Count)
                {
                    CreateConnection(nodes[indexA], nodes[indexB]);
                }
            }
        }
        
        currentParameters = data.parameters;
    }

    public List<MorphNode> GetNodes()
    {
        return nodes;
    }

    public List<MorphConnection> GetConnections()
    {
        return connections;
    }

    public MorphNode AddNode(Vector3 position)
    {
        foreach (var existingNode in nodes)
        {
            if (Vector3.Distance(existingNode.transform.position, position) < settings.nodeMinDistance * 0.5f)
            {
                return existingNode;
            }
        }
        return CreateNode(position);
    }

    public MorphConnection AddConnection(MorphNode nodeA, MorphNode nodeB)
    {
        return CreateConnection(nodeA, nodeB);
    }

    public void RemoveNode(MorphNode nodeToRemove)
    {
        if (nodeToRemove == null || !nodes.Contains(nodeToRemove))
            return;

        List<MorphConnection> connectionsToRemove = connections.Where(c => c.NodeA == nodeToRemove || c.NodeB == nodeToRemove).ToList();
        foreach (var connection in connectionsToRemove)
        {
            RemoveConnection(connection);
        }

        nodes.Remove(nodeToRemove);
        Destroy(nodeToRemove.gameObject);
    }

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
            GrowthResult result = growthAlgorithm.CalculateGrowth(nodes, connections, growthZone, currentParameters, influenceMap, layerMask);
            // You can process result as needed.
            UpdateMeshes();
        }
        else
        {
            Debug.LogError("Failed to create growth algorithm.");
        }
    }
    #endregion

    #region Private Methods
    private IEnumerator GrowMorphologyCoroutine(MorphologyParameters parameters)
    {
        isGenerating = true;
        generationProgress = 0f;
        
        IGrowthAlgorithm growthAlgorithm = CreateGrowthAlgorithm(parameters);
        PlaceSeedNodes(parameters);
        
        float volume = growthZone.size.x * growthZone.size.y * growthZone.size.z;
        int targetNodeCount = Mathf.RoundToInt(settings.maxNodes * parameters.density);
        targetNodeCount = Mathf.Clamp(targetNodeCount, 50, settings.maxNodes);
        
        while (nodes.Count < targetNodeCount && isGenerating)
        {
            growthAlgorithm.GrowStep(nodes, connections, growthZone, parameters);
            UpdateMeshes();
            generationProgress = (float)nodes.Count / targetNodeCount;
            yield return null;
            AddConnections(parameters.connectivity);
        }
        
        UpdateMeshes();
        OptimizeMorphology();
        MorphologyData finalData = ExportMorphologyData();
        isGenerating = false;
        generationProgress = 1f;
        MorphologyManager.Instance.OnMorphologyGenerationComplete(finalData);
        activeGrowthCoroutine = null;
    }
    
    private void PlaceSeedNodes(MorphologyParameters parameters)
    {
        int seedCount = settings.initialSeedCount;
        
        for (int i = 0; i < seedCount; i++)
        {
            Vector3 randomPosition = new Vector3(
                Random.Range(growthZone.min.x, growthZone.max.x),
                Random.Range(growthZone.min.y, growthZone.max.y),
                Random.Range(growthZone.min.z, growthZone.max.z)
            );
            CreateNode(randomPosition);
        }
        
        for (int i = 0; i < nodes.Count; i++)
        {
            for (int j = i + 1; j < nodes.Count; j++)
            {
                float distance = Vector3.Distance(nodes[i].transform.position, nodes[j].transform.position);
                if (distance <= settings.nodeMaxDistance)
                {
                    if (Random.value < parameters.connectivity)
                    {
                        CreateConnection(nodes[i], nodes[j]);
                    }
                }
            }
        }
    }
    
    public MorphNode CreateNode(Vector3 position)
    {
        GameObject nodeObject = Instantiate(nodePrototype, position, Quaternion.identity, morphologyContainer.transform);
        nodeObject.name = $"Node_{nodes.Count}";
        MorphNode node = nodeObject.GetComponent<MorphNode>();
        if (node == null)
        {
            node = nodeObject.AddComponent<MorphNode>();
        }
        node.Initialize(nodeMaterial);
        nodes.Add(node);
        return node;
    }
    
    public MorphConnection CreateConnection(MorphNode nodeA, MorphNode nodeB)
    {
        if (connections.Any(c => (c.NodeA == nodeA && c.NodeB == nodeB) || (c.NodeA == nodeB && c.NodeB == nodeA)))
            return null;
        
        GameObject connectionObject = Instantiate(connectionPrototype, morphologyContainer.transform);
        connectionObject.name = $"Connection_{connections.Count}";
        MorphConnection connection = connectionObject.GetComponent<MorphConnection>();
        if (connection == null)
        {
            connection = connectionObject.AddComponent<MorphConnection>();
        }
        connection.Initialize(nodeA, nodeB, connectionMaterial);
        connections.Add(connection);
        nodeA.AddConnection(connection);
        nodeB.AddConnection(connection);
        return connection;
    }
    
    private void AddConnections(float connectivity)
    {
        int attemptCount = Mathf.RoundToInt(nodes.Count * connectivity);
        for (int i = 0; i < attemptCount; i++)
        {
            int indexA = Random.Range(0, nodes.Count);
            int indexB = Random.Range(0, nodes.Count);
            if (indexA == indexB) continue;
            MorphNode nodeA = nodes[indexA];
            MorphNode nodeB = nodes[indexB];
            float distance = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            if (distance <= settings.nodeMaxDistance && distance >= settings.nodeMinDistance)
            {
                CreateConnection(nodeA, nodeB);
            }
        }
    }
    
    public void UpdateMeshes()
    {
        foreach (var node in nodes)
        {
            node.UpdateVisual();
        }
        foreach (var connection in connections)
        {
            connection.UpdateVisual();
        }
    }
    
    private void OptimizeMorphology()
    {
        List<MorphNode> isolatedNodes = nodes.Where(n => n.ConnectionCount == 0).ToList();
        foreach (var node in isolatedNodes)
        {
            nodes.Remove(node);
            Destroy(node.gameObject);
        }
    }
    
    private float CalculateDensity()
    {
        float volume = growthZone.size.x * growthZone.size.y * growthZone.size.z;
        return nodes.Count / volume;
    }
    
    private float CalculateAverageConnectivity()
    {
        if (nodes.Count == 0) return 0;
        float totalConnections = nodes.Sum(n => n.ConnectionCount);
        return totalConnections / nodes.Count;
    }
    
    private float CalculateVolume()
    {
        if (nodes.Count < 2) return 0;
        Vector3 min = nodes[0].transform.position;
        Vector3 max = nodes[0].transform.position;
        foreach (var node in nodes)
        {
            Vector3 pos = node.transform.position;
            min = Vector3.Min(min, pos);
            max = Vector3.Max(max, pos);
        }
        Vector3 size = max - min;
        return size.x * size.y * size.z;
    }
    
    private IGrowthAlgorithm CreateGrowthAlgorithm(MorphologyParameters parameters)
    {
        switch (parameters.biomorphType)
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
/// Interface for different growth algorithms.
/// </summary>
public interface IGrowthAlgorithm
{
    void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters);
    GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask);
}

/// <summary>
/// Growth algorithm that mimics slime mold behavior.
/// </summary>
public class MoldGrowthAlgorithm : IGrowthAlgorithm
{
    public void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        if (nodes.Count == 0) return;
        MorphNode sourceNode = nodes[Random.Range(0, nodes.Count)];
        Vector3 growthDirection = Random.onUnitSphere;
        Vector3 towardsCenter = (growthZone.center - sourceNode.transform.position).normalized;
        growthDirection = Vector3.Lerp(growthDirection, towardsCenter, 0.3f);
        float distance = Mathf.Lerp(2.0f, 5.0f, parameters.complexity);
        Vector3 newPosition = sourceNode.transform.position + growthDirection * distance;
        newPosition.x = Mathf.Clamp(newPosition.x, growthZone.min.x, growthZone.max.x);
        newPosition.y = Mathf.Clamp(newPosition.y, growthZone.min.y, growthZone.max.y);
        newPosition.z = Mathf.Clamp(newPosition.z, growthZone.min.z, growthZone.max.z);
        GameObject nodePrototype = sourceNode.gameObject;
        GameObject morphologyContainer = sourceNode.transform.parent.gameObject;
        GameObject newNodeObject = Object.Instantiate(nodePrototype, newPosition, Quaternion.identity, morphologyContainer.transform);
        newNodeObject.name = $"Node_{nodes.Count}";
        MorphNode newNode = newNodeObject.GetComponent<MorphNode>();
        if (newNode == null)
        {
            newNode = newNodeObject.AddComponent<MorphNode>();
        }
        newNode.Initialize(sourceNode.GetMaterial());
        nodes.Add(newNode);
        if (connections.Count > 0)
        {
            GameObject connectionPrototype = connections[0].gameObject;
            if (connectionPrototype != null)
            {
                GameObject newConnectionObject = Object.Instantiate(connectionPrototype, morphologyContainer.transform);
                newConnectionObject.name = $"Connection_{connections.Count}";
                MorphConnection newConnection = newConnectionObject.GetComponent<MorphConnection>();
                if (newConnection == null)
                {
                    newConnection = newConnectionObject.AddComponent<MorphConnection>();
                }
                newConnection.Initialize(sourceNode, newNode, connections[0].GetMaterial());
                connections.Add(newConnection);
                sourceNode.AddConnection(newConnection);
                newNode.AddConnection(newConnection);
            }
        }
    }
    
    public GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask)
    {
        int initialCount = nodes.Count;
        GrowStep(nodes, connections, growthZone, parameters);
        GrowthResult result = new GrowthResult();
        if (nodes.Count > initialCount)
        {
            MorphNode newNode = nodes[nodes.Count - 1];
            result.isValid = true;
            result.position = newNode.transform.position;
            result.parentNode = null;
            result.quality = 1.0f;
        }
        else
        {
            result.isValid = false;
        }
        return result;
    }
}

/// <summary>
/// Growth algorithm that mimics bone structure.
/// </summary>
public class BoneGrowthAlgorithm : IGrowthAlgorithm
{
    public void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        new MoldGrowthAlgorithm().GrowStep(nodes, connections, growthZone, parameters);
    }
    
    public GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask)
    {
        int initialCount = nodes.Count;
        GrowStep(nodes, connections, growthZone, parameters);
        GrowthResult result = new GrowthResult();
        if (nodes.Count > initialCount)
        {
            MorphNode newNode = nodes[nodes.Count - 1];
            result.isValid = true;
            result.position = newNode.transform.position;
            result.parentNode = null;
            result.quality = 1.0f;
        }
        else
        {
            result.isValid = false;
        }
        return result;
    }
}

/// <summary>
/// Growth algorithm that mimics coral formations.
/// </summary>
public class CoralGrowthAlgorithm : IGrowthAlgorithm
{
    public void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        new MoldGrowthAlgorithm().GrowStep(nodes, connections, growthZone, parameters);
    }
    
    public GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask)
    {
        int initialCount = nodes.Count;
        GrowStep(nodes, connections, growthZone, parameters);
        GrowthResult result = new GrowthResult();
        if (nodes.Count > initialCount)
        {
            MorphNode newNode = nodes[nodes.Count - 1];
            result.isValid = true;
            result.position = newNode.transform.position;
            result.parentNode = null;
            result.quality = 1.0f;
        }
        else
        {
            result.isValid = false;
        }
        return result;
    }
}

/// <summary>
/// Growth algorithm that mimics mycelium networks.
/// </summary>
public class MyceliumGrowthAlgorithm : IGrowthAlgorithm
{
    public void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        new MoldGrowthAlgorithm().GrowStep(nodes, connections, growthZone, parameters);
    }
    
    public GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask)
    {
        int initialCount = nodes.Count;
        GrowStep(nodes, connections, growthZone, parameters);
        GrowthResult result = new GrowthResult();
        if (nodes.Count > initialCount)
        {
            MorphNode newNode = nodes[nodes.Count - 1];
            result.isValid = true;
            result.position = newNode.transform.position;
            result.parentNode = null;
            result.quality = 1.0f;
        }
        else
        {
            result.isValid = false;
        }
        return result;
    }
}

/// <summary>
/// Custom growth algorithm that can be tuned with parameters.
/// </summary>
public class CustomGrowthAlgorithm : IGrowthAlgorithm
{
    public void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        new MoldGrowthAlgorithm().GrowStep(nodes, connections, growthZone, parameters);
    }
    
    public GrowthResult CalculateGrowth(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone,
        MorphologyParameters parameters, GrowthInfluenceMap influenceMap, LayerMask layerMask)
    {
        int initialCount = nodes.Count;
        GrowStep(nodes, connections, growthZone, parameters);
        GrowthResult result = new GrowthResult();
        if (nodes.Count > initialCount)
        {
            MorphNode newNode = nodes[nodes.Count - 1];
            result.isValid = true;
            result.position = newNode.transform.position;
            result.parentNode = null;
            result.quality = 1.0f;
        }
        else
        {
            result.isValid = false;
        }
        return result;
    }
}