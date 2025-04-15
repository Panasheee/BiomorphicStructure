using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core;

/// <summary>
/// Result of a growth calculation
/// </summary>
public struct GrowthResult
{
    /// <summary>
    /// Whether the growth calculation produced a valid result
    /// </summary>
    public bool isValid;
    
    /// <summary>
    /// Position for a new node
    /// </summary>
    public Vector3 position;
    
    public Vector3 direction;
    
    /// <summary>
    /// Probability or weight of this growth
    /// </summary>
    public float probability;
}