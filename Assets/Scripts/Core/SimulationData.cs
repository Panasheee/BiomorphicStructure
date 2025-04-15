using System.Collections.Generic;
using UnityEngine;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Interface for growth algorithms.
    /// </summary>
    public interface IGrowthAlgorithm
    {
        /// <summary>
        /// Calculates the next growth step.
        /// </summary>
        GrowthResult CalculateGrowth(
            List<MorphNode> nodes, 
            List<MorphConnection> connections, 
            Bounds bounds, 
            MorphologyParameters parameters,
            GrowthInfluenceMap influenceMap, // Assuming GrowthInfluenceMap is defined elsewhere or will be added
            LayerMask obstaclesMask
        );
        
        // Optional: Keep GrowStep if it's used by some algorithms, otherwise remove.
        // void GrowStep(List<MorphNode> nodes, List<MorphConnection> connections, Bounds growthZone, MorphologyParameters parameters);
    }

    /// <summary>
    /// Result of a growth calculation. Defines potential new node info.
    /// </summary>

    /// <summary>
    /// Holds the overall simulation settings.
    /// </summary>
    [System.Serializable]
    public class SimulationSettings
    {
        public SiteSettings siteSettings;
        public MorphologySettings morphologySettings;
        public ScenarioSettings scenarioSettings;
    }

    /// <summary>
    /// Settings for site generation.
    /// </summary>
    [System.Serializable]
    public class SiteSettings
    {
        public Vector3 siteSize = new Vector3(1000, 200, 1000);
        public float detailLevel = 1.0f; // Scale from 0 to 1
        public bool includeBuildings = true;
        public bool includeVegetation = true;
        public bool includeRoads = true;
    }

    /// <summary>
    /// Settings for morphology generation (distinct from parameters).
    /// </summary>
    [System.Serializable]
    public class MorphologySettings
    {
        public float nodeMinDistance = 1.0f;
        public float nodeMaxDistance = 5.0f;
        public float connectionProbability = 0.7f; // Used for post-growth connection phase
        public int initialSeedCount = 10;
        public int maxNodes = 1000;
        public float growthSpeed = 1.0f; // General speed factor
        public AnimationCurve growthCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        public float nodeScale = 0.5f;
        public float connectionScale = 0.2f;
    }

    /// <summary>
    /// Settings for scenario analysis.
    /// </summary>
    [System.Serializable]
    public class ScenarioSettings
    {
        public float analysisTimeStep = 0.1f;
        public int maxIterations = 1000;
        public float convergenceThreshold = 0.01f;
    }

    /// <summary>
    /// Parameters controlling a specific morphology generation process.
    /// </summary>
    [System.Serializable]
    public class MorphologyParameters
    {
        // Core Parameters
        public float density = 0.5f;         // Controls target node count relative to volume/maxNodes
        public float complexity = 0.5f;      // Influences branching, detail, etc. (interpretation depends on algorithm)
        public float connectivity = 0.5f;    // Influences how readily nodes connect (interpretation depends on algorithm)
        public float growthRate = 0.5f;      // Controls speed/steps of growth process
        public float adaptationRate = 0.5f;  // Controls speed of adaptation in response to scenarios

        // Type and Pattern
        public BiomorphType biomorphType = BiomorphType.Mold;
        public GrowthPattern growthPattern = GrowthPattern.Organic;

        // Additional potential parameters based on previous definitions
        public float adaptability = 0.5f; // May overlap with adaptationRate
        public float energyEfficiency = 0.5f; // Metric or parameter?
        public float resilience = 0.5f; // Metric or parameter?


        public enum BiomorphType
        {
            Standard, // Default/Placeholder
            Mold,
            Bone,
            Coral,
            Mycelium,
            Organic,     // Added
            Crystalline, // Added
            Fungal,      // Added - Note: May overlap with Mold/Mycelium conceptually
            Hybrid,      // Added
            Custom       // For user-defined behaviors
        }

        public enum GrowthPattern
        {
            Organic,   // Default, less constrained
            Directed,  // Towards a target
            Radial,    // Outwards from center
            Layered,   // Building in layers
            Adaptive   // Responding dynamically to environment during growth
        }
    }

    /// <summary>
    /// Data structure representing a generated morphology instance.
    /// </summary>
    [System.Serializable]
    public class MorphologyData
    {
        public string morphologyId; // Unique ID for this instance
        public List<Vector3> nodePositions;
        public List<int[]> connections; // Pairs of indices into nodePositions
        public Dictionary<string, float> metrics; // e.g., Volume, SurfaceArea, Density
        public MorphologyParameters parametersUsed; // Parameters that generated this
        public System.DateTime generationTime;
        // Consider adding reference to the actual GameObjects/components if needed runtime
    }

    /// <summary>
    /// Data structure for configuring a scenario analysis.
    /// </summary>
    [System.Serializable]
    public class ScenarioData
    {
        public string scenarioName;
        public string description;
        public string targetMorphologyId; // ID of the morphology to analyze (optional, defaults to latest)
        public Dictionary<string, float> environmentalFactors; // e.g., WindSpeed, SunlightIntensity
        public float simulationDuration; // Duration of the scenario simulation
        public bool recordHistory; // Whether to store time-series data
    }
    
    /// <summary>
    /// Results from a scenario analysis run.
    /// </summary>
    [System.Serializable]
    public class ScenarioResults
    {
        public string scenarioId; // Matches ScenarioData name or a unique run ID
        public string morphologyId; // ID of the morphology analyzed
        public Dictionary<string, float> metrics; // Metrics after scenario completion
        public Dictionary<string, List<float>> timeSeriesData; // Recorded history if enabled
        public bool adaptationSuccessful; // Did the morphology adapt as expected?
        public List<string> observations; // Qualitative notes or significant events
    }
    
    // Define GrowthInfluenceMap here if it's broadly used, or keep it where it's most relevant (e.g., GrowthSystem)
    // For now, assuming it exists or will be defined.
    // public class GrowthInfluenceMap { ... } 
}
