using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using System.IO;

namespace BiomorphicSim.UI
{
    /// <summary>
    /// Helper component to fix video background loading issues
    /// </summary>
    [RequireComponent(typeof(VideoPlayer))]
    public class VideoBackgroundFixer : MonoBehaviour
    {
        [Header("Video Settings")]
        [SerializeField] private string videoFileName = "Background";
        [SerializeField] private string videoFileExtension = ".mp4";
        [SerializeField] private bool loadFromStreamingAssets = true;
        [SerializeField] private RawImage targetImage;
        
        [Header("Fallback Settings")]
        [SerializeField] private bool showDebugInfo = true;
        [SerializeField] private Color fallbackColor = Color.black;
        
        private VideoPlayer videoPlayer;
        
        private void Awake()
        {
            videoPlayer = GetComponent<VideoPlayer>();
            
            if (targetImage == null)
            {
                // Try to find the RawImage on this GameObject
                targetImage = GetComponent<RawImage>();
                
                // If still not found, look for it in children
                if (targetImage == null)
                    targetImage = GetComponentInChildren<RawImage>();
            }
            
            if (videoPlayer == null || targetImage == null)
            {
                Debug.LogError("VideoBackgroundFixer: Missing VideoPlayer or RawImage components!");
                return;
            }
            
            // Make sure the video player is set up correctly
            SetupVideoPlayer();
        }
        
        private void SetupVideoPlayer()
        {
            // Make sure the video loops
            videoPlayer.isLooping = true;
            videoPlayer.playOnAwake = true;
            
            // First try loading from Resources
            VideoClip resourcesClip = Resources.Load<VideoClip>($"Videos/{videoFileName}");
            
            if (resourcesClip != null)
            {
                Debug.Log($"Video found in Resources folder: Videos/{videoFileName}");
                videoPlayer.clip = resourcesClip;
                EnsureTargetTextureSetup();
                return;
            }
            
            // If not found in Resources, try StreamingAssets
            if (loadFromStreamingAssets)
            {
                string streamingPath = Path.Combine(Application.streamingAssetsPath, $"Videos/{videoFileName}{videoFileExtension}");
                
                if (File.Exists(streamingPath))
                {
                    Debug.Log($"Video found in StreamingAssets: {streamingPath}");
                    videoPlayer.url = streamingPath;
                    videoPlayer.source = VideoSource.Url;
                    EnsureTargetTextureSetup();
                    return;
                }
                else
                {
                    Debug.LogWarning($"Video not found at StreamingAssets path: {streamingPath}");
                    
                    // Create the StreamingAssets directory if it doesn't exist
                    string streamingAssetsDir = Application.streamingAssetsPath;
                    if (!Directory.Exists(streamingAssetsDir))
                    {
                        Directory.CreateDirectory(streamingAssetsDir);
                        Debug.Log($"Created StreamingAssets directory at: {streamingAssetsDir}");
                    }
                    
                    // Create Videos subdirectory if it doesn't exist
                    string videosDir = Path.Combine(streamingAssetsDir, "Videos");
                    if (!Directory.Exists(videosDir))
                    {
                        Directory.CreateDirectory(videosDir);
                        Debug.Log($"Created Videos directory at: {videosDir}");
                    }
                    
                    Debug.Log($"Please place your video file at: {streamingPath}");
                }
            }
            
            // If we got here, we couldn't find the video - use fallback
            Debug.LogWarning("Video not found in Resources or StreamingAssets. Using fallback color.");
            targetImage.color = fallbackColor;
        }
        
        private void EnsureTargetTextureSetup()
        {
            // Make sure we have a render texture
            if (videoPlayer.renderMode != VideoRenderMode.RenderTexture)
            {
                videoPlayer.renderMode = VideoRenderMode.RenderTexture;
            }
            
            // Create render texture if needed
            if (videoPlayer.targetTexture == null)
            {
                RenderTexture renderTexture = new RenderTexture(1920, 1080, 24);
                videoPlayer.targetTexture = renderTexture;
            }
            
            // Assign texture to image
            targetImage.texture = videoPlayer.targetTexture;
            targetImage.color = Color.white; // Make sure the image is visible
            
            // Start playback
            videoPlayer.Play();
        }
        
        private void OnGUI()
        {
            if (!showDebugInfo) return;
            
            // Debug display in the bottom left corner
            GUILayout.BeginArea(new Rect(10, Screen.height - 120, 400, 100));
            GUILayout.Label($"<color=yellow>Video Player Status:</color>");
            GUILayout.Label($"Is Playing: {videoPlayer.isPlaying}");
            
            if (videoPlayer.clip != null)
                GUILayout.Label($"Clip Name: {videoPlayer.clip.name}");
            else if (!string.IsNullOrEmpty(videoPlayer.url))
                GUILayout.Label($"URL: {videoPlayer.url}");
            else
                GUILayout.Label("<color=red>No video source assigned</color>");
                
            GUILayout.Label($"Frame: {videoPlayer.frame} / {videoPlayer.frameCount}");
            GUILayout.EndArea();
        }
    }
}
