## Setting Up ArcGIS Map in BiomorphicSim

### Instructions

1. In Unity Editor, first create an ArcGIS Map GameObject:
   - Right-click in the Hierarchy > Create Empty
   - Rename it to "ArcGISMap"
   
2. Add the required ArcGIS components to the GameObject:
   - Select the ArcGISMap GameObject
   - Add Component > ArcGIS Maps SDK > ArcGIS Map View
   - Add Component > ArcGIS Maps SDK > ArcGIS Camera Component
   - Add Component > ArcGIS Maps SDK > ArcGIS Rendering Component
   - Add Component > BiomorphicSim > Map > MapSetup

3. Locate your existing Site_Terrain GameObject (the one with the magenta material)
   - Disable it or hide it (don't delete it in case you need to revert)
   
4. Update the ModernUIController GameObject's references:
   - Assign the ArcGISMap GameObject to the "Arcgis Map" field
   - Assign the Site_Terrain GameObject to the "Site Terrain" field

5. Test the implementation by pressing Play
   - The map should automatically load the satellite imagery for Wellington
   - If you encounter any issues, check the console for error messages
