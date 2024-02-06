using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using DirectoryInfo = System.IO.DirectoryInfo;

namespace Bettr.Editor
{
    
    public class DirectoryNode
    {
        public string Name;
        public List<DirectoryNode> Children = new List<DirectoryNode>();
        public List<string> Files = new List<string>();
    }
    
    public static class BettrCustomMenu
    {
        private const string PLUGIN_ROOT_DIRECTORY = "Assets/Bettr/Runtime/Plugin";
        private const string ASSET_BUNDLES_DIRECTORY = "Assets/Bettr/LocalStore/AssetBundles";
        private const string ASSET_BUNDLES_IOS_DIRECTORY = ASSET_BUNDLES_DIRECTORY + "/iOS";
        private const string ASSET_BUNDLES_OSX_DIRECTORY = ASSET_BUNDLES_DIRECTORY + "/OSX";
        private const string ASSET_BUNDLES_ANDROID_DIRECTORY = ASSET_BUNDLES_DIRECTORY + "/Android";
        private const string ASSET_BUNDLES_WEBGL_DIRECTORY = ASSET_BUNDLES_DIRECTORY + "/WebGL";
        private const string ASSET_BUNDLES_WINDOWS_DIRECTORY = ASSET_BUNDLES_DIRECTORY +  "/Windows";
        private const string ASSET_BUNDLES_LINUX_DIRECTORY = ASSET_BUNDLES_DIRECTORY +  "/Linux";
        private const string OUTCOMES_DIRECTORY = "Assets/Bettr/LocalStore/Outcomes";
        
        [MenuItem("Bettr/Tools/Export Module Unity Package")]
        public static void ExportPackage()
        {
            // Base directory to prepend
            string baseDirectory = "Assets/Bettr/Runtime/Plugin/";

            // Get the moduleName path from command line arguments
            string moduleName = GetArgument("-moduleName");

            // Validate the moduleName path
            if (string.IsNullOrEmpty(moduleName))
            {
                Console.WriteLine("-moduleName path not provided or invalid.");
                return;
            }

            // Create the full module path
            string fullModulePath = baseDirectory + moduleName;

            // Get the outputDirectory path from command line arguments
            string outputDirectory = GetArgument("-outputDirectory");

            // Generate directory tree JSON
            string directoryTreeJson = GenerateDirectoryTreeJson(fullModulePath);

            Debug.Log(directoryTreeJson);

            // JSON file path
            string jsonFilePath = $"{outputDirectory}/{moduleName}/{moduleName}-tree.json";
            CreateDirectory(jsonFilePath);

            // Save JSON to the module path
            File.WriteAllText(jsonFilePath, directoryTreeJson);

            // Validate the outputDirectory path
            if (string.IsNullOrEmpty(outputDirectory))
            {
                Console.WriteLine("-outputDirectory path not provided or invalid.");
                return;
            }

            string outputPackagePath = $"{outputDirectory}/{moduleName}/{moduleName}.unitypackage";
            CreateDirectory(outputPackagePath);

            // Export the package
            AssetDatabase.ExportPackage(fullModulePath, outputPackagePath, ExportPackageOptions.Recurse);

            // Optional: Log to confirm package creation
            Debug.Log("Package exported: " + outputPackagePath);
        }

        [MenuItem("Bettr/Bettr/Rebuild Assets")] 
        public static void BuildAssets()
        {
            BuildAssetBundles();
            BuildOutcomes();
        }
        
        [MenuItem("Bettr/Bettr/Cleanup Test Scenes")] 
        public static void CleanupTestScenes()
        {
            RemoveTestScenes(new DirectoryInfo("Assets/"));
        }

