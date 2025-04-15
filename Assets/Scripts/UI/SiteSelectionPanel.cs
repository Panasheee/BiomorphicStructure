using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// UI panel for selecting and configuring geographic simulation sites.
/// Allows selection between different predefined locations in Wellington.
/// </summary>
public class SiteSelectionPanel : MonoBehaviour
{
    #region Properties
    [Header("UI Components")]
    [SerializeField] private TMP_Dropdown siteDropdown;
    [SerializeField] private Button selectButton;
    [SerializeField] private TextMeshProUGUI siteInfoText;
    [SerializeField] private Image sitePreviewImage;
    [SerializeField] private RawImage siteMapImage;
    [SerializeField] private GameObject loadingIndicator;
    
    [Header("Site Settings")]
    [SerializeField] private Sprite[] sitePreviewSprites;
    [SerializeField] private Texture2D[] siteMapTextures;
    [SerializeField] private SiteData[] predefinedSites;
    
    [Header("References")]
    [SerializeField] private EnvironmentManager environmentManager;
    [SerializeField] private Transform sceneRoot;
    
    // Current site
    private int currentSiteIndex = -1;
    private bool isSiteLoading = false;
    
    // Events
    public delegate void SiteChangedHandler(SiteData site);
    public event SiteChangedHandler OnSiteChanged;
    #endregion

    #region Unity Methods    private void Awake()
    {
        // Find references if not assigned
        if (environmentManager == null)
            environmentManager = FindFirstObjectByType<EnvironmentManager>();
    }
    
    private void Start()
    {
        // Initialize UI
        SetupEventListeners();
        
        // Populate dropdown
        PopulateSiteDropdown();
        
        // Initialize with default site
        if (predefinedSites != null && predefinedSites.Length > 0)
        {
            SelectSite(0);
        }
    }
    #endregion

    #region Public Methods
    /// <summary>
    /// Select a site by index
    /// </summary>
    public void SelectSite(int index)
    {
        if (index < 0 || predefinedSites == null || index >= predefinedSites.Length)
            return;
        
        // Set current index
        currentSiteIndex = index;
        
        // Update UI
        UpdateSitePreview(index);
        
        // Set dropdown value
        if (siteDropdown != null && !isSiteLoading)
        {
            siteDropdown.value = index;
        }
        
        // Update environment manager with site coordinates
        if (environmentManager != null)
        {
            SiteData site = predefinedSites[index];
            environmentManager.SetGeographicLocation(site.latitude, site.longitude);
        }
        
        // Notify listeners
        if (OnSiteChanged != null)
            OnSiteChanged.Invoke(predefinedSites[index]);
    }
    
    /// <summary>
    /// Select a site by name
    /// </summary>
    public void SelectSiteByName(string siteName)
    {
        if (predefinedSites == null)
            return;
        
        for (int i = 0; i < predefinedSites.Length; i++)
        {
            if (predefinedSites[i].name == siteName)
            {
                SelectSite(i);
                return;
            }
        }
    }
    
    /// <summary>
    /// Load the selected site
    /// </summary>
    public void LoadSelectedSite()
    {
        if (currentSiteIndex < 0 || predefinedSites == null || currentSiteIndex >= predefinedSites.Length)
            return;
        
        StartCoroutine(LoadSiteCoroutine(currentSiteIndex));
    }
    
    /// <summary>
    /// Set panel visibility
    /// </summary>
    public void SetPanelVisible(bool visible)
    {
        gameObject.SetActive(visible);
    }
    #endregion

    #region Private Methods
    /// <summary>
    /// Set up event listeners for UI components
    /// </summary>
    private void SetupEventListeners()
    {
        // Add listener to dropdown
        if (siteDropdown != null)
            siteDropdown.onValueChanged.AddListener(OnSiteSelected);
        
        // Add listener to select button
        if (selectButton != null)
            selectButton.onClick.AddListener(LoadSelectedSite);
    }
    
