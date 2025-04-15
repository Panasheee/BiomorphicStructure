using UnityEngine;
using System.Collections.Generic;

namespace BiomorphicSim.Morphology
{
    /// <summary>
    /// Implements a space colonization algorithm for more organic, mold-like growth patterns.
    /// This algorithm creates structures similar to the reference images, with complex branching
    /// and interconnected networks.
    /// </summary>
    public class SpaceColonizationGrowth : IGrowthAlgorithm
    {
        // Algorithm parameters
        private float attractionRadius = 10f;        // How far an attraction point can influence nodes
        private float killRadius = 2f;               // Distance at which attraction points are removed
        private float branchLength = 1.5f;           // Length of new branches
        private float branchVariation = 0.5f;        // Random variation in branch length
        private float connectionProbability = 0.3f;  // Probability of creating connections between nearby nodes
        private int maxAttractors = 500;             // Maximum number of attraction points
        private int attractorsPerStep = 10;          // New attractors added per growth step
        private float envInfluenceStrength = 1.0f;   // How strongly environmental factors influence growth
        
        // Internal state
        private List<Vector3> attractionPoints = new List<Vector3>();
        private Dictionary<Vector3, List<int>> pointInfluences = new Dictionary<Vector3, List<int>>();
        private Vector3 environmentalInfluence = Vector3.zero; // Direction influenced by environment
        
        // Boundaries for attractor point generation
        private Vector3 minBounds = new Vector3(-50f, 0f, -50f);
        private Vector3 maxBounds = new Vector3(50f, 50f, 50f);
        
        // Constructor with customizable parameters
        public SpaceColonizationGrowth(float attractionRadius = 10f, float killRadius = 2f, 
                                      float branchLength = 1.5f, float envInfluenceStrength = 1.0f)
        {
            this.attractionRadius = attractionRadius;
            this.killRadius = killRadius;
            this.branchLength = branchLength;
            this.envInfluenceStrength = envInfluenceStrength;
        }
        
