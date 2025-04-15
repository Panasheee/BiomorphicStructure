using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using BiomorphicSim.Core; // For Core types
using BiomorphicSim.Utilities; // For utility classes

/// <summary>
/// Handles saving and loading of simulation data to/from files.
/// Supports morphology data, scenario results, and configuration settings.
/// </summary>
public class DataIO : MonoBehaviour
{
    [Header("File Settings")]
    [SerializeField] private string saveFolder = "MorphologySimulator";
    [SerializeField] private string morphologyExtension = ".morph";
    [SerializeField] private string scenarioExtension = ".scenario";
    [SerializeField] private string resultsExtension = ".results";
    [SerializeField] private string settingsExtension = ".config";
    
    // Singleton instance
    private static DataIO instance;
    
    // Property for accessing the singleton
    public static DataIO Instance
    {        get
        {
            if (instance == null)
            {
                instance = FindFirstObjectByType<DataIO>();
                
                if (instance == null)
                {
                    GameObject obj = new GameObject("DataIO");
                    instance = obj.AddComponent<DataIO>();
                }
            }
            
            return instance;
        }
    }
    
    private void Awake()
    {
        // Ensure singleton behavior
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        instance = this;
        DontDestroyOnLoad(gameObject);
        
        // Ensure save directory exists
        EnsureSaveDirectoryExists();
    }
    
    /// <summary>
    /// Saves morphology data to a file
    /// </summary>
    /// <param name="morphologyData">The data to save</param>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if save successful</returns>
    public bool SaveMorphology(MorphologyData morphologyData, string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, morphologyExtension);
            
            // Convert data to JSON
            string jsonData = JsonUtility.ToJson(morphologyData, true);
            
            // Fix for Dictionary serialization as it's not directly supported by JsonUtility
            if (morphologyData.metrics != null)
            {
                List<SerializableKeyValuePair> metricsList = new List<SerializableKeyValuePair>();
                foreach (var pair in morphologyData.metrics)
                {
                    metricsList.Add(new SerializableKeyValuePair { Key = pair.Key, Value = pair.Value });
                }
                
                SerializableDictionary serializableMetrics = new SerializableDictionary { Items = metricsList };
                string metricsJson = JsonUtility.ToJson(serializableMetrics);
                
                // Insert metrics JSON into main JSON
                jsonData = InsertPropertyIntoJson(jsonData, "metrics", metricsJson);
            }
            
            // Write to file
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"Morphology saved to {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving morphology: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads morphology data from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>The loaded morphology data, or null if loading failed</returns>
    public MorphologyData LoadMorphology(string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, morphologyExtension);
            
