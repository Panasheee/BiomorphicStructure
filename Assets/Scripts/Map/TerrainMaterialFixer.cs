// filepath: c:\Users\tyron\BiomorphicSim\Assets\Scripts\Map\TerrainMaterialFixer.cs
using UnityEngine;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// Fixes the magenta "missing material" issue by applying a proper terrain material.
    /// Attach this to the Site_Terrain GameObject that currently has the magenta material.
    /// </summary>
    public class TerrainMaterialFixer : MonoBehaviour
    {
        [Header("Material Settings")]
        [SerializeField] private Color terrainColor = new Color(0.46f, 0.52f, 0.32f); // Default green-brown color
        [SerializeField] private Texture2D terrainTexture; // Optional terrain texture
        [SerializeField] private float textureScale = 100f; // Scale of the texture tiling

        private void Start()
        {
            FixTerrainMaterial();
        }

        /// <summary>
        /// Creates and applies a proper terrain material to this GameObject's renderer
        /// </summary>
        public void FixTerrainMaterial()
        {
            // Get the renderer component
            Renderer renderer = GetComponent<Renderer>();
            if (renderer == null)
            {
                Debug.LogError("No Renderer found on this GameObject!");
                return;
            }

            // Create a new material using the Standard shader
            Material terrainMaterial = new Material(Shader.Find("Standard"));
            
            // Set the material color and properties
            terrainMaterial.color = terrainColor;
            
            // If a texture is assigned, use it
            if (terrainTexture != null)
            {
                terrainMaterial.mainTexture = terrainTexture;
                terrainMaterial.SetTextureScale("_MainTex", new Vector2(textureScale, textureScale));
            }
            
            // Apply the material to the renderer
            renderer.material = terrainMaterial;
            
            Debug.Log("Terrain material successfully applied to " + gameObject.name);
        }
    }
}
