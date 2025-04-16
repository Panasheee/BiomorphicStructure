# Morphology Simulator Setup Guide

This guide will walk you through the process of setting up the Morphology Simulator project in Unity from scratch, configuring all necessary components, and running your first simulation.

## 1. Initial Setup

### Create a New Unity Project

1. Open Unity Hub
2. Click "New Project"
3. Select the "3D" template
4. Name your project "MorphologySimulator"
5. Choose a location for your project
6. Click "Create Project"

### Project Settings

1. Go to Edit > Project Settings
2. In the Quality settings, adjust settings for better performance:
   - Set Anti Aliasing to "4x Multi Sampling"
   - Enable Soft Particles
   - Set Texture Quality to "Full Res"
3. In the Physics settings:
   - Disable "Auto Simulation" (we'll control physics manually)
   - Set "Default Solver Iterations" to 10 for more stable simulations
4. In the Player settings:
   - Set "Color Space" to "Linear" for better visual quality
   - Enable "GPU Skinning"

## 2. Import Required Packages

1. Go to Window > Package Manager
2. Add the following packages (using the "+" button > "Add package by name"):
   - `com.unity.textmeshpro` (version 3.0.6 or newer)
   - `com.unity.postprocessing` (version 3.2.2 or newer)
   - `com.unity.mathematics` (version 1.2.6 or newer)
   - `com.unity.probuilder` (version 5.0.6 or newer)

## 3. Create Folder Structure

Create the following folder structure in your Assets folder:

```
Assets/
├── Scenes/
├── Scripts/
│   ├── Core/
│   ├── Morphology/
│   ├── UI/
│   └── Utilities/
├── Prefabs/
├── Materials/
├── Data/
└── Resources/
```

## 4. Setting Up Basic Materials

### Create Essential Materials

1. Create the following materials in the Materials folder:
   - NodeMaterial.mat (for morphology nodes)
   - ConnectionMaterial.mat (for connections between nodes)
   - SiteMaterial.mat (for the Wellington site)
   - StressMaterial.mat (for visualizing stress)
   - AdaptationMaterial.mat (for visualizing adaptation)

2. Configure the materials:
   - NodeMaterial: Set color to white, smoothness to 0.7
   - ConnectionMaterial: Set color to light gray, smoothness to 0.5
   - StressMaterial: Set color to red, enable emission, set emission color to red
   - AdaptationMaterial: Set color to green, enable emission, set emission color to green

## 5. Create the Main Scene

1. Create a new scene in the Scenes folder (File > New Scene)
2. Save it as "Main.scene"
3. Add the following to the scene:
   - A directional light (GameObject > Light > Directional Light)
   - A camera (GameObject > Camera)
   - An empty GameObject named "MorphologySimulator"

## 6. Copy Scripts

1. Copy all the script files from this repository into the corresponding folders in your project's Script folder.
2. Allow Unity to compile the scripts.

## 7. Setting Up the Core System

### Create Manager Objects

1. Create empty GameObjects for each manager under the MorphologySimulator object:
   - MorphologyManager
   - SiteGenerator
   - MorphologyGenerator
   - ScenarioAnalyzer
   - VisualizationManager
   - AdaptationSystem
   - DataIO
   - UIManager

2. Add the corresponding script component to each GameObject:
   - Add MorphologyManager.cs to the MorphologyManager object
   - Add SiteGenerator.cs to the SiteGenerator object
   - ...and so on for each manager

### Configure MainController

1. Create a new empty GameObject named "MainController"
2. Add the MainController.cs script to it
3. In the Inspector, assign all the manager references:
   - Drag each manager GameObject to the corresponding field in the MainController

## 8. Node and Connection Prefabs

### Create Node Prefab

1. Create a new Sphere GameObject (GameObject > 3D Object > Sphere)
2. Rename it to "NodePrototype"
3. Scale it down to 0.5 units
4. Assign the NodeMaterial to it
5. Add a Rigidbody component (Add Component > Physics > Rigidbody)
   - Enable "Is Kinematic"
6. Add a Sphere Collider component if not already present
7. Create a prefab by dragging it into the Prefabs folder
8. Delete the GameObject from the scene

### Create Connection Prefab

1. Create a new Cylinder GameObject (GameObject > 3D Object > Cylinder)
2. Rename it to "ConnectionPrototype"
3. Scale it to (0.1, 1, 0.1)
4. Assign the ConnectionMaterial to it
5. Create a prefab by dragging it into the Prefabs folder
6. Delete the GameObject from the scene

## 9. Setting Up the UI

### Create Canvas

1. Create a new UI Canvas (GameObject > UI > Canvas)
2. Set its Render Mode to "Screen Space - Overlay"
3. Add a Canvas Scaler component (Add Component > Layout > Canvas Scaler)
   - Set UI Scale Mode to "Scale With Screen Size"
   - Set Reference Resolution to 1920x1080

### Create UI Panels

1. Create the following panels as children of the Canvas:
   - MainPanel
   - SiteSelectionPanel
   - ZoneSelectionPanel
   - MorphologyControlPanel
   - ScenarioPanel
   - ResultsPanel
   - SettingsPanel

2. For each panel, add a Panel component, set its color to semi-transparent black (RGBA: 0, 0, 0, 0.8)

### Add UI Controls

For each panel, add appropriate UI controls:

1. **SiteSelectionPanel**:
   - Add a "Generate Site" button
   - Add sliders for detail level
   - Add toggles for including buildings, vegetation, roads

2. **ZoneSelectionPanel**:
   - Add sliders for zone width, height, depth
   - Add "Select Random Zone" and "Confirm Zone" buttons
   - Add a text field for zone info

3. **MorphologyControlPanel**:
   - Add sliders for density, complexity, connectivity
   - Add dropdowns for biomorph type and growth pattern
   - Add "Generate Morphology" and "Reset Morphology" buttons

4. **ScenarioPanel**:
   - Add a dropdown for scenario selection
   - Add a scrolling area for environmental factors
   - Add "Run Scenario" and "Stop Scenario" buttons

5. **ResultsPanel**:
   - Add a text field for results display
   - Add a container for graphs
   - Add an "Export Results" button

6. **SettingsPanel**:
   - Add sliders for simulation parameters
   - Add visualization controls
   - Add "Apply Settings" button

### Create Tab Buttons

1. Add a horizontal layout group at the top of the canvas
2. Add a button for each panel:
   - Site Button
   - Zone Button
   - Morphology Button
   - Scenario Button
   - Results Button
   - Settings Button

## 10. Configure Manager References

Now that we have all the components, we need to configure the references in the Inspector for each manager:

### MorphologyManager

1. Select the MorphologyManager GameObject
2. In the Inspector, assign:
   - Node Prototype: Drag the NodePrototype prefab
   - Connection Prototype: Drag the ConnectionPrototype prefab
   - Default Materials: Assign the materials created earlier

### SiteGenerator

1. Select the SiteGenerator GameObject
2. In the Inspector, assign:
   - Terrain Material: Assign the SiteMaterial
   - For testing, you can use placeholder textures for heightmap and road network

### MorphologyGenerator 

1. Select the MorphologyGenerator GameObject
2. In the Inspector, assign:
   - Node Prototype: Drag the NodePrototype prefab
   - Connection Prototype: Drag the ConnectionPrototype prefab 
   - Materials: Assign the different node and connection materials

### ScenarioAnalyzer

1. Select the ScenarioAnalyzer GameObject
2. In the Inspector, assign:
   - Standard Material: Assign the NodeMaterial
   - Stress Material: Assign the StressMaterial
   - Adaptation Material: Assign the AdaptationMaterial

### UIManager

1. Select the UIManager GameObject
2. In the Inspector, assign all the UI elements created earlier:
   - Main Panel: Drag the MainPanel
   - Site Selection Panel: Drag the SiteSelectionPanel
   - Zone Selection Panel: Drag the ZoneSelectionPanel
   - ...and so on for all UI elements

### VisualizationManager

1. Select the VisualizationManager GameObject
2. In the Inspector, assign:
   - Main Camera: Drag the Main Camera from the scene
   - Main Light: Drag the Directional Light from the scene
   - Visualization Styles: Add a few styles and configure them

## 11. Add Test Data for Wellington Lambton Quay

For testing purposes, create placeholder data for Wellington Lambton Quay:

1. Create a new folder in Data called "Wellington"
2. If you have actual GIS data for Wellington, place it here
3. Otherwise, create:
   - A simple heightmap texture (512x512 pixels)
   - A building map texture (512x512 pixels)
   - A simple text file with placeholder coordinates

## 12. Connect Everything Through the MainController

1. Select the MainController GameObject
2. In the Inspector, assign:
   - All manager references
   - Wellington Lambton Quay data references
   - Default Settings

## 13. Create Default Settings

1. Select the MorphologyManager
2. In the Inspector, configure default settings for:
   - Site Generation
   - Morphology Generation
   - Scenario Analysis

## 14. Test Run

1. Press Play to run the simulation in the editor
2. Test the site generation functionality
3. Test zone selection
4. Test morphology generation
5. Test scenario analysis

## 15. Troubleshooting Common Issues

### Scripts Not Finding References

If you see error messages about null references:
1. Check that all prefabs and materials are correctly assigned in the Inspector
2. Verify that all required manager GameObjects are in the scene
3. Check that script execution order is set correctly (Edit > Project Settings > Script Execution Order)

### Performance Issues

If the simulation runs slowly:
1. Reduce the maximum node count in MorphologySettings
2. Disable real-time visualization during generation
3. Reduce the physics update frequency

### Visual Glitches

If you encounter visual artifacts:
1. Check material settings, especially transparency
2. Verify that node and connection scales are appropriate
3. Try changing the visualization style

## 16. Sample Implementation Steps

Here's a typical workflow to test that everything is working:

1. Press Play in the Unity editor
2. Click "Generate Site" in the Site Selection panel
3. Select a zone using the sliders or "Select Random Zone" button
4. Click "Confirm Zone"
5. In the Morphology panel, set:
   - Density: 0.5
   - Complexity: 0.6
   - Connectivity: 0.5
   - Biomorph Type: Mold
   - Growth Pattern: Organic
6. Click "Generate Morphology"
7. Once generation is complete, go to the Scenario panel
8. Select "Wind Adaptation" from the dropdown
9. Click "Run Scenario"
10. View the results in the Results panel

If all these steps work without errors, your setup is successful!

## 17. Next Steps

Now that you have the basic system working, you can:

1. Refine the Wellington Lambton Quay data for more accurate site representation
2. Create custom visualization styles
3. Implement additional biomorphic algorithms
4. Create custom environmental scenarios
5. Enhance the UI with more detailed controls and visualizations

## Support

If you encounter issues not covered in this guide, refer to the project's README.md or contact the repository maintainer for assistance.