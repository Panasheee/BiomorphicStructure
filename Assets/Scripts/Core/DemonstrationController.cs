using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using BiomorphicSim.Core;

namespace BiomorphicSim.Core
{
    /// <summary>
    /// Handles running various demonstration scenarios for the Biomorphic Simulation.
    /// </summary>
    public class DemonstrationController : MonoBehaviour
    {
        #region References
        [Header("Required Components")]
        [SerializeField] private MorphologyManager morphologyManager;
        [SerializeField] private SiteGenerator siteGenerator;
        [SerializeField] private MorphologyGenerator morphologyGenerator;
        [SerializeField] private ScenarioAnalyzer scenarioAnalyzer;
        [SerializeField] private UIManager uiManager;
        [SerializeField] private VisualizationManager visualizationManager;
        [SerializeField] private DataIO dataIO;

        // Need access to MainController's preset zones or define them here
        [Header("Preset Zones (Reference or Copy from MainController)")]
        [SerializeField] private Vector3[] zonePositions;
        [SerializeField] private Vector3[] zoneSizes;
        [SerializeField] private string[] zoneNames; // If needed by demos

        // Reference back to MainController if needed for site generation etc.
        [SerializeField] private MainController mainController;
        #endregion

        #region Public Methods
        /// <summary>
        /// Runs a specific demonstration based on the provided type string.
        /// </summary>
        /// <param name="demoType">Type of demo to run (e.g., "GrowthAndAdaptation")</param>
        public void RunDemonstration(string demoType)
        {
            // Ensure all references are set before starting
            if (!AreReferencesValid())
            {
                Debug.LogError("DemonstrationController is missing required component references. Cannot run demonstration.");
                return;
            }
            StartCoroutine(DemonstrationCoroutine(demoType));
        }
        #endregion

        #region Private Methods (Demonstration Coroutines)

        /// <summary>
        /// Coroutine that runs a demonstration of the system
        /// </summary>
        /// <param name="demoType">Type of demo to run</param>
        private IEnumerator DemonstrationCoroutine(string demoType)
        {
            Debug.Log($"Starting demonstration: {demoType}");

            switch (demoType)
            {
                case "GrowthAndAdaptation":
                    yield return StartCoroutine(GrowthAndAdaptationDemo());
                    break;

                case "WindAdaptation":
                    yield return StartCoroutine(WindAdaptationDemo());
                    break;

                case "BioTypeComparison":
                    yield return StartCoroutine(BioTypeComparisonDemo());
                    break;

                case "FullLifecycle":
                    yield return StartCoroutine(FullLifecycleDemo());
                    break;

                default:
                    Debug.LogWarning($"Unknown demo type: {demoType}");
                    break;
            }

            Debug.Log("Demonstration completed");
        }

        /// <summary>
        /// Demonstration of growth and adaptation
        /// </summary>
        private IEnumerator GrowthAndAdaptationDemo()
        {
            // Use 'ScenarioData' instead of 'BiomorphicSim.Core.ScenarioData'
            ScenarioData scenario = new ScenarioData
            {
                scenarioName = "Adaptive Growth Demo",
                description = "Demonstrates adaptation to changing environmental forces",
                simulationDuration = 15f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 0.8f },
                    { "Gravity", 0.5f },
                    { "Temperature", 0.4f },
                    // Optional: include extra parameters if you wish:
                    { "Density", 0.6f },
                    { "Complexity", 0.7f },
                    { "Connectivity", 0.5f }
                    // 'biomorphType' and 'growthPattern' can be passed as additional factors if desired
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario);
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }
            
