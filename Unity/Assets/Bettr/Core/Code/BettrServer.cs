using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void GetCallback(string requestURL, byte[] payload, bool success, string error);
    public delegate void PostCallback(string requestURL, List<IMultipartFormSection> formSections, byte[] payload, bool success, string error);
    
    public delegate void PutCallback(string requestURL, byte[] response, bool success, string error);

    public interface IBettrServer
    {
        IEnumerator LoadServerOutcome(string gameId);
        
        IEnumerator Get(string requestUri, GetCallback callback, params KeyValuePair<string, string>[] headers);

        IEnumerator Post(string requestURL, List<IMultipartFormSection> formSections, PostCallback callback,
            params KeyValuePair<string, string>[] headers);

        IEnumerator Put(string requestUri, byte[] bodyData, PutCallback callback, params KeyValuePair<string, string>[] headers);
        
        KeyValuePair<string, string> ApplicationJsonHeader => new KeyValuePair<string, string>("Content-Type", "application/json");
    }
    
    public class BettrServer : IBettrServer
    {
        public string ServerBaseURL { get; private set; }
        
        public KeyValuePair<string, string> ApplicationJsonHeader => new KeyValuePair<string, string>("Content-Type", "application/json");
        
        public BettrServer(string serverBaseURL)
        {
            ServerBaseURL = serverBaseURL;
            
            TileController.RegisterType<BettrServer>("BettrServer");
            TileController.AddToGlobals("BettrServer", this);
        }
        
        public IEnumerator LoadServerOutcome(string gameId)
        {
            yield break;
        }

        public IEnumerator Get(string requestUri, GetCallback callback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{ServerBaseURL}{requestUri}";
            var www = UnityWebRequest.Get(requestURL);
            if (headers != null)
            {
                foreach (var kvPair in headers)
                {
                    www.SetRequestHeader(kvPair.Key, kvPair.Value);
                }              
            }
            
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                callback(requestURL, null, false, www.error);
                yield break;
            }
            
            callback(requestURL, www.downloadHandler.data, true, null);
        }

        public IEnumerator Post(string requestURL, List<IMultipartFormSection> formSections, PostCallback callback, params KeyValuePair<string, string>[] headers)
        {
            var www = UnityWebRequest.Post(requestURL, formSections);
            if (headers != null)
            {
                foreach (var kvPair in headers)
                {
                    www.SetRequestHeader(kvPair.Key, kvPair.Value);
                }              
            }
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(requestURL, null, null,false, www.error);
                yield break;
            }
            
            callback(requestURL, formSections, www.downloadHandler.data, true, null);
        }
        
        public IEnumerator Put(string requestUri, byte[] bodyData, PutCallback callback, params KeyValuePair<string, string>[] headers)
        {
            var requestURL = $"{ServerBaseURL}/{requestUri}";
            
            // Check if bodyData is null or empty
            if (bodyData == null || bodyData.Length == 0)
            {
                Debug.LogError("Body data for PUT request is null or empty.");
                callback(requestUri, null, false, "Body data is null or empty.");
                yield break;
            }
            
            // Create a new UnityWebRequest with the PUT method
            var www = new UnityWebRequest(requestURL, "PUT")
            {
                uploadHandler = new UploadHandlerRaw(bodyData),
                downloadHandler = new DownloadHandlerBuffer()
            };
            if (headers != null)
            {
                foreach (var kvPair in headers)
                {
                    www.SetRequestHeader(kvPair.Key, kvPair.Value);
                }              
            }

            yield return www.SendWebRequest();

            // Check for errors
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
    }
}