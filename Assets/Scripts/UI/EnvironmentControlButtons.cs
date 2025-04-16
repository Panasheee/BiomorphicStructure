using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Handles environment control button functionality (Wind, Sun, Pedestrian)
    /// </summary>
    public class EnvironmentControlButtons : MonoBehaviour
    {
        [Header("Button References")]
        [SerializeField] private Button windButton;
        [SerializeField] private Button sunButton;
        [SerializeField] private Button pedestrianButton;
        
        [Header("Visual Feedback")]
        [SerializeField] private Color enabledColor = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        [SerializeField] private Color disabledColor = new Color(0.6f, 0.2f, 0.2f, 0.8f);
        
        // State tracking
        private bool windEnabled = false;
        private bool sunEnabled = false;
        private bool pedestrianEnabled = false;
        
        // Cached button images
        private Image windButtonImage;
        private Image sunButtonImage;
        private Image pedestrianButtonImage;
        
        private void Awake()
        {
            // Find buttons if not assigned in Inspector
            if (windButton == null)
                windButton = transform.Find("WindToggleButton")?.GetComponent<Button>();
            
            if (sunButton == null)
                sunButton = transform.Find("SunToggleButton")?.GetComponent<Button>();
                
            if (pedestrianButton == null)
                pedestrianButton = transform.Find("PedestrianToggleButton")?.GetComponent<Button>();
            
            // Register button callbacks
            SetupButtons();
        }
        
        private void SetupButtons()
        {
            // Setup Wind button
            if (windButton != null)
            {
                windButtonImage = windButton.GetComponent<Image>();
                windButton.onClick.AddListener(ToggleWind);
                
                // Get text component to update
                TextMeshProUGUI windText = windButton.GetComponentInChildren<TextMeshProUGUI>();
                if (windText != null)
                    windText.text = "WIND OFF";
            }
            
            // Setup Sun button
            if (sunButton != null)
            {
                sunButtonImage = sunButton.GetComponent<Image>();
                sunButton.onClick.AddListener(ToggleSun);
                
                // Get text component to update
                TextMeshProUGUI sunText = sunButton.GetComponentInChildren<TextMeshProUGUI>();
                if (sunText != null)
                    sunText.text = "SUN OFF";
            }
            
            // Setup Pedestrian button
            if (pedestrianButton != null)
            {
                pedestrianButtonImage = pedestrianButton.GetComponent<Image>();
                pedestrianButton.onClick.AddListener(TogglePedestrian);
                
                // Get text component to update
                TextMeshProUGUI pedText = pedestrianButton.GetComponentInChildren<TextMeshProUGUI>();
                if (pedText != null)
                    pedText.text = "PEDESTRIAN OFF";
            }
        }
        
        public void ToggleWind()
        {
            windEnabled = !windEnabled;
            Debug.Log($"<color=cyan>Wind simulation {(windEnabled ? "enabled" : "disabled")}</color>");
            
            // Update visual feedback
            if (windButtonImage != null)
            {
                windButtonImage.color = windEnabled ? enabledColor : disabledColor;
                
                // Update button text
                TextMeshProUGUI windText = windButton.GetComponentInChildren<TextMeshProUGUI>();
                if (windText != null)
                    windText.text = windEnabled ? "WIND ON" : "WIND OFF";
            }
            
            // TODO: Implement actual wind simulation effect
            // For now we'll just display a temporary visual indicator
            if (windEnabled)
                StartCoroutine(ShowWindEffect());
        }
        
        public void ToggleSun()
        {
            sunEnabled = !sunEnabled;
            Debug.Log($"<color=yellow>Sun analysis {(sunEnabled ? "enabled" : "disabled")}</color>");
            
            // Update visual feedback
            if (sunButtonImage != null)
            {
                sunButtonImage.color = sunEnabled ? enabledColor : disabledColor;
                
                // Update button text
                TextMeshProUGUI sunText = sunButton.GetComponentInChildren<TextMeshProUGUI>();
                if (sunText != null)
                    sunText.text = sunEnabled ? "SUN ON" : "SUN OFF";
            }
            
            // Simple sun light effect adjustment
            Light[] lights = FindObjectsOfType<Light>();
            foreach (Light light in lights)
            {
                if (light.type == LightType.Directional)
                {
                    light.intensity = sunEnabled ? 1.5f : 0.8f;
                    if (sunEnabled)
                    {
                        // Simulate a more direct sunlight angle
                        light.transform.rotation = Quaternion.Euler(50, -30, 0);
                    }
                    else
                    {
                        // Return to default angle
                        light.transform.rotation = Quaternion.Euler(50, 30, 0);
                    }
                }
            }
        }
        
        public void TogglePedestrian()
        {
            pedestrianEnabled = !pedestrianEnabled;
            Debug.Log($"<color=magenta>Pedestrian analysis {(pedestrianEnabled ? "enabled" : "disabled")}</color>");
            
            // Update visual feedback
            if (pedestrianButtonImage != null)
            {
                pedestrianButtonImage.color = pedestrianEnabled ? enabledColor : disabledColor;
                
                // Update button text
                TextMeshProUGUI pedText = pedestrianButton.GetComponentInChildren<TextMeshProUGUI>();
                if (pedText != null)
                    pedText.text = pedestrianEnabled ? "PEDESTRIAN ON" : "PEDESTRIAN OFF";
            }
            
            // TODO: Implement actual pedestrian simulation
            // For now, just show a debug message
            if (pedestrianEnabled)
            {
                // Create a temporary UI message showing this feature is in development
                StartCoroutine(ShowPedestrianAnalysis());
            }
        }
        
        private System.Collections.IEnumerator ShowWindEffect()
        {
            // Create a temporary particle system to visualize wind
            GameObject windEffect = new GameObject("WindVisualization");
            windEffect.transform.position = Camera.main.transform.position + Camera.main.transform.forward * 10;
            
            ParticleSystem particles = windEffect.AddComponent<ParticleSystem>();
            var main = particles.main;
            main.startSpeed = 5.0f;
            main.startSize = 0.2f;
            main.startLifetime = 4.0f;
            
            var emission = particles.emission;
            emission.rateOverTime = 50;
            
            var shape = particles.shape;
            shape.shapeType = ParticleSystemShapeType.Box;
            shape.scale = new Vector3(20, 10, 0.1f);
            
            // Set the particle system to emit in the direction of the wind
            particles.transform.rotation = Quaternion.Euler(0, 45, 0);
            
            var renderer = particles.GetComponent<ParticleSystemRenderer>();
            renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
            renderer.material.color = new Color(0.7f, 0.9f, 1.0f, 0.3f);
            
            // Keep wind visualization active for as long as wind is enabled
            while (windEnabled)
            {
                yield return null;
            }
            
            // Clean up when wind is disabled
            Destroy(windEffect);
        }
        
        private System.Collections.IEnumerator ShowPedestrianAnalysis()
        {
            // Create a temporary UI message
            GameObject messageObject = new GameObject("PedestrianMessage");
            messageObject.transform.SetParent(transform, false);
            
            RectTransform rect = messageObject.AddComponent<RectTransform>();
            rect.anchorMin = new Vector2(0.3f, 0.4f);
            rect.anchorMax = new Vector2(0.7f, 0.6f);
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image background = messageObject.AddComponent<Image>();
            background.color = new Color(0, 0, 0, 0.8f);
            
            GameObject textObject = new GameObject("Message");
            textObject.transform.SetParent(messageObject.transform, false);
            
            RectTransform textRect = textObject.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = new Vector2(20, 20);
            textRect.offsetMax = new Vector2(-20, -20);
            
            TextMeshProUGUI text = textObject.AddComponent<TextMeshProUGUI>();
            text.text = "Pedestrian Flow Analysis\nCalculating optimal paths...";
            text.fontSize = 18;
            text.alignment = TextAlignmentOptions.Center;
            text.color = Color.white;
            
            // Display for 3 seconds then fade out
            yield return new WaitForSeconds(3f);
            
            // Fade out
            float alpha = 1.0f;
            while (alpha > 0)
            {
                alpha -= Time.deltaTime;
                background.color = new Color(0, 0, 0, alpha * 0.8f);
                text.color = new Color(1, 1, 1, alpha);
                yield return null;
            }
            
            Destroy(messageObject);
        }
    }
}
