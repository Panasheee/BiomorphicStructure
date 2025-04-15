using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Manages the visualization aspects of the simulation.
    /// Controls camera movement, rendering styles, and visual effects.
    /// </summary>
    public class VisualizationManager : MonoBehaviour
    {
        [Header("Camera Settings")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private float rotationSpeed = 10f;
        [SerializeField] private float zoomSpeed = 5f;
        [SerializeField] private float minZoom = 5f;
        [SerializeField] private float maxZoom = 50f;
        [SerializeField] private Vector3 focusPoint = Vector3.zero;
        
        [Header("Render Settings")]
        [SerializeField] private Material nodeMaterial;
        [SerializeField] private Material connectionMaterial;
        [SerializeField] private Material wireframeMaterial;
        [SerializeField] private Material stressMaterial;
        
        [Header("Visual Effects")]
        [SerializeField] private GameObject vectorFieldPrefab;
        [SerializeField] private float vectorFieldScale = 1.0f;
        [SerializeField] private int vectorFieldDensity = 10;
        [SerializeField] private bool showVectorField = false;
        
        // Internal state
        private bool autoRotate = false;
        private bool wireframeMode = false;
        private Transform vectorFieldParent;
        private GameObject[] vectorField;
        
        public void Initialize()
        {
            Debug.Log("Initializing Visualization Manager...");
            
            // Find camera if not assigned
            if (mainCamera == null)
                mainCamera = Camera.main;
                
            // Create parent for vector field
            if (vectorFieldParent == null)
            {
                vectorFieldParent = new GameObject("VectorField").transform;
                vectorFieldParent.SetParent(transform);
            }
        }
        
        private void Update()
        {
            // Handle camera rotation
            if (autoRotate)
            {
                RotateCamera();
            }
            
            // Handle input (in a real implementation, you would use the new Input System)
            HandleInput();
        }
        
        private void HandleInput()
        {
            // Mouse drag for camera rotation
            if (Input.GetMouseButton(1)) // Right mouse button
            {
                float horizontalInput = Input.GetAxis("Mouse X");
                float verticalInput = Input.GetAxis("Mouse Y");
                
                RotateCameraWithInput(horizontalInput, verticalInput);
            }
            
            // Mouse wheel for zoom
            float scrollInput = Input.GetAxis("Mouse ScrollWheel");
            if (scrollInput != 0)
            {
                ZoomCamera(scrollInput * zoomSpeed);
            }
        }
        
        private void RotateCamera()
        {
            if (mainCamera == null)
                return;
                
            // Simple auto-rotation around focus point
            mainCamera.transform.RotateAround(focusPoint, Vector3.up, rotationSpeed * Time.deltaTime);
        }
        
        private void RotateCameraWithInput(float horizontal, float vertical)
        {
            if (mainCamera == null)
                return;
                
            // Rotate horizontally around world up vector
            mainCamera.transform.RotateAround(focusPoint, Vector3.up, horizontal * rotationSpeed);
            
            // Rotate vertically around right vector
            mainCamera.transform.RotateAround(focusPoint, mainCamera.transform.right, -vertical * rotationSpeed);
        }
        
        private void ZoomCamera(float zoomAmount)
        {
            if (mainCamera == null)
                return;
                
            // Calculate zoom direction
            Vector3 zoomDirection = (focusPoint - mainCamera.transform.position).normalized;
            
            // Move camera in zoom direction
            Vector3 newPosition = mainCamera.transform.position + zoomDirection * zoomAmount;
            
            // Limit distance to focus point
            float distanceToFocus = Vector3.Distance(newPosition, focusPoint);
            
            if (distanceToFocus > minZoom && distanceToFocus < maxZoom)
            {
                mainCamera.transform.position = newPosition;
            }
        }
        
        public void SetAutoRotate(bool enabled)
        {
            autoRotate = enabled;
            Debug.Log($"Auto-rotate {(enabled ? "enabled" : "disabled")}");
        }
        
        public void SetWireframeMode(bool enabled)
        {
            wireframeMode = enabled;
            
            // Update all materials
            UpdateRenderMode();
            
            Debug.Log($"Wireframe mode {(enabled ? "enabled" : "disabled")}");
        }
          private void UpdateRenderMode()
        {
            // Find all nodes and connections
            BiomorphicSim.Morphology.MorphologyManager morphologyManager = FindFirstObjectByType<BiomorphicSim.Morphology.MorphologyManager>(FindObjectsInactive.Include);
            
            if (morphologyManager == null)
                return;
                
            // In a real implementation, this would update the shader/material on all nodes and connections
            // This is a simplified version
            
            if (wireframeMode)
            {
                // Apply wireframe material to all renderers
            }
            else
            {
                // Apply standard materials to all renderers
            }
        }
        
        public void SetCameraZoom(float zoomValue)
        {
            // Map slider value (0-1) to zoom range
            float zoomAmount = Mathf.Lerp(minZoom, maxZoom, 1f - zoomValue);
            
            if (mainCamera != null)
            {
                // Calculate direction and distance
                Vector3 direction = (mainCamera.transform.position - focusPoint).normalized;
                
                // Set new position
                mainCamera.transform.position = focusPoint + direction * zoomAmount;
            }
        }
        
        public void SetCameraSpeed(float speed)
        {
            rotationSpeed = Mathf.Lerp(1f, 30f, speed);
            Debug.Log($"Camera rotation speed set to {rotationSpeed}");
        }
        
        public void SetVectorScale(float scale)
        {
            vectorFieldScale = Mathf.Lerp(0.1f, 5f, scale);
            
            // Update vector field if it exists
            UpdateVectorField();
            
            Debug.Log($"Vector field scale set to {vectorFieldScale}");
        }
        
        public void ToggleVectorField(bool show)
        {
            showVectorField = show;
            
            if (showVectorField)
            {
                CreateVectorField();
            }
            else
            {
                ClearVectorField();
            }
        }
        
        private void CreateVectorField()
        {
            if (vectorFieldPrefab == null)
                return;
                
            ClearVectorField();
              // Get bounds from site generator
            SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            Vector3 terrainSize = Vector3.one * 100f;
            
            if (siteGenerator != null)
            {
                terrainSize = siteGenerator.GetTerrainSize();
            }
            
            // Calculate number of vectors
            int xCount = Mathf.RoundToInt(terrainSize.x / vectorFieldDensity);
            int zCount = Mathf.RoundToInt(terrainSize.z / vectorFieldDensity);
            
            // Create array of vector instances
            vectorField = new GameObject[xCount * zCount];
            
            // Create vector field
            for (int z = 0; z < zCount; z++)
            {
                for (int x = 0; x < xCount; x++)
                {
                    // Calculate position
                    float xPos = x * vectorFieldDensity;
                    float zPos = z * vectorFieldDensity;
                    
                    Vector3 position = new Vector3(xPos, 0, zPos);
                    
                    // Sample height if site generator exists
                    if (siteGenerator != null)
                    {
                        position.y = Terrain.activeTerrain.SampleHeight(position);
                    }
                    
                    // Create vector instance
                    GameObject vector = Instantiate(vectorFieldPrefab, position, Quaternion.identity, vectorFieldParent);
                    vector.transform.localScale = Vector3.one * vectorFieldScale;
                    
                    // Store in array
                    int index = z * xCount + x;
                    vectorField[index] = vector;
                }
            }
            
            Debug.Log($"Created vector field with {xCount * zCount} vectors");
        }
        
        private void UpdateVectorField()
        {
            if (vectorField == null)
                return;
                
            foreach (GameObject vector in vectorField)
            {
                if (vector != null)
                {
                    vector.transform.localScale = Vector3.one * vectorFieldScale;
                }
            }
        }
        
        private void ClearVectorField()
        {
            if (vectorField != null)
            {
                foreach (GameObject vector in vectorField)
                {
                    if (vector != null)
                    {
                        Destroy(vector);
                    }
                }
                
                vectorField = null;
            }
        }
        
        public void TakeScreenshot()
        {
            StartCoroutine(CaptureScreenshot());
        }
        
        private IEnumerator CaptureScreenshot()
        {
            // Wait for end of frame to ensure all rendering is complete
            yield return new WaitForEndOfFrame();
            
            // Create the texture and read the screen pixels
            Texture2D screenshot = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
            screenshot.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
            screenshot.Apply();
            
            // Convert to PNG
            byte[] bytes = screenshot.EncodeToPNG();
            
            // Define path and filename
            string timestamp = System.DateTime.Now.ToString("yyyyMMdd-HHmmss");
            string filename = $"BiomorphicSim_Screenshot_{timestamp}.png";
            string path = Path.Combine(Application.persistentDataPath, filename);
            
            // Save to file
            File.WriteAllBytes(path, bytes);
            
            Debug.Log($"Screenshot saved: {path}");
            
            // Clean up
            Destroy(screenshot);
        }
        
        public void FocusOnMorphology()
        {            // Find the morphology manager
            BiomorphicSim.Morphology.MorphologyManager morphologyManager = FindFirstObjectByType<BiomorphicSim.Morphology.MorphologyManager>(FindObjectsInactive.Include);
            
            if (morphologyManager == null)
                return;
                
            // Calculate center point of morphology (simplified)
            focusPoint = Vector3.zero; // This would be calculated based on node positions
            
            // Update camera position to look at focus point
            if (mainCamera != null)
            {
                // Maintain distance but change direction
                float currentDistance = Vector3.Distance(mainCamera.transform.position, focusPoint);
                Vector3 newPosition = focusPoint - mainCamera.transform.forward * currentDistance;
                mainCamera.transform.position = newPosition;
                
                // Ensure camera is looking at focus point
                mainCamera.transform.LookAt(focusPoint);
            }
        }
        
        public void FocusOnSite()
        {
            // Focus camera on the center of the terrain/site
            if (mainCamera != null)
            {
                // Calculate the center of the site
                SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
                Vector3 siteCenter = Vector3.zero;
                
                if (siteGenerator != null)
                {
                    Vector3 terrainSize = siteGenerator.GetTerrainSize();
                    siteCenter = new Vector3(terrainSize.x / 2, 0, terrainSize.z / 2);
                }
                
                // Set focus point
                focusPoint = siteCenter;
                
                // Position camera to look at the site
                float distance = 150f; // Adjust as needed
                Vector3 cameraPos = siteCenter + new Vector3(0, 75f, -distance);
                mainCamera.transform.position = cameraPos;
                mainCamera.transform.LookAt(siteCenter);
            }
        }
        
        public void ToggleWindOverlay(bool show)
        {
            // Toggle visualization of wind overlay
            if (show)
            {
                // Create or show wind visualization
                CreateVectorField();
                
                // You can update the vector field to represent wind
                UpdateVectorFieldForWind();
            }
            else
            {
                // Hide wind visualization
                ClearVectorField();
            }
        }
        
        public void ToggleSunExposureOverlay(bool show)
        {
            // Toggle visualization of sun exposure
            SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            if (siteGenerator == null) return;
            
            if (show)
            {
                // Create a heatmap-like visualization for sun exposure
                CreateSunExposureOverlay();
            }
            else
            {
                // Hide sun exposure visualization
                ClearOverlay("SunExposureOverlay");
            }
        }
        
        public void TogglePedestrianOverlay(bool show)
        {
            // Toggle visualization of pedestrian activity
            SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            if (siteGenerator == null) return;
            
            if (show)
            {
                // Create a visualization for pedestrian paths/activity
                CreatePedestrianOverlay();
            }
            else
            {
                // Hide pedestrian visualization
                ClearOverlay("PedestrianOverlay");
            }
        }
        
        // Helper methods for overlays
        private void CreateSunExposureOverlay()
        {
            // Create parent object for overlay
            GameObject overlayParent = new GameObject("SunExposureOverlay");
            overlayParent.transform.SetParent(transform);
            
            // Example: Create a simple colored grid representing sun exposure
            SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            if (siteGenerator == null) return;
            
            Vector3 terrainSize = siteGenerator.GetTerrainSize();
            int resolution = 20; // Grid resolution
            float cellSize = terrainSize.x / resolution;
            
            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    // Calculate position
                    float xPos = x * cellSize;
                    float zPos = z * cellSize;
                    Vector3 position = new Vector3(xPos, 0, zPos);
                    
                    // Sample height if terrain exists
                    if (Terrain.activeTerrain != null)
                    {
                        position.y = Terrain.activeTerrain.SampleHeight(position);
                    }
                    
                    // Calculate sun exposure (example algorithm)
                    // Eastern areas get more morning sun, western areas get more afternoon sun
                    float normalizedX = (float)x / resolution;
                    float normalizedZ = (float)z / resolution;
                    float exposure = Mathf.PerlinNoise(normalizedX * 3f, normalizedZ * 3f);
                    exposure = exposure * 0.5f + 0.5f; // Normalize to 0.5-1.0 range
                    
                    // Add bias for eastern orientation
                    exposure += (1 - normalizedX) * 0.3f;
                    
                    // Create visualization object
                    GameObject cell = GameObject.CreatePrimitive(PrimitiveType.Quad);
                    cell.transform.SetParent(overlayParent.transform);
                    cell.transform.position = position + new Vector3(cellSize/2, 0.2f, cellSize/2); // Slightly above ground
                    cell.transform.localScale = new Vector3(cellSize, cellSize, 1);
                    cell.transform.eulerAngles = new Vector3(90, 0, 0); // Lay flat
                    
                    // Set color based on exposure
                    Renderer renderer = cell.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        // Create a material with orange-yellow color
                        Material mat = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        // Sun exposure color (yellow-orange)
                        mat.color = new Color(1f, Mathf.Lerp(0.5f, 0.8f, exposure), 0f, 0.4f);
                        renderer.material = mat;
                    }
                }
            }
        }
        
        private void CreatePedestrianOverlay()
        {
            // Create parent object for overlay
            GameObject overlayParent = new GameObject("PedestrianOverlay");
            overlayParent.transform.SetParent(transform);
            
            SiteGenerator siteGenerator = FindFirstObjectByType<SiteGenerator>(FindObjectsInactive.Include);
            if (siteGenerator == null) return;
            
            // In a real implementation, we'd use actual pedestrian data
            // For now, create a simplified visualization with higher traffic along main roads
            
            // Find the SiteGenerator to access roads
            List<Transform> roads = new List<Transform>();
            if (siteGenerator.transform.Find("Roads") != null)
            {
                Transform roadsParent = siteGenerator.transform.Find("Roads");
                foreach (Transform road in roadsParent)
                {
                    roads.Add(road);
                }
            }
            
            // If no roads found, create some sample paths
            if (roads.Count == 0)
            {
                // Create example pedestrian paths
                Vector3 terrainSize = siteGenerator.GetTerrainSize();
                
                // Main street path
                CreatePedestrianPath(
                    new Vector3(0, 0, terrainSize.z / 2), 
                    new Vector3(terrainSize.x, 0, terrainSize.z / 2), 
                    Color.blue, 
                    10f, // High traffic
                    overlayParent.transform
                );
                
                // Side street paths
                for (int i = 1; i < 4; i++)
                {
                    float xPos = terrainSize.x * i / 4;
                    CreatePedestrianPath(
                        new Vector3(xPos, 0, 0), 
                        new Vector3(xPos, 0, terrainSize.z), 
                        Color.cyan, 
                        5f, // Medium traffic
                        overlayParent.transform
                    );
                }
            }
            else
            {
                // Create paths based on existing roads
                foreach (Transform road in roads)
                {
                    // Extract road points if available (simplified)
                    Vector3 start = road.position;
                    Vector3 end = road.position + road.forward * 50f;
                    
                    // Determine traffic level based on road type or name
                    float trafficLevel = 5f;
                    if (road.name.Contains("main") || road.name.ToLower().Contains("lambton"))
                    {
                        trafficLevel = 10f; // High traffic for main roads
                    }
                    
                    CreatePedestrianPath(start, end, Color.blue, trafficLevel, overlayParent.transform);
                }
            }
        }
        
        private void CreatePedestrianPath(Vector3 start, Vector3 end, Color color, float intensity, Transform parent)
        {
            // Create a path visualization
            GameObject pathObj = new GameObject("PedestrianPath");
            pathObj.transform.SetParent(parent);
            
            // Create line renderer
            LineRenderer line = pathObj.AddComponent<LineRenderer>();
            line.positionCount = 2;
            
            // Sample terrain height
            if (Terrain.activeTerrain != null)
            {
                start.y = Terrain.activeTerrain.SampleHeight(start) + 0.2f; // Slightly above ground
                end.y = Terrain.activeTerrain.SampleHeight(end) + 0.2f;
            }
            
            line.SetPosition(0, start);
            line.SetPosition(1, end);
            
            // Style the line
            line.startWidth = intensity;
            line.endWidth = intensity;
            line.material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Set color with transparency
            Color pathColor = color;
            pathColor.a = 0.6f;
            line.startColor = pathColor;
            line.endColor = pathColor;
        }
        
        private void UpdateVectorFieldForWind()
        {
            if (vectorField == null)
                return;
                
            // Example: Get wind direction from scenario analyzer
            ScenarioAnalyzer scenarioAnalyzer = FindFirstObjectByType<ScenarioAnalyzer>(FindObjectsInactive.Include);
            Vector3 windDirection = Vector3.right; // Default wind direction (east)
            float windStrength = 1.0f;
            
            if (scenarioAnalyzer != null)
            {
                // In a real implementation, you'd get actual wind data from the analyzer
                // For this example, we'll use simplified values
                windDirection = scenarioAnalyzer.GetWindDirection();
                windStrength = scenarioAnalyzer.GetWindStrength();
            }
            
            // Update vector field visualization to show wind
            foreach (GameObject vector in vectorField)
            {
                if (vector != null)
                {
                    // Set direction
                    vector.transform.forward = windDirection;
                    
                    // Scale based on wind strength and add variation
                    float heightFactor = Mathf.Clamp01(vector.transform.position.y / 50f);
                    float strengthFactor = windStrength * (1f + heightFactor);
                    vector.transform.localScale = Vector3.one * vectorFieldScale * strengthFactor;
                    
                    // Color based on strength
                    Renderer renderer = vector.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        Color windColor = Color.Lerp(Color.blue, Color.cyan, strengthFactor / 2f);
                        renderer.material.color = windColor;
                    }
                }
            }
        }
        
        private void ClearOverlay(string overlayName)
        {
            Transform overlay = transform.Find(overlayName);
            if (overlay != null)
            {
                Destroy(overlay.gameObject);
            }
        }
    }
}