    /// <summary>
    /// Populate the site dropdown with available sites
    /// </summary>
    private void PopulateSiteDropdown()
    {
        if (siteDropdown == null || predefinedSites == null)
            return;
        
        // Clear options
        siteDropdown.ClearOptions();
        
        // Add site names
        List<string> options = new List<string>();
        foreach (SiteData site in predefinedSites)
        {
            options.Add(site.name);
        }
        
        siteDropdown.AddOptions(options);
    }
    
    /// <summary>
    /// Update the site preview UI
    /// </summary>
    private void UpdateSitePreview(int index)
    {
        if (index < 0 || predefinedSites == null || index >= predefinedSites.Length)
            return;
        
        SiteData site = predefinedSites[index];
        
        // Update info text
        if (siteInfoText != null)
        {
            siteInfoText.text = $"{site.name}\n" +
                              $"Location: {site.latitude:F4}°, {site.longitude:F4}°\n" +
                              $"Elevation: {site.elevation:F1}m\n" +
                              $"{site.description}";
        }
        
        // Update preview image
        if (sitePreviewImage != null && sitePreviewSprites != null && index < sitePreviewSprites.Length)
        {
            sitePreviewImage.sprite = sitePreviewSprites[index];
        }
        
        // Update map image
        if (siteMapImage != null && siteMapTextures != null && index < siteMapTextures.Length)
        {
            siteMapImage.texture = siteMapTextures[index];
        }
    }
    
    /// <summary>
    /// Coroutine to load a site
    /// </summary>
    private IEnumerator LoadSiteCoroutine(int index)
    {
        if (index < 0 || predefinedSites == null || index >= predefinedSites.Length)
            yield break;
        
        isSiteLoading = true;
        
        // Show loading indicator
        if (loadingIndicator != null)
            loadingIndicator.SetActive(true);
        
        // Disable select button during loading
        if (selectButton != null)
            selectButton.interactable = false;
        
        // Get site data
        SiteData site = predefinedSites[index];
        
        // Unload current scene if any
        if (sceneRoot != null)
        {
            // Disable all children
            for (int i = 0; i < sceneRoot.childCount; i++)
            {
                sceneRoot.GetChild(i).gameObject.SetActive(false);
            }
        }
        
        // Simulate loading time
        yield return new WaitForSeconds(1.5f);
        
        // Load the site's scene asset
        GameObject siteScene = null;
        if (!string.IsNullOrEmpty(site.scenePrefabPath))
        {
            GameObject prefab = Resources.Load<GameObject>(site.scenePrefabPath);
            if (prefab != null)
            {
                siteScene = Instantiate(prefab, Vector3.zero, Quaternion.identity);
                
                // Parent to scene root if available
                if (sceneRoot != null)
                {
                    siteScene.transform.SetParent(sceneRoot);
                }
            }
        }
        
        // Update environment manager with site data
        if (environmentManager != null)
        {
            environmentManager.SetGeographicLocation(site.latitude, site.longitude);
            environmentManager.SetLocationData(site.name, site.elevation, site.urbanDensity);
        }
        
        // Additional loading steps
        yield return new WaitForSeconds(0.5f);
        
        // Hide loading indicator
        if (loadingIndicator != null)
            loadingIndicator.SetActive(false);
        
        // Re-enable select button
        if (selectButton != null)
            selectButton.interactable = true;
        
        isSiteLoading = false;
        
        // Notify listeners
        if (OnSiteChanged != null)
            OnSiteChanged.Invoke(site);
    }
    
    /// <summary>
    /// Event handler for site selection
    /// </summary>
    private void OnSiteSelected(int index)
    {
        if (!isSiteLoading)
        {
            SelectSite(index);
        }
    }
    #endregion
}

/// <summary>
/// Structure to hold geographic site data
/// </summary>
[System.Serializable]
public class SiteData
{
    public string name;
    public string description;
    public float latitude;
    public float longitude;
    public float elevation;
    public float urbanDensity; // 0-1 ranging from rural to dense urban
    public string scenePrefabPath; // Path to the scene prefab in Resources folder
}
