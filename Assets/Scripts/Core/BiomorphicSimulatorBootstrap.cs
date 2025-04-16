using UnityEngine;
using BiomorphicSim.Map;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Bootstraps the Biomorphic Simulator by finding or creating all necessary components
    /// </summary>
    public class BiomorphicSimulatorBootstrap : MonoBehaviour
    {
        // Add the following method to automatically create ArcGIS map if missing
        private void EnsureArcGISMapExists()
        {
            // Look for existing ArcGIS Map
            GameObject arcgisMap = GameObject.Find("ArcGISMap");
            
            // If no ArcGIS Map exists, create one
            if (arcgisMap == null)
            {
                Debug.Log("Creating ArcGIS Map object - it's required for proper map display");
                arcgisMap = ArcGISMapUtility.CreateArcGISMap();
            }
            else
            {
                // Ensure it has all required components
                if (arcgisMap.GetComponent<ArcGISMapView>() == null ||
                    arcgisMap.GetComponent<ArcGISCameraComponent>() == null ||
                    arcgisMap.GetComponent<ArcGISRenderingComponent>() == null ||
                    arcgisMap.GetComponent<MapSetup>() == null)
                {
                    Debug.Log("ArcGIS Map object found but missing some components. Adding missing components...");
                    
                    if (arcgisMap.GetComponent<ArcGISMapView>() == null)
                        arcgisMap.AddComponent<ArcGISMapView>();
                        
                    if (arcgisMap.GetComponent<ArcGISCameraComponent>() == null)
                        arcgisMap.AddComponent<ArcGISCameraComponent>();
                        
                    if (arcgisMap.GetComponent<ArcGISRenderingComponent>() == null)
                        arcgisMap.AddComponent<ArcGISRenderingComponent>();
                        
                    if (arcgisMap.GetComponent<MapSetup>() == null)
                        arcgisMap.AddComponent<MapSetup>();
                }
            }
            
            // Update the MainController or ModernUIController reference to the ArcGIS Map
            UpdateArcGISMapReferences(arcgisMap);
        }
        
        private void UpdateArcGISMapReferences(GameObject arcgisMap)
        {
            // Find ModernUIController to update its reference
            UI.ModernUIController uiController = FindObjectOfType<UI.ModernUIController>();
            if (uiController != null)
            {
                // Use reflection to set the arcgisMapObject reference
                System.Reflection.FieldInfo field = 
                    typeof(UI.ModernUIController).GetField("arcgisMapObject", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                if (field != null)
                {
                    field.SetValue(uiController, arcgisMap);
                    Debug.Log("Updated ModernUIController reference to ArcGIS Map");
                }
            }
            
            // Also update MainController reference if it exists
            MainController mainController = FindObjectOfType<MainController>();
            if (mainController != null)
            {
                // Use reflection to update MainController if it has an arcgisMap field
                System.Reflection.FieldInfo field = 
                    typeof(MainController).GetField("arcgisMap", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                if (field != null)
                {
                    field.SetValue(mainController, arcgisMap);
                    Debug.Log("Updated MainController reference to ArcGIS Map");
                }
            }
        }
        
        private SiteGenerator siteGenerator;  // Add this field if it doesn't exist
        
        /// <summary>
        /// Returns a reference to the site generator component
        /// </summary>
        /// <returns>The SiteGenerator instance</returns>
        public SiteGenerator GetSiteGenerator()
        {
            if (siteGenerator == null)
            {
                // Try to find the SiteGenerator if not already assigned
                siteGenerator = FindObjectOfType<SiteGenerator>();
                
                if (siteGenerator == null)
                {
                    Debug.LogWarning("SiteGenerator not found. Creating a new SiteGenerator instance.");
                    GameObject siteGenObj = new GameObject("SiteGenerator");
                    siteGenerator = siteGenObj.AddComponent<SiteGenerator>();
                }
            }
            
            return siteGenerator;
        }
        
        // Call this method from your Awake or Start method
        // Example:
        // void Awake()
        // {
        //     EnsureArcGISMapExists();
        //     // your other initialization code...
        // }
    }
}