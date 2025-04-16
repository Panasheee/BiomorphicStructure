using UnityEngine;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Helper class to create Wellington buildings on the terrain
    /// </summary>
    public class WellingtonBuildingsCreator : MonoBehaviour
    {
        public static void CreateWellingtonBuildings(Transform parent, Terrain terrain)
        {
            Debug.Log("Creating Wellington buildings mockup for Lambton Quay area...");
            
            // Create a parent object for all buildings
            GameObject buildingsContainer = new GameObject("WellingtonBuildings");
            buildingsContainer.transform.SetParent(parent);
            
            // Create materials for different building types
            Material officeMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            officeMaterial.color = new Color(0.7f, 0.7f, 0.8f); // Light blue-gray for office buildings
            
            Material governmentMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            governmentMaterial.color = new Color(0.8f, 0.8f, 0.7f); // Light beige for government buildings
            
            Material retailMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            retailMaterial.color = new Color(0.8f, 0.7f, 0.7f); // Light pink-ish for retail
            
            // Create main Lambton Quay buildings (simplified representations)
            // These are approximate positions based on the area
            
            // The Terrace along the western side (multiple office buildings)
            for (int i = 0; i < 8; i++)
            {
                float xPos = 400 + Random.Range(-20f, 20f);
                float zPos = 100 + i * 60 + Random.Range(-10f, 10f);
                float height = Random.Range(30f, 60f);
                float width = Random.Range(20f, 40f);
                float depth = Random.Range(20f, 40f);
                
                CreateBuilding(buildingsContainer.transform, terrain,
                    new Vector3(xPos, 0, zPos), 
                    new Vector3(width, height, depth), 
                    officeMaterial, 
                    $"TerraceBuilding_{i}");
            }
            
            // Lambton Quay main street buildings (government and retail)
            for (int i = 0; i < 12; i++)
            {
                float xPos = 300 + Random.Range(-20f, 20f);
                float zPos = 80 + i * 40 + Random.Range(-5f, 5f);
                float height = Random.Range(15f, 40f);
                float width = Random.Range(25f, 50f);
                float depth = Random.Range(25f, 50f);
                
                Material buildingMat = i % 3 == 0 ? governmentMaterial : retailMaterial;
                
                CreateBuilding(buildingsContainer.transform, terrain,
                    new Vector3(xPos, 0, zPos), 
                    new Vector3(width, height, depth), 
                    buildingMat, 
                    $"LambtonQuayBuilding_{i}");
            }
            
            // Beehive (Parliament) - distinctive government building
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(350, 0, 500),
                new Vector3(80, 25, 60),
                governmentMaterial,
                "Parliament");
                
            // Old Government Buildings
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(320, 0, 450),
                new Vector3(70, 20, 70),
                governmentMaterial,
                "OldGovernmentBuildings");
                
            // TSB Arena / Shed 6 near waterfront
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(180, 0, 300),
                new Vector3(100, 15, 60),
                retailMaterial,
                "TSB_Arena");
                
            // Wellington Railway Station
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(250, 0, 520),
                new Vector3(120, 18, 60),
                governmentMaterial,
                "RailwayStation");
                
            // Add some landmark buildings
            // Michael Fowler Centre
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(200, 0, 400),
                new Vector3(50, 25, 50),
                retailMaterial,
                "MichaelFowlerCentre");
                
            // Create some waterfront features
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(150, 0, 250),
                new Vector3(30, 10, 30),
                retailMaterial,
                "WaterfrontBuilding_1");
                
            CreateBuilding(buildingsContainer.transform, terrain,
                new Vector3(120, 0, 350),
                new Vector3(25, 8, 25),
                retailMaterial,
                "WaterfrontBuilding_2");
                
            // Create main roads
            CreateRoad(buildingsContainer.transform, terrain, "LambtonQuay");
            CreateRoad(buildingsContainer.transform, terrain, "TheTerrace");
            CreateRoad(buildingsContainer.transform, terrain, "FeatherstonStreet");
            
            Debug.Log("Wellington buildings mockup created successfully");
        }
        
        private static void CreateBuilding(Transform parent, Terrain terrain, Vector3 position, Vector3 size, Material material, string name)
        {
            // Sample terrain height at building position
            float terrainHeight = 0;
            if (terrain != null)
            {
                terrainHeight = terrain.SampleHeight(position);
            }
            
            // Create building game object with cube primitive
            GameObject building = GameObject.CreatePrimitive(PrimitiveType.Cube);
            building.name = name;
            building.transform.SetParent(parent);
            
            // Position the building
            building.transform.position = new Vector3(position.x, terrainHeight + size.y/2, position.z);
            
            // Scale to desired size
            building.transform.localScale = size;
            
            // Apply material
            if (material != null)
            {
                building.GetComponent<Renderer>().material = material;
            }
        }
        
        private static void CreateRoad(Transform parent, Terrain terrain, string roadName)
        {
            GameObject road = new GameObject(roadName);
            road.transform.SetParent(parent);
            
            // Create a material for the road
            Material roadMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            roadMaterial.color = new Color(0.3f, 0.3f, 0.3f); // Dark gray for roads
            
            // Create a line renderer for the road
            LineRenderer lineRenderer = road.AddComponent<LineRenderer>();
            lineRenderer.material = roadMaterial;
            lineRenderer.startWidth = 15f; // Road width in meters
            lineRenderer.endWidth = 15f;
            
            // Set up road points based on the name
            Vector3[] points;
            
            switch (roadName)
            {
                case "LambtonQuay":
                    // Lambton Quay runs north-south along the eastern side
                    points = new Vector3[] {
                        new Vector3(250, 0, 50),
                        new Vector3(255, 0, 150),
                        new Vector3(260, 0, 250),
                        new Vector3(270, 0, 350),
                        new Vector3(280, 0, 450),
                        new Vector3(290, 0, 550)
                    };
                    break;
                    
                case "TheTerrace":
                    // The Terrace runs parallel to Lambton Quay but higher up
                    points = new Vector3[] {
                        new Vector3(400, 0, 50),
                        new Vector3(405, 0, 150),
                        new Vector3(410, 0, 250),
                        new Vector3(415, 0, 350),
                        new Vector3(420, 0, 450),
                        new Vector3(425, 0, 550)
                    };
                    break;
                    
                case "FeatherstonStreet":
                    // Featherston Street runs parallel between Lambton and The Terrace
                    points = new Vector3[] {
                        new Vector3(320, 0, 50),
                        new Vector3(325, 0, 150),
                        new Vector3(330, 0, 250),
                        new Vector3(335, 0, 350),
                        new Vector3(340, 0, 450),
                        new Vector3(345, 0, 550)
                    };
                    break;
                    
                default:
                    // Default simple road
                    points = new Vector3[] {
                        new Vector3(300, 0, 100),
                        new Vector3(300, 0, 500)
                    };
                    break;
            }
            
            // Adjust point heights to terrain
            for (int i = 0; i < points.Length; i++)
            {
                if (terrain != null)
                {
                    float y = terrain.SampleHeight(points[i]) + 0.2f; // 0.2m above terrain
                    points[i] = new Vector3(points[i].x, y, points[i].z);
                }
            }
            
            // Configure the line renderer
            lineRenderer.positionCount = points.Length;
            lineRenderer.SetPositions(points);
            lineRenderer.alignment = LineAlignment.TransformZ;
            lineRenderer.widthMultiplier = 1.0f;
            lineRenderer.numCapVertices = 4; // Rounded ends
            
            // Add a street name label
            GameObject nameLabel = new GameObject("RoadLabel");
            nameLabel.transform.SetParent(road.transform);
            
            // Position label in the middle of the road
            Vector3 midPoint = points[points.Length / 2];
            nameLabel.transform.position = new Vector3(midPoint.x, midPoint.y + 1f, midPoint.z);
            
            // Add TextMesh component for the label
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