        private static void BuildAssetBundles()
        {
            Debug.Log("Building asset bundles...");
            
            EnsurePluginAssetsHaveLabels(PLUGIN_ROOT_DIRECTORY);
            
            Debug.Log("...refreshing database before building asset bundles..");
            AssetDatabase.Refresh();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.StrictMode |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_IOS_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_IOS_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_OSX_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_OSX_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            
#endif
#if UNITY_ANDROID
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_ANDROID_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_ANDROID_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_WEBGL_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_WEBGL_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_OSX_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_OSX_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_WINDOWS_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_WINDOWS_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EmptyDirectory(new DirectoryInfo(ASSET_BUNDLES_LINUX_DIRECTORY));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(ASSET_BUNDLES_LINUX_DIRECTORY, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneLinux64);
#endif
            
            Debug.Log("...refreshing database after building asset bundles..");
            AssetDatabase.Refresh();
            
            Debug.Log("Modifying asset bundles manifest files...");
            ModifyAssetBundleManifestFiles();
            Debug.Log("...done modifying asset bundles manifest files.");
            
            Debug.Log("...refreshing database after modifying asset bundles..");
            AssetDatabase.Refresh();
            
            Debug.Log("...done building asset bundles.");
            
#if UNITY_IOS
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);
#endif
#if UNITY_ANDROID
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Standalone, BuildTarget.StandaloneLinux64);
#endif            
            
        }
        
        private static void EnsurePluginAssetsHaveLabels(string pluginRootDirectory)
        {
            if (string.IsNullOrEmpty(pluginRootDirectory))
            {
                return;
            }
            
            var pluginDirectories = Directory.GetDirectories(pluginRootDirectory);
            
            foreach (var pluginDirectory in pluginDirectories)
            {
                var assetLabel = ReadAssetLabel(pluginDirectory);
                if (!string.IsNullOrEmpty(assetLabel))
                {
                    var variantsRootDirectory = Path.Combine(pluginDirectory, "variants");
                    var variantsDirectories = Directory.GetDirectories(variantsRootDirectory);
                    foreach (var variantDirectory in variantsDirectories)
                    {
                        var assetSubLabel = ReadAssetSubLabel(variantDirectory);
                        var pluginRuntimeDirectory = Path.Combine(variantDirectory, "Runtime");
                        WalkDirectoryRecursive(pluginRuntimeDirectory, assetLabel, assetSubLabel);
                    }
                }
            }
        }
        
        private static string ReadAssetLabel(string directoryPath)
        {
            var di = new DirectoryInfo(directoryPath);
            var baseName = di.Name.ToLower();
            return baseName;
        }
        
        private static string ReadAssetSubLabel(string directoryPath)
        {
            var di = new DirectoryInfo(directoryPath);
            var baseName = di.Name.ToLower();
            return baseName;
        }
        
        private static void WalkDirectoryRecursive(string directoryPath, string assetLabel, string assetSubLabel)
        {
            if (Path.GetFileNameWithoutExtension(directoryPath).Equals("Editor")) return; // skip special Editor folder
            if (assetSubLabel.Contains('.')) throw new Exception("Asset sub label cannot contain a period.");
            var importer = AssetImporter.GetAtPath(directoryPath);
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(directoryPath);
            if (importer != null)
            {
                if (assetType != null && assetType != typeof(MonoScript))
                {
                    importer.assetBundleName = GetAssetBundleName(assetLabel, assetType, directoryPath);
                    importer.assetBundleVariant = GetAssetBundleVariant(assetSubLabel, assetType, directoryPath);
                }
            }
            
            var files = Directory.GetFiles(directoryPath);
            foreach (var file in files)
            {
                var assetPath = file;
                if (assetPath.EndsWith(".meta")) continue;
                if (assetPath.EndsWith(".DS_Store")) continue;
                importer = AssetImporter.GetAtPath(assetPath);
                assetType = AssetDatabase.GetMainAssetTypeAtPath(assetPath);
                if (importer != null)
                {
                    if (assetType != null && assetType != typeof(MonoScript))
                    {
                        importer.assetBundleName = GetAssetBundleName(assetLabel, assetType, assetPath);
                        importer.assetBundleVariant = GetAssetBundleVariant(assetSubLabel, assetType, assetPath);
                    }
                }
            }

            var subDirectories = Directory.GetDirectories(directoryPath);
            foreach (var subDirectory in subDirectories)
            {
                WalkDirectoryRecursive(subDirectory, assetLabel, assetSubLabel);
            }
        }

        private static void EmptyDirectory(this DirectoryInfo directory)
        {
            foreach(FileInfo file in directory.GetFiles()) file.Delete();
            foreach(DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }

        private static string GetAssetBundleName(string assetLabel, Type assetType, string directoryPath)
        {
            var isScene = assetType.Name == "SceneAsset";
            var suffix = isScene ? "_scenes" :"";
            var assetBundleName = $"{assetLabel}{suffix}";
            return assetBundleName;
        }
        
        private static string GetAssetBundleVariant(string assetSubLabel, Type assetType, string directoryPath)
        {
            return assetSubLabel;
        }
        
        private static void ModifyAssetBundleManifestFiles()
        {
            var files = Directory.GetFiles(ASSET_BUNDLES_DIRECTORY);
            foreach (var file in files)
            {
                if (file.EndsWith(".manifest"))
                {
                    try
                    {
                        var yamlContent = File.ReadAllText(file);
                        var deserializer = new DeserializerBuilder()
                            .WithNamingConvention(CamelCaseNamingConvention.Instance)
                            .Build();
                        var yamlObject = deserializer.Deserialize<Dictionary<string, object>>(yamlContent);
                        
                        if (yamlObject.TryGetValue("Assets", out var assets))
                        {
                            if (assets is List<object> assetList)
                            {
                                // Sort assetList so that all files ending in .cscript.txt are first
                                var sortedAssetList = assetList
                                    .OrderByDescending(asset => asset.ToString().EndsWith(".cscript.txt"))
                                    .ToList();

                                yamlObject["Assets"] = sortedAssetList;

                                // Serialize the updated yamlObject and save it
                                var serializer = new SerializerBuilder()
                                    .WithNamingConvention(CamelCaseNamingConvention.Instance)
                                    .Build();
                                var updatedYamlContent = serializer.Serialize(yamlObject);
                                File.WriteAllText(file, updatedYamlContent);
                            }
                            else
                            {
                                Debug.LogWarning("The 'Assets' key does not contain a list.");
                            }
                        }

                        // Process the YAML object as needed
                        // For example, you can modify the YAML content or just log it
                        Debug.Log(yamlObject);
                    }
                    catch (Exception ex)
                    {
                        Debug.LogError($"Error processing manifest file '{file}': {ex.Message}");
                    }
                }
            }
        }

        private static void RemoveTestScenes(this DirectoryInfo directory)
        {
            Debug.Log($"Removing test scenes under {directory.Name} ...");
            
            var regexPatterns = new Regex[]
            {
                new("^InitTestScene[0-9]+.unity$"),
                new("^InitTestScene[0-9]+.unity.meta$")
            };

            foreach (FileInfo file in directory.GetFiles())
            {
                foreach (var regexPattern in regexPatterns)
                {
                    if (regexPattern.IsMatch(file.Name))
                    {
                        file.Delete();
                    }
                }
            }
            
            Debug.Log("...refreshing database after removing test scenes..");
            AssetDatabase.Refresh();
        }

        private static void BuildOutcomes()
        {
            Debug.Log("Building outcomes...");
            
            EmptyDirectory(new DirectoryInfo(OUTCOMES_DIRECTORY));
            AssetDatabase.Refresh();
            
            Debug.Log("...refreshing database before building outcomes..");
            AssetDatabase.Refresh();
            
            var outcomeDirectories = Directory.GetDirectories(PLUGIN_ROOT_DIRECTORY, "Outcomes", SearchOption.AllDirectories);
            foreach (var outcomeDirectory in outcomeDirectories)
            {
                var outcomeFiles = new DirectoryInfo(outcomeDirectory).GetFiles("*.cscript.txt");
                foreach (var outcomeFile in outcomeFiles)
                {
                    var outcomeFilePath = Path.Combine(OUTCOMES_DIRECTORY, outcomeFile.Name);
                    File.Copy(outcomeFile.FullName, outcomeFilePath); }
                
            }
            
            Debug.Log("...refreshing database after building outcomes..");
            AssetDatabase.Refresh();
            
            Debug.Log("...done building outcomes.");
        }
        
        // Method to extract a specific command line argument
        private static string GetArgument(string name)
        {
            string[] args = Environment.GetCommandLineArgs();
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
            return null;
        }

        private static string GenerateDirectoryTreeJson(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var tree = GetDirectoryTree(dir);
            return JsonConvert.SerializeObject(tree, Formatting.Indented);
        }
        
        private static string[] excludedFileNames = { ".DS_Store", ".meta" };
        
        private static bool IsExcluded(string fileName)
        {
            return excludedFileNames.Any(excluded => fileName.EndsWith(excluded, StringComparison.OrdinalIgnoreCase));
        }

        private static DirectoryNode GetDirectoryTree(DirectoryInfo directoryInfo)
        {
            var directoryNode = new DirectoryNode { Name = directoryInfo.Name };
            foreach (var directory in directoryInfo.GetDirectories())
            {
                directoryNode.Children.Add(GetDirectoryTree(directory)); // Recursion
            }
            foreach (var file in directoryInfo.GetFiles())
            {
                if (!IsExcluded(file.Name))
                {
                    directoryNode.Files.Add(file.Name);
                }
            }
            return directoryNode;
        }

        // Call this method to create a directory
        private static void CreateDirectory(string path)
        {
            // Get the directory path from the file path
            path = Path.GetDirectoryName(path);
            
            // Check if the directory already exists
            if (!Directory.Exists(path))
            {
                // Create the directory
                Directory.CreateDirectory(path);

                // Refresh the AssetDatabase to show the new directory in Unity Editor
                AssetDatabase.Refresh();

                Debug.Log($"Directory created at: {path}");
            }
            else
            {
                Debug.Log($"Directory already exists at: {path}");
            }
        }
        
    }
}