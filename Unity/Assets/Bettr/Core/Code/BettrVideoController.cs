using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Networking;
using UnityEngine.Video;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrVideoPool
    {
        private static readonly int UseTransparency = Shader.PropertyToID("_UseTransparency");

        // ReSharper disable once InconsistentNaming
        private List<VideoPlayer> Pool = new List<VideoPlayer>();
        private int initialPoolSize;
        
        public BettrVideoPool(int initialPoolSize)
        {
            this.initialPoolSize = initialPoolSize;
            InitializePool(initialPoolSize);
        }

        public VideoPlayer Acquire()
        {
            // extract the video player from the pool and return it
            if (Pool.Count == 0)
            {
                Debug.LogError($"Video pool is empty. Possible bug in Acquire/Release or Increase the pool size. poolSize initialized={initialPoolSize} current={Pool.Count}");
                return null;
            }
            var videoPlayer = Pool[0];
            Pool.RemoveAt(0);
            return videoPlayer;
        }

        public void Release(VideoPlayer videoPlayer)
        {
            // Stop the video player and reset its properties
            videoPlayer.Stop();
            videoPlayer.url = null;
            videoPlayer.clip = null;
            videoPlayer.isLooping = false;
            // disable audio
            videoPlayer.audioOutputMode = VideoAudioOutputMode.None;
            // playOnAwake is false
            videoPlayer.playOnAwake = false;
            // wait for first frame is true
            videoPlayer.waitForFirstFrame = true;
            
            videoPlayer.time = 0; // Reset the playback time
            videoPlayer.gameObject.SetActive(false);

            // Do not release the render texture, as it will be reused in the pool
            // Resetting targetTexture to null will release the reference, which is not needed here
    
            // Add the video player back to the pool
            Pool.Add(videoPlayer);
        }

        private void InitializePool(int poolSize)
        {
            for (var i = 0; i < poolSize; i++)
            {
                // Create the game object
                var videoObject = new GameObject($"BettrVideoPlayer{i}");
                var videoPlayer = videoObject.AddComponent<VideoPlayer>();
        
                // Video player settings
                videoPlayer.playOnAwake = false;
                videoPlayer.isLooping = false;
                videoPlayer.audioOutputMode = VideoAudioOutputMode.None; // Disable audio
                videoPlayer.clip = null;
                videoPlayer.url = null;
                videoPlayer.time = 0; // Reset the playback time
                // wait for first frame
                videoPlayer.waitForFirstFrame = true;
                videoObject.SetActive(false);

                // Add to DontDestroyOnLoad
                Object.DontDestroyOnLoad(videoObject);

                // Create the render texture
                var renderTexture = new RenderTexture(960, 600, 16) // WebGL
                {
                    antiAliasing = 1, // No anti-aliasing for performance
                    wrapMode = TextureWrapMode.Clamp,
                    filterMode = FilterMode.Bilinear,
                    useMipMap = false,
                    autoGenerateMips = false,
                    enableRandomWrite = false,
                    useDynamicScale = false,
                    depthStencilFormat = GraphicsFormat.D16_UNorm // Default 16-bit depth
                };
                renderTexture.Create();

                // Assign the render texture to the video player
                videoPlayer.targetTexture = renderTexture;

                // Create a new material using the "Bettr/UnlitTextureWithDepth" shader and assign it to the video player
                var videoMaterial = new Material(Shader.Find("Bettr/UnlitTextureWithDepth"))
                {
                    mainTexture = renderTexture
                };
                videoMaterial.SetFloat(UseTransparency, 1); // Use transparency (1 = true, 0 = false)

                // Add MeshRenderer and set the material
                var meshRenderer = videoObject.AddComponent<MeshRenderer>();
                meshRenderer.material = videoMaterial;

                // Add the video player to the pool
                Pool.Add(videoPlayer);
            }
        }
        
        public void Dispose()
        {
            foreach (var videoPlayer in Pool)
            {
                if (videoPlayer.targetTexture != null)
                {
                    videoPlayer.targetTexture.Release();
                    Object.Destroy(videoPlayer.targetTexture);
                }
                if (videoPlayer.GetComponent<MeshRenderer>().material != null)
                {
                    Object.Destroy(videoPlayer.GetComponent<MeshRenderer>().material);
                }
                Object.Destroy(videoPlayer.gameObject);
            }
            Pool.Clear();
        }

    }

    
    [RequireComponent(typeof(AudioSource))]
    [Serializable]
    // attached to Core MainScene
    public class BettrVideoController : MonoBehaviour
    {
        public static string VideoServerBaseURL;
        
        public static BettrVideoController Instance { get; private set; }
        
        public bool HasBackgroundVideo { get; private set; }
        
        public bool VideoPreparationComplete { get; private set; }
        public bool VideoPreparationError { get; private set; }
        
        public bool VideoStartedPlaying { get; private set; }
        
        public bool VideoLoopPointReached { get; private set; }
        
        public BettrVideoPool VideoPool { get; private set; }
        
        public void Awake()
        {
            Instance = this;

            VideoPool = new BettrVideoPool(10);

        }
        
        private IEnumerator CheckVideoUrl(string url)
        {
            using UnityWebRequest request = UnityWebRequest.Head(url);
            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogWarning($"skipping invalid video URL: {url}");
                VideoPreparationComplete = true;
                VideoPreparationError = true;
                HasBackgroundVideo = false;
            }
        }

        public IEnumerator LoadBackgroundVideo(Table backgroundTable, string machineName, string machineVariant, string experimentVariant)
        {
            HasBackgroundVideo = false;
            
            // Get the BackgroundFBX property from the backgroundTable
            var backgroundFBX = backgroundTable["BackgroundFBX"] as PropertyGameObject;
            if (backgroundFBX == null)
            {
                // log info and quietly fail
                Debug.Log($"Failed to load backgroundFBX from backgroundTable machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            // get the VideoPlayer component from the backgroundFBX
            var videoPlayer = backgroundFBX.gameObject.GetComponent<VideoPlayer>();
            // if null, log and quietly fail
            if (videoPlayer == null)
            {
                Debug.Log($"Failed to load VideoPlayer from backgroundFBX machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }

            VideoPreparationComplete = false;
            VideoPreparationError = false;
            HasBackgroundVideo = false;
            
            // Create the URL for the background video
            var backgroundVideoName = $"{machineName}{machineVariant}BackgroundVideo";
            // Use .webm format
            var assetUrl = $"{VideoServerBaseURL}/video/latest/{backgroundVideoName}.webm";

            yield return CheckVideoUrl(assetUrl);

            if (VideoPreparationError)
            {
                yield break;
            }
            
            HasBackgroundVideo = true;
            
            // set the url of the video player to the assetUrl
            videoPlayer.url = assetUrl;
            
            // Prepare the video player
            videoPlayer.Prepare();
            // Add a listener for when the video is prepared
            videoPlayer.prepareCompleted += source =>
            {
                Debug.Log($"VideoPlayer loaded successfully machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                VideoPreparationComplete = true;
                VideoPreparationError = false;
            };
            
            // Add a listener for when there is an error
            videoPlayer.errorReceived += (VideoPlayer vp, string message) =>
            {
                Debug.Log($"VideoPlayer Error: {message} machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                VideoPreparationComplete = true;
                VideoPreparationError = true;
            };
            
            while (!VideoPreparationComplete)
            {
                yield return null;
            }
        }

        public IEnumerator PlayBackgroundVideo(Table backgroundTable, string machineName, string machineVariant, string experimentVariant)
        {
            if (VideoPreparationError)
            {
                Debug.Log($"Failed to load background video machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            if (!VideoPreparationComplete)
            {
                Debug.Log($"Background video not yet prepared machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            // Get the BackgroundFBX property from the backgroundTable
            var backgroundFBX = backgroundTable["BackgroundFBX"] as PropertyGameObject;
            if (backgroundFBX == null)
            {
                // log info and quietly fail
                Debug.Log($"Failed to load backgroundFBX from backgroundTable machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            // get the VideoPlayer component from the backgroundFBX
            var videoPlayer = backgroundFBX.gameObject.GetComponent<VideoPlayer>();
            // if null, log and quietly fail
            if (videoPlayer == null)
            {
                Debug.Log($"Failed to load VideoPlayer from backgroundFBX machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            // get the mesh renderer from the backgroundFBX
            var meshRenderer = backgroundFBX.gameObject.GetComponent<MeshRenderer>();
            // if null, log and quietly fail
            if (meshRenderer == null)
            {
                Debug.Log($"Failed to load MeshRenderer from backgroundFBX machineName={machineName}, machineVariant={machineVariant}, experimentVariant={experimentVariant}");
                yield break;
            }
            
            // TODO: video is now looping, so no need to switch back to the original material
            // var backgroundFBXMaterial = meshRenderer.material;
            
            // get the cached asset bundle
            // lowercase everything for safety
            var bundleName = $"{machineName}{machineVariant}".ToLower();
            var bundleVersion = $"{experimentVariant}".ToLower();
            var cachedAssetBundle = BettrAssetController.Instance.GetCachedAssetBundle(bundleName, bundleVersion, false);
            if (cachedAssetBundle == null)
            {
                // fail quietly
                Debug.Log($"Failed to load cached asset bundle for {bundleName} {bundleVersion}");
                yield break;
            }
            
            // get the material from the asset bundle. The material name will be called BackgroundVideoMaterial
            var backgroundVideoMaterial = cachedAssetBundle.LoadAsset<Material>("BackgroundVideoMaterial");
            
            // switch the material to the render BackgroundVideoMaterial material
            meshRenderer.material = backgroundVideoMaterial;
            
            VideoLoopPointReached = false;
                
            // Play the video
            videoPlayer.Play();
            
            VideoStartedPlaying = true;
            
            // Add a listener for when the video finishes playing
            videoPlayer.loopPointReached += source =>
            {
                VideoLoopPointReached = true;
            };
            
            while (!VideoLoopPointReached)
            {
                yield return null;
            }
            
            // meshRenderer.material = backgroundFBXMaterial;
            
            // TODO: FIXME: check for memory leaks
            // Do not stop the video player since its looping
            // videoPlayer.Stop();
            //
            // // videoPlayer.targetTexture?.Release();
            // // videoPlayer.targetTexture = null;
            // //
            // // videoPlayer.url = null;
            // // videoPlayer.clip = null;
        }
    }
}