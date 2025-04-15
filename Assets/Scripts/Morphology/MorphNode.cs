using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a node in the morphology structure.
/// Nodes are the basic building blocks that form the vertices of the morphology network.
/// </summary>
public class MorphNode : MonoBehaviour
{
    #region Properties
    // Node properties
    private float nodeSize = 0.5f;
    private Material nodeMaterial;
    
    // Connections to other nodes
    private List<MorphConnection> connections = new List<MorphConnection>();
    
    // Physics/Simulation Properties (Added based on errors)
    public Vector3 Velocity { get; set; } = Vector3.zero; 
    public bool IsAnchored { get; set; } = false; 
    
    // Node attributes
    public float Energy { get; set; } = 1.0f;
    public float GrowthPotential { get; set; } = 1.0f;
    public float Stress { get; set; } = 0.0f;
    
    // Statistics
    public int ConnectionCount => connections.Count;
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the node with a specific material (Added based on errors)
    /// </summary>
    public void Initialize(Material material)
    {
        nodeMaterial = material;
        EnsureMeshComponents(); // Ensure mesh exists
        UpdateVisual(); // Apply initial visual state
    }
    
    /// <summary>
    /// Updates the visual representation of the node
    /// </summary>
    public void UpdateVisual()
    {
        // Ensure we have a mesh renderer and filter
        EnsureMeshComponents();
        
        // Scale based on energy and connections
        float scaleFactor = nodeSize * (0.8f + 0.4f * Energy) * (0.5f + 0.1f * connections.Count);
        transform.localScale = new Vector3(scaleFactor, scaleFactor, scaleFactor);
        
        // Update material properties
        if (nodeMaterial != null)
        {
            MeshRenderer renderer = GetComponent<MeshRenderer>();
            if (renderer != null)
            {
                renderer.material = nodeMaterial;
                
                // Optional: Modify material properties based on node attributes
                if (renderer.material.HasProperty("_Energy"))
                {
                    renderer.material.SetFloat("_Energy", Energy);
                }
                
                if (renderer.material.HasProperty("_Stress"))
                {
                    renderer.material.SetFloat("_Stress", Stress);
                }
            }
        }
    }
    
    /// <summary>
    /// Updates node attributes based on its environment and connections
    /// </summary>
    public void UpdateAttributes(float deltaTime, Vector3 environmentalForce)
    {
        // Example attribute updates based on connections and environment
        
        // Energy decreases over time, more with more connections
        Energy = Mathf.Max(0.1f, Energy - deltaTime * 0.01f * connections.Count);
        
        // Growth potential decreases with connections
        GrowthPotential = Mathf.Max(0.0f, 1.0f - 0.1f * connections.Count);
        
        // Stress increases with environmental force
        Stress = Mathf.Min(1.0f, Stress + deltaTime * environmentalForce.magnitude * 0.1f);
        
        // Update visual after attribute changes
        UpdateVisual();
    }
    
    /// <summary>
    /// Adds a connection reference to this node (Added based on errors)
    /// </summary>
    public void AddConnection(MorphConnection connection)
    {
        if (!connections.Contains(connection))
        {
            connections.Add(connection);
        }
    }

    /// <summary>
    /// Removes a connection reference from this node (Added based on errors)
    /// </summary>
    public void RemoveConnection(MorphConnection connection)
    {
        connections.Remove(connection);
    }

    /// <summary>
    /// Applies a force to the node (Added based on errors)
    /// </summary>
    public void ApplyForce(Vector3 force)
    {
        if (!IsAnchored)
        {
            // Assuming simple physics for now, might need Rigidbody later
            // Velocity += force * Time.fixedDeltaTime; 
            // transform.position += Velocity * Time.fixedDeltaTime;
            // Placeholder: Add force handling logic if needed
        }
    }
    
    /// <summary>
    /// Returns the node's material (Added based on errors)
    /// </summary>
    public Material GetMaterial()
    {
        return nodeMaterial;
    }

    /// <summary>
    /// Gets all connected nodes
    /// </summary>
    public List<MorphNode> GetConnectedNodes()
    {
        List<MorphNode> connectedNodes = new List<MorphNode>();
        
        foreach (var connection in connections)
        {
            if (connection.NodeA == this)
            {
                connectedNodes.Add(connection.NodeB);
            }
            else if (connection.NodeB == this)
            {
                connectedNodes.Add(connection.NodeA);
            }
        }
        
        return connectedNodes;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Ensures the node has the necessary mesh components
    /// </summary>
    private void EnsureMeshComponents()
    {
        // Add MeshFilter if missing
        MeshFilter meshFilter = GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
            meshFilter.mesh = CreateSphereMesh(1.0f, 8);
        }
        
        // Add MeshRenderer if missing
        MeshRenderer meshRenderer = GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = gameObject.AddComponent<MeshRenderer>();
        }
    }
    
    /// <summary>
    /// Creates a simple sphere mesh
    /// </summary>
    private Mesh CreateSphereMesh(float radius, int subdivisions)
    {
        Mesh mesh = new Mesh();
        
        // For a simple prototype, you might use a primitive sphere instead
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        mesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempSphere);
        
        return mesh;
    }
    #endregion
}