        public GrowthPoint DetermineNextGrowthPoint(List<MorphologyManager.BiomorphNode> currentNodes)
        {
            GrowthPoint result = new GrowthPoint();
            
            // Add new attraction points each growth step (mocks environmental distribution)
            AddAttractionPoints();
            
            // Skip if no nodes or attraction points
            if (currentNodes.Count == 0 || attractionPoints.Count == 0)
                return result;
            
            // Reset influences
            pointInfluences.Clear();
            
            // For each attraction point, find nodes that influence it
            foreach (Vector3 point in attractionPoints)
            {
                pointInfluences[point] = new List<int>();
                
                for (int i = 0; i < currentNodes.Count; i++)
                {
                    Vector3 nodePos = currentNodes[i].gameObject.transform.position;
                    float distance = Vector3.Distance(nodePos, point);
                    
                    // If node is within attraction radius, it influences this point
                    if (distance < attractionRadius)
                    {
                        pointInfluences[point].Add(i);
                    }
                    
                    // If node is very close to point, mark point for removal
                    if (distance < killRadius)
                    {
                        pointInfluences[point].Clear();
                        break;
                    }
                }
            }
            
            // Calculate growth directions for each node
            Dictionary<int, Vector3> growthDirections = new Dictionary<int, Vector3>();
            Dictionary<int, int> influenceCount = new Dictionary<int, int>();
            
            foreach (var pair in pointInfluences)
            {
                Vector3 point = pair.Key;
                List<int> influences = pair.Value;
                
                // Skip points that have been killed (no influences)
                if (influences.Count == 0)
                    continue;
                
                // For each node that influences this point
                foreach (int nodeIndex in influences)
                {
                    Vector3 nodePos = currentNodes[nodeIndex].gameObject.transform.position;
                    Vector3 direction = (point - nodePos).normalized;
                    
                    // Add direction to node's growth direction
                    if (!growthDirections.ContainsKey(nodeIndex))
                    {
                        growthDirections[nodeIndex] = direction;
                        influenceCount[nodeIndex] = 1;
                    }
                    else
                    {
                        growthDirections[nodeIndex] += direction;
                        influenceCount[nodeIndex]++;
                    }
                }
            }
            
            // Remove attraction points that are too close to nodes
            attractionPoints.RemoveAll(point => !pointInfluences.ContainsKey(point) || 
                                      pointInfluences[point].Count == 0);
            
            // Select the node with the most influences
            int selectedNodeIndex = -1;
            int maxInfluences = 0;
            
            foreach (var pair in influenceCount)
            {
                if (pair.Value > maxInfluences && 
                    currentNodes[pair.Key].connections.Count < 5 && // Limit connections
                    currentNodes[pair.Key].energy > 0) // Must have energy
                {
                    maxInfluences = pair.Value;
                    selectedNodeIndex = pair.Key;
                }
            }
            
            // If no node selected, find one with energy
            if (selectedNodeIndex == -1)
            {
                List<MorphologyManager.BiomorphNode> eligibleNodes = new List<MorphologyManager.BiomorphNode>();
                
                foreach (var node in currentNodes)
                {
                    if (node.energy > 0 && node.connections.Count < 5)
                    {
                        eligibleNodes.Add(node);
                    }
                }
                
                if (eligibleNodes.Count > 0)
                {
                    int randomIndex = Random.Range(0, eligibleNodes.Count);
                    selectedNodeIndex = currentNodes.IndexOf(eligibleNodes[randomIndex]);
                      // Create a random growth direction
                    Vector3 growthDir = Random.onUnitSphere;
                    growthDir.y = Mathf.Abs(growthDir.y); // Bias upward
                    growthDirections[selectedNodeIndex] = growthDir;
                }
            }
            
            // If a node is selected, calculate new position            if (selectedNodeIndex != -1 && growthDirections.ContainsKey(selectedNodeIndex))
            {
                MorphologyManager.BiomorphNode selectedNode = currentNodes[selectedNodeIndex];
                Vector3 direction = growthDirections[selectedNodeIndex];
                direction.Normalize();
                Vector3 growthDir = direction;
                
                // Blend with environmental influence
                growthDir = Vector3.Lerp(growthDir, environmentalInfluence, 
                                        environmentalInfluence.magnitude * envInfluenceStrength);
                growthDir.Normalize();
                
                // Calculate branch length with some randomness
                float actualBranchLength = branchLength * (1f + Random.Range(-branchVariation, branchVariation));
                
                // Calculate new position
                Vector3 nodePosition = selectedNode.gameObject.transform.position;
                Vector3 newPosition = nodePosition + (growthDir * actualBranchLength);
                
                // Set result
                result.position = newPosition;
                result.parentNode = selectedNode;
                
                // Decrease energy of parent node
                selectedNode.energy -= 0.2f;
                
                // Attempt to create connections between nearby nodes (creates web-like structure)
                CreateNearbyConnections(currentNodes, selectedNodeIndex, newPosition);
            }
            
            return result;
        }
        
        /// <summary>
        /// Creates connections between nearby nodes to form web-like structures
        /// </summary>
        private void CreateNearbyConnections(List<MorphologyManager.BiomorphNode> nodes, int sourceIndex, Vector3 newPosition)
        {
            // This would be implemented in the MorphologyManager
            // We're just providing the logic here
        }
        
        /// <summary>
        /// Adds new attraction points within the defined bounds
        /// </summary>
        private void AddAttractionPoints()
        {
            // Don't exceed maximum
            if (attractionPoints.Count >= maxAttractors)
                return;
                
            for (int i = 0; i < attractorsPerStep; i++)
            {
                Vector3 point = new Vector3(
                    Random.Range(minBounds.x, maxBounds.x),
                    Random.Range(minBounds.y, maxBounds.y),
                    Random.Range(minBounds.z, maxBounds.z)
                );
                
                attractionPoints.Add(point);
            }
        }
        
        /// <summary>
        /// Sets the environmental influence vector which affects growth direction
        /// </summary>
        public void SetEnvironmentalInfluence(Vector3 influence)
        {
            environmentalInfluence = influence.normalized;
        }
        
        /// <summary>
        /// Sets the bounds for attraction point generation
        /// </summary>
        public void SetBounds(Vector3 min, Vector3 max)
        {
            minBounds = min;
            maxBounds = max;
        }
    }
}
