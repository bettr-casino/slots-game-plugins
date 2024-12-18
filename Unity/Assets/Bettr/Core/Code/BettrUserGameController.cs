using System.Collections;
using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Networking;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrUserGameController
    {
        public BettrServer bettrServer;

        public ConfigData configData;
        
        public BettrAssetScriptsController BettrAssetScriptsController { get; private set; }
        public static BettrUserGameController Instance { get; private set; }
        
        const string ErrorBlobDoesNotExist = "HTTP/1.1 404 Not Found";
        
        public BettrUserGameController(BettrAssetScriptsController scriptsController)
        {
            TileController.RegisterType<BettrUserGameController>("BettrUserGameController");
            TileController.AddToGlobals("BettrUserGameController", this);
            
            BettrAssetScriptsController = scriptsController;
            
            Instance = this;
        }
        
        public IEnumerator LoadUserGameTables()
        {
            Debug.Log($"Starting User Game Load");
            
            var userGameScriptText = "";

            // ReSharper disable once ConditionIsAlwaysTrueOrFalse
            yield return bettrServer.LoadUserGameBlob(storageCallback: (_, payload, success, error) =>
            {
                if (error == ErrorBlobDoesNotExist)
                {
                    return;
                }
                if (!success)
                {
                    Debug.LogError($"Error loading user game blob: {error}");
                    return;
                }
                string result = Encoding.UTF8.GetString(payload.value);
                userGameScriptText = result;
            });
            
            yield return LoadDefaultUserGameScriptTextFromWebAssets((_, payload, success, error) =>
            {
                if (!success)
                {
                    Debug.LogError($"Error loading user JSON: {error}");
                    return;
                }
                string result = Encoding.UTF8.GetString(payload.value);
                userGameScriptText = $"\n\n{result}\n\n\n\n\n\n{userGameScriptText}";
            });

            if (!string.IsNullOrWhiteSpace(userGameScriptText))
            {
                BettrAssetScriptsController.AddScript("user__game.cscript.txt", userGameScriptText);
            }
        }
        
        public IEnumerator LoadDefaultUserGameScriptTextFromWebAssets(GetStorageCallback storageCallback)
        {
            string webAssetName = "users/default/user__game.cscript.txt";
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
                var error = $"Error loading user game cscript txt from server: {www.error}";
                Debug.LogError(error);
                storageCallback?.Invoke(assetBundleURL, null, false, error);
            }
        }

        public string ConvertTableToLuaCode(Table table, int indent = 0)
        {
            var builder = new StringBuilder();
            string indentStr = new string(' ', indent * 2);
            builder.AppendLine(indentStr + "{");

            foreach (var pair in table.Pairs)
            {
                string key = FormatKey(pair.Key);
                string value = FormatValue(pair.Value, indent + 1);
                builder.AppendLine($"{indentStr}  {key} = {value},");
            }

            builder.AppendLine(indentStr + "}");
            return builder.ToString();
        }

        private static string FormatKey(DynValue key)
        {
            if (key.Type == DataType.String)
                return $"[{EscapeString(key.String)}]";
            if (key.Type == DataType.Number)
                return $"[{key.Number}]";

            // Handle other key types (e.g., user-defined objects)
            return $"[\"{key.ToString()}\"]";
        }

        private string FormatValue(DynValue value, int indent)
        {
            switch (value.Type)
            {
                case DataType.String:
                    return EscapeString(value.String);
                case DataType.Number:
                    return value.Number.ToString();
                case DataType.Boolean:
                    return value.Boolean ? "true" : "false";
                case DataType.Table:
                    return ConvertTableToLuaCode(value.Table, indent);
                case DataType.Nil:
                    return "nil";
                default:
                    return $"\"{value.ToString()}\""; // Fallback for unsupported types
            }
        }

        private static string EscapeString(string str)
        {
            return $"\"{str.Replace("\"", "\\\"")}\"";
        }

    }
}