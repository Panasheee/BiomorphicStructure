// filepath: c:\Users\tyron\BiomorphicSim\Assets\Scripts\Map\MapSetup.cs
using UnityEngine;
using System.Collections;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// Enhanced implementation of MapSetup
    /// Handles the setup and configuration of the ArcGIS map components
    /// </summary>
    public class MapSetup : MonoBehaviour
    {
        // Wellington, New Zealand (Lambton Quay area) coordinates
        [Header("Location Settings")]
        [SerializeField] private double latitude = -41.2865;
        [SerializeField] private double longitude = 174.7762;
        [SerializeField] private double altitude = 1000;
        
        [Header("Map Configuration")]
        [SerializeField] private string imageryServiceUrl = "https://services.arcgisonline.com/ArcGIS/rest/services/World_Imagery/MapServer";
        [SerializeField] private string elevationServiceUrl = "https://elevation3d.arcgis.com/arcgis/rest/services/WorldElevation3D/Terrain3D/ImageServer";
        [SerializeField] private bool autoInitialize = true;
        
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private GameObject originalTerrain;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugMessages = true;
        
        private ArcGISMapView mapView;
        private ArcGISCameraComponent cameraComponent;
        private ArcGISRenderingComponent renderingComponent;
        private bool isInitialized = false;
        
        void Awake()
        {
            if (showDebugMessages)
                Debug.Log("MapSetup: Initializing map setup component");
                
            // Find or add required ArcGIS components
            EnsureRequiredComponents();
        }
        
        void Start()
        {
            if (showDebugMessages)
            {
                Debug.Log("MapSetup: Starting initialization of map");
                Debug.Log($"Map will be centered at: Longitude {longitude}, Latitude {latitude}, Altitude {altitude}m");
                Debug.Log($"Using imagery service: {imageryServiceUrl}");
                Debug.Log($"Using elevation service: {elevationServiceUrl}");
            }
            
            // Find the main camera if not assigned
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            // Configure camera component if it exists
            if (cameraComponent != null && mainCamera != null)
            {
                cameraComponent.SetCameraTarget(mainCamera);
                
                if (showDebugMessages)
                    Debug.Log("MapSetup: Camera component configured with main camera");
            }
            
            // Disable original terrain if it exists and we're using ArcGIS
            if (originalTerrain != null)
            {
                originalTerrain.SetActive(false);
                
                if (showDebugMessages)
                    Debug.Log("MapSetup: Disabled original terrain");
            }
            
            if (autoInitialize)
                StartCoroutine(DelayedInitializeMap());
        }
        
        private IEnumerator DelayedInitializeMap()
        {
            // Wait a short moment to let other components initialize
            yield return new WaitForSeconds(0.5f);
            
            InitializeMap();
        }
        
        public void InitializeMap()
        {
            if (isInitialized)
            {
                if (showDebugMessages)
                    Debug.Log("MapSetup: Map already initialized");
                return;
            }
            
            if (showDebugMessages)
                Debug.Log("MapSetup: Initializing map...");
                
            // Configure map view component if it exists
            if (mapView != null)
            {
                mapView.SetLocation(latitude, longitude, altitude);
                
                if (showDebugMessages)
                    Debug.Log("MapSetup: Map view configured with coordinates");
            }
            
            // Mark as initialized
            isInitialized = true;
            
            if (showDebugMessages)
                Debug.Log("MapSetup: Map initialization complete");
        }
        
        private void EnsureRequiredComponents()
        {
            // Check for and add Map View component if missing
            mapView = GetComponent<ArcGISMapView>();
            if (mapView == null)
            {
                mapView = gameObject.AddComponent<ArcGISMapView>();
                if (showDebugMessages)
                    Debug.Log("MapSetup: Added missing ArcGIS Map View component");
            }
            
            // Check for and add Camera component if missing
            cameraComponent = GetComponent<ArcGISCameraComponent>();
            if (cameraComponent == null)
            {
                cameraComponent = gameObject.AddComponent<ArcGISCameraComponent>();
                if (showDebugMessages)
                    Debug.Log("MapSetup: Added missing ArcGIS Camera Component");
            }
            
            // Check for and add Rendering component if missing
            renderingComponent = GetComponent<ArcGISRenderingComponent>();
            if (renderingComponent == null)
            {
                renderingComponent = gameObject.AddComponent<ArcGISRenderingComponent>();
                if (showDebugMessages)
                    Debug.Log("MapSetup: Added missing ArcGIS Rendering Component");
            }
        }
        
        // Allows external scripts to get a reference to the map components
        public ArcGISMapView GetMapView() => mapView;
        public ArcGISCameraComponent GetCameraComponent() => cameraComponent;
        public ArcGISRenderingComponent GetRenderingComponent() => renderingComponent;
        
        public void SetLocation(double lat, double lon, double alt)
        {
            latitude = lat;
            longitude = lon;
            altitude = alt;
            
            if (mapView != null && isInitialized)
            {
                mapView.SetLocation(latitude, longitude, altitude);
                
                if (showDebugMessages)
                    Debug.Log($"MapSetup: Updated map location to: Lat {latitude}, Long {longitude}, Alt {altitude}m");
            }
        }
        
        public void EnableTerrainElevation(bool enabled)
        {
            if (renderingComponent != null)
            {
                renderingComponent.SetTerrainElevationEnabled(enabled);
            }
        }
        
        public void Enable3DBuildings(bool enabled)
        {
            if (renderingComponent != null)
            {
                renderingComponent.Set3DBuildingsEnabled(enabled);
            }
        }
    }
}
