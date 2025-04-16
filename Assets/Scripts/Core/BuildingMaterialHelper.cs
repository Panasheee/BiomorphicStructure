using UnityEngine;
using System.Collections.Generic;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Handles building materials and texturing for Wellington buildings
    /// </summary>
    public static class BuildingMaterialHelper
    {
        private static Dictionary<string, Material> cachedMaterials = new Dictionary<string, Material>();
        
        // Building textures
        public static Material GetOfficeBuildingMaterial()
        {
            if (cachedMaterials.ContainsKey("office"))
                return cachedMaterials["office"];
                
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Try to load a glass/window texture
            Texture2D glassTexture = Resources.Load<Texture2D>("Textures/BuildingGlass");
            if (glassTexture != null)
            {
                material.mainTexture = glassTexture;
            }
            else
            {
                // Create a procedural window texture
                Texture2D procGlassTexture = CreateWindowTexture(8, 16, Color.blue);
                material.mainTexture = procGlassTexture;
            }
            
            material.color = new Color(0.8f, 0.85f, 0.9f);
            material.SetFloat("_Glossiness", 0.8f); // Make it glossy like glass
            
            cachedMaterials["office"] = material;
            return material;
        }
        
        public static Material GetGovernmentBuildingMaterial()
        {
            if (cachedMaterials.ContainsKey("government"))
                return cachedMaterials["government"];
                
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Try to load a stone/concrete texture
            Texture2D stoneTexture = Resources.Load<Texture2D>("Textures/BuildingStone");
            if (stoneTexture != null)
            {
                material.mainTexture = stoneTexture;
            }
            else
            {
                // Create a procedural stone texture
                Texture2D procStoneTexture = CreateStoneTexture(512, 512);
                material.mainTexture = procStoneTexture;
            }
            
            material.color = new Color(0.85f, 0.82f, 0.78f);
            material.SetFloat("_Glossiness", 0.2f); // Less glossy
            
            cachedMaterials["government"] = material;
            return material;
        }
        
        public static Material GetRetailBuildingMaterial()
        {
            if (cachedMaterials.ContainsKey("retail"))
                return cachedMaterials["retail"];
                
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Try to load a retail/storefront texture
            Texture2D retailTexture = Resources.Load<Texture2D>("Textures/BuildingRetail");
            if (retailTexture != null)
            {
                material.mainTexture = retailTexture;
            }
            else
            {
                // Create a procedural storefront texture
                Texture2D procRetailTexture = CreateStorefrontTexture(512, 512);
                material.mainTexture = procRetailTexture;
            }
            
            material.color = new Color(0.9f, 0.85f, 0.8f);
            material.SetFloat("_Glossiness", 0.4f); // Medium glossiness
            
            cachedMaterials["retail"] = material;
            return material;
        }
        
        public static Material GetRoofMaterial()
        {
            if (cachedMaterials.ContainsKey("roof"))
                return cachedMaterials["roof"];
                
            Material material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            
            // Create a procedural roof texture
            Texture2D roofTexture = CreateRoofTexture(256, 256);
            material.mainTexture = roofTexture;
            material.color = new Color(0.3f, 0.3f, 0.3f);
            
            cachedMaterials["roof"] = material;
            return material;
        }
        
        // Texture generation methods
        private static Texture2D CreateWindowTexture(int windowsX, int windowsY, Color tint)
        {
            int width = windowsX * 16;  // 16 pixels per window
            int height = windowsY * 16;
            
            Texture2D texture = new Texture2D(width, height);
            
            // Fill with window pattern
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    int windowX = x / 16;
                    int windowY = y / 16;
                    
                    // Window frame
                    bool isFrame = (x % 16 == 0) || (y % 16 == 0) || (x % 16 == 15) || (y % 16 == 15);
                    
                    // Window reflection pattern
                    float reflectionX = (x % 16) / 16.0f;
                    float reflectionY = (y % 16) / 16.0f;
                    float reflection = Mathf.Pow(reflectionX * reflectionY, 0.5f);
                    
                    if (isFrame)
                    {
                        // Dark frame
                        texture.SetPixel(x, y, new Color(0.1f, 0.1f, 0.15f));
                    }
                    else
                    {
                        // Glass with reflection
                        float r = 0.4f + reflection * 0.5f + tint.r * 0.1f;
                        float g = 0.45f + reflection * 0.5f + tint.g * 0.1f;
                        float b = 0.5f + reflection * 0.5f + tint.b * 0.1f;
                        
                        texture.SetPixel(x, y, new Color(r, g, b));
                    }
                }
            }
            
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }
        
        private static Texture2D CreateStoneTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            
            // Fill with stone pattern
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Create noise pattern for stone
                    float noise1 = Mathf.PerlinNoise(x * 0.05f, y * 0.05f);
                    float noise2 = Mathf.PerlinNoise(x * 0.1f, y * 0.1f);
                    float combined = (noise1 + noise2) * 0.5f;
                    
                    // Create subtle stone color variation
                    float r = 0.7f + combined * 0.15f;
                    float g = 0.7f + combined * 0.15f;
                    float b = 0.65f + combined * 0.15f;
                    
                    texture.SetPixel(x, y, new Color(r, g, b));
                }
            }
            
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }
        
        private static Texture2D CreateStorefrontTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            
            // Fill with storefront pattern
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Create large storefront windows with frames
                    bool isVerticalFrame = (x % 64 < 4) || (x % 64 > 60);
                    bool isHorizontalFrame = (y % 48 < 4) || (y % 48 > 44);
                    bool isShowcase = (y < height / 3) && !isVerticalFrame && !isHorizontalFrame;
                    
                    if (isVerticalFrame || isHorizontalFrame)
                    {
                        // Frame color
                        texture.SetPixel(x, y, new Color(0.5f, 0.4f, 0.3f));
                    }
                    else if (isShowcase)
                    {
                        // Showcase window - more transparent
                        float reflection = Mathf.PerlinNoise(x * 0.01f, y * 0.01f) * 0.5f;
                        texture.SetPixel(x, y, new Color(0.6f + reflection, 0.6f + reflection, 0.7f + reflection));
                    }
                    else
                    {
                        // Upper floors - more regular windows or wall
                        bool isWindow = (x % 32 > 4) && (x % 32 < 28) && (y % 24 > 4) && (y % 24 < 20);
                        if (isWindow)
                        {
                            float reflection = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.3f;
                            texture.SetPixel(x, y, new Color(0.55f + reflection, 0.55f + reflection, 0.6f + reflection));
                        }
                        else
                        {
                            // Wall material
                            float noise = Mathf.PerlinNoise(x * 0.05f, y * 0.05f) * 0.1f;
                            texture.SetPixel(x, y, new Color(0.75f + noise, 0.7f + noise, 0.65f + noise));
                        }
                    }
                }
            }
            
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }
        
        private static Texture2D CreateRoofTexture(int width, int height)
        {
            Texture2D texture = new Texture2D(width, height);
            
            // Fill with roof pattern
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // Create gravel/tar roof pattern
                    float noise = Mathf.PerlinNoise(x * 0.1f, y * 0.1f) * 0.15f;
                    
                    // Darker color for roof
                    float value = 0.2f + noise;
                    texture.SetPixel(x, y, new Color(value, value, value));
                }
            }
            
            texture.Apply();
            texture.wrapMode = TextureWrapMode.Repeat;
            return texture;
        }
    }
}
