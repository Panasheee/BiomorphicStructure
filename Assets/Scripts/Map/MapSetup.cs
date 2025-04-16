// filepath: c:\Users\tyron\BiomorphicSim\Assets\Scripts\Map\MapSetup.cs
using UnityEngine;
using System;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// Sets up the map component with satellite imagery and elevation data for Wellington.
    /// Currently uses a simplified implementation until ArcGIS SDK issues are resolved.
    /// </summary>
    public class MapSetup : MonoBehaviour
    {
        // Wellington, New Zealand (Lambton Quay area) coordinates
        [SerializeField] private double latitude = -41.2865;
        [SerializeField] private double longitude = 174.7762;
        [SerializeField] private double altitude = 1000;
        
        // Map configuration
        [SerializeField] private string imageryServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
        [SerializeField] private string elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";
        
        void Start()
        {
            Debug.Log("MapSetup: Starting initialization of map");
            Debug.Log($"Map will be centered at: Longitude {longitude}, Latitude {latitude}, Altitude {altitude}m");
            Debug.Log($"Using imagery service: {imageryServiceUrl}");
            Debug.Log($"Using elevation service: {elevationServiceUrl}");
            
            // Check if we have an ArcGIS map component
            var mapComponent = GetComponent<MonoBehaviour>();
            if (mapComponent != null)
            {
                Debug.Log("Found map component on this GameObject");
                
                // Implementation will be completed after ArcGIS SDK integration is fixed
                // For now, just log that we need to set up the map properly
                Debug.LogWarning("ArcGIS SDK integration needs to be fixed before map setup can be completed");
            }
            else
            {
                Debug.LogWarning("No map component found on this GameObject");
            }
        }
    }
}
