using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using BiomorphicSim.UI;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Handles user interaction with the site, including zone selection and camera controls.
    /// </summary>
    public class SiteInteractionController : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private VisualizationManager visualizationManager;
        [SerializeField] private ModernUIController uiController;
        
        [Header("Camera Controls")]
        [SerializeField] private float orbitSpeed = 60f;
        [SerializeField] private float panSpeed = 30f;
        [SerializeField] private float zoomSpeed = 50f;
        [SerializeField] private float flightSpeed = 40f;
        [SerializeField] private float minZoomDistance = 10f;
        [SerializeField] private float maxZoomDistance = 200f;
        [SerializeField] private Vector3 focusPoint = Vector3.zero;
        
        [Header("Selection Visualization")]
        [SerializeField] private GameObject zoneSelectionIndicatorPrefab;
        [SerializeField] private float selectionRadius = 5f;
        [SerializeField] private Color validSelectionColor = new Color(0f, 1f, 0.5f, 0.5f);
        [SerializeField] private Color invalidSelectionColor = new Color(1f, 0f, 0f, 0.5f);
        
        // State
        private Vector3 lastMousePosition;
        private bool isOrbiting = false;
        private bool isPanning = false;
        private bool isSelectionModeActive = false;
        private GameObject currentSelectionIndicator;
        private Coroutine selectionCoroutine;
        
        // Constraints for site interaction
        private Bounds siteBounds;
        
        private void Start()
        {
            // Find references if not set
            if (mainCamera == null)
                mainCamera = Camera.main;
              if (visualizationManager == null)
                visualizationManager = FindFirstObjectByType<VisualizationManager>(FindObjectsInactive.Include);
            
            if (uiController == null)
                uiController = FindFirstObjectByType<ModernUIController>(FindObjectsInactive.Include);
            
            // Set up initial camera position
            ResetCameraPosition();
            
            // Start with selection mode inactive
            isSelectionModeActive = false;
        }
        
        private void Update()
        {
            // Handle camera controls
            HandleMouseInput();
            
            // Handle keyboard flight controls
            HandleKeyboardInput();
            
            // Handle site selection
            if (isSelectionModeActive)
                HandleSiteSelection();
        }
        
        #region Camera Controls
        
        private void HandleMouseInput()
        {
            // Ignore mouse input if over UI
            if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                return;
            
            // Middle mouse button or right mouse button for orbit
            if (Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2))
            {
                lastMousePosition = Input.mousePosition;
                isOrbiting = true;
            }
            
            // Release orbit
            if (Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2))
            {
                isOrbiting = false;
            }
            
            // Shift + Middle/Right mouse button for pan
            if ((Input.GetMouseButtonDown(1) || Input.GetMouseButtonDown(2)) && Input.GetKey(KeyCode.LeftShift))
            {
                lastMousePosition = Input.mousePosition;
                isOrbiting = false;
                isPanning = true;
            }
            
            // Release pan
            if ((Input.GetMouseButtonUp(1) || Input.GetMouseButtonUp(2)) && isPanning)
            {
                isPanning = false;
            }
            
            // Perform orbit
            if (isOrbiting)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                PerformOrbit(mouseDelta);
                lastMousePosition = Input.mousePosition;
            }
            
            // Perform pan
            if (isPanning)
            {
                Vector3 mouseDelta = Input.mousePosition - lastMousePosition;
                PerformPan(mouseDelta);
                lastMousePosition = Input.mousePosition;
            }
            
            // Zoom with mouse wheel
            float scrollDelta = Input.GetAxis("Mouse ScrollWheel");
            if (scrollDelta != 0)
            {
                PerformZoom(scrollDelta);
            }
        }
        
        private void PerformOrbit(Vector3 mouseDelta)
        {
            if (mainCamera == null)
                return;
            
            float horizontalRotation = -mouseDelta.x * orbitSpeed * Time.deltaTime;
            float verticalRotation = -mouseDelta.y * orbitSpeed * Time.deltaTime;
            
            // Rotate horizontally around world up vector
            mainCamera.transform.RotateAround(focusPoint, Vector3.up, horizontalRotation);
            
            // Rotate vertically around local right vector
            mainCamera.transform.RotateAround(focusPoint, mainCamera.transform.right, verticalRotation);
            
            // Prevent rotation beyond poles
            Vector3 currentEuler = mainCamera.transform.eulerAngles;
            if (currentEuler.x > 180)
                currentEuler.x -= 360;
            
            currentEuler.x = Mathf.Clamp(currentEuler.x, -80f, 80f);
            mainCamera.transform.eulerAngles = currentEuler;
        }
        
        private void PerformPan(Vector3 mouseDelta)
        {
            if (mainCamera == null)
                return;
            
            // Calculate pan amount
            float distance = Vector3.Distance(mainCamera.transform.position, focusPoint);
            float horizontalPan = -mouseDelta.x * panSpeed * Time.deltaTime * (distance / 100f);
            float verticalPan = -mouseDelta.y * panSpeed * Time.deltaTime * (distance / 100f);
            
            // Apply pan
            Vector3 right = mainCamera.transform.right * horizontalPan;
            Vector3 up = mainCamera.transform.up * verticalPan;
            Vector3 pan = right + up;
            
            // Move both camera and focus point
            mainCamera.transform.position += pan;
            focusPoint += pan;
            
            // Constrain panning to site bounds with margin
            if (siteBounds.size != Vector3.zero)
            {
                float margin = 50f;
                Bounds expandedBounds = new Bounds(siteBounds.center, siteBounds.size + new Vector3(margin, margin, margin));
                focusPoint = new Vector3(
                    Mathf.Clamp(focusPoint.x, expandedBounds.min.x, expandedBounds.max.x),
                    Mathf.Clamp(focusPoint.y, expandedBounds.min.y, expandedBounds.max.y),
                    Mathf.Clamp(focusPoint.z, expandedBounds.min.z, expandedBounds.max.z)
                );
                
                // Adjust camera position to maintain focus point
                Vector3 dirFromFocus = (mainCamera.transform.position - focusPoint).normalized;
                float currentDistance = Vector3.Distance(mainCamera.transform.position, focusPoint);
                mainCamera.transform.position = focusPoint + dirFromFocus * currentDistance;
            }
        }
        
        private void PerformZoom(float scrollDelta)
        {
            if (mainCamera == null)
                return;
            
            // Calculate zoom direction and amount
            Vector3 zoomDirection = (focusPoint - mainCamera.transform.position).normalized;
            float zoomAmount = scrollDelta * zoomSpeed;
            
            // Calculate new distance
            float currentDistance = Vector3.Distance(mainCamera.transform.position, focusPoint);
            float newDistance = Mathf.Clamp(currentDistance - zoomAmount, minZoomDistance, maxZoomDistance);
            
            // Apply zoom
            mainCamera.transform.position = focusPoint - zoomDirection * newDistance;
        }
        
        public void ResetCameraPosition()
        {
            if (mainCamera == null)
                return;
            
            // Set up initial camera position
            mainCamera.transform.position = new Vector3(0, 50, -100);
            mainCamera.transform.LookAt(Vector3.zero);
            focusPoint = Vector3.zero;
        }
        
        public void SetFocusPoint(Vector3 point)
        {
            focusPoint = point;
            
            // Move camera to look at new focus point while maintaining distance
            if (mainCamera != null)
            {
                float currentDistance = Vector3.Distance(mainCamera.transform.position, focusPoint);
                Vector3 direction = (mainCamera.transform.position - focusPoint).normalized;
                mainCamera.transform.position = focusPoint + direction * currentDistance;
                mainCamera.transform.LookAt(focusPoint);
            }
        }
        
        #endregion
        
        #region Site Selection
        
        public void SetSiteBounds(Bounds bounds)
        {
            siteBounds = bounds;
        }
        
        public void EnableSelectionMode(bool enable)
        {
            isSelectionModeActive = enable;
            
            if (enable)
            {
                // Create selection indicator if needed
                if (currentSelectionIndicator == null && zoneSelectionIndicatorPrefab != null)
                {
                    currentSelectionIndicator = Instantiate(zoneSelectionIndicatorPrefab, Vector3.zero, Quaternion.identity);
                    currentSelectionIndicator.transform.localScale = Vector3.one * selectionRadius * 2f;
                    
                    // Initially hide indicator
                    currentSelectionIndicator.SetActive(false);
                }
                
                // Start selection coroutine
                if (selectionCoroutine == null)
                    selectionCoroutine = StartCoroutine(UpdateSelectionIndicator());
            }
            else
            {
                // Hide selection indicator
                if (currentSelectionIndicator != null)
                    currentSelectionIndicator.SetActive(false);
                
                // Stop selection coroutine
                if (selectionCoroutine != null)
                {
                    StopCoroutine(selectionCoroutine);
                    selectionCoroutine = null;
                }
            }
        }
        
        private void HandleSiteSelection()
        {
            // Check for left click for selection
            if (Input.GetMouseButtonDown(0) && !EventSystem.current.IsPointerOverGameObject())
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                
                if (Physics.Raycast(ray, out hit))
                {
                    // Check if hit point is on the terrain
                    if (hit.collider.CompareTag("Terrain") || hit.collider.CompareTag("Site"))
                    {
                        // Check if hit point is within valid bounds
                        if (IsValidSelectionPoint(hit.point))
                        {
                            // Notify UI controller of selection
                            if (uiController != null)
                                uiController.OnSiteClicked(hit.point);
                            
                            // Set focus to selected point
                            SetFocusPoint(hit.point);
                            
                            // Play selection effect
                            PlaySelectionEffect(hit.point);
                            
                            // Disable selection mode
                            EnableSelectionMode(false);
                        }
                        else
                        {
                            Debug.Log("Invalid selection location");
                            
                            // Show temporary invalid selection indicator
                            StartCoroutine(ShowInvalidSelectionIndicator(hit.point));
                        }
                    }
                }
            }
        }
        
        private IEnumerator UpdateSelectionIndicator()
        {
            while (isSelectionModeActive)
            {
                if (currentSelectionIndicator != null)
                {
                    Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                    RaycastHit hit;
                    
                    if (Physics.Raycast(ray, out hit))
                    {
                        // Check if hit point is on the terrain
                        if (hit.collider.CompareTag("Terrain") || hit.collider.CompareTag("Site"))
                        {
                            // Show and position indicator
                            currentSelectionIndicator.SetActive(true);
                            currentSelectionIndicator.transform.position = hit.point;
                            
                            // Update color based on validity
                            bool isValid = IsValidSelectionPoint(hit.point);
                            Renderer renderer = currentSelectionIndicator.GetComponent<Renderer>();
                            if (renderer != null)
                            {
                                renderer.material.color = isValid ? validSelectionColor : invalidSelectionColor;
                            }
                        }
                        else
                        {
                            // Hide indicator if not hovering over terrain
                            currentSelectionIndicator.SetActive(false);
                        }
                    }
                    else
                    {
                        // Hide indicator if no hit
                        currentSelectionIndicator.SetActive(false);
                    }
                }
                
                yield return null;
            }
        }
        
        private bool IsValidSelectionPoint(Vector3 point)
        {
            // Check if point is within site bounds
            if (siteBounds.size != Vector3.zero)
            {
                return siteBounds.Contains(point);
            }
            
            // If no bounds defined, all points are valid
            return true;
        }
        
        private IEnumerator ShowInvalidSelectionIndicator(Vector3 point)
        {
            GameObject indicator = Instantiate(zoneSelectionIndicatorPrefab, point, Quaternion.identity);
            indicator.transform.localScale = Vector3.one * selectionRadius * 2f;
            
            Renderer renderer = indicator.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = invalidSelectionColor;
            }
            
            // Fade out
            float duration = 1.5f;
            float elapsed = 0f;
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float alpha = Mathf.Lerp(0.5f, 0f, elapsed / duration);
                
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = alpha;
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            Destroy(indicator);
        }
        
        private void PlaySelectionEffect(Vector3 point)
        {
            // Create a pulse effect at the selection point
            GameObject effect = Instantiate(zoneSelectionIndicatorPrefab, point, Quaternion.identity);
            effect.transform.localScale = Vector3.one * selectionRadius * 2f;
            
            Renderer renderer = effect.GetComponent<Renderer>();
            if (renderer != null)
            {
                renderer.material.color = validSelectionColor;
            }
            
            // Scale up and fade out
            StartCoroutine(ScalePulseEffect(effect));
        }
        
        private IEnumerator ScalePulseEffect(GameObject effect)
        {
            float duration = 1.0f;
            float elapsed = 0f;
            
            Vector3 initialScale = effect.transform.localScale;
            Vector3 targetScale = initialScale * 2f;
            
            Renderer renderer = effect.GetComponent<Renderer>();
            
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                
                // Scale up
                effect.transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                
                // Fade out
                if (renderer != null)
                {
                    Color color = renderer.material.color;
                    color.a = Mathf.Lerp(0.5f, 0f, t);
                    renderer.material.color = color;
                }
                
                yield return null;
            }
            
            Destroy(effect);
        }
        
        #endregion

        private void HandleKeyboardInput()
        {
            if (mainCamera == null)
                return;
                
            // Skip keyboard controls if over UI elements
            if (EventSystem.current != null && EventSystem.current.currentSelectedGameObject != null)
                return;
                
            float moveSpeed = flightSpeed * Time.deltaTime;
            
            // Create movement vector based on WASD keys
            Vector3 movement = Vector3.zero;
            
            // Forward/Backward movement (W/S)
            if (Input.GetKey(KeyCode.W))
                movement += mainCamera.transform.forward * moveSpeed;
            if (Input.GetKey(KeyCode.S))
                movement -= mainCamera.transform.forward * moveSpeed;
                
            // Strafe movement (A/D)
            if (Input.GetKey(KeyCode.A))
                movement -= mainCamera.transform.right * moveSpeed;
            if (Input.GetKey(KeyCode.D))
                movement += mainCamera.transform.right * moveSpeed;
                
            // Vertical movement (Q/E keys)
            if (Input.GetKey(KeyCode.Q))
                movement -= Vector3.up * moveSpeed;
            if (Input.GetKey(KeyCode.E))
                movement += Vector3.up * moveSpeed;
                
            // Apply movement to camera
            if (movement.magnitude > 0)
            {
                mainCamera.transform.position += movement;
                
                // Update focus point when flying
                focusPoint = mainCamera.transform.position + mainCamera.transform.forward * 10f;
                
                // Constrain to site bounds with large margin for flight
                if (siteBounds.size != Vector3.zero)
                {
                    float margin = 200f;
                    Bounds expandedBounds = new Bounds(siteBounds.center, siteBounds.size + new Vector3(margin, margin, margin));
                    
                    mainCamera.transform.position = new Vector3(
                        Mathf.Clamp(mainCamera.transform.position.x, expandedBounds.min.x, expandedBounds.max.x),
                        Mathf.Clamp(mainCamera.transform.position.y, expandedBounds.min.y, expandedBounds.max.y),
                        Mathf.Clamp(mainCamera.transform.position.z, expandedBounds.min.z, expandedBounds.max.z)
                    );
                }
            }
        }
    }
}