            // Check if file exists
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Morphology file not found: {filePath}");
                return null;
            }
            
            // Read JSON from file
            string jsonData = File.ReadAllText(filePath);
            // Instead of using the unqualified type, fully qualify it:
            BiomorphicSim.Core.MorphologyData morphologyData = JsonUtility.FromJson<BiomorphicSim.Core.MorphologyData>(jsonData);
            return morphologyData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading morphology: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Saves scenario data to a file
    /// </summary>  
    /// <param name="scenarioData">The data to save</param>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if save successful</returns>
    public bool SaveScenario(ScenarioData scenarioData, string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, scenarioExtension);
            
            // Convert data to JSON
            string jsonData = JsonUtility.ToJson(scenarioData, true);
            
            // Fix for Dictionary serialization
            if (scenarioData.environmentalFactors != null)
            {
                List<SerializableKeyValuePair> factorsList = new List<SerializableKeyValuePair>();
                foreach (var pair in scenarioData.environmentalFactors)
                {
                    factorsList.Add(new SerializableKeyValuePair { Key = pair.Key, Value = pair.Value });
                }
                
                SerializableDictionary serializableFactors = new SerializableDictionary { Items = factorsList };
                string factorsJson = JsonUtility.ToJson(serializableFactors);
                
                // Insert factors JSON into main JSON
                jsonData = InsertPropertyIntoJson(jsonData, "environmentalFactors", factorsJson);
            }
            
            // Write to file
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"Scenario saved to {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving scenario: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads scenario data from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>The loaded scenario data, or null if loading failed</returns>
    public ScenarioData LoadScenario(string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, scenarioExtension);
            
            // Check if file exists
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Scenario file not found: {filePath}");
                return null;
            }
            
            // Read JSON from file
            string jsonData = File.ReadAllText(filePath);
            
            // Parse JSON to object
            ScenarioData scenarioData = JsonUtility.FromJson<ScenarioData>(jsonData);
            
            // Fix for Dictionary deserialization
            if (jsonData.Contains("\"environmentalFactors\":"))
            {
                string factorsJson = ExtractPropertyFromJson(jsonData, "environmentalFactors");
                if (!string.IsNullOrEmpty(factorsJson))
                {
                    SerializableDictionary serializableFactors = JsonUtility.FromJson<SerializableDictionary>(factorsJson);
                    
                    scenarioData.environmentalFactors = new Dictionary<string, float>();
                    foreach (var item in serializableFactors.Items)
                    {
                        scenarioData.environmentalFactors[item.Key] = item.Value;
                    }
                }
            }
            
            Debug.Log($"Scenario loaded from {filePath}");
            return scenarioData;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading scenario: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Saves scenario results to a file
    /// </summary>
    /// <param name="results">The results to save</param>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if save successful</returns>
    public bool SaveResults(ScenarioResults results, string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, resultsExtension);
            
            // Convert to JSON
            string jsonData = JsonUtility.ToJson(results, true);
            
            // Fix for Dictionary serialization
            // For metrics dictionary
            if (results.metrics != null)
            {
                List<SerializableKeyValuePair> metricsList = new List<SerializableKeyValuePair>();
                foreach (var pair in results.metrics)
                {
                    metricsList.Add(new SerializableKeyValuePair { Key = pair.Key, Value = pair.Value });
                }
                
                SerializableDictionary serializableMetrics = new SerializableDictionary { Items = metricsList };
                string metricsJson = JsonUtility.ToJson(serializableMetrics);
                
                // Insert metrics JSON into main JSON
                jsonData = InsertPropertyIntoJson(jsonData, "metrics", metricsJson);
            }
            
            // For time series data
            if (results.timeSeriesData != null)
            {
                List<SerializableTimeSeries> timeSeriesList = new List<SerializableTimeSeries>();
                foreach (var pair in results.timeSeriesData)
                {
                    timeSeriesList.Add(new SerializableTimeSeries { Key = pair.Key, Values = pair.Value.ToArray() });
                }
                
                SerializableTimeSeriesData serializableTimeSeries = new SerializableTimeSeriesData { Items = timeSeriesList };
                string timeSeriesJson = JsonUtility.ToJson(serializableTimeSeries);
                
                // Insert time series JSON into main JSON
                jsonData = InsertPropertyIntoJson(jsonData, "timeSeriesData", timeSeriesJson);
            }
            
            // Write to file
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"Results saved to {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving results: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads scenario results from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>The loaded scenario results, or null if loading failed</returns>
    public ScenarioResults LoadResults(string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, resultsExtension);
            
            // Check if file exists
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Results file not found: {filePath}");
                return null;
            }
            
            // Read JSON from file
            string jsonData = File.ReadAllText(filePath);
            
            // Parse JSON to object
            ScenarioResults results = JsonUtility.FromJson<ScenarioResults>(jsonData);
            
            // Fix for Dictionary deserialization
            // For metrics dictionary
            if (jsonData.Contains("\"metrics\":"))
            {
                string metricsJson = ExtractPropertyFromJson(jsonData, "metrics");
                if (!string.IsNullOrEmpty(metricsJson))
                {
                    SerializableDictionary serializableMetrics = JsonUtility.FromJson<SerializableDictionary>(metricsJson);
                    
                    results.metrics = new Dictionary<string, float>();
                    foreach (var item in serializableMetrics.Items)
                    {
                        results.metrics[item.Key] = item.Value;
                    }
                }
            }
            
            // For time series data
            if (jsonData.Contains("\"timeSeriesData\":"))
            {
                string timeSeriesJson = ExtractPropertyFromJson(jsonData, "timeSeriesData");
                if (!string.IsNullOrEmpty(timeSeriesJson))
                {
                    SerializableTimeSeriesData serializableTimeSeries = JsonUtility.FromJson<SerializableTimeSeriesData>(timeSeriesJson);
                    
                    results.timeSeriesData = new Dictionary<string, List<float>>();
                    foreach (var item in serializableTimeSeries.Items)
                    {
                        results.timeSeriesData[item.Key] = new List<float>(item.Values);
                    }
                }
            }
            
            Debug.Log($"Results loaded from {filePath}");
            return results;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading results: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Saves simulation settings to a file
    /// </summary>
    /// <param name="settings">The settings to save</param>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if save successful</returns>
    public bool SaveSettings(SimulationSettings settings, string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, settingsExtension);
            
            // Convert to JSON
            string jsonData = JsonUtility.ToJson(settings, true);
            
            // Write to file
            File.WriteAllText(filePath, jsonData);
            
            Debug.Log($"Settings saved to {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving settings: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads simulation settings from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>The loaded settings, or null if loading failed</returns>
    public SimulationSettings LoadSettings(string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = GetSaveFilePath(fileName, settingsExtension);
            
            // Check if file exists
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Settings file not found: {filePath}");
                return null;
            }
            
            // Read JSON from file
            string jsonData = File.ReadAllText(filePath);
            
            // Parse JSON to object
            SimulationSettings settings = JsonUtility.FromJson<SimulationSettings>(jsonData);
            
            Debug.Log($"Settings loaded from {filePath}");
            return settings;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading settings: {e.Message}");
            return null;
        }
    }
    
    /// <summary>
    /// Gets a list of all saved morphology files
    /// </summary>
    /// <returns>Array of file names (without extensions)</returns>
    public string[] GetSavedMorphologyFiles()
    {
        return GetSavedFiles(morphologyExtension);
    }
    
    /// <summary>
    /// Gets a list of all saved scenario files
    /// </summary>
    /// <returns>Array of file names (without extensions)</returns>
    public string[] GetSavedScenarioFiles()
    {
        return GetSavedFiles(scenarioExtension);
    }
    
    /// <summary>
    /// Gets a list of all saved results files
    /// </summary>
    /// <returns>Array of file names (without extensions)</returns>
    public string[] GetSavedResultsFiles()
    {
        return GetSavedFiles(resultsExtension);
    }
    
    /// <summary>
    /// Gets a list of all saved settings files
    /// </summary>
    /// <returns>Array of file names (without extensions)</returns>
    public string[] GetSavedSettingsFiles()
    {
        return GetSavedFiles(settingsExtension);
    }
    
    /// <summary>
    /// Exports a morphology to a mesh file for external use
    /// </summary>
    /// <param name="morphologyData">Morphology data to export</param>
    /// <param name="fileName">File name for the export</param>
    /// <param name="format">Export format (OBJ, STL, etc.)</param>
    /// <returns>True if export was successful</returns>
    public bool ExportMorphologyToMesh(MorphologyData morphologyData, string fileName, string format)
    {
        try
        {
            // Create mesh from morphology data
            Mesh mesh = GenerateMeshFromMorphology(morphologyData);
            
            // Determine file extension based on format
            string extension = "." + format.ToLower();
            
            // Create the full file path
            string filePath = Path.Combine(Application.persistentDataPath, "Exports", fileName + extension);
            
            // Ensure the exports directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            // Export based on format
            if (format.ToUpper() == "OBJ")
            {
                ExportToOBJ(mesh, filePath);
            }
            else if (format.ToUpper() == "STL")
            {
                ExportToSTL(mesh, filePath);
            }
            else
            {
                Debug.LogError($"Unsupported export format: {format}");
                return false;
            }
            
            Debug.Log($"Morphology exported to {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error exporting morphology: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Exports data as CSV for analysis in external applications
    /// </summary>
    /// <param name="data">Dictionary of named values</param>
    /// <param name="fileName">File name for the export</param>
    /// <returns>True if export was successful</returns>
    public bool ExportToCSV(Dictionary<string, List<float>> data, string fileName)
    {
        try
        {
            // Create the full file path
            string filePath = Path.Combine(Application.persistentDataPath, "Exports", fileName + ".csv");
            
            // Ensure the exports directory exists
            Directory.CreateDirectory(Path.GetDirectoryName(filePath));
            
            // Build CSV content
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            
            // Write header row
            List<string> keys = new List<string>(data.Keys);
            sb.AppendLine(string.Join(",", keys));
            
            // Determine the maximum length of any series
            int maxLength = 0;
            foreach (var values in data.Values)
            {
                maxLength = Mathf.Max(maxLength, values.Count);
            }
            
            // Write data rows
            for (int i = 0; i < maxLength; i++)
            {
                List<string> rowValues = new List<string>();
                
                foreach (var key in keys)
                {
                    if (data[key].Count > i)
                    {
                        rowValues.Add(data[key][i].ToString());
                    }
                    else
                    {
                        rowValues.Add("");
                    }
                }
                
                sb.AppendLine(string.Join(",", rowValues));
            }
            
            // Write to file
            File.WriteAllText(filePath, sb.ToString());
            
            Debug.Log($"Data exported to CSV: {filePath}");
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error exporting to CSV: {e.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Gets the creation date of a saved file
    /// </summary>
    /// <param name="fileName">File name (without extension)</param>
    /// <param name="extension">File extension</param>
    /// <returns>The creation date, or DateTime.MinValue if file not found</returns>
    public DateTime GetFileCreationDate(string fileName, string extension)
    {
        string filePath = GetSaveFilePath(fileName, extension);
        
        if (File.Exists(filePath))
        {
            return File.GetCreationTime(filePath);
        }
        
        return DateTime.MinValue;
    }
    
    /// <summary>
    /// Deletes a saved file
    /// </summary>
    /// <param name="fileName">File name (without extension)</param>
    /// <param name="extension">File extension</param>
    /// <returns>True if deletion was successful</returns>
    public bool DeleteFile(string fileName, string extension)
    {
        try
        {
            string filePath = GetSaveFilePath(fileName, extension);
            
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                Debug.Log($"File deleted: {filePath}");
                return true;
            }
            else
            {
                Debug.LogWarning($"File not found for deletion: {filePath}");
                return false;
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Error deleting file: {e.Message}");
            return false;
        }
    }
    
    #region Private Helper Methods
    /// <summary>
    /// Ensures the save directory exists
    /// </summary>
    private void EnsureSaveDirectoryExists()
    {
        string saveDirectory = Path.Combine(Application.persistentDataPath, saveFolder);
        
        if (!Directory.Exists(saveDirectory))
        {
            Directory.CreateDirectory(saveDirectory);
            Debug.Log($"Created save directory: {saveDirectory}");
        }
    }
    
    /// <summary>
    /// Gets the full file path for a save file
    /// </summary>
    /// <param name="fileName">File name (without extension)</param>
    /// <param name="extension">File extension</param>
    /// <returns>The full file path</returns>
    private string GetSaveFilePath(string fileName, string extension)
    {
        // Ensure extension starts with a dot
        if (!extension.StartsWith("."))
        {
            extension = "." + extension;
        }
        
        return Path.Combine(Application.persistentDataPath, saveFolder, fileName + extension);
    }
    
    /// <summary>
    /// Gets a list of saved files with the specified extension
    /// </summary>
    /// <param name="extension">File extension to search for</param>
    /// <returns>Array of file names (without extensions)</returns>
    private string[] GetSavedFiles(string extension)
    {
        try
        {
            // Ensure extension starts with a dot
            if (!extension.StartsWith("."))
            {
                extension = "." + extension;
            }
            
            string saveDirectory = Path.Combine(Application.persistentDataPath, saveFolder);
            
            if (!Directory.Exists(saveDirectory))
            {
                return new string[0];
            }
            
            // Get all files with the specified extension
            string[] filePaths = Directory.GetFiles(saveDirectory, "*" + extension);
            List<string> fileNames = new List<string>();
            
            // Extract just the file names without extension
            foreach (string filePath in filePaths)
            {
                string fileName = Path.GetFileNameWithoutExtension(filePath);
                fileNames.Add(fileName);
            }
            
            return fileNames.ToArray();
        }
        catch (Exception e)
        {
            Debug.LogError($"Error getting saved files: {e.Message}");
            return new string[0];
        }
    }
    
    /// <summary>
    /// Inserts a property into a JSON string
    /// </summary>
    /// <param name="json">The original JSON string</param>
    /// <param name="propertyName">Name of the property to insert</param>
    /// <param name="propertyJson">JSON value of the property</param>
    /// <returns>The modified JSON string</returns>
    private string InsertPropertyIntoJson(string json, string propertyName, string propertyJson)
    {
        // Find either the end of the object "{" or a property separator "," 
        int insertPos = json.LastIndexOf('}');
        
        if (insertPos > 0)
        {
            // Check if we need to add a comma (not empty object)
            bool needsComma = json.Substring(0, insertPos).TrimEnd().EndsWith("}") || 
                              json.Substring(0, insertPos).TrimEnd().EndsWith("\"") ||
                              json.Substring(0, insertPos).TrimEnd().EndsWith("'") ||
                              json.Substring(0, insertPos).TrimEnd().EndsWith("]") ||
                              json.Substring(0, insertPos).TrimEnd().EndsWith("true") ||
                              json.Substring(0, insertPos).TrimEnd().EndsWith("false") ||
                              char.IsDigit(json.Substring(0, insertPos).TrimEnd()[json.Substring(0, insertPos).TrimEnd().Length - 1]);
            
            string insert = (needsComma ? "," : "") + $"\"{propertyName}\":{propertyJson}";
            return json.Insert(insertPos, insert);
        }
        
        return json;
    }
    
    /// <summary>
    /// Extracts a property value from a JSON string
    /// </summary>
    /// <param name="json">The JSON string</param>
    /// <param name="propertyName">Name of the property to extract</param>
    /// <returns>The JSON value of the property</returns>
    private string ExtractPropertyFromJson(string json, string propertyName)
    {
        // Find the property
        string propertyTag = $"\"{propertyName}\":";
        int startPos = json.IndexOf(propertyTag);
        
        if (startPos < 0)
        {
            return null;
        }
        
        startPos += propertyTag.Length;
        
        // Find the end of the property value (account for nested objects)
        int braceCount = 0;
        int bracketCount = 0;
        bool inQuotes = false;
        bool escape = false;
        
        int endPos = startPos;
        
        while (endPos < json.Length)
        {
            char c = json[endPos];
            
            if (escape)
            {
                escape = false;
            }
            else if (c == '\\')
            {
                escape = true;
            }
            else if (c == '"')
            {
                inQuotes = !inQuotes;
            }
            else if (!inQuotes)
            {
                if (c == '{')
                {
                    braceCount++;
                }
                else if (c == '}')
                {
                    braceCount--;
                }
                else if (c == '[')
                {
                    bracketCount++;
                }
                else if (c == ']')
                {
                    bracketCount--;
                }
                else if (c == ',' && braceCount == 0 && bracketCount == 0)
                {
                    break;
                }
            }
            
            endPos++;
        }
        
        return json.Substring(startPos, endPos - startPos);
    }
    
    /// <summary>
    /// Generates a mesh from morphology data
    /// </summary>
    /// <param name="morphologyData">The morphology data</param>
    /// <returns>The generated mesh</returns>
    private Mesh GenerateMeshFromMorphology(MorphologyData morphologyData)
{
    return MeshGenerator.GenerateMorphologyMesh(
        morphologyData.nodePositions,
        morphologyData.connections,
        (BiomorphicSim.Utilities.MorphologyParameters.BiomorphType)((int)morphologyData.parametersUsed.biomorphType)
    );
}
    
    /// <summary>
    /// Exports a mesh to OBJ format
    /// </summary>
    /// <param name="mesh">The mesh to export</param>
    /// <param name="filePath">Path for the output file</param>
    private void ExportToOBJ(Mesh mesh, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("# MorphologySimulator OBJ Export");
            writer.WriteLine($"# {DateTime.Now}");
            writer.WriteLine();
            
            // Write vertices
            Vector3[] vertices = mesh.vertices;
            for (int i = 0; i < vertices.Length; i++)
            {
                Vector3 v = vertices[i];
                writer.WriteLine($"v {v.x} {v.y} {v.z}");
            }
            
            // Write UVs
            Vector2[] uvs = mesh.uv;
            for (int i = 0; i < uvs.Length; i++)
            {
                Vector2 uv = uvs[i];
                writer.WriteLine($"vt {uv.x} {uv.y}");
            }
            
            // Write normals
            Vector3[] normals = mesh.normals;
            for (int i = 0; i < normals.Length; i++)
            {
                Vector3 n = normals[i];
                writer.WriteLine($"vn {n.x} {n.y} {n.z}");
            }
            
            writer.WriteLine();
            writer.WriteLine("g MorphologyMesh");
            
            // Write triangles
            int[] triangles = mesh.triangles;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                // OBJ indices are 1-based
                int idx1 = triangles[i] + 1;
                int idx2 = triangles[i + 1] + 1;
                int idx3 = triangles[i + 2] + 1;
                
                // Format: f vertex/uv/normal
                writer.WriteLine($"f {idx1}/{idx1}/{idx1} {idx2}/{idx2}/{idx2} {idx3}/{idx3}/{idx3}");
            }
        }
    }
    
    /// <summary>
    /// Exports a mesh to STL format
    /// </summary>
    /// <param name="mesh">The mesh to export</param>
    /// <param name="filePath">Path for the output file</param>
    private void ExportToSTL(Mesh mesh, string filePath)
    {
        using (StreamWriter writer = new StreamWriter(filePath))
        {
            writer.WriteLine("solid MorphologySimulatorExport");
            
            // Get mesh data
            Vector3[] vertices = mesh.vertices;
            int[] triangles = mesh.triangles;
            Vector3[] normals = mesh.normals;
            
            // Write each triangle
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int idx1 = triangles[i];
                int idx2 = triangles[i + 1];
                int idx3 = triangles[i + 2];
                
                // Calculate the normal for this face
                Vector3 normal = Vector3.Cross(
                    vertices[idx2] - vertices[idx1],
                    vertices[idx3] - vertices[idx1]
                ).normalized;
                
                writer.WriteLine($"  facet normal {normal.x} {normal.y} {normal.z}");
                writer.WriteLine("    outer loop");
                
                Vector3 v1 = vertices[idx1];
                Vector3 v2 = vertices[idx2];
                Vector3 v3 = vertices[idx3];
                
                writer.WriteLine($"      vertex {v1.x} {v1.y} {v1.z}");
                writer.WriteLine($"      vertex {v2.x} {v2.y} {v2.z}");
                writer.WriteLine($"      vertex {v3.x} {v3.y} {v3.z}");
                
                writer.WriteLine("    endloop");
                writer.WriteLine("  endfacet");
            }
            
            writer.WriteLine("endsolid MorphologySimulatorExport");
        }
    }
    #endregion
}

#region Serialization Helper Classes
/// <summary>
/// Helper class for serializing Dictionary<string, float>
/// </summary>
[System.Serializable]
public class SerializableKeyValuePair
{
    public string Key;
    public float Value;
}

/// <summary>
/// Helper class for serializing Dictionary<string, float>
/// </summary>
[System.Serializable]
public class SerializableDictionary
{
    public List<SerializableKeyValuePair> Items;
}

/// <summary>
/// Helper class for serializing time series data
/// </summary>
[System.Serializable]
public class SerializableTimeSeries
{
    public string Key;
    public float[] Values;
}

/// <summary>
/// Helper class for serializing Dictionary<string, List<float>>
/// </summary>
[System.Serializable]
public class SerializableTimeSeriesData
{
    public List<SerializableTimeSeries> Items;
}
#endregion