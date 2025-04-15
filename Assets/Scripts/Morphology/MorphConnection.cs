using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a connection between two morphology nodes, acting like a spring
/// that can adapt to environmental forces.
/// </summary>
public class MorphConnection : MonoBehaviour
{
    [Header("Connection Properties")]
    [SerializeField] private MorphNode nodeA;
    [SerializeField] private MorphNode nodeB;
    [SerializeField] private float springConstant = 10f;
    [SerializeField] private float dampingFactor = 0.5f;
    [SerializeField] private float restLength = 2f;
    [SerializeField] private float maxLength = 5f;
    [SerializeField] private float minLength = 0.5f;
    [SerializeField] private float thickness = 0.2f;
    
    [Header("Adaptation")]
    [SerializeField] private float adaptationRate = 0.01f;
    [SerializeField] private bool canAdapt = true;
    [SerializeField] private bool canBreak = true;
    [SerializeField] private float breakThreshold = 2.0f; // How much strain before breaking
    
    [Header("Visualization")]
    [SerializeField] private MeshRenderer meshRenderer;
    [SerializeField] private Material defaultMaterial;
    [SerializeField] private Color baseColor = Color.gray;
    
    // Internal state
    private float currentLength;
    private float strain = 0f; // Normalized strain (0-1)
    private float age = 0f;
    private Mesh mesh;
    
    // Properties
    public MorphNode NodeA { get => nodeA; }
    public MorphNode NodeB { get => nodeB; }
    public float SpringConstant { get => springConstant; set => springConstant = value; }
    public float RestLength { get => restLength; set => restLength = value; }
    public float Strain { get => strain; }
    public float Age { get => age; }
    public float Strength { get; set; } = 1.0f; // Default value, adjust as needed
    public float Stress { get => strain; } // Map Stress to existing strain property
    
    #region Unity Lifecycle
    
    private void Awake()
    {
        // Initialize renderer if needed
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
            
            if (meshRenderer == null)
            {
                meshRenderer = gameObject.AddComponent<MeshRenderer>();
            }
        }
        
        // Create a cylinder mesh for the connection
        if (TryGetComponent(out MeshFilter meshFilter))
        {
            mesh = CreateCylinderMesh();
            meshFilter.mesh = mesh;
        }
        else
        {
            MeshFilter filter = gameObject.AddComponent<MeshFilter>();
            mesh = CreateCylinderMesh();
            filter.mesh = mesh;
        }
        
