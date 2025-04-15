using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Represents a connection between two nodes in a morphological structure.
    /// </summary>
    public class MorphConnection : MonoBehaviour
    {
        private MorphNode nodeA;
        private MorphNode nodeB;
        private Material material;
        
        /// <summary>
        /// The first node in the connection.
        /// </summary>
        public MorphNode NodeA => nodeA;
        
        /// <summary>
        /// The second node in the connection.
        /// </summary>
        public MorphNode NodeB => nodeB;
        
        /// <summary>
        /// The length of the connection.
        /// </summary>
        public float Length
        {
            get
            {
                if (nodeA == null || nodeB == null) return 0f;
                return Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            }
        }
        
        /// <summary>
        /// Initializes the connection between two nodes.
        /// </summary>
        public void Initialize(MorphNode a, MorphNode b, Material connectionMaterial)
        {
            nodeA = a;
            nodeB = b;
            material = connectionMaterial;
            
            // Position the connection between the nodes
            UpdateVisual();
        }
        
        /// <summary>
        /// Updates the visual representation of the connection.
        /// </summary>
        public void UpdateVisual()
        {
            if (nodeA == null || nodeB == null) return;
            
            // Position at midpoint
            transform.position = (nodeA.transform.position + nodeB.transform.position) * 0.5f;
            
            // Orient towards the second node
            transform.LookAt(nodeB.transform);
            
            // Scale based on distance
            float distance = Vector3.Distance(nodeA.transform.position, nodeB.transform.position);
            transform.localScale = new Vector3(0.05f, 0.05f, distance);
        }
    }
}
