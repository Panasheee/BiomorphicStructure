// filepath: c:\Users\tyron\BiomorphicSim\Assets\Scripts\Core\Billboard.cs
using UnityEngine;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Makes an object always face the camera
    /// Used for building and street name labels
    /// </summary>
    public class Billboard : MonoBehaviour
    {
        private Camera mainCamera;
        
        private void Start()
        {
            // Find the main camera
            mainCamera = Camera.main;
        }
        
        private void LateUpdate()
        {
            if (mainCamera == null)
            {
                mainCamera = Camera.main;
                if (mainCamera == null)
                    return;
            }
            
            // Make the object face the camera
            transform.LookAt(transform.position + mainCamera.transform.rotation * Vector3.forward,
                mainCamera.transform.rotation * Vector3.up);
        }
    }
}
