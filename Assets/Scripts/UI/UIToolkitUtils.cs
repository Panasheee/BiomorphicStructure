using UnityEngine;
using UnityEngine.UIElements;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Utility class for UI Toolkit related functionality
    /// </summary>
    public static class UIToolkitUtils
    {
        /// <summary>
        /// Applies standard text settings to a TextMeshProUGUI component
        /// </summary>
        public static void ApplyStandardTextSettings(TMPro.TextMeshProUGUI textComponent, 
            string text = "", 
            TextAnchor alignment = TextAnchor.MiddleCenter,
            float fontSize = 14f,
            Color? color = null)
        {
            if (textComponent == null) return;
            
            textComponent.text = text;
            textComponent.color = color ?? Color.white;
            textComponent.fontSize = fontSize;
            
            // Convert Unity's TextAnchor to TextMeshPro's TextAlignmentOptions
            textComponent.alignment = alignment switch
            {
                TextAnchor.UpperLeft => TMPro.TextAlignmentOptions.TopLeft,
                TextAnchor.UpperCenter => TMPro.TextAlignmentOptions.Top,
                TextAnchor.UpperRight => TMPro.TextAlignmentOptions.TopRight,
                TextAnchor.MiddleLeft => TMPro.TextAlignmentOptions.Left,
                TextAnchor.MiddleCenter => TMPro.TextAlignmentOptions.Center,
                TextAnchor.MiddleRight => TMPro.TextAlignmentOptions.Right,
                TextAnchor.LowerLeft => TMPro.TextAlignmentOptions.BottomLeft,
                TextAnchor.LowerCenter => TMPro.TextAlignmentOptions.Bottom,
                TextAnchor.LowerRight => TMPro.TextAlignmentOptions.BottomRight,
                _ => TMPro.TextAlignmentOptions.Center
            };
        }
    }
}
