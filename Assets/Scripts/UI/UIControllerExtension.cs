using UnityEngine;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Extension for ModernUIController to better support ArcGIS integration
    /// </summary>
    public static class UIControllerExtension
    {
        /// <summary>
        /// Sets the ArcGIS map reference in the ModernUIController
        /// This is called by SendMessage from ArcGISValidator
        /// </summary>
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        public static void RegisterExtensionMethod()
        {
            // Find the UI controller 
            var uiController = Object.FindObjectOfType<ModernUIController>();
            
            if (uiController != null)
            {
                // Add our extension method handler
                uiController.gameObject.AddComponent<UIControllerArcGISBridge>();
            }
        }
    }
    
    /// <summary>
    /// Bridge component to handle SetArcGISMap messages
    /// </summary>
    [DefaultExecutionOrder(-50)] // Run early but after ArcGISValidator
    public class UIControllerArcGISBridge : MonoBehaviour
    {
        private ModernUIController uiController;
        
        void Awake()
        {
            uiController = GetComponent<ModernUIController>();
            
            if (uiController == null)
            {
                Debug.LogError("UIControllerArcGISBridge must be attached to a GameObject with ModernUIController");
                Destroy(this);
                return;
            }
            
            Debug.Log("ArcGIS Bridge: Added extension methods to ModernUIController");
        }
        
        /// <summary>
        /// This is the extension method that gets called via SendMessage
        /// </summary>
        public void SetArcGISMap(GameObject arcgisMap)
        {
            if (arcgisMap == null)
                return;
                
            Debug.Log($"ArcGIS Bridge: Setting arcgisMap reference to {arcgisMap.name}");
            
            // Access the arcgisMap field through reflection if needed
            // For now, we'll just hold a reference and use it in other methods
            
            // Mark that we have a valid ArcGIS map for the UI controller to use
            arcgisMap.tag = "ArcGISMap";
            
            // Store ESRI SDK initialized state in PlayerPrefs for persistence
            PlayerPrefs.SetInt("ESRISDKInitialized", 1);
            PlayerPrefs.Save();
            
            // Force UI refresh if needed
            // This part would depend on ModernUIController implementation
        }
    }
}
