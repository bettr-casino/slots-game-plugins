using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using CrayonScript.Code;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using Random = UnityEngine.Random;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class BettrOutcomeRequestPayload
    {
        [JsonProperty("gameId")]
        public string GameId { get; set; }
        
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("hashKey")]
        public string HashKey { get; set; }
    }
    
    [Serializable]
    public class BettrOutcomeResponsePayload
    {
        [JsonProperty("outcome_file_content_base64")]
        public string OutcomeFileContentBase64 { get; set; }
        
        [JsonProperty("outcome_number")]
        public int OutcomeNumber { get; set; }
        
        [JsonProperty("digital_signature_base64")]
        public string DigitalSignatureBase64 { get; set; }
        
        [JsonProperty("digital_signature_payload_str")]
        public string DigitalSignaturePayloadStr { get; set; }
    }
    
    [Serializable]
    public class BettrOutcomeController
    {
        [NonSerialized]  public bool UseFileSystemOutcomes = true;
        
        [NonSerialized]  public string FileSystemOutcomesBaseURL = "Assets/Bettr/LocalStore/Outcomes";
        
        // ReSharper disable once UnassignedField.Global
        [NonSerialized]  public string WebOutcomesBaseURL;
        
        public int OutcomeNumber { get; set; }
        
        public string HashKey { get; private set; }
        
        public BettrAssetScriptsController BettrAssetScriptsController { get; private set; }
        
        public BettrUserController BettrUserController { get; private set; }

        public BettrOutcomeController(BettrAssetScriptsController bettrAssetScriptsController, BettrUserController bettrUserController, string hashKey)
        {
            TileController.RegisterType<BettrOutcomeController>("BettrOutcomeController");
            TileController.AddToGlobals("BettrOutcomeController", this);

            this.BettrAssetScriptsController = bettrAssetScriptsController;
            this.BettrUserController = bettrUserController;
            this.HashKey = hashKey;
        }
        
        public IEnumerator LoadServerOutcome(string gameId)
        {
            // Load the Outcomes
            if (UseFileSystemOutcomes)
            {
                yield return LoadFileSystemOutcome(gameId);
            }
            else
            {
                yield return LoadWebOutcome(gameId);
            }
        }
        
        IEnumerator LoadWebOutcome(string gameId)
        {
            var bettrOutcomeRequestPayload = new BettrOutcomeRequestPayload()
            {
                GameId = gameId,
                UserId = BettrUserController.BettrUserConfig.UserId,
                HashKey = HashKey,
            };

            var requestPayload = JsonConvert.SerializeObject(bettrOutcomeRequestPayload);
            
            using UnityWebRequest www = UnityWebRequest.Post(WebOutcomesBaseURL, requestPayload, "application/json");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                yield break;
            }
            
            try
            {
                var responseJson = www.downloadHandler.text;
                var responsePayload = JsonConvert.DeserializeObject<BettrOutcomeResponsePayload>(responseJson);

                var scriptBase64 = responsePayload.OutcomeFileContentBase64;
                var script = Encoding.UTF8.GetString(Convert.FromBase64String(scriptBase64));
                
                var outcomeNumber = responsePayload.OutcomeNumber;
                
                var className = $"{gameId}Outcome{outcomeNumber:09}";
                
                BettrAssetScriptsController.AddScript(className, script);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing response JSON: {ex.Message}");
                throw;
            }
        }

        IEnumerator LoadFileSystemOutcome(string gameId)
        {
            var outcomeNumber = (OutcomeNumber > 0) ? OutcomeNumber : GetRandomOutcomeNumber(gameId);
            var className = $"{gameId}Outcome{outcomeNumber:D9}";
            var assetBundleManifestURL = $"{FileSystemOutcomesBaseURL}/{className}.cscript.txt";
            var assetBundleManifestBytes = File.ReadAllBytes(assetBundleManifestURL);

            var script = Encoding.ASCII.GetString(assetBundleManifestBytes);

            BettrAssetScriptsController.AddScript(className, script);
            
            yield break;
        }

        private int GetRandomOutcomeNumber(string gameId)
        {
            var regex = new Regex($@"^{gameId}Outcome\d{{9}}\.cscript.txt$");
            var files = Directory.GetFiles($"{FileSystemOutcomesBaseURL}");
            var filteredFiles = files.Where(file => regex.IsMatch(Path.GetFileName(file))).ToArray();
            var outcomeCount = filteredFiles.Length;
            return outcomeCount > 0 ? Random.Range(1, outcomeCount + 1) : 0;
        }
    }
}