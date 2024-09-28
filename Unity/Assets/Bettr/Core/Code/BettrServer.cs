using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CrayonScript.Code;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void GetStorageCallback(string requestURL, StorageResponse storageResponse, bool success, string error);

    public delegate void PutStorageCallback(string requestURL, StorageResponse response, bool success, string error);
    
    public delegate void PutUserCallback(string requestURL, AuthResponse response, bool success, string error);
    
    public delegate void GetUserExperimentsCallback(string requestURL, UserExperimentsResponse userExperimentsResponse, bool success, string error);
    
    [Serializable]
    public class StorageRequest
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
    public class StorageResponse
    {
        [JsonProperty("value")]
        public string value;
        
        [JsonProperty("cas")]
        public string cas;
    }
    
    [Serializable]
    public class AuthRequest
    {
        [JsonProperty("username")]
        public string username;
        
        [JsonProperty("create_user")]
        public bool createUser;
    }
    
    [Serializable]
    public class AnonUser
    {
        [JsonProperty("id")]
        public string Id { get; set; }
        
        [JsonProperty("created_at")]
        public long CreatedAt { get; set; }
        
        [JsonProperty("session_token")]
        public string SessionToken { get; set; }
        
        [JsonProperty("refreshed_at")]
        public long RefreshedAt { get; set; }
        
        [JsonProperty("token_validity_seconds")]
        public long TokenValiditySeconds { get; set; }
        
        [JsonProperty("is_verified")]
        public bool IsVerified { get; set; }
        
        [JsonProperty("is_banned")]
        public bool IsBanned { get; set; }
        
        [JsonProperty("tags")]
        public List<string> Tags { get; set; }
        
        [JsonProperty("created")]
        public bool Created { get; set; }
        
        [JsonProperty("first_login")]
        public long FirstLogin { get; set; }
    }
    
    [Serializable]
    public class AuthResponse
    {
        [JsonProperty("user")]
        public AnonUser User { get; set; }
        public string error;
        public bool isLocal;
        public bool isError;
    }
    
    [Serializable]
    public class UserExperimentsResponse
    {
        [JsonProperty("user_experiments")]
        public List<BettrUserExperiment> UserExperiments { get; set; }
    }
    
    public class IgnoreZeroValueContractResolver : DefaultContractResolver
    {
        protected override JsonProperty CreateProperty(System.Reflection.MemberInfo member, MemberSerialization memberSerialization)
        {
            var property = base.CreateProperty(member, memberSerialization);

            // Check if the property type is an integer or a floating-point number
            if (property.PropertyType == typeof(int) || property.PropertyType == typeof(long) ||
                property.PropertyType == typeof(float) || property.PropertyType == typeof(double) ||
                property.PropertyType == typeof(decimal))
            {
                // Set a custom value provider that returns null for values equal to 0
                property.ValueProvider = new NullZeroValueProvider(property.ValueProvider);
            }

            return property;
        }

        private class NullZeroValueProvider : IValueProvider
        {
            private readonly IValueProvider _underlyingValueProvider;

            public NullZeroValueProvider(IValueProvider underlyingValueProvider)
            {
                _underlyingValueProvider = underlyingValueProvider;
            }

            public object GetValue(object target)
            {
                var value = _underlyingValueProvider.GetValue(target);
                if (value is int intValue && intValue == 0 ||
                    value is long longValue && longValue == 0 ||
                    value is float floatValue && floatValue == 0 ||
                    value is double doubleValue && doubleValue == 0 ||
                    value is decimal decimalValue && decimalValue == 0)
                {
                    return null;
                }

                return value;
            }

            public void SetValue(object target, object value)
            {
                _underlyingValueProvider.SetValue(target, value);
            }
        }
    }
    
    [Serializable]
    public class BettrServer
    {
        public bool useLocalServer = true;
        public string fileSystemLocalStorageBaseURL = "Assets/Bettr/LocalStore/LocalServer";

        public ConfigData configData;
        
        public KeyValuePair<string, string> ApplicationJsonHeader => new KeyValuePair<string, string>("Content-Type", "application/json");
        
        public KeyValuePair<string, string> SessionTokenHeader => new KeyValuePair<string, string>("Token", AuthResponse.User.SessionToken);
        
        public AuthResponse AuthResponse { get; private set; }
        
        public Dictionary<string, string> CasValues = new Dictionary<string, string>();
        
        public BettrServer()
        {
            TileController.RegisterType<BettrServer>("BettrServer");
            TileController.AddToGlobals("BettrServer", this);
        }
        
        public IEnumerator Login(string userId)
        {
            if (useLocalServer)
            {
                AuthResponse = new AuthResponse() { isLocal = true, User = new AnonUser()
                {
                    Id = userId,
                }};
                yield break;
            }
            var requestUri = "/auth/login/anon";
            var authRequest = new AuthRequest()
            {
                username = userId,
                createUser = true,
            };
            yield return PutUser(requestUri, authRequest, (requestURL, authResponse, success, error) =>
            {
                if (!success)
                {
                    var errorResponse = new AuthResponse
                    {
                        error = $"User JSON retrieval error: url={requestURL} error={error}"
                    };
                    Debug.LogError(errorResponse.error);
                }
                else if (authResponse == null)
                {
                    var errorResponse = new AuthResponse
                    {
                        error = "empty payload retrieved from url={url}"
                    };
                    Debug.LogError(errorResponse.error);
                }
                else
                {
                    AuthResponse = authResponse;
                }
            }, ApplicationJsonHeader);
        }
        
        public IEnumerator LoadUserBlob(GetStorageCallback storageCallback)
        {
            Debug.Log($"Starting LoadUserBlob");
            if (useLocalServer)
            {
                string localFilePath = Path.Combine(fileSystemLocalStorageBaseURL, "users", $"default.json");
                if (File.Exists(localFilePath))
                {
                    string json = File.ReadAllText(localFilePath);
                    StorageResponse storageResponse = new StorageResponse()
                    {
                        value = json,
                        cas = "local",
                    };
                    storageCallback(localFilePath, storageResponse, true, null);
                }
                else
                {
                    var error = $"Local user blob file not found at path: {localFilePath}";
                    Debug.LogError(error);
                    storageCallback(localFilePath, null, false, error);
                }
                yield break;
            }
            var requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/user";
            yield return GetStorage(requestUri, storageCallback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator PutUserBlob(BettrUserConfig userData, PutStorageCallback storageCallback)
        {
            Debug.Log($"Starting PutUserBlob");
            if (useLocalServer)
            {
                yield break;
            }
            string bodyData = JsonConvert.SerializeObject(userData);
            string requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/user";
            yield return PutStorage(requestUri, bodyData, 0, true, storageCallback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator LoadMechanicBlob(string mechanicName, GetStorageCallback storageCallback)
        {
            Debug.Log($"Starting LoadGameBlob");
            if (useLocalServer)
            {
                string localFilePath = Path.Combine(fileSystemLocalStorageBaseURL, "users", $"default.json");
                if (File.Exists(localFilePath))
                {
                    string json = File.ReadAllText(localFilePath);
                    StorageResponse storageResponse = new StorageResponse()
                    {
                        value = json,
                        cas = "local",
                    };
                    storageCallback(localFilePath, storageResponse, true, null);
                }
                else
                {
                    var error = $"Local mechanic blob file not found at path: {localFilePath}";
                    Debug.LogError(error);
                    storageCallback(localFilePath, null, false, error);
                }
                yield break;
            }
            var requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/mechanic__{mechanicName}";
            yield return GetStorage(requestUri, storageCallback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator PutMechanicBlob(BettrMechanicConfig mechanicConfig, PutStorageCallback storageCallback)
        {
            Debug.Log($"Starting PutMechanicBlob");
            if (useLocalServer)
            {
                yield break;
            }
            string bodyData = JsonConvert.SerializeObject(mechanicConfig);
            string requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/mechanic__{mechanicConfig.MechanicName}";
            yield return PutStorage(requestUri, bodyData, 0, true, storageCallback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator LoadEvents(GetStorageCallback storageCallback)
        {
            Debug.Log($"Starting PutEvent");
            if (useLocalServer)
            {
                yield break;
            }
            string requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/events";
            yield return GetStorage(requestUri, storageCallback, ApplicationJsonHeader);
        }
        
        public IEnumerator PutEvents(BettrUserEvents userEvents, PutStorageCallback storageCallback)
        {
            Debug.Log($"Starting PutEvent");
            if (useLocalServer)
            {
                yield break;
            }
            string bodyData = JsonConvert.SerializeObject(userEvents);
            string requestUri = $"/storage/owner/{AuthResponse.User.Id}/protected/blobs/events";
            yield return PutStorage(requestUri, bodyData, 0, true, storageCallback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator GetStorage(string requestUri, GetStorageCallback storageCallback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            var www = UnityWebRequest.Get(requestURL);
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                storageCallback(requestURL, null, false, www.error);
                yield break;
            }
            // update the cas value
            byte[] bytesData = www.downloadHandler.data;
            var storageResponse = JsonConvert.DeserializeObject<StorageResponse>(Encoding.UTF8.GetString(bytesData));
            CasValues[requestUri] = storageResponse.cas;
            storageCallback(requestURL, storageResponse, true, null);
        }
        
        public IEnumerator PutStorage(string requestUri, string value, long ttl, bool create, PutStorageCallback storageCallback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            if (string.IsNullOrEmpty(value))
            {
                Debug.LogError("Body data for PUT request is null or empty.");
                storageCallback(requestUri, null, false, "Body data is null or empty.");
                yield break;
            }

            CasValues.TryGetValue(requestUri, out var cas);

            var storageRequest = new StorageRequest()
            {
                value = value,
                ttl = ttl,
                create = create,
                cas = cas,
            };
            
            string jsonPayload = JsonConvert.SerializeObject(storageRequest, new JsonSerializerSettings()
            {
                ContractResolver = new IgnoreZeroValueContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
            
            var www = new UnityWebRequest(requestURL, "PUT")
            {
                uploadHandler = new UploadHandlerRaw(bodyBytes),
                downloadHandler = new DownloadHandlerBuffer()
            };
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                storageCallback(requestUri, null, false, www.error);
            }
            else
            {
                byte[] bytesData = www.downloadHandler.data;
                var storageResponse = JsonConvert.DeserializeObject<StorageResponse>(Encoding.UTF8.GetString(bytesData));
                CasValues[requestUri] = storageResponse.cas;
                storageCallback(requestUri, storageResponse, true, null);
            }
        }

        public IEnumerator PutUser(string requestUri, AuthRequest authRequest, PutUserCallback userCallback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            string jsonPayload = JsonConvert.SerializeObject(authRequest);
            byte[] bodyBytes = Encoding.UTF8.GetBytes(jsonPayload);
            
            var www = new UnityWebRequest(requestURL, "PUT")
            {
                uploadHandler = new UploadHandlerRaw(bodyBytes),
                downloadHandler = new DownloadHandlerBuffer()
            };
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                userCallback(requestUri, null, false, www.error);
            }
            else
            {
                byte[] bytesData = www.downloadHandler.data;
                var authResponse = JsonConvert.DeserializeObject<AuthResponse>(Encoding.UTF8.GetString(bytesData));
                userCallback(requestUri, authResponse, true, null);
            }
        }
        
        public IEnumerator GetUserExperiments(GetUserExperimentsCallback userExperimentsCallback)
        {
            string userId = AuthResponse.User.Id;
            var requestUri = $"/experiments/users/{userId}?include_inactive_experiments={UnityWebRequest.EscapeURL("true")}";
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            if (useLocalServer)
            {
                var localUserExperiments = new UserExperimentsResponse()
                {
                    UserExperiments = new List<BettrUserExperiment>(),
                };
                userExperimentsCallback(requestURL, localUserExperiments, true, null);
                yield break;
            }
            var headers = new KeyValuePair<string, string>[]
            {
                ApplicationJsonHeader,
                SessionTokenHeader,
            };
            var www = UnityWebRequest.Get(requestURL);
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                userExperimentsCallback(requestURL, null, false, www.error);
                yield break;
            }
            // update the cas value
            byte[] bytesData = www.downloadHandler.data;
            var userExperimentsResponse = JsonConvert.DeserializeObject<UserExperimentsResponse>(Encoding.UTF8.GetString(bytesData));
            // no cas for this response
            userExperimentsCallback(requestURL, userExperimentsResponse, true, null);
        }

        private void UpdateHeaders(UnityWebRequest www, params KeyValuePair<string, string>[] headers)
        {
            if (headers != null)
            {
                foreach (var kvPair in headers)
                {
                    www.SetRequestHeader(kvPair.Key, kvPair.Value);
                    Debug.Log("Header: " + kvPair.Key + " = " + kvPair.Value);
                }              
            }
        }
    }
}