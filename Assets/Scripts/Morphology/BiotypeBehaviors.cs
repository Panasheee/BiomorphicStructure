using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Implements specialized growth behaviors for different biological morphology types.
/// This class provides detailed algorithms for how each biomorph type grows and adapts.
/// </summary>
public class BiotypeBehaviors : MonoBehaviour
{
    [Header("Mold Behavior Settings")]
    [SerializeField] private float moldExplorationRate = 0.7f;
    [SerializeField] private float moldBranchingRate = 0.3f;
    [SerializeField] private float moldResourceSensitivity = 0.8f;
    [SerializeField] private float moldConnectionStrength = 0.5f;
    
    [Header("Bone Behavior Settings")]
    [SerializeField] private float boneStructuralIntegrity = 0.9f;
    [SerializeField] private float boneStressResponse = 0.8f;
    [SerializeField] private float boneTrabeculaeFormation = 0.6f;
    [SerializeField] private float boneReinforcementRate = 0.4f;
    
    [Header("Coral Behavior Settings")]
    [SerializeField] private float coralGrowthVerticalBias = 0.7f;
    [SerializeField] private float coralBranchingFrequency = 0.5f;
    [SerializeField] private float coralPlateFormation = 0.4f;
    [SerializeField] private float coralDensity = 0.6f;
    
    [Header("Mycelium Behavior Settings")]
    [SerializeField] private float myceliumNetworkExpansion = 0.8f;
    [SerializeField] private float myceliumBranchingRate = 0.6f;
    [SerializeField] private float myceliumResourceSensitivity = 0.9f;
    [SerializeField] private float myceliumConnectionThinness = 0.3f;
    
    [Header("Custom Behavior Settings")]
    [SerializeField] private float customParameter1 = 0.5f;
    [SerializeField] private float customParameter2 = 0.5f;
    [SerializeField] private float customParameter3 = 0.5f;
    [SerializeField] private float customParameter4 = 0.5f;
    
    [Header("Environmental Responses")]
    [SerializeField] private float windResponseFactor = 1.0f;
    [SerializeField] private float gravityResponseFactor = 1.0f;
    [SerializeField] private float lightResponseFactor = 1.0f;
    [SerializeField] private float pedestrianResponseFactor = 1.0f;

    /// <summary>
    /// Gets the appropriate growth algorithm for a given morphology type.
    /// </summary>
    /// <param name="parameters">Morphology parameters</param>
    /// <returns>Growth algorithm for the specified morphology type</returns>
    public IGrowthAlgorithm GetGrowthAlgorithm(MorphologyParameters parameters)
    {
        switch (parameters.biomorphType)
        {
            case MorphologyParameters.BiomorphType.Mold:
                return new DetailedMoldGrowthAlgorithm(
                    moldExplorationRate, 
                    moldBranchingRate,
                    moldResourceSensitivity,
                    moldConnectionStrength,
                    parameters
                );
                
            case MorphologyParameters.BiomorphType.Bone:
                return new DetailedBoneGrowthAlgorithm(
                    boneStructuralIntegrity,
                    boneStressResponse,
                    boneTrabeculaeFormation,
                    boneReinforcementRate,
                    parameters
                );
                
            case MorphologyParameters.BiomorphType.Coral:
                return new DetailedCoralGrowthAlgorithm(
                    coralGrowthVerticalBias,
                    coralBranchingFrequency,
                    coralPlateFormation,
                    coralDensity,
                    parameters
                );
                
            case MorphologyParameters.BiomorphType.Mycelium:
                return new DetailedMyceliumGrowthAlgorithm(
                    myceliumNetworkExpansion,
                    myceliumBranchingRate,
                    myceliumResourceSensitivity,
                    myceliumConnectionThinness,
                    parameters
                );
                
            case MorphologyParameters.BiomorphType.Custom:
                return new DetailedCustomGrowthAlgorithm(
                    customParameter1,
                    customParameter2,
                    customParameter3,
                    customParameter4,
                    parameters
                );
                
            default:
                throw new System.ArgumentException($"Unexpected BiomorphType: {parameters.biomorphType}");
        }
    }
    
    /// <summary>
    /// Modifies a force vector based on biomorph type.
    /// </summary>
    public Vector3 ModifyForceForBiomorph(Vector3 force, MorphologyParameters.BiomorphType biomorphType, string forceName)
    {
        float modifier = 1.0f;
        
        switch (forceName)
        {
            case "Wind":
                modifier = windResponseFactor;
                break;
            case "Gravity":
                modifier = gravityResponseFactor;
                break;
            case "Sunlight":
            case "Temperature":
                modifier = lightResponseFactor;
                break;
            case "PedestrianFlow":
                modifier = pedestrianResponseFactor;
                break;
        }
        
        switch (biomorphType)
        {
            case MorphologyParameters.BiomorphType.Mold:
                if (forceName == "Gravity")
                    modifier *= 0.7f;
                else if (forceName == "Wind")
                    modifier *= 1.2f;
                break;
            case MorphologyParameters.BiomorphType.Bone:
                if (forceName == "Gravity")
                    modifier *= 0.4f;
                else if (forceName == "Wind")
                    modifier *= 0.6f;
                break;
            case MorphologyParameters.BiomorphType.Coral:
                if (forceName == "Gravity")
                    modifier *= 0.5f;
                else if (forceName == "Sunlight" || forceName == "Temperature")
                    modifier *= 1.5f;
                break;
            case MorphologyParameters.BiomorphType.Mycelium:
                if (forceName == "PedestrianFlow")
                    modifier *= 1.4f;
                else if (forceName == "Temperature")
                    modifier *= 1.3f;
                break;
        }
        
        return force * modifier;
    }
    
