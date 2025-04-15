using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

/// <summary>
/// UI panel for managing simulation scenarios.
/// Allows loading, saving, and configuring different environmental and morphological scenarios.
/// </summary>
public class ScenarioPanel : MonoBehaviour
{
    #region Properties
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown scenarioDropdown;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button newButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private TMP_InputField scenarioNameInput;
    [SerializeField] private TextMeshProUGUI descriptionText;
    
    [Header("Time Controls")]
    [SerializeField] private Slider timeSpeedSlider;
    [SerializeField] private TextMeshProUGUI timeSpeedText;
    [SerializeField] private TextMeshProUGUI currentDateText;
    [SerializeField] private Toggle pauseToggle;
    [SerializeField] private Button resetTimeButton;
    
    [Header("Environment Settings")]
    [SerializeField] private Slider temperatureSlider;
    [SerializeField] private Slider windSpeedSlider;
    [SerializeField] private Slider humiditySlider;
    [SerializeField] private Slider pedestrianDensitySlider;
    [SerializeField] private Slider sunlightIntensitySlider;
    [SerializeField] private Toggle rainToggle;
    [SerializeField] private TMP_Dropdown seasonDropdown;
    [SerializeField] private Toggle useRealisticWeatherToggle;
    
    [Header("References")]
    [SerializeField] private EnvironmentManager environmentManager;
    [SerializeField] private MorphologyControlPanel morphologyPanel;
    
    // Scenario data
    private List<SimulationScenario> availableScenarios = new List<SimulationScenario>();
    private SimulationScenario currentScenario = new SimulationScenario();
    private bool isLoadingScenario = false;
      // Events
    public delegate void ScenarioChangedHandler(SimulationScenario scenario);
    public event ScenarioChangedHandler OnScenarioChanged;
    #endregion

    #region Unity Methods
    
    private void Awake()
    {
        // Find references if not assigned
        if (environmentManager == null)
            environmentManager = FindFirstObjectByType<EnvironmentManager>();
        
        if (morphologyPanel == null)
            morphologyPanel = FindFirstObjectByType<MorphologyControlPanel>();
    }
    
    private void Start()
    {
        // Initialize UI
        SetupEventListeners();
        
        // Load available scenarios
        LoadAvailableScenarios();
        
        // Update dropdown
        UpdateScenarioDropdown();
        
        // Initialize with default scenario if available
        if (availableScenarios.Count > 0)
        {
            LoadScenario(availableScenarios[0]);
        }
        else
        {
            // Create default scenario
            CreateDefaultScenario();
        }
        
        // Update UI
        UpdateUIFromScenario();
    }
    
