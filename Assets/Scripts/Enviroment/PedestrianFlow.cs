using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
using System.Linq;

/// <summary>
/// Simulates pedestrian movement patterns in Wellington Lambton Quay.
/// Creates realistic pedestrian density and flow direction based on time and location.
/// </summary>
public class PedestrianFlow : MonoBehaviour
{
    #region Properties
    [Header("Pedestrian Data")]
    [SerializeField] private bool useRealData = true;
    [SerializeField] private TextAsset pedestrianCountsFile;
    [SerializeField] private PedestrianProfile[] hourlyProfiles;
    
    [Header("Flow Paths")]
    [SerializeField] private List<PedestrianFlowPath> flowPaths = new List<PedestrianFlowPath>();
    [SerializeField] private bool showFlowPaths = true;
    [SerializeField] private Color pathColor = new Color(0, 0.5f, 1, 0.5f);
    
    [Header("Visualization")]
    [SerializeField] private GameObject pedestrianPrefab;
    [SerializeField] private int maxPedestrianCount = 200;
    [SerializeField] private bool showVisualRepresentation = true;
    [SerializeField] private float pedestrianSpeedBase = 1.5f; // meters per second
    [SerializeField] private float pedestrianSpeedVariation = 0.3f;
    
    [Header("Density Map")]
    [SerializeField] private Vector2 mapSize = new Vector2(500, 500);
    [SerializeField] private float cellSize = 5f;
    [SerializeField] private bool showDensityMap = false;
    [SerializeField] private Color lowDensityColor = Color.blue;
    [SerializeField] private Color highDensityColor = Color.red;
    
    // Current state
    private PedestrianState currentState = new PedestrianState();
    private DateTime currentDateTime;
    private float timeScale = 1.0f;
    private bool isSimulationRunning = false;
    
    // For tracking time
    private float simulationTimer = 0f;
    private TimeSpan simulationTimeStep = TimeSpan.FromMinutes(15); // Update every 15 sim-minutes
    
    // Density map data
    private float[,] densityMap;
    private GameObject densityMapVisual;
    
    // Pedestrian instances
    private List<GameObject> activePedestrians = new List<GameObject>();
    private List<float[,]> densityMapsForHours = new List<float[,]>(); // Cached density maps by hour
    
    // Pedestrian data
    private Dictionary<DateTime, PedestrianTimePoint> pedestrianData = new Dictionary<DateTime, PedestrianTimePoint>();
    #endregion

    #region Unity Methods
    private void Awake()
    {
        // Initialize density map
        int xCells = Mathf.CeilToInt(mapSize.x / cellSize);
        int yCells = Mathf.CeilToInt(mapSize.y / cellSize);
        densityMap = new float[xCells, yCells];
        
        // Set current date/time to now
        currentDateTime = DateTime.Now;
        
        // Initialize hourly profiles if needed
        if (hourlyProfiles == null || hourlyProfiles.Length == 0)
        {
            InitializeDefaultProfiles();
        }
    }
    
    private void Start()
    {
        // Create density map visual
        if (showDensityMap)
        {
            CreateDensityMapVisual();
        }
    }
    
    private void Update()
    {
        if (!isSimulationRunning) return;
        
        // Advance simulation time
        simulationTimer += Time.deltaTime * timeScale;
        
        // Update flow every timeStep
        if (simulationTimer >= simulationTimeStep.TotalSeconds)
        {
            simulationTimer = 0f;
            currentDateTime = currentDateTime.Add(simulationTimeStep);
            UpdatePedestrianFlow();
        }
        
        // Update visualization every frame
        if (showVisualRepresentation)
        {
            UpdatePedestrianVisuals();
        }
    }
    
