using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using BiomorphicSim.Morphology;
using BiomorphicSim.UI;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Analyzes environmental scenarios and applies forces to the morphology.
    /// Simulates external factors like wind, light, and gravity.
    /// </summary>
    public class ScenarioAnalyzer : MonoBehaviour
    {
        [Header("Scenario Settings")]
        [SerializeField] private float scenarioDuration = 10.0f;
        [SerializeField] private float scenarioIntensity = 1.0f;
        
        [Header("Wind Settings")]
        [SerializeField] private float windSpeed = 5.0f;
        [SerializeField] private Vector3 windDirection = new Vector3(1, 0, 0);
        [SerializeField] private float windGustFrequency = 0.5f;
        [SerializeField] private float windGustIntensity = 1.5f;
        
        [Header("Light Settings")]
        [SerializeField] private Transform sunTransform;
        [SerializeField] private float lightIntensity = 1.0f;
        
        [Header("Visualization")]
        [SerializeField] private GameObject windVectorPrefab;
        [SerializeField] private GameObject lightVectorPrefab;
        
        // References
        private MorphologyManager morphologyManager;
        private UIManager uiManager;
        
        // Internal data
        private Dictionary<string, ScenarioData> scenarios = new Dictionary<string, ScenarioData>();
        private List<GameObject> vectorVisualizations = new List<GameObject>();
        
        // Structure to hold scenario data
        [System.Serializable]
        public class ScenarioData
        {
            public string name;
            public ScenarioType type;
            public Vector3 direction;
            public float intensity;
            public float duration;
            public bool cyclic;
            public float frequency;
            
            // Constructor
            public ScenarioData(string scenarioName, ScenarioType scenarioType, Vector3 scenarioDirection, 
                               float scenarioIntensity, float scenarioDuration, bool isCyclic = false, float cycleFrequency = 1.0f)
            {
                name = scenarioName;
                type = scenarioType;
                direction = scenarioDirection;
                intensity = scenarioIntensity;
                duration = scenarioDuration;
                cyclic = isCyclic;
                frequency = cycleFrequency;
            }
        }
        
        // Types of scenarios
        public enum ScenarioType
        {
            Wind,
            Light,
            Gravity,
            Temperature,
            Custom
        }
        
        public void Initialize()
        {
            Debug.Log("Initializing Scenario Analyzer...");
            
            // Get references
            morphologyManager = FindObjectOfType<MorphologyManager>();
            uiManager = FindObjectOfType<UIManager>();
            
            // Set up default scenarios
            SetupDefaultScenarios();
        }
        
        private void SetupDefaultScenarios()
        {
            // Wind scenario
            scenarios.Add("Wind_East", new ScenarioData(
                "Wind from East", 
                ScenarioType.Wind, 
                new Vector3(1, 0, 0), 
                windSpeed, 
                scenarioDuration, 
                true, 
                windGustFrequency));
                
            // Wind scenario (different direction)
            scenarios.Add("Wind_North", new ScenarioData(
                "Wind from North", 
                ScenarioType.Wind, 
                new Vector3(0, 0, 1), 
                windSpeed, 
                scenarioDuration, 
                true, 
                windGustFrequency));
                
            // Light scenario
            if (sunTransform != null)
            {
                scenarios.Add("Light_Above", new ScenarioData(
                    "Light from Above", 
                    ScenarioType.Light, 
                    sunTransform.forward, 
                    lightIntensity, 
                    scenarioDuration));
            }
            else
            {
                scenarios.Add("Light_Above", new ScenarioData(
                    "Light from Above", 
                    ScenarioType.Light, 
                    new Vector3(0, -1, 0), 
                    lightIntensity, 
                    scenarioDuration));
            }
            
            // Gravity scenario
            scenarios.Add("Gravity_Increase", new ScenarioData(
                "Increased Gravity", 
                ScenarioType.Gravity, 
                Physics.gravity.normalized, 
                Physics.gravity.magnitude * 2, 
                scenarioDuration));
                
            Debug.Log($"Set up {scenarios.Count} default scenarios");
        }
        
        public void RunScenario(string scenarioName)
        {
            if (!scenarios.ContainsKey(scenarioName))
            {
                Debug.LogWarning($"Scenario {scenarioName} not found!");
                return;
            }
            
            ScenarioData scenario = scenarios[scenarioName];
            StartCoroutine(RunScenarioCoroutine(scenario));
        }
        
        private IEnumerator RunScenarioCoroutine(ScenarioData scenario)
        {
            Debug.Log($"Running scenario: {scenario.name}");
            
            // Create visualization
            ClearVisualizations();
            CreateScenarioVisualization(scenario);
            
            // Store original physics values
            Vector3 originalGravity = Physics.gravity;
            
            // Apply scenario modifications
            float elapsedTime = 0f;
            
            while (elapsedTime < scenario.duration)
            {
                float normalizedTime = elapsedTime / scenario.duration;
                float currentIntensity = scenario.intensity;
                
                // Apply cyclic variations if enabled
                if (scenario.cyclic)
                {
                    float cycleFactor = Mathf.Sin(normalizedTime * scenario.frequency * Mathf.PI * 2);
                    currentIntensity *= 1 + cycleFactor * windGustIntensity;
                }
                
                // Apply forces based on scenario type
                switch (scenario.type)
                {
                    case ScenarioType.Wind:
                        ApplyWindForce(scenario.direction, currentIntensity);
                        break;
                        
                    case ScenarioType.Light:
                        ApplyLightForce(scenario.direction, currentIntensity);
                        break;
                        
                    case ScenarioType.Gravity:
                        Physics.gravity = scenario.direction * currentIntensity;
                        break;
                        
                    case ScenarioType.Temperature:
                        ApplyTemperatureEffect(currentIntensity);
                        break;
                        
                    case ScenarioType.Custom:
                        ApplyCustomForce(scenario.direction, currentIntensity);
                        break;
                }
                
                // Update UI with scenario progress
                if (uiManager != null)
                {
                    uiManager.UpdateScenarioProgress(normalizedTime, scenario.name);
                }
                
                // Wait for next frame
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Restore original physics values
            Physics.gravity = originalGravity;
            
            // Clear visualizations
            ClearVisualizations();
            
            Debug.Log($"Scenario {scenario.name} completed");
            
            // Update UI with completion
            if (uiManager != null)
            {
                uiManager.ScenarioCompleted(scenario.name);
            }
        }
        
        private void ApplyWindForce(Vector3 direction, float intensity)
        {
            if (morphologyManager == null)
                return;
                
            Vector3 force = direction.normalized * intensity;
            
            // Apply force to whole structure
            morphologyManager.ApplyExternalForce(Vector3.zero, 1000f, force);
        }
        
        private void ApplyLightForce(Vector3 direction, float intensity)
        {
            if (morphologyManager == null)
                return;
                
            Vector3 force = direction.normalized * intensity * 0.5f;
            
            // Apply light force (phototropism) to whole structure
            morphologyManager.ApplyExternalForce(Vector3.zero, 1000f, force);
        }
        
        private void ApplyTemperatureEffect(float intensity)
        {
            // Temperature implementation would go here
            // This could affect node movement, connection strengths, etc.
        }
        
        private void ApplyCustomForce(Vector3 direction, float intensity)
        {
            if (morphologyManager == null)
                return;
                
            Vector3 force = direction.normalized * intensity;
            
            // Apply custom force to whole structure
            morphologyManager.ApplyExternalForce(Vector3.zero, 1000f, force);
        }
        
        private void CreateScenarioVisualization(ScenarioData scenario)
        {
            // Choose correct prefab
            GameObject prefab = null;
            
            switch (scenario.type)
            {
                case ScenarioType.Wind:
                    prefab = windVectorPrefab;
                    break;
                    
                case ScenarioType.Light:
                    prefab = lightVectorPrefab;
                    break;
                    
                default:
                    return;
            }
            
            if (prefab == null)
                return;
                
            // Create visualization
            GameObject visualization = Instantiate(prefab, Vector3.zero, Quaternion.identity);
            visualization.transform.SetParent(transform);
            
            // Scale and rotate based on direction and intensity
            visualization.transform.forward = scenario.direction;
            visualization.transform.localScale = Vector3.one * Mathf.Clamp(scenario.intensity, 1f, 5f);
            
            // Add to list for tracking
            vectorVisualizations.Add(visualization);
        }
        
        private void ClearVisualizations()
        {
            foreach (GameObject visualization in vectorVisualizations)
            {
                if (visualization != null)
                    Destroy(visualization);
            }
            
            vectorVisualizations.Clear();
        }
        
        public void AddCustomScenario(string name, ScenarioType type, Vector3 direction, float intensity, float duration)
        {
            ScenarioData newScenario = new ScenarioData(name, type, direction, intensity, duration);
            
            if (scenarios.ContainsKey(name))
            {
                scenarios[name] = newScenario;
            }
            else
            {
                scenarios.Add(name, newScenario);
            }
            
            Debug.Log($"Added custom scenario: {name}");
        }
        
        public List<string> GetAvailableScenarios()
        {
            List<string> result = new List<string>();
            
            foreach (var scenarioPair in scenarios)
            {
                result.Add(scenarioPair.Key);
            }
            
            return result;
        }
        
        public void Cleanup()
        {
            ClearVisualizations();
        }
    }
}