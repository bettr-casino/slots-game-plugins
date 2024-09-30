using System;
using System.Collections;
using System.Collections.Generic;
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

        [JsonProperty("gameVariantId")]
        public string GameVariantId { get; set; }
        
        [JsonProperty("userId")]
        public string UserId { get; set; }
        
        [JsonProperty("hashKey")]
        public string HashKey { get; set; }
        
        [JsonProperty("useGeneratedOutcomes")]
        public bool UseGeneratedOutcomes { get; set; }
        
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
        
        [NonSerialized]  public string FileSystemOutcomesBaseURL = "Assets/Bettr/LocalStore/LocalOutcomes";

        [NonSerialized] public string GeneratedOutcomesServerBaseURL =
            "https://bettr-casino-generated-outcomes.s3.us-west-2.amazonaws.com";
        
        // ReSharper disable once UnassignedField.Global
        [NonSerialized]  public string WebOutcomesBaseURL;

        [NonSerialized] public Dictionary<string, int> OutcomeCounts = new Dictionary<string, int>();
        
        public int OutcomeNumber { get; set; }
        
        public string HashKey { get; private set; }
        
        public BettrAssetScriptsController BettrAssetScriptsController { get; private set; }
        
        public BettrUserController BettrUserController { get; private set; }
        
        public BettrExperimentController BettrExperimentController { get; private set; }

        public BettrOutcomeController(BettrAssetScriptsController bettrAssetScriptsController, BettrUserController bettrUserController, BettrExperimentController experimentController, string hashKey)
        {
            TileController.RegisterType<BettrOutcomeController>("BettrOutcomeController");
            TileController.AddToGlobals("BettrOutcomeController", this);

            this.BettrAssetScriptsController = bettrAssetScriptsController;
            this.BettrUserController = bettrUserController;
            this.BettrExperimentController = experimentController;
            this.HashKey = hashKey;
        }
        
        public IEnumerator LoadServerOutcome(string gameId, string gameVariantId)
        {
            // Load the Outcomes
            if (UseFileSystemOutcomes)
            {
                yield return LoadFileSystemOutcome(gameId, gameVariantId);
            }
            else
            {
                yield return LoadWebOutcome(gameId, gameVariantId);
            }
        }
        
        // TODO: FIXME: Include the variant 
        IEnumerator LoadWebOutcome(string gameId, string gameVariantId)
        {
            // check the experiment "Outcomes"
            var useGeneratedOutcomes = BettrExperimentController.UseGeneratedOutcomes();

            if (OutcomeNumber > 0)
            {
                if (BettrUserController.UserInDevMode)
                {
                    var devOutcomeClassName = $"{gameId}Outcome{OutcomeNumber:000000000}.cscript.txt";
                    
                    string outcomeURL = $"{GeneratedOutcomesServerBaseURL}/latest/Outcomes/{gameId}/{gameVariantId}/{devOutcomeClassName}";
                    using UnityWebRequest wwws3 = UnityWebRequest.Get(outcomeURL);
                    yield return wwws3.SendWebRequest();

                    if (wwws3.result == UnityWebRequest.Result.Success)
                    {
                        var script = wwws3.downloadHandler.text;
                
                        BettrAssetScriptsController.AddScript(devOutcomeClassName, script);
                    }
                    else
                    {
                        var error = $"Error loading Outcome={devOutcomeClassName} from server: {wwws3.error}";
                        Debug.LogError(error);
                    }

                    OutcomeNumber = 0;
                        
                    yield break;
                }
            }
            
            var bettrOutcomeRequestPayload = new BettrOutcomeRequestPayload()
            {
                GameId = gameId,
                GameVariantId = gameVariantId,
                UserId = BettrUserController.BettrUserConfig.UserId,
                HashKey = HashKey,
                UseGeneratedOutcomes = useGeneratedOutcomes
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
                
                var className = $"{gameId}Outcome{outcomeNumber:000000000}";
                
                BettrAssetScriptsController.AddScript(className, script);
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error parsing response JSON: {ex.Message}");
                throw;
            }
        }

        IEnumerator LoadFileSystemOutcome(string gameId, string gameVariantId)
        {
            var outcomeNumber = (OutcomeNumber > 0) ? OutcomeNumber : GetRandomOutcomeNumber(gameId, gameVariantId);
            var className = $"{gameId}Outcome{outcomeNumber:D9}";
            var fileName = $"{className}.cscript.txt";
            var filePath = Path.Combine(FileSystemOutcomesBaseURL, gameId, gameVariantId, fileName);
            var assetBundleManifestURL = filePath;
            var assetBundleManifestBytes = File.ReadAllBytes(assetBundleManifestURL);

            var script = Encoding.ASCII.GetString(assetBundleManifestBytes);

            BettrAssetScriptsController.AddScript(className, script);
            
            yield break;
        }

        private int GetRandomOutcomeNumber(string gameId, string gameVariantId)
        {
            if (OutcomeCounts.TryGetValue(gameId, out var count))
            {
                if (count > 0)
                {
                    return Random.Range(1, count + 1);
                }
            }
            
            var regex = new Regex($@"^{gameId}Outcome\d{{9}}\.cscript.txt$");
            var directoryPath = Path.Combine(FileSystemOutcomesBaseURL, gameId, gameVariantId);
            var files = Directory.GetFiles(directoryPath);
            var filteredFiles = files.Where(file => regex.IsMatch(Path.GetFileName(file))).ToArray();
            var outcomeCount = filteredFiles.Length;
            
            OutcomeCounts[gameId] = outcomeCount;
            
            return outcomeCount > 0 ? Random.Range(1, outcomeCount + 1) : 0;
        }
    }
}