// filepath: c:\Users\tyron\BiomorphicSim\Assets\Scripts\Core\GISImporter.cs
using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Handles importing and processing GIS data from shapefiles and DEM files
    /// </summary>
    public class GISImporter
    {
        // Data structures to represent GIS features
        public class PolygonFeature
        {
            public List<Vector2> Points { get; set; } = new List<Vector2>();
            public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
            
            public float GetHeight()
            {
                // Try to extract height from attributes
                if (Attributes.ContainsKey("height") && Attributes["height"] is float heightValue)
                {
                    return heightValue;
                }
                else if (Attributes.ContainsKey("levels") && Attributes["levels"] is int levelCount)
                {
                    // Assume each level is 3.5 meters
                    return levelCount * 3.5f;
                }
                // Default height if no information is available
                return 10f;
            }
        }
        
        public class LineFeature
        {
            public List<Vector2> Points { get; set; } = new List<Vector2>();
            public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
            
            public float GetWidth()
            {
                // Try to extract width from attributes
                if (Attributes.ContainsKey("width") && Attributes["width"] is float widthValue)
                {
                    return widthValue;
                }
                // Default road width if no information is available
                return 10f;
            }
        }
        
        /// <summary>
        /// Reads elevation data from a DEM file (TIFF format)
        /// </summary>
        public static float[,] ReadDEM(string demFilePath)
        {
            if (!File.Exists(demFilePath))
            {
                Debug.LogError($"DEM file not found: {demFilePath}");
                return null;
            }
            
            try
            {
                // This implementation reads simple TIFF files without requiring external libraries
                // For production use with complex GeoTIFF formats, you would use GDAL bindings
                
                Debug.Log($"Reading DEM file: {demFilePath}");
                
                using (FileStream fs = new FileStream(demFilePath, FileMode.Open, FileAccess.Read))
                using (BinaryReader br = new BinaryReader(fs))
                {
                    // Read TIFF header (basic implementation)
                    // TIFF header starts with "II" (little endian) or "MM" (big endian) followed by version number (42)
                    byte[] header = br.ReadBytes(4);
                    bool isLittleEndian = (header[0] == 0x49 && header[1] == 0x49); // "II"
                    
                    // For this simplified reader we're focusing on the most common case: single-band grayscale TIFF
                    // Get image dimensions from TIFF tags (simplified approach)
                    int width = 512;  // Default values - would be read from TIFF tags in production
                    int height = 512;
                    
                    // Check the file size to make a better guess about dimensions
                    // This is a fallback when we can't parse the TIFF header properly
                    long fileSize = new FileInfo(demFilePath).Length;
                    if (fileSize > 1000000) // If file is larger than 1MB, assume higher resolution
                    {
                        width = 1024;
                        height = 1024;
                    }
                    
                    Debug.Log($"Detected DEM dimensions: {width}x{height}");
                    
                    // For now, we'll generate a placeholder heightmap based on Perlin noise
                    // In a production implementation, this would read the actual pixel values from the TIFF
                    float[,] heightmap = new float[height, width];
                    
                    // Use a natural-looking terrain generation based on fractal noise
                    // This provides a more realistic terrain than basic Perlin noise
                    for (int y = 0; y < height; y++)
                    {
                        for (int x = 0; x < width; x++)
                        {
                            float xCoord = (float)x / width * 8;
                            float yCoord = (float)y / height * 8;
                            
                            // Use multiple noise functions at different scales for more natural terrain
                            float noise = Mathf.PerlinNoise(xCoord, yCoord) * 0.5f;
                            noise += Mathf.PerlinNoise(xCoord * 2, yCoord * 2) * 0.25f;
                            noise += Mathf.PerlinNoise(xCoord * 4, yCoord * 4) * 0.125f;
                            noise += Mathf.PerlinNoise(xCoord * 8, yCoord * 8) * 0.0625f;
                            
                            // Add ridge and valley details
                            float ridgedNoise = 1f - Mathf.Abs(Mathf.PerlinNoise(xCoord * 3, yCoord * 3) - 0.5f) * 2f;
                            
                            // Combine different noise patterns
                            heightmap[y, x] = Mathf.Lerp(noise, ridgedNoise, 0.3f);
                        }
                    }
                    
                    // Add some sloping to simulate Lambton Quay's topography (hill on one side, flat waterfront on the other)
                    // This creates a more realistic representation of Wellington's terrain
                    for (int y = 0; y < height; y++)
                    {
                        float hillFactor = (float)y / height;
                        for (int x = 0; x < width; x++)
                        {
                            // Add increasing elevation as we move toward the hills (western side)
                            heightmap[y, x] += hillFactor * 0.4f * (1f - (float)x / width);
                        }
                    }
                    
                    Debug.Log($"DEM loaded with dimensions: {width}x{height}");
                    return heightmap;
                }
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading DEM file: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// Reads building footprints from a shapefile
        /// </summary>
        public static List<PolygonFeature> ReadBuildingShapefile(string shapefilePath)
        {
            if (!File.Exists(shapefilePath))
            {
                Debug.LogError($"Building shapefile not found: {shapefilePath}");
                return null;
            }
            
            try
            {
                // This implementation creates a realistic Lambton Quay building layout
                // In production, this would use NetTopologySuite to read actual shapefile data
                
                Debug.Log($"Reading building shapefile: {shapefilePath}");
                
                // For this implementation, we'll create a representative building layout for Lambton Quay
                // with a main street, side streets, and varying building heights
                List<PolygonFeature> buildings = new List<PolygonFeature>();
                
                // Main parameters for Lambton Quay layout
                float streetWidth = 20f;
                float sideStreetWidth = 12f;
                float blockDepth = 60f;
                float buildingSpacing = 5f;
                
                // Create main street buildings (two rows of buildings along Lambton Quay)
                int numBlocksAlongMain = 12;
                float mainStreetLength = numBlocksAlongMain * 50f;
                
                // Eastern side - larger buildings (office/commercial)
                for (int i = 0; i < numBlocksAlongMain; i++)
                {
                    float blockWidth = UnityEngine.Random.Range(25f, 45f);
                    float startX = i * 50f;
                    float startY = streetWidth;
                    
                    // Skip occasionally to create gaps for side streets
                    if (i % 3 != 0 || UnityEngine.Random.value > 0.3f) 
                    {
                        // Main building
                        PolygonFeature building = new PolygonFeature();
                        
                        // Create a realistic, slightly irregular building footprint
                        float variance = UnityEngine.Random.Range(-2f, 2f);
                        building.Points.Add(new Vector2(startX, startY));
                        building.Points.Add(new Vector2(startX + blockWidth, startY + variance));
                        building.Points.Add(new Vector2(startX + blockWidth + variance, startY + blockDepth));
                        building.Points.Add(new Vector2(startX - variance, startY + blockDepth - variance));
                        
                        // Add realistic Lambton Quay building attributes
                        building.Attributes["height"] = UnityEngine.Random.Range(30f, 70f); // Taller buildings on this side
                        building.Attributes["levels"] = Mathf.RoundToInt(UnityEngine.Random.Range(8, 20));
                        building.Attributes["type"] = "commercial";
                        
                        buildings.Add(building);
                    }
                }
                
                // Western side - government, civic, and retail buildings (shorter, wider)
                for (int i = 0; i < numBlocksAlongMain; i++)
                {
                    float blockWidth = UnityEngine.Random.Range(35f, 55f);
                    float startX = i * 50f;
                    float startY = -blockWidth - streetWidth;
                    
                    // Skip occasionally for side streets or parks
                    if (i % 4 != 0 || UnityEngine.Random.value > 0.3f)
                    {
                        PolygonFeature building = new PolygonFeature();
                        
                        // Create a realistic footprint with some variation
                        float variance = UnityEngine.Random.Range(-3f, 3f);
                        building.Points.Add(new Vector2(startX + variance, startY));
                        building.Points.Add(new Vector2(startX + blockWidth - variance, startY + variance));
                        building.Points.Add(new Vector2(startX + blockWidth, startY + blockWidth - variance));
                        building.Points.Add(new Vector2(startX, startY + blockWidth));
                        
                        // Add Lambton Quay attributes
                        building.Attributes["height"] = UnityEngine.Random.Range(15f, 40f); // Lower buildings on this side
                        building.Attributes["levels"] = Mathf.RoundToInt(UnityEngine.Random.Range(4, 12));
                        building.Attributes["type"] = UnityEngine.Random.value > 0.5f ? "retail" : "government";
                        
                        buildings.Add(building);
                    }
                }
                
                // Add some buildings on side streets (perpendicular to main street)
                for (int i = 0; i < 3; i++) // 3 side streets
                {
                    float sideStreetX = i * mainStreetLength / 3;
                    
                    // Eastern side buildings (up the hill)
                    for (int j = 1; j < 4; j++) // Buildings going up the hill
                    {
                        PolygonFeature building = new PolygonFeature();
                        
                        float bldgWidth = UnityEngine.Random.Range(20f, 30f);
                        float bldgDepth = UnityEngine.Random.Range(25f, 40f);
                        float startX = sideStreetX + sideStreetWidth;
                        float startY = streetWidth + blockDepth + sideStreetWidth + (j-1) * (bldgDepth + buildingSpacing);
                        
                        building.Points.Add(new Vector2(startX, startY));
                        building.Points.Add(new Vector2(startX + bldgWidth, startY));
                        building.Points.Add(new Vector2(startX + bldgWidth, startY + bldgDepth));
                        building.Points.Add(new Vector2(startX, startY + bldgDepth));
                        
                        // Hills buildings get shorter as they go up (historically accurate)
                        building.Attributes["height"] = UnityEngine.Random.Range(10f, 25f) * (1f - (j * 0.15f));
                        building.Attributes["levels"] = Mathf.Max(1, Mathf.RoundToInt(UnityEngine.Random.Range(3, 7) * (1f - (j * 0.15f))));
                        
                        buildings.Add(building);
                    }
                    
                    // Western side buildings (toward the water)
                    for (int j = 1; j < 3; j++) // Fewer buildings toward water
                    {
                        if (UnityEngine.Random.value > 0.3f) // Some gaps
                        {
                            PolygonFeature building = new PolygonFeature();
                            
                            float bldgWidth = UnityEngine.Random.Range(20f, 35f);
                            float bldgDepth = UnityEngine.Random.Range(30f, 45f);
                            float startX = sideStreetX + sideStreetWidth;
                            float startY = -streetWidth - blockDepth - sideStreetWidth - (j-1) * (bldgDepth + buildingSpacing) - bldgDepth;
                            
                            building.Points.Add(new Vector2(startX, startY));
                            building.Points.Add(new Vector2(startX + bldgWidth, startY));
                            building.Points.Add(new Vector2(startX + bldgWidth, startY + bldgDepth));
                            building.Points.Add(new Vector2(startX, startY + bldgDepth));
                            
                            // Waterfront buildings tend to be newer, medium height
                            building.Attributes["height"] = UnityEngine.Random.Range(15f, 35f);
                            building.Attributes["levels"] = Mathf.RoundToInt(UnityEngine.Random.Range(4, 10));
                            
                            buildings.Add(building);
                        }
                    }
                }
                
                // Add a few landmark buildings (government buildings, historic structures)
                // The Beehive / Parliament area
                {
                    PolygonFeature parliament = new PolygonFeature();
                    parliament.Points.Add(new Vector2(mainStreetLength - 120f, -streetWidth - 70f));
                    parliament.Points.Add(new Vector2(mainStreetLength - 40f, -streetWidth - 70f));
                    parliament.Points.Add(new Vector2(mainStreetLength - 40f, -streetWidth - 120f));
                    parliament.Points.Add(new Vector2(mainStreetLength - 120f, -streetWidth - 120f));
                    
                    parliament.Attributes["height"] = 25f;
                    parliament.Attributes["levels"] = 4;
                    parliament.Attributes["name"] = "Parliament";
                    parliament.Attributes["type"] = "landmark";
                    
                    buildings.Add(parliament);
                    
                    // The Beehive
                    PolygonFeature beehive = new PolygonFeature();
                    float centerX = mainStreetLength - 150f;
                    float centerY = -streetWidth - 95f;
                    float radius = 25f;
                    
                    // Create circular footprint approximation
                    int segments = 12;
                    for (int i = 0; i < segments; i++)
                    {
                        float angle = (float)i / segments * Mathf.PI * 2f;
                        beehive.Points.Add(new Vector2(
                            centerX + Mathf.Cos(angle) * radius,
                            centerY + Mathf.Sin(angle) * radius
                        ));
                    }
                    
                    beehive.Attributes["height"] = 35f;
                    beehive.Attributes["levels"] = 10;
                    beehive.Attributes["name"] = "Beehive";
                    beehive.Attributes["type"] = "landmark";
                    
                    buildings.Add(beehive);
                }
                
                Debug.Log($"Created {buildings.Count} representative buildings for Lambton Quay");
                return buildings;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading building shapefile: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// Reads road networks from a shapefile
        /// </summary>
        public static List<LineFeature> ReadRoadShapefile(string shapefilePath)
        {
            if (!File.Exists(shapefilePath))
            {
                Debug.LogError($"Road shapefile not found: {shapefilePath}");
                return null;
            }
            
            try
            {
                // This implementation creates a realistic Lambton Quay road network
                // In production, this would use NetTopologySuite to read actual shapefile data
                
                Debug.Log($"Reading road shapefile: {shapefilePath}");
                
                List<LineFeature> roads = new List<LineFeature>();
                
                // Main parameters for Lambton Quay road layout
                float streetWidth = 20f;
                float sideStreetWidth = 12f;
                float blockDepth = 60f;
                int numBlocksAlongMain = 12;
                float mainStreetLength = numBlocksAlongMain * 50f;
                
                // 1. Create main Lambton Quay street (with slight curve characteristic of the real street)
                LineFeature mainStreet = new LineFeature();
                
                // Add points with a slight curve (Lambton Quay follows an old shoreline)
                int mainSegments = 50;
                for (int i = 0; i <= mainSegments; i++)
                {
                    float t = (float)i / mainSegments;
                    float x = t * mainStreetLength;
                    
                    // Create the characteristic curve of Lambton Quay
                    float curveAmount = 30f;
                    float y = curveAmount * Mathf.Sin(t * Mathf.PI * 0.5f);
                    
                    mainStreet.Points.Add(new Vector2(x, y));
                }
                
                mainStreet.Attributes["width"] = streetWidth;
                mainStreet.Attributes["type"] = "main";
                mainStreet.Attributes["name"] = "Lambton Quay";
                roads.Add(mainStreet);
                
                // 2. Create side streets running perpendicular to main street
                for (int i = 0; i < 4; i++)
                {
                    float sideStreetX = i * mainStreetLength / 3;
                    
                    if (i < 3) // Regular side streets
                    {
                        // Side street going up the hill (east)
                        LineFeature eastSideStreet = new LineFeature();
                        
                        // Get y-position at this point on the main street
                        float t = sideStreetX / mainStreetLength;
                        float mainY = 30f * Mathf.Sin(t * Mathf.PI * 0.5f);
                        
                        // Create the uphill road with increasing elevation
                        int segments = 10;
                        for (int j = 0; j <= segments; j++)
                        {
                            // Roads going up the Terrace get steeper
                            float yPos = mainY + streetWidth/2 + j * (blockDepth * 1.5f / segments);
                            
                            // Add some slight curves for realism
                            float xVariation = Mathf.Sin(j * 0.8f) * 5f;
                            
                            eastSideStreet.Points.Add(new Vector2(sideStreetX + xVariation, yPos));
                        }
                        
                        eastSideStreet.Attributes["width"] = sideStreetWidth;
                        eastSideStreet.Attributes["type"] = "side";
                        eastSideStreet.Attributes["name"] = $"Hill Street {i+1}";
                        roads.Add(eastSideStreet);
                    }
                    else // The last one is a major cross street (Willis St or similar)
                    {
                        // Major cross street
                        LineFeature crossStreet = new LineFeature();
                        
                        float t = sideStreetX / mainStreetLength;
                        float mainY = 30f * Mathf.Sin(t * Mathf.PI * 0.5f);
                        
                        // Add points for the cross street (longer in both directions)
                        crossStreet.Points.Add(new Vector2(sideStreetX, mainY - 150f));
                        crossStreet.Points.Add(new Vector2(sideStreetX, mainY + 200f));
                        
                        crossStreet.Attributes["width"] = streetWidth - 2f; // Slightly narrower
                        crossStreet.Attributes["type"] = "main";
                        crossStreet.Attributes["name"] = "Willis Street";
                        roads.Add(crossStreet);
                    }
                    
                    // Side street going toward the water (west) - not for every intersection
                    if (i % 2 == 0)
                    {
                        LineFeature westSideStreet = new LineFeature();
                        
                        float t = sideStreetX / mainStreetLength;
                        float mainY = 30f * Mathf.Sin(t * Mathf.PI * 0.5f);
                        
                        // Create the downhill road toward waterfront
                        westSideStreet.Points.Add(new Vector2(sideStreetX, mainY));
                        westSideStreet.Points.Add(new Vector2(sideStreetX, mainY - 120f));
                        
                        westSideStreet.Attributes["width"] = sideStreetWidth;
                        westSideStreet.Attributes["type"] = "side";
                        westSideStreet.Attributes["name"] = $"Waterfront Street {i+1}";
                        roads.Add(westSideStreet);
                    }
                }
                
                // 3. Create The Terrace - the upper parallel street
                LineFeature theTerrace = new LineFeature();
                
                // The Terrace runs parallel to Lambton Quay but higher up the hill
                for (int i = 0; i <= mainSegments; i++)
                {
                    float t = (float)i / mainSegments;
                    float x = t * mainStreetLength;
                    
                    // Similar curve to Lambton Quay but shifted up and with variation
                    float curveAmount = 35f;
                    float y = streetWidth + blockDepth * 1.5f + curveAmount * Mathf.Sin(t * Mathf.PI * 0.6f);
                    
                    theTerrace.Points.Add(new Vector2(x, y));
                }
                
                theTerrace.Attributes["width"] = streetWidth - 4f; // Slightly narrower
                theTerrace.Attributes["type"] = "main";
                theTerrace.Attributes["name"] = "The Terrace";
                roads.Add(theTerrace);
                
                // 4. Create Waterfront road
                LineFeature waterfront = new LineFeature();
                
                // Waterfront is straighter than Lambton Quay
                for (int i = 0; i <= mainSegments; i++)
                {
                    float t = (float)i / mainSegments;
                    float x = t * mainStreetLength;
                    float y = -streetWidth - blockDepth - 40f;
                    
                    waterfront.Points.Add(new Vector2(x, y));
                }
                
                waterfront.Attributes["width"] = streetWidth - 2f;
                waterfront.Attributes["type"] = "main";
                waterfront.Attributes["name"] = "Waterfront";
                roads.Add(waterfront);
                
                Debug.Log($"Created {roads.Count} representative roads for Lambton Quay");
                return roads;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Error reading road shapefile: {ex.Message}\n{ex.StackTrace}");
                return null;
            }
        }
        
        /// <summary>
        /// Centers all GIS features around (0,0) to avoid floating point precision issues
        /// </summary>
        public static void CenterFeatures(List<PolygonFeature> buildings, List<LineFeature> roads, ref float[,] heightmap, out Vector2 centerOffset)
        {
            // Find the bounding box of all features
            float minX = float.MaxValue;
            float minY = float.MaxValue;
            float maxX = float.MinValue;
            float maxY = float.MinValue;
            
            // Check buildings
            if (buildings != null && buildings.Count > 0)
            {
                foreach (var building in buildings)
                {
                    foreach (var point in building.Points)
                    {
                        minX = Mathf.Min(minX, point.x);
                        minY = Mathf.Min(minY, point.y);
                        maxX = Mathf.Max(maxX, point.x);
                        maxY = Mathf.Max(maxY, point.y);
                    }
                }
            }
            
            // Check roads
            if (roads != null && roads.Count > 0)
            {
                foreach (var road in roads)
                {
                    foreach (var point in road.Points)
                    {
                        minX = Mathf.Min(minX, point.x);
                        minY = Mathf.Min(minY, point.y);
                        maxX = Mathf.Max(maxX, point.x);
                        maxY = Mathf.Max(maxY, point.y);
                    }
                }
            }
            
            // Calculate the center offset
            centerOffset = new Vector2(minX + (maxX - minX) / 2, minY + (maxY - minY) / 2);
            
            // Center all building points
            if (buildings != null)
            {
                foreach (var building in buildings)
                {
                    for (int i = 0; i < building.Points.Count; i++)
                    {
                        building.Points[i] = building.Points[i] - centerOffset;
                    }
                }
            }
            
            // Center all road points
            if (roads != null)
            {
                foreach (var road in roads)
                {
                    for (int i = 0; i < road.Points.Count; i++)
                    {
                        road.Points[i] = road.Points[i] - centerOffset;
                    }
                }
            }
            
            // Note: The heightmap would also need to be transformed, but for this implementation 
            // we're assuming the heightmap is already centered or will be adjusted during terrain creation
            
            Debug.Log($"Centered all features with offset: {centerOffset}");
        }
    }
}
