using System;
using System.Collections;
using System.IO;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Video;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [RequireComponent(typeof(AudioSource))]
    [Serializable]
    // attached to Core MainScene
    public class BettrVideoController : MonoBehaviour
    {
        public static string VideoServerBaseURL;
        
        public static BettrVideoController Instance { get; private set; }
        
        public bool VideoPreparationComplete { get; private set; }
        public bool VideoPreparationError { get; private set; }
        
        public bool VideoLoopPointReached { get; private set; }
        
        public void Awake()
        {
            Instance = this;
        }

        public IEnumerator LoadBackgroundVideo(Table backgroundTable, string machineName, string machineVariant, string experimentVariant)
        {
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
            
            // Create the URL for the background video
            var backgroundVideoName = $"{machineName}{machineVariant}BackgroundVideo";
            var assetUrl = $"{VideoServerBaseURL}/video/latest/{backgroundVideoName}.mp4";
            
            // set the url of the video player to the assetUrl
            videoPlayer.url = assetUrl;
            
            VideoPreparationComplete = false;
            VideoPreparationError = false;
            
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
            var material = cachedAssetBundle.LoadAsset<Material>("BackgroundVideoMaterial");
            
            // switch the material to the render BackgroundVideoMaterial material
            meshRenderer.material = material;
            
            VideoLoopPointReached = false;
                
            // Play the video
            videoPlayer.Play();
            
            // Add a listener for when the video finishes playing
            videoPlayer.loopPointReached += source =>
            {
                VideoLoopPointReached = true;
            };
            
            while (!VideoLoopPointReached)
            {
                yield return null;
            }
        }
    }
}