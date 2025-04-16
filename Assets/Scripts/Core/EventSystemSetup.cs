using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Ensures that a proper EventSystem exists in the scene
    /// </summary>
    public class EventSystemSetup : MonoBehaviour
    {
        void Awake()
        {
            // Check if EventSystem exists
            if (FindObjectOfType<EventSystem>() == null)
            {
                // Create EventSystem
                GameObject eventSystem = new GameObject("EventSystem");
                eventSystem.AddComponent<EventSystem>();
                eventSystem.AddComponent<StandaloneInputModule>();
                Debug.Log("Created EventSystem automatically");
            }
        }
    }
}
