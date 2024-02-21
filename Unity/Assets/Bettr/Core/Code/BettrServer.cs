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
    public delegate void GetCallback(string requestURL, byte[] payload, bool success, string error);
    public delegate void PostCallback(string requestURL, List<IMultipartFormSection> formSections, byte[] payload, bool success, string error);
    
    public delegate void PutCallback(string requestURL, byte[] response, bool success, string error);
    
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
            string jsonPayload = JsonConvert.SerializeObject(authRequest);
            byte[] bodyData = Encoding.UTF8.GetBytes(jsonPayload);
            yield return Put(requestUri, bodyData, (requestURL, payload, success, error) =>
            {
                if (!success)
                {
                    var errorResponse = new AuthResponse
                    {
                        error = $"User JSON retrieval error: url={requestURL} error={error}"
                    };
                    Debug.LogError(errorResponse.error);
                }
                else if (payload.Length == 0)
                {
                    var errorResponse = new AuthResponse
                    {
                        error = "empty payload retrieved from url={url}"
                    };
                    Debug.LogError(errorResponse.error);
                }
                else
                {
                    AuthResponse = JsonConvert.DeserializeObject<AuthResponse>(Encoding.UTF8.GetString(payload));
                }
            }, ApplicationJsonHeader);
        }
        
        public IEnumerator LoadUserBlob(GetCallback callback)
        {
            Debug.Log($"Starting LoadUserBlob");
            if (useLocalServer)
            {
                string localFilePath = Path.Combine(fileSystemLocalStorageBaseURL, "users", $"{AuthResponse.User.Id}.json");
                if (File.Exists(localFilePath))
                {
                    string json = File.ReadAllText(localFilePath);
                    byte[] payload = Encoding.UTF8.GetBytes(json);
                    callback(localFilePath, payload, true, null);
                }
                else
                {
                    var error = $"Local user blob file not found at path: {localFilePath}";
                    Debug.LogError(error);
                    callback(localFilePath, null, false, error);
                }
                yield break;
            }
            var requestUri = $"storage/owner/{AuthResponse.User.Id}/protected/blobs/user";
            yield return Get(requestUri, callback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator PutUserBlob(byte[] bodyData, PutCallback callback)
        {
            Debug.Log($"Starting PutUserBlob");
            if (useLocalServer)
            {
                yield break;
            }
            string requestUri = $"storage/owner/{AuthResponse.User.Id}/protected/blobs/user";
            yield return Put(requestUri, bodyData, callback, SessionTokenHeader, ApplicationJsonHeader);
        }
        
        public IEnumerator Get(string requestUri, GetCallback callback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            var www = UnityWebRequest.Get(requestURL);
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                callback(requestURL, null, false, www.error);
                yield break;
            }
            callback(requestURL, www.downloadHandler.data, true, null);
        }

        public IEnumerator Put(string requestUri, byte[] bodyData, PutCallback callback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{configData.ServerBaseURL}{requestUri}";
            if (bodyData == null || bodyData.Length == 0)
            {
                Debug.LogError("Body data for PUT request is null or empty.");
                callback(requestUri, null, false, "Body data is null or empty.");
                yield break;
            }
            var www = new UnityWebRequest(requestURL, "PUT")
            {
                uploadHandler = new UploadHandlerRaw(bodyData),
                downloadHandler = new DownloadHandlerBuffer()
            };
            UpdateHeaders(www, headers);
            yield return www.SendWebRequest();
            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(requestUri, null, false, www.error);
            }
            else
            {
                callback(requestUri, www.downloadHandler.data, true, null);
            }
        }

        private void UpdateHeaders(UnityWebRequest www, params KeyValuePair<string, string>[] headers)
        {
            if (headers != null)
            {
                foreach (var kvPair in headers)
                {
                    www.SetRequestHeader(kvPair.Key, kvPair.Value);
                }              
            }
        }
    }
}