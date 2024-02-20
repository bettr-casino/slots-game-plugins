using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void GetOutcomesCallback(string requestURL, byte[] payload, bool success, string error);
    
    public class BettrOutcomesServer
    {
        public string ServerBaseURL { get; private set; }
        
        public BettrOutcomesServer(string serverBaseURL)
        {
            ServerBaseURL = serverBaseURL;
            
            TileController.RegisterType<BettrServer>("BettrServer");
            TileController.AddToGlobals("BettrServer", this);
        }
        
        public IEnumerator LoadServerOutcome(string gameId)
        {
            yield break;
        }

        public IEnumerator Get(string requestUri, GetOutcomesCallback callback)
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
    }
}