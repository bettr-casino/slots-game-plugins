using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

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
        
        public void Awake()
        {
            Instance = this;
        }

        public IEnumerator LoadBackgroundVideo(string bundleName)
        {
            yield return LoadS3SystemBackgroundVideo(bundleName);
        }
        
        public IEnumerator LoadS3SystemBackgroundVideo(string bundleName)
        {
            // Create the URL for the background music
            var backgroundAudioClipName = $"{bundleName}BackgroundMusic";
            // TODO: fixme
            var assetUrl = $"{VideoServerBaseURL}/{backgroundAudioClipName}.mp3";

            // Use UnityWebRequest to load the AudioClip from S3
            using (UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(assetUrl, AudioType.MPEG))
            {
                // Send the request
                yield return request.SendWebRequest();

                // Check for errors
                if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
                {
                    Debug.LogError($"Error loading audio clip from {assetUrl}: {request.error}");
                    yield break;
                }

                // Get the AudioClip from the request
                AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

                // Now you can use the AudioClip, for example:
                if (clip != null)
                {
                    Debug.Log("Audio clip successfully loaded from S3.");

                    // Example: Set it to an AudioSource and play
                    AudioSource audioSource = GetComponent<AudioSource>();
                    audioSource.clip = clip;
                    audioSource.Play();
                }
                else
                {
                    Debug.LogError($"Failed to load audio clip from {assetUrl}");
                }
            }
        }
    }
}