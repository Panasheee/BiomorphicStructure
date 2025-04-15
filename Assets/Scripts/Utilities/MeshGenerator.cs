using System.Collections.Generic;
using UnityEngine;

namespace BiomorphicSim.Utilities
{
    /// <summary>
    /// Utility class for generating meshes for morphology structures.
    /// </summary>
    public static class MeshGenerator
    {
        #region Public Methods
        /// <summary>
        /// Generates a complete mesh for a morphology based on nodes and connections
        /// </summary>
        /// <param name="nodes">List of node positions</param>
        /// <param name="connections">List of connections between nodes (pairs of indices)</param>
        /// <param name="morphType">Type of morphology to generate</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateMorphologyMesh(List<Vector3> nodes, List<int[]> connections, MorphologyParameters.BiomorphType morphType)
        {
            Mesh mesh = new Mesh();
            
            switch (morphType)
            {
                case MorphologyParameters.BiomorphType.Mold:
                    return GenerateMoldMesh(nodes, connections);
                    
                case MorphologyParameters.BiomorphType.Bone:
                    return GenerateBoneMesh(nodes, connections);
                    
                case MorphologyParameters.BiomorphType.Coral:
                    return GenerateCoralMesh(nodes, connections);
                    
                case MorphologyParameters.BiomorphType.Mycelium:
                    return GenerateMyceliumMesh(nodes, connections);
                    
                default:
                    return GenerateMoldMesh(nodes, connections);
            }
        }
        
        /// <summary>
        /// Creates a metaball-like mesh from a collection of points and connections
        /// </summary>
        /// <param name="nodes">Node positions</param>
        /// <param name="connections">Node connections</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateMoldMesh(List<Vector3> nodes, List<int[]> connections)
        {
            // This would typically use marching cubes or metaball techniques
            // For simplicity in this prototype, we'll create a mesh from nodes and tubes
            
            // Create lists to gather all vertices and triangles
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create spheres for nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                AddNodeSphere(nodes[i], 0.5f, 8, vertices, triangles, uvs);
            }
            
            // Create tubes for connections
            foreach (int[] connection in connections)
            {
                if (connection.Length == 2)
                {
                    int nodeA = connection[0];
                    int nodeB = connection[1];
                    
                    if (nodeA >= 0 && nodeA < nodes.Count && nodeB >= 0 && nodeB < nodes.Count)
                    {
                        AddConnectionTube(nodes[nodeA], nodes[nodeB], 0.2f, 8, vertices, triangles, uvs);
                    }
                }
            }
            
