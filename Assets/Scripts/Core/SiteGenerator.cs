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
        }        public void GenerateDefaultSite()
        {
            Debug.Log("BiomorphicSim: Generating Lambton Quay site with accurate topography...");
            
            // Clear any existing terrain/buildings
            ClearBuildings();
            
            // Create accurate Wellington topography first
            CreateWellingtonTopography();
            
            // Create a representation of the Wellington buildings since SLPK files can't be loaded directly
            CreateWellingtonBuildings();
            
            Debug.Log("Lambton Quay site with buildings created successfully");
        }
          private void CreateWellingtonBuildings()
        {
            Debug.Log("Creating Wellington buildings mockup for Lambton Quay area...");
            
            // Create a parent object for all buildings
            GameObject buildingsContainer = new GameObject("WellingtonBuildings");
            buildingsContainer.transform.SetParent(transform);
            
            // Get materials from helper class
            Material officeMaterial = BuildingMaterialHelper.GetOfficeBuildingMaterial();
            Material governmentMaterial = BuildingMaterialHelper.GetGovernmentBuildingMaterial();
            Material retailMaterial = BuildingMaterialHelper.GetRetailBuildingMaterial();
            Material roofMaterial = BuildingMaterialHelper.GetRoofMaterial();
            
            // Create main Lambton Quay buildings (simplified representations)
            // These are approximate positions based on the area
            
            // The Terrace along the western side (multiple office buildings)
            for (int i = 0; i < 8; i++)
            {
                float xPos = 400 + UnityEngine.Random.Range(-20f, 20f);
                float zPos = 100 + i * 60 + UnityEngine.Random.Range(-10f, 10f);
                float height = UnityEngine.Random.Range(30f, 60f);
                float width = UnityEngine.Random.Range(20f, 40f);
                float depth = UnityEngine.Random.Range(20f, 40f);
                
                CreateDetailedBuilding(buildingsContainer.transform, 
                    new Vector3(xPos, 0, zPos), 
                    new Vector3(width, height, depth), 
                    officeMaterial, 
                    roofMaterial,
                    $"TerraceBuilding_{i}");
            }
            
            // Lambton Quay main street buildings (government and retail)
            for (int i = 0; i < 12; i++)
            {
                float xPos = 300 + UnityEngine.Random.Range(-20f, 20f);
                float zPos = 80 + i * 40 + UnityEngine.Random.Range(-5f, 5f);
                float height = UnityEngine.Random.Range(15f, 40f);
                float width = UnityEngine.Random.Range(25f, 50f);
                float depth = UnityEngine.Random.Range(25f, 50f);
                
                Material buildingMat = i % 3 == 0 ? governmentMaterial : retailMaterial;
                
                CreateDetailedBuilding(buildingsContainer.transform, 
                    new Vector3(xPos, 0, zPos), 
                    new Vector3(width, height, depth), 
                    buildingMat,
                    roofMaterial,
                    $"LambtonQuayBuilding_{i}");
            }
            
            // Beehive (Parliament) - distinctive government building
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(350, 0, 500),
                new Vector3(80, 25, 60),
                governmentMaterial,
                roofMaterial,
                "Parliament",
                true); // Special landmark
                
            // Old Government Buildings
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(320, 0, 450),
                new Vector3(70, 20, 70),
                governmentMaterial,
                roofMaterial,
                "OldGovernmentBuildings",
                true); // Special landmark
                
            // TSB Arena / Shed 6 near waterfront
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(180, 0, 300),
                new Vector3(100, 15, 60),
                retailMaterial,
                roofMaterial,
                "TSB_Arena");
                
            // Wellington Railway Station
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(250, 0, 520),
                new Vector3(120, 18, 60),
                governmentMaterial,
                roofMaterial,
                "RailwayStation",
                true); // Special landmark
                
            // Add some landmark buildings
            // Michael Fowler Centre
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(200, 0, 400),
                new Vector3(50, 25, 50),
                retailMaterial,
                roofMaterial,
                "MichaelFowlerCentre",
                true); // Special landmark
                
            // Create some waterfront features
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(150, 0, 250),
                new Vector3(30, 10, 30),
                retailMaterial,
                roofMaterial,
                "WaterfrontBuilding_1");
                
            CreateDetailedBuilding(buildingsContainer.transform,
                new Vector3(120, 0, 350),
                new Vector3(25, 8, 25),
                retailMaterial,
                roofMaterial,
                "WaterfrontBuilding_2");
            
            // Add some streets/roads to make the city look more realistic
            CreateWellingtonRoads(buildingsContainer.transform);
                
            Debug.Log("Wellington buildings mockup created successfully");
        }
        
        private void CreateDetailedBuilding(Transform parent, Vector3 position, Vector3 size, 
                                           Material wallMaterial, Material roofMaterial, string name, 
                                           bool isLandmark = false)
        {
            // Sample terrain height at building position
            float terrainHeight = 0;
            if (terrain != null)
            {
                terrainHeight = terrain.SampleHeight(position);
            }
            
            // Create building parent
            GameObject buildingParent = new GameObject(name);
            buildingParent.transform.SetParent(parent);
            buildingParent.transform.position = new Vector3(position.x, terrainHeight, position.z);
            
            // Create main building body (walls)
            GameObject buildingBody = GameObject.CreatePrimitive(PrimitiveType.Cube);
            buildingBody.name = name + "_Body";
            buildingBody.transform.SetParent(buildingParent.transform);
            buildingBody.transform.localPosition = new Vector3(0, size.y / 2f, 0);
            buildingBody.transform.localScale = new Vector3(size.x, size.y, size.z);
            
            // Apply wall material
            if (wallMaterial != null)
            {
                buildingBody.GetComponent<Renderer>().material = wallMaterial;
                
                // If it's a landmark, add some subtle glow effect
                if (isLandmark)
                {
                    buildingBody.GetComponent<Renderer>().material.EnableKeyword("_EMISSION");
                    buildingBody.GetComponent<Renderer>().material.SetColor("_EmissionColor", new Color(0.1f, 0.1f, 0.1f));
                }
            }
            
            // Create roof
            GameObject roof = GameObject.CreatePrimitive(PrimitiveType.Cube);
            roof.name = name + "_Roof";
            roof.transform.SetParent(buildingParent.transform);
            roof.transform.localPosition = new Vector3(0, size.y + 0.1f, 0);
            roof.transform.localScale = new Vector3(size.x, 0.2f, size.z);
            
            // Apply roof material
            if (roofMaterial != null)
            {
                roof.GetComponent<Renderer>().material = roofMaterial;
            }
            
            // Add some details for larger buildings
            if (size.x > 30 && size.z > 30)
            {
                // Add some rooftop structures
                AddRooftopStructures(buildingParent.transform, size);
            }
            
            // For landmarks, add a small text label
            if (isLandmark)
            {
                GameObject label = new GameObject(name + "_Label");
                label.transform.SetParent(buildingParent.transform);
                label.transform.localPosition = new Vector3(0, size.y + 5f, 0);
                
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = name;
                textMesh.fontSize = 14;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.LowerCenter;
                textMesh.color = Color.white;
                
                // Make the text face the camera
                label.AddComponent<Billboard>();
            }
            
            // Add to the list for tracking
            buildings.Add(buildingParent);
        }
        
        private void AddRooftopStructures(Transform buildingTransform, Vector3 buildingSize)
        {
            // Add a few small structures on the roof
            int numStructures = UnityEngine.Random.Range(1, 4);
            
            for (int i = 0; i < numStructures; i++)
            {
                GameObject structure = GameObject.CreatePrimitive(PrimitiveType.Cube);
                structure.name = buildingTransform.name + "_RoofStructure_" + i;
                structure.transform.SetParent(buildingTransform);
                
                // Random size and position on the roof
                float width = UnityEngine.Random.Range(3f, 8f);
                float height = UnityEngine.Random.Range(3f, 6f);
                float depth = UnityEngine.Random.Range(3f, 8f);
                
                float xPos = UnityEngine.Random.Range(-buildingSize.x/2f + width, buildingSize.x/2f - width);
                float zPos = UnityEngine.Random.Range(-buildingSize.z/2f + depth, buildingSize.z/2f - depth);
                
                structure.transform.localPosition = new Vector3(xPos, buildingSize.y + height/2f, zPos);
                structure.transform.localScale = new Vector3(width, height, depth);
                
                // Apply a simple gray material
                structure.GetComponent<Renderer>().material.color = new Color(0.6f, 0.6f, 0.6f);
            }
        }
        
        private void CreateWellingtonRoads(Transform parent)
        {
            // Create main roads of Wellington CBD
            GameObject roadsContainer = new GameObject("WellingtonRoads");
            roadsContainer.transform.SetParent(parent);
            
            // Create road material
            Material roadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMaterial.color = new Color(0.2f, 0.2f, 0.2f); // Dark asphalt
            
            // Create main streets
            CreateRoad(roadsContainer.transform, "Lambton Quay", roadMaterial, new Vector3[] {
                new Vector3(270, 0, 50),
                new Vector3(280, 0, 200),
                new Vector3(290, 0, 350),
                new Vector3(300, 0, 500)
            }, 15f);
            
            CreateRoad(roadsContainer.transform, "The Terrace", roadMaterial, new Vector3[] {
                new Vector3(410, 0, 50),
                new Vector3(420, 0, 200),
                new Vector3(430, 0, 350),
                new Vector3(440, 0, 500)
            }, 12f);
            
            CreateRoad(roadsContainer.transform, "Featherston Street", roadMaterial, new Vector3[] {
                new Vector3(320, 0, 50),
                new Vector3(330, 0, 200),
                new Vector3(340, 0, 350),
                new Vector3(350, 0, 500)
            }, 12f);
            
            // Create some connecting streets
            for (int i = 0; i < 5; i++)
            {
                float zPos = 100 + i * 100;
                CreateRoad(roadsContainer.transform, "Cross Street " + i, roadMaterial, new Vector3[] {
                    new Vector3(270, 0, zPos),
                    new Vector3(440, 0, zPos)
                }, 10f);
            }
        }
        
        private void CreateRoad(Transform parent, string roadName, Material roadMaterial, Vector3[] points, float width)
        {
            if (points.Length < 2)
                return;
                
            GameObject road = new GameObject(roadName);
            road.transform.SetParent(parent);
            
            // Create a line renderer for the road
            LineRenderer roadRenderer = road.AddComponent<LineRenderer>();
            roadRenderer.material = roadMaterial;
            roadRenderer.startWidth = width;
            roadRenderer.endWidth = width;
            roadRenderer.positionCount = points.Length;
            
            // Get heights from terrain and set positions
            for (int i = 0; i < points.Length; i++)
            {
                if (terrain != null)
                {
                    float y = terrain.SampleHeight(points[i]) + 0.1f; // Slightly above terrain
                    points[i].y = y;
                }
            }
            
            roadRenderer.SetPositions(points);
            
            // Add road name label in the middle
            if (points.Length >= 2)
            {
                int middleIndex = points.Length / 2;
                Vector3 labelPos = points[middleIndex];
                
                GameObject label = new GameObject(roadName + "_Label");
                label.transform.SetParent(road.transform);
                label.transform.position = new Vector3(labelPos.x, labelPos.y + 0.2f, labelPos.z);
                
                TextMesh textMesh = label.AddComponent<TextMesh>();
                textMesh.text = roadName;
                textMesh.fontSize = 12;
                textMesh.alignment = TextAlignment.Center;
                textMesh.anchor = TextAnchor.LowerCenter;
                textMesh.color = Color.white;
                
                // Make text face up
                label.transform.rotation = Quaternion.Euler(90, 0, 0);
            }
        }
        