        // Initialize material
        if (meshRenderer.sharedMaterial == null)
        {
            if (defaultMaterial != null)
            {
                meshRenderer.material = new Material(defaultMaterial);
            }
            else
            {
                Material material = new Material(Shader.Find("Standard"));
                material.color = baseColor;
                meshRenderer.material = material;
            }
        }
    }
    
    private void Start()
    {
        // Set initial rest length if not already set
        if (restLength <= 0 && nodeA != null && nodeB != null)
        {
            restLength = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
        }
        
        // Register connection with nodes
        if (nodeA != null) nodeA.AddConnection(this);
        if (nodeB != null) nodeB.AddConnection(this);
        
        // Update visuals
        UpdateVisual();
    }
    
    private void Update()
    {
        // Update age
        age += Time.deltaTime;
        
        // Check if connection should break
        if (canBreak && strain > breakThreshold)
        {
            BreakConnection();
            return;
        }
        
        // Adapt to environmental forces if needed
        if (canAdapt && adaptationRate > 0)
        {
            Adapt();
        }
        
        // Update the visual representation
        UpdateVisual();
    }
    
    private void FixedUpdate()
    {
        if (nodeA == null || nodeB == null) return;
        
        // Apply spring forces to nodes
        ApplySpringForces();
    }
    
    #endregion
    
    #region Public Methods
    
    /// <summary>
    /// Initialize the connection between two nodes with a material
    /// </summary>
    /// <param name="start">First node</param>
    /// <param name="end">Second node</param>
    /// <param name="material">Material to use for the connection</param>
    public void Initialize(MorphNode start, MorphNode end, Material material)
    {
        nodeA = start;
        nodeB = end;
        
        if (nodeA != null && nodeB != null)
        {
            // Set initial rest length
            restLength = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            
            // Register with nodes
            nodeA.AddConnection(this);
            nodeB.AddConnection(this);
        }
        
        // Assign material
        if (meshRenderer != null && material != null)
        {
            meshRenderer.material = new Material(material);
        }
        
        // Update visual
        UpdateVisual();
    }
    
    /// <summary>
    /// Updates the visual representation of the connection
    /// </summary>
    public void UpdateVisual()
    {
        if (nodeA == null || nodeB == null) return;
        
        // Position and scale
        Vector3 midPoint = (nodeA.transform.position + nodeB.transform.position) / 2f;
        transform.position = midPoint;
        
        // Calculate the length and direction
        Vector3 direction = nodeB.transform.position - nodeA.transform.position;
        currentLength = direction.magnitude;
        
        // Update strain
        strain = Mathf.Clamp01(Mathf.Abs(currentLength - restLength) / restLength);
        
        // Look at the target
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Scale the connection - adjust radius based on thickness, length based on distance
        transform.localScale = new Vector3(thickness, thickness, currentLength);
    }
    
    /// <summary>
    /// Gets the current material of the connection
    /// </summary>
    /// <returns>The current material</returns>
    public Material GetMaterial()
    {
        if (meshRenderer != null)
        {
            return meshRenderer.sharedMaterial;
        }
        return null;
    }
    
    /// <summary>
    /// Updates connection attributes based on time and average force (Modified based on errors)
    /// </summary>
    public void UpdateAttributes(float deltaTime, Vector3 averageForce)
    {
        // Placeholder: Add logic for updating connection attributes based on time and force.
        // For example, Strength might change based on age, stress, or applied force.
        // Stress (strain) is already updated in UpdateVisual().
        // You might want to adjust Strength or other properties here.
        
        // Example: Increase strength slightly under moderate stress
        if (Stress > 0.3f && Stress < 0.7f)
        {
            Strength += deltaTime * adaptationRate * 0.1f;
        }
        // Example: Decrease strength under high stress
        else if (Stress >= 0.7f)
        {
             Strength -= deltaTime * adaptationRate * 0.2f;
        }
        Strength = Mathf.Clamp(Strength, 0.1f, 2.0f); // Keep strength within bounds
    }
    
    /// <summary>
    /// Breaks the connection, removing it from both nodes
    /// </summary>
    public void BreakConnection()
    {
        // Remove from nodes
        if (nodeA != null) nodeA.RemoveConnection(this);
        if (nodeB != null) nodeB.RemoveConnection(this);
        
        // Destroy the connection
        Destroy(gameObject);
    }
    
    #endregion
    
    #region Private Methods
    
    /// <summary>
    /// Applies spring forces to the connected nodes
    /// </summary>
    private void ApplySpringForces()
    {
        if (nodeA == null || nodeB == null) return;
        
        // Calculate spring force
        Vector3 direction = nodeB.transform.position - nodeA.transform.position;
        float distance = direction.magnitude;
        
        if (distance == 0) return; // Avoid division by zero
        
        Vector3 directionNormalized = direction / distance;
        
        // Calculate Hooke's law: F = -k * (x - x₀)
        float displacement = distance - restLength;
        float forceMagnitude = springConstant * displacement;
        
        // Calculate damping based on relative velocity
        Vector3 relativeVelocity = nodeB.Velocity - nodeA.Velocity;
        float dampingForce = dampingFactor * Vector3.Dot(relativeVelocity, directionNormalized);
        
        // Add damping to force magnitude
        forceMagnitude += dampingForce;
        
        // Apply the force to both nodes in opposite directions
        Vector3 springForce = directionNormalized * forceMagnitude;
        
        if (!nodeA.IsAnchored)
        {
            nodeA.ApplyForce(springForce);
        }
        
        if (!nodeB.IsAnchored)
        {
            nodeB.ApplyForce(-springForce);
        }
    }
    
    /// <summary>
    /// Adapts the connection properties based on strain
    /// </summary>
    private void Adapt()
    {
        if (!canAdapt || adaptationRate <= 0) return;
        
        // High strain over time causes adaptation
        if (strain > 0.5f)
        {
            // Gradually adjust rest length to reduce strain
            restLength = Mathf.Lerp(restLength, currentLength, adaptationRate * Time.deltaTime);
            
            // Limit to maximum length
            restLength = Mathf.Clamp(restLength, minLength, maxLength);
            
            // Also adapt spring constant - get stiffer with age
            springConstant *= (1 + adaptationRate * 0.01f * Time.deltaTime);
        }
    }
    
    /// <summary>
    /// Creates a simple cylinder mesh for the connection
    /// </summary>
    /// <returns>A cylinder mesh</returns>
    private Mesh CreateCylinderMesh()
    {
        // Create a temporary cylinder primitive
        GameObject tempCylinder = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        
        // Get the mesh
        Mesh cylinderMesh = tempCylinder.GetComponent<MeshFilter>().sharedMesh;
        
        // Rotate the mesh so cylinder axis is aligned with local Z axis
        tempCylinder.transform.eulerAngles = new Vector3(90, 0, 0);
        Matrix4x4 rotationMatrix = Matrix4x4.TRS(Vector3.zero, tempCylinder.transform.rotation, Vector3.one);
        
        Vector3[] vertices = cylinderMesh.vertices;
        for (int i = 0; i < vertices.Length; i++)
        {
            vertices[i] = rotationMatrix.MultiplyPoint3x4(vertices[i]);
        }
        cylinderMesh.vertices = vertices;
        
        // Update the mesh
        cylinderMesh.RecalculateNormals();
        cylinderMesh.RecalculateBounds();
        
        // Destroy the temporary object
        Destroy(tempCylinder);
        
        return cylinderMesh;
    }
    
    #endregion
}
