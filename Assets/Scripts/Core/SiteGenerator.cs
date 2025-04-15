using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core; // Add this line to use the correct SiteSettings class

/// <summary>
/// Handles the generation of the Wellington Lambton Quay site as a 3D environment.
/// This includes terrain, buildings, roads, and other relevant features.
/// </summary>
public class SiteGenerator : MonoBehaviour
{
    #region References
    [Header("Terrain")]
    [SerializeField] private Material terrainMaterial;
    [SerializeField] private Texture2D heightmapTexture;
    
    [Header("Buildings")]
    [SerializeField] private GameObject[] buildingPrefabs;
    [SerializeField] private Material[] buildingMaterials;
    
    [Header("Roads")]
    [SerializeField] private Material roadMaterial;
    [SerializeField] private Texture2D roadNetworkTexture;
    
    [Header("Vegetation")]
    [SerializeField] private GameObject[] vegetationPrefabs;
    
    [Header("Water")]
    [SerializeField] private Material waterMaterial;
    
    [Header("Settings")]
    [SerializeField] private SiteSettings settings;
    #endregion

    #region State
    // Generated site container
    private GameObject siteContainer;
    
    // Site components
    private GameObject terrainObject;
    private GameObject buildingsContainer;
    private GameObject roadsContainer;
    private GameObject vegetationContainer;
    private GameObject waterObject;
    
    // Site bounds
    private Bounds siteBounds;
      // Wellington Lambton Quay specific data
    private LambtonQuayData lambtonQuayData;
    #endregion

    #region Public Methods
    /// <summary>
    /// Sets the heightmap texture used for terrain generation
    /// </summary>
    public void SetHeightmapTexture(Texture2D heightmap)
    {
        heightmapTexture = heightmap;
        Debug.Log("Heightmap texture set for site generation");
    }
    
    /// <summary>
    /// Sets the building map texture used for building placement
    /// </summary>
    public void SetBuildingMapTexture(Texture2D buildingMap)
    {
        Debug.Log("Building map texture set for site generation");
        // Store the building map for use during generation
    }
    
