using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel that provides controls for adjusting visualization settings
/// </summary>
public class VisualizationPanel : MonoBehaviour
{
    [Header("Panel Elements")]
    [SerializeField] private GameObject mainPanel;
    [SerializeField] private TMP_Dropdown styleDropdown;
    [SerializeField] private Slider transparencySlider;
    [SerializeField] private Slider nodeThicknessSlider;
    [SerializeField] private Slider connectionThicknessSlider;
    [SerializeField] private Slider glowIntensitySlider;
    [SerializeField] private Toggle stressColorsToggle;
    [SerializeField] private Toggle autoRotateToggle;
    [SerializeField] private Toggle showForcesToggle;
    [SerializeField] private TMP_Dropdown renderModeDropdown;
    [SerializeField] private Button screenshotButton;
    [SerializeField] private Button resetViewButton;
    
    [Header("Optional UI Elements")]
    [SerializeField] private Slider rotationSpeedSlider;
    [SerializeField] private Slider stressIntensitySlider;
    [SerializeField] private Toggle bloomToggle;
    
    [Header("Configuration")]
    [SerializeField] private bool initializeOnStart = true;
    [SerializeField] private bool refreshStylesOnEnable = true;
    
    // Reference to visualization manager (will find if not set)
    private VisualizationManager visualizationManager;
    
    // Start is called before the first frame update
    void Start()
    {
        if (initializeOnStart)
        {
            InitializePanel();
        }
    }
    
    private void OnEnable()
    {
        if (refreshStylesOnEnable)
        {
            RefreshStyleDropdown();
        }
    }
    
    /// <summary>
    /// Initializes the visualization panel with current settings and connects event handlers
    /// </summary>
    public void InitializePanel()
    {
        // Get visualization manager reference if not set
        if (visualizationManager == null)
        {
            visualizationManager = VisualizationManager.Instance;
            
            if (visualizationManager == null)
            {
                Debug.LogError("Could not find VisualizationManager. UI controls will not function.");
                return;
            }
        }
        
        // Initialize dropdown with available styles
        RefreshStyleDropdown();
        
        // Initialize sliders with current values
        if (transparencySlider != null)
        {
            transparencySlider.value = 1.0f; // Default to fully opaque
            transparencySlider.onValueChanged.AddListener(OnTransparencyChanged);
        }
        
        if (nodeThicknessSlider != null)
        {
            nodeThicknessSlider.value = 1.0f; // Default value
            nodeThicknessSlider.onValueChanged.AddListener(OnNodeThicknessChanged);
        }
        
        if (connectionThicknessSlider != null)
        {
            connectionThicknessSlider.value = 1.0f; // Default value
            connectionThicknessSlider.onValueChanged.AddListener(OnConnectionThicknessChanged);
        }
        
        if (glowIntensitySlider != null)
        {
            glowIntensitySlider.value = 0.0f; // Default to no glow
            glowIntensitySlider.onValueChanged.AddListener(OnGlowIntensityChanged);
        }
        
        // Initialize toggles
        if (stressColorsToggle != null)
        {
            stressColorsToggle.isOn = true; // Default to on
            stressColorsToggle.onValueChanged.AddListener(OnStressColorsToggled);
        }
        
        if (autoRotateToggle != null)
        {
            autoRotateToggle.isOn = false; // Default to off
            autoRotateToggle.onValueChanged.AddListener(OnAutoRotateToggled);
        }
        
        if (showForcesToggle != null)
        {
            showForcesToggle.isOn = true; // Default to on
            showForcesToggle.onValueChanged.AddListener(OnShowForcesToggled);
        }
        
        // Initialize render mode dropdown
        if (renderModeDropdown != null)
        {
            renderModeDropdown.ClearOptions();
            renderModeDropdown.AddOptions(new List<string> { "Solid", "Transparent", "Wireframe" });
            renderModeDropdown.value = 0; // Default to solid
            renderModeDropdown.onValueChanged.AddListener(OnRenderModeChanged);
        }
        
        // Initialize buttons
        if (screenshotButton != null)
        {
            screenshotButton.onClick.AddListener(OnScreenshotButtonClicked);
        }
        
        if (resetViewButton != null)
        {
            resetViewButton.onClick.AddListener(OnResetViewButtonClicked);
        }
        
        // Initialize optional UI elements
        if (rotationSpeedSlider != null)
        {
            rotationSpeedSlider.value = 10f; // Default rotation speed
            rotationSpeedSlider.onValueChanged.AddListener(OnRotationSpeedChanged);
        }
        
        if (stressIntensitySlider != null)
        {
            stressIntensitySlider.value = 1.0f; // Default value
            stressIntensitySlider.onValueChanged.AddListener(OnStressIntensityChanged);
        }
        
        if (bloomToggle != null)
        {
            bloomToggle.isOn = false; // Default to off
            bloomToggle.onValueChanged.AddListener(OnBloomToggled);
        }
    }
    