            if (uiManager != null)
            {
                // Declare the results with the fully qualified type.
                ScenarioResults results = scenarioAnalyzer.GetResults();
                if (results != null)
                {
                    uiManager.DisplayScenarioResults(results);
                }
                else
                {
                    Debug.LogWarning("No results available to show in GrowthAndAdaptationDemo.");
                }
            }
        }

        /// <summary>
        /// Demonstration of wind adaptation
        /// </summary>
        private IEnumerator WindAdaptationDemo()
        {
            if (siteGenerator.GetSiteBounds().size == Vector3.zero && mainController != null)
            {
                mainController.GenerateLambtonQuaySite();
                yield return new WaitForSeconds(1f);
            }
              if (zonePositions != null && zonePositions.Length > 1 && zoneSizes != null && zoneSizes.Length > 1)
            {
                Bounds zone = new Bounds(zonePositions[1], zoneSizes[1]);
                morphologyManager.SetSelectedZone(zone);
                yield return new WaitForSeconds(0.5f);
            }

            ScenarioData scenario = new ScenarioData
            {
                scenarioName = "Wind Adaptation",
                description = "Tests how structure adapts to strong wind forces",
                simulationDuration = 12f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 1.0f },
                    { "Gravity", 0.3f }
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario);
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }
            
            if (uiManager != null)
            {
                ScenarioResults results = scenarioAnalyzer.GetResults();
                if (results != null)
                {
                    uiManager.DisplayScenarioResults(results);
                }
                else
                {
                    Debug.LogWarning("No results available to show in WindAdaptationDemo.");
                }
            }
        }

        /// <summary>
        /// Demonstration comparing different bio-types
        /// </summary>
        private IEnumerator BioTypeComparisonDemo()
        {
            if (siteGenerator.GetSiteBounds().size == Vector3.zero && mainController != null)
            {
                mainController.GenerateLambtonQuaySite();
                yield return new WaitForSeconds(1f);
            }

            MorphologyParameters.BiomorphType[] types = new MorphologyParameters.BiomorphType[]
            {
                MorphologyParameters.BiomorphType.Mold,
                MorphologyParameters.BiomorphType.Bone,
                MorphologyParameters.BiomorphType.Coral,
                MorphologyParameters.BiomorphType.Mycelium
            };

            // Replace with fully qualified type
            ScenarioData commonScenario = new ScenarioData
            {
                scenarioName = "Biotype Comparison",
                description = "Comparing how different biotypes respond to the same conditions",
                simulationDuration = 10f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 0.7f },
                    { "Gravity", 0.6f },
                    { "Temperature", 0.3f },
                    { "PedestrianFlow", 0.5f }
                }
            };

            List<ScenarioResults> comparisonResults = new List<ScenarioResults>();

            for (int i = 0; i < types.Length; i++)
            {
                if (zonePositions != null && zonePositions.Length > 0 && zoneSizes != null && zoneSizes.Length > 0)
                {
                    Bounds zone = new Bounds(zonePositions[0], zoneSizes[0]);
                    morphologyManager.SetSelectedZone(zone);
                    yield return new WaitForSeconds(0.5f);
                }

                MorphologyParameters parameters = new MorphologyParameters
                {
                    density = 0.5f,
                    complexity = 0.5f,
                    connectivity = 0.5f,
                    biomorphType = types[i],
                    growthPattern = MorphologyParameters.GrowthPattern.Organic
                };

                // Change this line
                morphologyManager.GenerateMorphology(parameters);

                // To this (using fully qualified name):
                morphologyManager.GenerateMorphology(new BiomorphicSim.Core.MorphologyParameters
                {
                    density = 0.5f,
                    complexity = 0.5f,
                    connectivity = 0.5f,
                    biomorphType = types[i],
                    growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Organic
                });

                while (morphologyGenerator.GenerationProgress < 1f)
                {
                    yield return null;
                }

                yield return new WaitForSeconds(1f);

                commonScenario.scenarioName = $"Biotype Comparison - {types[i]}";
                MorphologyManager.Instance.RunScenarioAnalysis(commonScenario);

                while (scenarioAnalyzer.AnalysisProgress < 1f)
                {
                    yield return null;
                }

                ScenarioResults currentResult = scenarioAnalyzer.GetResults();
                if (currentResult != null)
                {
                    comparisonResults.Add(currentResult);
                }

                string screenshotPath = visualizationManager.TakeScreenshot();
                Debug.Log($"Screenshot saved: {screenshotPath}");

                if (uiManager != null && currentResult != null)
                {
                    uiManager.DisplayScenarioResults(currentResult);
                    yield return new WaitForSeconds(3f);
                }

                if (dataIO != null)
                {
                    string saveName = $"BioComparison_{types[i]}";

                    MorphologyData morphologyData = morphologyGenerator.ExportMorphologyData();
                    if (morphologyData != null)
                    {
                        dataIO.SaveMorphology(morphologyData, saveName);
                    }

                    ScenarioResults results = scenarioAnalyzer.GetResults();
                    if (results != null)
                    {
                        dataIO.SaveResults(results, saveName);
                    }
                }
            }

            if (uiManager != null)
            {
                uiManager.ShowComparisonPanel(comparisonResults.Count > 0 ? comparisonResults : null);
            }
        }

        /// <summary>
        /// Demonstration of the full lifecycle from site to morphology to adaptation
        /// </summary>
        private IEnumerator FullLifecycleDemo()
        {
            if (siteGenerator.GetSiteBounds().size == Vector3.zero && mainController != null)
            {
                mainController.GenerateLambtonQuaySite();
                yield return new WaitForSeconds(2f);
            }

            if (zonePositions != null && zonePositions.Length > 2 && zoneSizes != null && zoneSizes.Length > 2)
            {
                Bounds zone = new Bounds(zonePositions[2], zoneSizes[2]);
                morphologyManager.SetSelectedZone(zone);
            }
            else
            {
                Vector3 siteCenter = siteGenerator.GetSiteBounds().center;
                Vector3 zoneSize = new Vector3(30, 30, 30);
                Bounds zone = new Bounds(siteCenter, zoneSize);
                morphologyManager.SetSelectedZone(zone);
            }
              yield return new WaitForSeconds(1f);

            // Using fully qualified name:
            morphologyManager.GenerateMorphology(new BiomorphicSim.Core.MorphologyParameters
            {
                density = 0.7f,
                complexity = 0.8f,
                connectivity = 0.6f,
                biomorphType = BiomorphicSim.Core.MorphologyParameters.BiomorphType.Custom,
                growthPattern = BiomorphicSim.Core.MorphologyParameters.GrowthPattern.Adaptive
            });

            while (morphologyGenerator.GenerationProgress < 1f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(2f);

            // Replace with fully qualified type
            ScenarioData scenario1 = new ScenarioData
            {
                scenarioName = "Initial Growth",
                description = "Initial growth phase with minimal environmental forces",
                simulationDuration = 8f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Gravity", 0.4f },
                    { "SiteAttraction", 0.3f }
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario1);

            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            // Replace with fully qualified type
            ScenarioData scenario2 = new ScenarioData
            {
                scenarioName = "Wind Stress Adaptation",
                description = "Adaptation to increasing wind forces",
                simulationDuration = 10f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 0.8f },
                    { "Gravity", 0.4f },
                    { "SiteAttraction", 0.2f }
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario2);
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            // Replace with fully qualified type
            ScenarioData scenario3 = new ScenarioData
            {
                scenarioName = "Pedestrian Interaction",
                description = "Adaptation to pedestrian flow patterns",
                simulationDuration = 12f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 0.3f },
                    { "Gravity", 0.4f },
                    { "PedestrianFlow", 0.9f },
                    { "SiteAttraction", 0.2f }
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario3);
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }

            yield return new WaitForSeconds(1.5f);

            // Replace with fully qualified type
            ScenarioData scenario4 = new ScenarioData
            {
                scenarioName = "Extreme Weather",
                description = "Testing resilience to extreme weather conditions",
                simulationDuration = 15f,
                recordHistory = true,
                environmentalFactors = new Dictionary<string, float>
                {
                    { "Wind", 1.0f },
                    { "Gravity", 0.7f },
                    { "Temperature", 0.9f },
                    { "SiteAttraction", 0.5f }
                }
            };

            MorphologyManager.Instance.RunScenarioAnalysis(scenario4);
            while (scenarioAnalyzer.AnalysisProgress < 1f)
            {
                yield return null;
            }

            // Assuming lifecycleData is generated elsewhere
            var lifecycleData = new List<MorphologyData>();

            if (uiManager != null)
            {
                uiManager.ShowLifecycleComparisonPanel(lifecycleData.Count > 0 ? lifecycleData : null);
            }

            if (dataIO != null)
            {
                string saveName = "FullLifecycleDemo";

                MorphologyData morphologyData = morphologyGenerator.ExportMorphologyData();
                if (morphologyData != null)
                {
                    dataIO.SaveMorphology(morphologyData, saveName);
                }

                ScenarioResults results = scenarioAnalyzer.GetResults();
                if (results != null)
                {
                    dataIO.SaveResults(results, saveName);
                }

                visualizationManager.ExportCurrentView(saveName);
            }
        }

        /// <summary>
        /// Checks if all required component references are assigned.
        /// </summary>
        /// <returns>True if all references are valid, false otherwise.</returns>
        private bool AreReferencesValid()
        {
            bool isValid = true;
            if (morphologyManager == null) { Debug.LogError("DemonstrationController: MorphologyManager reference not set."); isValid = false; }
            if (siteGenerator == null) { Debug.LogError("DemonstrationController: SiteGenerator reference not set."); isValid = false; }
            if (morphologyGenerator == null) { Debug.LogError("DemonstrationController: MorphologyGenerator reference not set."); isValid = false; }
            if (scenarioAnalyzer == null) { Debug.LogError("DemonstrationController: ScenarioAnalyzer reference not set."); isValid = false; }
            if (uiManager == null) { Debug.LogError("DemonstrationController: UIManager reference not set."); isValid = false; }
            if (visualizationManager == null) { Debug.LogError("DemonstrationController: VisualizationManager reference not set."); isValid = false; }
            if (dataIO == null) { Debug.LogError("DemonstrationController: DataIO reference not set."); isValid = false; }
            // Add check for mainController if it's essential for all demos
            // if (mainController == null) { Debug.LogError("DemonstrationController: MainController reference not set."); isValid = false; }

            // Check zone data if required by demos
            if (zonePositions == null || zoneSizes == null) { Debug.LogWarning("DemonstrationController: Zone positions or sizes not set. Some demos might not work correctly."); }
            else if (zonePositions.Length != zoneSizes.Length) { Debug.LogWarning("DemonstrationController: Zone positions and sizes arrays have different lengths."); }


            return isValid;
        }

        #endregion
    }
} // End namespace
