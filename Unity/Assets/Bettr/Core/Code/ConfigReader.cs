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
        public static string DefaultTaskCode { get; } = "00000"; 
        
        public string AssetsVersion { get; set; }
        
        public string AssetsBaseURL { get; set; }
        public string MainBundleName { get; set; }
        public string MainBundleVariant { get; set; }
        
        public string OutcomesBaseURL { get; set; }
        
        public string ServerURL { get; set; }
        
        public bool UseFileSystemAssetBundles { get; set; }
        
        public bool UseFileSystemOutcomes { get; set; }
        
        public string TaskCode => PlayerPrefs.GetString(TaskCodeKey, DefaultTaskCode);
        
#if UNITY_IOS
        public string WebAssetsBaseURL => TaskCode.Equals(DefaultTaskCode) ? 
            $"{AssetsBaseURL}/assets/{AssetsVersion}/iOS" : 
            $"{AssetsBaseURL}/developers/{TaskCode}/assets/iOS";
#endif
#if UNITY_ANDROID
        public string WebAssetsBaseURL => $"{AssetsBaseURL}/assets/{AssetsVersion}/Android";
#endif
#if UNITY_WEBGL
        public string WebAssetsBaseURL => $"{AssetsBaseURL}/assets/{AssetsVersion}/WebGL";
#endif
#if UNITY_STANDALONE_OSX
        public string WebAssetsBaseURL => $"{AssetsBaseURL}/assets/{AssetsVersion}/OSX";
#endif
        
        public string WebOutcomesBaseURL => $"{OutcomesBaseURL}";
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

