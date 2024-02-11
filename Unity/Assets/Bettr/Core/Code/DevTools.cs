using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class HierarchyRoot
    {
        public long CreatedAtTimestamp;
        public readonly List<HierarchyItem> Children = new List<HierarchyItem>();
    }
    
    public class HierarchyItem
    {
        public string Name;
        public readonly List<HierarchyItem> Children = new List<HierarchyItem>();
    }
    
    [Serializable]
    public class GameStatePayload
    {
        [JsonProperty("taskHashKey")]
        public string TaskHashKey { get; set; }
        
        [JsonProperty("zipDataBase64")]
        public string ZipDataBase64 { get; set; }
        
        [JsonProperty("source")]
        public string Source { get; set; }
    }
    
    public class DevTools : MonoBehaviour
    {
        private bool _isCaptureInProgress = false;

        private const string STORE_GAME_STATE_URL =
            "https://kdc3oqvvq32yvl7lt4uw5fdbae0kiobe.lambda-url.us-west-2.on.aws/";
        
        public static DevTools Instance { get; private set; }
        
        private bool _isEnabled = false;
        public void Enable() { _isEnabled = true; }

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            DontDestroyOnLoad(gameObject);
        }

        public IEnumerator CaptureSceneState()
        {
            if (!_isEnabled)
            {
                Debug.Log("DevTools is not enabled. Ignoring ScreenCapture request");
            }
            
            // Check if a capture operation is already in progress
            if (_isCaptureInProgress)
            {
                Debug.Log("CaptureSceneState is already in progress. Ignoring the new request.");
                yield break; // Exit the coroutine
            }

            _isCaptureInProgress = true;
            
            yield return new WaitForEndOfFrame();

            int width = Screen.width;
            int height = Screen.height;
            Texture2D screenTexture = new Texture2D(width, height, TextureFormat.RGB24, false);

            screenTexture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            screenTexture.Apply();

            byte[] imageData = screenTexture.EncodeToPNG();
            Destroy(screenTexture);

            var root = new HierarchyRoot();
            root.CreatedAtTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            foreach (GameObject obj in SceneManager.GetActiveScene().GetRootGameObjects())
            {
                // Add object introspection logic here. For example:
                var item = new HierarchyItem {Name = obj.name};
                root.Children.Add(item);
                IntrospectHierarchy(item, obj);
            }

            yield return UploadToLambda(root, imageData);
            
            _isCaptureInProgress = false;
        }

        private void IntrospectHierarchy(HierarchyItem item, GameObject obj)
        {
            for (int i = 0; i < obj.transform.childCount; i++)
            {
                var childObj = obj.transform.GetChild(i).gameObject;
                item.Children.Add(new HierarchyItem {Name = childObj.name});
                IntrospectHierarchy(item.Children[i], childObj);
            }
        }

        private byte[] ZipSceneState(HierarchyRoot hierarchyRoot, byte[] imageData)
        {
            // Serialize JSON
            var serializedJson = JsonConvert.SerializeObject(hierarchyRoot, Formatting.Indented);
            byte[] zipData;
            using var memoryStream = new MemoryStream();
            using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
            {
                // Add image file
                var imageEntry = archive.CreateEntry("image.png");
                using (var entryStream = imageEntry.Open())
                {
                    entryStream.Write(imageData, 0, imageData.Length);
                }
                // Add JSON file
                var jsonEntry = archive.CreateEntry("hierarchy.json");
                using (var entryStream = jsonEntry.Open())
                using (var streamWriter = new StreamWriter(entryStream))
                {
                    streamWriter.Write(serializedJson);
                    streamWriter.Flush();
                }
            }
            memoryStream.Flush();
            // Convert memory stream to byte array
            zipData = memoryStream.ToArray();
            return zipData;
        }

        private IEnumerator UploadToLambda(HierarchyRoot sceneHierarchyRoot, byte[] imageData, int maxRetries = 3)
        {
            var taskId = PlayerPrefs.GetString(ConfigData.TaskCodeKey, ConfigData.DefaultTaskCode);
            Debug.Log($"UploadToLambda using stored PlayerPrefs taskId {ConfigData.TaskCodeKey}=" + taskId);
            
            var source = "desktop";
            if (Application.isMobilePlatform)
            {
                source = "mobile";
            }

            var zipData = ZipSceneState(sceneHierarchyRoot, imageData);
            var zipDataBase64 = Convert.ToBase64String(zipData);
            var gameStatePayload = new GameStatePayload()
            {
                TaskHashKey = taskId,
                ZipDataBase64 = zipDataBase64,
                Source = source
                
            };
            var gameStatePayloadString = JsonConvert.SerializeObject(gameStatePayload);
            int attempts = 0;
            bool success = false;
            while (attempts < maxRetries && !success)
            {
                using UnityWebRequest www = UnityWebRequest.Post(STORE_GAME_STATE_URL, gameStatePayloadString, "application/json");
                yield return www.SendWebRequest();
                
                if (www.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"Error uploading to Lambda: {www.error}");
                    attempts++;
                }
                else
                {
                    Debug.Log("Upload complete!");
                    success = true;
                }
            }
            if (!success)
            {
                Debug.LogError("Failed to upload after maximum retries.");
            }
        }

    }
}