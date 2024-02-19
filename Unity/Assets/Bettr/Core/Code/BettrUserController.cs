using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using CrayonScript.Code;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using UnityEngine.Serialization;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class AuthRequest
    {
        [JsonProperty("username")]
        public string username;
        
        [JsonProperty("create_user")]
        public bool createUser;
    }

    [Serializable]
    public class AuthResponse
    {
        [JsonProperty("user")]
        public AnonUser User { get; set; }
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
    public class UserBlobResponse
    {
        [JsonProperty("value")]
        public string value;
        
        [JsonProperty("cas")]
        public string cas;
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

    public interface IBettrUserController
    {
        BettrUserConfig BettrUserConfig { get; }
        string GetUserId();
        IEnumerator Login();
        IEnumerator LoadUserBlob();
        IEnumerator PutUserBlob();
    }
    
    [Serializable]
    public class BettrUserController : IBettrUserController
    {
        public IBettrServer BettrServer { get; private set; }
        
        public BettrUserConfig BettrUserConfig { get; private set; }
        
        public AuthResponse AuthResponse { get; private set; }
        
        public UserBlobResponse UserBlobResponse { get; private set; }
        
        public bool UserIsLoggedIn { get; private set; }
        
        public string SessionToken { get; private set; }
        
        public KeyValuePair<string, string> SessionTokenHeader => new KeyValuePair<string, string>("Token", AuthResponse.User.SessionToken);
        
        const string ErrorUserBlobDoesNotExist = "HTTP/1.1 404 Not Found";

        private static BettrUserConfig DefaultBettrUserConfig => new BettrUserConfig()
        {
            Coins = 1000,
            XP = 0,
            Level = 1,
            
            LobbyScene = new BettrSceneConfig()
            {
                BundleName = "mainlobby",
                BundleVersion = "v0_1_0",
            },
            
            LobbyCards = new List<BettrLobbyCardConfig>()
            {
                new()
                {
                    Group = "New Releases",
                    MachineName = "Game001",
                    BundleName = "lobbycard",
                    BundleVersion = "v0_1_0",
                    PrefabName = "Game001LobbyCard",
                    Format = "standard"
                },
                new()
                {
                    Group = "New Releases",
                    MachineName = "Game002",
                    BundleName = "lobbycard",
                    BundleVersion = "v0_1_0",
                    PrefabName = "Game002LobbyCard",
                    Format = "standard"
                }
            }
            
            
        };

        private UserBlobResponse DefaultUserBlobResponse => new UserBlobResponse()
        {
            value = JsonConvert.SerializeObject(DefaultBettrUserConfig),
        };
        
        public BettrUserController(IBettrServer bettrServer)
        {
            TileController.RegisterType<BettrUserController>("BettrUserController");
            TileController.AddToGlobals("BettrUserController", this);
            
            TileController.RegisterType<BettrUserConfig>("BettrUserConfig");
            
            TileController.RegisterType<AuthRequest>("AuthRequest");
            TileController.RegisterType<AuthResponse>("AuthResponse");

            BettrServer = bettrServer;
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

            UserIsLoggedIn = false;
            SessionToken = "";
            
            var userId = GetUserId();
            var authRequest = new AuthRequest()
            {
                username = userId,
                createUser = true,
            };
            
            string jsonPayload = JsonConvert.SerializeObject(authRequest);
            byte[] bodyData = Encoding.UTF8.GetBytes(jsonPayload);
            byte[] downloadedPayload = null;
            
            yield return BettrServer.Put("auth/login/anon", bodyData, (url, payload, success, error) =>
            {
                if (!success)
                {
                    Debug.LogError($"Login error: url={url} error={error}");
                    return;
                }

                if (payload.Length == 0)
                {
                    Debug.LogError("Empty payload retrieved from url=" + url);
                    return;
                }

                downloadedPayload = payload;
            }, BettrServer.ApplicationJsonHeader);
            
            if (downloadedPayload != null)
            {
                AuthResponse = JsonConvert.DeserializeObject<AuthResponse>(Encoding.UTF8.GetString(downloadedPayload));
                TileController.AddToGlobals("AuthResponse", AuthResponse);
                SessionToken = AuthResponse.User.SessionToken;
                UserIsLoggedIn = true;
            }
        }
        
        public IEnumerator LoadUserBlob()
        {
            Debug.Log($"Starting LoadUserBlob");
            
            UserBlobResponse = new UserBlobResponse();
            BettrUserConfig = new BettrUserConfig();

            if (!UserIsLoggedIn)
            {
                Debug.LogError("Cannot load UserBlob. User is not logged in.");
                yield break;
            }
            
            var userBlobExists = true;

            string requestUri = $"storage/owner/{AuthResponse.User.Id}/protected/blobs/user";
            yield return BettrServer.Get(requestUri, (requestURL, payload, success, error) =>
            {
                
                if (error == ErrorUserBlobDoesNotExist)
                {
                    userBlobExists = false;
                    return;
                }
                
                if (!success)
                {
                    Debug.LogError($"Error retrieving user blob: {error}");
                    return;
                }

                if (payload == null || payload.Length == 0)
                {
                    Debug.LogError("Empty payload retrieved.");
                    return;
                }

                string responseJson = Encoding.UTF8.GetString(payload);
                Debug.Log($"User blob response: {responseJson}");

                try
                {
                    UserBlobResponse = JsonConvert.DeserializeObject<UserBlobResponse>(responseJson);
                    BettrUserConfig = JsonConvert.DeserializeObject<BettrUserConfig>(UserBlobResponse.value);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error parsing JSON response: {e.Message}");
                }
            }, SessionTokenHeader, BettrServer.ApplicationJsonHeader);
            
            if (!userBlobExists)
            {
                Debug.Log("User blob does not exist. Creating new default user blob.");
                UserBlobResponse = DefaultUserBlobResponse;
                BettrUserConfig = JsonConvert.DeserializeObject<BettrUserConfig>(UserBlobResponse.value);
            }
        }
        
        public IEnumerator PutUserBlob()
        {
            if (!UserIsLoggedIn)
            {
                Debug.LogError("User is not logged in.");
                yield break;
            }

            string requestUri = $"storage/owner/{AuthResponse.User.Id}/protected/blobs/user";

            // Create the UserBlobRequest object
            UserBlobRequest userBlobRequest = new UserBlobRequest
            {
                value = JsonConvert.SerializeObject(BettrUserConfig),
                create = true,
                cas = UserBlobResponse.cas,
            };

            // Serialize the UserBlobRequest object to JSON, excluding properties with null values
            string jsonPayload = JsonConvert.SerializeObject(userBlobRequest, new JsonSerializerSettings
            {
                ContractResolver = new IgnoreZeroValueContractResolver(),
                NullValueHandling = NullValueHandling.Ignore,
            });

            // Convert the JSON payload to a byte array
            byte[] bodyData = Encoding.UTF8.GetBytes(jsonPayload);

            yield return BettrServer.Put(requestUri, bodyData, (url, payload, success, error) =>
            {
                if (!success)
                {
                    Debug.LogError($"Error putting user blob: {error}");
                    return;
                }

                Debug.Log("User blob successfully updated.");
            }, SessionTokenHeader, BettrServer.ApplicationJsonHeader);
            
        }

    }
}