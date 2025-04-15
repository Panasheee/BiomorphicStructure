using UnityEngine;
using TMPro;
using UnityEngine.UI;
using BiomorphicSim.Core;
using BiomorphicSim.Morphology;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Main UI controller for the BiomorphicSim application.
    /// Handles UI events and interactions with the core systems.
    /// </summary>
    public class MainUIController : MonoBehaviour
    {
        [Header("System References")]
        [SerializeField] private MainController mainController;
        [SerializeField] private SiteGenerator siteGenerator;
        [SerializeField] private MorphologyManager morphologyManager;
        [SerializeField] private ScenarioAnalyzer scenarioAnalyzer;
        [SerializeField] private VisualizationManager visualizationManager;
        
        [Header("UI Elements")]
        [SerializeField] private GameObject mainPanel;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private GameObject visualizationPanel;
        [SerializeField] private GameObject scenarioPanel;
        
        // Start is called before the first frame update
        void Start()
        {
            Initialize();
        }
        
        public void Initialize()
        {
            Debug.Log("Initializing UI Manager...");
            
            // Find references if not set
            if (mainController == null) mainController = FindFirstObjectByType<MainController>(FindObjectsInactive.Include);
            if (siteGenerator == null) siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            if (morphologyManager == null) morphologyManager = FindFirstObjectByType<MorphologyManager>(FindObjectsInactive.Include);
            if (scenarioAnalyzer == null) scenarioAnalyzer = FindFirstObjectByType<ScenarioAnalyzer>(FindObjectsInactive.Include);
            if (visualizationManager == null) visualizationManager = FindFirstObjectByType<VisualizationManager>(FindObjectsInactive.Include);
            
            // Set up UI panels
            ShowMainPanel();
        }
        
        // UI navigation methods
        public void ShowMainPanel()
        {
            TogglePanel(mainPanel, true);
            TogglePanel(settingsPanel, false);
            TogglePanel(visualizationPanel, false);
            TogglePanel(scenarioPanel, false);
        }
        
        public void ShowSettingsPanel()
        {
            TogglePanel(mainPanel, false);
            TogglePanel(settingsPanel, true);
            TogglePanel(visualizationPanel, false);
            TogglePanel(scenarioPanel, false);
        }
        
        public void ShowVisualizationPanel()
        {
            TogglePanel(mainPanel, false);
            TogglePanel(settingsPanel, false);
            TogglePanel(visualizationPanel, true);
            TogglePanel(scenarioPanel, false);
        }
        
        public void ShowScenarioPanel()
        {
            TogglePanel(mainPanel, false);
            TogglePanel(settingsPanel, false);
            TogglePanel(visualizationPanel, false);
            TogglePanel(scenarioPanel, true);
        }
        
        private void TogglePanel(GameObject panel, bool show)
        {
            if (panel != null)
                panel.SetActive(show);
        }
        
        // Methods needed for scenario analyzer compatibility
        public void UpdateScenarioProgress(float progress, string scenarioName)
        {
            Debug.Log($"Scenario {scenarioName} progress: {progress:P0}");
            
            // Update progress bar (if you have one)
            // For example:
            Image progressBar = GetComponentInChildren<Image>(true);
            if (progressBar != null && progressBar.name.Contains("Progress"))
            {
                progressBar.fillAmount = progress;
            }
            
            // Update status text
            TextMeshProUGUI statusText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (statusText != null && statusText.name.Contains("Status"))
            {
                statusText.text = $"RUNNING: {scenarioName.ToUpper()} ({progress:P0})";
            }
        }
        
        public void ScenarioCompleted(string scenarioName)
        {
            Debug.Log($"Scenario {scenarioName} completed");
            
            // Update UI to show completion
            // For example:
            TextMeshProUGUI statusText = GetComponentInChildren<TextMeshProUGUI>(true);
            if (statusText != null && statusText.name.Contains("Status"))
            {
                statusText.text = $"COMPLETED: {scenarioName.ToUpper()}";
            }
            
            // Show results
            // Add code to display scenario results or transition to results screen
        }
    }
}
