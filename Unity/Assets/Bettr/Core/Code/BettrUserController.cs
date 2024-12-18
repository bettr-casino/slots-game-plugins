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
        
        public BettrUserConfig BettrPreviewUserConfig { get; private set; }
        
        
        public static BettrUserController Instance { get; private set; }
        
        public bool UserIsLoggedIn { get; private set; }
        
        public bool UserInDevMode { get; private set; }

        public bool UserInPreviewMode { get; private set; }
        
        public bool UserInSlamStopMode { get; private set; }
        
        public int UserPreviewModeSpins { get; private set; }

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
        
        public void DisableUserPreviewMode()
        {
            UserInPreviewMode = false;
            UserPreviewModeSpins = 0;
            TileController.AddToGlobals("BettrUser", BettrUserConfig);
            BettrPreviewUserConfig.Coins = 0;
        }

        public void EnableUserPreviewMode()
        {
            UserInPreviewMode = true;
            UserPreviewModeSpins = 5; // TODO: FIXME: hardcoded spins for preview mode
            TileController.AddToGlobals("BettrUser", BettrPreviewUserConfig);
            // reset the BettrPreviewUserConfig Coins to 1000000
            BettrPreviewUserConfig.Coins = 1000000; // TODO: FIXME: hardcoded coins for preview mode
        }
        
        public void InitUserInSlamStopMode()
        {
            UserInSlamStopMode = false;
        }
        
        public void EnableUserInSlamStopMode()
        {
            UserInSlamStopMode = true;
        }
        
        public void DisableUserInSlamStopMode()
        {
            UserInSlamStopMode = false;
        }
        
        public IEnumerator SetUserDevMode()
        {
            UserInDevMode = false;
            
            var userId = GetUserId();
            
            string webAssetName = $"users/default/devs/{userId}.json";
            string assetURL = $"{configData.AssetsServerBaseURL}/{webAssetName}";
            using UnityWebRequest www = UnityWebRequest.Get(assetURL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] jsonBytes = www.downloadHandler.data;
                if (jsonBytes != null && jsonBytes.Length > 0)
                {
                    UserInDevMode = true;
                }
            }
            else
            {
                var error = $"Error loading user JSON from server: {www.error}";
                Debug.LogError(error);
            }
            Debug.Log($"UserId={userId} UserInDevMode={UserInDevMode}");
        }

        public IEnumerator Login()
        {
            bool insertUserBlob = true;
            Debug.Log($"Starting User Login");
            
            BettrUserConfig = null;
            BettrPreviewUserConfig = null;
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
                        insertUserBlob = true;
                        return;
                    }
                    if (!success)
                    {
                        Debug.LogError($"Error loading user blob: {error}");
                        return;
                    }
                    string result = Encoding.UTF8.GetString(payload.value);
                    var userBlob = JsonConvert.DeserializeObject<BettrUserConfig>(result);
                    BettrUserConfig = userBlob;
                });
            
                // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                if (insertUserBlob)
                {
                    yield return LoadDefaultUserJsonFromWebAssets((_, payload, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error loading user JSON: {error}");
                            return;
                        }
                        string result = Encoding.UTF8.GetString(payload.value);
                        var user = JsonConvert.DeserializeObject<BettrUserConfig>(result);
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
             // Create the BettrPreviewUserConfig
             this.BettrPreviewUserConfig = new BettrUserConfig()
             {
                 UserId = "PreviewUser",
                 Coins = 0,
                 // TODO: FIXME adjust Level and XP as needed
             };
             
            TileController.AddToGlobals("BettrUser", BettrUserConfig);
        }
        
        public IEnumerator LoadDefaultUserJsonFromWebAssets(GetStorageCallback storageCallback)
        {
            string webAssetName = "users/default/user.json";
            string assetBundleURL = $"{configData.AssetsServerBaseURL}/{webAssetName}";
            using UnityWebRequest www = UnityWebRequest.Get(assetBundleURL);
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                byte[] bytes = www.downloadHandler.data;
                StorageResponse response = new StorageResponse()
                {
                    cas = null,
                    value = bytes,
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