    private void OnDrawGizmos()
    {
        if (!showFlowPaths) return;
        
        // Draw flow paths
        Gizmos.color = pathColor;
        
        foreach (var path in flowPaths)
        {
            if (path.pathPoints.Count < 2) continue;
            
            for (int i = 0; i < path.pathPoints.Count - 1; i++)
            {
                Gizmos.DrawLine(path.pathPoints[i], path.pathPoints[i + 1]);
                
                // Draw direction arrows
                Vector3 direction = path.pathPoints[i + 1] - path.pathPoints[i];
                float distance = direction.magnitude;
                direction.Normalize();
                
                if (distance > 5f)
                {
                    Vector3 arrowPos = path.pathPoints[i] + direction * (distance * 0.5f);
                    Vector3 right = Vector3.Cross(Vector3.up, direction).normalized * 1f;
                    
                    Gizmos.DrawLine(arrowPos, arrowPos - direction * 2f + right);
                    Gizmos.DrawLine(arrowPos, arrowPos - direction * 2f - right);
                }
            }
            
            // Draw flow points
            for (int i = 0; i < path.pathPoints.Count; i++)
            {
                Gizmos.DrawSphere(path.pathPoints[i], 0.5f);
            }
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the pedestrian flow system
    /// </summary>
    public void Initialize()
    {
        // Initialize density map
        UpdateDensityMap();
        
        // Cache density maps for each hour for faster simulation
        CacheDensityMaps();
        
        // If we have a prefab, create initial pedestrians
        if (pedestrianPrefab != null && showVisualRepresentation)
        {
            CreatePedestrianPool();
        }
        
        Debug.Log("Pedestrian flow system initialized");
    }
    
    /// <summary>
    /// Sets the date and time for the simulation
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    public void SetDateTime(DateTime dateTime)
    {
        currentDateTime = dateTime;
        UpdatePedestrianFlow();
    }
    
    /// <summary>
    /// Sets the time scale for the simulation
    /// </summary>
    /// <param name="scale">Time scale (1.0 = real-time)</param>
    public void SetTimeScale(float scale)
    {
        timeScale = Mathf.Max(0.1f, scale);
    }
    
    /// <summary>
    /// Starts the pedestrian flow simulation
    /// </summary>
    public void StartSimulation()
    {
        isSimulationRunning = true;
        simulationTimer = 0f;
        UpdatePedestrianFlow();
    }
    
    /// <summary>
    /// Stops the pedestrian flow simulation
    /// </summary>
    public void StopSimulation()
    {
        isSimulationRunning = false;
    }
    
    /// <summary>
    /// Gets the current pedestrian flow state
    /// </summary>
    /// <returns>Current pedestrian state</returns>
    public PedestrianState GetCurrentPedestrianFlow()
    {
        return currentState;
    }
    
    /// <summary>
    /// Sets manual pedestrian flow settings (for non-realistic mode)
    /// </summary>
    /// <param name="state">Pedestrian state to set</param>
    public void SetManualPedestrianFlow(PedestrianState state)
    {
        currentState = state;
        
        // Update visualization
        if (showVisualRepresentation)
        {
            UpdatePedestrianVisuals();
        }
    }
    
    /// <summary>
    /// Gets the pedestrian density at a specific position
    /// </summary>
    /// <param name="position">World position</param>
    /// <returns>Density value (0-1)</returns>
    public float GetDensityAt(Vector3 position)
    {
        // Convert world position to density map coordinates
        int x = Mathf.FloorToInt((position.x + mapSize.x / 2) / cellSize);
        int y = Mathf.FloorToInt((position.z + mapSize.y / 2) / cellSize);
        
        // Check bounds
        if (x < 0 || x >= densityMap.GetLength(0) || y < 0 || y >= densityMap.GetLength(1))
        {
            return 0f;
        }
        
        return densityMap[x, y];
    }
    
    /// <summary>
    /// Gets the dominant flow direction at a specific position
    /// </summary>
    /// <param name="position">World position</param>
    /// <returns>Flow direction vector</returns>
    public Vector3 GetFlowDirectionAt(Vector3 position)
    {
        // Find closest path point
        PedestrianFlowPath closestPath = null;
        int closestPointIndex = -1;
        float closestDistance = float.MaxValue;
        
        foreach (var path in flowPaths)
        {
            for (int i = 0; i < path.pathPoints.Count; i++)
            {
                float distance = Vector3.Distance(position, path.pathPoints[i]);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestPath = path;
                    closestPointIndex = i;
                }
            }
        }
        
        // If no path found or too far, return zero vector
        if (closestPath == null || closestDistance > 20f)
        {
            return Vector3.zero;
        }
        
        // Get direction along path
        if (closestPointIndex < closestPath.pathPoints.Count - 1)
        {
            // Direction to next point
            return (closestPath.pathPoints[closestPointIndex + 1] - closestPath.pathPoints[closestPointIndex]).normalized;
        }
        else if (closestPointIndex > 0)
        {
            // Direction from previous point
            return (closestPath.pathPoints[closestPointIndex] - closestPath.pathPoints[closestPointIndex - 1]).normalized;
        }
        
        // Fallback
        return Vector3.forward;
    }
    
    /// <summary>
    /// Loads pedestrian data from a file
    /// </summary>
    /// <param name="dataFile">Text asset containing pedestrian data</param>
    public void LoadPedestrianData(TextAsset dataFile)
    {
        if (dataFile == null)
        {
            Debug.LogError("No pedestrian data file provided");
            return;
        }
        
        try
        {
            string[] lines = dataFile.text.Split('\n');
            
            // Skip header line if present
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i].Trim();
                if (string.IsNullOrEmpty(line)) continue;
                
                string[] values = line.Split(',');
                if (values.Length < 4) continue; // Need at least date, time, count, flow direction
                
                try
                {
                    // Parse date and time
                    string dateStr = values[0].Trim();
                    string timeStr = values[1].Trim();
                    DateTime dateTime = DateTime.Parse($"{dateStr} {timeStr}");
                    
                    // Parse count and direction
                    int count = int.Parse(values[2].Trim());
                    float direction = float.Parse(values[3].Trim());
                    
                    // Parse optional location
                    Vector3 location = Vector3.zero;
                    if (values.Length >= 7)
                    {
                        float x = float.Parse(values[4].Trim());
                        float y = float.Parse(values[5].Trim());
                        float z = float.Parse(values[6].Trim());
                        location = new Vector3(x, y, z);
                    }
                    
                    PedestrianTimePoint point = new PedestrianTimePoint
                    {
                        dateTime = dateTime,
                        count = count,
                        flowDirection = Quaternion.Euler(0, direction, 0) * Vector3.forward,
                        location = location
                    };
                    
                    // Add to data dictionary
                    DateTime key = new DateTime(
                        dateTime.Year,
                        dateTime.Month,
                        dateTime.Day,
                        dateTime.Hour,
                        dateTime.Minute / 15 * 15, // Round to 15-minute increments
                        0
                    );
                    
                    pedestrianData[key] = point;
                }
                catch (Exception e)
                {
                    Debug.LogWarning($"Error parsing pedestrian data line {i}: {e.Message}");
                }
            }
            
            Debug.Log($"Loaded {pedestrianData.Count} pedestrian data points");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading pedestrian data: {e.Message}");
        }
    }
    
    /// <summary>
    /// Adds a pedestrian flow path
    /// </summary>
    /// <param name="path">Flow path to add</param>
    public void AddFlowPath(PedestrianFlowPath path)
    {
        flowPaths.Add(path);
        
        // Update density map
        UpdateDensityMap();
    }
    
    /// <summary>
    /// Creates a flow path between two points
    /// </summary>
    /// <param name="start">Start position</param>
    /// <param name="end">End position</param>
    /// <param name="weight">Path weight/importance</param>
    /// <returns>Created flow path</returns>
    public PedestrianFlowPath CreateFlowPath(Vector3 start, Vector3 end, float weight = 1.0f)
    {
        PedestrianFlowPath path = new PedestrianFlowPath
        {
            pathPoints = new List<Vector3> { start, end },
            pathWeight = weight
        };
        
        flowPaths.Add(path);
        
        // Update density map
        UpdateDensityMap();
        
        return path;
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Initializes default hourly profiles
    /// </summary>
    private void InitializeDefaultProfiles()
    {
        // Typical pedestrian flow patterns for a business district like Lambton Quay
        hourlyProfiles = new PedestrianProfile[24];
        
        // Early morning (0-6)
        for (int i = 0; i < 6; i++)
        {
            hourlyProfiles[i] = new PedestrianProfile
            {
                pedestrianDensity = 0.05f,
                flowIntensity = 0.2f,
                primaryDirection = 0f // North
            };
        }
        
        // Morning commute (6-9)
        hourlyProfiles[6] = new PedestrianProfile { pedestrianDensity = 0.3f, flowIntensity = 0.7f, primaryDirection = 0f };
        hourlyProfiles[7] = new PedestrianProfile { pedestrianDensity = 0.6f, flowIntensity = 0.9f, primaryDirection = 0f };
        hourlyProfiles[8] = new PedestrianProfile { pedestrianDensity = 0.9f, flowIntensity = 0.9f, primaryDirection = 0f };
        
        // Business hours (9-17)
        for (int i = 9; i < 17; i++)
        {
            hourlyProfiles[i] = new PedestrianProfile
            {
                pedestrianDensity = 0.7f,
                flowIntensity = 0.5f,
                primaryDirection = i % 2 == 0 ? 0f : 180f // Alternating flow
            };
        }
        
        // Lunch peak (12-13)
        hourlyProfiles[12] = new PedestrianProfile { pedestrianDensity = 0.8f, flowIntensity = 0.6f, primaryDirection = 90f };
        
        // Evening commute (17-20)
        hourlyProfiles[17] = new PedestrianProfile { pedestrianDensity = 0.9f, flowIntensity = 0.9f, primaryDirection = 180f };
        hourlyProfiles[18] = new PedestrianProfile { pedestrianDensity = 0.7f, flowIntensity = 0.8f, primaryDirection = 180f };
        hourlyProfiles[19] = new PedestrianProfile { pedestrianDensity = 0.4f, flowIntensity = 0.7f, primaryDirection = 180f };
        
        // Evening/night (20-24)
        hourlyProfiles[20] = new PedestrianProfile { pedestrianDensity = 0.3f, flowIntensity = 0.4f, primaryDirection = 180f };
        hourlyProfiles[21] = new PedestrianProfile { pedestrianDensity = 0.2f, flowIntensity = 0.3f, primaryDirection = 180f };
        hourlyProfiles[22] = new PedestrianProfile { pedestrianDensity = 0.1f, flowIntensity = 0.3f, primaryDirection = 180f };
        hourlyProfiles[23] = new PedestrianProfile { pedestrianDensity = 0.05f, flowIntensity = 0.2f, primaryDirection = 180f };
    }
    
    /// <summary>
    /// Updates the pedestrian flow based on current time
    /// </summary>
    private void UpdatePedestrianFlow()
    {
        PedestrianState newState = new PedestrianState();
        
        // Try to get data from real data if available
        if (useRealData && TryGetRealPedestrianData(currentDateTime, out PedestrianTimePoint point))
        {
            newState.density = Mathf.Clamp01(point.count / 1000f); // Normalize to 0-1 range
            newState.flowDirection = point.flowDirection;
            newState.peakLocations = new Vector3[] { point.location };
        }
        else
        {
            // Fall back to simulated data
            int hour = currentDateTime.Hour;
            PedestrianProfile profile = hourlyProfiles[hour];
            
            // Add some time-based variation
            float dayFactor = (float)currentDateTime.DayOfWeek / 6f; // 0 = Sunday, 1 = Saturday
            
            // Weekday vs weekend adjustment
            bool isWeekend = dayFactor >= 5f/6f || dayFactor == 0;
            float weekendFactor = isWeekend ? 0.6f : 1.0f;
            
            // Density varies by weekday/weekend
            newState.density = profile.pedestrianDensity * weekendFactor;
            
            // Add minute-based variation
            float minuteFactor = currentDateTime.Minute / 60f;
            float variation = Mathf.Sin(minuteFactor * Mathf.PI * 2f) * 0.1f;
            newState.density = Mathf.Clamp01(newState.density + variation);
            
            // Direction with slight variation
            float directionVariation = UnityEngine.Random.Range(-15f, 15f);
            float direction = profile.primaryDirection + directionVariation;
            newState.flowDirection = Quaternion.Euler(0, direction, 0) * Vector3.forward;
            
            // Peak locations - use path points with highest weights
            newState.peakLocations = GetPeakLocations(3);
        }
        
        // Update current state
        currentState = newState;
        
        // Update density map
        int hourIndex = currentDateTime.Hour;
        if (densityMapsForHours.Count > hourIndex)
        {
            // Use cached map
            densityMap = densityMapsForHours[hourIndex];
        }
        else
        {
            // Generate new map
            UpdateDensityMap();
        }
        
        // Update density map visual
        if (showDensityMap && densityMapVisual != null)
        {
            UpdateDensityMapVisual();
        }
    }
    
    /// <summary>
    /// Tries to get real pedestrian data for a given time
    /// </summary>
    /// <param name="dateTime">Date and time</param>
    /// <param name="point">Output pedestrian data point</param>
    /// <returns>True if data was found</returns>
    private bool TryGetRealPedestrianData(DateTime dateTime, out PedestrianTimePoint point)
    {
        // Round to the nearest 15 minutes for lookup
        DateTime key = new DateTime(
            dateTime.Year,
            dateTime.Month,
            dateTime.Day,
            dateTime.Hour,
            dateTime.Minute / 15 * 15,
            0
        );
        
        // Try exact match
        if (pedestrianData.TryGetValue(key, out point))
        {
            return true;
        }
        
        // Try to find data for same day of week and time
        DateTime weekdayTimeKey = new DateTime(
            2023, // Use a common year
            1, // January
            1 + (int)dateTime.DayOfWeek, // 1-7 based on day of week
            dateTime.Hour,
            dateTime.Minute / 15 * 15,
            0
        );
        
        foreach (var entry in pedestrianData)
        {
            if (entry.Key.Hour == weekdayTimeKey.Hour && 
                entry.Key.Minute == weekdayTimeKey.Minute && 
                entry.Key.DayOfWeek == weekdayTimeKey.DayOfWeek)
            {
                point = entry.Value;
                return true;
            }
        }
        
        // Try to find closest time match
        var sameHourEntries = pedestrianData.Where(e => e.Key.Hour == dateTime.Hour).ToList();
        if (sameHourEntries.Count > 0)
        {
            point = sameHourEntries[0].Value;
            return true;
        }
        
        // No suitable data found
        point = default;
        return false;
    }
    
    /// <summary>
    /// Updates the density map based on current time and flow paths
    /// </summary>
    private void UpdateDensityMap()
    {
        // Reset density map
        int xCells = densityMap.GetLength(0);
        int yCells = densityMap.GetLength(1);
        
        for (int x = 0; x < xCells; x++)
        {
            for (int y = 0; y < yCells; y++)
            {
                densityMap[x, y] = 0f;
            }
        }
        
        // Get appropriate profile for current time
        int hour = currentDateTime.Hour;
        PedestrianProfile profile = hourlyProfiles[hour];
        
        // Generate density along paths
        foreach (var path in flowPaths)
        {
            if (path.pathPoints.Count < 2) continue;
            
            // Path density factor
            float pathFactor = path.pathWeight * profile.pedestrianDensity;
            
            // Add density along path with falloff
            for (int i = 0; i < path.pathPoints.Count - 1; i++)
            {
                Vector3 start = path.pathPoints[i];
                Vector3 end = path.pathPoints[i + 1];
                float distance = Vector3.Distance(start, end);
                
                // Skip if too close
                if (distance < 1f) continue;
                
                // Sample points along the path
                int samples = Mathf.CeilToInt(distance / cellSize);
                for (int s = 0; s <= samples; s++)
                {
                    float t = (float)s / samples;
                    Vector3 pos = Vector3.Lerp(start, end, t);
                    
                    // Add density with falloff from path center
                    AddDensityAt(pos, pathFactor, 10f);
                }
            }
            
            // Add extra density at path nodes
            foreach (var point in path.pathPoints)
            {
                AddDensityAt(point, pathFactor * 1.5f, 15f);
            }
        }
        
        // Normalize density map
        NormalizeDensityMap();
    }
    
    /// <summary>
    /// Adds density at a position with radial falloff
    /// </summary>
    /// <param name="position">World position</param>
    /// <param name="amount">Density amount to add</param>
    /// <param name="radius">Radius of influence</param>
    private void AddDensityAt(Vector3 position, float amount, float radius)
    {
        // Convert world position to density map coordinates
        int centerX = Mathf.FloorToInt((position.x + mapSize.x / 2) / cellSize);
        int centerY = Mathf.FloorToInt((position.z + mapSize.y / 2) / cellSize);
        
        // Determine affected cells (square for simplicity)
        int radiusCells = Mathf.CeilToInt(radius / cellSize);
        int minX = Mathf.Max(0, centerX - radiusCells);
        int maxX = Mathf.Min(densityMap.GetLength(0) - 1, centerX + radiusCells);
        int minY = Mathf.Max(0, centerY - radiusCells);
        int maxY = Mathf.Min(densityMap.GetLength(1) - 1, centerY + radiusCells);
        
        // Add density with falloff
        for (int x = minX; x <= maxX; x++)
        {
            for (int y = minY; y <= maxY; y++)
            {
                // Calculate world position of cell center
                float worldX = (x * cellSize) - mapSize.x / 2 + cellSize / 2;
                float worldZ = (y * cellSize) - mapSize.y / 2 + cellSize / 2;
                Vector3 cellPos = new Vector3(worldX, 0, worldZ);
                
                // Calculate distance and falloff
                float distance = Vector3.Distance(new Vector3(position.x, 0, position.z), cellPos);
                if (distance > radius) continue;
                
                // Quadratic falloff
                float falloff = 1f - (distance / radius) * (distance / radius);
                densityMap[x, y] += amount * falloff;
            }
        }
    }
    
    /// <summary>
    /// Normalizes the density map to 0-1 range
    /// </summary>
    private void NormalizeDensityMap()
    {
        float maxDensity = 0f;
        
        // Find maximum density
        int xCells = densityMap.GetLength(0);
        int yCells = densityMap.GetLength(1);
        
        for (int x = 0; x < xCells; x++)
        {
            for (int y = 0; y < yCells; y++)
            {
                maxDensity = Mathf.Max(maxDensity, densityMap[x, y]);
            }
        }
        
        // Avoid division by zero
        if (maxDensity < 0.001f) return;
        
        // Normalize
        for (int x = 0; x < xCells; x++)
        {
            for (int y = 0; y < yCells; y++)
            {
                densityMap[x, y] /= maxDensity;
            }
        }
    }
    
    /// <summary>
    /// Creates a visual representation of the density map
    /// </summary>
    private void CreateDensityMapVisual()
    {
        // Create or get visual object
        if (densityMapVisual == null)
        {
            densityMapVisual = new GameObject("DensityMapVisual");
            densityMapVisual.transform.parent = transform;
            densityMapVisual.transform.localPosition = Vector3.zero;
        }
        
        // Create plane mesh
        int xCells = densityMap.GetLength(0);
        int yCells = densityMap.GetLength(1);
        
        Mesh mesh = new Mesh();
        
        // Create vertices (one per cell)
        Vector3[] vertices = new Vector3[xCells * yCells];
        Color[] colors = new Color[xCells * yCells];
        
        for (int x = 0; x < xCells; x++)
        {
            for (int y = 0; y < yCells; y++)
            {
                int index = y * xCells + x;
                
                // Calculate world position of cell center
                float worldX = (x * cellSize) - mapSize.x / 2 + cellSize / 2;
                float worldZ = (y * cellSize) - mapSize.y / 2 + cellSize / 2;
                
                vertices[index] = new Vector3(worldX, 0.1f, worldZ);
                
                // Color based on density
                float density = densityMap[x, y];
                colors[index] = Color.Lerp(lowDensityColor, highDensityColor, density);
            }
        }
        
        // Create triangles (two per cell)
        int[] triangles = new int[(xCells - 1) * (yCells - 1) * 6];
        int triangleIndex = 0;
        
        for (int x = 0; x < xCells - 1; x++)
        {
            for (int y = 0; y < yCells - 1; y++)
            {
                int vertexIndex = y * xCells + x;
                
                // First triangle
                triangles[triangleIndex++] = vertexIndex;
                triangles[triangleIndex++] = vertexIndex + xCells;
                triangles[triangleIndex++] = vertexIndex + 1;
                
                // Second triangle
                triangles[triangleIndex++] = vertexIndex + 1;
                triangles[triangleIndex++] = vertexIndex + xCells;
                triangles[triangleIndex++] = vertexIndex + xCells + 1;
            }
        }
        
        // Set mesh data
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.colors = colors;
        
        // Create or update mesh filter
        MeshFilter meshFilter = densityMapVisual.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = densityMapVisual.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = mesh;
        
        // Create or update mesh renderer
        MeshRenderer meshRenderer = densityMapVisual.GetComponent<MeshRenderer>();
        if (meshRenderer == null)
        {
            meshRenderer = densityMapVisual.AddComponent<MeshRenderer>();
        }
        
        // Create material if needed
        if (meshRenderer.material == null)
        {
            meshRenderer.material = new Material(Shader.Find("Standard"));
            meshRenderer.material.SetFloat("_Glossiness", 0f);
            meshRenderer.material.SetFloat("_Metallic", 0f);
        }
    }
    
    /// <summary>
    /// Updates the visual representation of the density map
    /// </summary>
    private void UpdateDensityMapVisual()
    {
        if (densityMapVisual == null) return;
        
        MeshFilter meshFilter = densityMapVisual.GetComponent<MeshFilter>();
        if (meshFilter == null || meshFilter.mesh == null) return;
        
        Mesh mesh = meshFilter.mesh;
        
        // Update colors based on density
        int xCells = densityMap.GetLength(0);
        int yCells = densityMap.GetLength(1);
        
        Color[] colors = new Color[xCells * yCells];
        
        for (int x = 0; x < xCells; x++)
        {
            for (int y = 0; y < yCells; y++)
            {
                int index = y * xCells + x;
                
                // Color based on density
                float density = densityMap[x, y];
                colors[index] = Color.Lerp(lowDensityColor, highDensityColor, density);
            }
        }
        
        mesh.colors = colors;
    }
    
    /// <summary>
    /// Creates and initializes pedestrian instances
    /// </summary>
    private void CreatePedestrianPool()
    {
        // Clear any existing pedestrians
        foreach (var ped in activePedestrians)
        {
            if (ped != null)
            {
                Destroy(ped);
            }
        }
        
        activePedestrians.Clear();
        
        // Create initial pool of pedestrians (inactive)
        for (int i = 0; i < maxPedestrianCount; i++)
        {
            GameObject ped = Instantiate(pedestrianPrefab, transform);
            ped.SetActive(false);
            activePedestrians.Add(ped);
        }
    }
    
    /// <summary>
    /// Updates the pedestrian visual representation
    /// </summary>
    private void UpdatePedestrianVisuals()
    {
        if (pedestrianPrefab == null || activePedestrians.Count == 0) return;
        
        // Calculate how many pedestrians to show based on density
        int pedestriansToShow = Mathf.RoundToInt(currentState.density * maxPedestrianCount);
        
        // Activate/deactivate pedestrians
        for (int i = 0; i < activePedestrians.Count; i++)
        {
            if (i < pedestriansToShow)
            {
                // Activate this pedestrian
                GameObject ped = activePedestrians[i];
                
                if (!ped.activeSelf)
                {
                    // Position at a random location with higher density
                    ped.transform.position = GetRandomPositionByDensity();
                    ped.SetActive(true);
                    
                    // Give random speed
                    PedestrianController controller = ped.GetComponent<PedestrianController>();
                    if (controller != null)
                    {
                        controller.speed = pedestrianSpeedBase + UnityEngine.Random.Range(-pedestrianSpeedVariation, pedestrianSpeedVariation);
                    }
                }
                
                // Update movement
                UpdatePedestrianMovement(ped);
            }
            else
            {
                // Deactivate this pedestrian
                activePedestrians[i].SetActive(false);
            }
        }
    }
    
    /// <summary>
    /// Updates movement for a pedestrian
    /// </summary>
    /// <param name="pedestrian">Pedestrian to update</param>
    private void UpdatePedestrianMovement(GameObject pedestrian)
    {
        if (pedestrian == null) return;
        
        PedestrianController controller = pedestrian.GetComponent<PedestrianController>();
        if (controller == null)
        {
            controller = pedestrian.AddComponent<PedestrianController>();
            controller.speed = pedestrianSpeedBase;
        }
        
        // Check if pedestrian needs a new target
        if (!controller.hasTarget || Vector3.Distance(pedestrian.transform.position, controller.targetPosition) < 1f)
        {
            // Get flow direction at current position
            Vector3 flowDirection = GetFlowDirectionAt(pedestrian.transform.position);
            
            // If no strong flow, use global flow direction
            if (flowDirection.magnitude < 0.1f)
            {
                flowDirection = currentState.flowDirection;
            }
            
            // Add some randomness
            flowDirection = Quaternion.Euler(0, UnityEngine.Random.Range(-30f, 30f), 0) * flowDirection;
            
            // Set distance based on density (more dense = shorter paths)
            float distance = Mathf.Lerp(10f, 30f, 1f - currentState.density);
            
            // Calculate target position
            Vector3 targetPosition = pedestrian.transform.position + flowDirection * distance;
            
            // Check if target is within bounds
            if (targetPosition.x < -mapSize.x / 2 || targetPosition.x > mapSize.x / 2 ||
                targetPosition.z < -mapSize.y / 2 || targetPosition.z > mapSize.y / 2)
            {
                // Out of bounds, reposition to other side (wraparound)
                Vector3 newPosition = pedestrian.transform.position;
                
                if (targetPosition.x < -mapSize.x / 2) newPosition.x = mapSize.x / 2 - 5f;
                else if (targetPosition.x > mapSize.x / 2) newPosition.x = -mapSize.x / 2 + 5f;
                
                if (targetPosition.z < -mapSize.y / 2) newPosition.z = mapSize.y / 2 - 5f;
                else if (targetPosition.z > mapSize.y / 2) newPosition.z = -mapSize.y / 2 + 5f;
                
                // Get a position with appropriate density
                Vector3 densityPosition = GetRandomPositionByDensity();
                newPosition.y = 0;
                
                // Blend between edge and density-based position
                pedestrian.transform.position = Vector3.Lerp(newPosition, densityPosition, 0.5f);
                
                // Calculate new target
                flowDirection = GetFlowDirectionAt(pedestrian.transform.position);
                targetPosition = pedestrian.transform.position + flowDirection * distance;
            }
            
            // Set new target
            controller.SetTarget(targetPosition);
        }
        
        // Update controller
        controller.UpdateMovement();
    }
    
    /// <summary>
    /// Gets a random position weighted by density
    /// </summary>
    /// <returns>A position with higher probability in denser areas</returns>
    private Vector3 GetRandomPositionByDensity()
    {
        // Try weighted random selection
        int maxAttempts = 20;
        
        for (int attempt = 0; attempt < maxAttempts; attempt++)
        {
            // Pick random position
            float x = UnityEngine.Random.Range(-mapSize.x / 2, mapSize.x / 2);
            float z = UnityEngine.Random.Range(-mapSize.y / 2, mapSize.y / 2);
            Vector3 position = new Vector3(x, 0, z);
            
            // Get density at this position
            float density = GetDensityAt(position);
            
            // Accept with probability proportional to density
            if (UnityEngine.Random.value < density)
            {
                return position;
            }
        }
        
        // Fallback - use a peak location
        Vector3[] peaks = GetPeakLocations(1);
        if (peaks.Length > 0)
        {
            return peaks[0] + new Vector3(
                UnityEngine.Random.Range(-5f, 5f),
                0,
                UnityEngine.Random.Range(-5f, 5f)
            );
        }
        
        // Final fallback - totally random
        return new Vector3(
            UnityEngine.Random.Range(-mapSize.x / 2, mapSize.x / 2),
            0,
            UnityEngine.Random.Range(-mapSize.y / 2, mapSize.y / 2)
        );
    }
    
    /// <summary>
    /// Gets the positions with highest pedestrian density
    /// </summary>
    /// <param name="count">Number of peak locations to get</param>
    /// <returns>Array of peak locations</returns>
    private Vector3[] GetPeakLocations(int count)
    {
        // Simple implementation - use path points with highest weights
        List<KeyValuePair<Vector3, float>> pointWeights = new List<KeyValuePair<Vector3, float>>();
        
        foreach (var path in flowPaths)
        {
            foreach (var point in path.pathPoints)
            {
                float density = GetDensityAt(point);
                pointWeights.Add(new KeyValuePair<Vector3, float>(point, density));
            }
        }
        
        // Sort by density (descending)
        pointWeights.Sort((a, b) => b.Value.CompareTo(a.Value));
        
        // Get top points
        Vector3[] result = new Vector3[Mathf.Min(count, pointWeights.Count)];
        for (int i = 0; i < result.Length; i++)
        {
            result[i] = pointWeights[i].Key;
        }
        
        return result;
    }
    
    /// <summary>
    /// Caches density maps for each hour of the day for faster simulation
    /// </summary>
    private void CacheDensityMaps()
    {
        densityMapsForHours.Clear();
        
        // Store original date/time
        DateTime originalDateTime = currentDateTime;
        
        // Create a map for each hour
        for (int hour = 0; hour < 24; hour++)
        {
            // Set time to this hour
            currentDateTime = new DateTime(2023, 1, 1, hour, 0, 0);
            
            // Generate density map
            UpdateDensityMap();
            
            // Save a copy
            float[,] hourMap = new float[densityMap.GetLength(0), densityMap.GetLength(1)];
            Array.Copy(densityMap, hourMap, densityMap.Length);
            
            densityMapsForHours.Add(hourMap);
        }
        
        // Restore original date/time
        currentDateTime = originalDateTime;
    }
    #endregion
}

