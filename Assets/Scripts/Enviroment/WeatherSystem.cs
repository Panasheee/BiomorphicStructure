using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;

/// <summary>
/// Simulates realistic weather conditions for the Wellington Lambton Quay site.
/// Includes wind patterns, temperature variations, and precipitation.
/// </summary>
public class WeatherSystem : MonoBehaviour
{
    #region Properties
    [Header("Weather Data")]
    [SerializeField] private bool useRealWeatherData = true;
    [SerializeField] private WeatherData[] historicalWeatherData;
    [SerializeField] private MonthlyWeatherProfile[] monthlyProfiles;
    
    [Header("Wind Settings")]
    [SerializeField] private Vector3 primaryWindDirection = new Vector3(1, 0, 0.5f).normalized;
    [SerializeField] private float windVariability = 0.3f;
    [SerializeField] private float windGustiness = 0.2f;
    [SerializeField] private AnimationCurve windDailyPattern;
    
    [Header("Temperature Settings")]
    [SerializeField] private float baseDayTemperature = 20.0f;  // Degrees Celsius
    [SerializeField] private float baseNightTemperature = 10.0f; // Degrees Celsius
    [SerializeField] private float temperatureVariability = 3.0f;
    [SerializeField] private AnimationCurve temperatureDailyPattern;
    
    [Header("Rain Settings")]
    [SerializeField] private float baseRainProbability = 0.3f;
    [SerializeField] private float maxRainIntensity = 0.8f;
    [SerializeField] private float rainDuration = 3.0f; // Hours
    
    [Header("Cloud Settings")]
    [SerializeField] private float baseCloudCoverage = 0.4f;
    [SerializeField] private float cloudVariability = 0.3f;
    [SerializeField] private AnimationCurve cloudDailyPattern;
    
    [Header("Visualization")]
    [SerializeField] private ParticleSystem rainParticleSystem;
    [SerializeField] private GameObject cloudObject;
    [SerializeField] private Light sunLight;
    [SerializeField] private Material skyboxMaterial;
    
    // Current weather state
    private WeatherState currentWeather = new WeatherState();
    private DateTime currentDateTime;
    private float timeScale = 1.0f;
    private bool isSimulationRunning = false;
    
    // Weather data cache
    private Dictionary<DateTime, WeatherData> weatherDataCache = new Dictionary<DateTime, WeatherData>();
    
    // Rain state
    private bool isRaining = false;
    private float rainTimer = 0f;
    private float nextRainTime = 0f;
    
    // For tracking time
    private float simulationTimer = 0f;
    private TimeSpan simulationTimeStep = TimeSpan.FromMinutes(10); // Update every 10 sim-minutes
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Set current date/time to now
        currentDateTime = DateTime.Now;
        
        // Initialize default weather state
        InitializeDefaultWeather();
        
