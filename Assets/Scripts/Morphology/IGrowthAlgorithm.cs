using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Interface for growth algorithms that can be used by the GrowthSystem.
/// Implementations provide different growth patterns for various biomorph types.
/// </summary>
public interface IGrowthAlgorithm
{
    /// <summary>
    /// Calculates growth for a morphological structure.
    /// </summary>
    /// <param name="nodes">Current nodes in the structure</param>
    /// <param name="connections">Current connections in the structure</param>
    /// <param name="bounds">Growth bounds</param>
    /// <param name="parameters">Morphology parameters</param>
    /// <param name="influenceMap">Influence map for growth guidance</param>
    /// <param name="obstaclesMask">Layers to avoid during growth</param>
    /// <returns>Result of growth calculation</returns>
    GrowthResult CalculateGrowth(
        List<MorphNode> nodes, 
        List<MorphConnection> connections, 
        Bounds bounds, 
        MorphologyParameters parameters,
        GrowthInfluenceMap influenceMap,
        LayerMask obstaclesMask
        
    );
}

/// <summary>
/// Result of a growth calculation
/// </summary>
public struct GrowthResult
{
    public bool isValid;       // Whether the growth result is valid
    public Vector3 position;    // Position of the potential new node
    public MorphNode parentNode; // The node from which this growth originates
    public float quality;      // A metric indicating the quality/priority of this growth point (e.g., 0-1)
}
