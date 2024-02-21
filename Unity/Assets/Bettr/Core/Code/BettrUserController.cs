using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CrayonScript.Code;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.Serialization;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class UserBlobRequest
    {
        [JsonProperty("value")]
        public string value;
        
        [JsonProperty("ttl")]
        public long ttl;
        
        [JsonProperty("create")]
        public bool create;
        
        [JsonProperty("cas")]
        public string cas;
    }
    
    [Serializable]
    public class BettrUserController
    {
        public BettrServer bettrServer;
        
        public string webAssetBaseURL;
        
        public BettrUserConfig BettrUserConfig { get; private set; }
        
        public bool UserIsLoggedIn { get; private set; }
        
        const string ErrorUserBlobDoesNotExist = "HTTP/1.1 404 Not Found";

        public BettrUserController()
        {
            TileController.RegisterType<BettrUserController>("BettrUserController");
            TileController.AddToGlobals("BettrUserController", this);
            
            TileController.RegisterType<BettrUserConfig>("BettrUserConfig");
        }
        
        public string GetUserId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            var uniqueId = $"{deviceId}";
            return uniqueId;            
        }

        public IEnumerator Login()
        {
            Debug.Log($"Starting User Login");
            
            BettrUserConfig = null;
            UserIsLoggedIn = false;
            
            var userId = GetUserId();
            yield return bettrServer.Login(userId);

            UserIsLoggedIn = !bettrServer.AuthResponse.isError;
            if (UserIsLoggedIn)
            {
                var userDoesNotExist = false;
                yield return bettrServer.LoadUserBlob(callback: (url, payload, success, error) =>
                {
                    if (error == ErrorUserBlobDoesNotExist)
                    {
                        userDoesNotExist = true;
                        return;
                    }
                    if (!success)
                    {
                        Debug.LogError($"Error loading user blob: {error}");
                        return;
                    }
                    var userBlob = JsonConvert.DeserializeObject<BettrUserConfig>(Encoding.UTF8.GetString(payload));
                    BettrUserConfig = userBlob;
                });

                if (!userDoesNotExist)
                {
                    yield return LoadUserJsonFromWebAssets((url, payload, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error loading user JSON: {error}");
                            return;
                        }
                        var userJson = Encoding.UTF8.GetString(payload);
                        var user = JsonConvert.DeserializeObject<BettrUserConfig>(userJson);
                        BettrUserConfig = user;
                    });
                }
            }
        }
        
        public IEnumerator LoadUserJsonFromWebAssets(GetCallback callback)
        {
            string webAssetName = "users/default/user.json";
            
            string assetBundleURL = $"{webAssetBaseURL}/{webAssetName}";
            using UnityWebRequest www = UnityWebRequest.Get(assetBundleURL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] jsonBytes = www.downloadHandler.data;
                callback(assetBundleURL, jsonBytes, true, null);
            }
            else
            {
                var error = $"Error loading user JSON from server: {www.error}";
                Debug.LogError(error);
                callback?.Invoke(assetBundleURL, null, false, error);
            }
        }
    }
}