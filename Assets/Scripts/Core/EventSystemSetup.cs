using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// This component ensures there is exactly one EventSystem in the scene.
/// Place this on the same GameObject as your BiomorphicSimulatorBootstrap.
/// </summary>
public class EventSystemSetup : MonoBehaviour
{
    // Awake is called before any Start methods, so this will ensure we have an EventSystem
    // before any UI interactions are attempted
    private void Awake()
    {
        // First remove any existing EventSystems to avoid conflicts
        EventSystem[] existingSystems = FindObjectsOfType<EventSystem>();
        if (existingSystems.Length > 1)
        {
            Debug.Log($"Found {existingSystems.Length} EventSystems - removing duplicates");
            // Keep only the first one
            for (int i = 1; i < existingSystems.Length; i++)
            {
                DestroyImmediate(existingSystems[i].gameObject);
            }
        }
        
        // If no EventSystem exists, create one
        if (existingSystems.Length == 0)
        {
            Debug.Log("No EventSystem found - creating a new one");
            GameObject eventSystem = new GameObject("EventSystem");
            eventSystem.AddComponent<EventSystem>();
            eventSystem.AddComponent<StandaloneInputModule>();
            
            // Set it as a child of this GameObject to keep hierarchy organized
            eventSystem.transform.SetParent(transform);
        }
        else
        {
            Debug.Log("Using existing EventSystem");
        }
    }
}
