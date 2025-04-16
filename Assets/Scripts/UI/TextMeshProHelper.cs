using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Helper class for creating and configuring TextMeshPro UI elements
    /// </summary>
    public static class TextMeshProHelper
    {
        /// <summary>
        /// Creates a TextMeshProUGUI component on a GameObject
        /// </summary>
        public static TextMeshProUGUI CreateText(
            GameObject parent, 
            string text, 
            TextAlignmentOptions alignment = TextAlignmentOptions.Center,
            float fontSize = 14f,
            Color? color = null)
        {
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(parent.transform, false);
            
            RectTransform textRect = textObj.AddComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            TextMeshProUGUI tmpText = textObj.AddComponent<TextMeshProUGUI>();
            tmpText.text = text;
            tmpText.color = color ?? Color.white;
            tmpText.alignment = alignment;
            tmpText.fontSize = fontSize;
            
            return tmpText;
        }
        
        /// <summary>
        /// Creates a button with TextMeshProUGUI label
        /// </summary>
        public static Button CreateButton(
            Transform parent,
            string text,
            Vector2 anchorMin,
            Vector2 anchorMax,
            TextAlignmentOptions textAlignment = TextAlignmentOptions.Center,
            float fontSize = 14f,
            Color? backgroundColor = null,
            Color? textColor = null)
        {
            GameObject buttonObj = new GameObject(text + "Button");
            buttonObj.transform.SetParent(parent, false);
            
            RectTransform rectTransform = buttonObj.AddComponent<RectTransform>();
            rectTransform.anchorMin = anchorMin;
            rectTransform.anchorMax = anchorMax;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;
            
            Image image = buttonObj.AddComponent<Image>();
            image.color = backgroundColor ?? new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            Button button = buttonObj.AddComponent<Button>();
            button.transition = Selectable.Transition.ColorTint;
            
            // Add text
            CreateText(buttonObj, text, textAlignment, fontSize, textColor);
            
            return button;
        }
    }
}
