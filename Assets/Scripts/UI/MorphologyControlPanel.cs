using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for controlling morphology parameters and adaptations.
/// Allows users to adjust growth parameters, view statistics, and control adaptation behaviors.
/// </summary>
public class MorphologyControlPanel : MonoBehaviour
{
    #region Properties
    [Header("UI Components")]
    [SerializeField] private Slider densitySlider;
    [SerializeField] private Slider complexitySlider;
    [SerializeField] private Slider connectivitySlider;
    [SerializeField] private Slider growthRateSlider;
    [SerializeField] private Slider adaptationRateSlider;
    [SerializeField] private Toggle autoAdaptToggle;
    [SerializeField] private TMP_Dropdown biomorphTypeDropdown;
    [SerializeField] private Button applyButton;
    [SerializeField] private Button resetButton;
    [SerializeField] private Button randomizeButton;
    
    [Header("Value Display")]
    [SerializeField] private TextMeshProUGUI densityValueText;
    [SerializeField] private TextMeshProUGUI complexityValueText;
    [SerializeField] private TextMeshProUGUI connectivityValueText;
    [SerializeField] private TextMeshProUGUI growthRateValueText;
    [SerializeField] private TextMeshProUGUI adaptationRateValueText;
    
    [Header("Statistics")]
    [SerializeField] private TextMeshProUGUI nodeCountText;
    [SerializeField] private TextMeshProUGUI connectionCountText;
    [SerializeField] private TextMeshProUGUI adaptationCountText;
    [SerializeField] private TextMeshProUGUI stabilityScoreText;
    [SerializeField] private GameObject statsPanel;
    
    [Header("Visualization")]
    [SerializeField] private Toggle showStressToggle;
    [SerializeField] private Toggle showAdaptationsToggle;
    [SerializeField] private Toggle showGrowthPotentialToggle;
    [SerializeField] private Slider visualScaleSlider;
    
    [Header("References")]
    [SerializeField] private AdaptationSystem adaptationSystem;
    [SerializeField] private PerformanceOptimizer performanceOptimizer;
    
    // Parameter cache
    private MorphologyParameters currentParameters = new MorphologyParameters();
    private MorphologyParameters.BiomorphType currentBiomorphType;
    private bool isDirty = false;
      // Events
    public delegate void MorphologyParametersChangedHandler(MorphologyParameters parameters);
    public event MorphologyParametersChangedHandler OnParametersChanged;
    #endregion

    #region Unity Methods
    
    private void Awake()
    {
        // Default values if not set
        if (adaptationSystem == null)
            adaptationSystem = FindFirstObjectByType<AdaptationSystem>();
        
        if (performanceOptimizer == null)
            performanceOptimizer = PerformanceOptimizer.Instance;
    }
    
    private void Start()
    {
        // Initialize dropdown options
        InitializeBiomorphDropdown();
        
        // Set up listeners
        SetupEventListeners();
        
        // Initialize parameters
        InitializeParameters();
        
        // Update UI
        UpdateUIFromParameters();
        
        // Update statistics
        UpdateStatistics();
    }
    