    /// <summary>
    /// Loads geo data from a text asset
    /// </summary>
    public void LoadGeoData(TextAsset geoData)
    {
        Debug.Log("Geo data loaded for site generation");
        // Parse geo data for use during generation
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Initializes the site generator
    /// </summary>
    public void Initialize()
    {
        // Create site container if it doesn't exist
        if (siteContainer == null)
        {
            siteContainer = new GameObject("SiteContainer");
            siteContainer.transform.parent = transform;
        }
        
        // Create sub-containers
        if (buildingsContainer == null)
        {
            buildingsContainer = new GameObject("BuildingsContainer");
            buildingsContainer.transform.parent = siteContainer.transform;
        }
        
        if (roadsContainer == null)
        {
            roadsContainer = new GameObject("RoadsContainer");
            roadsContainer.transform.parent = siteContainer.transform;
        }
        
        if (vegetationContainer == null)
        {
            vegetationContainer = new GameObject("VegetationContainer");
            vegetationContainer.transform.parent = siteContainer.transform;
        }
        
        // Initialize Lambton Quay data
        lambtonQuayData = new LambtonQuayData();
    }
    
    /// <summary>
    /// Updates the site generation settings
    /// </summary>
    /// <param name="newSettings">New settings to apply</param>
    public void UpdateSettings(SiteSettings newSettings)
    {
        settings = newSettings;
    }
    
    /// <summary>
    /// Generates the Wellington Lambton Quay site
    /// </summary>
    /// <param name="siteSettings">Settings for site generation</param>
    public void GenerateSite(SiteSettings siteSettings)
    {
        UpdateSettings(siteSettings);
        
        // Clear existing site if any
        ClearSite();
        
        // Generate site components
        GenerateTerrain();
        
        if (settings.includeBuildings)
        {
            GenerateBuildings();
        }
        
        if (settings.includeRoads)
        {
            GenerateRoads();
        }
        
        if (settings.includeVegetation)
        {
            GenerateVegetation();
        }
        
        GenerateWater();
        
        // Calculate site bounds
        CalculateSiteBounds();
        
        Debug.Log("Wellington Lambton Quay site generation complete.");
    }
    
    /// <summary>
    /// Clears the generated site
    /// </summary>
    public void ClearSite()
    {
        // Destroy all children of the site container
        foreach (Transform child in siteContainer.transform)
        {
            Destroy(child.gameObject);
        }
        
        // Recreate sub-containers
        buildingsContainer = new GameObject("BuildingsContainer");
        buildingsContainer.transform.parent = siteContainer.transform;
        
        roadsContainer = new GameObject("RoadsContainer");
        roadsContainer.transform.parent = siteContainer.transform;
        
        vegetationContainer = new GameObject("VegetationContainer");
        vegetationContainer.transform.parent = siteContainer.transform;
        
        // Reset state
        terrainObject = null;
        waterObject = null;
    }
    
    /// <summary>
    /// Returns the bounds of the generated site
    /// </summary>
    public Bounds GetSiteBounds()
    {
        return siteBounds;
    }
    
    /// <summary>
    /// Returns a random zone within the site bounds
    /// </summary>
    /// <param name="zoneSize">Size of the zone</param>
    public Bounds GetRandomZone(Vector3 zoneSize)
    {
        Vector3 min = siteBounds.min + zoneSize * 0.5f;
        Vector3 max = siteBounds.max - zoneSize * 0.5f;
        
        Vector3 randomCenter = new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
        
        return new Bounds(randomCenter, zoneSize);
    }
    
    /// <summary>
    /// Returns a random building position within the site
    /// </summary>
    public Vector3 GetRandomBuildingPosition()
    {
        BuildingData randomBuilding = lambtonQuayData.buildings[Random.Range(0, lambtonQuayData.buildings.Count)];
        return new Vector3(randomBuilding.position.x, randomBuilding.position.y, randomBuilding.position.z);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Generates the terrain for the site
    /// </summary>
    private void GenerateTerrain()
    {
        Debug.Log("Generating terrain...");
        
        // Create terrain object
        terrainObject = new GameObject("Terrain");
        terrainObject.transform.parent = siteContainer.transform;
        
        // Add mesh components
        MeshFilter meshFilter = terrainObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = terrainObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = terrainObject.AddComponent<MeshCollider>();
        
        // Create terrain mesh
        if (heightmapTexture != null)
        {
            // Generate mesh from heightmap
            Mesh terrainMesh = GenerateTerrainMeshFromHeightmap(heightmapTexture);
            meshFilter.mesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
        }
        else
        {
            // Generate procedural terrain mesh
            Mesh terrainMesh = GenerateProceduralTerrainMesh(settings.siteSize.x, settings.siteSize.z, 100);
            meshFilter.mesh = terrainMesh;
            meshCollider.sharedMesh = terrainMesh;
        }
        
        // Apply material
        if (terrainMaterial != null)
        {
            meshRenderer.material = terrainMaterial;
        }
        else
        {
            // Create basic terrain material
            meshRenderer.material = CreateBasicTerrainMaterial();
        }
    }
    
    /// <summary>
    /// Generates buildings for the site
    /// </summary>
    private void GenerateBuildings()
    {
        Debug.Log("Generating buildings...");
        
        // Use Lambton Quay data to place buildings
        foreach (BuildingData building in lambtonQuayData.buildings)
        {
            // Determine which building prefab to use
            GameObject buildingPrefab = null;
            
            if (buildingPrefabs != null && buildingPrefabs.Length > 0)
            {
                // Select a prefab based on building type
                int prefabIndex = Mathf.Min(building.buildingType, buildingPrefabs.Length - 1);
                buildingPrefab = buildingPrefabs[prefabIndex];
            }
            
            // If no prefab available, create procedural building
            if (buildingPrefab == null)
            {
                CreateProceduralBuilding(building);
            }
            else
            {
                // Instantiate prefab
                GameObject buildingObject = Instantiate(
                    buildingPrefab,
                    new Vector3(building.position.x, building.position.y, building.position.z),
                    Quaternion.Euler(0, building.rotation, 0),
                    buildingsContainer.transform
                );
                
                // Scale to match building dimensions
                buildingObject.transform.localScale = new Vector3(
                    building.size.x,
                    building.size.y,
                    building.size.z
                );
                
                // Name the building
                buildingObject.name = $"Building_{building.id}_{building.name}";
                
                // Apply material if available
                if (buildingMaterials != null && buildingMaterials.Length > 0)
                {
                    MeshRenderer renderer = buildingObject.GetComponent<MeshRenderer>();
                    if (renderer != null)
                    {
                        int materialIndex = Mathf.Min(building.buildingType, buildingMaterials.Length - 1);
                        renderer.material = buildingMaterials[materialIndex];
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// Generates roads for the site
    /// </summary>
    private void GenerateRoads()
    {
        Debug.Log("Generating roads...");
        
        // Create road network object
        GameObject roadNetwork = new GameObject("RoadNetwork");
        roadNetwork.transform.parent = roadsContainer.transform;
        
        // Add mesh components
        MeshFilter meshFilter = roadNetwork.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = roadNetwork.AddComponent<MeshRenderer>();
        
        // Create road mesh
        if (roadNetworkTexture != null)
        {
            // Generate mesh from texture
            Mesh roadMesh = GenerateRoadMeshFromTexture(roadNetworkTexture);
            meshFilter.mesh = roadMesh;
        }
        else
        {
            // Generate procedural road mesh
            Mesh roadMesh = GenerateProceduralRoadMesh();
            meshFilter.mesh = roadMesh;
        }
        
        // Apply material
        if (roadMaterial != null)
        {
            meshRenderer.material = roadMaterial;
        }
        else
        {
            // Create basic road material
            meshRenderer.material = CreateBasicRoadMaterial();
        }
        
        // Add specific Lambton Quay roads
        foreach (RoadData road in lambtonQuayData.roads)
        {
            CreateRoadSegment(road);
        }
    }
    
    /// <summary>
    /// Generates vegetation for the site
    /// </summary>
    private void GenerateVegetation()
    {
        Debug.Log("Generating vegetation...");
        
        // Add vegetation based on Lambton Quay data
        foreach (VegetationData vegetation in lambtonQuayData.vegetation)
        {
            // Determine which vegetation prefab to use
            GameObject vegetationPrefab = null;
            
            if (vegetationPrefabs != null && vegetationPrefabs.Length > 0)
            {
                // Select a prefab based on vegetation type
                int prefabIndex = Mathf.Min(vegetation.vegetationType, vegetationPrefabs.Length - 1);
                vegetationPrefab = vegetationPrefabs[prefabIndex];
            }
            
            // If no prefab available, create procedural vegetation
            if (vegetationPrefab == null)
            {
                CreateProceduralVegetation(vegetation);
            }
            else
            {
                // Instantiate prefab
                GameObject vegetationObject = Instantiate(
                    vegetationPrefab,
                    new Vector3(vegetation.position.x, vegetation.position.y, vegetation.position.z),
                    Quaternion.Euler(0, vegetation.rotation, 0),
                    vegetationContainer.transform
                );
                
                // Scale appropriately
                vegetationObject.transform.localScale = new Vector3(
                    vegetation.scale,
                    vegetation.scale,
                    vegetation.scale
                );
                
                // Name the vegetation
                vegetationObject.name = $"Vegetation_{vegetation.id}_{vegetation.type}";
            }
        }
        
        // Add random vegetation if needed
        if (lambtonQuayData.vegetation.Count < 50)
        {
            int additionalVegetation = 50 - lambtonQuayData.vegetation.Count;
            AddRandomVegetation(additionalVegetation);
        }
    }
    
    /// <summary>
    /// Generates water features for the site
    /// </summary>
    private void GenerateWater()
    {
        Debug.Log("Generating water features...");
        
        // Create water object
        waterObject = new GameObject("Water");
        waterObject.transform.parent = siteContainer.transform;
        
        // Add mesh components
        MeshFilter meshFilter = waterObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = waterObject.AddComponent<MeshRenderer>();
        
        // Create water mesh
        Mesh waterMesh = GenerateWaterMesh();
        meshFilter.mesh = waterMesh;
        
        // Apply material
        if (waterMaterial != null)
        {
            meshRenderer.material = waterMaterial;
        }
        else
        {
            // Create basic water material
            meshRenderer.material = CreateBasicWaterMaterial();
        }
    }
    
    /// <summary>
    /// Calculates the bounds of the entire site
    /// </summary>
    private void CalculateSiteBounds()
    {
        // Start with terrain bounds
        siteBounds = new Bounds(terrainObject.transform.position, settings.siteSize);
        
        // Expand to include all site objects
        foreach (Transform child in siteContainer.transform)
        {
            Renderer renderer = child.GetComponent<Renderer>();
            if (renderer != null)
            {
                siteBounds.Encapsulate(renderer.bounds);
            }
            
            // Include children of containers
            foreach (Transform grandchild in child)
            {
                renderer = grandchild.GetComponent<Renderer>();
                if (renderer != null)
                {
                    siteBounds.Encapsulate(renderer.bounds);
                }
            }
        }
    }
    
    /// <summary>
    /// Creates a procedural building based on building data
    /// </summary>
    private void CreateProceduralBuilding(BuildingData building)
    {
        // Create building object
        GameObject buildingObject = new GameObject($"Building_{building.id}_{building.name}");
        buildingObject.transform.parent = buildingsContainer.transform;
        buildingObject.transform.position = new Vector3(building.position.x, building.position.y, building.position.z);
        buildingObject.transform.rotation = Quaternion.Euler(0, building.rotation, 0);
        
        // Add mesh components
        MeshFilter meshFilter = buildingObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = buildingObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = buildingObject.AddComponent<MeshCollider>();
        
        // Create building mesh
        Mesh buildingMesh = GenerateBuildingMesh(building.size.x, building.size.y, building.size.z, building.buildingType);
        meshFilter.mesh = buildingMesh;
        meshCollider.sharedMesh = buildingMesh;
        
        // Apply material
        if (buildingMaterials != null && buildingMaterials.Length > 0)
        {
            int materialIndex = Mathf.Min(building.buildingType, buildingMaterials.Length - 1);
            meshRenderer.material = buildingMaterials[materialIndex];
        }
        else
        {
            // Create basic building material
            meshRenderer.material = CreateBasicBuildingMaterial(building.buildingType);
        }
    }
    
    /// <summary>
    /// Creates a road segment based on road data
    /// </summary>
    private void CreateRoadSegment(RoadData road)
    {
        // Create road object
        GameObject roadObject = new GameObject($"Road_{road.id}_{road.name}");
        roadObject.transform.parent = roadsContainer.transform;
        
        // Add mesh components
        MeshFilter meshFilter = roadObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = roadObject.AddComponent<MeshRenderer>();
        
        // Create road mesh from spline points
        Mesh roadMesh = GenerateRoadMeshFromSpline(road.splinePoints, road.width);
        meshFilter.mesh = roadMesh;
        
        // Apply material
        if (roadMaterial != null)
        {
            meshRenderer.material = roadMaterial;
        }
        else
        {
            // Create basic road material
            meshRenderer.material = CreateBasicRoadMaterial();
        }
    }
    
    /// <summary>
    /// Creates procedural vegetation based on vegetation data
    /// </summary>
    private void CreateProceduralVegetation(VegetationData vegetation)
    {
        // Create vegetation object
        GameObject vegetationObject = new GameObject($"Vegetation_{vegetation.id}_{vegetation.type}");
        vegetationObject.transform.parent = vegetationContainer.transform;
        vegetationObject.transform.position = new Vector3(vegetation.position.x, vegetation.position.y, vegetation.position.z);
        vegetationObject.transform.rotation = Quaternion.Euler(0, vegetation.rotation, 0);
        
        // Add mesh components
        MeshFilter meshFilter = vegetationObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = vegetationObject.AddComponent<MeshRenderer>();
        
        // Create vegetation mesh based on type
        Mesh vegetationMesh = null;
        
        switch (vegetation.type)
        {
            case "Tree":
                vegetationMesh = GenerateTreeMesh(vegetation.scale);
                break;
                
            case "Bush":
                vegetationMesh = GenerateBushMesh(vegetation.scale);
                break;
                
            case "Flower":
                vegetationMesh = GenerateFlowerMesh(vegetation.scale);
                break;
                
            default:
                vegetationMesh = GenerateGenericVegetationMesh(vegetation.scale);
                break;
        }
        
        meshFilter.mesh = vegetationMesh;
        
        // Create and apply material
        meshRenderer.material = CreateVegetationMaterial(vegetation.type);
    }
    
    /// <summary>
    /// Adds random vegetation throughout the site
    /// </summary>
    private void AddRandomVegetation(int count)
    {
        for (int i = 0; i < count; i++)
        {
            // Create random vegetation data
            VegetationData vegetation = new VegetationData
            {
                id = i + 1000, // Start from a high number to avoid conflicts
                type = Random.value > 0.5f ? "Tree" : "Bush",
                vegetationType = Random.Range(0, 3),
                position = new Vector3(
                    Random.Range(siteBounds.min.x + 10, siteBounds.max.x - 10),
                    0, // Will be adjusted to terrain height
                    Random.Range(siteBounds.min.z + 10, siteBounds.max.z - 10)
                ),
                rotation = Random.Range(0, 360),
                scale = Random.Range(0.8f, 1.5f)
            };
            
            // Adjust height to terrain
            vegetation.position.y = GetTerrainHeightAt(vegetation.position) + 0.1f;
            
            // Create the vegetation
            CreateProceduralVegetation(vegetation);
        }
    }
    
    /// <summary>
    /// Gets the terrain height at the specified position
    /// </summary>
    private float GetTerrainHeightAt(Vector3 position)
    {
        // Simple implementation for now - just return 0
        // A more sophisticated implementation would use raycasting or terrain height sampling
        return 0f;
    }
    
    #region Mesh Generation Methods
    /// <summary>
    /// Generates a terrain mesh from a heightmap texture
    /// </summary>
    private Mesh GenerateTerrainMeshFromHeightmap(Texture2D heightmap)
    {
        Mesh mesh = new Mesh();
        
        int width = heightmap.width;
        int height = heightmap.height;
        
        // Create vertices
        Vector3[] vertices = new Vector3[(width + 1) * (height + 1)];
        for (int z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                // Sample heightmap
                float y = 0;
                if (x < width && z < height)
                {
                    y = heightmap.GetPixel(x, z).grayscale * settings.siteSize.y;
                }
                
                // Set vertex
                vertices[z * (width + 1) + x] = new Vector3(
                    x * settings.siteSize.x / width - settings.siteSize.x / 2,
                    y,
                    z * settings.siteSize.z / height - settings.siteSize.z / 2
                );
            }
        }
        
        // Create triangles
        int[] triangles = new int[width * height * 6];
        int index = 0;
        for (int z = 0; z < height; z++)
        {
            for (int x = 0; x < width; x++)
            {
                // First triangle
                triangles[index++] = z * (width + 1) + x;
                triangles[index++] = (z + 1) * (width + 1) + x;
                triangles[index++] = z * (width + 1) + x + 1;
                
                // Second triangle
                triangles[index++] = z * (width + 1) + x + 1;
                triangles[index++] = (z + 1) * (width + 1) + x;
                triangles[index++] = (z + 1) * (width + 1) + x + 1;
            }
        }
        
        // Create UVs
        Vector2[] uvs = new Vector2[(width + 1) * (height + 1)];
        for (int z = 0; z <= height; z++)
        {
            for (int x = 0; x <= width; x++)
            {
                uvs[z * (width + 1) + x] = new Vector2((float)x / width, (float)z / height);
            }
        }
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        // Calculate normals
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a procedural terrain mesh
    /// </summary>
    private Mesh GenerateProceduralTerrainMesh(float width, float depth, int resolution)
    {
        Mesh mesh = new Mesh();
        
        // Create a flat mesh with the given dimensions
        // For a proper terrain, you would use noise functions to generate height variation
        
        int xCount = resolution;
        int zCount = resolution;
        
        // Create vertices
        Vector3[] vertices = new Vector3[(xCount + 1) * (zCount + 1)];
        for (int z = 0; z <= zCount; z++)
        {
            for (int x = 0; x <= xCount; x++)
            {
                float xPos = x * width / xCount - width / 2;
                float zPos = z * depth / zCount - depth / 2;
                
                // Use Perlin noise for height
                float xCoord = (float)x / xCount * 3f;
                float zCoord = (float)z / zCount * 3f;
                float y = Mathf.PerlinNoise(xCoord, zCoord) * 10f; // Adjust multiplier for terrain height
                
                vertices[z * (xCount + 1) + x] = new Vector3(xPos, y, zPos);
            }
        }
        
        // Create triangles
        int[] triangles = new int[xCount * zCount * 6];
        int index = 0;
        for (int z = 0; z < zCount; z++)
        {
            for (int x = 0; x < xCount; x++)
            {
                // First triangle
                triangles[index++] = z * (xCount + 1) + x;
                triangles[index++] = (z + 1) * (xCount + 1) + x;
                triangles[index++] = z * (xCount + 1) + x + 1;
                
                // Second triangle
                triangles[index++] = z * (xCount + 1) + x + 1;
                triangles[index++] = (z + 1) * (xCount + 1) + x;
                triangles[index++] = (z + 1) * (xCount + 1) + x + 1;
            }
        }
        
        // Create UVs
        Vector2[] uvs = new Vector2[(xCount + 1) * (zCount + 1)];
        for (int z = 0; z <= zCount; z++)
        {
            for (int x = 0; x <= xCount; x++)
            {
                uvs[z * (xCount + 1) + x] = new Vector2((float)x / xCount, (float)z / zCount);
            }
        }
        
        // Assign to mesh
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        // Calculate normals
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a road mesh from a texture
    /// </summary>
    private Mesh GenerateRoadMeshFromTexture(Texture2D texture)
    {
        // For a real implementation, you would analyze the texture to extract road paths
        // Here we'll create a simple mesh slightly above the terrain
        
        Mesh mesh = new Mesh();
        
        // Create a simple quad for now
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-settings.siteSize.x / 2, 0.1f, -settings.siteSize.z / 2);
        vertices[1] = new Vector3(settings.siteSize.x / 2, 0.1f, -settings.siteSize.z / 2);
        vertices[2] = new Vector3(-settings.siteSize.x / 2, 0.1f, settings.siteSize.z / 2);
        vertices[3] = new Vector3(settings.siteSize.x / 2, 0.1f, settings.siteSize.z / 2);
        
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;
        
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a procedural road mesh
    /// </summary>
    private Mesh GenerateProceduralRoadMesh()
    {
        // For a real implementation, you would generate roads based on algorithms
        // Here we'll create a simple grid of roads
        
        Mesh mesh = new Mesh();
        
        // Create a list to hold all road vertices
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Create a grid of roads
        float roadWidth = 5f;
        float spacing = 50f;
        
        // Create north-south roads
        for (float x = -settings.siteSize.x / 2; x <= settings.siteSize.x / 2; x += spacing)
        {
            // Add a road segment
            int baseIndex = vertices.Count;
            
            vertices.Add(new Vector3(x - roadWidth / 2, 0.1f, -settings.siteSize.z / 2));
            vertices.Add(new Vector3(x + roadWidth / 2, 0.1f, -settings.siteSize.z / 2));
            vertices.Add(new Vector3(x - roadWidth / 2, 0.1f, settings.siteSize.z / 2));
            vertices.Add(new Vector3(x + roadWidth / 2, 0.1f, settings.siteSize.z / 2));
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
            
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
        }
        
        // Create east-west roads
        for (float z = -settings.siteSize.z / 2; z <= settings.siteSize.z / 2; z += spacing)
        {
            // Add a road segment
            int baseIndex = vertices.Count;
            
            vertices.Add(new Vector3(-settings.siteSize.x / 2, 0.1f, z - roadWidth / 2));
            vertices.Add(new Vector3(settings.siteSize.x / 2, 0.1f, z - roadWidth / 2));
            vertices.Add(new Vector3(-settings.siteSize.x / 2, 0.1f, z + roadWidth / 2));
            vertices.Add(new Vector3(settings.siteSize.x / 2, 0.1f, z + roadWidth / 2));
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
            
            uvs.Add(new Vector2(0, 0));
            uvs.Add(new Vector2(1, 0));
            uvs.Add(new Vector2(0, 1));
            uvs.Add(new Vector2(1, 1));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a road mesh from spline points
    /// </summary>
    private Mesh GenerateRoadMeshFromSpline(List<Vector3> splinePoints, float width)
    {
        Mesh mesh = new Mesh();
        
        if (splinePoints.Count < 2)
        {
            Debug.LogError("Not enough spline points to create a road segment");
            return mesh;
        }
        
        // Create vertices along the spline
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // For each segment of the spline
        for (int i = 0; i < splinePoints.Count - 1; i++)
        {
            Vector3 current = splinePoints[i];
            Vector3 next = splinePoints[i + 1];
            
            // Calculate perpendicular direction
            Vector3 forward = (next - current).normalized;
            Vector3 right = Vector3.Cross(Vector3.up, forward).normalized;
            
            // Add vertices for this segment
            int baseIndex = vertices.Count;
            
            vertices.Add(current + right * width / 2);
            vertices.Add(current - right * width / 2);
            vertices.Add(next + right * width / 2);
            vertices.Add(next - right * width / 2);
            
            // Add triangles
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
            
            // Add UVs
            float uvY = (float)i / (splinePoints.Count - 1);
            float uvYNext = (float)(i + 1) / (splinePoints.Count - 1);
            
            uvs.Add(new Vector2(0, uvY));
            uvs.Add(new Vector2(1, uvY));
            uvs.Add(new Vector2(0, uvYNext));
            uvs.Add(new Vector2(1, uvYNext));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a building mesh
    /// </summary>
    private Mesh GenerateBuildingMesh(float width, float height, float depth, int buildingType)
    {
        Mesh mesh = new Mesh();
        
        // Create a simple box mesh for the building
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        // Bottom vertices
        vertices.Add(new Vector3(-width / 2, 0, -depth / 2));
        vertices.Add(new Vector3(width / 2, 0, -depth / 2));
        vertices.Add(new Vector3(width / 2, 0, depth / 2));
        vertices.Add(new Vector3(-width / 2, 0, depth / 2));
        
        // Top vertices
        vertices.Add(new Vector3(-width / 2, height, -depth / 2));
        vertices.Add(new Vector3(width / 2, height, -depth / 2));
        vertices.Add(new Vector3(width / 2, height, depth / 2));
        vertices.Add(new Vector3(-width / 2, height, depth / 2));
        
        // Front face
        triangles.Add(0);
        triangles.Add(4);
        triangles.Add(1);
        triangles.Add(1);
        triangles.Add(4);
        triangles.Add(5);
        
        // Right face
        triangles.Add(1);
        triangles.Add(5);
        triangles.Add(2);
        triangles.Add(2);
        triangles.Add(5);
        triangles.Add(6);
        
        // Back face
        triangles.Add(2);
        triangles.Add(6);
        triangles.Add(3);
        triangles.Add(3);
        triangles.Add(6);
        triangles.Add(7);
        
        // Left face
        triangles.Add(3);
        triangles.Add(7);
        triangles.Add(0);
        triangles.Add(0);
        triangles.Add(7);
        triangles.Add(4);
        
        // Top face
        triangles.Add(4);
        triangles.Add(7);
        triangles.Add(5);
        triangles.Add(5);
        triangles.Add(7);
        triangles.Add(6);
        
        // Bottom face
        triangles.Add(0);
        triangles.Add(1);
        triangles.Add(3);
        triangles.Add(1);
        triangles.Add(2);
        triangles.Add(3);
        
        // Basic UVs
        for (int i = 0; i < 8; i++)
        {
            uvs.Add(new Vector2(i < 4 ? 0 : 1, i % 4 * 0.33f));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a water mesh
    /// </summary>
    private Mesh GenerateWaterMesh()
    {
        Mesh mesh = new Mesh();
        
        // Create a simple quad for water
        Vector3[] vertices = new Vector3[4];
        vertices[0] = new Vector3(-settings.siteSize.x / 2, -0.5f, -settings.siteSize.z / 2);
        vertices[1] = new Vector3(settings.siteSize.x / 2, -0.5f, -settings.siteSize.z / 2);
        vertices[2] = new Vector3(-settings.siteSize.x / 2, -0.5f, settings.siteSize.z / 2);
        vertices[3] = new Vector3(settings.siteSize.x / 2, -0.5f, settings.siteSize.z / 2);
        
        int[] triangles = new int[6];
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 2;
        triangles[4] = 3;
        triangles[5] = 1;
        
        Vector2[] uvs = new Vector2[4];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(10, 0);
        uvs[2] = new Vector2(0, 10);
        uvs[3] = new Vector2(10, 10);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a tree mesh
    /// </summary>
    private Mesh GenerateTreeMesh(float scale)
    {
        Mesh mesh = new Mesh();
        
        // Create a simple tree mesh with trunk and foliage
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        float trunkHeight = 2f * scale;
        float trunkRadius = 0.2f * scale;
        float crownRadius = 1.5f * scale;
        int segments = 8;
        
        // Trunk bottom circle
        vertices.Add(new Vector3(0, 0, 0)); // Center
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * trunkRadius, 0, Mathf.Sin(angle) * trunkRadius));
        }
        
        // Trunk top circle
        vertices.Add(new Vector3(0, trunkHeight, 0)); // Center
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * trunkRadius, trunkHeight, Mathf.Sin(angle) * trunkRadius));
        }
        
        // Foliage - cone
        vertices.Add(new Vector3(0, trunkHeight + crownRadius * 2, 0)); // Tip
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * crownRadius, trunkHeight, Mathf.Sin(angle) * crownRadius));
        }
        
        // Add triangles for trunk bottom
        for (int i = 0; i < segments; i++)
        {
            triangles.Add(0);
            triangles.Add(1 + (i + 1) % segments);
            triangles.Add(1 + i);
        }
        
        // Add triangles for trunk sides
        for (int i = 0; i < segments; i++)
        {
            int nextI = (i + 1) % segments;
            
            triangles.Add(1 + i);
            triangles.Add(1 + nextI);
            triangles.Add(segments + 1 + i);
            
            triangles.Add(segments + 1 + i);
            triangles.Add(1 + nextI);
            triangles.Add(segments + 1 + nextI);
        }
        
        // Add triangles for trunk top
        for (int i = 0; i < segments; i++)
        {
            triangles.Add(segments + 1);
            triangles.Add(segments + 1 + i);
            triangles.Add(segments + 1 + (i + 1) % segments);
        }
        
        // Add triangles for foliage
        int tipIndex = segments * 2 + 2;
        for (int i = 0; i < segments; i++)
        {
            triangles.Add(tipIndex);
            triangles.Add(tipIndex + 1 + (i + 1) % segments);
            triangles.Add(tipIndex + 1 + i);
        }
        
        // Simple UVs
        for (int i = 0; i < vertices.Count; i++)
        {
            uvs.Add(new Vector2(0, 0));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a bush mesh
    /// </summary>
    private Mesh GenerateBushMesh(float scale)
    {
        // Create a simple spherical bush
        GameObject tempSphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        Mesh sphereMesh = tempSphere.GetComponent<MeshFilter>().sharedMesh;
        Destroy(tempSphere);
        
        Mesh mesh = new Mesh();
        mesh.vertices = sphereMesh.vertices;
        mesh.triangles = sphereMesh.triangles;
        mesh.uv = sphereMesh.uv;
        mesh.normals = sphereMesh.normals;
        
        // Scale the vertices
        Vector3[] scaledVertices = new Vector3[mesh.vertices.Length];
        for (int i = 0; i < mesh.vertices.Length; i++)
        {
            scaledVertices[i] = mesh.vertices[i] * scale;
        }
        mesh.vertices = scaledVertices;
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a flower mesh
    /// </summary>
    private Mesh GenerateFlowerMesh(float scale)
    {
        // Create a simple cylindrical stem with a disc on top
        Mesh mesh = new Mesh();
        
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();
        
        float stemHeight = 0.5f * scale;
        float stemRadius = 0.05f * scale;
        float flowerRadius = 0.2f * scale;
        int segments = 8;
        
        // Stem bottom
        vertices.Add(new Vector3(0, 0, 0));
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * stemRadius, 0, Mathf.Sin(angle) * stemRadius));
        }
        
        // Stem top
        vertices.Add(new Vector3(0, stemHeight, 0));
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * stemRadius, stemHeight, Mathf.Sin(angle) * stemRadius));
        }
        
        // Flower top
        vertices.Add(new Vector3(0, stemHeight, 0));
        for (int i = 0; i < segments; i++)
        {
            float angle = i * Mathf.PI * 2f / segments;
            vertices.Add(new Vector3(Mathf.Cos(angle) * flowerRadius, stemHeight, Mathf.Sin(angle) * flowerRadius));
        }
        
        // Add triangles for stem bottom
        for (int i = 0; i < segments; i++)
        {
            triangles.Add(0);
            triangles.Add(1 + (i + 1) % segments);
            triangles.Add(1 + i);
        }
        
        // Add triangles for stem sides
        for (int i = 0; i < segments; i++)
        {
            int nextI = (i + 1) % segments;
            
            triangles.Add(1 + i);
            triangles.Add(1 + nextI);
            triangles.Add(segments + 1 + i);
            
            triangles.Add(segments + 1 + i);
            triangles.Add(1 + nextI);
            triangles.Add(segments + 1 + nextI);
        }
        
        // Add triangles for flower
        int flowerCenterIndex = segments * 2 + 2;
        for (int i = 0; i < segments; i++)
        {
            triangles.Add(flowerCenterIndex);
            triangles.Add(flowerCenterIndex + 1 + i);
            triangles.Add(flowerCenterIndex + 1 + (i + 1) % segments);
        }
        
        // Simple UVs
        for (int i = 0; i < vertices.Count; i++)
        {
            uvs.Add(new Vector2(0, 0));
        }
        
        mesh.vertices = vertices.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.uv = uvs.ToArray();
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    
    /// <summary>
    /// Generates a generic vegetation mesh
    /// </summary>
    private Mesh GenerateGenericVegetationMesh(float scale)
    {
        // Create crossed quads for billboard vegetation
        Mesh mesh = new Mesh();
        
        Vector3[] vertices = new Vector3[8];
        
        // First quad
        vertices[0] = new Vector3(-0.5f, 0, 0) * scale;
        vertices[1] = new Vector3(0.5f, 0, 0) * scale;
        vertices[2] = new Vector3(-0.5f, 1, 0) * scale;
        vertices[3] = new Vector3(0.5f, 1, 0) * scale;
        
        // Second quad
        vertices[4] = new Vector3(0, 0, -0.5f) * scale;
        vertices[5] = new Vector3(0, 0, 0.5f) * scale;
        vertices[6] = new Vector3(0, 1, -0.5f) * scale;
        vertices[7] = new Vector3(0, 1, 0.5f) * scale;
        
        int[] triangles = new int[12];
        
        // First quad
        triangles[0] = 0;
        triangles[1] = 2;
        triangles[2] = 1;
        triangles[3] = 1;
        triangles[4] = 2;
        triangles[5] = 3;
        
        // Second quad
        triangles[6] = 4;
        triangles[7] = 6;
        triangles[8] = 5;
        triangles[9] = 5;
        triangles[10] = 6;
        triangles[11] = 7;
        
        Vector2[] uvs = new Vector2[8];
        uvs[0] = new Vector2(0, 0);
        uvs[1] = new Vector2(1, 0);
        uvs[2] = new Vector2(0, 1);
        uvs[3] = new Vector2(1, 1);
        
        uvs[4] = new Vector2(0, 0);
        uvs[5] = new Vector2(1, 0);
        uvs[6] = new Vector2(0, 1);
        uvs[7] = new Vector2(1, 1);
        
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        
        mesh.RecalculateNormals();
        
        return mesh;
    }
    #endregion
    
    #region Material Creation Methods
    /// <summary>
    /// Creates a basic terrain material
    /// </summary>
    private Material CreateBasicTerrainMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.5f, 0.5f, 0.3f);
        return material;
    }
    
    /// <summary>
    /// Creates a basic building material
    /// </summary>
    private Material CreateBasicBuildingMaterial(int buildingType)
    {
        Material material = new Material(Shader.Find("Standard"));
        
        // Color based on building type
        switch (buildingType)
        {
            case 0: // Commercial
                material.color = new Color(0.7f, 0.7f, 0.8f);
                break;
                
            case 1: // Residential
                material.color = new Color(0.8f, 0.7f, 0.6f);
                break;
                
            case 2: // Government
                material.color = new Color(0.6f, 0.7f, 0.7f);
                break;
                
            default:
                material.color = new Color(0.7f, 0.7f, 0.7f);
                break;
        }
        
        return material;
    }
    
    /// <summary>
    /// Creates a basic road material
    /// </summary>
    private Material CreateBasicRoadMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.2f, 0.2f, 0.2f);
        return material;
    }
    
