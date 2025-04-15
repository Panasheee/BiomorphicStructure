using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// Simulates realistic sunlight conditions for Wellington Lambton Quay.
/// Calculates sun position based on time, date, and geographic coordinates.
/// </summary>
public class SunlightSimulator : MonoBehaviour
{
    #region Properties
    [Header("Location Settings")]
    [SerializeField] private float latitude = -41.2865f;  // Wellington latitude (negative for Southern Hemisphere)
    [SerializeField] private float longitude = 174.7762f; // Wellington longitude
    [SerializeField] private int timeZoneOffset = 12;     // Wellington timezone (UTC+12)
    
    [Header("Sunlight Settings")]
    [SerializeField] private Light sunLight;
    [SerializeField] private Light moonLight;
    [SerializeField] private Material skyboxMaterial;
    [SerializeField] private float sunIntensityMultiplier = 1.0f;
    [SerializeField] private AnimationCurve sunIntensityCurve;
    
    [Header("Shadow Settings")]
    [SerializeField] private float shadowDistanceDay = 100f;
    [SerializeField] private float shadowDistanceNight = 50f;
    [SerializeField] private bool enableDynamicShadows = true;
    
    // Current state
    private DateTime currentDateTime;
    private SunlightState currentState = new SunlightState();
    private float timeScale = 1.0f;
    private bool isSimulationRunning = false;
    
    // For tracking time
    private float simulationTimer = 0f;
    private TimeSpan simulationTimeStep = TimeSpan.FromMinutes(1); // Update every minute
    
    // Sun position calculation cache
    private struct DatePosition { public DateTime date; public Vector3 position; }
    private List<DatePosition> positionCache = new List<DatePosition>();
    private int maxCacheSize = 1440; // Store up to 24 hours of minute-by-minute data
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Set current date/time to now
        currentDateTime = DateTime.Now;
        
        // Find sun light if not assigned
        if (sunLight == null)
        {
            sunLight = GetComponent<Light>();
            
            if (sunLight == null)
            {
                GameObject sunObject = new GameObject("Sun Light");
                sunObject.transform.parent = transform;
                sunLight = sunObject.AddComponent<Light>();
                sunLight.type = LightType.Directional;
                sunLight.shadows = LightShadows.Soft;
            }
        }
        