    private void Update()
    {
        // Update time display
        if (environmentManager != null && currentDateText != null)
        {
            DateTime currentDate = environmentManager.CurrentDateTime;
            currentDateText.text = currentDate.ToString("dd MMM yyyy HH:mm");
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Load a scenario by name
    /// </summary>
    public void LoadScenarioByName(string name)
    {
        SimulationScenario scenario = availableScenarios.Find(s => s.name == name);
        if (scenario != null)
        {
            LoadScenario(scenario);
        }
    }
    
    /// <summary>
    /// Save the current scenario
    /// </summary>
    public void SaveCurrentScenario()
    {
        // Get name from input field
        if (scenarioNameInput != null)
        {
            string name = scenarioNameInput.text;
            if (string.IsNullOrEmpty(name))
            {
                Debug.LogWarning("Cannot save scenario without a name");
                return;
            }
            
            currentScenario.name = name;
        }
        
        // Update scenario from UI
        UpdateScenarioFromUI();
        
        // Find if scenario already exists
        int existingIndex = availableScenarios.FindIndex(s => s.name == currentScenario.name);
        if (existingIndex >= 0)
        {
            // Replace existing
            availableScenarios[existingIndex] = currentScenario;
        }
        else
        {
            // Add new
            availableScenarios.Add(currentScenario);
        }
        
        // Save scenarios
        SaveScenarios();
        
        // Update dropdown
        UpdateScenarioDropdown();
        
        // Select in dropdown
        if (scenarioDropdown != null)
        {
            int index = availableScenarios.FindIndex(s => s.name == currentScenario.name);
            if (index >= 0)
            {
                scenarioDropdown.value = index;
            }
        }
    }
    
    /// <summary>
    /// Create a new scenario
    /// </summary>
    public void CreateNewScenario()
    {
        currentScenario = new SimulationScenario();
        currentScenario.name = "New Scenario " + (availableScenarios.Count + 1);
        currentScenario.description = "A new simulation scenario";
        currentScenario.dateTime = DateTime.Now;
        
        // Default environment settings
        currentScenario.temperature = 20.0f;
        currentScenario.windSpeed = 5.0f;
        currentScenario.humidity = 0.6f;
        currentScenario.pedestrianDensity = 0.5f;
        currentScenario.sunlightIntensity = 0.8f;
        currentScenario.isRaining = false;
        currentScenario.season = Season.Summer;
        currentScenario.useRealisticWeather = true;
        
        // Default morphology parameters
        currentScenario.morphologyParameters = new MorphologyParameters();
        
        // Update UI
        UpdateUIFromScenario();
        
        // Set name input field
        if (scenarioNameInput != null)
        {
            scenarioNameInput.text = currentScenario.name;
        }
    }
    
    /// <summary>
    /// Delete the current scenario
    /// </summary>
    public void DeleteCurrentScenario()
    {
        int index = availableScenarios.FindIndex(s => s.name == currentScenario.name);
        if (index >= 0)
        {
            availableScenarios.RemoveAt(index);
            
            // Save scenarios
            SaveScenarios();
            
            // Update dropdown
            UpdateScenarioDropdown();
            
            // Load first scenario or create default
            if (availableScenarios.Count > 0)
            {
                LoadScenario(availableScenarios[0]);
            }
            else
            {
                CreateDefaultScenario();
            }
        }
    }
    
    /// <summary>
    /// Set time speed
    /// </summary>
    public void SetTimeSpeed(float speed)
    {
        if (environmentManager != null)
        {
            environmentManager.SetTimeScale(speed);
        }
        
        if (timeSpeedText != null)
        {
            timeSpeedText.text = $"{speed:F1}x";
        }
    }
    
    /// <summary>
    /// Toggle pause
    /// </summary>
    public void TogglePause(bool paused)
    {
        if (environmentManager != null)
        {
            environmentManager.SetPaused(paused);
        }
    }
    
    /// <summary>
    /// Reset time to scenario start
    /// </summary>
    public void ResetTime()
    {
        if (environmentManager != null && currentScenario != null)
        {
            environmentManager.SetDateTime(currentScenario.dateTime);
        }
    }
    
    /// <summary>
    /// Set panel visibility
    /// </summary>
    public void SetPanelVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Set up event listeners for UI components
    /// </summary>
    private void SetupEventListeners()
    {
        // Add listeners to buttons
        if (loadButton != null)
            loadButton.onClick.AddListener(LoadSelectedScenario);
        
        if (saveButton != null)
            saveButton.onClick.AddListener(SaveCurrentScenario);
        
        if (newButton != null)
            newButton.onClick.AddListener(CreateNewScenario);
        
        if (deleteButton != null)
            deleteButton.onClick.AddListener(DeleteCurrentScenario);
        
        if (resetTimeButton != null)
            resetTimeButton.onClick.AddListener(ResetTime);
        
        // Add listener to dropdown
        if (scenarioDropdown != null)
            scenarioDropdown.onValueChanged.AddListener(OnScenarioSelected);
        
        // Add listener to time speed slider
        if (timeSpeedSlider != null)
            timeSpeedSlider.onValueChanged.AddListener(SetTimeSpeed);
        
        // Add listener to pause toggle
        if (pauseToggle != null)
            pauseToggle.onValueChanged.AddListener(TogglePause);
        
        // Add listeners to environment controls
        if (temperatureSlider != null)
            temperatureSlider.onValueChanged.AddListener(OnTemperatureChanged);
        
        if (windSpeedSlider != null)
            windSpeedSlider.onValueChanged.AddListener(OnWindSpeedChanged);
        
        if (humiditySlider != null)
            humiditySlider.onValueChanged.AddListener(OnHumidityChanged);
        
        if (pedestrianDensitySlider != null)
            pedestrianDensitySlider.onValueChanged.AddListener(OnPedestrianDensityChanged);
        
        if (sunlightIntensitySlider != null)
            sunlightIntensitySlider.onValueChanged.AddListener(OnSunlightIntensityChanged);
        
        if (rainToggle != null)
            rainToggle.onValueChanged.AddListener(OnRainToggleChanged);
        
        if (seasonDropdown != null)
            seasonDropdown.onValueChanged.AddListener(OnSeasonChanged);
        
        if (useRealisticWeatherToggle != null)
            useRealisticWeatherToggle.onValueChanged.AddListener(OnUseRealisticWeatherChanged);
    }
    
    /// <summary>
    /// Load available scenarios from PlayerPrefs
    /// </summary>
    private void LoadAvailableScenarios()
    {
        availableScenarios.Clear();
        
        // Try to load from PlayerPrefs
        if (PlayerPrefs.HasKey("ScenariosCount"))
        {
            int count = PlayerPrefs.GetInt("ScenariosCount");
            
            for (int i = 0; i < count; i++)
            {
                string scenarioJson = PlayerPrefs.GetString("Scenario_" + i);
                if (!string.IsNullOrEmpty(scenarioJson))
                {
                    try
                    {
                        SimulationScenario scenario = JsonUtility.FromJson<SimulationScenario>(scenarioJson);
                        availableScenarios.Add(scenario);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"Error loading scenario: {e.Message}");
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Save scenarios to PlayerPrefs
    /// </summary>
    private void SaveScenarios()
    {
        // Save count
        PlayerPrefs.SetInt("ScenariosCount", availableScenarios.Count);
        
        // Save each scenario
        for (int i = 0; i < availableScenarios.Count; i++)
        {
            string scenarioJson = JsonUtility.ToJson(availableScenarios[i]);
            PlayerPrefs.SetString("Scenario_" + i, scenarioJson);
        }
        
        PlayerPrefs.Save();
    }
    
    /// <summary>
    /// Update the scenario dropdown with available scenarios
    /// </summary>
    private void UpdateScenarioDropdown()
    {
        if (scenarioDropdown == null) return;
        
        // Clear options
        scenarioDropdown.ClearOptions();
        
        // Add available scenarios
        List<string> options = new List<string>();
        foreach (SimulationScenario scenario in availableScenarios)
        {
            options.Add(scenario.name);
        }
        
        scenarioDropdown.AddOptions(options);
    }
    
    /// <summary>
    /// Create a default scenario
    /// </summary>
    private void CreateDefaultScenario()
    {
        currentScenario = new SimulationScenario();
        currentScenario.name = "Default Scenario";
        currentScenario.description = "Default simulation scenario for Wellington Lambton Quay";
        currentScenario.dateTime = DateTime.Now;
        
        // Environment settings
        currentScenario.temperature = 20.0f;
        currentScenario.windSpeed = 5.0f;
        currentScenario.humidity = 0.6f;
        currentScenario.pedestrianDensity = 0.5f;
        currentScenario.sunlightIntensity = 0.8f;
        currentScenario.isRaining = false;
        currentScenario.season = Season.Summer;
        currentScenario.useRealisticWeather = true;
        
        // Morphology parameters
        currentScenario.morphologyParameters = new MorphologyParameters();
        
        // Add to available scenarios
        availableScenarios.Add(currentScenario);
        
        // Save scenarios
        SaveScenarios();
        
        // Update dropdown
        UpdateScenarioDropdown();
    }
    
    /// <summary>
    /// Load the selected scenario
    /// </summary>
    private void LoadSelectedScenario()
    {
        if (scenarioDropdown == null) return;
        
        int index = scenarioDropdown.value;
        if (index >= 0 && index < availableScenarios.Count)
        {
            LoadScenario(availableScenarios[index]);
        }
    }
    
    /// <summary>
    /// Load a scenario
    /// </summary>
    private void LoadScenario(SimulationScenario scenario)
    {
        isLoadingScenario = true;
        
        // Store reference
        currentScenario = scenario;
        
        // Update environment manager
        if (environmentManager != null)
        {
            environmentManager.SetDateTime(scenario.dateTime);
            
            if (!scenario.useRealisticWeather)
            {
                environmentManager.SetManualWeather(
                    scenario.temperature, 
                    scenario.windSpeed,
                    scenario.humidity,
                    scenario.pedestrianDensity,
                    scenario.sunlightIntensity,
                    scenario.isRaining,
                    (int)scenario.season
                );
            }
            
            environmentManager.SetUseRealisticWeather(scenario.useRealisticWeather);
        }
        
        // Update morphology panel
        if (morphologyPanel != null && scenario.morphologyParameters != null)
        {
            morphologyPanel.UpdateParameters(scenario.morphologyParameters);
        }
        
        // Update UI
        UpdateUIFromScenario();
        
        // Notify listeners
        if (OnScenarioChanged != null)
            OnScenarioChanged.Invoke(scenario);
        
        isLoadingScenario = false;
    }
    
    /// <summary>
    /// Update UI from the current scenario
    /// </summary>
    private void UpdateUIFromScenario()
    {
        if (scenarioNameInput != null)
            scenarioNameInput.text = currentScenario.name;
        
        if (descriptionText != null)
            descriptionText.text = currentScenario.description;
        
        if (temperatureSlider != null)
            temperatureSlider.value = currentScenario.temperature;
        
        if (windSpeedSlider != null)
            windSpeedSlider.value = currentScenario.windSpeed;
        
        if (humiditySlider != null)
            humiditySlider.value = currentScenario.humidity;
        
        if (pedestrianDensitySlider != null)
            pedestrianDensitySlider.value = currentScenario.pedestrianDensity;
        
        if (sunlightIntensitySlider != null)
            sunlightIntensitySlider.value = currentScenario.sunlightIntensity;
        
        if (rainToggle != null)
            rainToggle.isOn = currentScenario.isRaining;
        
        if (seasonDropdown != null)
            seasonDropdown.value = (int)currentScenario.season;
        
        if (useRealisticWeatherToggle != null)
            useRealisticWeatherToggle.isOn = currentScenario.useRealisticWeather;
    }
    
    /// <summary>
    /// Update scenario from UI
    /// </summary>
    private void UpdateScenarioFromUI()
    {
        if (scenarioNameInput != null)
            currentScenario.name = scenarioNameInput.text;
        
        // Environment settings are updated through events
        
        // Get current morphology parameters
        if (morphologyPanel != null)
        {
            // Note: This assumes the morphology panel has a method to get current parameters
            // currentScenario.morphologyParameters = morphologyPanel.GetCurrentParameters();
        }
    }
    
    #region UI Event Handlers
    private void OnScenarioSelected(int index)
    {
        if (!isLoadingScenario && index >= 0 && index < availableScenarios.Count)
        {
            LoadScenario(availableScenarios[index]);
        }
    }
    
    private void OnTemperatureChanged(float value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.temperature = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetTemperature(value);
        }
    }
    
    private void OnWindSpeedChanged(float value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.windSpeed = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetWindSpeed(value);
        }
    }
    
    private void OnHumidityChanged(float value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.humidity = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetHumidity(value);
        }
    }
    
    private void OnPedestrianDensityChanged(float value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.pedestrianDensity = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetPedestrianDensity(value);
        }
    }
    
    private void OnSunlightIntensityChanged(float value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.sunlightIntensity = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetSunlightIntensity(value);
        }
    }
    
    private void OnRainToggleChanged(bool value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.isRaining = value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetRaining(value);
        }
    }
    
    private void OnSeasonChanged(int value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.season = (Season)value;
        
        if (environmentManager != null && !currentScenario.useRealisticWeather)
        {
            environmentManager.SetSeason(value);
        }
    }
    
    private void OnUseRealisticWeatherChanged(bool value)
    {
        if (isLoadingScenario) return;
        
        currentScenario.useRealisticWeather = value;
        
        if (environmentManager != null)
        {
            environmentManager.SetUseRealisticWeather(value);
        }
    }
    #endregion
    #endregion
}

/// <summary>
/// Structure to hold simulation scenario data
/// </summary>
[System.Serializable]
public class SimulationScenario
{
    public string name;
    public string description;
    public DateTime dateTime;
    
    // Environment settings
    public float temperature;
    public float windSpeed;
    public float humidity;
    public float pedestrianDensity;
    public float sunlightIntensity;
    public bool isRaining;
    public Season season;
    public bool useRealisticWeather;
    
    // Morphology parameters
    public MorphologyParameters morphologyParameters;
}

/// <summary>
/// Seasons enum
/// </summary>
public enum Season
{
    Winter,
    Spring,
    Summer,
    Autumn
}
