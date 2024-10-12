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
        public static bool UseFileSystemVideo = true;
        public static string FileSystemVideoBaseURL => "Bettr/LocalStore/LocalVideo";
        public static string VideoServerBaseURL;
        
        public static BettrVideoController Instance { get; private set; }
        
        public void Awake()
        {
            Instance = this;
        }

        public IEnumerator LoadBackgroundAudio(string bundleName)
        {
            if (UseFileSystemVideo)
            {
                yield return LoadFileSystemBackgroundVideo(bundleName);
            }
            else
            {
                yield return LoadS3SystemBackgroundVideo(bundleName);
            }
        }

        public IEnumerator LoadFileSystemBackgroundVideo(string bundleName)
        {
            var backgroundAudioClipName = $"{bundleName}BackgroundMusic";
            var assetPath = Path.Combine(FileSystemVideoBaseURL, $"{backgroundAudioClipName}.mp3");
            var absolutePath = Path.Combine(Application.dataPath, assetPath);
            var absoluteFileUrl = $"file://{absolutePath}";
            AudioClip clip = null;
            using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(absoluteFileUrl, AudioType.MPEG);
            yield return request.SendWebRequest();
            if (request.result == UnityWebRequest.Result.ConnectionError || request.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError($"Error loading audio clip from {absoluteFileUrl}: {request.error}");
                yield break;
            }
            clip = DownloadHandlerAudioClip.GetContent(request);
            clip.name = backgroundAudioClipName;
            if (clip != null)
            {
                Debug.Log("Audio clip successfully loaded from file system.");
            }
            else
            {
                Debug.Log($"Failed to load audio clip from {absoluteFileUrl}");
            }
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