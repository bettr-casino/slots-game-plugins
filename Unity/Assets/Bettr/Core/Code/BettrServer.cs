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
    
    public class BettrServer
    {
        public string ServerBaseURL { get; private set; }
        
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

        public IEnumerator Get(string requestUri, GetCallback callback)
        {
            var requestURL = $"{ServerBaseURL}{requestUri}";
            var www = UnityWebRequest.Get(requestURL);
            yield return www.SendWebRequest();
            
            if (www.result != UnityWebRequest.Result.Success) {
                Debug.Log(www.error);
                callback(requestURL, null, false, www.error);
                yield break;
            }
            
            callback(requestURL, www.downloadHandler.data, true, null);
            
        }

        public IEnumerator Post(string requestURL, List<IMultipartFormSection> formSections, PostCallback callback)
        {
            var www = UnityWebRequest.Post(requestURL, formSections);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(www.error);
                callback(requestURL, null, null,false, www.error);
                yield break;
            }
            
            callback(requestURL, formSections, www.downloadHandler.data, true, null);
        }
    }
}