            // Create the mesh
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Creates a bone-like mesh from a collection of points and connections
        /// </summary>
        /// <param name="nodes">Node positions</param>
        /// <param name="connections">Node connections</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateBoneMesh(List<Vector3> nodes, List<int[]> connections)
        {
            // For bone, we want a more angular, structured look
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create joint nodes using octahedrons
            for (int i = 0; i < nodes.Count; i++)
            {
                AddNodeOctahedron(nodes[i], 0.6f, vertices, triangles, uvs);
            }
            
            // Create connecting struts with thicker middle
            foreach (int[] connection in connections)
            {
                if (connection.Length == 2)
                {
                    int nodeA = connection[0];
                    int nodeB = connection[1];
                    
                    if (nodeA >= 0 && nodeA < nodes.Count && nodeB >= 0 && nodeB < nodes.Count)
                    {
                        AddConnectionBoneStrut(nodes[nodeA], nodes[nodeB], 0.2f, 0.35f, 6, vertices, triangles, uvs);
                    }
                }
            }
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Creates a coral-like mesh from a collection of points and connections
        /// </summary>
        /// <param name="nodes">Node positions</param>
        /// <param name="connections">Node connections</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateCoralMesh(List<Vector3> nodes, List<int[]> connections)
        {
            // For coral, we want a more organic, branching structure with flat plates
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create leaf-like nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                // Only add leaf plates to nodes with few connections (endpoints)
                int connectionCount = CountNodeConnections(i, connections);
                if (connectionCount <= 2)
                {
                    AddNodeCoralPlate(nodes[i], 0.7f, 0.1f, vertices, triangles, uvs);
                }
                else
                {
                    AddNodeSphere(nodes[i], 0.3f, 6, vertices, triangles, uvs);
                }
            }
            
            // Create branch tubes
            foreach (int[] connection in connections)
            {
                if (connection.Length == 2)
                {
                    int nodeA = connection[0];
                    int nodeB = connection[1];
                    
                    if (nodeA >= 0 && nodeA < nodes.Count && nodeB >= 0 && nodeB < nodes.Count)
                    {
                        // Vary thickness based on distance from root
                        float thickness = 0.2f * (1.0f - Mathf.Min(nodes[nodeA].y, nodes[nodeB].y) / 10.0f);
                        thickness = Mathf.Clamp(thickness, 0.05f, 0.3f);
                        
                        AddConnectionTube(nodes[nodeA], nodes[nodeB], thickness, 6, vertices, triangles, uvs);
                    }
                }
            }
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Creates a mycelium-like mesh from a collection of points and connections
        /// </summary>
        /// <param name="nodes">Node positions</param>
        /// <param name="connections">Node connections</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateMyceliumMesh(List<Vector3> nodes, List<int[]> connections)
        {
            // For mycelium, we want very thin connections and small nodes
            List<Vector3> vertices = new List<Vector3>();
            List<int> triangles = new List<int>();
            List<Vector2> uvs = new List<Vector2>();
            
            // Create small spherical nodes
            for (int i = 0; i < nodes.Count; i++)
            {
                int connectionCount = CountNodeConnections(i, connections);
                float radius = 0.2f + connectionCount * 0.05f;
                AddNodeSphere(nodes[i], radius, 6, vertices, triangles, uvs);
            }
            
            // Create thin connections
            foreach (int[] connection in connections)
            {
                if (connection.Length == 2)
                {
                    int nodeA = connection[0];
                    int nodeB = connection[1];
                    
                    if (nodeA >= 0 && nodeA < nodes.Count && nodeB >= 0 && nodeB < nodes.Count)
                    {
                        // Thinner tubes with slight variation
                        float thickness = 0.1f + Random.Range(-0.05f, 0.05f);
                        thickness = Mathf.Max(0.05f, thickness);
                        
                        AddConnectionTube(nodes[nodeA], nodes[nodeB], thickness, 5, vertices, triangles, uvs);
                    }
                }
            }
            
            Mesh mesh = new Mesh();
            mesh.vertices = vertices.ToArray();
            mesh.triangles = triangles.ToArray();
            mesh.uv = uvs.ToArray();
            
            mesh.RecalculateNormals();
            
            return mesh;
        }
        
        /// <summary>
        /// Generates a smooth surface mesh using metaballs technique
        /// </summary>
        /// <param name="nodes">Points for metaballs</param>
        /// <param name="radius">Radius of influence for each point</param>
        /// <param name="threshold">Threshold value for surface extraction</param>
        /// <param name="resolution">Grid resolution</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateMetaballMesh(List<Vector3> nodes, float radius, float threshold, int resolution)
        {
            // Calculate bounds
            Bounds bounds = CalculateBounds(nodes, radius);
            
            // Create the grid
            float[,,] grid = new float[resolution, resolution, resolution];
            
            // Calculate grid cell size
            Vector3 cellSize = bounds.size / (resolution - 1);
            
            // Fill the grid with scalar field values
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 pos = bounds.min + new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z);
                        grid[x, y, z] = CalculateScalarField(pos, nodes, radius);
                    }
                }
            }
            
            // Extract the surface using Marching Cubes algorithm
            return MarchingCubes.GenerateMesh(grid, bounds.min, cellSize, threshold);
        }
        
        /// <summary>
        /// Converts a morphology into a voronoi-like cellular structure
        /// </summary>
        /// <param name="nodes">Node positions</param>
        /// <param name="connections">Node connections</param>
        /// <param name="voronoiPoints">Points for voronoi cells</param>
        /// <param name="resolution">Grid resolution</param>
        /// <returns>Generated mesh</returns>
        public static Mesh GenerateVoronoiMesh(List<Vector3> nodes, List<int[]> connections, int resolution)
        {
            // Calculate bounds
            Bounds bounds = CalculateBounds(nodes, 1.0f);
            
            // Generate voronoi points (use nodes plus random points)
            List<Vector3> voronoiPoints = new List<Vector3>(nodes);
            
            // Add some random points
            int additionalPoints = 100;
            for (int i = 0; i < additionalPoints; i++)
            {
                Vector3 randomPoint = new Vector3(
                    Random.Range(bounds.min.x, bounds.max.x),
                    Random.Range(bounds.min.y, bounds.max.y),
                    Random.Range(bounds.min.z, bounds.max.z)
                );
                voronoiPoints.Add(randomPoint);
            }
            
            // Create a signed distance field
            float[,,] grid = new float[resolution, resolution, resolution];
            Vector3 cellSize = bounds.size / (resolution - 1);
            
            for (int x = 0; x < resolution; x++)
            {
                for (int y = 0; y < resolution; y++)
                {
                    for (int z = 0; z < resolution; z++)
                    {
                        Vector3 pos = bounds.min + new Vector3(x * cellSize.x, y * cellSize.y, z * cellSize.z);
                        
                        // Check if this point is inside the morphology (approximated)
                        bool isInside = IsPointInsideMorphology(pos, nodes, connections, 1.0f);
                        
                        if (isInside)
                        {
                            // Calculate closest voronoi edge
                            float voronoiDistance = CalculateVoronoiDistance(pos, voronoiPoints);
                            
                            // Positive inside, negative outside
                            grid[x, y, z] = 0.5f - voronoiDistance;
                        }
                        else
                        {
                            // Outside the morphology
                            grid[x, y, z] = -1.0f;
                        }
                    }
                }
            }
            
            // Extract surface
            return MarchingCubes.GenerateMesh(grid, bounds.min, cellSize, 0.0f);
        }
        #endregion

        #region Helper Methods
        /// <summary>
        /// Adds a spherical node to the mesh data
        /// </summary>
        private static void AddNodeSphere(Vector3 position, float radius, int segments, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            int baseIndex = vertices.Count;
            
            // Create vertices
            // Top and bottom vertices
            vertices.Add(position + Vector3.up * radius);
            vertices.Add(position + Vector3.down * radius);
            
            // Add UVs for top and bottom
            uvs.Add(new Vector2(0.5f, 1.0f));
            uvs.Add(new Vector2(0.5f, 0.0f));
            
            // Middle vertices (rings)
            for (int ring = 1; ring < segments; ring++)
            {
                float phi = Mathf.PI * ring / segments;
                float y = radius * Mathf.Cos(phi);
                float ringRadius = radius * Mathf.Sin(phi);
                
                for (int i = 0; i < segments; i++)
                {
                    float theta = 2 * Mathf.PI * i / segments;
                    
                    float x = ringRadius * Mathf.Cos(theta);
                    float z = ringRadius * Mathf.Sin(theta);
                    
                    vertices.Add(position + new Vector3(x, y, z));
                    
                    // Calculate UV coordinates
                    float u = (float)i / segments;
                    float v = (float)ring / segments;
                    
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Add triangles
            // Top cap
            for (int i = 0; i < segments; i++)
            {
                int nextI = (i + 1) % segments;
                triangles.Add(baseIndex); // Top vertex
                triangles.Add(baseIndex + 2 + i);
                triangles.Add(baseIndex + 2 + nextI);
            }
            
            // Middle rings
            for (int ring = 0; ring < segments - 2; ring++)
            {
                int ringStartIndex = baseIndex + 2 + ring * segments;
                int nextRingStartIndex = ringStartIndex + segments;
                
                for (int i = 0; i < segments; i++)
                {
                    int nextI = (i + 1) % segments;
                    
                    // First triangle
                    triangles.Add(ringStartIndex + i);
                    triangles.Add(nextRingStartIndex + i);
                    triangles.Add(ringStartIndex + nextI);
                    
                    // Second triangle
                    triangles.Add(ringStartIndex + nextI);
                    triangles.Add(nextRingStartIndex + i);
                    triangles.Add(nextRingStartIndex + nextI);
                }
            }
            
            // Bottom cap
            int lastRingStartIndex = baseIndex + 2 + (segments - 2) * segments;
            for (int i = 0; i < segments; i++)
            {
                int nextI = (i + 1) % segments;
                triangles.Add(baseIndex + 1); // Bottom vertex
                triangles.Add(lastRingStartIndex + nextI);
                triangles.Add(lastRingStartIndex + i);
            }
        }
        
        /// <summary>
        /// Adds a tubular connection between two points
        /// </summary>
        private static void AddConnectionTube(Vector3 start, Vector3 end, float radius, int segments, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            int baseIndex = vertices.Count;
            
            // Direction from start to end
            Vector3 direction = (end - start).normalized;
            
            // Find two perpendicular directions
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            if (perpendicular.magnitude < 0.01f)
            {
                perpendicular = Vector3.Cross(direction, Vector3.forward);
            }
            perpendicular.Normalize();
            
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular).normalized;
            
            // Create vertices for the rings at start and end
            for (int ring = 0; ring < 2; ring++)
            {
                Vector3 center = ring == 0 ? start : end;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle = 2 * Mathf.PI * i / segments;
                    
                    Vector3 offset = perpendicular * Mathf.Cos(angle) + perpendicular2 * Mathf.Sin(angle);
                    vertices.Add(center + offset * radius);
                    
                    // Calculate UV coordinates
                    float u = (float)i / segments;
                    float v = ring;
                    
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Connect the rings with triangles
            for (int i = 0; i < segments; i++)
            {
                int nextI = (i + 1) % segments;
                
                // First triangle
                triangles.Add(baseIndex + i);
                triangles.Add(baseIndex + segments + i);
                triangles.Add(baseIndex + nextI);
                
                // Second triangle
                triangles.Add(baseIndex + nextI);
                triangles.Add(baseIndex + segments + i);
                triangles.Add(baseIndex + segments + nextI);
            }
        }
        
        /// <summary>
        /// Adds an octahedral node (for bone-like structures)
        /// </summary>
        private static void AddNodeOctahedron(Vector3 position, float size, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            int baseIndex = vertices.Count;
            
            // Create vertices - octahedron has 6 vertices
            vertices.Add(position + Vector3.up * size);           // Top
            vertices.Add(position + Vector3.forward * size);      // Front
            vertices.Add(position + Vector3.right * size);        // Right
            vertices.Add(position + Vector3.back * size);         // Back
            vertices.Add(position + Vector3.left * size);         // Left
            vertices.Add(position + Vector3.down * size);         // Bottom
            
            // Add UVs
            for (int i = 0; i < 6; i++)
            {
                uvs.Add(new Vector2((float)i / 6, 0));
            }
            
            // Add triangles (8 triangular faces for octahedron)
            // Top half
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 2);
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 3);
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 4);
            
            triangles.Add(baseIndex);
            triangles.Add(baseIndex + 4);
            triangles.Add(baseIndex + 1);
            
            // Bottom half
            triangles.Add(baseIndex + 5);
            triangles.Add(baseIndex + 2);
            triangles.Add(baseIndex + 1);
            
            triangles.Add(baseIndex + 5);
            triangles.Add(baseIndex + 3);
            triangles.Add(baseIndex + 2);
            
            triangles.Add(baseIndex + 5);
            triangles.Add(baseIndex + 4);
            triangles.Add(baseIndex + 3);
            
            triangles.Add(baseIndex + 5);
            triangles.Add(baseIndex + 1);
            triangles.Add(baseIndex + 4);
        }
        
        /// <summary>
        /// Adds a bone-like strut between two points (thicker in the middle)
        /// </summary>
        private static void AddConnectionBoneStrut(Vector3 start, Vector3 end, float radiusEnd, float radiusMiddle, int segments, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            int baseIndex = vertices.Count;
            
            // Direction from start to end
            Vector3 direction = (end - start).normalized;
            float length = Vector3.Distance(start, end);
            
            // Find two perpendicular directions
            Vector3 perpendicular = Vector3.Cross(direction, Vector3.up);
            if (perpendicular.magnitude < 0.01f)
            {
                perpendicular = Vector3.Cross(direction, Vector3.forward);
            }
            perpendicular.Normalize();
            
            Vector3 perpendicular2 = Vector3.Cross(direction, perpendicular).normalized;
            
            // Create vertices for three rings (start, middle, end)
            for (int ring = 0; ring < 3; ring++)
            {
                Vector3 center = start + direction * (length * ring / 2);
                float radius = ring == 1 ? radiusMiddle : radiusEnd;
                
                for (int i = 0; i < segments; i++)
                {
                    float angle = 2 * Mathf.PI * i / segments;
                    
                    Vector3 offset = perpendicular * Mathf.Cos(angle) + perpendicular2 * Mathf.Sin(angle);
                    vertices.Add(center + offset * radius);
                    
                    // Calculate UV coordinates
                    float u = (float)i / segments;
                    float v = (float)ring / 2;
                    
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Connect the rings with triangles
            for (int ring = 0; ring < 2; ring++)
            {
                int ringStartIndex = baseIndex + ring * segments;
                int nextRingStartIndex = ringStartIndex + segments;
                
                for (int i = 0; i < segments; i++)
                {
                    int nextI = (i + 1) % segments;
                    
                    // First triangle
                    triangles.Add(ringStartIndex + i);
                    triangles.Add(nextRingStartIndex + i);
                    triangles.Add(ringStartIndex + nextI);
                    
                    // Second triangle
                    triangles.Add(ringStartIndex + nextI);
                    triangles.Add(nextRingStartIndex + i);
                    triangles.Add(nextRingStartIndex + nextI);
                }
            }
        }
        
        /// <summary>
        /// Adds a coral-like plate (for coral structures)
        /// </summary>
        private static void AddNodeCoralPlate(Vector3 position, float size, float thickness, List<Vector3> vertices, List<int> triangles, List<Vector2> uvs)
        {
            int baseIndex = vertices.Count;
            
            // Create random orientation
            Quaternion rotation = Quaternion.Euler(
                Random.Range(-30f, 30f),
                Random.Range(0f, 360f),
                Random.Range(-30f, 30f)
            );
            
            // Create a flat, slightly wavy plate
            int resolution = 5;
            float step = size / resolution;
            
            for (int x = 0; x <= resolution; x++)
            {
                for (int z = 0; z <= resolution; z++)
                {
                    float xPos = x * step - size / 2;
                    float zPos = z * step - size / 2;
                    
                    // Add some noise for wavy effect
                    float noise = Mathf.PerlinNoise(x * 0.5f, z * 0.5f) * 0.2f * size;
                    
                    // Create top and bottom vertices
                    Vector3 localPos = new Vector3(xPos, noise, zPos);
                    Vector3 topPos = position + rotation * localPos + rotation * (Vector3.up * thickness / 2);
                    Vector3 bottomPos = position + rotation * localPos + rotation * (Vector3.down * thickness / 2);
                    
                    vertices.Add(topPos);
                    vertices.Add(bottomPos);
                    
                    // Add UVs
                    float u = (float)x / resolution;
                    float v = (float)z / resolution;
                    
                    uvs.Add(new Vector2(u, v));
                    uvs.Add(new Vector2(u, v));
                }
            }
            
            // Create quads between adjacent vertices
            for (int x = 0; x < resolution; x++)
            {
                for (int z = 0; z < resolution; z++)
                {
                    int topLeft = baseIndex + (x * (resolution + 1) + z) * 2;
                    int topRight = baseIndex + (x * (resolution + 1) + z + 1) * 2;
                    int bottomLeft = baseIndex + ((x + 1) * (resolution + 1) + z) * 2;
                    int bottomRight = baseIndex + ((x + 1) * (resolution + 1) + z + 1) * 2;
                    
                    // Top face
                    triangles.Add(topLeft);
                    triangles.Add(bottomLeft);
                    triangles.Add(topRight);
                    
                    triangles.Add(topRight);
                    triangles.Add(bottomLeft);
                    triangles.Add(bottomRight);
                    
                    // Bottom face
                    triangles.Add(topLeft + 1);
                    triangles.Add(topRight + 1);
                    triangles.Add(bottomLeft + 1);
                    
                    triangles.Add(topRight + 1);
                    triangles.Add(bottomRight + 1);
                    triangles.Add(bottomLeft + 1);
                }
            }
            
            // Add edges
            for (int x = 0; x < resolution; x++)
            {
                // Front edge
                int frontTopLeft = baseIndex + (x * (resolution + 1)) * 2;
                int frontTopRight = baseIndex + ((x + 1) * (resolution + 1)) * 2;
                int frontBottomLeft = baseIndex + (x * (resolution + 1)) * 2 + 1;
                int frontBottomRight = baseIndex + ((x + 1) * (resolution + 1)) * 2 + 1;
                
                triangles.Add(frontTopLeft);
                triangles.Add(frontBottomLeft);
                triangles.Add(frontTopRight);
                
                triangles.Add(frontBottomLeft);
                triangles.Add(frontBottomRight);
                triangles.Add(frontTopRight);
                
                // Back edge
                int backTopLeft = baseIndex + (x * (resolution + 1) + resolution) * 2;
                int backTopRight = baseIndex + ((x + 1) * (resolution + 1) + resolution) * 2;
                int backBottomLeft = baseIndex + (x * (resolution + 1) + resolution) * 2 + 1;
                int backBottomRight = baseIndex + ((x + 1) * (resolution + 1) + resolution) * 2 + 1;
                
                triangles.Add(backTopLeft);
                triangles.Add(backTopRight);
                triangles.Add(backBottomLeft);
                
                triangles.Add(backBottomLeft);
                triangles.Add(backTopRight);
                triangles.Add(backBottomRight);
            }
            
            for (int z = 0; z < resolution; z++)
            {
                // Left edge
                int leftTopLeft = baseIndex + z * 2;
                int leftTopRight = baseIndex + (z + 1) * 2;
                int leftBottomLeft = baseIndex + z * 2 + 1;
                int leftBottomRight = baseIndex + (z + 1) * 2 + 1;
                
                triangles.Add(leftTopLeft);
                triangles.Add(leftTopRight);
                triangles.Add(leftBottomLeft);
                
                triangles.Add(leftBottomLeft);
                triangles.Add(leftTopRight);
                triangles.Add(leftBottomRight);
                
                // Right edge
                int rightTopLeft = baseIndex + (resolution * (resolution + 1) + z) * 2;
                int rightTopRight = baseIndex + (resolution * (resolution + 1) + z + 1) * 2;
                int rightBottomLeft = baseIndex + (resolution * (resolution + 1) + z) * 2 + 1;
                int rightBottomRight = baseIndex + (resolution * (resolution + 1) + z + 1) * 2 + 1;
                
                triangles.Add(rightTopLeft);
                triangles.Add(rightBottomLeft);
                triangles.Add(rightTopRight);
                
                triangles.Add(rightBottomLeft);
                triangles.Add(rightBottomRight);
                triangles.Add(rightTopRight);
            }
        }
        
        /// <summary>
        /// Counts how many connections a node has
        /// </summary>
        private static int CountNodeConnections(int nodeIndex, List<int[]> connections)
        {
            int count = 0;
            
            foreach (int[] connection in connections)
            {
                if (connection[0] == nodeIndex || connection[1] == nodeIndex)
                {
                    count++;
                }
            }
            
            return count;
        }
        
        /// <summary>
        /// Calculates the bounding box for a list of points
        /// </summary>
        private static Bounds CalculateBounds(List<Vector3> points, float padding)
        {
            if (points.Count == 0)
            {
                return new Bounds(Vector3.zero, Vector3.one * padding * 2);
            }
            
            Vector3 min = points[0];
            Vector3 max = points[0];
            
            foreach (Vector3 point in points)
            {
                min = Vector3.Min(min, point);
                max = Vector3.Max(max, point);
            }
            
            // Add padding
            min -= Vector3.one * padding;
            max += Vector3.one * padding;
            
            return new Bounds((min + max) / 2, max - min);
        }
        
        /// <summary>
        /// Calculates the scalar field value at a point for metaball generation
        /// </summary>
        private static float CalculateScalarField(Vector3 point, List<Vector3> metaballCenters, float radius)
        {
            float sum = 0;
            
            foreach (Vector3 center in metaballCenters)
            {
                float distance = Vector3.Distance(point, center);
                if (distance < radius)
                {
                    // Smooth fall-off function
                    float value = 1.0f - (distance * distance) / (radius * radius);
                    sum += value * value;
                }
            }
            
            return sum;
        }
        
        /// <summary>
        /// Determines if a point is inside the morphology volume
        /// </summary>
        private static bool IsPointInsideMorphology(Vector3 point, List<Vector3> nodes, List<int[]> connections, float threshold)
        {
            // Simple implementation: check if point is within threshold distance of any node or connection
            
            // Check nodes
            foreach (Vector3 node in nodes)
            {
                if (Vector3.Distance(point, node) < threshold)
                {
                    return true;
                }
            }
            
            // Check connections
            foreach (int[] connection in connections)
            {
                if (connection.Length != 2) continue;
                
                Vector3 start = nodes[connection[0]];
                Vector3 end = nodes[connection[1]];
                
                // Check distance to line segment
                float distance = DistancePointLineSegment(point, start, end);
                if (distance < threshold * 0.5f)
                {
                    return true;
                }
            }
            
            return false;
        }
        
        /// <summary>
        /// Calculates the distance from a point to a line segment
        /// </summary>
        private static float DistancePointLineSegment(Vector3 point, Vector3 start, Vector3 end)
        {
            Vector3 line = end - start;
            float lineLength = line.magnitude;
            line.Normalize();
            
            Vector3 v = point - start;
            float d = Vector3.Dot(v, line);
            d = Mathf.Clamp(d, 0f, lineLength);
            
            Vector3 nearestPoint = start + line * d;
            return Vector3.Distance(point, nearestPoint);
        }
        
        /// <summary>
        /// Calculates the distance to the nearest voronoi edge
        /// </summary>
        private static float CalculateVoronoiDistance(Vector3 point, List<Vector3> voronoiPoints)
        {
            if (voronoiPoints.Count < 2) return 0;
            
            // Find the two closest points
            float minDist1 = float.MaxValue;
            float minDist2 = float.MaxValue;
            int index1 = 0;
            
            for (int i = 0; i < voronoiPoints.Count; i++)
            {
                float dist = Vector3.Distance(point, voronoiPoints[i]);
                
                if (dist < minDist1)
                {
                    minDist2 = minDist1;
                    minDist1 = dist;
                    index1 = i;
                }
                else if (dist < minDist2)
                {
                    minDist2 = dist;
                }
            }
            
            // Distance to the voronoi edge is proportional to the difference
            // between distances to the two closest points
            return Mathf.Abs(minDist1 - minDist2);
        }
        #endregion

        #region Marching Cubes Data (Moved outside ProcessCube)
        // Precomputed edge table for Marching Cubes
        // Maps cube index (based on corner values relative to threshold) to edges intersected
        private static readonly int[] edgeTable = { /* ... Full table data ... */ };

        // Precomputed triangle table for Marching Cubes
        // Maps cube index to vertex indices forming triangles
        private static readonly int[,] triTable = { /* ... Full table data ... */ };

        // Defines the 12 edges of a cube based on corner indices (0-7)
        private static readonly int[][] edgeCorners = new int[12][]
        {
            new int[] { 0, 1 }, new int[] { 1, 2 }, new int[] { 2, 3 }, new int[] { 3, 0 }, // Bottom face
            new int[] { 4, 5 }, new int[] { 5, 6 }, new int[] { 6, 7 }, new int[] { 7, 4 }, // Top face
            new int[] { 0, 4 }, new int[] { 1, 5 }, new int[] { 2, 6 }, new int[] { 3, 7 }  // Connecting edges
        };
        #endregion

        /// <summary>
        /// Processes a single cube for the Marching Cubes algorithm
        /// </summary>
        private static void ProcessCube(int cubeIndex, Vector3[] corners, float[] values, float threshold, List<Vector3> vertices, List<int> triangles)
        {
            // Only process if some corners are above and some are below the threshold
            if (cubeIndex == 0 || cubeIndex == 255) return;
            
            int baseIndex = vertices.Count;
            Vector3[] edgeVertices = new Vector3[12]; // Store interpolated vertices for this cube
            
            // Simplified: For each edge that crosses the threshold, add a vertex
            // In real marching cubes, we'd use the precomputed edge table
            for (int edge = 0; edge < 12; edge++)
            {
                int corner1 = edgeCorners[edge][0]; // Should work now
                int corner2 = edgeCorners[edge][1]; // Should work now
                
                // Check if this edge crosses the threshold
                if ((values[corner1] > threshold != values[corner2] > threshold)) // Simplified check
                {
                    // Interpolate between corners to find the exact crossing point
                    float t = (threshold - values[corner1]) / (values[corner2] - values[corner1]);
                    edgeVertices[edge] = Vector3.Lerp(corners[corner1], corners[corner2], t);
                }
            }
            
            // Use the triangle table to create triangles
            // triTable[cubeIndex] gives a sequence of vertex indices (up to 15, ending with -1)
            for (int i = 0; triTable[cubeIndex, i] != -1; i += 3)
            {
                // Get the indices of the edges that form the vertices of the triangle
                int edgeIndex1 = triTable[cubeIndex, i];
                int edgeIndex2 = triTable[cubeIndex, i + 1];
                int edgeIndex3 = triTable[cubeIndex, i + 2];

                // Add the interpolated vertices to the main vertex list
                // We need a way to map edge indices back to actual vertices added
                // This simplified approach might duplicate vertices; a real implementation uses a vertex dictionary
                
                // For simplicity here, let's just add them directly if they were calculated
                // This part needs refinement for a correct Marching Cubes implementation
                // A proper implementation would reuse vertices on shared edges.
                
                // This is likely where the original CS0029/CS0021 errors might have stemmed from
                // if the logic was trying to misuse the tables. Let's add vertices and triangles
                // based on the triTable indices referring to the calculated edgeVertices.

                // Check if edge vertices exist (were calculated because edge crossed threshold)
                // This check is basic; real MC handles vertex reuse.
                if (edgeVertices[edgeIndex1] != Vector3.zero && 
                    edgeVertices[edgeIndex2] != Vector3.zero && 
                    edgeVertices[edgeIndex3] != Vector3.zero)
                {
                    int vert1Index = vertices.Count;
                    vertices.Add(edgeVertices[edgeIndex1]);
                    int vert2Index = vertices.Count;
                    vertices.Add(edgeVertices[edgeIndex2]);
                    int vert3Index = vertices.Count;
                    vertices.Add(edgeVertices[edgeIndex3]);

                    triangles.Add(vert1Index);
                    triangles.Add(vert2Index);
                    triangles.Add(vert3Index);
                }
            }
        }
    }
}