using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central manager for all environmental factors affecting the morphology.
/// Coordinates weather, pedestrian flow, sunlight, and other environmental systems.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    #region Properties
    [Header("Environment Systems")]
    [SerializeField] private WeatherSystem weatherSystem;
    [SerializeField] private PedestrianFlow pedestrianFlowSystem;
    [SerializeField] private SunlightSimulator sunlightSystem;
    
    [Header("Environment Settings")]
    [SerializeField] private bool useRealisticWeather = true;
    [SerializeField] private bool useRealisticSunlight = true;
    [SerializeField] private bool useRealisticPedestrianFlow = true;
    [SerializeField] private float environmentUpdateInterval = 1.0f;
    
    [Header("Data Sources")]
    [SerializeField] private TextAsset weatherDataFile;
    [SerializeField] private TextAsset pedestrianDataFile;
    [SerializeField] private float latitude = -41.2865f;  // Wellington latitude
    [SerializeField] private float longitude = 174.7762f; // Wellington longitude
    
    // Current environment state
    private EnvironmentState currentState = new EnvironmentState();
    private Dictionary<string, float> environmentalFactors = new Dictionary<string, float>();
    
    // Update tracking
    private float lastUpdateTime = 0f;
    private bool isEnvironmentActive = false;
    
    // Event for environment changes
    public delegate void EnvironmentUpdatedHandler(Dictionary<string, float> factors);
    public event EnvironmentUpdatedHandler OnEnvironmentUpdated;
    
    // Singleton instance
    public static EnvironmentManager Instance { get; private set; }
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        
        // Find references if not set
        if (weatherSystem == null)
        {
            weatherSystem = GetComponentInChildren<WeatherSystem>();
        }
        
        if (pedestrianFlowSystem == null)
        {
            pedestrianFlowSystem = GetComponentInChildren<PedestrianFlow>();
        }
        
        if (sunlightSystem == null)
        {
            sunlightSystem = GetComponentInChildren<SunlightSimulator>();
        }
        
        // Initialize environment factors
        InitializeEnvironmentalFactors();
    }
    
    private void Start()
    {
        // Initialize systems
        if (weatherSystem != null)
        {
            if (weatherDataFile != null)
            {
                weatherSystem.LoadWeatherData(weatherDataFile);
            }
            weatherSystem.Initialize();
        }
        
        if (pedestrianFlowSystem != null)
        {
            if (pedestrianDataFile != null)
            {
                pedestrianFlowSystem.LoadPedestrianData(pedestrianDataFile);
            }
            pedestrianFlowSystem.Initialize();
        }
        
        if (sunlightSystem != null)
        {
            sunlightSystem.SetLocation(latitude, longitude);
            sunlightSystem.Initialize();
        }
    }
    
    private void Update()
    {
        if (!isEnvironmentActive) return;
        
        // Update environment at specified interval
        if (Time.time - lastUpdateTime >= environmentUpdateInterval)
        {
            UpdateEnvironment();
            lastUpdateTime = Time.time;
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Starts the environment simulation
    /// </summary>
    /// <param name="startDateTime">Starting date and time for the simulation</param>
    /// <param name="timeScale">Speed of time passage (1.0 = real-time)</param>
    public void StartEnvironmentSimulation(System.DateTime startDateTime, float timeScale = 1.0f)
    {
        // Set date/time on all systems
        if (weatherSystem != null)
        {
            weatherSystem.SetDateTime(startDateTime);
            weatherSystem.SetTimeScale(timeScale);
        }
        
        if (pedestrianFlowSystem != null)
        {
            pedestrianFlowSystem.SetDateTime(startDateTime);
            pedestrianFlowSystem.SetTimeScale(timeScale);
        }
        
        if (sunlightSystem != null)
        {
            sunlightSystem.SetDateTime(startDateTime);
            sunlightSystem.SetTimeScale(timeScale);
        }
        
        isEnvironmentActive = true;
        lastUpdateTime = Time.time;
        
        Debug.Log($"Environment simulation started at {startDateTime}, time scale: {timeScale}x");
    }
    
    /// <summary>
    /// Stops the environment simulation
    /// </summary>
    public void StopEnvironmentSimulation()
    {
        isEnvironmentActive = false;
        
        // Stop all systems
        if (weatherSystem != null)
        {
            weatherSystem.StopSimulation();
        }
        
        if (pedestrianFlowSystem != null)
        {
            pedestrianFlowSystem.StopSimulation();
        }
        
        if (sunlightSystem != null)
        {
            sunlightSystem.StopSimulation();
        }
        
        Debug.Log("Environment simulation stopped");
    }
    
    /// <summary>
    /// Sets a specific environmental factor manually
    /// </summary>
    /// <param name="factorName">Name of the environmental factor</param>
    /// <param name="value">New value (typically 0-1 range)</param>
    public void SetEnvironmentalFactor(string factorName, float value)
    {
        if (environmentalFactors.ContainsKey(factorName))
        {
            environmentalFactors[factorName] = value;
            Debug.Log($"Set environmental factor {factorName} to {value}");
        }
        else
        {
            environmentalFactors.Add(factorName, value);
            Debug.Log($"Added new environmental factor {factorName} with value {value}");
        }
        
        // Notify listeners
        OnEnvironmentUpdated?.Invoke(new Dictionary<string, float>(environmentalFactors));
    }
    
    /// <summary>
    /// Gets the current value of an environmental factor
    /// </summary>
    /// <param name="factorName">Name of the factor</param>
    /// <returns>Current value, or 0 if not found</returns>
    public float GetEnvironmentalFactor(string factorName)
    {
        if (environmentalFactors.TryGetValue(factorName, out float value))
        {
            return value;
        }
        return 0f;
    }
    
    /// <summary>
    /// Gets all current environmental factors
    /// </summary>
    /// <returns>Dictionary of all environmental factors</returns>
    public Dictionary<string, float> GetAllEnvironmentalFactors()
    {
        return new Dictionary<string, float>(environmentalFactors);
    }
    
    /// <summary>
    /// Gets the current environment state
    /// </summary>
    /// <returns>Current environment state</returns>
    public EnvironmentState GetCurrentState()
    {
        return currentState;
    }
    
    /// <summary>
    /// Sets the environment state from a scenario
    /// </summary>
    /// <param name="scenarioData">Scenario data containing environmental factors</param>
    public void SetEnvironmentFromScenario(ScenarioData scenarioData)
    {
        if (scenarioData.environmentalFactors != null)
        {
            // Clear existing factors
            environmentalFactors.Clear();
            
            // Add scenario factors
            foreach (var factor in scenarioData.environmentalFactors)
            {
                environmentalFactors[factor.Key] = factor.Value;
            }
            
            // Update systems
            UpdateEnvironmentSystems();
            
            // Notify listeners
            OnEnvironmentUpdated?.Invoke(new Dictionary<string, float>(environmentalFactors));
            
            Debug.Log($"Environment set from scenario: {scenarioData.scenarioName}");
        }
    }
    
    /// <summary>
    /// Toggle between realistic and manual environment control
    /// </summary>
    /// <param name="useRealistic">Whether to use realistic simulation</param>
    public void SetRealisticEnvironment(bool useRealistic)
    {
        useRealisticWeather = useRealistic;
        useRealisticSunlight = useRealistic;
        useRealisticPedestrianFlow = useRealistic;
        
        Debug.Log($"Set environment to {(useRealistic ? "realistic" : "manual")} mode");
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes default environmental factors
    /// </summary>
    private void InitializeEnvironmentalFactors()
    {
        environmentalFactors.Clear();
        
        // Add default factors
        environmentalFactors["Wind"] = 0.2f;
        environmentalFactors["Temperature"] = 0.5f;
        environmentalFactors["Humidity"] = 0.5f;
        environmentalFactors["SunlightIntensity"] = 0.5f;
        environmentalFactors["SunlightDirection"] = 0.5f;
        environmentalFactors["PedestrianDensity"] = 0.3f;
        environmentalFactors["PedestrianDirection"] = 0.5f;
        environmentalFactors["Gravity"] = 1.0f; // Always on at full strength
        
        // Initialize current state
        currentState = new EnvironmentState
        {
            dateTime = System.DateTime.Now,
            temperature = 20f, // degrees Celsius
            windSpeed = 5f,    // km/h
            windDirection = new Vector3(1, 0, 0),
            humidity = 0.5f,
            sunlightIntensity = 0.5f,
            sunlightDirection = new Vector3(0.5f, -1, 0.5f),
            pedestrianDensity = 0.3f,
            pedestrianFlowDirection = new Vector3(1, 0, 0),
            rainIntensity = 0f,
            cloudCoverage = 0.2f
        };
    }
    
    /// <summary>
    /// Updates the environment based on all systems
    /// </summary>
    private void UpdateEnvironment()
    {
        // Update current state from systems
        if (weatherSystem != null && useRealisticWeather)
        {
            WeatherState weatherState = weatherSystem.GetCurrentWeather();
            currentState.temperature = weatherState.temperature;
            currentState.windSpeed = weatherState.windSpeed;
            currentState.windDirection = weatherState.windDirection;
            currentState.humidity = weatherState.humidity;
            currentState.rainIntensity = weatherState.rainIntensity;
            currentState.cloudCoverage = weatherState.cloudCoverage;
            
            // Update environmental factors from weather
            environmentalFactors["Wind"] = Mathf.Clamp01(weatherState.windSpeed / 50f); // Normalize to 0-1 range
            environmentalFactors["Temperature"] = Mathf.Clamp01((weatherState.temperature + 10) / 40f); // -10 to 30 C range
            environmentalFactors["Humidity"] = weatherState.humidity;
        }
        
        if (sunlightSystem != null && useRealisticSunlight)
        {
            SunlightState sunlightState = sunlightSystem.GetCurrentSunlight();
            currentState.sunlightIntensity = sunlightState.intensity;
            currentState.sunlightDirection = sunlightState.direction;
            currentState.dateTime = sunlightState.dateTime;
            
            // Update environmental factors from sunlight
            environmentalFactors["SunlightIntensity"] = sunlightState.intensity;
            environmentalFactors["SunlightDirection"] = (Vector3.Angle(sunlightState.direction, Vector3.down) / 180f);
        }
        
        if (pedestrianFlowSystem != null && useRealisticPedestrianFlow)
        {
            PedestrianState pedestrianState = pedestrianFlowSystem.GetCurrentPedestrianFlow();
            currentState.pedestrianDensity = pedestrianState.density;
            currentState.pedestrianFlowDirection = pedestrianState.flowDirection;
            
            // Update environmental factors from pedestrian flow
            environmentalFactors["PedestrianDensity"] = pedestrianState.density;
            environmentalFactors["PedestrianDirection"] = (Vector3.Angle(pedestrianState.flowDirection, Vector3.right) / 180f);
        }
        
        // Always update gravity (constant)
        environmentalFactors["Gravity"] = 1.0f;
        
        // Notify listeners
        OnEnvironmentUpdated?.Invoke(new Dictionary<string, float>(environmentalFactors));
    }
    
    /// <summary>
    /// Updates environment systems based on current factors
    /// </summary>
    private void UpdateEnvironmentSystems()
    {
        // Only update systems when in manual mode
        
        if (weatherSystem != null && !useRealisticWeather)
        {
            WeatherState weatherState = new WeatherState();
            
            if (environmentalFactors.TryGetValue("Wind", out float windValue))
            {
                weatherState.windSpeed = windValue * 50f; // Convert 0-1 to 0-50 km/h
            }
            
            if (environmentalFactors.TryGetValue("Temperature", out float tempValue))
            {
                weatherState.temperature = tempValue * 40f - 10f; // Convert 0-1 to -10 to 30 C
            }
            
            if (environmentalFactors.TryGetValue("Humidity", out float humidityValue))
            {
                weatherState.humidity = humidityValue;
            }
            
            weatherSystem.SetManualWeather(weatherState);
        }
        
        if (sunlightSystem != null && !useRealisticSunlight)
        {
            SunlightState sunlightState = new SunlightState();
            
            if (environmentalFactors.TryGetValue("SunlightIntensity", out float intensityValue))
            {
                sunlightState.intensity = intensityValue;
            }
            
            if (environmentalFactors.TryGetValue("SunlightDirection", out float directionValue))
            {
                float angle = directionValue * 180f;
                sunlightState.direction = Quaternion.Euler(angle, 0, 0) * Vector3.down;
            }
            
            sunlightSystem.SetManualSunlight(sunlightState);
        }
        
        if (pedestrianFlowSystem != null && !useRealisticPedestrianFlow)
        {
            PedestrianState pedestrianState = new PedestrianState();
            
            if (environmentalFactors.TryGetValue("PedestrianDensity", out float densityValue))
            {
                pedestrianState.density = densityValue;
            }
            
            if (environmentalFactors.TryGetValue("PedestrianDirection", out float directionValue))
            {
                float angle = directionValue * 180f;
                pedestrianState.flowDirection = Quaternion.Euler(0, angle, 0) * Vector3.right;
            }
            
            pedestrianFlowSystem.SetManualPedestrianFlow(pedestrianState);
        }
    }
    #endregion
}

/// <summary>
/// Represents the complete environmental state
/// </summary>
[System.Serializable]
public class EnvironmentState
{
    public System.DateTime dateTime;
    public float temperature;        // Degrees Celsius
    public float windSpeed;          // km/h
    public Vector3 windDirection;    // Normalized direction vector
    public float humidity;           // 0-1 range
    public float sunlightIntensity;  // 0-1 range
    public Vector3 sunlightDirection;// Normalized direction vector
    public float pedestrianDensity;  // 0-1 range
    public Vector3 pedestrianFlowDirection; // Normalized direction vector
    public float rainIntensity;      // 0-1 range
    public float cloudCoverage;      // 0-1 range
}