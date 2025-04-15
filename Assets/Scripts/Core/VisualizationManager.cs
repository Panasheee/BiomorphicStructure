using UnityEngine;
using System.Collections;
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
            BiomorphicSim.Morphology.MorphologyManager morphologyManager = FindObjectOfType<BiomorphicSim.Morphology.MorphologyManager>();
            
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
            SiteGenerator siteGenerator = FindObjectOfType<SiteGenerator>();
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
        {
            // Find the morphology manager
            BiomorphicSim.Morphology.MorphologyManager morphologyManager = FindObjectOfType<BiomorphicSim.Morphology.MorphologyManager>();
            
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
    }
}