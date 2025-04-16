using UnityEngine;
using UnityEngine.UIElements;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Custom UI elements for BiomorphicSim UI Toolkit implementation
    /// </summary>
    
    // Editable Label (replacing ArcGISEditableLabel)
    [UxmlElement]
    public partial class EditableLabel : Label
    {
        // Constructor handles initialization
        public EditableLabel()
        {
            this.RegisterCallback<MouseDownEvent>(OnMouseDown);
        }
        
        private void OnMouseDown(MouseDownEvent evt)
        {
            if (evt.clickCount == 2)
            {
                // Double-click to enable editing
                // Implement your edit functionality here
                Debug.Log("Double-clicked to edit: " + text);
            }
        }
    }
    
    // Any additional custom UI elements you need would be added below with [UxmlElement] attributes
    [UxmlElement]
    public partial class BiomorphicButton : Button
    {
        public BiomorphicButton()
        {
            // Apply your standard styling
            AddToClassList("biomorphic-button");
        }
    }
    
    [UxmlElement]
    public partial class BiomorphicPanel : VisualElement
    {
        public BiomorphicPanel()
        {
            AddToClassList("biomorphic-panel");
        }
    }
    
    // Add more custom elements as needed
}
