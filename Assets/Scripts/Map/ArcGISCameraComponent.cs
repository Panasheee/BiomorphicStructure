using UnityEngine;
using System.Collections;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// Enhanced implementation of ArcGIS Camera Component
    /// This is a placeholder for the actual ESRI SDK component with improved functionality
    /// </summary>
    public class ArcGISCameraComponent : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera targetCamera;
        [SerializeField] private float movementSpeed = 10f;
        [SerializeField] private float rotationSpeed = 100f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minAltitude = 100f;
        [SerializeField] private float maxAltitude = 5000f;
        
        [Header("Camera Positioning")]
        [SerializeField] private Vector3 initialPosition = new Vector3(0, 500, -500);
        [SerializeField] private Vector3 initialRotation = new Vector3(45, 0, 0);
        [SerializeField] private bool autoPositionCamera = true;
        
        [Header("Debug")]
        [SerializeField] private bool showDebugMessages = true;
        
        private bool isInitialized = false;
        private bool isMapReady = false;
        private ArcGISMapView mapView;
        
        void Awake()
        {
            if (showDebugMessages)
                Debug.Log("[ArcGIS Camera] Initializing camera component");
                
            // If no camera is assigned, try to use the main camera
            if (targetCamera == null)
            {
                targetCamera = Camera.main;
                
                if (targetCamera == null)
                {
                    Debug.LogWarning("[ArcGIS Camera] No main camera found! Creating a new camera.");
                    
                    // Create a new camera if none exists
                    GameObject cameraObj = new GameObject("ArcGIS Camera");
                    targetCamera = cameraObj.AddComponent<Camera>();
                    targetCamera.tag = "MainCamera";
                }
                
                if (showDebugMessages)
                    Debug.Log("[ArcGIS Camera] Using main camera: " + targetCamera.name);
            }
            
            // Get reference to the map view
            mapView = GetComponent<ArcGISMapView>();
            if (mapView == null && showDebugMessages)
                Debug.LogWarning("[ArcGIS Camera] No ArcGIS Map View component found on this GameObject!");
        }
        
        void Start()
        {
            if (targetCamera != null && autoPositionCamera)
            {
                SetInitialCameraPosition();
            }
            
            isInitialized = true;
            
            if (showDebugMessages)
                Debug.Log("[ArcGIS Camera] Camera component initialized");
        }
        
        public void OnMapReady()
        {
            isMapReady = true;
            
            if (showDebugMessages)
                Debug.Log("[ArcGIS Camera] Map is ready, camera functionality fully enabled");
                
            if (autoPositionCamera)
            {
                StartCoroutine(SmoothlyFocusOnMapCenter());
            }
        }
        
        private void SetInitialCameraPosition()
        {
            if (targetCamera == null) return;
            
            targetCamera.transform.position = initialPosition;
            targetCamera.transform.eulerAngles = initialRotation;
            
            if (showDebugMessages)
                Debug.Log("[ArcGIS Camera] Set camera to initial position: " + initialPosition);
        }
        
        private IEnumerator SmoothlyFocusOnMapCenter()
        {
            if (targetCamera == null) yield break;
            
            Vector3 startPosition = targetCamera.transform.position;
            Quaternion startRotation = targetCamera.transform.rotation;
            
            Vector3 targetPosition = new Vector3(0, 500, -500); // Default view position
            Quaternion targetRotation = Quaternion.Euler(45, 0, 0);
            
            float duration = 2.0f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Apply smooth easing
                float smoothT = Mathf.SmoothStep(0, 1, t);
                
                targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
                targetCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
                
                yield return null;
            }
            
            if (showDebugMessages)
                Debug.Log("[ArcGIS Camera] Camera focused on map center");
        }
        
        public void FocusOnLocation(double latitude, double longitude, float altitude)
        {
            if (mapView == null || targetCamera == null) return;
            
            // Calculate world position from geographic coordinates
            Vector3 worldPos = mapView.GeographicToWorldPosition(latitude, longitude, 0);
            
            // Set target position above the point
            Vector3 targetPosition = worldPos + new Vector3(0, altitude, 0);
            
            StartCoroutine(SmoothlyMoveCamera(targetPosition, Quaternion.Euler(90, 0, 0)));
            
            if (showDebugMessages)
                Debug.Log($"[ArcGIS Camera] Focusing on location: Lat {latitude}, Long {longitude}");
        }
        
        public void FocusOnPoint(Vector3 worldPosition, float altitude)
        {
            if (targetCamera == null) return;
            
            Vector3 targetPosition = worldPosition + new Vector3(0, altitude, 0);
            StartCoroutine(SmoothlyMoveCamera(targetPosition, Quaternion.Euler(90, 0, 0)));
            
            if (showDebugMessages)
                Debug.Log($"[ArcGIS Camera] Focusing on point: {worldPosition}");
        }
        
        private IEnumerator SmoothlyMoveCamera(Vector3 targetPosition, Quaternion targetRotation)
        {
            if (targetCamera == null) yield break;
            
            Vector3 startPosition = targetCamera.transform.position;
            Quaternion startRotation = targetCamera.transform.rotation;
            
            float duration = 1.5f;
            float elapsed = 0;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / duration);
                
                // Apply smooth easing
                float smoothT = Mathf.SmoothStep(0, 1, t);
                
                targetCamera.transform.position = Vector3.Lerp(startPosition, targetPosition, smoothT);
                targetCamera.transform.rotation = Quaternion.Slerp(startRotation, targetRotation, smoothT);
                
                yield return null;
            }
        }
        
        public bool IsInitialized()
        {
            return isInitialized;
        }
        
        public void SetCameraTarget(Camera camera)
        {
            if (camera != null)
            {
                targetCamera = camera;
                
                if (showDebugMessages)
                    Debug.Log("[ArcGIS Camera] Camera target set to: " + camera.name);
                    
                if (isInitialized && autoPositionCamera)
                {
                    SetInitialCameraPosition();
                }
            }
        }
    }
}
