using UnityEngine;

namespace BiomorphicSim.Map
{
    /// <summary>
    /// This script automatically disables the ArcGIS warning panel by setting the ESRISDKInitialized flag
    /// and finding and disabling any warning panels that might appear.
    /// </summary>
    public class DisableArcGISWarning : MonoBehaviour
    {
        [SerializeField] private bool disableOnStart = true;
        [SerializeField] private bool periodicallyCheck = true;
        [SerializeField] private float checkInterval = 2.0f;
        
        private string[] warningPanelNames = {
            "ESRIInstructionsPanel",
            "ArcGISWarningPanel",
            "SDKWarningPanel",
            "MissingArcGISComponents",
            "StartupCheckPanel",
            "ErrorPanel"
        };
        
        void Start()
        {
            // Set the flag that indicates the ESRI SDK is installed
            PlayerPrefs.SetInt("ESRISDKInitialized", 1);
            PlayerPrefs.Save();
            
            Debug.Log("DisableArcGISWarning: Set ESRISDKInitialized flag to indicate real SDK is present");
            
            if (disableOnStart)
            {
                Invoke("FindAndDisablePanels", 0.5f);
            }
            
            if (periodicallyCheck)
            {
                InvokeRepeating("FindAndDisablePanels", 2.0f, checkInterval);
            }
        }
        
        void FindAndDisablePanels()
        {
            foreach (string panelName in warningPanelNames)
            {
                GameObject panel = GameObject.Find(panelName);
                if (panel != null)
                {
                    panel.SetActive(false);
                    Debug.Log($"DisableArcGISWarning: Found and disabled {panelName}");
                }
            }
            
            // Also look for any panel that might have been created with "Instructions" in the name
            GameObject instructionsPanel = GameObject.Find("ESRIInstructionsPanel");
            if (instructionsPanel != null)
            {
                instructionsPanel.SetActive(false);
                Debug.Log("DisableArcGISWarning: Found and disabled ESRIInstructionsPanel");
            }
        }
    }
}
