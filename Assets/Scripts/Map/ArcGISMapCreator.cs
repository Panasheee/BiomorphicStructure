using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEditor;
using BiomorphicSim.UI;  // For LoadingSpinner reference

namespace BiomorphicSim.Map
{
#if UNITY_EDITOR
    /// <summary>
    /// Editor utility for automatically creating and setting up an ArcGIS Map GameObject
    /// with all required components
    /// </summary>
    public class ArcGISMapCreator : MonoBehaviour
    {
        // Menu item to create a new ArcGIS Map
        [MenuItem("BiomorphicSim/Create/ArcGIS Map")]
        public static void CreateArcGISMap()
        {
            // Create the main map object
            GameObject mapObject = new GameObject("ArcGISMap");
            
            // Add all required components
            mapObject.AddComponent<ArcGISMapView>();
            mapObject.AddComponent<ArcGISCameraComponent>();
            mapObject.AddComponent<ArcGISRenderingComponent>();
            mapObject.AddComponent<MapSetup>();
            
            // Position it at the origin
            mapObject.transform.position = Vector3.zero;
            
            // Select the new object
            Selection.activeGameObject = mapObject;
            
            Debug.Log("Created new ArcGIS Map GameObject with all required components");
            
            // Update references in ModernUIController if found
            UpdateModernUIControllerReferences(mapObject);
        }
        
        private static void UpdateModernUIControllerReferences(GameObject arcgisMapObject)
        {
            // Find ModernUIController
            BiomorphicSim.UI.ModernUIController uiController = 
                GameObject.FindObjectOfType<BiomorphicSim.UI.ModernUIController>();
                
            if (uiController != null)
            {
                // Use reflection to set the arcgisMapObject reference
                System.Reflection.FieldInfo field = 
                    typeof(BiomorphicSim.UI.ModernUIController).GetField("arcgisMapObject", 
                        System.Reflection.BindingFlags.Instance | 
                        System.Reflection.BindingFlags.Public | 
                        System.Reflection.BindingFlags.NonPublic);
                        
                if (field != null)
                {
                    field.SetValue(uiController, arcgisMapObject);
                    Debug.Log("Updated ModernUIController reference to new ArcGIS Map GameObject");
                }
            }
        }
        
        // Menu item to fix reference missing errors
        [MenuItem("BiomorphicSim/Fix/Missing ArcGIS Components")]
        public static void FixMissingArcGISComponents()
        {
            // Find existing ArcGIS Map GameObject
            GameObject existingMap = GameObject.Find("ArcGISMap");
            
            if (existingMap != null)
            {
                // Check for and add missing components
                bool hasMapView = existingMap.GetComponent<ArcGISMapView>() != null;
                bool hasCameraComponent = existingMap.GetComponent<ArcGISCameraComponent>() != null;
                bool hasRenderingComponent = existingMap.GetComponent<ArcGISRenderingComponent>() != null;
                bool hasMapSetup = existingMap.GetComponent<MapSetup>() != null;
                
                if (!hasMapView)
                {
                    existingMap.AddComponent<ArcGISMapView>();
                    Debug.Log("Added missing ArcGIS Map View component");
                }
                
                if (!hasCameraComponent)
                {
                    existingMap.AddComponent<ArcGISCameraComponent>();
                    Debug.Log("Added missing ArcGIS Camera Component");
                }
                
                if (!hasRenderingComponent)
                {
                    existingMap.AddComponent<ArcGISRenderingComponent>();
                    Debug.Log("Added missing ArcGIS Rendering Component");
                }
                
                if (!hasMapSetup)
                {
                    existingMap.AddComponent<MapSetup>();
                    Debug.Log("Added missing MapSetup component");
                }
                
                // Update ModernUIController reference
                UpdateModernUIControllerReferences(existingMap);
                
                Debug.Log("Fixed missing ArcGIS components on existing ArcGISMap GameObject");
            }
            else
            {
                // If no map exists, create a new one
                CreateArcGISMap();
            }
        }
    }
#endif

    /// <summary>
    /// Runtime utility for creating ArcGIS Map objects
    /// Can be used to create maps at runtime or in initialization code
    /// </summary>
    public static class ArcGISMapUtility
    {
        /// <summary>
        /// Creates a new ArcGIS Map GameObject with all required components at runtime
        /// </summary>
        public static GameObject CreateArcGISMap()
        {
            // Create the main map object
            GameObject mapObject = new GameObject("ArcGISMap");
            
            // Add all required components
            mapObject.AddComponent<ArcGISMapView>();
            mapObject.AddComponent<ArcGISCameraComponent>();
            mapObject.AddComponent<ArcGISRenderingComponent>();
            MapSetup mapSetup = mapObject.AddComponent<MapSetup>();
            
            // Position it at the origin
            mapObject.transform.position = Vector3.zero;
            
            // Initialize the map
            mapSetup.InitializeMap();
            
            Debug.Log("Created new ArcGIS Map GameObject with all required components at runtime");
            
            return mapObject;
        }
        
        /// <summary>
        /// Finds an existing ArcGIS Map or creates a new one if none exists
        /// </summary>
        public static GameObject GetOrCreateArcGISMap()
        {
            // Look for existing map first
            GameObject existingMap = GameObject.Find("ArcGISMap");
            
            if (existingMap != null)
            {
                // Ensure it has all required components
                if (existingMap.GetComponent<ArcGISMapView>() == null)
                    existingMap.AddComponent<ArcGISMapView>();
                    
                if (existingMap.GetComponent<ArcGISCameraComponent>() == null)
                    existingMap.AddComponent<ArcGISCameraComponent>();
                    
                if (existingMap.GetComponent<ArcGISRenderingComponent>() == null)
                    existingMap.AddComponent<ArcGISRenderingComponent>();
                    
                if (existingMap.GetComponent<MapSetup>() == null)
                    existingMap.AddComponent<MapSetup>();
                
                return existingMap;
            }
            
            // Create new map if none exists
            return CreateArcGISMap();
        }
    }
}