private void CreateWellingtonTopography()
        {
            Debug.Log("Creating accurate Wellington topography for Lambton Quay area...");
            
            // Ensure we have a terrain to work with
            if (terrain == null)
            {
                GameObject terrainObject = new GameObject("Wellington_Terrain");
                terrainObject.transform.SetParent(transform);
                terrain = terrainObject.AddComponent<Terrain>();
                terrainObject.AddComponent<TerrainCollider>();
            }
            
            // Configure the terrain data for Wellington CBD area
            // The CBD area from Whitmore St to Willis/Lambton corner is approx 500-600m
            TerrainData terrainData = new TerrainData();
            
            // Set appropriate terrain size: 600m x 600m for the CBD section
            terrainData.size = new Vector3(600, 50, 600); // X, Y (height), Z dimensions in meters
            
            // Wellington has steep hills around the CBD
            // Set heightmap resolution (256 is a good balance between detail and performance)
            terrainData.heightmapResolution = 257; // Must be 2^n + 1
            
            // Create a heightmap that represents Wellington's topography
            // Lambton Quay area sits at the base of a steep hill (Terrace)
            float[,] heights = new float[terrainData.heightmapResolution, terrainData.heightmapResolution];
            
            // Parameters for Wellington topography
            float terraceSteepness = 0.4f; // Steepness of the Wellington Terrace
            float terraceHeight = 0.3f;    // Relative height of the terrace (0-1)
            float terracePosition = 0.65f; // Position of the terrace from west edge (0-1)
            float harbourFlatness = 0.05f; // How flat the harbour-side area is
            
            // Generate heightmap
            for (int y = 0; y < terrainData.heightmapResolution; y++)
            {
                for (int x = 0; x < terrainData.heightmapResolution; x++)
                {
                    // Normalize coordinates to 0-1 range
                    float nx = (float)x / (terrainData.heightmapResolution - 1);
                    float ny = (float)y / (terrainData.heightmapResolution - 1);
                    
                    // Base height value
                    float height = 0;
                    
                    // Create the steep terrace that rises west of Lambton Quay
                    if (nx > terracePosition)
                    {
                        // Calculate height based on distance from terrace
                        float terraceDistance = (nx - terracePosition) / (1 - terracePosition);
                        height = terraceHeight * Mathf.Pow(terraceDistance, 1.0f / terraceSteepness);
                        
                        // Add some noise to the hills
                        height += 0.05f * Mathf.PerlinNoise(nx * 10, ny * 10);
                    }
                    else
                    {
                        // Lambton Quay and harbour-side area is relatively flat
                        height = harbourFlatness * nx / terracePosition;
                        
                        // Add slight noise for natural terrain
                        height += 0.01f * Mathf.PerlinNoise(nx * 15, ny * 15);
                    }
                    
                    // Add a slight north-south gradient (the CBD slopes down toward the harbor)
                    height += 0.05f * (1 - ny);
                    
                    // Store the height value (clamped to 0-1)
                    heights[y, x] = Mathf.Clamp01(height);
                }
            }            // Apply the heightmap to terrain
            terrainData.SetHeights(0, 0, heights);
            
            // Try to load a satellite texture of Wellington from resources
            Texture2D wellingtonTexture = Resources.Load<Texture2D>("Textures/WellingtonSatellite");
            
            // If no Wellington satellite texture found, try to load a general urban texture
            if (wellingtonTexture == null)
            {
                wellingtonTexture = Resources.Load<Texture2D>("Textures/UrbanGround");
                
                // If still no texture found, create a simple grey texture with a slight green tint
                if (wellingtonTexture == null)
                {
                    Debug.LogWarning("No terrain texture found. Creating a default texture.");
                    wellingtonTexture = new Texture2D(1024, 1024);
                    
                    // Create a more interesting texture with variation instead of solid purple
                    for (int y = 0; y < wellingtonTexture.height; y++)
                    {
                        for (int x = 0; x < wellingtonTexture.width; x++)
                        {
                            // Create a slightly varied green-gray color for urban areas
                            float noise = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.2f;
                            float r = 0.4f + noise * 0.1f;
                            float g = 0.45f + noise * 0.15f;
                            float b = 0.4f + noise * 0.1f;
                            wellingtonTexture.SetPixel(x, y, new Color(r, g, b));
                        }
                    }
                    wellingtonTexture.Apply();
                }
            }
            
            // Create a terrain layer for the texture
            TerrainLayer terrainLayer = new TerrainLayer();
            terrainLayer.diffuseTexture = wellingtonTexture;
            terrainLayer.tileSize = new Vector2(600, 600); // Tile size matching the terrain size
            terrainLayer.tileOffset = Vector2.zero;
            
            // Apply the terrain layer
            terrainData.terrainLayers = new TerrainLayer[] { terrainLayer };
            
            // Apply terrain settings
            terrain.terrainData = terrainData;
            
            // Ensure the terrain collider uses the same data
            TerrainCollider terrainCollider = terrain.GetComponent<TerrainCollider>();
            if (terrainCollider != null)
            {
                terrainCollider.terrainData = terrainData;
            }
            
            // Apply material settings
            if (terrainMaterial != null)
            {
                terrain.materialTemplate = terrainMaterial;
            }
            
            Debug.Log("Wellington topography created successfully");
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
            Debug.Log("BiomorphicSim: Generating random site...");
            
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
            
            // Store as current heightmap
            currentHeightmap = randomHeightmap;
            currentBuildingMap = null;
            
            // Apply to terrain directly instead of calling GenerateSiteFromData to avoid recursive loops
            ApplyHeightmap(randomHeightmap);
            
            // Clear any existing buildings
            ClearBuildings();
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