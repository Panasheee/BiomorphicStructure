using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Helper class to ensure the SiteViewerScreen has all necessary UI elements
    /// </summary>
    public class SiteViewerScreenSetup : MonoBehaviour
    {
        [Header("Required UI Elements")]
        [SerializeField] private Button backButton;
        [SerializeField] private Button runAnalysisButton;
        [SerializeField] private Button toggleWindButton;
        [SerializeField] private Button toggleSunButton;
        [SerializeField] private Button togglePedestrianButton;
        
        [Header("Optional UI Elements")]
        [SerializeField] private TextMeshProUGUI siteNameText;
        [SerializeField] private TextMeshProUGUI siteInfoText;
        
        private ModernUIController uiController;
        
        private void Awake()
        {
            // Find UI controller
            uiController = FindFirstObjectByType<ModernUIController>();
            
            if (uiController == null)
            {
                Debug.LogError("SiteViewerScreenSetup could not find ModernUIController!");
                return;
            }
            
            // Make sure all required UI elements exist
            CreateMissingElements();
            
            // Link buttons to the UI controller
            LinkButtonsToController();
        }
        
        private void CreateMissingElements()
        {
            // Get the rect transform of this screen
            RectTransform screenRect = GetComponent<RectTransform>();
            if (screenRect == null)
            {
                screenRect = gameObject.AddComponent<RectTransform>();
                screenRect.anchorMin = Vector2.zero;
                screenRect.anchorMax = Vector2.one;
                screenRect.offsetMin = Vector2.zero;
                screenRect.offsetMax = Vector2.zero;
            }
            
            // Create back button if missing
            if (backButton == null)
            {
                GameObject backButtonObj = new GameObject("BackButton");
                backButtonObj.transform.SetParent(transform, false);
                
                RectTransform backRect = backButtonObj.AddComponent<RectTransform>();
                backRect.anchorMin = new Vector2(0.02f, 0.93f);
                backRect.anchorMax = new Vector2(0.12f, 0.98f);
                backRect.offsetMin = Vector2.zero;
                backRect.offsetMax = Vector2.zero;
                
                Image backImg = backButtonObj.AddComponent<Image>();
                backImg.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
                backButton = backButtonObj.AddComponent<Button>();
                backButton.transition = Selectable.Transition.ColorTint;
                
                // Add text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(backButtonObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.text = "BACK";
                tmpText.color = Color.white;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.fontSize = 16;
            }
            
            // Create run analysis button if missing
            if (runAnalysisButton == null)
            {
                GameObject runButtonObj = new GameObject("RunAnalysisButton");
                runButtonObj.transform.SetParent(transform, false);
                
                RectTransform runRect = runButtonObj.AddComponent<RectTransform>();
                runRect.anchorMin = new Vector2(0.85f, 0.93f);
                runRect.anchorMax = new Vector2(0.98f, 0.98f);
                runRect.offsetMin = Vector2.zero;
                runRect.offsetMax = Vector2.zero;
                
                Image runImg = runButtonObj.AddComponent<Image>();
                runImg.color = new Color(0.2f, 0.6f, 0.3f, 0.8f);
                
                runAnalysisButton = runButtonObj.AddComponent<Button>();
                runAnalysisButton.transition = Selectable.Transition.ColorTint;
                
                // Add text
                GameObject textObj = new GameObject("Text");
                textObj.transform.SetParent(runButtonObj.transform, false);
                
                RectTransform textRect = textObj.AddComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;
                
                TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                tmpText.text = "RUN ANALYSIS";
                tmpText.color = Color.white;
                tmpText.alignment = TextAlignmentOptions.Center;
                tmpText.fontSize = 16;
            }
            
            // Create toggle buttons panel if missing
            if (toggleWindButton == null || toggleSunButton == null || togglePedestrianButton == null)
            {
                GameObject togglePanel = new GameObject("TogglePanel");
                togglePanel.transform.SetParent(transform, false);
                
                RectTransform panelRect = togglePanel.AddComponent<RectTransform>();
                panelRect.anchorMin = new Vector2(0.02f, 0.7f);
                panelRect.anchorMax = new Vector2(0.15f, 0.9f);
                panelRect.offsetMin = Vector2.zero;
                panelRect.offsetMax = Vector2.zero;
                
                // Create wind toggle button
                if (toggleWindButton == null)
                {
                    GameObject windButtonObj = new GameObject("WindToggleButton");
                    windButtonObj.transform.SetParent(togglePanel.transform, false);
                    
                    RectTransform windRect = windButtonObj.AddComponent<RectTransform>();
                    windRect.anchorMin = new Vector2(0, 0.7f);
                    windRect.anchorMax = new Vector2(1, 0.9f);
                    windRect.offsetMin = Vector2.zero;
                    windRect.offsetMax = Vector2.zero;
                    
                    Image windImg = windButtonObj.AddComponent<Image>();
                    windImg.color = new Color(0.2f, 0.2f, 0.6f, 0.8f);
                    
                    toggleWindButton = windButtonObj.AddComponent<Button>();
                    toggleWindButton.transition = Selectable.Transition.ColorTint;
                    
                    // Add text
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(windButtonObj.transform, false);
                    
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                    
                    TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                    tmpText.text = "WIND";
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontSize = 14;
                }
                
                // Create sun exposure toggle button
                if (toggleSunButton == null)
                {
                    GameObject sunButtonObj = new GameObject("SunToggleButton");
                    sunButtonObj.transform.SetParent(togglePanel.transform, false);
                    
                    RectTransform sunRect = sunButtonObj.AddComponent<RectTransform>();
                    sunRect.anchorMin = new Vector2(0, 0.4f);
                    sunRect.anchorMax = new Vector2(1, 0.6f);
                    sunRect.offsetMin = Vector2.zero;
                    sunRect.offsetMax = Vector2.zero;
                    
                    Image sunImg = sunButtonObj.AddComponent<Image>();
                    sunImg.color = new Color(0.6f, 0.6f, 0.2f, 0.8f);
                    
                    toggleSunButton = sunButtonObj.AddComponent<Button>();
                    toggleSunButton.transition = Selectable.Transition.ColorTint;
                    
                    // Add text
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(sunButtonObj.transform, false);
                    
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                    
                    TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                    tmpText.text = "SUN";
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontSize = 14;
                }
                
                // Create pedestrian toggle button
                if (togglePedestrianButton == null)
                {
                    GameObject pedButtonObj = new GameObject("PedestrianToggleButton");
                    pedButtonObj.transform.SetParent(togglePanel.transform, false);
                    
                    RectTransform pedRect = pedButtonObj.AddComponent<RectTransform>();
                    pedRect.anchorMin = new Vector2(0, 0.1f);
                    pedRect.anchorMax = new Vector2(1, 0.3f);
                    pedRect.offsetMin = Vector2.zero;
                    pedRect.offsetMax = Vector2.zero;
                    
                    Image pedImg = pedButtonObj.AddComponent<Image>();
                    pedImg.color = new Color(0.6f, 0.2f, 0.2f, 0.8f);
                    
                    togglePedestrianButton = pedButtonObj.AddComponent<Button>();
                    togglePedestrianButton.transition = Selectable.Transition.ColorTint;
                    
                    // Add text
                    GameObject textObj = new GameObject("Text");
                    textObj.transform.SetParent(pedButtonObj.transform, false);
                    
                    RectTransform textRect = textObj.AddComponent<RectTransform>();
                    textRect.anchorMin = Vector2.zero;
                    textRect.anchorMax = Vector2.one;
                    textRect.offsetMin = Vector2.zero;
                    textRect.offsetMax = Vector2.zero;
                    
                    TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
                    tmpText.text = "PEDESTRIAN";
                    tmpText.color = Color.white;
                    tmpText.alignment = TextAlignmentOptions.Center;
                    tmpText.fontSize = 12;
                }
            }
            
            // Create site info panel if missing
            if (siteNameText == null || siteInfoText == null)
            {
                GameObject infoPanel = new GameObject("SiteInfoPanel");
                infoPanel.transform.SetParent(transform, false);
                
                RectTransform infoRect = infoPanel.AddComponent<RectTransform>();
                infoRect.anchorMin = new Vector2(0.7f, 0.02f);
                infoRect.anchorMax = new Vector2(0.98f, 0.15f);
                infoRect.offsetMin = Vector2.zero;
                infoRect.offsetMax = Vector2.zero;
                
                Image infoImg = infoPanel.AddComponent<Image>();
                infoImg.color = new Color(0.2f, 0.2f, 0.2f, 0.6f);
                
                // Create site name text
                if (siteNameText == null)
                {
                    GameObject nameObj = new GameObject("SiteNameText");
                    nameObj.transform.SetParent(infoPanel.transform, false);
                    
                    RectTransform nameRect = nameObj.AddComponent<RectTransform>();
                    nameRect.anchorMin = new Vector2(0, 0.6f);
                    nameRect.anchorMax = new Vector2(1, 1);
                    nameRect.offsetMin = new Vector2(10, 0);
                    nameRect.offsetMax = new Vector2(-10, 0);
                    
                    siteNameText = nameObj.AddComponent<TextMeshProUGUI>();
                    siteNameText.text = "Lambton Quay, Wellington";
                    siteNameText.color = Color.white;
                    siteNameText.alignment = TextAlignmentOptions.Left;
                    siteNameText.fontSize = 16;
                    siteNameText.fontStyle = FontStyles.Bold;
                }
                
                // Create site info text
                if (siteInfoText == null)
                {
                    GameObject infoTextObj = new GameObject("SiteInfoText");
                    infoTextObj.transform.SetParent(infoPanel.transform, false);
                    
                    RectTransform infoTextRect = infoTextObj.AddComponent<RectTransform>();
                    infoTextRect.anchorMin = new Vector2(0, 0);
                    infoTextRect.anchorMax = new Vector2(1, 0.6f);
                    infoTextRect.offsetMin = new Vector2(10, 5);
                    infoTextRect.offsetMax = new Vector2(-10, 0);
                    
                    siteInfoText = infoTextObj.AddComponent<TextMeshProUGUI>();
                    siteInfoText.text = "Click to select zone for morphology growth";
                    siteInfoText.color = Color.white;
                    siteInfoText.alignment = TextAlignmentOptions.Left;
                    siteInfoText.fontSize = 12;
                }
            }
        }
        
        private void LinkButtonsToController()
        {
            if (uiController == null)
                return;
                  // Link back button
            if (backButton != null)
            {
                backButton.onClick.RemoveAllListeners();
                backButton.onClick.AddListener(() => {
                    // Go back to start screen
                    if (uiController != null) {
                        uiController.ShowOnlyScreen(uiController.GetStartScreen());
                    }
                });
            }
            
            // Run analysis button is already linked in ModernUIController.SetupUIEvents()
              // Link toggle buttons
            if (toggleWindButton != null)
            {
                toggleWindButton.onClick.RemoveAllListeners();
                toggleWindButton.onClick.AddListener(() => {
                    // Toggle wind overlay via the visualization manager
                    if (uiController != null && uiController.GetVisualizationManager() != null) {
                        bool currentState = toggleWindButton.GetComponent<Image>().color.a > 0.5f;
                        uiController.GetVisualizationManager().ToggleWindOverlay(!currentState);
                        
                        // Visual feedback
                        Color color = toggleWindButton.GetComponent<Image>().color;
                        toggleWindButton.GetComponent<Image>().color = new Color(
                            color.r, color.g, color.b, 
                            !currentState ? 1.0f : 0.5f);
                    }
                });
            }
              if (toggleSunButton != null)
            {
                toggleSunButton.onClick.RemoveAllListeners();
                toggleSunButton.onClick.AddListener(() => {
                    // Toggle sun exposure overlay via the visualization manager
                    if (uiController != null && uiController.GetVisualizationManager() != null) {
                        bool currentState = toggleSunButton.GetComponent<Image>().color.a > 0.5f;
                        uiController.GetVisualizationManager().ToggleSunExposureOverlay(!currentState);
                        
                        // Visual feedback
                        Color color = toggleSunButton.GetComponent<Image>().color;
                        toggleSunButton.GetComponent<Image>().color = new Color(
                            color.r, color.g, color.b, 
                            !currentState ? 1.0f : 0.5f);
                    }
                });
            }
              if (togglePedestrianButton != null)
            {
                togglePedestrianButton.onClick.RemoveAllListeners();
                togglePedestrianButton.onClick.AddListener(() => {
                    // Toggle pedestrian overlay via the visualization manager
                    if (uiController != null && uiController.GetVisualizationManager() != null) {
                        bool currentState = togglePedestrianButton.GetComponent<Image>().color.a > 0.5f;
                        uiController.GetVisualizationManager().TogglePedestrianOverlay(!currentState);
                        
                        // Visual feedback
                        Color color = togglePedestrianButton.GetComponent<Image>().color;
                        togglePedestrianButton.GetComponent<Image>().color = new Color(
                            color.r, color.g, color.b, 
                            !currentState ? 1.0f : 0.5f);
                    }
                });
            }
        }
    }
}