    /// <summary>
    /// Creates a basic water material
    /// </summary>
    private Material CreateBasicWaterMaterial()
    {
        Material material = new Material(Shader.Find("Standard"));
        material.color = new Color(0.2f, 0.5f, 0.8f, 0.7f);
        return material;
    }
    
    /// <summary>
    /// Creates a vegetation material
    /// </summary>
    private Material CreateVegetationMaterial(string vegetationType)
    {
        Material material = new Material(Shader.Find("Standard"));
        
        switch (vegetationType)
        {
            case "Tree":
                material.color = new Color(0.1f, 0.5f, 0.1f);
                break;
                
            case "Bush":
                material.color = new Color(0.2f, 0.6f, 0.2f);
                break;
                
            case "Flower":
                material.color = new Color(0.9f, 0.5f, 0.5f);
                break;
                
            default:
                material.color = new Color(0.3f, 0.6f, 0.3f);
                break;
        }
        
        return material;
    }
    #endregion
    #endregion
}

/// <summary>
/// Data class for Wellington Lambton Quay
/// </summary>
public class LambtonQuayData
{
    public List<BuildingData> buildings = new List<BuildingData>();
    public List<RoadData> roads = new List<RoadData>();
    public List<VegetationData> vegetation = new List<VegetationData>();
    
