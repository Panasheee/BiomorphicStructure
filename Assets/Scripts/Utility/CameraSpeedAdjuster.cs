using UnityEngine;

namespace BiomorphicSim.Utility
{
    /// <summary>
    /// Simple utility to modify camera movement speed
    /// </summary>
    [DefaultExecutionOrder(-200)] // Run very early
    public class CameraSpeedAdjuster : MonoBehaviour
    {
        [SerializeField] private float speedMultiplier = 3.0f;
        [SerializeField] private bool applyOnStart = true;
        
        private void Start()
        {
            if (applyOnStart)
            {
                IncreaseMovementSpeed();
            }
        }
        
        public void IncreaseMovementSpeed()
        {
            // Look for standard Unity camera controllers
            // First we'll try the standard FPS controller
            var fpsController = FindObjectOfType<UnityEngine.CharacterController>();
            if (fpsController != null)
            {
                var controller = fpsController.GetComponent<MonoBehaviour>();
                if (controller != null)
                {
                    // Try to find speed variables through reflection
                    Debug.Log($"Found character controller: {controller.GetType().Name}");
                    IncreaseSpeedViaReflection(controller);
                    return;
                }
            }
            
            // Try to find any custom camera controllers
            MonoBehaviour[] scripts = FindObjectsOfType<MonoBehaviour>();
            foreach (MonoBehaviour script in scripts)
            {
                string typeName = script.GetType().Name.ToLower();
                
                // Look for likely camera controller scripts
                if (typeName.Contains("camera") && (typeName.Contains("control") || 
                    typeName.Contains("movement") || typeName.Contains("navigate")))
                {
                    Debug.Log($"Found potential camera controller: {script.GetType().Name}");
                    IncreaseSpeedViaReflection(script);
                }
                
                // Also check for first person controllers
                if ((typeName.Contains("fps") || typeName.Contains("first") || 
                     typeName.Contains("person")) && typeName.Contains("control"))
                {
                    Debug.Log($"Found potential FPS controller: {script.GetType().Name}");
                    IncreaseSpeedViaReflection(script);
                }
            }
        }
        
        private void IncreaseSpeedViaReflection(MonoBehaviour script)
        {
            // Get all fields in the script
            var fields = script.GetType().GetFields(System.Reflection.BindingFlags.Instance | 
                                                     System.Reflection.BindingFlags.Public | 
                                                     System.Reflection.BindingFlags.NonPublic);
            
            foreach (var field in fields)
            {
                string fieldName = field.Name.ToLower();
                
                // Look for fields that might control movement speed
                if ((fieldName.Contains("speed") || fieldName.Contains("velocity") || 
                     fieldName.Contains("movement")) && !fieldName.Contains("rotation"))
                {
                    // Make sure it's a numeric type
                    if (field.FieldType == typeof(float))
                    {
                        float currentValue = (float)field.GetValue(script);
                        float newValue = currentValue * speedMultiplier;
                        field.SetValue(script, newValue);
                        Debug.Log($"<color=green>Increased {script.GetType().Name}.{field.Name} " +
                                  $"from {currentValue} to {newValue}</color>");
                    }
                    else if (field.FieldType == typeof(int))
                    {
                        int currentValue = (int)field.GetValue(script);
                        int newValue = Mathf.RoundToInt(currentValue * speedMultiplier);
                        field.SetValue(script, newValue);
                        Debug.Log($"<color=green>Increased {script.GetType().Name}.{field.Name} " +
                                  $"from {currentValue} to {newValue}</color>");
                    }
                    else if (field.FieldType == typeof(Vector3))
                    {
                        Vector3 currentValue = (Vector3)field.GetValue(script);
                        Vector3 newValue = currentValue * speedMultiplier;
                        field.SetValue(script, newValue);
                        Debug.Log($"<color=green>Increased {script.GetType().Name}.{field.Name} " +
                                  $"from {currentValue} to {newValue}</color>");
                    }
                }
            }
        }
    }
}
