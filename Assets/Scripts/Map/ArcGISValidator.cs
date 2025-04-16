using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// Validates ArcGIS components and fixes common setup issues
    /// </summary>
    [DefaultExecutionOrder(-100)] // Run early in initialization
    public class ArcGISValidator : MonoBehaviour
    {
        [SerializeField] private bool autoFix = true;
        [SerializeField] private bool logDiagnostics = true;
        
        // Reference to the ModernUIController - will be found automatically
        private BiomorphicSim.UI.ModernUIController uiController;
        
        void Awake()
        {
            Debug.Log("ArcGIS Validator: Starting component validation");
            
            // Register for the ArcGIS components initialization
            StartCoroutine(ValidateAfterDelay());
        }
        
        IEnumerator ValidateAfterDelay()
        {
            // Wait for all components to initialize
            yield return new WaitForSeconds(1f);
            
            // Run the validation
            ValidateComponents();
            
            // Periodically revalidate to ensure connection
            while (autoFix)
            {
                yield return new WaitForSeconds(5f);
                ValidateComponents();
            }
        }
        
        public bool ValidateComponents()
        {
            Debug.Log("<color=cyan>=== ArcGIS Component Validation ===</color>");
            
            // Find UI controller if not already set
            if (uiController == null)
                uiController = FindObjectOfType<BiomorphicSim.UI.ModernUIController>();
            
            // Check each component
            bool mapViewFound = ValidateComponent<ArcGISMapView>("ArcGIS Map View");
            bool cameraFound = ValidateComponent<ArcGISCameraComponent>("ArcGIS Camera Component");
            bool renderingFound = ValidateComponent<ArcGISRenderingComponent>("ArcGIS Rendering Component");
            bool mapSetupFound = ValidateComponent<MapSetup>("Map Setup");
            
            // Check if the ArcGIS map object is properly connected to the UI controller
            ValidateUIConnection();
            
            // Overall status
            bool allComponentsValid = mapViewFound && cameraFound && renderingFound && mapSetupFound;
            
            Debug.Log($"<color=cyan>=== Validation Complete: {(allComponentsValid ? "<color=green>PASSED</color>" : "<color=red>FAILED</color>")} ===</color>");
            
            return allComponentsValid;
        }
        
        private bool ValidateComponent<T>(string componentName) where T : Component
        {
            var components = FindObjectsOfType<T>();
            bool found = components != null && components.Length > 0;
            
            string status = found ? 
                "<color=green>✓ FOUND</color>" : 
                "<color=red>✗ MISSING</color>";
            
            if (logDiagnostics)
                Debug.Log($"{componentName}: {status}");
            
            if (found && logDiagnostics)
            {
                // Output which GameObject has this component
                foreach (var component in components)
                {
                    Debug.Log($" - Found on: {component.gameObject.name}");
                    
                    // Check if the rendering component is properly initialized
                    if (component is ArcGISRenderingComponent arcgisRenderer)
                    {
                        Debug.Log($" - Initialized: {arcgisRenderer.IsInitialized()}");
                        Debug.Log($" - Map Ready: {arcgisRenderer.IsMapReady()}");
                    }
                }
            }
            
            return found;
        }
        
        private void ValidateUIConnection()
        {
            if (uiController == null)
            {
                Debug.LogWarning("UI Controller not found - cannot validate connection");
                return;
            }
            
            // In UI controller, "arcgisMap" field should reference our ArcGIS map GameObject
            GameObject arcgisMapObject = FindArcGISMapObject();
            
            if (arcgisMapObject != null)
            {
                Debug.Log($"Found ArcGIS Map GameObject: {arcgisMapObject.name}");
                
                // Fix the connection at runtime - use reflection to set private fields in ModernUIController
                FixUIControllerConnection(arcgisMapObject);
            }
            else
            {
                Debug.LogError("Could not find a valid ArcGIS Map GameObject!");
            }
        }
        
        private GameObject FindArcGISMapObject()
        {
            // First check for objects that contain all required components
            ArcGISMapView[] mapViews = FindObjectsOfType<ArcGISMapView>();
            
            foreach (var mapView in mapViews)
            {
                GameObject mapObj = mapView.gameObject;
                
                // Check if this object has all required components
                if (mapObj.GetComponent<ArcGISCameraComponent>() != null &&
                    mapObj.GetComponent<ArcGISRenderingComponent>() != null &&
                    mapObj.GetComponent<MapSetup>() != null)
                {
                    return mapObj;
                }
            }
            
            // If no object has all components, return the first GameObject with ArcGISMapView
            if (mapViews.Length > 0)
                return mapViews[0].gameObject;
                
            return null;
        }
        
        private void FixUIControllerConnection(GameObject arcgisMapObject)
        {
            // Using SendMessage to call a public method on the ModernUIController
            // This avoids using reflection to set private fields
            // The UI controller needs to implement this method
            if (uiController != null && autoFix)
            {
                // Get instance of rendering component to pass along
                ArcGISRenderingComponent renderingComponent = 
                    arcgisMapObject.GetComponent<ArcGISRenderingComponent>();
                
                // Use SetArcGISMap method with both the GameObject and the component
                uiController.gameObject.SendMessage(
                    "SetArcGISMap", 
                    arcgisMapObject, 
                    SendMessageOptions.DontRequireReceiver);
                    
                Debug.Log("<color=green>✓ FIXED</color> ArcGIS Map connection to UI Controller");
            }
            else if (uiController != null)
            {
                Debug.Log("ArcGIS Map connection needs repair but autoFix is disabled");
            }
        }
    }
}