    public LambtonQuayData()
    {
        // Initialize with some example data for Lambton Quay
        InitializeBuildings();
        InitializeRoads();
        InitializeVegetation();
    }
    
    private void InitializeBuildings()
    {
        // Add some example buildings based on Lambton Quay
        // In a real implementation, this would load from a data file or API
        
        buildings.Add(new BuildingData
        {
            id = 1,
            name = "Lambton Quay Plaza",
            buildingType = 0, // Commercial
            position = new Vector3(0, 0, 0),
            rotation = 0,
            size = new Vector3(30, 50, 20)
        });
        
        buildings.Add(new BuildingData
        {
            id = 2,
            name = "Government Building",
            buildingType = 2, // Government
            position = new Vector3(50, 0, 30),
            rotation = 15,
            size = new Vector3(40, 30, 40)
        });
        
        buildings.Add(new BuildingData
        {
            id = 3,
            name = "Apartment Complex",
            buildingType = 1, // Residential
            position = new Vector3(-40, 0, 20),
            rotation = 0,
            size = new Vector3(25, 40, 25)
        });
        
        // Add more buildings...
        for (int i = 4; i <= 20; i++)
        {
            buildings.Add(new BuildingData
            {
                id = i,
                name = "Building " + i,
                buildingType = Random.Range(0, 3),
                position = new Vector3(
                    Random.Range(-200, 200),
                    0,
                    Random.Range(-200, 200)
                ),
                rotation = Random.Range(0, 360),
                size = new Vector3(
                    Random.Range(10, 50),
                    Random.Range(10, 100),
                    Random.Range(10, 50)
                )
            });
        }
    }
    
