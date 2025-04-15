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

    // Added property for current date/time
    public System.DateTime CurrentDateTime
    {
        get { return currentState.dateTime; }
    }
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
    /// Sets the geographic location (latitude and longitude).
    /// </summary>
    public void SetGeographicLocation(float latitude, float longitude)
    {
        this.latitude = latitude;
        this.longitude = longitude;
        if (sunlightSystem != null)
        {
            sunlightSystem.SetLocation(latitude, longitude);
        }
        Debug.Log($"Set geographic location to Lat: {latitude}, Lon: {longitude}");
    }

    /// <summary>
    /// Sets additional location data (placeholder).
    /// </summary>
    public void SetLocationData(string name, float elevation, float urbanDensity)
    {
        // Placeholder: Store or use this data as needed
        Debug.Log($"Set location data - Name: {name}, Elevation: {elevation}, Urban Density: {urbanDensity}");
    }

    /// <summary>
    /// Sets the simulation time scale.
    /// </summary>
    public void SetTimeScale(float scale)
    {
        if (weatherSystem != null) weatherSystem.SetTimeScale(scale);
        if (pedestrianFlowSystem != null) pedestrianFlowSystem.SetTimeScale(scale);
        if (sunlightSystem != null) sunlightSystem.SetTimeScale(scale);
        Debug.Log($"Set time scale to {scale}x");
    }    /// <summary>
    /// Pauses or resumes the environment simulation.
    /// </summary>
    public void SetPaused(bool paused)
    {
        isEnvironmentActive = !paused;
        if (weatherSystem != null)
        {
            // Use StartSimulation/StopSimulation instead of SetPaused
            if (paused)
                weatherSystem.StopSimulation();
            else
                weatherSystem.StartSimulation();
        }
        if (pedestrianFlowSystem != null)
        {
            // Use StartSimulation/StopSimulation instead of SetPaused
            if (paused)
                pedestrianFlowSystem.StopSimulation();
            else
                pedestrianFlowSystem.StartSimulation();
        }
        if (sunlightSystem != null)
        {
            // Use StartSimulation/StopSimulation instead of SetPaused
            if (paused)
                sunlightSystem.StopSimulation();
            else
                sunlightSystem.StartSimulation();
        }
        Debug.Log($"Environment simulation {(paused ? "paused" : "resumed")}");
    }

    /// <summary>
    /// Sets the current date and time for the simulation.
    /// </summary>
    public void SetDateTime(System.DateTime dateTime)
    {
        currentState.dateTime = dateTime;
        if (weatherSystem != null) weatherSystem.SetDateTime(dateTime);
        if (pedestrianFlowSystem != null) pedestrianFlowSystem.SetDateTime(dateTime);
        if (sunlightSystem != null) sunlightSystem.SetDateTime(dateTime);
        Debug.Log($"Set simulation date/time to {dateTime}");
    }

    /// <summary>
    /// Sets manual weather conditions based on individual parameters.
    /// </summary>
    public void SetManualWeather(float temperature, float windSpeed, float humidity, float pedestrianDensity, float sunlightIntensity, bool isRaining, int season)
    {
        // This method seems designed to set multiple factors at once from ScenarioPanel
        // We can update the internal state and potentially the factors dictionary
        SetTemperature(temperature);
        SetWindSpeed(windSpeed);
        SetHumidity(humidity);
        SetPedestrianDensity(pedestrianDensity);
        SetSunlightIntensity(sunlightIntensity);
        SetRaining(isRaining);
        SetSeason(season);

        // Ensure systems reflect manual settings if not using realistic mode
        UpdateEnvironmentSystems();
        Debug.Log("Set manual weather conditions from scenario.");
    }

    /// <summary>
    /// Sets whether to use realistic weather simulation.
    /// </summary>
    public void SetUseRealisticWeather(bool useRealistic)
    {
        useRealisticWeather = useRealistic;
        // We might need separate flags if we want independent control
        useRealisticSunlight = useRealistic;
        useRealisticPedestrianFlow = useRealistic;
        Debug.Log($"Set realistic weather mode to: {useRealistic}");
        if (!useRealistic)
        {
            // If switching to manual, apply current manual values
            UpdateEnvironmentSystems();
        }
    }    /// <summary>
    /// Sets the manual temperature.
    /// </summary>
    public void SetTemperature(float value)
    {
        currentState.temperature = value;
        SetEnvironmentalFactor("Temperature", Mathf.Clamp01((value + 10) / 40f));
        if (!useRealisticWeather && weatherSystem != null) 
        {
            // Create weather state with the updated temperature
            WeatherState state = weatherSystem.GetCurrentWeather();
            state.temperature = value;
            weatherSystem.SetManualWeather(state);
        }
    }

    /// <summary>
    /// Sets the manual wind speed.
    /// </summary>
    public void SetWindSpeed(float value)
    {
        currentState.windSpeed = value;
        SetEnvironmentalFactor("Wind", Mathf.Clamp01(value / 50f));
        if (!useRealisticWeather && weatherSystem != null) 
        {
            // Create weather state with the updated wind speed
            WeatherState state = weatherSystem.GetCurrentWeather();
            state.windSpeed = value;
            weatherSystem.SetManualWeather(state);
        }
    }

    /// <summary>
    /// Sets the manual humidity.
    /// </summary>
    public void SetHumidity(float value)
    {
        currentState.humidity = Mathf.Clamp01(value);
        SetEnvironmentalFactor("Humidity", currentState.humidity);
        if (!useRealisticWeather && weatherSystem != null) weatherSystem.SetManualHumidity(currentState.humidity);
    }

    /// <summary>
    /// Sets the manual pedestrian density.
    /// </summary>
    public void SetPedestrianDensity(float value)
    {
        currentState.pedestrianDensity = Mathf.Clamp01(value);
        SetEnvironmentalFactor("PedestrianDensity", currentState.pedestrianDensity);
        if (!useRealisticPedestrianFlow && pedestrianFlowSystem != null) pedestrianFlowSystem.SetManualDensity(currentState.pedestrianDensity);
    }

    /// <summary>
    /// Sets the manual sunlight intensity.
    /// </summary>
    public void SetSunlightIntensity(float value)
    {
        currentState.sunlightIntensity = Mathf.Clamp01(value);
        SetEnvironmentalFactor("SunlightIntensity", currentState.sunlightIntensity);
        if (!useRealisticSunlight && sunlightSystem != null) sunlightSystem.SetManualIntensity(currentState.sunlightIntensity);
    }

    /// <summary>
    /// Sets whether it is manually raining.
    /// </summary>
    public void SetRaining(bool value)
    {
        currentState.rainIntensity = value ? 0.5f : 0f; // Assuming 0.5 for rain on
        // SetEnvironmentalFactor("RainIntensity", currentState.rainIntensity); // Need to add this factor if used
        if (!useRealisticWeather && weatherSystem != null) weatherSystem.SetManualRaining(value);
    }

    /// <summary>
    /// Sets the manual season (placeholder for potential effects).
    /// </summary>
    public void SetSeason(int value)
    {
        // Placeholder: Store season, potentially affect other factors like temp/sunlight
        Debug.Log($"Set manual season to: {(Season)value}");
        // Example: if (!useRealisticWeather) { /* adjust temp based on season */ }
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
            // Use the new individual SetManual methods if they exist in WeatherSystem
            // Otherwise, construct a WeatherState and use SetManualWeather(WeatherState)
            weatherSystem.SetManualTemperature(currentState.temperature);
            weatherSystem.SetManualWindSpeed(currentState.windSpeed);
            weatherSystem.SetManualHumidity(currentState.humidity);
            weatherSystem.SetManualRaining(currentState.rainIntensity > 0);
            // Assuming WeatherSystem has these methods, otherwise adapt
        }

        if (sunlightSystem != null && !useRealisticSunlight)
        {
            // Use the new individual SetManual methods if they exist in SunlightSimulator
            sunlightSystem.SetManualIntensity(currentState.sunlightIntensity);
            // sunlightSystem.SetManualDirection(...); // Need to handle direction if controlled manually
            // Assuming SunlightSimulator has these methods, otherwise adapt
        }

        if (pedestrianFlowSystem != null && !useRealisticPedestrianFlow)
        {
            // Use the new individual SetManual methods if they exist in PedestrianFlow
            pedestrianFlowSystem.SetManualDensity(currentState.pedestrianDensity);
            // pedestrianFlowSystem.SetManualDirection(...); // Need to handle direction if controlled manually
            // Assuming PedestrianFlow has these methods, otherwise adapt
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