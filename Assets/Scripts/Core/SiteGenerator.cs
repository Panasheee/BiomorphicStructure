using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Generates a dynamic geographic site based on input data like heightmaps and building maps.
    /// </summary>
    public class SiteGenerator : MonoBehaviour
    {
        [Header("Terrain Settings")]
        [SerializeField] private int terrainSize = 500;
        [SerializeField] private int terrainResolution = 256;
        [SerializeField] private float terrainHeight = 50f;
        
        [Header("Site Data")]
        [SerializeField] private Texture2D defaultHeightmap;
        [SerializeField] private Texture2D defaultBuildingMap;
        [SerializeField] private Material terrainMaterial;
        
        [Header("Building Generation")]
        [SerializeField] private bool generateBuildings = true;
        [SerializeField] private GameObject buildingPrefab;
        [SerializeField] private float buildingHeightMultiplier = 1.0f;
        
        // References
        private Terrain terrain;
        private Transform buildingsParent;
        private List<GameObject> buildings = new List<GameObject>();
        
        // Current site data
        private Texture2D currentHeightmap;
        private Texture2D currentBuildingMap;
        
        public void Initialize()
        {
            Debug.Log("Initializing Site Generator...");
            
            // Create terrain if it doesn't exist
            if (terrain == null)
            {
                GameObject terrainObject = new GameObject("Site_Terrain");
                terrain = terrainObject.AddComponent<Terrain>();
                terrainObject.AddComponent<TerrainCollider>();
                
                // Configure the terrain
                terrain.terrainData = new TerrainData();
                terrain.terrainData.heightmapResolution = terrainResolution;
                terrain.terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);
                
                // Apply material if provided
                if (terrainMaterial != null)
                    terrain.materialTemplate = terrainMaterial;
            }
            
            // Create parent for buildings
            if (buildingsParent == null)
            {
                buildingsParent = new GameObject("Buildings").transform;
                buildingsParent.SetParent(transform);
            }
        }
        
        public void GenerateDefaultSite()
        {
            if (defaultHeightmap != null)
            {
                GenerateSiteFromData(defaultHeightmap, defaultBuildingMap);
            }
            else
            {
                GenerateRandomSite();
            }
        }
        
        public void GenerateSiteFromData(Texture2D heightmap, Texture2D buildingMap)
        {
            Debug.Log("Generating site from provided data...");
            
            // Store current data
            currentHeightmap = heightmap;
            currentBuildingMap = buildingMap;
            
            // Apply heightmap to terrain
            ApplyHeightmap(heightmap);
            
            // Generate buildings if needed
            if (generateBuildings && buildingMap != null)
            {
                ClearBuildings();
                GenerateBuildings(buildingMap);
            }
        }
        
        public void GenerateRandomSite()
        {
            Debug.Log("Generating random site...");
            
            // Create a random heightmap
            Texture2D randomHeightmap = new Texture2D(terrainResolution, terrainResolution);
            
            // Simple perlin noise heightmap
            for (int y = 0; y < terrainResolution; y++)
            {
                for (int x = 0; x < terrainResolution; x++)
                {
                    float xCoord = (float)x / terrainResolution * 5;
                    float yCoord = (float)y / terrainResolution * 5;
                    float sample = Mathf.PerlinNoise(xCoord, yCoord);
                    randomHeightmap.SetPixel(x, y, new Color(sample, sample, sample));
                }
            }
            randomHeightmap.Apply();
            
            // Generate the site with the random heightmap
            GenerateSiteFromData(randomHeightmap, null);
        }
        
        private void ApplyHeightmap(Texture2D heightmap)
        {
            if (terrain == null || heightmap == null)
                return;
            
            // Resize heightmap to match terrain resolution if needed
            int resolution = terrain.terrainData.heightmapResolution;
            float[,] heights = new float[resolution, resolution];
            
            for (int y = 0; y < resolution; y++)
            {
                for (int x = 0; x < resolution; x++)
                {
                    // Sample the heightmap (using bilinear filtering)
                    float normX = (float)x / (resolution - 1);
                    float normY = (float)y / (resolution - 1);
                    
                    Color pixelColor = heightmap.GetPixelBilinear(normX, normY);
                    heights[y, x] = pixelColor.grayscale;
                }
            }
            
            // Apply the heights to the terrain
            terrain.terrainData.SetHeights(0, 0, heights);
        }
        
        private void GenerateBuildings(Texture2D buildingMap)
        {
            if (buildingPrefab == null || buildingMap == null)
                return;
                
            int width = buildingMap.width;
            int height = buildingMap.height;
            
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    Color pixel = buildingMap.GetPixel(x, y);
                    
                    // If pixel is not black (indicating a building)
                    if (pixel.grayscale > 0.1f)
                    {
                        // Calculate world position
                        float normX = (float)x / width;
                        float normY = (float)y / height;
                        
                        float terrainX = normX * terrainSize;
                        float terrainZ = normY * terrainSize;
                        
                        // Get height at this position
                        float terrainHeight = terrain.SampleHeight(new Vector3(terrainX, 0, terrainZ));
                        
                        // Instantiate building
                        GameObject building = Instantiate(buildingPrefab, 
                            new Vector3(terrainX, terrainHeight, terrainZ), 
                            Quaternion.identity, 
                            buildingsParent);
                            
                        // Scale building based on pixel brightness
                        float buildingHeight = pixel.grayscale * buildingHeightMultiplier;
                        building.transform.localScale = new Vector3(
                            building.transform.localScale.x,
                            buildingHeight,
                            building.transform.localScale.z);
                            
                        // Add to list for tracking
                        buildings.Add(building);
                    }
                }
            }
            
            Debug.Log($"Generated {buildings.Count} buildings");
        }
        
        private void ClearBuildings()
        {
            foreach (GameObject building in buildings)
            {
                if (building != null)
                    Destroy(building);
            }
            
            buildings.Clear();
        }
          public Vector3 GetRandomPointOnTerrain()
        {
            float x = UnityEngine.Random.Range(0, terrainSize);
            float z = UnityEngine.Random.Range(0, terrainSize);
            float y = terrain.SampleHeight(new Vector3(x, 0, z));
            
            return new Vector3(x, y, z);
        }
        
        public Vector3 GetTerrainSize()
        {
            return new Vector3(terrainSize, terrainHeight, terrainSize);
        }
        
        public void Cleanup()
        {
            ClearBuildings();
        }
        
        public void ImportSiteData(string folderPath)
        {
            Debug.Log($"Importing site data from: {folderPath}");
            
            if (!Directory.Exists(folderPath))
            {
                Debug.LogError($"Folder not found: {folderPath}");
                return;
            }
            
            try
            {
                // 1. Look for DEM file
                string demPath = FindFileWithExtension(folderPath, ".tif");
                if (string.IsNullOrEmpty(demPath))
                {
                    demPath = FindFileWithExtension(folderPath, ".tiff");
                }
                
                if (string.IsNullOrEmpty(demPath))
                {
                    Debug.LogError("No DEM file (.tif or .tiff) found in the selected folder");
                    return;
                }
                
                // 2. Look for building shapefile
                string buildingShapefilePath = FindFileWithPattern(folderPath, "*building*.shp");
                if (string.IsNullOrEmpty(buildingShapefilePath))
                {
                    buildingShapefilePath = FindFileWithExtension(folderPath, ".shp"); // Try any shapefile if no building-specific one found
                }
                
                // 3. Look for road shapefile
                string roadShapefilePath = FindFileWithPattern(folderPath, "*road*.shp");
                
                // 4. Load and process DEM
                Debug.Log("Loading terrain from DEM...");
                float[,] heightmap = GISImporter.ReadDEM(demPath);
                if (heightmap == null)
                {
                    Debug.LogError("Failed to load DEM file");
                    return;
                }
                
                // 5. Load building shapefile
                List<GISImporter.PolygonFeature> buildings = null;
                if (!string.IsNullOrEmpty(buildingShapefilePath))
                {
                    Debug.Log("Loading buildings from shapefile...");
                    buildings = GISImporter.ReadBuildingShapefile(buildingShapefilePath);
                }
                
                // 6. Load road shapefile
                List<GISImporter.LineFeature> roads = null;
                if (!string.IsNullOrEmpty(roadShapefilePath))
                {
                    Debug.Log("Loading roads from shapefile...");
                    roads = GISImporter.ReadRoadShapefile(roadShapefilePath);
                }
                
                // 7. Center all features to avoid floating point precision issues
                Vector2 centerOffset;
                GISImporter.CenterFeatures(buildings, roads, ref heightmap, out centerOffset);
                
                // 8. Create terrain
                CreateTerrainFromHeightmap(heightmap);
                
                // 9. Create buildings if available
                if (buildings != null && buildings.Count > 0)
                {
                    ClearBuildings();
                    CreateBuildingsFromGISData(buildings);
                }
                
                // 10. Create roads if available
                if (roads != null && roads.Count > 0)
                {
                    CreateRoadsFromGISData(roads);
                }
                
                Debug.Log("Site data import complete!");
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error importing site data: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private string FindFileWithExtension(string folderPath, string extension)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath, $"*{extension}");
                return files.Length > 0 ? files[0] : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error searching for files: {ex.Message}");
                return null;
            }
        }
        
        private string FindFileWithPattern(string folderPath, string pattern)
        {
            try
            {
                string[] files = Directory.GetFiles(folderPath, pattern);
                return files.Length > 0 ? files[0] : null;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error searching for files: {ex.Message}");
                return null;
            }
        }
        
        private void CreateTerrainFromHeightmap(float[,] heightmap)
        {
            if (terrain == null || heightmap == null)
                return;
            
            int resolution = heightmap.GetLength(0);
            
            // Create or update terrain data
            if (terrain.terrainData == null)
            {
                terrain.terrainData = new TerrainData();
            }
            
            // Configure terrain
            TerrainData terrainData = terrain.terrainData;
            terrainData.heightmapResolution = resolution;
            terrainData.size = new Vector3(terrainSize, terrainHeight, terrainSize);
            
            // Apply heights
            terrainData.SetHeights(0, 0, heightmap);
            
            // Ensure the terrain collider is updated
            TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
            if (terrainCollider != null)
            {
                terrainCollider.terrainData = terrainData;
            }
            
            // Apply material if provided
            if (terrainMaterial != null)
                terrain.materialTemplate = terrainMaterial;
            
            Debug.Log($"Created terrain with resolution: {resolution}x{resolution}");
        }
        
        private void CreateBuildingsFromGISData(List<GISImporter.PolygonFeature> buildingFeatures)
        {
            if (buildingFeatures == null || buildingFeatures.Count == 0)
                return;
            
            // Create a materials dictionary to assign different materials based on building type
            Dictionary<string, Material> buildingMaterials = new Dictionary<string, Material>();
            
            // Create default building materials
            Material commercialMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            commercialMaterial.color = new Color(0.8f, 0.8f, 0.9f); // Light blue-gray for commercial buildings
            buildingMaterials["commercial"] = commercialMaterial;
            
            Material retailMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            retailMaterial.color = new Color(0.9f, 0.85f, 0.8f); // Tan color for retail buildings
            buildingMaterials["retail"] = retailMaterial;
            
            Material governmentMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            governmentMaterial.color = new Color(0.85f, 0.85f, 0.8f); // Light beige for government buildings
            buildingMaterials["government"] = governmentMaterial;
            
            Material landmarkMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            landmarkMaterial.color = new Color(0.95f, 0.9f, 0.8f); // Light cream for landmark buildings
            buildingMaterials["landmark"] = landmarkMaterial;
            
            Material defaultMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            defaultMaterial.color = new Color(0.85f, 0.85f, 0.85f); // Light gray default
            
            foreach (var buildingFeature in buildingFeatures)
            {
                try
                {
                    // Extract building height and type
                    float buildingHeight = buildingFeature.GetHeight();
                    string buildingType = "default";
                    
                    if (buildingFeature.Attributes.ContainsKey("type") && 
                        buildingFeature.Attributes["type"] is string typeValue)
                    {
                        buildingType = typeValue;
                    }
                    
                    // Calculate center position of the building
                    Vector2 center = Vector2.zero;
                    foreach (var point in buildingFeature.Points)
                    {
                        center += point;
                    }
                    center /= buildingFeature.Points.Count;
                    
                    // Sample terrain height at the building position
                    float terrainHeight = terrain.SampleHeight(new Vector3(center.x, 0, center.y));
                    
                    // Create the building mesh from the polygon
                    GameObject building = CreateExtrudedBuildingMesh(
                        buildingFeature.Points, 
                        buildingHeight * buildingHeightMultiplier, 
                        terrainHeight);
                    
                    building.transform.SetParent(buildingsParent);
                    
                    // Apply material based on building type
                    Renderer renderer = building.GetComponent<Renderer>();
                    if (renderer != null)
                    {
                        if (buildingMaterials.ContainsKey(buildingType))
                        {
                            renderer.material = buildingMaterials[buildingType];
                        }
                        else
                        {
                            renderer.material = defaultMaterial;
                        }
                        
                        // For landmark buildings, add a slight emissive glow
                        if (buildingType == "landmark")
                        {
                            renderer.material.SetColor("_EmissionColor", new Color(0.3f, 0.3f, 0.2f));
                            renderer.material.EnableKeyword("_EMISSION");
                        }
                    }
                    
                    // Add to list for tracking
                    buildings.Add(building);
                    
                    // Add building name as TextMesh if it exists
                    if (buildingFeature.Attributes.ContainsKey("name") && 
                        buildingFeature.Attributes["name"] is string nameValue)
                    {
                        GameObject nameLabel = new GameObject($"Label_{nameValue}");
                        nameLabel.transform.SetParent(building.transform);
                        
                        // Position slightly above the building
                        nameLabel.transform.localPosition = new Vector3(0, buildingHeight * buildingHeightMultiplier + 5f, 0);
                        
                        // Add TextMesh component
                        TextMesh textMesh = nameLabel.AddComponent<TextMesh>();
                        textMesh.text = nameValue;
                        textMesh.fontSize = 30;
                        textMesh.alignment = TextAlignment.Center;
                        textMesh.anchor = TextAnchor.LowerCenter;
                        textMesh.color = Color.white;
                        
                        // Make text face the camera
                        nameLabel.AddComponent<Billboard>();
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error creating building: {ex.Message}");
                }
            }
            
            Debug.Log($"Created {buildings.Count} buildings from GIS data");
        }
        
        /// <summary>
        /// Creates an extruded building mesh from a polygon
        /// </summary>
        private GameObject CreateExtrudedBuildingMesh(List<Vector2> footprint, float height, float baseHeight)
        {
            GameObject building = new GameObject("Building");
            
            // We need at least 3 points to make a polygon
            if (footprint == null || footprint.Count < 3)
            {
                Debug.LogError("Invalid building footprint: need at least 3 points");
                return building;
            }
            
            try
            {
                // Create mesh components
                MeshFilter meshFilter = building.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = building.AddComponent<MeshRenderer>();
                
                Mesh mesh = new Mesh();
                
                // Calculate vertices
                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uvs = new List<Vector2>();
                
                // Bottom vertices (at terrain height)
                foreach (Vector2 point in footprint)
                {
                    vertices.Add(new Vector3(point.x, baseHeight, point.y));
                }
                
                // Top vertices (at building height + terrain height)
                foreach (Vector2 point in footprint)
                {
                    vertices.Add(new Vector3(point.x, baseHeight + height, point.y));
                }
                
                // Create triangles for the sides (quads made of 2 triangles each)
                int vertexCount = footprint.Count;
                for (int i = 0; i < vertexCount; i++)
                {
                    int next = (i + 1) % vertexCount;
                    
                    // First triangle of the quad
                    triangles.Add(i);
                    triangles.Add(next);
                    triangles.Add(i + vertexCount);
                    
                    // Second triangle of the quad
                    triangles.Add(next);
                    triangles.Add(next + vertexCount);
                    triangles.Add(i + vertexCount);
                }
                
                // Create triangles for the top face (using simple fan triangulation)
                for (int i = 1; i < vertexCount - 1; i++)
                {
                    triangles.Add(vertexCount); // First top vertex
                    triangles.Add(vertexCount + i);
                    triangles.Add(vertexCount + i + 1);
                }
                
                // Create UVs
                for (int i = 0; i < vertices.Count; i++)
                {
                    // Simple UV mapping - could be improved
                    if (i < vertexCount) // Bottom vertices
                    {
                        uvs.Add(new Vector2(vertices[i].x * 0.05f, vertices[i].z * 0.05f));
                    }
                    else // Top vertices
                    {
                        uvs.Add(new Vector2(vertices[i].x * 0.05f, vertices[i].z * 0.05f));
                    }
                }
                
                // Assign to mesh
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.uv = uvs.ToArray();
                
                // Recalculate normals and bounds
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                
                // Assign the mesh to the filter
                meshFilter.mesh = mesh;
                
                // Add a box collider for physics
                BoxCollider collider = building.AddComponent<BoxCollider>();
                collider.center = new Vector3(0, height/2, 0);
                collider.size = new Vector3(mesh.bounds.size.x, height, mesh.bounds.size.z);
                
                // Position the building
                building.transform.position = new Vector3(0, 0, 0);
                
                return building;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating building mesh: {ex.Message}");
                return building;
            }
        }
        
        private void CreateRoadsFromGISData(List<GISImporter.LineFeature> roadFeatures)
        {
            if (roadFeatures == null || roadFeatures.Count == 0)
                return;
            
            // Create a parent object for all roads
            GameObject roadsParent = new GameObject("Roads");
            roadsParent.transform.SetParent(transform);
            
            // Create different materials for road types
            Material mainRoadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            mainRoadMaterial.color = new Color(0.3f, 0.3f, 0.3f); // Dark gray for main roads
            
            Material sideRoadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            sideRoadMaterial.color = new Color(0.4f, 0.4f, 0.4f); // Slightly lighter for side roads
            
            int roadCounter = 0;
            foreach (var roadFeature in roadFeatures)
            {
                try
                {
                    if (roadFeature.Points.Count < 2)
                        continue;
                    
                    // Extract road properties
                    float roadWidth = roadFeature.GetWidth();
                    string roadType = "side";
                    string roadName = $"Road_{roadCounter++}";
                    
                    if (roadFeature.Attributes.ContainsKey("type") && 
                        roadFeature.Attributes["type"] is string typeValue)
                    {
                        roadType = typeValue;
                    }
                    
                    if (roadFeature.Attributes.ContainsKey("name") && 
                        roadFeature.Attributes["name"] is string nameValue)
                    {
                        roadName = nameValue;
                    }
                    
                    // Create a game object for this road
                    GameObject road = new GameObject(roadName);
                    road.transform.SetParent(roadsParent.transform);
                    
                    if (roadFeature.Points.Count >= 30 || roadType == "main")
                    {
                        // For complex roads or main streets, create a proper mesh
                        CreateRoadMesh(road, roadFeature.Points, roadWidth, baseHeight: 0.1f);
                        
                        // Apply appropriate material
                        Renderer renderer = road.GetComponent<Renderer>();
                        if (renderer != null)
                        {
                            renderer.material = roadType == "main" ? mainRoadMaterial : sideRoadMaterial;
                        }
                    }
                    else
                    {
                        // For simpler roads, use a LineRenderer for efficiency
                        LineRenderer lineRenderer = road.AddComponent<LineRenderer>();
                        lineRenderer.startWidth = roadWidth;
                        lineRenderer.endWidth = roadWidth;
                        lineRenderer.material = roadType == "main" ? mainRoadMaterial : sideRoadMaterial;
                        lineRenderer.alignment = LineAlignment.TransformZ;
                        lineRenderer.widthMultiplier = 1.0f;
                        lineRenderer.numCapVertices = 4; // Rounded ends
                        
                        // Set positions based on the road points
                        lineRenderer.positionCount = roadFeature.Points.Count;
                        
                        for (int i = 0; i < roadFeature.Points.Count; i++)
                        {
                            Vector2 point = roadFeature.Points[i];
                            float y = terrain.SampleHeight(new Vector3(point.x, 0, point.y));
                            // Add a small offset to prevent z-fighting with terrain
                            lineRenderer.SetPosition(i, new Vector3(point.x, y + 0.1f, point.y));
                        }
                    }
                    
                    // Add road name as TextMesh
                    if (roadType == "main")
                    {
                        // Calculate a good position for the road name (middle of the road)
                        int middleIndex = roadFeature.Points.Count / 2;
                        if (middleIndex > 0 && middleIndex < roadFeature.Points.Count)
                        {
                            Vector2 point = roadFeature.Points[middleIndex];
                            float y = terrain.SampleHeight(new Vector3(point.x, 0, point.y));
                            
                            GameObject nameLabel = new GameObject($"Label_{roadName}");
                            nameLabel.transform.SetParent(road.transform);
                            nameLabel.transform.position = new Vector3(point.x, y + 1f, point.y);
                            
                            // Add TextMesh component
                            TextMesh textMesh = nameLabel.AddComponent<TextMesh>();
                            textMesh.text = roadName;
                            textMesh.fontSize = 30;
                            textMesh.alignment = TextAlignment.Center;
                            textMesh.anchor = TextAnchor.LowerCenter;
                            textMesh.color = Color.white;
                            
                            // Make text face up
                            nameLabel.transform.rotation = Quaternion.Euler(90, 0, 0);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Debug.LogError($"Error creating road: {ex.Message}");
                }
            }
            
            Debug.Log($"Created {roadCounter} roads from GIS data");
        }
        
        /// <summary>
        /// Creates a proper road mesh from a set of points
        /// </summary>
        private void CreateRoadMesh(GameObject roadObject, List<Vector2> points, float width, float baseHeight)
        {
            if (points.Count < 2)
                return;
            
            try
            {
                // Add mesh components
                MeshFilter meshFilter = roadObject.AddComponent<MeshFilter>();
                MeshRenderer meshRenderer = roadObject.AddComponent<MeshRenderer>();
                
                Mesh mesh = new Mesh();
                
                List<Vector3> vertices = new List<Vector3>();
                List<int> triangles = new List<int>();
                List<Vector2> uvs = new List<Vector2>();
                
                // Calculate perpendicular vectors for road width
                float halfWidth = width / 2;
                float uvScale = 0.1f; // Scale UVs to repeat texture
                float totalRoadLength = 0;
                
                // First, calculate the total length for UV mapping
                for (int i = 0; i < points.Count - 1; i++)
                {
                    totalRoadLength += Vector2.Distance(points[i], points[i + 1]);
                }
                
                // Generate vertices along the road
                float currentDistance = 0;
                
                for (int i = 0; i < points.Count; i++)
                {
                    Vector2 point = points[i];
                    Vector2 perpendicular;
                    
                    // Calculate direction and perpendicular
                    if (i == 0) // First point
                    {
                        Vector2 direction = (points[1] - points[0]).normalized;
                        perpendicular = new Vector2(-direction.y, direction.x);
                    }
                    else if (i == points.Count - 1) // Last point
                    {
                        Vector2 direction = (points[i] - points[i - 1]).normalized;
                        perpendicular = new Vector2(-direction.y, direction.x);
                    }
                    else // Middle points - use average direction for smoother corners
                    {
                        Vector2 dir1 = (points[i] - points[i - 1]).normalized;
                        Vector2 dir2 = (points[i + 1] - points[i]).normalized;
                        Vector2 avgDir = ((dir1 + dir2) / 2).normalized;
                        perpendicular = new Vector2(-avgDir.y, avgDir.x);
                        
                        // Update distance for UV mapping
                        currentDistance += Vector2.Distance(points[i - 1], points[i]);
                    }
                    
                    // Get height at this point on the terrain
                    float y = terrain.SampleHeight(new Vector3(point.x, 0, point.y)) + baseHeight;
                    
                    // Add vertices for both sides of the road
                    Vector3 leftVert = new Vector3(point.x + perpendicular.x * halfWidth, y, point.y + perpendicular.y * halfWidth);
                    Vector3 rightVert = new Vector3(point.x - perpendicular.x * halfWidth, y, point.y - perpendicular.y * halfWidth);
                    
                    vertices.Add(leftVert);
                    vertices.Add(rightVert);
                    
                    // Add UVs
                    float u = currentDistance / totalRoadLength * uvScale;
                    uvs.Add(new Vector2(0, u)); // Left edge
                    uvs.Add(new Vector2(1, u)); // Right edge
                }
                
                // Create triangles (two for each quad segment)
                for (int i = 0; i < points.Count - 1; i++)
                {
                    int baseIndex = i * 2;
                    
                    // First triangle
                    triangles.Add(baseIndex);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 1);
                    
                    // Second triangle
                    triangles.Add(baseIndex + 1);
                    triangles.Add(baseIndex + 2);
                    triangles.Add(baseIndex + 3);
                }
                
                // Assign to mesh
                mesh.vertices = vertices.ToArray();
                mesh.triangles = triangles.ToArray();
                mesh.uv = uvs.ToArray();
                
                // Recalculate normals and bounds
                mesh.RecalculateNormals();
                mesh.RecalculateBounds();
                
                // Assign the mesh to the filter
                meshFilter.mesh = mesh;
                
                // Add collider for physics
                MeshCollider collider = roadObject.AddComponent<MeshCollider>();
                collider.sharedMesh = mesh;
                
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error creating road mesh: {ex.Message}\n{ex.StackTrace}");
            }
        }
    }
}