    private void Update()
    {
        // Periodically update statistics
        if (Time.frameCount % 30 == 0)
        {
            UpdateStatistics();
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Apply the current parameters to the morphology
    /// </summary>
    public void ApplyParameters()
    {
        if (!isDirty) return;
        
        // Check if we need to optimize based on system performance
        if (performanceOptimizer != null)
        {
            MorphologyParameters optimizedParams = 
                performanceOptimizer.GetOptimizedMorphologyParameters(currentParameters);
            
            // If parameters were optimized, update UI to reflect actual values
            if (!AreParametersEqual(currentParameters, optimizedParams))
            {
                currentParameters = optimizedParams;
                UpdateUIFromParameters();
            }
        }
        
        // Notify listeners of parameter changes
        if (OnParametersChanged != null)
            OnParametersChanged.Invoke(currentParameters);
        
        isDirty = false;
    }
    
    /// <summary>
    /// Reset parameters to default values
    /// </summary>
    public void ResetParameters()
    {
        // Set default values
        currentParameters.density = 0.5f;
        currentParameters.complexity = 0.5f;
        currentParameters.connectivity = 0.5f;
        currentParameters.growthRate = 0.5f;
        currentParameters.adaptationRate = 0.5f;
        currentParameters.biomorphType = MorphologyParameters.BiomorphType.Standard;
        
        // Update UI
        UpdateUIFromParameters();
        
        // Mark as dirty
        isDirty = true;
    }
    
    /// <summary>
    /// Randomize parameters
    /// </summary>
    public void RandomizeParameters()
    {
        // Generate random values
        currentParameters.density = Random.Range(0.3f, 0.8f);
        currentParameters.complexity = Random.Range(0.3f, 0.8f);
        currentParameters.connectivity = Random.Range(0.3f, 0.8f);
        currentParameters.growthRate = Random.Range(0.4f, 0.7f);
        currentParameters.adaptationRate = Random.Range(0.4f, 0.7f);
        
        // Random biomorph type
        int randomType = Random.Range(0, System.Enum.GetValues(typeof(MorphologyParameters.BiomorphType)).Length);
        currentParameters.biomorphType = (MorphologyParameters.BiomorphType)randomType;
        
        // Update UI
        UpdateUIFromParameters();
        
        // Mark as dirty
        isDirty = true;
    }
    
    /// <summary>
    /// Toggle the statistics panel
    /// </summary>
    public void ToggleStatisticsPanel()
    {
        if (statsPanel != null)
            statsPanel.SetActive(!statsPanel.activeSelf);
    }
    
    /// <summary>
    /// Set visibility of the panel
    /// </summary>
    public void SetPanelVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    
    /// <summary>
    /// Updates the morphology parameters from external source
    /// </summary>
    public void UpdateParameters(MorphologyParameters parameters)
    {
        currentParameters = parameters;
        UpdateUIFromParameters();
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initialize the biomorph type dropdown
    /// </summary>
    private void InitializeBiomorphDropdown()
    {
        if (biomorphTypeDropdown == null) return;
        
        biomorphTypeDropdown.ClearOptions();
        
        // Add options for each biomorph type
        List<string> options = new List<string>();
        foreach (MorphologyParameters.BiomorphType type in System.Enum.GetValues(typeof(MorphologyParameters.BiomorphType)))
        {
            options.Add(GetDisplayNameForBiomorphType(type));
        }
        
        biomorphTypeDropdown.AddOptions(options);
    }
    
    /// <summary>
    /// Get display name for a biomorph type
    /// </summary>
    private string GetDisplayNameForBiomorphType(MorphologyParameters.BiomorphType type)
    {
        switch (type)
        {
            case MorphologyParameters.BiomorphType.Standard:
                return "Standard";
            case MorphologyParameters.BiomorphType.Organic:
                return "Organic";
            case MorphologyParameters.BiomorphType.Crystalline:
                return "Crystalline";
            case MorphologyParameters.BiomorphType.Fungal:
                return "Fungal";
            case MorphologyParameters.BiomorphType.Coral:
                return "Coral";
            case MorphologyParameters.BiomorphType.Hybrid:
                return "Hybrid";
            default:
                return type.ToString();
        }
    }
    
    /// <summary>
    /// Set up event listeners for UI components
    /// </summary>
    private void SetupEventListeners()
    {
        // Add listeners to sliders
        if (densitySlider != null)
            densitySlider.onValueChanged.AddListener(OnDensityChanged);
        
        if (complexitySlider != null)
            complexitySlider.onValueChanged.AddListener(OnComplexityChanged);
        
        if (connectivitySlider != null)
            connectivitySlider.onValueChanged.AddListener(OnConnectivityChanged);
        
        if (growthRateSlider != null)
            growthRateSlider.onValueChanged.AddListener(OnGrowthRateChanged);
        
        if (adaptationRateSlider != null)
            adaptationRateSlider.onValueChanged.AddListener(OnAdaptationRateChanged);
        
        // Add listener to biomorph type dropdown
        if (biomorphTypeDropdown != null)
            biomorphTypeDropdown.onValueChanged.AddListener(OnBiomorphTypeChanged);
        
        // Add listeners to buttons
        if (applyButton != null)
            applyButton.onClick.AddListener(ApplyParameters);
        
        if (resetButton != null)
            resetButton.onClick.AddListener(ResetParameters);
        
        if (randomizeButton != null)
            randomizeButton.onClick.AddListener(RandomizeParameters);
        
        // Add listeners to toggles
        if (autoAdaptToggle != null)
            autoAdaptToggle.onValueChanged.AddListener(OnAutoAdaptChanged);
        
        if (showStressToggle != null)
            showStressToggle.onValueChanged.AddListener(OnShowStressChanged);
        
        if (showAdaptationsToggle != null)
            showAdaptationsToggle.onValueChanged.AddListener(OnShowAdaptationsChanged);
        
        if (showGrowthPotentialToggle != null)
            showGrowthPotentialToggle.onValueChanged.AddListener(OnShowGrowthPotentialChanged);
        
        if (visualScaleSlider != null)
            visualScaleSlider.onValueChanged.AddListener(OnVisualScaleChanged);
    }
    
    /// <summary>
    /// Initialize parameters with default values
    /// </summary>
    private void InitializeParameters()
    {
        // Set default values
        currentParameters.density = 0.5f;
        currentParameters.complexity = 0.5f;
        currentParameters.connectivity = 0.5f;
        currentParameters.growthRate = 0.5f;
        currentParameters.adaptationRate = 0.5f;
        currentParameters.biomorphType = MorphologyParameters.BiomorphType.Standard;
    }
    
    /// <summary>
    /// Update UI components to reflect current parameters
    /// </summary>
    private void UpdateUIFromParameters()
    {
        // Update sliders
        if (densitySlider != null)
            densitySlider.value = currentParameters.density;
        
        if (complexitySlider != null)
            complexitySlider.value = currentParameters.complexity;
        
        if (connectivitySlider != null)
            connectivitySlider.value = currentParameters.connectivity;
        
        if (growthRateSlider != null)
            growthRateSlider.value = currentParameters.growthRate;
        
        if (adaptationRateSlider != null)
            adaptationRateSlider.value = currentParameters.adaptationRate;
        
        // Update dropdown
        if (biomorphTypeDropdown != null)
            biomorphTypeDropdown.value = (int)currentParameters.biomorphType;
        
        // Update text displays
        UpdateValueTexts();
    }
    
    /// <summary>
    /// Update text displays for parameter values
    /// </summary>
    private void UpdateValueTexts()
    {
        if (densityValueText != null)
            densityValueText.text = (currentParameters.density * 100).ToString("0") + "%";
        
        if (complexityValueText != null)
            complexityValueText.text = (currentParameters.complexity * 100).ToString("0") + "%";
        
        if (connectivityValueText != null)
            connectivityValueText.text = (currentParameters.connectivity * 100).ToString("0") + "%";
        
        if (growthRateValueText != null)
            growthRateValueText.text = (currentParameters.growthRate * 100).ToString("0") + "%";
        
        if (adaptationRateValueText != null)
            adaptationRateValueText.text = (currentParameters.adaptationRate * 100).ToString("0") + "%";
    }
    
    /// <summary>
    /// Update statistics display
    /// </summary>
    private void UpdateStatistics()
    {
        if (adaptationSystem == null) return;
        
        int nodeCount = adaptationSystem.NodeCount;
        int connectionCount = adaptationSystem.ConnectionCount;
        int adaptationCount = adaptationSystem.AdaptationCount;
        float stabilityScore = adaptationSystem.StabilityScore;
        
        if (nodeCountText != null)
            nodeCountText.text = $"Nodes: {nodeCount}";
        
        if (connectionCountText != null)
            connectionCountText.text = $"Connections: {connectionCount}";
        
        if (adaptationCountText != null)
            adaptationCountText.text = $"Adaptations: {adaptationCount}";
        
        if (stabilityScoreText != null)
            stabilityScoreText.text = $"Stability: {stabilityScore:P0}";
    }
    
    /// <summary>
    /// Compare two parameter sets for equality
    /// </summary>
    private bool AreParametersEqual(MorphologyParameters a, MorphologyParameters b)
    {
        return Mathf.Approximately(a.density, b.density) &&
               Mathf.Approximately(a.complexity, b.complexity) &&
               Mathf.Approximately(a.connectivity, b.connectivity) &&
               Mathf.Approximately(a.growthRate, b.growthRate) &&
               Mathf.Approximately(a.adaptationRate, b.adaptationRate) &&
               a.biomorphType == b.biomorphType;
    }
    
    #region UI Event Handlers
    private void OnDensityChanged(float value)
    {
        currentParameters.density = value;
        UpdateValueTexts();
        isDirty = true;
    }
    
    private void OnComplexityChanged(float value)
    {
        currentParameters.complexity = value;
        UpdateValueTexts();
        isDirty = true;
    }
    
    private void OnConnectivityChanged(float value)
    {
        currentParameters.connectivity = value;
        UpdateValueTexts();
        isDirty = true;
    }
    
    private void OnGrowthRateChanged(float value)
    {
        currentParameters.growthRate = value;
        UpdateValueTexts();
        isDirty = true;
    }
    
    private void OnAdaptationRateChanged(float value)
    {
        currentParameters.adaptationRate = value;
        UpdateValueTexts();
        isDirty = true;
    }
    
    private void OnBiomorphTypeChanged(int index)
    {
        currentParameters.biomorphType = (MorphologyParameters.BiomorphType)index;
        isDirty = true;
    }
    
    private void OnAutoAdaptChanged(bool value)
    {
        if (adaptationSystem != null)
            adaptationSystem.SetAutoAdaptation(value);
    }
    
    private void OnShowStressChanged(bool value)
    {
        if (adaptationSystem != null)
            adaptationSystem.SetStressVisualization(value);
    }
    
    private void OnShowAdaptationsChanged(bool value)
    {
        if (adaptationSystem != null)
            adaptationSystem.SetAdaptationVisualization(value);
    }
    
    private void OnShowGrowthPotentialChanged(bool value)
    {
        if (adaptationSystem != null)
            adaptationSystem.SetGrowthPotentialVisualization(value);
    }
    
    private void OnVisualScaleChanged(float value)
    {
        if (adaptationSystem != null)
            adaptationSystem.SetVisualizationScale(value);
    }
    #endregion
    #endregion
}
