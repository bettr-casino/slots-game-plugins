using System;
using System.Collections;
using System.Collections.Generic;
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
        
        public bool UserIsLoggedIn { get; private set; }
        
        const string ErrorBlobDoesNotExist = "HTTP/1.1 404 Not Found";

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
                yield return bettrServer.LoadUserBlob(storageCallback: (_, payload, success, error) =>
                {
                    if (error == ErrorBlobDoesNotExist)
                    {
                        userDoesNotExist = true;
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

                if (userDoesNotExist)
                {
                    yield return LoadUserJsonFromWebAssets((_, payload, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error loading user JSON: {error}");
                            return;
                        }
                        var user = JsonConvert.DeserializeObject<BettrUserConfig>(payload.value);
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
                    // this is eminently hackable, but it's just a demo
                    // TODO: move this to snapser
                    var events = new BettrUserEvents()
                    {
                        Events = new List<BettrUserEvent>(),
                    };
                    yield return bettrServer.LoadEvents((_, response, success, error) =>
                    {
                        if (error == ErrorBlobDoesNotExist)
                        {
                            userDoesNotExist = true;
                            return;
                        }
                        
                        if (!success)
                        {
                            Debug.LogError($"Error putting new user gift event: {error}");
                            return;
                        }

                        events = JsonConvert.DeserializeObject<BettrUserEvents>(response.value);
                    });
                    // create a new user event and fail if already exists
                    var newUserGiftEvent = new BettrUserEvent()
                    {
                        Persistent = true,
                        Acked = false,
                        EventId = "NewUserGift",
                        Value = "1000",
                    };
                    events.Events.Add(newUserGiftEvent);
                    yield return bettrServer.PutEvents(events, (_, _, success, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError($"Error putting new user gift event: {error}");
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