    /// <summary>
    /// Gets a material property modifier for visualization based on biomorph type.
    /// </summary>
    public float GetMaterialPropertyModifier(MorphologyParameters.BiomorphType biomorphType, string propertyName)
    {
        switch (propertyName)
        {
            case "Thickness":
                switch (biomorphType)
                {
                    case MorphologyParameters.BiomorphType.Mold: return 0.8f;
                    case MorphologyParameters.BiomorphType.Bone: return 1.2f;
                    case MorphologyParameters.BiomorphType.Coral: return 1.0f;
                    case MorphologyParameters.BiomorphType.Mycelium: return 0.6f;
                    default: return 1.0f;
                }
            case "Roughness":
                switch (biomorphType)
                {
                    case MorphologyParameters.BiomorphType.Mold: return 0.7f;
                    case MorphologyParameters.BiomorphType.Bone: return 0.5f;
                    case MorphologyParameters.BiomorphType.Coral: return 0.9f;
                    case MorphologyParameters.BiomorphType.Mycelium: return 0.3f;
                    default: return 0.6f;
                }
            case "Glow":
                switch (biomorphType)
                {
                    case MorphologyParameters.BiomorphType.Mold: return 0.2f;
                    case MorphologyParameters.BiomorphType.Bone: return 0.0f;
                    case MorphologyParameters.BiomorphType.Coral: return 0.4f;
                    case MorphologyParameters.BiomorphType.Mycelium: return 0.3f;
                    default: return 0.1f;
                }
            default:
                return 1.0f;
        }
    }
}

/// <summary>
/// Detailed growth algorithm for mold-like structures.
/// </summary>
public class DetailedMoldGrowthAlgorithm : IGrowthAlgorithm
{
    private float explorationRate;
    private float branchingRate;
    private float resourceSensitivity;
    private float connectionStrength;
    private MorphologyParameters parameters;
    
    public DetailedMoldGrowthAlgorithm(float explorationRate, float branchingRate, float resourceSensitivity, float connectionStrength, MorphologyParameters parameters)
    {
        this.explorationRate = explorationRate;
        this.branchingRate = branchingRate;
        this.resourceSensitivity = resourceSensitivity;
        this.connectionStrength = connectionStrength;
        this.parameters = parameters;
    }
    
    // Placeholder implementation—fill in detailed mold logic here.
    public void GrowStep(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        throw new System.NotImplementedException();
    }
    
    // Updated to return a GrowthResult.
    public GrowthResult CalculateGrowth(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone,
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
/// Detailed growth algorithm for bone-like structures.
/// </summary>
public class DetailedBoneGrowthAlgorithm : IGrowthAlgorithm
{
    private float structuralIntegrity;
    private float stressResponse;
    private float trabeculaeFormation;
    private float reinforcementRate;
    private MorphologyParameters parameters;
    
    public DetailedBoneGrowthAlgorithm(float structuralIntegrity, float stressResponse, float trabeculaeFormation, float reinforcementRate, MorphologyParameters parameters)
    {
        this.structuralIntegrity = structuralIntegrity;
        this.stressResponse = stressResponse;
        this.trabeculaeFormation = trabeculaeFormation;
        this.reinforcementRate = reinforcementRate;
        this.parameters = parameters;
    }
    
    public void GrowStep(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        throw new System.NotImplementedException();
    }
    
    public GrowthResult CalculateGrowth(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone,
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
/// Detailed growth algorithm for coral-like structures.
/// </summary>
public class DetailedCoralGrowthAlgorithm : IGrowthAlgorithm
{
    private float verticalBias;
    private float branchingFrequency;
    private float plateFormation;
    private float density;
    private MorphologyParameters parameters;
    
    public DetailedCoralGrowthAlgorithm(float verticalBias, float branchingFrequency, float plateFormation, float density, MorphologyParameters parameters)
    {
        this.verticalBias = verticalBias;
        this.branchingFrequency = branchingFrequency;
        this.plateFormation = plateFormation;
        this.density = density;
        this.parameters = parameters;
    }
    
    public void GrowStep(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        throw new System.NotImplementedException();
    }
    
    public GrowthResult CalculateGrowth(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone,
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
/// Detailed growth algorithm for mycelium-like structures.
/// </summary>
public class DetailedMyceliumGrowthAlgorithm : IGrowthAlgorithm
{
    private float networkExpansion;
    private float branchingRate;
    private float resourceSensitivity;
    private float connectionThinness;
    private MorphologyParameters parameters;
    
    public DetailedMyceliumGrowthAlgorithm(float networkExpansion, float branchingRate, float resourceSensitivity, float connectionThinness, MorphologyParameters parameters)
    {
        this.networkExpansion = networkExpansion;
        this.branchingRate = branchingRate;
        this.resourceSensitivity = resourceSensitivity;
        this.connectionThinness = connectionThinness;
        this.parameters = parameters;
    }
    
    public void GrowStep(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        throw new System.NotImplementedException();
    }
    
    public GrowthResult CalculateGrowth(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone,
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
/// Detailed growth algorithm for custom structures.
/// </summary>
public class DetailedCustomGrowthAlgorithm : IGrowthAlgorithm
{
    private float param1, param2, param3, param4;
    private MorphologyParameters parameters;
    
    public DetailedCustomGrowthAlgorithm(float param1, float param2, float param3, float param4, MorphologyParameters parameters)
    {
        this.param1 = param1;
        this.param2 = param2;
        this.param3 = param3;
        this.param4 = param4;
        this.parameters = parameters;
    }
    
    public void GrowStep(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters)
    {
        throw new System.NotImplementedException();
    }
    
    public GrowthResult CalculateGrowth(System.Collections.Generic.List<MorphNode> nodes, System.Collections.Generic.List<MorphConnection> connections, Bounds growthZone,
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