    private void InitializeRoads()
    {
        // Add main Lambton Quay road
        RoadData lambtonQuay = new RoadData
        {
            id = 1,
            name = "Lambton Quay",
            width = 10,
            splinePoints = new List<Vector3>()
        };
        
        // Add points to define the road path
        lambtonQuay.splinePoints.Add(new Vector3(-200, 0, -100));
        lambtonQuay.splinePoints.Add(new Vector3(-100, 0, -50));
        lambtonQuay.splinePoints.Add(new Vector3(0, 0, 0));
        lambtonQuay.splinePoints.Add(new Vector3(100, 0, 50));
        lambtonQuay.splinePoints.Add(new Vector3(200, 0, 100));
        
        roads.Add(lambtonQuay);
        
        // Add crossing streets
        for (int i = 0; i < 5; i++)
        {
            RoadData crossStreet = new RoadData
            {
                id = i + 2,
                name = "Cross Street " + (i + 1),
                width = 8,
                splinePoints = new List<Vector3>()
            };
            
            float pos = -200 + i * 100;
            crossStreet.splinePoints.Add(new Vector3(pos, 0, -200));
            crossStreet.splinePoints.Add(new Vector3(pos, 0, -100));
            crossStreet.splinePoints.Add(new Vector3(pos, 0, 0));
            crossStreet.splinePoints.Add(new Vector3(pos, 0, 100));
            crossStreet.splinePoints.Add(new Vector3(pos, 0, 200));
            
            roads.Add(crossStreet);
        }
    }
    