        // Initialize moon light if not assigned
        if (moonLight == null && sunLight != null)
        {
            GameObject moonObject = new GameObject("Moon Light");
            moonObject.transform.parent = transform;
            moonLight = moonObject.AddComponent<Light>();
            moonLight.type = LightType.Directional;
            moonLight.shadows = LightShadows.Soft;
            moonLight.intensity = 0.1f;
            moonLight.color = new Color(0.6f, 0.6f, 0.8f);
        }
    }
    
    private void Update()
    {
        if (!isSimulationRunning) return;
        
        // Advance simulation time
        simulationTimer += Time.deltaTime * timeScale;
        
        // Update sunlight every minute
        if (simulationTimer >= simulationTimeStep.TotalSeconds)
        {
            simulationTimer = 0f;
            currentDateTime = currentDateTime.Add(simulationTimeStep);
            UpdateSunlight();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the sunlight simulator
    /// </summary>
    public void Initialize()
    {
        // Setup sunlight curve if not defined
        if (sunIntensityCurve.keys.Length == 0)
        {
            // Default curve - lowest at midnight, highest at noon
            sunIntensityCurve = new AnimationCurve(
                new Keyframe(0f, 0f),     // Midnight
                new Keyframe(0.25f, 0f),  // 6 AM - dawn begins
                new Keyframe(0.5f, 1f),   // Noon - full intensity
                new Keyframe(0.75f, 0f),  // 6 PM - dusk ends
                new Keyframe(1f, 0f)      // Midnight again
            );
        }
        
        // Initialize sun light properties
        if (sunLight != null)
        {
            sunLight.shadows = enableDynamicShadows ? LightShadows.Soft : LightShadows.None;
        }
        
        // Calculate initial sun position
        UpdateSunlight();
        
        Debug.Log("Sunlight simulator initialized");
    }
    
    /// <summary>
    /// Sets the date and time for the simulation
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    public void SetDateTime(DateTime dateTime)
    {
        currentDateTime = dateTime;
        UpdateSunlight();
    }
    
    /// <summary>
    /// Sets the geographic location for the simulation
    /// </summary>
    /// <param name="lat">Latitude (-90 to 90)</param>
    /// <param name="lon">Longitude (-180 to 180)</param>
    public void SetLocation(float lat, float lon)
    {
        latitude = lat;
        longitude = lon;
        
        // Clear cache when location changes
        positionCache.Clear();
        
        UpdateSunlight();
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
    /// Starts the sunlight simulation
    /// </summary>
    public void StartSimulation()
    {
        isSimulationRunning = true;
        simulationTimer = 0f;
        UpdateSunlight();
    }
    
    /// <summary>
    /// Stops the sunlight simulation
    /// </summary>
    public void StopSimulation()
    {
        isSimulationRunning = false;
    }
    
    /// <summary>
    /// Gets the current sunlight state
    /// </summary>
    /// <returns>Current sunlight state</returns>
    public SunlightState GetCurrentSunlight()
    {
        return currentState;
    }
    
    /// <summary>
    /// Gets a future sunlight state
    /// </summary>
    /// <param name="hoursAhead">Hours into the future</param>
    /// <returns>Future sunlight state</returns>
    public SunlightState GetFutureSunlight(float hoursAhead)
    {
        DateTime futureTime = currentDateTime.AddHours(hoursAhead);
        
        SunlightState state = new SunlightState();
        state.dateTime = futureTime;
        state.direction = CalculateSunDirection(futureTime);
        
        // Calculate sun altitude to determine intensity
        float altitude = CalculateSunAltitude(futureTime);
        float dayFactor = Mathf.InverseLerp(-18f, 90f, altitude); // -18° is astronomical twilight
        
        // Time of day (0-1 where 0/1 = midnight, 0.5 = noon)
        float timeOfDay = (float)(futureTime.Hour * 60 + futureTime.Minute) / 1440f;
        
        // Sun intensity from curve and altitude
        state.intensity = sunIntensityCurve.Evaluate(timeOfDay) * dayFactor * sunIntensityMultiplier;
        
        return state;
    }
    
    /// <summary>
    /// Sets manual sunlight settings (for non-realistic mode)
    /// </summary>
    /// <param name="state">Sunlight state to set</param>
    public void SetManualSunlight(SunlightState state)
    {
        currentState = state;
        
        // Apply the state to the light
        ApplySunlightState(state);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Updates the sunlight based on current date/time
    /// </summary>
    private void UpdateSunlight()
    {
        // Calculate new sun direction
        Vector3 sunDirection = CalculateSunDirection(currentDateTime);
        
        // Calculate sun altitude to determine intensity
        float altitude = CalculateSunAltitude(currentDateTime);
        float dayFactor = Mathf.InverseLerp(-18f, 90f, altitude); // -18° is astronomical twilight
        
        // Time of day (0-1 where 0/1 = midnight, 0.5 = noon)
        float timeOfDay = (float)(currentDateTime.Hour * 60 + currentDateTime.Minute) / 1440f;
        
        // Sun intensity from curve and altitude
        float intensity = sunIntensityCurve.Evaluate(timeOfDay) * dayFactor * sunIntensityMultiplier;
        
        // Update current state
        currentState.dateTime = currentDateTime;
        currentState.direction = sunDirection;
        currentState.intensity = intensity;
        
        // Apply the state to the light
        ApplySunlightState(currentState);
    }
    
    /// <summary>
    /// Applies a sunlight state to the scene lights
    /// </summary>
    /// <param name="state">Sunlight state to apply</param>
    private void ApplySunlightState(SunlightState state)
    {
        if (sunLight == null) return;
        
        // Update sun direction (inverted since light shines in opposite direction)
        sunLight.transform.rotation = Quaternion.LookRotation(-state.direction);
        
        // Update sun intensity
        sunLight.intensity = state.intensity;
        
        // Calculate color temperature based on altitude
        float altitude = Vector3.Angle(state.direction, Vector3.down);
        float normalizedAltitude = Mathf.Clamp01(altitude / 90f);
        
        // Lower altitude = warmer (more orange/red) light
        float temperature = Mathf.Lerp(2000f, 6500f, normalizedAltitude);
        
        // Apply color temperature if supported
        if (sunLight.useColorTemperature)
        {
            sunLight.colorTemperature = temperature;
        }
        else
        {
            // Approximate temperature as color
            Color sunColor = ColorFromTemperature(temperature);
            sunLight.color = sunColor;
        }
        
        // Update moon light (opposite direction, only visible at night)
        if (moonLight != null)
        {
            moonLight.transform.rotation = Quaternion.LookRotation(state.direction);
            moonLight.intensity = Mathf.Clamp01(0.2f - state.intensity * 0.5f);
        }
        
        // Update shadow distance based on time of day
        if (enableDynamicShadows)
        {
            float shadowDistance = Mathf.Lerp(shadowDistanceNight, shadowDistanceDay, state.intensity);
            QualitySettings.shadowDistance = shadowDistance;
        }
        
        // Update skybox if available
        if (skyboxMaterial != null)
        {
            // Sky tint varies by time of day
            Color skyTint;
            
            if (normalizedAltitude > 0.2f)
            {
                // Daytime
                skyTint = Color.Lerp(new Color(0.5f, 0.5f, 1f), new Color(0.4f, 0.6f, 1f), normalizedAltitude);
            }
            else if (normalizedAltitude > 0.05f)
            {
                // Twilight
                skyTint = Color.Lerp(new Color(0.6f, 0.4f, 0.6f), new Color(0.5f, 0.5f, 1f), normalizedAltitude / 0.2f);
            }
            else
            {
                // Night
                skyTint = new Color(0.1f, 0.1f, 0.2f);
            }
            
            skyboxMaterial.SetColor("_SkyTint", skyTint);
            
            // Exposure based on time of day
            float exposure = Mathf.Lerp(0.8f, 1.2f, normalizedAltitude);
            skyboxMaterial.SetFloat("_Exposure", exposure);
        }
    }
    
    /// <summary>
    /// Calculates the sun direction vector for a given date and time
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <returns>Direction vector (from earth to sun)</returns>
    private Vector3 CalculateSunDirection(DateTime dateTime)
    {
        // Check cache first
        foreach (var entry in positionCache)
        {
            if (Math.Abs((entry.date - dateTime).TotalMinutes) < 1)
            {
                return entry.position;
            }
        }
        
        // Calculate sun position using astronomical formulas
        // This is simplified but reasonably accurate for visualization
        
        // Day of year (1-366)
        int dayOfYear = dateTime.DayOfYear;
        
        // Time of day in hours, corrected for timezone
        float timeOfDay = dateTime.Hour + dateTime.Minute / 60f;
        
        // Convert latitude and longitude to radians
        float latRad = Mathf.Deg2Rad * latitude;
        float lonRad = Mathf.Deg2Rad * longitude;
        
        // Solar declination angle
        float declination = 23.45f * Mathf.Sin(Mathf.Deg2Rad * (360f / 365f) * (dayOfYear - 81));
        float declinationRad = Mathf.Deg2Rad * declination;
        
        // Hour angle
        float solarTime = CalculateSolarTime(dateTime, longitude, timeZoneOffset);
        float hourAngle = 15f * (solarTime - 12f);
        float hourAngleRad = Mathf.Deg2Rad * hourAngle;
        
        // Calculate altitude and azimuth
        float sinAltitude = Mathf.Sin(latRad) * Mathf.Sin(declinationRad) + 
                            Mathf.Cos(latRad) * Mathf.Cos(declinationRad) * Mathf.Cos(hourAngleRad);
        float altitude = Mathf.Asin(sinAltitude);
        
        float cosAzimuth = (Mathf.Sin(declinationRad) - Mathf.Sin(latRad) * sinAltitude) / 
                           (Mathf.Cos(latRad) * Mathf.Cos(altitude));
        
        // Clamp to avoid NaN
        cosAzimuth = Mathf.Clamp(cosAzimuth, -1f, 1f);
        float azimuth = Mathf.Acos(cosAzimuth);
        
        // Correct azimuth for afternoon
        if (hourAngle > 0)
        {
            azimuth = Mathf.PI * 2 - azimuth;
        }
        
        // Convert to direction vector (Unity coordinates)
        float x = Mathf.Sin(azimuth) * Mathf.Cos(altitude);
        float y = Mathf.Sin(altitude);
        float z = Mathf.Cos(azimuth) * Mathf.Cos(altitude);
        
        Vector3 direction = new Vector3(x, y, z).normalized;
        
        // Add to cache
        positionCache.Add(new DatePosition { date = dateTime, position = direction });
        
        // Limit cache size
        if (positionCache.Count > maxCacheSize)
        {
            positionCache.RemoveAt(0);
        }
        
        return direction;
    }
    
    /// <summary>
    /// Calculates the solar time (corrected for equation of time)
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <param name="longitude">Longitude in degrees</param>
    /// <param name="timeZone">Timezone offset in hours</param>
    /// <returns>Solar time in hours</returns>
    private float CalculateSolarTime(DateTime dateTime, float longitude, int timeZone)
    {
        // Day of year (1-366)
        int dayOfYear = dateTime.DayOfYear;
        
        // Standard time in decimal hours
        float standardTime = dateTime.Hour + dateTime.Minute / 60f;
        
        // Calculate equation of time correction (in minutes)
        float b = 2f * Mathf.PI * (dayOfYear - 81) / 365f;
        float equationOfTime = 9.87f * Mathf.Sin(2 * b) - 7.53f * Mathf.Cos(b) - 1.5f * Mathf.Sin(b);
        
        // Longitude correction (4 minutes per degree difference from timezone reference longitude)
        float longitudeCorrection = 4f * (longitude - 15f * timeZone);
        
        // Total correction in minutes
        float totalCorrection = equationOfTime + longitudeCorrection;
        
        // Solar time in hours
        float solarTime = standardTime + totalCorrection / 60f;
        
        return solarTime;
    }
    
    /// <summary>
    /// Calculates the sun's altitude in degrees above the horizon
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <returns>Altitude in degrees</returns>
    private float CalculateSunAltitude(DateTime dateTime)
    {
        Vector3 direction = CalculateSunDirection(dateTime);
        float altitude = Vector3.Angle(direction, Vector3.down) - 90f;
        return altitude;
    }
    
    /// <summary>
    /// Approximates a color from a color temperature in Kelvin
    /// </summary>
    /// <param name="temperature">Color temperature in Kelvin</param>
    /// <returns>Approximate RGB color</returns>
    private Color ColorFromTemperature(float temperature)
    {
        // Simple approximation of blackbody radiation
        float temp = temperature / 100f;
        
        float red, green, blue;
        
        if (temp <= 66f)
        {
            red = 1f;
            green = Mathf.Clamp01(0.39008157876f * Mathf.Log(temp) - 0.63184144378f);
        }
        else
        {
            red = Mathf.Clamp01(1.29293618606f * Mathf.Pow(temp - 60f, -0.1332047592f));
            green = Mathf.Clamp01(1.12989086451f * Mathf.Pow(temp - 60f, -0.0755148492f));
        }
        
        if (temp >= 66f)
        {
            blue = 1f;
        }
        else if (temp <= 19f)
        {
            blue = 0f;
        }
        else
        {
            blue = Mathf.Clamp01(0.54320678911f * Mathf.Log(temp - 10f) - 1.19625408914f);
        }
        
        return new Color(red, green, blue);
    }
    #endregion
}

/// <summary>
/// Represents the sunlight state at a specific time
/// </summary>
[System.Serializable]
public struct SunlightState
{
    public DateTime dateTime;
    public Vector3 direction;   // Direction from earth to sun
    public float intensity;     // Light intensity (0-1)
}