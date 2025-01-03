using System;
using System.IO;
using UnityEngine;
using YamlDotNet.Serialization;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class ConfigData
    {
        public static string TaskCodeKey { get; }  = "Bettr__TASK_CODE_KEY";
        public static string DefaultTaskCode { get; } = "abc123"; 
        
        public string AssetsVersion { get; set; }
        
        public string AssetsServerBaseURL { get; set; }
        
        public string AudioServerBaseURL { get; set; }
        
        public string VideoServerBaseURL { get; set; }
        public string OutcomesServerBaseURL { get; set; }
        
        public string ServerBaseURL { get; set; }
        
        public bool UseFileSystemAssetBundles { get; set; }
        
        public bool UseFileSystemAudio { get; set; }
        
        public bool UseFileSystemOutcomes { get; set; }
        
        public bool UseLocalServer { get; set; }
        
        public string TaskCode => PlayerPrefs.GetString(TaskCodeKey, DefaultTaskCode);
        
// Begin WebAssetsBaseURL       
        
#if UNITY_IOS
        public string WebAssetsBaseURL => $"{AssetsServerBaseURL}/assets/{AssetsVersion}/iOS";
#endif
#if UNITY_ANDROID
        public string WebAssetsBaseURL => $"{AssetsServerBaseURL}/assets/{AssetsVersion}/Android";
#endif
#if UNITY_WEBGL
        public string WebAssetsBaseURL => $"{AssetsServerBaseURL}/assets/{AssetsVersion}/WebGL";
#endif
#if UNITY_STANDALONE_OSX
        public string WebAssetsBaseURL => $"{AssetsServerBaseURL}/assets/{AssetsVersion}/OSX";
#endif
        
// End WebAssetsBaseURL        
        
        
        public string WebOutcomesBaseURL => $"{OutcomesServerBaseURL}";
    }

    public static class ConfigReader
    {
        public static ConfigData Parse(string yamlText)
        {
            if (yamlText == null)
            {
                Debug.LogError("Config.yaml yamlText is not assigned.");
                return null;
            }

            DeserializerBuilder deserializerBuilder = new DeserializerBuilder();
            var deserializer = deserializerBuilder.Build();

            // Deserialize the YAML content from configFile.text into a C# data structure
            using var reader = new StringReader(yamlText);
            var configData = deserializer.Deserialize<ConfigData>(reader);

            return configData;
        }
    }
}