    private void InitializeVegetation()
    {
        // Add trees along Lambton Quay
        for (int i = 0; i < 20; i++)
        {
            float t = i / 19f; // Normalized position along the road
            
            vegetation.Add(new VegetationData
            {
                id = i + 1,
                type = "Tree",
                vegetationType = Random.Range(0, 2),
                position = new Vector3(
                    -200 + t * 400 + Random.Range(-5, 5),
                    0,
                    -100 + t * 200 + Random.Range(-5, 5)
                ),
                rotation = Random.Range(0, 360),
                scale = Random.Range(0.8f, 1.2f)
            });
        }
        
        // Add bushes and flowers
        for (int i = 0; i < 15; i++)
        {
            vegetation.Add(new VegetationData
            {
                id = i + 21,
                type = "Bush",
                vegetationType = 0,
                position = new Vector3(
                    Random.Range(-200, 200),
                    0,
                    Random.Range(-200, 200)
                ),
                rotation = Random.Range(0, 360),
                scale = Random.Range(0.5f, 1.0f)
            });
            
            vegetation.Add(new VegetationData
            {
                id = i + 36,
                type = "Flower",
                vegetationType = 0,
                position = new Vector3(
                    Random.Range(-200, 200),
                    0,
                    Random.Range(-200, 200)
                ),
                rotation = Random.Range(0, 360),
                scale = Random.Range(0.3f, 0.6f)
            });
        }
    }
}

/// <summary>
/// Data structure for a building
/// </summary>
public class BuildingData
{
    public int id;
    public string name;
    public int buildingType; // 0=Commercial, 1=Residential, 2=Government
    public Vector3 position;
    public float rotation;
    public Vector3 size;
}

/// <summary>
/// Data structure for a road
/// </summary>
public class RoadData
{
    public int id;
    public string name;
    public float width;
    public List<Vector3> splinePoints;
}

/// <summary>
/// Data structure for vegetation
/// </summary>
public class VegetationData
{
    public int id;
    public string type; // Tree, Bush, Flower
    public int vegetationType;
    public Vector3 position;
    public float rotation;
    public float scale;
}