        // Set up visualization
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop();
        }
        
        if (cloudObject != null)
        {
            cloudObject.SetActive(false);
        }
    }
    
    private void Update()
    {
        if (!isSimulationRunning) return;
        
        // Advance simulation time
        simulationTimer += Time.deltaTime * timeScale;
        
        // Update weather every timeStep
        if (simulationTimer >= simulationTimeStep.TotalSeconds)
        {
            simulationTimer = 0f;
            currentDateTime = currentDateTime.Add(simulationTimeStep);
            UpdateWeather();
        }
        
        // Update rain state
        if (isRaining)
        {
            rainTimer += Time.deltaTime * timeScale;
            if (rainTimer >= rainDuration * 3600f) // Convert hours to seconds
            {
                StopRain();
            }
        }
        else if (nextRainTime > 0)
        {
            nextRainTime -= Time.deltaTime * timeScale;
            if (nextRainTime <= 0)
            {
                TryStartRain();
            }
        }
        
        // Update visualization every frame
        UpdateVisualization();
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the weather system
    /// </summary>
    public void Initialize()
    {
        // Ensure weather profiles are set up
        if (monthlyProfiles == null || monthlyProfiles.Length < 12)
        {
            InitializeDefaultProfiles();
        }
        
        // Initialize rain system
        ScheduleNextRain();
        
        Debug.Log("Weather system initialized");
    }
    
    /// <summary>
    /// Sets the date and time for the simulation
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    public void SetDateTime(DateTime dateTime)
    {
        currentDateTime = dateTime;
        UpdateWeather();
    }
    
    /// <summary>
    /// Sets the time scale for the simulation
    /// </summary>
    /// <param name="scale">Time scale (1.0 = real-time)</param>
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0.1f, scale);
    }
    
    /// <summary>
    /// Starts the weather simulation
    /// </summary>
    public void StartSimulation()
    {
        isSimulationRunning = true;
        simulationTimer = 0f;
        UpdateWeather();
    }
    
    /// <summary>
    /// Stops the weather simulation
    /// </summary>
    public void StopSimulation()
    {
        isSimulationRunning = false;
    }
    
    /// <summary>
    /// Gets the current weather state
    /// </summary>
    /// <returns>Current weather state</returns>
    public WeatherState GetCurrentWeather()
    {
        return currentWeather;
    }
    
    /// <summary>
    /// Gets the weather forecast for a future time
    /// </summary>
    /// <param name="hoursAhead">Hours into the future</param>
    /// <returns>Forecasted weather state</returns>
    public WeatherState GetWeatherForecast(float hoursAhead)
    {
        DateTime forecastTime = currentDateTime.AddHours(hoursAhead);
        
        if (useRealWeatherData && TryGetRealWeatherData(forecastTime, out WeatherData data))
        {
            return ConvertToWeatherState(data);
        }
        
        // Fall back to simulation
        return SimulateWeather(forecastTime);
    }
    
    /// <summary>
    /// Loads weather data from a file
    /// </summary>
    /// <param name="dataFile">Text asset containing weather data</param>
    public void LoadWeatherData(TextAsset dataFile)
    {
        if (dataFile == null)
        {
            Debug.LogError("No weather data file provided");
            return;
        }
        
        try
        {
            string[] lines = dataFile.text.Split('\n');
            List<WeatherData> loadedData = new List<WeatherData>();
            
            // Skip header line if present
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                string[] values = line.Split(',');
                if (values.Length < 6) continue; // Need at least date, time, temp, wind speed, direction, humidity
                
                try
                {
                    WeatherData entry = new WeatherData();
                    
                    // Parse date and time
                    string dateStr = values[0].Trim();
                    string timeStr = values[1].Trim();
                    DateTime dateTime = DateTime.Parse($"{dateStr} {timeStr}");
                    entry.timestamp = dateTime;
                    
                    // Parse other values
                    entry.temperature = float.Parse(values[2].Trim());
                    entry.windSpeed = float.Parse(values[3].Trim());
                    
                    float windDirection = float.Parse(values[4].Trim());
                    entry.windDirection = Quaternion.Euler(0, windDirection, 0) * Vector3.forward;
                    
                    entry.humidity = float.Parse(values[5].Trim());
                    
                    // Optional values
                    if (values.Length > 6)
                    {
                        entry.rainIntensity = float.Parse(values[6].Trim());
                    }
                    
                    if (values.Length > 7)
                    {
                        entry.cloudCoverage = float.Parse(values[7].Trim());
                    }
                    
                    loadedData.Add(entry);
                    
                    // Add to cache
                    DateTime key = new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute / 10 * 10, // Round to 10-minute increments
                        0
                    );
                    
                    weatherDataCache[key] = entry;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error parsing weather data line {i}: {e.Message}");
                }
            }
            
            historicalWeatherData = loadedData.ToArray();
            Debug.Log($"Loaded {loadedData.Count} weather data entries");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading weather data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Sets manual weather settings (for non-realistic mode)
    /// </summary>
    /// <param name="weather">Weather state to set</param>
    public void SetManualWeather(WeatherState weather)
    {
        currentWeather = weather;
        
        // Update visualization
        UpdateVisualization();
        
        // Update rain state
        if (weather.rainIntensity > 0.1f && !isRaining)
        {
            StartRain(weather.rainIntensity);
        }
        else if (weather.rainIntensity <= 0.1f && isRaining)
        {
            StopRain();
        }
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes the default weather state
    /// </summary>
    private void InitializeDefaultWeather()
    {
        currentWeather = new WeatherState
        {
            temperature = 20.0f,
            windSpeed = 5.0f,
            windDirection = primaryWindDirection,
            humidity = 0.5f,
            rainIntensity = 0.0f,
            cloudCoverage = 0.2f
        };
    }
    
    /// <summary>
    /// Initializes default monthly profiles for Wellington
    /// </summary>
    private void InitializeDefaultProfiles()
    {
        // Wellington monthly averages
        monthlyProfiles = new MonthlyWeatherProfile[12];
        
        // Summer (December - February in Southern Hemisphere)
        monthlyProfiles[11] = new MonthlyWeatherProfile // December
        {
            averageDayTemp = 20.0f,
            averageNightTemp = 14.0f,
            averageWindSpeed = 22.0f,
            rainProbability = 0.25f,
            averageCloudCover = 0.4f,
            humidity = 0.7f
        };
        
        monthlyProfiles[0] = new MonthlyWeatherProfile // January
        {
            averageDayTemp = 21.0f,
            averageNightTemp = 15.0f,
            averageWindSpeed = 20.0f,
            rainProbability = 0.2f,
            averageCloudCover = 0.3f,
            humidity = 0.65f
        };
        
        monthlyProfiles[1] = new MonthlyWeatherProfile // February
        {
            averageDayTemp = 21.0f,
            averageNightTemp = 15.0f,
            averageWindSpeed = 19.0f,
            rainProbability = 0.2f,
            averageCloudCover = 0.3f,
            humidity = 0.65f
        };
        
        // Autumn (March - May)
        monthlyProfiles[2] = new MonthlyWeatherProfile // March
        {
            averageDayTemp = 19.0f,
            averageNightTemp = 13.0f,
            averageWindSpeed = 20.0f,
            rainProbability = 0.3f,
            averageCloudCover = 0.4f,
            humidity = 0.7f
        };
        
        monthlyProfiles[3] = new MonthlyWeatherProfile // April
        {
            averageDayTemp = 17.0f,
            averageNightTemp = 11.0f,
            averageWindSpeed = 19.0f,
            rainProbability = 0.35f,
            averageCloudCover = 0.5f,
            humidity = 0.75f
        };
        
        monthlyProfiles[4] = new MonthlyWeatherProfile // May
        {
            averageDayTemp = 14.0f,
            averageNightTemp = 9.0f,
            averageWindSpeed = 20.0f,
            rainProbability = 0.4f,
            averageCloudCover = 0.6f,
            humidity = 0.8f
        };
        
        // Winter (June - August)
        monthlyProfiles[5] = new MonthlyWeatherProfile // June
        {
            averageDayTemp = 12.0f,
            averageNightTemp = 7.0f,
            averageWindSpeed = 22.0f,
            rainProbability = 0.45f,
            averageCloudCover = 0.7f,
            humidity = 0.85f
        };
        
        monthlyProfiles[6] = new MonthlyWeatherProfile // July
        {
            averageDayTemp = 11.0f,
            averageNightTemp = 6.0f,
            averageWindSpeed = 23.0f,
            rainProbability = 0.5f,
            averageCloudCover = 0.7f,
            humidity = 0.85f
        };
        
        monthlyProfiles[7] = new MonthlyWeatherProfile // August
        {
            averageDayTemp = 11.0f,
            averageNightTemp = 6.0f,
            averageWindSpeed = 24.0f,
            rainProbability = 0.5f,
            averageCloudCover = 0.7f,
            humidity = 0.85f
        };
        
        // Spring (September - November)
        monthlyProfiles[8] = new MonthlyWeatherProfile // September
        {
            averageDayTemp = 13.0f,
            averageNightTemp = 8.0f,
            averageWindSpeed = 23.0f,
            rainProbability = 0.4f,
            averageCloudCover = 0.6f,
            humidity = 0.8f
        };
        
        monthlyProfiles[9] = new MonthlyWeatherProfile // October
        {
            averageDayTemp = 15.0f,
            averageNightTemp = 10.0f,
            averageWindSpeed = 22.0f,
            rainProbability = 0.35f,
            averageCloudCover = 0.5f,
            humidity = 0.75f
        };
        
        monthlyProfiles[10] = new MonthlyWeatherProfile // November
        {
            averageDayTemp = 17.0f,
            averageNightTemp = 11.0f,
            averageWindSpeed = 21.0f,
            rainProbability = 0.3f,
            averageCloudCover = 0.4f,
            humidity = 0.7f
        };
    }
    
    /// <summary>
    /// Updates the current weather based on time
    /// </summary>
    private void UpdateWeather()
    {
        // Get weather data from real data if available
        if (useRealWeatherData && TryGetRealWeatherData(currentDateTime, out WeatherData data))
        {
            currentWeather = ConvertToWeatherState(data);
        }
        else
        {
            // Fall back to simulation
            currentWeather = SimulateWeather(currentDateTime);
        }
        
        // Check if we should start/stop rain
        if (currentWeather.rainIntensity > 0.1f && !isRaining)
        {
            StartRain(currentWeather.rainIntensity);
        }
        else if (currentWeather.rainIntensity <= 0.1f && isRaining)
        {
            StopRain();
        }
    }
    
    /// <summary>
    /// Tries to get real weather data for a given time
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <param name="data">Output weather data</param>
    /// <returns>True if data was found</returns>
    private bool TryGetRealWeatherData(DateTime dateTime, out WeatherData data)
    {
        // Round to the nearest 10 minutes for cache lookup
        DateTime key = new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute / 10 * 10,
            0
        );
        
        // Try to get from cache
        if (weatherDataCache.TryGetValue(key, out data))
        {
            return true;
        }
        
        // Try to find nearest entry in historical data
        if (historicalWeatherData != null && historicalWeatherData.Length > 0)
        {
            WeatherData nearest = historicalWeatherData[0];
            TimeSpan minDiff = TimeSpan.MaxValue;
            
            foreach (var entry in historicalWeatherData)
            {
                TimeSpan diff = entry.timestamp - dateTime;
                if (diff.Duration() < minDiff)
                {
                    minDiff = diff.Duration();
                    nearest = entry;
                }
            }
            
            // If within 30 minutes, use it
            if (minDiff.TotalMinutes <= 30)
            {
                data = nearest;
                
                // Cache this result
                weatherDataCache[key] = data;
                
                return true;
            }
        }
        
        // No suitable data found
        data = default;
        return false;
    }
    
    /// <summary>
    /// Simulates weather based on time and monthly profiles
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <returns>Simulated weather state</returns>
    private WeatherState SimulateWeather(DateTime dateTime)
    {
        // Get monthly profile
        MonthlyWeatherProfile profile = monthlyProfiles[dateTime.Month - 1];
        
        // Calculate time of day factor (0-1)
        float hourOfDay = dateTime.Hour + dateTime.Minute / 60f;
        float dayFactor = Mathf.Clamp01(hourOfDay / 24f);
        
        // Temperature varies throughout the day
        float temperatureFactor = temperatureDailyPattern.Evaluate(dayFactor);
        float temperature = Mathf.Lerp(profile.averageNightTemp, profile.averageDayTemp, temperatureFactor);
        
        // Add some daily variation
        int dayHash = dateTime.Day + dateTime.Month * 31 + dateTime.Year * 31 * 12;
        System.Random rng = new System.Random(dayHash);
        float dailyVariation = (float)rng.NextDouble() * 2f - 1f; // -1 to 1
        temperature += dailyVariation * temperatureVariability;
        
        // Wind speed varies throughout the day
        float windFactor = windDailyPattern.Evaluate(dayFactor);
        float windSpeed = profile.averageWindSpeed * (0.7f + windFactor * 0.6f);
        
        // Add gusts
        float gustFactor = Mathf.PerlinNoise(dateTime.Minute / 60f, dateTime.Hour / 24f);
        windSpeed *= 1.0f + gustFactor * windGustiness;
        
        // Wind direction
        Vector3 windDirection = CalculateWindDirection(dateTime);
        
        // Humidity
        float humidity = profile.humidity;
        
        // Cloud coverage varies throughout the day
        float cloudFactor = cloudDailyPattern.Evaluate(dayFactor);
        float cloudCoverage = profile.averageCloudCover * (0.8f + cloudFactor * 0.4f);
        
        // Add daily variation
        cloudCoverage += (float)rng.NextDouble() * cloudVariability * 2f - cloudVariability;
        cloudCoverage = Mathf.Clamp01(cloudCoverage);
        
        // Rain intensity (mostly determined by weather system events)
        float rainIntensity = isRaining ? currentWeather.rainIntensity : 0f;
        
        // Create weather state
        return new WeatherState
        {
            temperature = temperature,
            windSpeed = windSpeed,
            windDirection = windDirection,
            humidity = humidity,
            rainIntensity = rainIntensity,
            cloudCoverage = cloudCoverage
        };
    }
    
    /// <summary>
    /// Calculates wind direction based on time
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <returns>Wind direction vector</returns>
    private Vector3 CalculateWindDirection(DateTime dateTime)
    {
        // Base direction
        Vector3 baseDirection = primaryWindDirection;
        
        // Add time-based variation
        float hourVariation = Mathf.Sin(dateTime.Hour / 24f * Mathf.PI * 2f) * windVariability;
        float dayVariation = Mathf.Cos(dateTime.Day / 30f * Mathf.PI * 2f) * windVariability;
        
        // Create rotation
        Quaternion rotation = Quaternion.Euler(0, hourVariation * 30f + dayVariation * 20f, 0);
        
        return rotation * baseDirection;
    }
    
    /// <summary>
    /// Converts weather data to weather state
    /// </summary>
    /// <param name="data">Weather data</param>
    /// <returns>Weather state</returns>
    private WeatherState ConvertToWeatherState(WeatherData data)
    {
        return new WeatherState
        {
            temperature = data.temperature,
            windSpeed = data.windSpeed,
            windDirection = data.windDirection,
            humidity = data.humidity,
            rainIntensity = data.rainIntensity,
            cloudCoverage = data.cloudCoverage
        };
    }
    
    /// <summary>
    /// Schedules the next rain event
    /// </summary>
    private void ScheduleNextRain()
    {
        if (isRaining) return;
        
        // Get monthly profile
        MonthlyWeatherProfile profile = monthlyProfiles[currentDateTime.Month - 1];
        
        // Calculate hours until next rain
        float rainProbabilityPerHour = profile.rainProbability / 24f;
        
        // Higher probability when clouds are present
        rainProbabilityPerHour *= 1f + currentWeather.cloudCoverage;
        
        // Random time until next rain check
        float hoursUntilRainCheck = -Mathf.Log(1f - UnityEngine.Random.value) / rainProbabilityPerHour;
        
        // Set next rain time (in seconds)
        nextRainTime = hoursUntilRainCheck * 3600f;
        
        Debug.Log($"Next rain check in {hoursUntilRainCheck:F1} hours");
    }
    
    /// <summary>
    /// Attempts to start a rain event
    /// </summary>
    private void TryStartRain()
    {
        // Get monthly profile
        MonthlyWeatherProfile profile = monthlyProfiles[currentDateTime.Month - 1];
        
        // Higher chance when cloudy
        float rainChance = profile.rainProbability * (1f + currentWeather.cloudCoverage);
        
        if (UnityEngine.Random.value < rainChance)
        {
            // Determine intensity
            float intensity = UnityEngine.Random.Range(0.2f, maxRainIntensity);
            
            // Start rain
            StartRain(intensity);
        }
        else
        {
            // Try again later
            ScheduleNextRain();
        }
    }
    
    /// <summary>
    /// Starts a rain event
    /// </summary>
    /// <param name="intensity">Rain intensity (0-1)</param>
    private void StartRain(float intensity)
    {
        isRaining = true;
        rainTimer = 0f;
        
        // Set intensity in weather state
        currentWeather.rainIntensity = intensity;
        
        // Increase cloud coverage
        currentWeather.cloudCoverage = Mathf.Max(currentWeather.cloudCoverage, 0.5f + intensity * 0.3f);
        
        // Show rain visualization
        if (rainParticleSystem != null)
        {
            var emission = rainParticleSystem.emission;
            emission.rateOverTime = 1000 * intensity;
            rainParticleSystem.Play();
        }
        
        Debug.Log($"Rain started with intensity {intensity:F2}");
    }
    
    /// <summary>
    /// Stops the current rain event
    /// </summary>
    private void StopRain()
    {
        isRaining = false;
        
        // Clear rain from weather state
        currentWeather.rainIntensity = 0f;
        
        // Decrease cloud coverage slightly
        currentWeather.cloudCoverage *= 0.8f;
        
        // Stop rain visualization
        if (rainParticleSystem != null)
        {
            rainParticleSystem.Stop();
        }
        
        // Schedule next rain
        ScheduleNextRain();
        
        Debug.Log("Rain stopped");
    }
    
    /// <summary>
    /// Updates the visual representation of the weather
    /// </summary>
    private void UpdateVisualization()
    {
        // Update clouds
        if (cloudObject != null)
        {
            cloudObject.SetActive(currentWeather.cloudCoverage > 0.3f);
            
            // Scale cloud size with coverage
            float scale = 1.0f + currentWeather.cloudCoverage * 0.5f;
            cloudObject.transform.localScale = new Vector3(scale, 1.0f, scale);
            
            // Adjust opacity
            Renderer renderer = cloudObject.GetComponent<Renderer>();
            if (renderer != null && renderer.material != null)
            {
                Color color = renderer.material.color;
                color.a = currentWeather.cloudCoverage * 0.8f;
                renderer.material.color = color;
            }
        }
        
        // Update sun light
        if (sunLight != null)
        {
            // Dim light based on cloud coverage
            float intensity = 1.0f - currentWeather.cloudCoverage * 0.7f;
            sunLight.intensity = intensity;
            
            // Adjust color temperature based on time of day and weather
            float dayFactor = Mathf.Clamp01((currentDateTime.Hour + currentDateTime.Minute / 60f - 6) / 12f);
            float temperature = Mathf.Lerp(2000, 6500, dayFactor); // 2000K (dawn/dusk) to 6500K (midday)
            
            // Clouds make light bluer
            temperature += currentWeather.cloudCoverage * 1000;
            
            sunLight.colorTemperature = temperature;
        }
        
        // Update skybox
        if (skyboxMaterial != null)
        {
            // Adjust sky tint based on cloud coverage
            Color skyTint = Color.Lerp(
                new Color(0.5f, 0.7f, 1.0f), // Clear blue
                new Color(0.7f, 0.7f, 0.7f), // Cloudy gray
                currentWeather.cloudCoverage
            );
            
            skyboxMaterial.SetColor("_SkyTint", skyTint);
            
            // Adjust atmosphere thickness
            float thickness = 1.0f + currentWeather.cloudCoverage * 0.5f;
            skyboxMaterial.SetFloat("_AtmosphereThickness", thickness);
        }
    }
    #endregion
}

/// <summary>
/// Stores historical weather data
/// </summary>
[System.Serializable]
public struct WeatherData
{
    public DateTime timestamp;
    public float temperature;     // Degrees Celsius
    public float windSpeed;       // km/h
    public Vector3 windDirection; // Normalized direction vector
    public float humidity;        // 0-1 range
    public float rainIntensity;   // 0-1 range
    public float cloudCoverage;   // 0-1 range
}

/// <summary>
/// Represents current weather conditions
/// </summary>
[System.Serializable]
public struct WeatherState
{
    public float temperature;     // Degrees Celsius
    public float windSpeed;       // km/h
    public Vector3 windDirection; // Normalized direction vector
    public float humidity;        // 0-1 range
    public float rainIntensity;   // 0-1 range
    public float cloudCoverage;   // 0-1 range
}

/// <summary>
/// Weather profile for a month
/// </summary>
[System.Serializable]
public struct MonthlyWeatherProfile
{
    public float averageDayTemp;    // Degrees Celsius
    public float averageNightTemp;  // Degrees Celsius
    public float averageWindSpeed;  // km/h
    public float rainProbability;   // 0-1 chance of rain per day
    public float averageCloudCover; // 0-1 average cloud coverage
    public float humidity;          // 0-1 average humidity
}