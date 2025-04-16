using UnityEngine;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Simple component to rotate a UI element continuously, typically used for loading spinners.
    /// </summary>
    public class LoadingSpinner : MonoBehaviour
    {
        /// <summary>
        /// The rotation speed in degrees per second.
        /// </summary>
        public float rotationSpeed = 360f;
        
        private RectTransform rectTransform;
        
        private void Awake()
        {
            rectTransform = GetComponent<RectTransform>();
        }
        
        private void Update()
        {
            if (rectTransform != null)
            {
                // Rotate around the Z axis continuously
                rectTransform.Rotate(0, 0, -rotationSpeed * Time.deltaTime);
            }
        }
    }
}