/// <summary>
/// Represents pedestrian flow data at a specific time
/// </summary>
[System.Serializable]
public struct PedestrianTimePoint
{
    public DateTime dateTime;
    public int count;
    public Vector3 flowDirection;
    public Vector3 location;
}

/// <summary>
/// Represents the pedestrian flow state
/// </summary>
[System.Serializable]
public struct PedestrianState
{
    public float density;          // Overall pedestrian density (0-1)
    public Vector3 flowDirection;  // Primary flow direction
    public Vector3[] peakLocations; // Locations with highest density
}

/// <summary>
/// Profile for pedestrian behavior by hour
/// </summary>
[System.Serializable]
public struct PedestrianProfile
{
    public float pedestrianDensity;  // 0-1 range
    public float flowIntensity;      // 0-1 range (higher = stronger flow direction)
    public float primaryDirection;   // Degrees (0 = north)
}

/// <summary>
/// Defines a pedestrian flow path
/// </summary>
[System.Serializable]
public class PedestrianFlowPath
{
    public List<Vector3> pathPoints = new List<Vector3>();
    public float pathWeight = 1.0f;
    public bool isOneWay = false;
}

/// <summary>
/// Controls movement for an individual pedestrian
/// </summary>
public class PedestrianController : MonoBehaviour
{
    public Vector3 targetPosition;
    public float speed = 1.5f;
    public bool hasTarget = false;
    
    public void SetTarget(Vector3 position)
    {
        targetPosition = position;
        hasTarget = true;
        
        // Rotate to face target
        Vector3 direction = (targetPosition - transform.position).normalized;
        if (direction != Vector3.zero)
        {
            transform.forward = new Vector3(direction.x, 0, direction.z);
        }
    }
    
    public void UpdateMovement()
    {
        if (!hasTarget) return;
        
        // Move toward target
        Vector3 direction = (targetPosition - transform.position).normalized;
        transform.position += direction * speed * Time.deltaTime;
        
        // Rotate to face direction
        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(new Vector3(direction.x, 0, direction.z));
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
        
        // Check if reached target
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            hasTarget = false;
        }
    }
}