    /// <summary>
    /// Refreshes the visualization style dropdown with current available styles
    /// </summary>
    public void RefreshStyleDropdown()
    {
        if (styleDropdown == null || visualizationManager == null) return;
        
        // Get all style names
        string[] styleNames = visualizationManager.GetAllStyleNames();
        
        // Clear and repopulate dropdown
        styleDropdown.ClearOptions();
        styleDropdown.AddOptions(new List<string>(styleNames));
        
        // Set current style
        string currentStyle = visualizationManager.GetCurrentStyleName();
        for (int i = 0; i < styleNames.Length; i++)
        {
            if (styleNames[i] == currentStyle)
            {
                styleDropdown.value = i;
                break;
            }
        }
        
        // Add listener
        styleDropdown.onValueChanged.AddListener(OnStyleChanged);
    }
    
    #region Event Handlers
    
    private void OnStyleChanged(int index)
    {
        if (visualizationManager != null)
        {
            visualizationManager.ApplyVisualizationStyle(index);
        }
    }
    
    private void OnTransparencyChanged(float value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetTransparency(value);
        }
    }
    
    private void OnNodeThicknessChanged(float value)
    {
        if (visualizationManager != null)
        {
            // Access the nodeThicknessMultiplier field - should be made public or have a setter
            var field = typeof(VisualizationManager).GetField("nodeThicknessMultiplier", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(visualizationManager, value);
                visualizationManager.ApplyCurrentStyle();
            }
        }
    }
    
    private void OnConnectionThicknessChanged(float value)
    {
        if (visualizationManager != null)
        {
            // Access the connectionThicknessMultiplier field - should be made public or have a setter
            var field = typeof(VisualizationManager).GetField("connectionThicknessMultiplier", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(visualizationManager, value);
                visualizationManager.ApplyCurrentStyle();
            }
        }
    }
    
    private void OnGlowIntensityChanged(float value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetGlowEffect(value);
        }
    }
    
    private void OnStressColorsToggled(bool value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetShowStressColors(value);
        }
    }
    
    private void OnAutoRotateToggled(bool value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetAutoRotate(value);
        }
    }
    
    private void OnShowForcesToggled(bool value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetShowForces(value);
        }
    }
    
    private void OnRenderModeChanged(int value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetRenderingMode(value);
        }
    }
    
    private void OnScreenshotButtonClicked()
    {
        if (visualizationManager != null)
        {
            visualizationManager.ExportCurrentView();
        }
    }
      private void OnResetViewButtonClicked()
    {
        if (visualizationManager != null)
        {
            // Get all nodes to frame
            MorphNode[] nodes = FindObjectsByType<MorphNode>(FindObjectsSortMode.None);
            if (nodes.Length > 0)
            {
                visualizationManager.FrameMorphology(new List<MorphNode>(nodes));
            }
        }
    }
    
    private void OnRotationSpeedChanged(float value)
    {
        if (visualizationManager != null)
        {
            visualizationManager.SetAutoRotateSpeed(value);
        }
    }
    
    private void OnStressIntensityChanged(float value)
    {
        if (visualizationManager != null)
        {
            // Access the stressColorIntensity field - should be made public or have a setter
            var field = typeof(VisualizationManager).GetField("stressColorIntensity", 
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            if (field != null)
            {
                field.SetValue(visualizationManager, value);
                visualizationManager.ApplyCurrentStyle();
            }
        }
    }
    
    private void OnBloomToggled(bool value)
    {
        // This would require adding post-processing support to the VisualizationManager
        if (visualizationManager != null)
        {
            // Example implementation if method exists:
            // visualizationManager.SetBloomEffect(value);
        }
    }
    
    #endregion
    
    /// <summary>
    /// Toggles the visualization panel visibility
    /// </summary>
    public void TogglePanel()
    {
        if (mainPanel != null)
        {
            mainPanel.SetActive(!mainPanel.activeSelf);
        }
    }
}
