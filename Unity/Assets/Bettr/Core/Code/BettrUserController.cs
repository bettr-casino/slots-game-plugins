using System;
using System.Collections;
using System.Text;
using CrayonScript.Code;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrUserController
    {
        public BettrServer bettrServer;

        public ConfigData configData;
        
        public BettrUserConfig BettrUserConfig { get; private set; }
        
        public static BettrUserController Instance { get; private set; }
        
        public bool UserIsLoggedIn { get; private set; }
        
        const string ErrorBlobDoesNotExist = "HTTP/1.1 404 Not Found";

        public BettrUserController()
        {
            TileController.RegisterType<BettrUserController>("BettrUserController");
            TileController.AddToGlobals("BettrUserController", this);
            
            TileController.RegisterType<BettrUserConfig>("BettrUserConfig");
            TileController.RegisterType<BettrMechanicConfig>("BettrMechanicConfig");
            
            Instance = this;
        }
        
        public string GetUserId()
        {
            var deviceId = SystemInfo.deviceUniqueIdentifier;
            deviceId = "EE0DE516-5053-5142-80AC-2D878E91215C"; // TODO: remove hardcoded user after testing id: "b19e240f-79d5-4ab1-a844-48c97bc1d154"
            var uniqueId = $"{deviceId}";
            return uniqueId;            
        }

        public IEnumerator Login()
        {
            bool replaceBlob = true;
            Debug.Log($"Starting User Login");
            
            BettrUserConfig = null;
            UserIsLoggedIn = false;

            var userId = GetUserId();
            
            yield return bettrServer.Login(userId);
            
            UserIsLoggedIn = !bettrServer.AuthResponse.isError;
            
             if (UserIsLoggedIn)
            {
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                yield return bettrServer.LoadUserBlob(storageCallback: (_, payload, success, error) =>
                {
                    if (error == ErrorBlobDoesNotExist)
                    {
                        replaceBlob = true;
                        return;
                    }
                    if (!success)
                    {
                        Debug.LogError($"Error loading user blob: {error}");
                        return;
                    }
                    var userBlob = JsonConvert.DeserializeObject<BettrUserConfig>(payload.value);
                    BettrUserConfig = userBlob;
                });
            
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (replaceBlob)
                {
                    yield return LoadUserJsonFromWebAssets((_, payload, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error loading user JSON: {error}");
                            return;
                        }
                        var user = JsonConvert.DeserializeObject<BettrUserConfig>(payload.value);
                        user.UserId = userId; // device id
                        BettrUserConfig = user;
                    });
                    
                    // put this back into the server
                    yield return bettrServer.PutUserBlob(BettrUserConfig, (_, _, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error putting user blob: {error}");
                        }
                    });
                }
            }
            TileController.AddToGlobals("BettrUser", BettrUserConfig);
        }
        
        public IEnumerator LoadUserJsonFromWebAssets(GetStorageCallback storageCallback)
        {
            string webAssetName = "users/default/user.json";
            string assetBundleURL = $"{configData.AssetsServerBaseURL}/{webAssetName}";
            using UnityWebRequest www = UnityWebRequest.Get(assetBundleURL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] jsonBytes = www.downloadHandler.data;
                string jsonData = Encoding.UTF8.GetString(jsonBytes);
                StorageResponse response = new StorageResponse()
                {
                    cas = null,
                    value = jsonData,
                };
                storageCallback(assetBundleURL, response, true, null);
            }
            else
            {
                var error = $"Error loading user JSON from server: {www.error}";
                Debug.LogError(error);
                storageCallback?.Invoke(assetBundleURL, null, false, error);
            }
        }
    }
}