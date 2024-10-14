using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using Bettr.Editor.generators;
using Bettr.Editor.generators.mechanics;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using Newtonsoft.Json;
using Scriban;
using Scriban.Runtime;
using TMPro;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using DirectoryInfo = System.IO.DirectoryInfo;
using Exception = System.Exception;
using Object = UnityEngine.Object;

namespace Bettr.Editor
{
    public class DirectoryNode
    {
        public string Name;
        // ReSharper disable once CollectionNeverQueried.Global
        internal readonly List<DirectoryNode> Children = new List<DirectoryNode>();
        // ReSharper disable once CollectionNeverQueried.Global
        internal readonly List<string> Files = new List<string>();
    }
    
    [Serializable]
    public class GameConfigsWrapper
    {
        // Dictionary to hold game configurations
        public Dictionary<string, GameDetails> GameConfigs { get; set; }
    }

    [Serializable]
    public class GameDetails
    {
        public int OutcomeCount { get; set; }
        public Dictionary<string, GameVariantDetails> GameVariantConfigs { get; set; }
    }
    
    [Serializable]
    public class GameVariantDetails
    {
        public int OutcomeCount { get; set; }
    }
    
    public static class BettrMenu
    {
        private const string PluginRootDirectory = "Assets/Bettr/Runtime/Plugin";
        private const string AssetBundlesDirectory = "Assets/Bettr/LocalStore/AssetBundles";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesIOSDirectory = AssetBundlesDirectory + "/iOS";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesOSXDirectory = AssetBundlesDirectory + "/OSX";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesAndroidDirectory = AssetBundlesDirectory + "/Android";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesWebglDirectory = AssetBundlesDirectory + "/WebGL";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesWindowsDirectory = AssetBundlesDirectory +  "/Windows";
        // ReSharper disable once UnusedMember.Local
        private const string AssetBundlesLinuxDirectory = AssetBundlesDirectory +  "/Linux";
        // ReSharper disable once UnusedMember.Local
        private const string OutcomesDirectory = "Assets/Bettr/LocalStore/Outcomes";
        // ReSharper disable once UnusedMember.Local
        private const string LocalServerDirectory = "Assets/Bettr/LocalStore/LocalServer";
        private const string LocalOutcomesDirectory = "Assets/Bettr/LocalStore/LocalOutcomes";
        private const string LocalAudioDirectory = "Assets/Bettr/LocalStore/LocalAudio";

        private const string LocalVideoDirectory = "Assets/Bettr/LocalStore/LocalVideo";
        private const string AssetsServerBaseURL = "https://bettr-casino-assets.s3.us-west-2.amazonaws.com";
        private const string OutcomesServerBaseURL = "https://bettr-casino-outcomes.s3.us-west-2.amazonaws.com";
        private const string GatewayUrl = "https://3wcgnl14qb.execute-api.us-west-2.amazonaws.com/gateway";
        
        private const string AUDIO_FORMAT = ".wav";
        
        private static HashSet<string> AssetLabelsCache = new HashSet<string>();
        private static HashSet<string> AssetVariantsCache = new HashSet<string>();
        
        [MenuItem("Bettr/Tools/Check Material Shader")]
        public static void CheckMaterialShaderFromMenu()
        {
            // Get the asset bundle path and material name using file panels
            string assetBundlePath =
                "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001epicancientadventures.epicancientadventures";

            // Input dialog for the material name
            string materialName = "F5.mat";

            if (string.IsNullOrEmpty(materialName))
            {
                Debug.LogError("Material name is invalid.");
                return;
            }

            // Call the method to check the material shader
            CheckMaterialShader(assetBundlePath, materialName);
        }

        public static void CheckMaterialShader(string assetBundlePath, string materialName)
        {
            // Load the asset bundle
            AssetBundle bundle = AssetBundle.LoadFromFile(assetBundlePath);
            if (bundle == null)
            {
                Debug.LogError("Failed to load AssetBundle!");
                return;
            }

            // Load the material
            Material material = bundle.LoadAsset<Material>(materialName);
            if (material == null)
            {
                Debug.LogError($"Material {materialName} not found in the AssetBundle.");
                bundle.Unload(false);
                return;
            }

            // Check the shader being used by the material
            Shader shader = material.shader;
            if (shader == null)
            {
                Debug.LogError($"Material {materialName} does not have a valid shader assigned.");
            }
            else
            {
                Debug.Log($"Material {materialName} is using shader: {shader.name}");

                // Compare if the shader matches the expected shader in the main build
                Shader expectedShader = Shader.Find("Bettr/Symbol"); // Replace with your actual shader path
                if (shader == expectedShader)
                {
                    Debug.Log("The material is using the correct shader.");
                }
                else
                {
                    Debug.LogError("The material is not using the expected shader.");
                }
            }

            // Always unload the bundle after use
            bundle.Unload(false);
        }
        
        [MenuItem("Bettr/Tools/Check Asset Bundle Load")]
        public static void CheckAssetBundleLoad()
        {
            string mainPath = "Assets/Bettr/LocalStore/AssetBundles/WebGL/main.v0_1_0";
            AssetBundle mainBundle = AssetBundle.LoadFromFile(mainPath);

            string bundle1Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001epicancientadventures.epicancientadventures";
            AssetBundle bundle1 = AssetBundle.LoadFromFile(bundle1Path);
            if (bundle1 == null)
            {
                Debug.LogError($"Bundle 1 AssetBundle at {bundle1Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 1 loaded successfully.");
            }
            string bundle2Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001epicatlantistreasures.epicatlantistreasures";
            AssetBundle bundle2 = AssetBundle.LoadFromFile(bundle2Path);
            if (bundle2 == null)
            {
                Debug.LogError($"Bundle 2 AssetBundle at {bundle2Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 2 loaded successfully.");
            }
            string bundle3Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001epicancientadventures_scenes.epicancientadventures";
            AssetBundle bundle3 = AssetBundle.LoadFromFile(bundle3Path);
            if (bundle3 == null)
            {
                Debug.LogError($"Bundle 3 AssetBundle at {bundle3Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 3 loaded successfully.");
            }
            string bundle4Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001epicatlantistreasures_scenes.epicatlantistreasures";
            AssetBundle bundle4 = AssetBundle.LoadFromFile(bundle4Path);
            if (bundle4 == null)
            {
                Debug.LogError($"Bundle 4 AssetBundle at {bundle4Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 4 loaded successfully.");
            }
            mainBundle.Unload(false);
            if (bundle1 != null) bundle1.Unload(false);
            if (bundle2 != null) bundle2.Unload(false);
            if (bundle3 != null) bundle3.Unload(false);
            if (bundle4 != null) bundle4.Unload(false);
        }

        [MenuItem("Bettr/Tools/Compare Asset Bundles")]
        public static void CompareAssetBundles()
        {
            string bundle1Path = EditorUtility.OpenFilePanel("Select first AssetBundle", "", "");
            if (string.IsNullOrEmpty(bundle1Path))
            {
                Debug.LogError($"AssetBundle path {bundle1Path} is not valid.");
                return;
            }
            string bundle1Name = Path.GetFileNameWithoutExtension(bundle1Path);
            AssetBundle bundle1 = AssetBundle.LoadFromFile(bundle1Path);
            if (bundle1 == null)
            {
                Debug.LogError($"AssetBundle at {bundle1Path} failed to load.");
                return;
            }
            List<string> bundle1Assets = new List<string>(bundle1.GetAllAssetNames());
            bundle1.Unload(false);

            string bundle2Path = EditorUtility.OpenFilePanel("Select second AssetBundle", "", "");
            if (string.IsNullOrEmpty(bundle2Path))
            {
                Debug.LogError($"AssetBundle path {bundle2Path} is not valid.");
                return;
            }
            string bundle2Name = Path.GetFileNameWithoutExtension(bundle2Path);
            AssetBundle bundle2 = AssetBundle.LoadFromFile(bundle2Path);
            if (bundle2 == null)
            {
                Debug.LogError($"AssetBundle at {bundle2Path} failed to load.");
                return;
            }
            List<string> bundle2Assets = new List<string>(bundle2.GetAllAssetNames());
            bundle2.Unload(false);

            CompareAssetNames(bundle1Path, bundle2Path, bundle1Assets, bundle2Assets);
            
            string manifest1Path = $"{bundle1Path}.manifest";
            string manifest2Path = $"{bundle2Path}.manifest";
            
            CompareAssetBundleDependencies(manifest1Path, manifest2Path);
        }

        private static void CompareAssetNames(string bundle1Path, string bundle2Path, List<string> bundle1Assets, List<string> bundle2Assets)
        {
            // Compare asset names
            List<string> common1Assets = new List<string>(bundle1Assets);
            common1Assets = common1Assets.Intersect(bundle2Assets).ToList();

            if (common1Assets.Count > 0)
            {
                Debug.Log($"{bundle1Path} has {bundle1Assets.Count} Assets, {common1Assets.Count} Common assets found in {bundle1Path} that are also in {bundle2Path} :");
            }
            else
            {
                Debug.Log("No common asset names found comparing bundle1 with bundle2");
            }

            List<string> common2Assets = new List<string>(bundle2Assets);
            common2Assets = common2Assets.Intersect(bundle1Assets).ToList();

            if (common2Assets.Count > 0)
            {
                Debug.Log($"{bundle2Path} has {bundle2Assets.Count} Assets, {common2Assets.Count} Common assets found in {bundle2Path} that are also in {bundle1Path} :");
            }
            else
            {
                Debug.Log("No common asset names found comparing bundle2 with bundle1");
            }

            if (common1Assets.Count > 0)
            {
                Debug.Log($"List of {common1Assets.Count} Common assets found in {bundle1Path} that are also in {bundle2Path} :");
                foreach (string asset in common1Assets)
                {
                    Debug.Log(asset);
                }
            }

            if (common2Assets.Count > 0)
            {
                Debug.Log($"List of {common2Assets.Count}  Common assets found in {bundle2Path} that are also in {bundle1Path} :");
                foreach (string asset in common2Assets)
                {
                    Debug.Log(asset);
                }
            }
        }

        public static void CompareAssetBundleDependencies(string manifest1Path, string manifest2Path)
        {
            // Helper function to read dependencies from the .manifest file
            List<string> LoadManifestDependencies(string manifestFilePath)
            {
                List<string> dependencies = new List<string>();

                // Read the manifest file as a text file
                string[] lines = File.ReadAllLines(manifestFilePath);

                // Find the "Dependencies:" section and capture the dependencies listed under it
                bool dependenciesSection = false;
                foreach (var line in lines)
                {
                    if (line.Contains("Dependencies:"))
                    {
                        dependenciesSection = true;
                        continue;
                    }

                    if (dependenciesSection)
                    {
                        if (string.IsNullOrWhiteSpace(line)) // End of the dependencies section
                            break;

                        dependencies.Add(line.Trim());
                    }
                }

                return dependencies;
            }

            // Load the dependencies for both manifest files
            List<string> dependencies1 = LoadManifestDependencies(manifest1Path);
            List<string> dependencies2 = LoadManifestDependencies(manifest2Path);

            // Compare the dependencies
            var commonDependencies = dependencies1.Intersect(dependencies2).ToList();

            // Output the results
            if (commonDependencies.Count > 0)
            {
                Debug.Log($"Found {commonDependencies.Count} common dependencies between {Path.GetFileName(manifest1Path)} and {Path.GetFileName(manifest2Path)}:");
                var index = 0;
                foreach (var dependency in commonDependencies)
                {
                    Debug.Log($"Common Dependency {index+1}: {dependency}");
                    index += 1;
                }
            }
            else
            {
                Debug.Log($"No common dependencies found between {Path.GetFileName(manifest1Path)} and {Path.GetFileName(manifest2Path)}.");
            }
        }


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
        
        [MenuItem("Tools/Update Prefab References")]
        static void UpdatePrefabReferences()
        {
            string newDirectoryPath = "Assets/Bettr/Runtime/Plugin/Game001Alpha"; // Path to the cloned directory
            string oldDirectoryPath = "Assets/Bettr/Runtime/Plugin/Game001"; // Path to the cloned directory
            
            // Step 1: Get all prefabs in the new directory
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { newDirectoryPath });

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                string[] prefabLines = File.ReadAllLines(prefabPath);

                for (int i = 0; i < prefabLines.Length; i++)
                {
                    if (prefabLines[i].Contains("guid:"))
                    {
                        string oldGuid = Regex.Match(prefabLines[i], "guid: (.+?),").Groups[1].Value;
                        string oldAssetPath = AssetDatabase.GUIDToAssetPath(oldGuid);

                        if (!string.IsNullOrEmpty(oldAssetPath) && oldAssetPath.StartsWith(oldDirectoryPath))
                        {
                            string newAssetPath = oldAssetPath.Replace(oldDirectoryPath, newDirectoryPath);
                            string newGuid = AssetDatabase.AssetPathToGUID(newAssetPath);

                            prefabLines[i] = prefabLines[i].Replace(oldGuid, newGuid);
                            Debug.Log($"Updated GUID in prefab: {prefabPath}");
                        }
                    }
                }

                File.WriteAllLines(prefabPath, prefabLines);
                AssetDatabase.Refresh();
            }

            Debug.Log("GUIDs updated successfully.");
        }

        // [MenuItem("Bettr/Install/Verify")]
        public static void VerifyInstall()
        {
            var canPost = PostToService();
            if (!canPost)
            {
                EditorUtility.DisplayDialog("Error", "Verify Failed: [ Gateway]", "OK");
                return;
            }
            
            EditorUtility.DisplayDialog("Success", "Verify Successful.", "OK");
        }
        
        [MenuItem("Bettr/PlayMode/Start/Rebuild Assets + WebGL")] 
        public static void StartRebuildAssetsWebGL()
        {
            // Ensure you are not in play mode when making these changes
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exiting play mode before executing this command.");
                EditorApplication.isPlaying = false;
            }

            CleanupTestScenes();
            BuildAssets();

            // Switch to iOS build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);

            // Path to your specific scene. Adjust the path as necessary.
            const string scenePath = "Assets/Bettr/Core/Scenes/MainScene.unity";

            // Open the specified scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.EnterPlaymode();
        }
        
        [MenuItem("Bettr/PlayMode/Start/WebGL")] 
        public static void StartWebGL()
        {
            // Ensure you are not in play mode when making these changes
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exiting play mode before executing this command.");
                EditorApplication.isPlaying = false;
            }

            // Switch to iOS build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.WebGL, BuildTarget.WebGL);
            
            BuildLocalServer();

            // Path to your specific scene. Adjust the path as necessary.
            const string scenePath = "Assets/Bettr/Core/Scenes/MainScene.unity";

            // Open the specified scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Bettr/PlayMode/Start/iOS")] 
        public static void StartiOS()
        {
            // Ensure you are not in play mode when making these changes
            if (EditorApplication.isPlaying)
            {
                Debug.LogWarning("Exiting play mode before executing this command.");
                EditorApplication.isPlaying = false;
            }

            // Switch to iOS build target
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            // Path to your specific scene. Adjust the path as necessary.
            const string scenePath = "Assets/Bettr/Core/Scenes/MainScene.unity";

            // Open the specified scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Bettr/Build/Assets/Dangerous/All")] 
        public static void BuildAssets()
        {
            BuildAssetBundles();
            BuildLocalServer();
            BuildLocalOutcomes();
        }
        
        public static HashSet<string> GetAllAssetLabels()
        {
            return AssetLabelsCache;
        }
        
        [MenuItem("Bettr/Build/Assets/SingleGame")]
        public static void BuildSingleGameAssetsAndAudio()
        {
            Debug.Log("Building asset bundles...");
            
            EnsurePluginAssetsHaveLabels(PluginRootDirectory);
            
            Debug.Log("...refreshing database before building asset bundles..");
            AssetDatabase.Refresh();
            
            var buildAssetLabel = "game001epicancientadventures";
            var assetLabels = GetAllAssetLabels();

            List<AssetBundleBuild> buildMapList = new List<AssetBundleBuild>();
            // loop over the AssetLabelsCache  
            foreach (var assetLabel in AssetLabelsCache)
            {
                foreach (var assetVariant in AssetVariantsCache)
                {
                    if (assetLabel.StartsWith(buildAssetLabel))
                    {
                        // create a new AssetBundleBuild
                        AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                        assetBundleBuild.assetBundleName = $"{assetLabel}.{assetVariant}";
                        assetBundleBuild.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleBuild.assetBundleName);
                        buildMapList.Add(assetBundleBuild);
                    }
                }
            }
            
            // convert buildMap to an array
            var buildMap = buildMapList.ToArray();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesIOSDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesIOSDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            AssetDatabase.Refresh();
            
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            AssetDatabase.Refresh();
            
#endif
#if UNITY_ANDROID
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesAndroidDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesAndroidDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWebglDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWebglDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWindowsDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWindowsDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesLinuxDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesLinuxDirectory, 
                buildMap,
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
            
            BuildLocalAudio(new List<string>() {buildAssetLabel});
            BuildLocalVideo();
        }
        
        public static void BuildGameAssets()
        {
            Debug.Log("Building asset bundles...");
            
            EnsurePluginAssetsHaveLabels(PluginRootDirectory);
            
            Debug.Log("...refreshing database before building asset bundles..");
            AssetDatabase.Refresh();
            
            // Build a custom set of asset bundles
            // extract the -game from the command line arguments
            string game = GetArgument("-game");
            Debug.Log($"Building asset bundles for game: {game}");
            // get all the asset labels
            var assetLabels = GetAllAssetLabels();
            // now filter the asset labels to get the ones that start with the game
            // ensure game is lowercase
            game = game.ToLower();
            var gameAssetLabels = assetLabels.Where(label => label.StartsWith(game)).ToArray();
            Debug.Log($"Found {gameAssetLabels.Length} asset labels for game: {game}");
            Debug.Log(string.Join(",", gameAssetLabels));
            // now generate a build map for the gameAssetLabels
            List<AssetBundleBuild> buildMapList = new List<AssetBundleBuild>();
            List<string> buildAssetLabels = new List<string>();
            // loop over the AssetLabelsCache  
            foreach (var assetLabel in AssetLabelsCache)
            {
                foreach (var assetVariant in AssetVariantsCache)
                {
                    // if the assetLabel starts with the game
                    if (assetLabel.StartsWith(game))
                    {
                        buildAssetLabels.Add(assetLabel);
                        // create a new AssetBundleBuild
                        AssetBundleBuild assetBundleBuild = new AssetBundleBuild();
                        assetBundleBuild.assetBundleName = $"{assetLabel}.{assetVariant}";
                        assetBundleBuild.assetNames = AssetDatabase.GetAssetPathsFromAssetBundle(assetBundleBuild.assetBundleName);
                        buildMapList.Add(assetBundleBuild);
                    }
                }
            }
            
            // convert buildMap to an array
            var buildMap = buildMapList.ToArray();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesIOSDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesIOSDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            AssetDatabase.Refresh();
            
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            AssetDatabase.Refresh();
            
#endif
#if UNITY_ANDROID
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesAndroidDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesAndroidDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWebglDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWebglDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWindowsDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWindowsDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesLinuxDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesLinuxDirectory, 
                buildMap,
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

        public static void BuildAssetsCommandLine()
        {
            string[] args = Environment.GetCommandLineArgs();

            // Default values for assetLabel and assetSubLabel
            string assetLabel = "";
            string assetSubLabel = "";

            // Find the labels passed via command line
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "-assetLabel" && i + 1 < args.Length)
                {
                    assetLabel = args[i + 1];
                }
                if (args[i] == "-assetSubLabel" && i + 1 < args.Length)
                {
                    assetSubLabel = args[i + 1];
                }
            }
            
            // throw an error if assetLabel is empty or assetSubLabel is empty or null
            if (string.IsNullOrEmpty(assetLabel) || string.IsNullOrEmpty(assetSubLabel))
            {
                Debug.LogError($"Asset Label or Asset Sub Label is empty or null. assetLabel={assetLabel} assetSubLabel={assetSubLabel}");
                return;
            }
            
            var assetBundleName = $"{assetLabel}.{assetSubLabel}";
            Debug.Log($"Building asset bundle: assetBundleName={assetBundleName}");
            
            EnsurePluginAssetsHaveLabels(PluginRootDirectory);
            
            AssetDatabase.Refresh();

            // Find assets that contain both assetLabel and assetSubLabel
            var selectedAssets = AssetDatabase.FindAssets("")
                .Select(AssetDatabase.GUIDToAssetPath)
                .Where(path => {
                    var importer = AssetImporter.GetAtPath(path);

                    if (importer != null)
                    {
                        // Get asset bundle name and variant
                        string bundleName = importer.assetBundleName;
                        string bundleVariant = importer.assetBundleVariant;

                        // Compare assetLabel with assetBundleName and assetSubLabel with assetBundleVariant
                        bool hasCorrectName = bundleName == assetLabel;
                        bool hasCorrectVariant = bundleVariant == assetSubLabel;

                        return hasCorrectName && hasCorrectVariant;
                    }

                    return false; // If no importer is found, return false
                })
                .ToArray();


            if (selectedAssets.Length == 0)
            {
                Debug.LogError($"No assets found with the specified labels assetLabel={assetLabel} assetSubLabel={assetSubLabel}");
                return;
            }
            
            // here are the selected assets
            Debug.Log($"Selected assets: {selectedAssets.Length}");
            Debug.Log(string.Join(",", selectedAssets));

            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = assetBundleName;
            buildMap[0].assetNames = selectedAssets;
            
            Debug.Log($"...refreshing database before building asset bundle assetBundleName={assetBundleName}...");
            AssetDatabase.Refresh();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesIOSDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesIOSDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            AssetDatabase.Refresh();
            
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            AssetDatabase.Refresh();
#endif
#if UNITY_ANDROID
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesAndroidDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesAndroidDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWebglDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWebglDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWindowsDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWindowsDirectory, 
                buildMap,
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesLinuxDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesLinuxDirectory, 
                buildMap,
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
        
        [MenuItem("Bettr/Tools/Check Prefabs for Background.jpg")]
        private static void CheckMaterialsForBackgroundTexture()
        {
            // Get all prefab assets in the project
            string[] prefabGUIDs = AssetDatabase.FindAssets("t:Prefab");

            bool textureFound = false;

            foreach (string guid in prefabGUIDs)
            {
                // Get the path of the prefab
                string prefabPath = AssetDatabase.GUIDToAssetPath(guid);
                
                // if prefabPath does not end with BackgroundFBX.prefab, skip
                if (!prefabPath.EndsWith("BackgroundFBX.prefab"))
                {
                    continue;
                }
                
                // skip if this is a variant1
                if (prefabPath.Contains("variant1"))
                {
                    continue;
                }
                
                // Load the prefab asset
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

                if (prefab != null)
                {
                    // Get all renderers in the prefab to access materials
                    Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>();

                    foreach (Renderer renderer in renderers)
                    {
                        // Check each material of the renderer
                        foreach (Material material in renderer.sharedMaterials)
                        {
                            if (material != null)
                            {
                                // Check if the material has a texture named "Background.jpg"
                                foreach (string propertyName in material.GetTexturePropertyNames())
                                {
                                    Texture texture = material.GetTexture(propertyName);

                                    if (texture != null && texture.name == "Background")
                                    {
                                        string texturePath = AssetDatabase.GetAssetPath(texture);

                                        if (texturePath.EndsWith("Background.jpg"))
                                        {
                                            Debug.Log($"Prefab '{prefab.name}' uses Material '{material.name}' with 'Background.jpg' at path: {prefabPath}");
                                            textureFound = true;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            
            if (!textureFound)
            {
                Debug.Log("No prefabs are using 'Background.jpg'.");
            }
        }

        [MenuItem("Bettr/Tools/Fix Symbols Material Alpha")]
        public static void FixSymbolsMaterialsAlpha()
        {
            int processCount = 0;
            
            Dictionary<string, string> loadedMachineModels = new Dictionary<string, string>();
            TileController.StaticInit();
            
            // Step 1: Get all prefabs in the new directory
            string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { PluginRootDirectory });

            foreach (string prefabGuid in prefabGuids)
            {
                string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
                // skip if it does not contain "/control/" 
                if (!prefabPath.Contains("/control/"))
                {
                    continue;
                }
                // get the prefab file name
                string prefabFileName = Path.GetFileName(prefabPath);
                // skip if it doesn't start with Game<NNN>BaseGameSymbol<SymbolName>
                // use regex to filter out the prefabs that are not symbols
                var regex = new Regex(@"(Game\d{3})BaseGameSymbol(\w+)");
                if (!regex.IsMatch(prefabFileName))
                {
                    continue;
                }
                // extract the machine name, and symbol name from the matched groups
                var match = regex.Match(prefabFileName);
                var machineName = match.Groups[1].Value;
                var symbolName = match.Groups[2].Value;
                
                // Assets/Bettr/Runtime/Plugin/Game006/variants/WheelsIndustrialRevolution/control/Runtime/Asset/Prefabs/Game006BaseGameReel5.prefab
                string runtimeAssetPath = Path.GetDirectoryName(Path.GetDirectoryName((prefabPath)));
                var machineModelName =$"{machineName}Models";

                if (!loadedMachineModels.ContainsKey(machineModelName))
                {
                    string modelDestinationPath = Path.Combine(runtimeAssetPath, "Models",  $"{machineModelName}.cscript.txt");
                    var modelTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(modelDestinationPath);
                    var machineModelScript = modelTextAsset.text;
                    loadedMachineModels.Add(machineModelName, machineModelScript);
                }
                
                var loadedMachineModelScript = loadedMachineModels[machineModelName];
                DynValue dynValue = TileController.LuaScript.LoadString(loadedMachineModelScript, codeFriendlyName: machineModelName);
                TileController.LuaScript.Call(dynValue);
                
                var symbolTable = GetTable($"{machineName}BaseGameSymbolTable");
                var symbols = GetTablePkArray(symbolTable);
                
                // if symbolName not in symbols skip
                if (!symbols.Contains(symbolName))
                {
                    continue;
                }
                
                // Load the control prefab asset
                GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                // get the "Quad"  game object - below Pivot
                var quadTransform = prefab.transform.Find("Pivot").Find("Quad");
                if (quadTransform == null)
                {
                    Debug.LogWarning($"Quad not found in prefab: {prefabFileName}");
                    continue;
                }
                GameObject quad = quadTransform.gameObject;
                // get the material for the quad
                Material material = quad.GetComponent<Renderer>().sharedMaterial;
                // fix the alpha of the material color to 1
                Color color = material.color;
                color.a = 0.80f;
                material.color = color;
                
                // bump the process count
                processCount++;
                
                Debug.Log($"processed prefab: {prefabFileName}");
                
                // save the changes and ensure refresh
                EditorUtility.SetDirty(material);
                AssetDatabase.SaveAssets();
            }
            
            AssetDatabase.Refresh();
            
            // log the process count
            Debug.Log($"Processed {processCount} prefabs.");
        }
        
        [MenuItem("Bettr/Tools/Sync Game Scripts")]
        public static void SyncGameScripts()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        var machineModelName =$"{machineName}Models";
                        
                        string modelDestinationPath = Path.Combine(runtimeAssetPath, "Models",  $"{machineModelName}.cscript.txt");
                        
                        Debug.Log($"Processing Loading Model file for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                        
                        // Load and run the Model file
                        var modelTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(modelDestinationPath);
                        var machineModelScript = modelTextAsset.text;
                        TileController.StaticInit();
                        DynValue dynValue = TileController.LuaScript.LoadString(machineModelScript, codeFriendlyName: machineModelName);
                        TileController.LuaScript.Call(dynValue);
                        
                        Debug.Log($"Processing Scripts for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        ProcessScripts(machineName, machineVariant, experimentVariant, runtimeAssetPath);
                        
                        processCount++;
                    }
                }
            }
            
            Debug.Log($"SyncGameScripts: Processed {processCount} machine variants.");
        }

        [MenuItem("Bettr/Tools/Archived/Fix Audio WebGL Settings")]
        public static void FixAudioWebGLSettings()
        {
            EnforceWebGLAudioOverride();
        }
        
        [MenuItem("Bettr/Tools/Archived/Sync Audio Files")]
        public static void SyncAudioFiles()
        {
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);

            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }

                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }

                var machineVariantsDirs = variantsDir.GetDirectories();

                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir.GetDirectories();

                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir.Name;
                        string experimentVariant = experimentVariantDir.Name;

                        string runtimeAssetPath =
                            $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // Delete audio files properly
                        DeleteAudio(machineName, machineVariant, experimentVariant, runtimeAssetPath);
                    }
                }
                
                // refresh database
                AssetDatabase.Refresh();

                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir.GetDirectories();

                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir.Name;
                        string experimentVariant = experimentVariantDir.Name;

                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }

                        // Copy and process audio files properly
                        ProcessAudio(machineName, machineVariant, experimentVariant, runtimeAssetPath);

                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();

            Debug.Log($"SyncAudioFiles: Processed {processCount} machine variants.");
        }
        
        [MenuItem("Bettr/Tools/Archived/Cleanup Runtime Audio Files")]
        public static void CleanupAudioFiles()
        {
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);

            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }

                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }

                var machineVariantsDirs = variantsDir.GetDirectories();

                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir.GetDirectories();

                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir.Name;
                        string experimentVariant = experimentVariantDir.Name;

                        string runtimeAssetPath =
                            $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // Delete audio files properly
                        DeleteAudio(machineName, machineVariant, experimentVariant, runtimeAssetPath);
                    }
                }
            }
            
            AssetDatabase.Refresh();

            Debug.Log($"CleanupAudioFiles: Processed {processCount} machine variants.");
        }

        private static void EnforceWebGLAudioOverride()
        {
            string audioSourcePath = "Assets/Bettr/Editor/audio";
            if (!Directory.Exists(audioSourcePath))
            {
                Debug.LogWarning($"No audio found at: {audioSourcePath}");
                return;
            }
            // get the existing files
            string[] audioSourceFiles = Directory.GetFiles(audioSourcePath);
            foreach (var audioSourceFile in audioSourceFiles)
            {
                // check if audioSourceFile is a .ogg file
                if (!audioSourceFile.EndsWith(AUDIO_FORMAT))
                {
                    continue;
                }
                
                AudioImporter audioImporter = (AudioImporter)AssetImporter.GetAtPath(audioSourceFile);
                if (audioImporter == null)
                {
                    Debug.LogError($"Failed to get AudioImporter for {audioSourceFile}");
                    continue;
                }
                AudioImporterSampleSettings webGLSettings = audioImporter.GetOverrideSampleSettings("WebGL");

                // Change the compression format to Vorbis
                webGLSettings.compressionFormat = AudioCompressionFormat.Vorbis;
                webGLSettings.loadType = AudioClipLoadType.CompressedInMemory;

                // Set the override for WebGL
                audioImporter.SetOverrideSampleSettings("WebGL", webGLSettings);
                
                // save changes
                audioImporter.SaveAndReimport();

                // Reimport the audio clip to apply changes
                AssetDatabase.ImportAsset(audioSourceFile);
                Debug.Log("Updated compression format for: " + audioSourceFile);
            }
        }


        private static void DeleteAudio(string machineName, string machineVariant, string experimentVariant,
            string runtimeAssetPath)
        {
            string audioSourcePath = "Assets/Bettr/Editor/audio";

            string destinationPath = Path.Combine(runtimeAssetPath, "Audio");

            if (!Directory.Exists(audioSourcePath))
            {
                Debug.LogWarning($"No audio found at: {audioSourcePath}");
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            // get the existing files
            string[] existingAudioFiles = Directory.GetFiles(destinationPath);
            foreach (var existingAudioFile in existingAudioFiles)
            {
                string fileName = Path.GetFileName(existingAudioFile);
                string assetDestinationPath = $"{destinationPath}/{fileName}";

                if (File.Exists(assetDestinationPath))
                {
                    File.Delete(assetDestinationPath);
                }
            }
            
            // if the directory is now empty delete it
            if (Directory.GetFiles(destinationPath).Length == 0)
            {
                Directory.Delete(destinationPath);
                // delete the meta file as well
                File.Delete($"{destinationPath}.meta");
            }
        }

        private static void ProcessAudio(string machineName, string machineVariant, string experimentVariant, string runtimeAssetPath)
        {
            string audioSourcePath = "Assets/Bettr/Editor/audio";
            
            string destinationPath = Path.Combine(runtimeAssetPath, "Audio");

            if (!Directory.Exists(audioSourcePath))
            {
                Debug.LogWarning($"No audio found at: {audioSourcePath}");
                return;
            }

            if (!Directory.Exists(destinationPath))
            {
                Directory.CreateDirectory(destinationPath);
            }

            string[] audioFilesWav = Directory.GetFiles(audioSourcePath, "*.wav");
            string[] audioFilesMp3 = Directory.GetFiles(audioSourcePath, "*.mp3");
            string[] audioFilesOgg = Directory.GetFiles(audioSourcePath, "*.ogg");
                        
            // combine into single audioFiles array
            string[] audioFiles = audioFilesWav.Concat(audioFilesMp3).Concat(audioFilesOgg).ToArray();            

            foreach (var audioFile in audioFiles)
            {
                string fileName = Path.GetFileName(audioFile);
                string assetSourcePath = $"{audioSourcePath}/{fileName}";
                string assetDestinationPath = $"{destinationPath}/{fileName}";

                // Use AssetDatabase to copy assets with settings intact
                if (!AssetDatabase.CopyAsset(assetSourcePath, assetDestinationPath))
                {
                    Debug.LogError($"Failed to copy asset from {assetSourcePath} to {assetDestinationPath}");
                    continue;
                }

                // Import the asset at the new location to retain settings
                AssetDatabase.ImportAsset(assetDestinationPath, ImportAssetOptions.ForceUpdate);

                Debug.Log($"Copied and imported audio asset: {assetDestinationPath}");
            }
        }
        
        private static GameObject FindGameObjectInPrefab(GameObject prefab, string gameObjectName)
        {
            Transform[] allTransforms = prefab.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in allTransforms)
            {
                if (transform.gameObject.name == gameObjectName)
                {
                    return transform.gameObject;
                }
            }

            return null;
        }
        
        private static void SetLayerRecursively(GameObject gameObject, int layer)
        {
            gameObject.layer = layer;
            foreach (Transform child in gameObject.transform)
            {
                SetLayerRecursively(child.gameObject, layer);
            }
        }
        
        [MenuItem("Bettr/Tools/Fix Status Texts")]
        public static void FixStatusTexts()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the prefabs directory
                        string prefabsDirectory = Path.Combine(runtimeAssetPath, "Prefabs");
                        // get the Game<NNN>BaseGameMachine.prefab
                        string machinePrefabPath = Directory.GetFiles(prefabsDirectory, $"{machineName}BaseGameMachine.prefab", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(machinePrefabPath))
                        {
                            Debug.LogError($"Machine prefab not found: {machineName}BaseGameMachine.prefab");
                            continue;
                        }
                        
                        // load the Prefab asset
                        GameObject machinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(machinePrefabPath);
                        if (machinePrefab == null)
                        {
                            Debug.LogError($"Failed to load prefab: {machineName}BaseGameMachine.prefab");
                            continue;
                        }
                        
                        // Get the WinSymbols GameObject
                        GameObject winSymbols = FindGameObjectInPrefab(machinePrefab, "WinSymbols");
                        winSymbols.transform.localPosition = new Vector3(-6.34f, -4.55f, -20);
                        winSymbols.transform.localScale = new Vector3(1, 1, 1);
                        // Switch to SLOT_OVERLAY Layer
                        SetLayerRecursively(winSymbols, LayerMask.NameToLayer("SLOT_OVERLAY"));
                        
                        // Get the GoodLuckTexts and PaysText Game Objects which are descendants of the machine prefab
                        GameObject goodLuckText = FindGameObjectInPrefab(machinePrefab, "GoodLuckText");
                        GameObject paysText = FindGameObjectInPrefab(machinePrefab, "PaysText");                                               
                        // Fix the scale of the paysText component to match the goodLuckText component
                        paysText.transform.localScale = goodLuckText.transform.localScale;
                        // Get the RectTransform height of the goodLuckText component
                        RectTransform goodLuckTextRectTransform = goodLuckText.GetComponent<RectTransform>();
                        if (machineName is "Game004" or "Game007")
                        {
                            var vector2 = goodLuckTextRectTransform.anchoredPosition;
                            vector2.y = -3.2f;
                            goodLuckTextRectTransform.anchoredPosition = vector2;
                            winSymbols.transform.localPosition = new Vector3(-6.34f, -3.9f, -20);
                        }
                        
                        // Get the RectTransform height of the paysText component
                        RectTransform paysTextRectTransform = paysText.GetComponent<RectTransform>();
                        // Set the height of the paysText component to match the goodLuckText component
                        paysTextRectTransform.sizeDelta = new Vector2(paysTextRectTransform.sizeDelta.x, goodLuckTextRectTransform.sizeDelta.y);
                        // Set the PosY of the paysText component to match the goodLuckText component
                        paysTextRectTransform.anchoredPosition = new Vector2(paysTextRectTransform.anchoredPosition.x, goodLuckTextRectTransform.anchoredPosition.y);
                        // Set the Transform X
                        paysText.transform.localPosition = new Vector3(-3.6f, paysText.transform.localPosition.y, paysText.transform.localPosition.z);
                        // change the paysText 
                        paysText.GetComponent<TextMeshPro>().text = "x {0} = {1,3} x {2} = {3,3}";
                        
                        // save the prefab
                        PrefabUtility.SaveAsPrefabAsset(machinePrefab, machinePrefabPath);
                        
                        Debug.Log($"Fixing Status Text for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Processed Fix Status Texts {processCount} machine variants.");
        }
        
        private static void ConfigureDefaultLightingSettings(LightingSettings lightingSettings)
        {
            // Enable Auto Generate
            lightingSettings.autoGenerate = true;

            // Realtime Lighting Settings
            lightingSettings.bakedGI = true;
            lightingSettings.realtimeGI = false;
            lightingSettings.mixedBakeMode = MixedLightingMode.Shadowmask;

            // Lightmapping Settings
            lightingSettings.lightmapper = LightingSettings.Lightmapper.ProgressiveGPU;
            lightingSettings.environmentImportanceSampling = true;
            lightingSettings.directSampleCount = 32;
            lightingSettings.indirectSampleCount = 512;
            lightingSettings.environmentSampleCount = 256;
            lightingSettings.lightProbeSampleCountMultiplier = 4;
            lightingSettings.maxBounces = 2;
            lightingSettings.filteringMode = LightingSettings.FilterMode.Auto;
            lightingSettings.lightmapResolution = 40f;
            lightingSettings.lightmapCompression = LightmapCompression.HighQuality;
            lightingSettings.lightmapPadding = 2;
            lightingSettings.albedoBoost = 1f;

            Debug.Log("Updated default lighting settings based on user configuration.");
        }
        
        [MenuItem("Bettr/Tools/Fix Base Game Directional Light")]
        public static void FixBaseGameDirectionalLight()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the Scenes directory
                        string scenesDirectory = Path.Combine(runtimeAssetPath, "Scenes");
                        // get the {machineName}{machineVariant}Scene.unity scene
                        string scenePath = Directory.GetFiles(scenesDirectory, $"{machineName}{machineVariant}Scene.unity", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            Debug.LogError($"Scene not found: {machineName}{machineVariant}Scene.unity");
                            continue;
                        }
                        
                        // add a Directional Light to the scene - inline the code below
                        // Load the scene
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        
                        // check if the scene has a Directional Light
                        var hasDirectionalLight = false;
                        GameObject[] gameObjects = scene.GetRootGameObjects();
                        foreach (GameObject gameObject in gameObjects)
                        {
                            if (gameObject.GetComponent<Light>() != null)
                            {
                                Debug.Log($"Directional Light already exists in scene: {scenePath}");
                                hasDirectionalLight = true;
                            }
                        }

                        GameObject directionalLight = null;
                        Light light = null;
                        
                        if (hasDirectionalLight)
                        {
                            directionalLight = GameObject.Find("Directional Light");
                            light = directionalLight.GetComponent<Light>();
                        }
                        else
                        {
                            directionalLight = new GameObject("Directional Light");
                            light = directionalLight.AddComponent<Light>();
                        }
                        
                        // Set the Light Type to Directional
                        light.type = LightType.Directional;
                        // Set the Light Color to white
                        light.color = Color.white;
                        // Set the Light Intensity to 1
                        light.intensity = 2;
                        // Set the Light Rotation to 45, 45, 0
                        directionalLight.transform.rotation = Quaternion.Euler(50, -30, 0);
                        // Set the Light Position to 0, 0, 0
                        directionalLight.transform.position = new Vector3(0, 0, 0);
                        // no shadows
                        light.shadows = LightShadows.None;
                        // Save the scene
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        
                        string lightingSettingsPath = $"{scenesDirectory}/{machineName}{machineVariant}LightingSettings.asset";
                        // check if the LightingSettings asset exists
                        if (File.Exists(lightingSettingsPath))
                        {
                            Debug.Log($"LightingSettings asset already exists: {lightingSettingsPath}");
                            continue;
                        }
                        
                        // load the scene
                        scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        // set the scene to the sceneAsset
                        LightingSettings lightingSettings = new LightingSettings();

                        // Save the new Lighting Settings asset to the project folder
                        AssetDatabase.CreateAsset(lightingSettings, lightingSettingsPath);
                        AssetDatabase.SaveAssets();
                        
                        // load the asset
                        lightingSettings = AssetDatabase.LoadAssetAtPath<LightingSettings>(lightingSettingsPath);
                        
                        Lightmapping.lightingSettings = lightingSettings;
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        
                        Debug.Log($"Fix Base Game Directional Light for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Processed Fix Background {processCount} machine variants.");
        }
        
        [MenuItem("Bettr/Tools/Fix Game Scene")]
        public static void FixGameScene()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // Load the {{machine}}{{machineVariant}}BaseGameBackground prefab under the runtime assets "prefabs" directory
                        string prefabsDirectory = Path.Combine(runtimeAssetPath, "Prefabs");
                        string backgroundPrefabPath = Directory.GetFiles(prefabsDirectory, $"{machineName}BaseGameBackground.prefab", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(backgroundPrefabPath))
                        {
                            Debug.LogError($"Background prefab not found: {machineName}BaseGameBackground.prefab");
                            continue;
                        }
                        // load the prefab
                        GameObject backgroundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(backgroundPrefabPath);
                        if (backgroundPrefab == null)
                        {
                            Debug.LogError($"Failed to load prefab: {machineName}BaseGameBackground.prefab");
                            continue;
                        }
                        
                        // load the GameScene of the form {{machineName}}{{machineVariant}}Scene.unity in Editor
                        string scenesDirectory = Path.Combine(runtimeAssetPath, "Scenes");
                        string scenePath = Directory.GetFiles(scenesDirectory, $"{machineName}{machineVariant}Scene.unity", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(scenePath))
                        {
                            Debug.LogError($"Scene not found: {machineName}{machineVariant}Scene.unity");
                            continue;
                        }
                        // load the scene in this editor script
                        Scene scene = EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
                        
                        // find the game object called "BaseGameBackgroundParent" in the open scene and attach this backgroundPrefab to it
                        GameObject baseGameBackgroundParent = GameObject.Find("BaseGameBackgroundParent");
                        if (baseGameBackgroundParent == null)
                        {
                            Debug.LogError($"BaseGameBackgroundParent not found in scene: {machineName}{machineVariant}Scene.unity");
                            continue;
                        }
                        // get the "Pivot" child of baseGameBackgroundParent
                        Transform pivot = baseGameBackgroundParent.transform.Find("Pivot");
                        if (pivot == null)
                        {
                            Debug.LogError($"Pivot not found in BaseGameBackgroundParent");
                            continue;
                        }
                        
                        // remove any existing child of "Pivot"
                        foreach (Transform child in pivot)
                        {
                            Object.DestroyImmediate(child.gameObject);
                        }
                        
                        // instantiate the backgroundPrefab
                        GameObject background = PrefabUtility.InstantiatePrefab(backgroundPrefab) as GameObject;
                        // null check background
                        if (background == null)
                        {
                            Debug.LogError($"Failed to instantiate background prefab: {machineName}{machineVariant}BaseGameBackgroundPrefab.prefab");
                            continue;
                        }
                        
                        // set the parent of the background to the baseGameBackgroundParent
                        background.transform.SetParent(pivot);
                        
                        // Save the changes to the scene
                        EditorSceneManager.MarkSceneDirty(scene);
                        EditorSceneManager.SaveScene(scene);
                        
                        Debug.Log($"Fixing Background for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Processed Fix Game Scene {processCount} machine variants.");
        }
        
        [MenuItem("Bettr/Tools/Fix Background Shader")]
        public static void FixBackgroundShader()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the prefabs directory
                        string prefabsDirectory = Path.Combine(runtimeAssetPath, "Prefabs");
                        string materialsDirectory = Path.Combine(runtimeAssetPath, "Materials");
                        string texturesDirectory = Path.Combine(runtimeAssetPath, "Textures");
                        
                        // The {machineName}BaseGameBackground.prefab uses the BackgroundFBX.prefab which uses the Material_with_Texture material in the Materials
                        // I want to switch it to use the Standard shader
                        
                        // get the Material_with_Texture material
                        string materialPath = Directory.GetFiles(materialsDirectory, "Material_with_Texture.mat", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        // load the Material_with_Texture material
                        Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
                        // get the Standard shader
                        Shader standardShader = Shader.Find("Standard");
                        // set the shader of the material to the Standard shader
                        material.shader = standardShader;
                        
                        Debug.Log($"Fixing Background for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Processed Fix Background {processCount} machine variants.");
        }
        
        [MenuItem("Bettr/Tools/Fix Audio Source")]
        public static void FixAudioSource()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the prefabs directory
                        string prefabsDirectory = Path.Combine(runtimeAssetPath, "Prefabs");
                        // get the Game<NNN>BaseGameMachine.prefab
                        string machinePrefabPath = Directory.GetFiles(prefabsDirectory, $"{machineName}BaseGameMachine.prefab", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(machinePrefabPath))
                        {
                            Debug.LogError($"Machine prefab not found: {machineName}BaseGameMachine.prefab");
                            continue;
                        }
                        
                        // load the Prefab asset
                        GameObject machinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(machinePrefabPath);
                        if (machinePrefab == null)
                        {
                            Debug.LogError($"Failed to load prefab: {machineName}BaseGameMachine.prefab");
                            continue;
                        }
                        
                        // remove any audio source from the top level game object of the machine prefab
                        AudioSource[] audioSources = machinePrefab.GetComponents<AudioSource>();
                        foreach (var audioSource in audioSources)
                        {
                            Object.DestroyImmediate(audioSource, true);
                        }
                        
                        string audioDirectory = Path.Combine(coreAssetPath, "Audio");
                        // get the audio files under the audio directory
                        string[] audioFiles = Directory.GetFiles(audioDirectory, $"*{AUDIO_FORMAT}", SearchOption.AllDirectories);
                        
                        // loop over the audio files
                        foreach (var audioFile in audioFiles)
                        {
                            // load the audio clip
                            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(audioFile);
                            if (audioClip == null)
                            {
                                Debug.LogError($"Failed to load audio clip: {audioFile}");
                                continue;
                            }
                            
                            // add an audio source to the top level game object of the machine prefab
                            AudioSource audioSource = machinePrefab.AddComponent<AudioSource>();
                            audioSource.clip = audioClip;
                            audioSource.playOnAwake = false;
                            audioSource.loop = true;
                            audioSource.pitch = 1;
                            audioSource.priority = 128;
                            audioSource.spatialBlend = 0; // 0 = 2D
                            audioSource.volume = 1; // can turn off using Key "V"
                            audioSource.panStereo = 0; // -1 = left, 1 = right
                        }
                        
                        // save the prefab
                        PrefabUtility.SaveAsPrefabAsset(machinePrefab, machinePrefabPath);
                        
                        Debug.Log($"Fixing Audio Source for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");
                
                        processCount++;
                    }
                }
            }
            
            AssetDatabase.Refresh();
            
            Debug.Log($"Processed Fix Audio Source {processCount} machine variants.");
        }
        
        [MenuItem("Bettr/Tools/Fix Settings Volume Button")]
        public static void FixSettingsVolumeButton()
        {
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            string coreAssetPath = $"Assets/Bettr/Core";

            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);

                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }

                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null) continue;

                var machineVariantsDirs = variantsDir?.GetDirectories();

                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null) continue;

                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;

                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }

                        // Get the prefab
                        string prefabsDirectory = Path.Combine(runtimeAssetPath, "Prefabs");
                        string machinePrefabPath = Directory.GetFiles(prefabsDirectory, $"{machineName}BaseGameMachine.prefab", SearchOption.TopDirectoryOnly).FirstOrDefault();
                        if (string.IsNullOrEmpty(machinePrefabPath))
                        {
                            Debug.LogError($"Machine prefab not found: {machineName}BaseGameMachine.prefab");
                            continue;
                        }

                        GameObject machinePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(machinePrefabPath);
                        if (machinePrefab == null)
                        {
                            Debug.LogError($"Failed to load prefab: {machineName}BaseGameMachine.prefab");
                            continue;
                        }

                        // Instantiate the prefab for editing
                        GameObject prefabInstance = PrefabUtility.InstantiatePrefab(machinePrefab) as GameObject;
                        if (prefabInstance == null)
                        {
                            Debug.LogError($"Failed to instantiate prefab: {machineName}BaseGameMachine.prefab");
                            continue;
                        }

                        GameObject nextGameObject = FindGameObjectInPrefab(prefabInstance, "Next");
                        GameObject pivotGameObject = nextGameObject.transform.parent.gameObject;
                        GameObject firstChildGameObject = pivotGameObject.transform.GetChild(0).gameObject;
                        GameObject volumeGameObject = null;

                        if (firstChildGameObject.name == "Volume")
                        {
                            volumeGameObject = firstChildGameObject;
                        }
                        else
                        {
                            RectTransform firstChildRectTransform = firstChildGameObject.GetComponent<RectTransform>();
                            if (firstChildRectTransform != null)
                            {
                                var anchoredPosition = firstChildRectTransform.anchoredPosition;
                                anchoredPosition.x -= 70;
                                volumeGameObject = Object.Instantiate(nextGameObject, pivotGameObject.transform);
                                volumeGameObject.name = "Volume";

                                RectTransform volumeRectTransform = volumeGameObject.GetComponent<RectTransform>();
                                if (volumeRectTransform != null)
                                {
                                    volumeRectTransform.anchoredPosition = anchoredPosition;
                                }

                                volumeGameObject.transform.SetAsFirstSibling();
                            }
                        }

                        // Get the TextMeshProUGUI component of the "Text" child of volumeGameObject
                        TextMeshProUGUI textMeshProUGUI = volumeGameObject.transform.Find("Text").GetComponent<TextMeshProUGUI>();
                        textMeshProUGUI.text = "Vol";

                        EventTrigger eventTrigger = volumeGameObject.GetComponent<EventTrigger>();

                        if (eventTrigger != null && eventTrigger.triggers.Count > 0)
                        {
                            // Get the first entry in the EventTrigger (assuming it's the one you want to modify)
                            EventTrigger.Entry entry = eventTrigger.triggers[0];

                            // Get the existing target (the object that contains the method)
                            var triggerEvent = entry.callback;
                            MonoBehaviour existingTarget = triggerEvent.GetPersistentTarget(0) as MonoBehaviour;
                            
                            MethodInfo methodInfo = existingTarget.GetType().GetMethod("OnPointerClick", new[] { typeof(string) });

                            // Get the method we want to call as a UnityAction<string>
                            UnityAction<string> methodCall = (UnityAction<string>)Delegate.CreateDelegate(typeof(UnityAction<string>), existingTarget, methodInfo);

                            if (methodCall != null)
                            {
                                // Remove the old listeners to add a new one
                                UnityEventTools.RemovePersistentListener(entry.callback, methodCall);

                                // Use UnityEventTools to add the new listener with the updated "Volume" parameter
                                UnityEventTools.AddStringPersistentListener(entry.callback, methodCall, "Volume");

                                Debug.Log("Updated EventTrigger parameter to 'Volume'.");
                            }
                            else
                            {
                                Debug.LogError("Could not find method 'OnPointerClick' on the target.");
                            }
                        }

                        // Apply changes to the prefab
                        PrefabUtility.ApplyPrefabInstance(prefabInstance, InteractionMode.UserAction);

                        // Clean up
                        Object.DestroyImmediate(prefabInstance);

                        Debug.Log($"Fixing Fix Settings Volume Button for machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant}");

                        processCount++;
                    }
                }
            }

            AssetDatabase.Refresh();

            Debug.Log($"Processed Fix Settings Volume Button {processCount} machine variants.");
        }

        
        [MenuItem("Bettr/Tools/Sync Symbol Textures")]
        public static void SyncSymbolTextures()
        {
            // Walk the entire directory tree under the plugin root directory
            var processCount = 0;
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        var machineModelName =$"{machineName}Models";
                        
                        string modelDestinationPath = Path.Combine(runtimeAssetPath, "Models",  $"{machineModelName}.cscript.txt");
                        
                        // Load and run the Model file
                        var modelTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(modelDestinationPath);
                        var machineModelScript = modelTextAsset.text;
                        TileController.StaticInit();
                        DynValue dynValue = TileController.LuaScript.LoadString(machineModelScript, codeFriendlyName: machineModelName);
                        TileController.LuaScript.Call(dynValue);
                        
                        var symbolTable = GetTable($"{machineName}BaseGameSymbolTable");
                        var pkArray = GetTablePkArray(symbolTable);
                        
                        string textureDestinationPath = Path.Combine(runtimeAssetPath, "Textures");
                        string textureSourcePath = $"Assets/Bettr/Editor/textures/{machineName}/{machineVariant}";
                        
                        // walk the pkArray
                        foreach (var pk in pkArray)
                        {
                            var jpgName = $"{pk}.jpg";
                            // copy from source path to destination path
                            var sourcePath = Path.Combine(textureSourcePath, jpgName);
                            var destinationPath = Path.Combine(textureDestinationPath, jpgName);
                            File.Copy(sourcePath, destinationPath, true);
                        }
                        
                        
                        processCount++;
                    }
                }
            }
            
            Debug.Log($"Processed {processCount} symbol textures.");
        }
        

        [MenuItem("Bettr/Build/BackgroundTextures")]
        public static void SyncBackgroundTextures()
        {
            // Walk the entire directory tree under the plugin root directory
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                
                        Environment.SetEnvironmentVariable("machineName", machineName);
                        Environment.SetEnvironmentVariable("machineVariant", machineVariant);
                        Environment.SetEnvironmentVariable("experimentVariant", experimentVariant);
                
                        SyncBackgroundTexturesFromCommandLine();
                    }
                }
            }
        }

        public static void SyncBackgroundTexturesFromCommandLine()
        {
            // assign texture "Background.png" to the FBX "BackgroundFBX.fbx"s material in the same directory
            
            string machineName = GetArgument("-machineName");
            string machineVariant = GetArgument("-machineVariant");
            string experimentVariant = GetArgument("-experimentVariant");

            string pathPrefix = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset/";
            string texturesPathPrefix = $"{pathPrefix}/Textures/";
            string fbxPathPrefix = $"{pathPrefix}/Prefabs/";
            string texturePath = $"{texturesPathPrefix}/Background.png";
            string fbxPath = $"{fbxPathPrefix}/BackgroundFBX.prefab";
            // ensure both exist
            if (!File.Exists(texturePath))
            {
                Debug.Log($"Texture not found at path: {texturePath} continuing...");
                return;
            }
            if (!File.Exists(fbxPath))
            {
                Debug.Log($"FBX not found at path: {fbxPath} continuing...");
                return;
            }
            Debug.Log($"Processing machineName={machineName} machineVariant={machineVariant} experimentVariant={experimentVariant} texture={texturePath} fbx={fbxPath}");
            // load the texture
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            // load the fbx
            GameObject fbx = AssetDatabase.LoadAssetAtPath<GameObject>(fbxPath);
            // get the material
            Material material = fbx.GetComponentInChildren<MeshRenderer>().sharedMaterial;
            // assign the texture to the material
            material.mainTexture = texture;

        }

        // [MenuItem("Bettr/Build/Game001 - Epic Wins")]
        public static void BuildGame001()
        {
            BuildMachines("Game001", "EpicAncientAdventures");
            // BuildMachines("Game001", "EpicAtlantisTreasures");
            // BuildMachines("Game001", "EpicClockworkChronicles");
            // BuildMachines("Game001", "EpicCosmicVoyage");
            // BuildMachines("Game001", "EpicDragonsHoard");
            // BuildMachines("Game001", "EpicEnchantedForest");
            // BuildMachines("Game001", "EpicGalacticQuest");
            // BuildMachines("Game001", "EpicGuardiansOfOlympus");
            // BuildMachines("Game001", "EpicLostCityOfGold");
            // BuildMachines("Game001", "EpicMysticalLegends");
            // BuildMachines("Game001", "EpicPharosFortune");
            // BuildMachines("Game001", "EpicPiratesPlunder");
            // BuildMachines("Game001", "EpicSamuraisFortune");
        }
        
        // [MenuItem("Bettr/Build/Game002 - Buffalo")]
        public static void BuildGame002()
        {
            BuildMachines("Game002", "BuffaloTreasureHunter");
        }

        // [MenuItem("Bettr/Build/Game003 - HighStakes")]
        public static void BuildGame003()
        {
            BuildMachines("Game003", "HighStakesAlpineAdventure");
        }

        // [MenuItem("Bettr/Build/Game004 - CleopatraRiches")]
        public static void BuildGame004()
        {
            BuildMachines("Game004", "CleopatraRiches");
        }

        // [MenuItem("Bettr/Build/Game005 - 88FortunesDancingDrums")]
        public static void BuildGame005()
        {
            BuildMachines("Game005", "88FortunesDancingDrums");
        }

        // [MenuItem("Bettr/Build/Game006 - WheelOfFortuneTripleExtremeSpin")]
        public static void BuildGame006()
        {
            BuildMachines("Game006", "WheelOfFortuneTripleExtremeSpin");
        }

        // [MenuItem("Bettr/Build/Game007 - TrueVegas")]
        public static void BuildGame007()
        {
            BuildMachines("Game007", "TrueVegasInfiniteSpins");
        }

        // [MenuItem("Bettr/Build/Game008 - GodsOfOlympusZeus")]
        public static void BuildGame008()
        {
            BuildMachines("Game008", "GodsOfOlympusZeus");
        }

        // [MenuItem("Bettr/Build/Game009 - PlanetMooneyMooCash")]
        public static void BuildGame009()
        {
            BuildMachines("Game009", "PlanetMooneyMooCash");
        }
        
        private static void ImportFBX(string machineName, string machineVariant, string experimentVariant)
        {
            // // Copy across the background texture
            // string sourceTexturePath =  $"Assets/Bettr/Editor/textures/{machineName}/{machineVariant}/Background.jpg";
            // string destinationTexturePath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset/Textures/Background.jpg";
            // File.Copy(sourceTexturePath, destinationTexturePath, overwrite: true);
            // AssetDatabase.Refresh();

            
            string sourcePath =  $"Assets/Bettr/Editor/fbx/{machineName}/{machineVariant}/";
            string destinationPathPrefix = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset/";
            string fbxFilename = $"Background_fbx_common_textured.fbx";
            string targetFbxFilename = $"BackgroundFBX.fbx";

            BettrFBXController.ImportFBX(sourcePath, destinationPathPrefix, fbxFilename, targetFbxFilename);
        }

        private static void BuildMachines(string machineName, string machineVariant)
        {
            var currentDir = Environment.CurrentDirectory;
            var modelsDir = $"{currentDir}/../../../bettr-infrastructure/bettr-infrastructure/tools/publish-data/published_models";
            
            Environment.SetEnvironmentVariable("machineName", machineName);
            Environment.SetEnvironmentVariable("machineVariant", machineVariant);
            Environment.SetEnvironmentVariable("machineModel", $"{modelsDir}/{machineName}/{machineVariant}/{machineName}Models.lua");

            BuildMachinesFromCommandLine();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void BuildMachinesFromCommandLine()
        {
            SetupCoreAssetPath();
            string machineName = GetArgument("-machineName");
            string machineVariant = GetArgument("-machineVariant");
            string machineModel = GetArgument("-machineModel");
            
            string experimentVariant = "control";
            
            ClearRuntimeAssetPath(machineName, machineVariant, experimentVariant);
            SetupMachine(machineName, machineVariant, experimentVariant, machineModel);
            ImportFBX(machineName, machineVariant, experimentVariant);
            BuildMachine(machineName, machineVariant, experimentVariant);
            
            experimentVariant = "variant1";
            
            ClearRuntimeAssetPath(machineName, machineVariant, experimentVariant);
            SetupMachine(machineName, machineVariant, experimentVariant, machineModel);
            ImportFBX(machineName, machineVariant, experimentVariant);
            BuildMachine(machineName, machineVariant, experimentVariant);
        }
        
        private static void CreateOrReplaceMaterial(string machineName, string machineVariant, string experimentVariant)
        {
            string materialName = $"{machineName}__{machineVariant}__LobbyCard";
            string materialPath = $"Assets/Bettr/Runtime/Plugin/LobbyCard/variants/v0_1_0/Runtime/Asset/{machineName}/{experimentVariant}/LobbyCard/Materials/{materialName}.mat";
            string texturePath = $"Assets/Bettr/Runtime/Plugin/LobbyCard/variants/v0_1_0/Runtime/Asset/{machineName}/{experimentVariant}/LobbyCard/Materials/{materialName}.jpg";
    
            // Load or create the material
            Material material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (material == null)
            {
                material = new Material(Shader.Find("Unlit/Texture"));
                AssetDatabase.CreateAsset(material, materialPath);
                AssetDatabase.Refresh();
            }
            
            // Set the render queue to 3000
            material.renderQueue = 3000;

            // Load the texture
            Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
            if (texture != null)
            {
                material.mainTexture = texture;
            }
            else
            {
                Debug.LogWarning($"Texture not found at path: {texturePath}");
            }

            // Create or replace the material asset
            AssetDatabase.SaveAssets();
        }
        
        [MenuItem("Bettr/Build/LobbyCard/Materials")]
        public static void BuildMaterials()
        {
            // Game001 Variants
            CreateOrReplaceMaterial("Game001", "AncientAdventures", "control");
            CreateOrReplaceMaterial("Game001", "AtlantisTreasures", "control");
            CreateOrReplaceMaterial("Game001", "ClockworkChronicles", "control");
            CreateOrReplaceMaterial("Game001", "DragonsHoard", "control");
            CreateOrReplaceMaterial("Game001", "EnchantedForest", "control");
            CreateOrReplaceMaterial("Game001", "GalacticQuest", "control");
            CreateOrReplaceMaterial("Game001", "GuardiansOfOlympus", "control");
            CreateOrReplaceMaterial("Game001", "LostCityOfGold", "control");
            CreateOrReplaceMaterial("Game001", "MysticalLegends", "control");
            CreateOrReplaceMaterial("Game001", "PharosFortune", "control");
            CreateOrReplaceMaterial("Game001", "PiratesPlunder", "control");
            CreateOrReplaceMaterial("Game001", "SamuraisFortune", "control");

            // Game002 Variants
            CreateOrReplaceMaterial("Game002", "BuffaloAdventureQuest", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloCanyonRiches", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloFrontierFortune", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloJackpotMadness", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloMagicSpins", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloMoonlitMagic", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloSafariExpedition", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloSpiritQuest", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloThunderstorm", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloTreasureHunter", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloWheelOfRiches", "control");
            CreateOrReplaceMaterial("Game002", "BuffaloWildPicks", "control");

            // Game003 Variants
            CreateOrReplaceMaterial("Game003", "HighStakesAlpineAdventure", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesCascadingCash", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesHotLinks", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesJungleQuest", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesMegaMultipliers", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesMonacoThrills", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesSafariAdventure", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesSpaceOdyssey", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesStackedSpins", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesUnderwaterAdventure", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesWildSpins", "control");
            CreateOrReplaceMaterial("Game003", "HighStakesWonderWays", "control");

            // Game004 Variants
            CreateOrReplaceMaterial("Game004", "RichesBeverlyHillMansions", "control");
            CreateOrReplaceMaterial("Game004", "RichesBillionaireBets", "control");
            CreateOrReplaceMaterial("Game004", "RichesDiamondDash", "control");
            CreateOrReplaceMaterial("Game004", "RichesGalacticGoldRush", "control");
            CreateOrReplaceMaterial("Game004", "RichesJetsetJackpot", "control");
            CreateOrReplaceMaterial("Game004", "RichesMysticForest", "control");
            CreateOrReplaceMaterial("Game004", "RichesPharaohsRiches", "control");
            CreateOrReplaceMaterial("Game004", "RichesPiratesBounty", "control");
            CreateOrReplaceMaterial("Game004", "RichesRaceToRiches", "control");
            CreateOrReplaceMaterial("Game004", "RichesRoyalHeist", "control");
            CreateOrReplaceMaterial("Game004", "RichesRubyRush", "control");
            CreateOrReplaceMaterial("Game004", "RichesSapphireSprint", "control");

            // Game005 Variants
            CreateOrReplaceMaterial("Game005", "FortunesCelestialFortune", "control");
            CreateOrReplaceMaterial("Game005", "FortunesFortuneTeller", "control");
            CreateOrReplaceMaterial("Game005", "FortunesFourLeafClover", "control");
            CreateOrReplaceMaterial("Game005", "FortunesJadeOfFortune", "control");
            CreateOrReplaceMaterial("Game005", "FortunesLuckyBamboo", "control");
            CreateOrReplaceMaterial("Game005", "FortunesLuckyCharms", "control");
            CreateOrReplaceMaterial("Game005", "FortunesManekiNeko", "control");
            CreateOrReplaceMaterial("Game005", "FortunesMysticForest", "control");
            CreateOrReplaceMaterial("Game005", "FortunesNorseAcorns", "control");
            CreateOrReplaceMaterial("Game005", "FortunesPharaohsRiches", "control");
            CreateOrReplaceMaterial("Game005", "FortunesShootingStars", "control");
            CreateOrReplaceMaterial("Game005", "FortunesVikingVoyage", "control");

            // Game006 Variants
            CreateOrReplaceMaterial("Game006", "WheelsAncientKingdom", "control");
            CreateOrReplaceMaterial("Game006", "WheelsCapitalCityTycoon", "control");
            CreateOrReplaceMaterial("Game006", "WheelsEmpireBuilder", "control");
            CreateOrReplaceMaterial("Game006", "WheelsFantasyKingdom", "control");
            CreateOrReplaceMaterial("Game006", "WheelsGlobalInvestor", "control");
            CreateOrReplaceMaterial("Game006", "WheelsIndustrialRevolution", "control");
            CreateOrReplaceMaterial("Game006", "WheelsJurassicJungle", "control");
            CreateOrReplaceMaterial("Game006", "WheelsMythicalRealm", "control");
            CreateOrReplaceMaterial("Game006", "WheelsRealEstateMoghul", "control");
            CreateOrReplaceMaterial("Game006", "WheelsSpaceColonization", "control");
            CreateOrReplaceMaterial("Game006", "WheelsTreasuresIslandTycoon", "control");
            CreateOrReplaceMaterial("Game006", "WheelsUnderwaterEmpire", "control");

            // Game007 Variants
            CreateOrReplaceMaterial("Game007", "TrueVegasDiamondDazzle", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasGoldRush", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasInfiniteSpins", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasLucky7s", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasLuckyCharms", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasMegaJackpot", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasMegaWheels", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasRubyRiches", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasSuper7s", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasTripleSpins", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasWheelBonanza", "control");
            CreateOrReplaceMaterial("Game007", "TrueVegasWildCherries", "control");

            // Game008 Variants
            CreateOrReplaceMaterial("Game008", "GodsAncientEgyptian", "control");
            CreateOrReplaceMaterial("Game008", "GodsCelestialBeasts", "control");
            CreateOrReplaceMaterial("Game008", "GodsCelestialGuardians", "control");
            CreateOrReplaceMaterial("Game008", "GodsDivineRiches", "control");
            CreateOrReplaceMaterial("Game008", "GodsElementalMasters", "control");
            CreateOrReplaceMaterial("Game008", "GodsEternalDivinity", "control");
            CreateOrReplaceMaterial("Game008", "GodsHeavenlyMonarchs", "control");
            CreateOrReplaceMaterial("Game008", "GodsMysticPantheon", "control");
            CreateOrReplaceMaterial("Game008", "GodsMythicDeities", "control");
            CreateOrReplaceMaterial("Game008", "GodsNorseLegends", "control");
            CreateOrReplaceMaterial("Game008", "GodsSacredLegends", "control");
            CreateOrReplaceMaterial("Game008", "GodsTitansOfWealth", "control");

            // Game009 Variants
            CreateOrReplaceMaterial("Game009", "SpaceInvadersApolloAdventures", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersAsteroidMiners", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersBlackHoleExplorers", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersCosmicRaiders", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersGalacticPioneers", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersInterstellarTreasureHunters", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersNebulaNavigators", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersQuantumExplorers", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersRaidersOfPlanetMooney", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersStarshipSalvagers", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersStellarExpedition", "control");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersVoyagersOfTheCosmos", "control");
            
            Debug.Log($"LobbyCard materials updated successfully.");
        }

        
        //[MenuItem("Bettr/Assets/Cleanup")]
        private static void CleanupTestScenes()
        {
            RemoveTestScenes(new DirectoryInfo("Assets/"));
        }

        private static void BuildAssetBundles()
        {
            Debug.Log("Building asset bundles...");
            
            EnsurePluginAssetsHaveLabels(PluginRootDirectory);
            
            Debug.Log("...refreshing database before building asset bundles..");
            AssetDatabase.Refresh();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.ForceRebuildAssetBundle |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesIOSDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesIOSDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            AssetDatabase.Refresh();
            
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            AssetDatabase.Refresh();
            
#endif
#if UNITY_ANDROID
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesAndroidDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesAndroidDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWebglDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWebglDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesWindowsDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWindowsDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EnsureEmptyDirectory(new DirectoryInfo(AssetBundlesLinuxDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesLinuxDirectory, 
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

            AssetLabelsCache = new HashSet<string>();
            AssetVariantsCache = new HashSet<string>();
            var pluginMachineGroupDirectories = Directory.GetDirectories(pluginRootDirectory);
            foreach (var pluginMachineGroupDirectory in pluginMachineGroupDirectories)
            {
                var assetLabel = ReadAssetLabel(pluginMachineGroupDirectory);
                if (assetLabel.Contains('.')) throw new Exception("Asset sub label cannot contain a period.");
                if (!string.IsNullOrEmpty(assetLabel))
                {
                    var rootDirectory = Path.Combine(pluginMachineGroupDirectory, "variants");
                    var pluginMachineDirectories = Directory.GetDirectories(rootDirectory);
                    foreach (var pluginMachineDirectory in pluginMachineDirectories)
                    {
                        // all the Experiment variants are under this directory
                        var assetSubLabel = ReadAssetSubLabel(pluginMachineDirectory);
                        if (assetSubLabel.Contains('.')) throw new Exception("Asset sub label cannot contain a period.");
                        var experimentVariantsDirectory = Directory.GetDirectories(pluginMachineDirectory);
                        foreach (var experimentVariantDirectory in experimentVariantsDirectory)
                        {
                            var assetVariantLabel = ReadAssetVariantLabel(experimentVariantDirectory);
                            if (assetVariantLabel.Contains('.')) throw new Exception("Asset variant label cannot contain a period.");
                            var pluginRuntimeDirectory = Path.Combine(experimentVariantDirectory, "Runtime");
                            WalkDirectoryRecursive(pluginRuntimeDirectory, assetLabel, assetSubLabel, assetVariantLabel);
                        }
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
        
        private static string ReadAssetVariantLabel(string directoryPath)
        {
            var di = new DirectoryInfo(directoryPath);
            var baseName = di.Name.ToLower();
            return baseName;
        }
        
        private static void WalkDirectoryRecursive(string directoryPath, string assetLabel, string assetSubLabel, string assetVariantLabel)
        {
            if (Path.GetFileNameWithoutExtension(directoryPath).Equals("Editor")) return; // skip special Editor folder
            var importer = AssetImporter.GetAtPath(directoryPath);
            var assetType = AssetDatabase.GetMainAssetTypeAtPath(directoryPath);
            if (importer != null)
            {
                if (assetType != null && assetType != typeof(MonoScript))
                {
                    importer.assetBundleName = "";
                    importer.assetBundleVariant = "";
                    Debug.Log($"clearing importer assetBundleName and assetBundleVariant for assetPath={directoryPath}");
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
                        importer.assetBundleName = GetAssetBundleName(assetLabel, assetSubLabel, assetType);
                        importer.assetBundleVariant = GetAssetBundleVariant(assetVariantLabel);
                        // add these to the cache
                        AssetLabelsCache.Add(importer.assetBundleName);
                        AssetVariantsCache.Add(importer.assetBundleVariant);
                        Debug.Log($"setting asset labels for assetBundleName={importer.assetBundleName} assetBundleVariant={importer.assetBundleVariant} assetPath={assetPath}");
                    }
                }
            }

            var subDirectories = Directory.GetDirectories(directoryPath);
            foreach (var subDirectory in subDirectories)
            {
                WalkDirectoryRecursive(subDirectory, assetLabel, assetSubLabel, assetVariantLabel);
            }
        }

        private static void EnsureEmptyDirectory(this DirectoryInfo directory)
        {
            if (!directory.Exists) directory.Create();
            foreach(DirectoryInfo subDirectory in directory.GetDirectories()) subDirectory.Delete(true);
        }
        
        private static void BuildDirectory(this DirectoryInfo directory, bool force = false)
        {
            if (!directory.Exists || force)
            {
                directory.Create();
            }
        }

        private static string GetAssetBundleName(string assetLabel, string assetSubLabel, Type assetType)
        {
            var isAudio = assetType.Name == "AudioClip";
            // audio clips will be loaded outside the AssetBundle
            if (isAudio)  { return ""; }
            var isScene = assetType.Name == "SceneAsset";
            var suffix = isScene ? "_scenes" : "";
            var assetBundleName = $"{assetLabel}{assetSubLabel}{suffix}";
            return assetBundleName;
        }
        
        private static string GetAssetBundleVariant(string label)
        {
            return label;
        }
        
        private static void ModifyAssetBundleManifestFiles()
        {
            var directories = Directory.GetDirectories(AssetBundlesDirectory);
            // get the current build target
            var buildTarget = EditorUserBuildSettings.activeBuildTarget;
            foreach (var directory in directories)
            {
                // get the base name of the directory
                var subDirectory = Path.GetFileName(directory);
                if (subDirectory == "iOS" && buildTarget != BuildTarget.iOS) continue;
                if (subDirectory == "OSX" && buildTarget != BuildTarget.StandaloneOSX) continue;
                if (subDirectory == "Android" && buildTarget != BuildTarget.Android) continue;
                if (subDirectory == "WebGL" && buildTarget != BuildTarget.WebGL) continue;
                if (subDirectory == "Windows" && buildTarget != BuildTarget.StandaloneWindows64) continue;
                if (subDirectory == "Linux" && buildTarget != BuildTarget.StandaloneLinux64) continue;
                
                var files = Directory.GetFiles(directory);
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
                                    // Sort assets in the desired order: Shaders, Scripts, Materials, and then others
                                    var sortedAssetList = assetList
                                        .OrderBy(asset =>
                                        {
                                            string assetStr = asset.ToString();

                                            // Sort shaders first
                                            if (assetStr.EndsWith(".shader", StringComparison.OrdinalIgnoreCase))
                                                return 0;
                                            
                                            // Sort wav files second
                                            if (assetStr.EndsWith(".wav", StringComparison.OrdinalIgnoreCase)
                                                || assetStr.EndsWith(".mp3", StringComparison.OrdinalIgnoreCase)
                                                    || assetStr.EndsWith(".ogg", StringComparison.OrdinalIgnoreCase))
                                                return 1;

                                            // Sort scripts second
                                            if (assetStr.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || assetStr.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                                                return 2;

                                            // Sort materials third
                                            if (assetStr.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                                                return 3;

                                            // Everything else last
                                            return 4;
                                        })
                                        .ThenBy(asset => asset.ToString()) // Secondary alphabetical sort
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
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error processing manifest file '{file}': {ex.Message}");
                        }
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
        
        [MenuItem("Bettr/Build/Local Server")] 
        public static void BuildLocalServer()
        {
            Debug.Log("Refreshing database before building local server.");
            
            BuildDirectory(new DirectoryInfo(LocalServerDirectory));
            AssetDatabase.Refresh();

            var bettrUserConfigJsonData = LoadUserJsonFromWebAssets();

            var usersDirectory = $"{LocalServerDirectory}/users";
            BuildDirectory(new DirectoryInfo(usersDirectory));
            AssetDatabase.Refresh();

            var destinationFilePath = $"{usersDirectory}/default.json";

            if (bettrUserConfigJsonData != null)
            {
                File.WriteAllText(destinationFilePath, bettrUserConfigJsonData);
                Debug.Log("Copied latest user data from s3.");
            }
            else
            {
                Debug.LogError("Failed to load bettrUserConfig.");
            }

            Debug.Log("Refreshing database after building local server.");
            AssetDatabase.Refresh();
        }
        
        [MenuItem("Bettr/Build/Local Outcomes")] 
        public static void BuildLocalOutcomes()
        {
            Debug.Log("Refreshing database before building local outcomes.");
            
            BuildDirectory(new DirectoryInfo(LocalOutcomesDirectory));

            var allGameOutcomes = LoadOutcomesFromWeb();
            foreach (var gameOutcomes in allGameOutcomes)
            {
                var localGameOutcomesDirectory = $"{LocalOutcomesDirectory}/{gameOutcomes.Key}";
                BuildDirectory(new DirectoryInfo(localGameOutcomesDirectory));

                foreach (var gameOutcome in gameOutcomes.Value)
                {
                    var localGameOutcomeDirectory = $"{localGameOutcomesDirectory}/{gameOutcome.Key}";
                    File.WriteAllText(localGameOutcomeDirectory, gameOutcome.Value);

                }
            }
            
            var allGameVariantOutcomes = LoadGameVariantOutcomesFromWeb();
            foreach (var gameOutcomes in allGameVariantOutcomes)
            {
                foreach (var gameVariantOutcomes in gameOutcomes.Value)
                {
                    var localGameVariantOutcomesDirectory = $"{LocalOutcomesDirectory}/{gameOutcomes.Key}/{gameVariantOutcomes.Key}";
                    BuildDirectory(new DirectoryInfo(localGameVariantOutcomesDirectory));
                    foreach (var gameVariantOutcome in gameVariantOutcomes.Value)
                    {
                        var localGameVariantOutcomeFile = $"{localGameVariantOutcomesDirectory}/{gameVariantOutcome.Key}";
                        File.WriteAllText(localGameVariantOutcomeFile, gameVariantOutcome.Value);
                    }
                }
            }

            Debug.Log("Refreshing database after building local outcomes.");
            
            AssetDatabase.Refresh();
        }

        public static void BuildVideo()
        {
            BuildLocalVideo();
        }

        public static void BuildLocalVideo(List<string> buildAssetLabel = null)
        {
            Debug.Log("Refreshing database before building video.");
            
            BuildDirectory(new DirectoryInfo(LocalAudioDirectory));

            // walk the directory
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the audio directory
                        string audioDirectory = Path.Combine(runtimeAssetPath, "Audio");
                        // check if the audioDirectory exists and if not continue
                        if (!Directory.Exists(audioDirectory))
                        {
                            Debug.Log($"Directory not found: {audioDirectory}");
                            continue;
                        }
                        
                        // get the audio files under this directory.
                        var audioFiles = Directory.GetFiles(audioDirectory);
                        // filter out files that are not mp3 or wav or ogg
                        var audioFilesToCopy = audioFiles.Where(file => file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".ogg")).ToList();
                        // copy the audio files to the local audio directory
                        foreach (var audioFile in audioFilesToCopy)
                        {
                            string audioFileName = Path.GetFileName(audioFile);
                            bool includeFile = buildAssetLabel == null || buildAssetLabel.Count == 0 ||
                                               buildAssetLabel.Exists(s =>
                                                   audioFile.StartsWith(s, StringComparison.OrdinalIgnoreCase));
                            if (includeFile)
                            {
                                string destinationPath = Path.Combine(LocalAudioDirectory, audioFileName);
                                File.Copy(audioFile, destinationPath, true);                                
                            }
                        }
                    }
                }
            }
            
            Debug.Log("Refreshing database after building local audio.");                
            AssetDatabase.Refresh();
        }

        public static void BuildAudio()
        {
            BuildLocalAudio();
        }
        
        public static void BuildLocalAudio(List<string> buildAssetLabel = null)
        {
            Debug.Log("Refreshing database before building audio.");
            
            BuildDirectory(new DirectoryInfo(LocalAudioDirectory));

            // walk the directory
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the audio directory
                        string audioDirectory = Path.Combine(runtimeAssetPath, "Audio");
                        // check if the audioDirectory exists and if not continue
                        if (!Directory.Exists(audioDirectory))
                        {
                            Debug.Log($"Directory not found: {audioDirectory}");
                            continue;
                        }
                        
                        // get the audio files under this directory.
                        var audioFiles = Directory.GetFiles(audioDirectory);
                        // filter out files that are not mp3 or wav or ogg
                        var audioFilesToCopy = audioFiles.Where(file => file.EndsWith(".mp3") || file.EndsWith(".wav") || file.EndsWith(".ogg")).ToList();
                        // copy the audio files to the local audio directory
                        foreach (var audioFile in audioFilesToCopy)
                        {
                            string audioFileName = Path.GetFileName(audioFile);
                            bool includeFile = buildAssetLabel == null || buildAssetLabel.Count == 0 ||
                                               buildAssetLabel.Exists(s =>
                                                   audioFile.StartsWith(s, StringComparison.OrdinalIgnoreCase));
                            if (includeFile)
                            {
                                string destinationPath = Path.Combine(LocalAudioDirectory, audioFileName);
                                File.Copy(audioFile, destinationPath, true);                                
                            }
                        }
                    }
                }
            }
            
            Debug.Log("Refreshing database after building local audio.");                
            AssetDatabase.Refresh();
        }

        [MenuItem("Bettr/Build/Video")] 
        public static void BuildLocalVideo()
        {
            Debug.Log("Refreshing database before building video.");
            
            BuildDirectory(new DirectoryInfo(LocalVideoDirectory));

            // walk the directory
            var pluginMachineGroupDirectories = Directory.GetDirectories(PluginRootDirectory);
            for (var i = 0; i < pluginMachineGroupDirectories.Length; i++)
            {
                var machineNameDir = new DirectoryInfo(pluginMachineGroupDirectories[i]);
                // Check that the MachineName starts with "Game" and is not "Game001Alpha"
                if (!machineNameDir.Name.StartsWith("Game") || machineNameDir.Name == "Game001Alpha")
                {
                    continue;
                }
                var variantsDir = machineNameDir.GetDirectories().FirstOrDefault(d => d.Name == "variants");
                if (variantsDir == null)
                {
                    continue;
                }
                var machineVariantsDirs = variantsDir?.GetDirectories();
                // loop over machineVariantsDir
                foreach (var machineVariantsDir in machineVariantsDirs)
                {
                    var experimentVariantDirs = machineVariantsDir?.GetDirectories();
                    if (experimentVariantDirs == null)
                    {
                        continue;
                    }
                    // loop over experimentVariantDirs
                    foreach (var experimentVariantDir in experimentVariantDirs)
                    {
                        // now extract the machineName from machineNameDir, machineVariant from machineVariantsDir, and experimentVariant from experimentVariantDir
                        string machineName = machineNameDir.Name;
                        string machineVariant = machineVariantsDir?.Name;
                        string experimentVariant = experimentVariantDir?.Name;
                        
                        string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
                        if (!Directory.Exists(runtimeAssetPath))
                        {
                            Debug.LogError($"Directory not found: {runtimeAssetPath}");
                            continue;
                        }
                        
                        // get the video directory
                        string videoDirectory = Path.Combine(runtimeAssetPath, "Video");
                        // check if the videoDirectory exists and if not continue
                        if (!Directory.Exists(videoDirectory))
                        {
                            Debug.Log($"Directory not found: {videoDirectory}");
                            continue;
                        }
                        
                        // get the video files under this directory.
                        var videoFiles = Directory.GetFiles(videoDirectory);
                        // filter out files that are not mp4
                        var videoFilesToCopy = videoFiles.Where(file => file.EndsWith(".mp4")).ToList();
                        // copy the video files to the local video directory
                        foreach (var videoFile in videoFilesToCopy)
                        {
                            string videoFileName = Path.GetFileName(videoFile);
                            string destinationPath = Path.Combine(LocalVideoDirectory, videoFileName);
                            File.Copy(videoFile, destinationPath, true);
                        }
                    }
                }
            }
            
            Debug.Log("Refreshing database after building local video.");                
            AssetDatabase.Refresh();
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

            if (name.StartsWith("-"))
            {
                name = name.Substring(1);
            }
            // fallback to environment variables
            return Environment.GetEnvironmentVariable(name);
        }

        private static string GenerateDirectoryTreeJson(string path)
        {
            DirectoryInfo dir = new DirectoryInfo(path);
            var tree = GetDirectoryTree(dir);
            return JsonConvert.SerializeObject(tree, Formatting.Indented);
        }
        
        private static readonly string[] ExcludedFileNames = { ".DS_Store", ".meta" };
        
        private static bool IsExcluded(string fileName)
        {
            return ExcludedFileNames.Any(excluded => fileName.EndsWith(excluded, StringComparison.OrdinalIgnoreCase));
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
                if (path != null)
                {
                    Directory.CreateDirectory(path);

                    // Refresh the AssetDatabase to show the new directory in Unity Editor
                    AssetDatabase.Refresh();

                    Debug.Log($"Directory created at: {path}");
                }
            }
            else
            {
                Debug.Log($"Directory already exists at: {path}");
            }
        }
        
        private static string LoadUserJsonFromWebAssets()
        {
            string webAssetName = "users/default/user.json";
            string assetBundleURL = $"{AssetsServerBaseURL}/{webAssetName}";

            using (var webClient = new WebClient())
            {
                try
                {
                    byte[] jsonBytes = webClient.DownloadData(assetBundleURL);
                    string jsonString = Encoding.UTF8.GetString(jsonBytes);
                    return jsonString;
                }
                catch (Exception ex)
                {
                    var error = $"Error loading user JSON from server: {ex.Message}";
                    Debug.LogError(error);
                    return null;
                }
            }
        }

        private static Dictionary<string, Dictionary<string, string>> LoadOutcomesFromWeb()
        {
            var allOutcomes = new Dictionary<string, Dictionary<string, string>>();
            var gameConfigs = DownloadGameConfigs();
            foreach (var gameConfig in gameConfigs.GameConfigs)
            {
                var gameId = gameConfig.Key;
                var gameDetails = gameConfig.Value;
                var outcomes = DownloadOutcomes(gameId, gameDetails.OutcomeCount);
                allOutcomes[gameId] = outcomes;
            }
            return allOutcomes;
        }
        
        private static Dictionary<string, Dictionary<string, Dictionary<string, string>>> LoadGameVariantOutcomesFromWeb()
        {
            var allOutcomes = new Dictionary<string, Dictionary<string, Dictionary<string, string>>>();
            var gameConfigs = DownloadGameConfigs();
            foreach (var gameConfig in gameConfigs.GameConfigs)
            {
                var gameId = gameConfig.Key;
                var gameDetails = gameConfig.Value;
                if (gameDetails.GameVariantConfigs != null)
                {
                    allOutcomes[gameId] = new Dictionary<string, Dictionary<string, string>>();
                    foreach (var gameVariantConfig in gameDetails.GameVariantConfigs)
                    {
                        var gameVariantId = gameVariantConfig.Key;
                        var gameVariantDetails = gameVariantConfig.Value;
                        var outcomes = DownloadGameVariantOutcomes(gameId, gameVariantId, gameVariantDetails.OutcomeCount);
                        allOutcomes[gameId][gameVariantId] = outcomes;
                    }
                }
            }
            return allOutcomes;
        }

        public static GameConfigsWrapper DownloadGameConfigs()
        {
            var configAssetName = "latest/Configs/GameConfigs.yaml";
            var configAssetURL = $"{OutcomesServerBaseURL}/{configAssetName}";
            using var webClient = new WebClient();
            var yamlContent = webClient.DownloadString(configAssetURL);

            var deserializer = new DeserializerBuilder()
                .Build();
            
            // Deserialize the YAML content to the GameConfig object
            var gameConfigs = deserializer.Deserialize<GameConfigsWrapper>(yamlContent);

            return gameConfigs;
        }
        
        static Dictionary<string, string> DownloadOutcomes(string gameId, int outcomeCount)
        {
            var outcomes = new Dictionary<string, string>();
            using var webClient = new WebClient();
            for (var i = 0; i < outcomeCount; i++)
            {
                var outcomeId = (i+1).ToString("D9"); // Format as a 9-digit number with leading zeros
                var fileName = $"{gameId}Outcome{outcomeId}.cscript.txt";
                var fileUrl = $"{OutcomesServerBaseURL}/latest/Outcomes/{fileName}";

                try
                {
                    var fileContents = webClient.DownloadString(fileUrl);
                    outcomes[fileName] = fileContents;

                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to download {fileUrl}: {ex.Message}");
                }
            }

            return outcomes;
        }
        
        static Dictionary<string, string> DownloadGameVariantOutcomes(string gameId, string gameVariantId, int outcomeCount)
        {
            var outcomes = new Dictionary<string, string>();
            using var webClient = new WebClient();
            for (var i = 0; i < outcomeCount; i++)
            {
                var outcomeId = (i+1).ToString("D9"); // Format as a 9-digit number with leading zeros
                var fileName = $"{gameId}Outcome{outcomeId}.cscript.txt";
                var fileUrl = $"{OutcomesServerBaseURL}/latest/Outcomes/{gameId}/{gameVariantId}/{fileName}";

                try
                {
                    var fileContents = webClient.DownloadString(fileUrl);
                    outcomes[fileName] = fileContents;

                }
                catch (Exception ex)
                {
                    Debug.LogError($"Failed to download {fileUrl}: {ex.Message}");
                }
            }

            return outcomes;
        }

        private static void SetupCoreAssetPath()
        {
            string corePath = $"Assets/Bettr/Core";
            EnsureDirectory(corePath);
            
            InstanceComponent.CorePath = corePath;
        }

        private static void ClearRuntimeAssetPath(string machineName, string machineVariant, string experimentVariant)
        {
            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
            
            if (Directory.Exists(runtimeAssetPath))
            {
                // Delete all files in the directory
                foreach (string file in Directory.GetFiles(runtimeAssetPath))
                {
                    File.Delete(file);
                }

                // Delete all subdirectories in the directory
                foreach (string subDir in Directory.GetDirectories(runtimeAssetPath))
                {
                    Directory.Delete(subDir, true);
                }

                Debug.Log($"Cleared runtime asset path: {runtimeAssetPath}");
            }
            else
            {
                Debug.LogWarning($"Directory does not exist: {runtimeAssetPath}");
            }
            
            AssetDatabase.Refresh();
        }
        
        private static void SetupMachine(string machineName, string machineVariant, string experimentVariant, string machineModel)
        {
            string machineModelName = Path.GetFileNameWithoutExtension(machineModel);

            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
            EnsureDirectory(runtimeAssetPath);

            string[] subDirectories = { "Animators", "Materials", "Models", "FBX", "Prefabs", "Scenes", "Scripts", "Textures" };
            foreach (string subDir in subDirectories)
            {
                EnsureDirectory(Path.Combine(runtimeAssetPath, subDir));
            }
            
            // Copy the machine model file and rename its extension
            string modelDestinationPath = Path.Combine(runtimeAssetPath, "Models",  $"{machineModelName}.cscript.txt");
            Debug.Log($"Copying machine model {machineName} {machineVariant} from: {machineModel} to: {modelDestinationPath}");
            File.Copy(machineModel, modelDestinationPath, overwrite: true);
            
            AssetDatabase.Refresh();
            
            // Load and run the Model file
            var modelTextAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(modelDestinationPath);
            var machineModelScript = modelTextAsset.text;
            TileController.StaticInit();
            DynValue dynValue = TileController.LuaScript.LoadString(machineModelScript, codeFriendlyName: machineModelName);
            TileController.LuaScript.Call(dynValue);
            
            ProcessScripts(machineName, machineVariant, experimentVariant, runtimeAssetPath);
        }

        private static void BuildMachine(string machineName, string machineVariant, string experimentVariant)
        {
            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/{experimentVariant}/Runtime/Asset";
            EnsureDirectory(runtimeAssetPath);
            
            var machines = GetTable($"{machineName}Machines");
            for (var index = 1; index <= machines.Length; index++)
            {
                var machineData = (Table) machines[index];
                ProcessGame(machineData, machineName, machineVariant, runtimeAssetPath);
            }
            
            // Common to all machines
            // Apply mechanics
            ProcessMechanics(machineName, machineVariant, runtimeAssetPath);
            ProcessScene(machineName, machineVariant, runtimeAssetPath);
            
        }
        
        private static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
                Debug.Log("Directory created at: " + path);
            }
        }

        private static void CopyScripts(string scriptsPath, string[] filePaths, string machineName, string machineVariant, string experimentVariant, string runtimeAssetPath)
        {
            var mechanicsTable = GetTable($"{machineName}Mechanics");
            var baseGameMechanics = GetTableArray<string>(mechanicsTable, "BaseGame", "Mechanic");
            // convert baseGameMechanics array to PascalCase using ConvertCamelToPascalCase
            baseGameMechanics = new List<string>(baseGameMechanics.Select(ConvertCamelToPascalCase).ToArray());
            
            var bundleName = $"{machineName}{machineVariant}".ToLower();
            var bundleVersion = $"{experimentVariant}".ToLower();
            
            foreach (string filePath in filePaths)
            {
                var reelCount = GetReelCount(machineName);
                var scribanTemplate = ParseScribanTemplate(scriptsPath, filePath);
                var model = new Dictionary<string, object>
                {
                    { "machineName", machineName },
                    { "machineVariant", machineVariant },
                    { "experimentVariant", experimentVariant },
                    { "bundleName", bundleName },
                    { "bundleVersion", bundleVersion },
                    { "machines", new string[]
                        {
                            "BaseGame",
                        } 
                    },
                    { "reelCount", reelCount },
                    { "baseGameMechanics", baseGameMechanics},
                };
                var scriptText = scribanTemplate.Render(model);
                var scriptName = Path.GetFileNameWithoutExtension(filePath); // remove the .template
                scriptName = Regex.Replace(scriptName, @"^Game", machineName);
                
                var destinationPath = Path.Combine(runtimeAssetPath, "Scripts", $"{scriptName}");
                File.WriteAllText(destinationPath, scriptText);
            }
        }

        private static void ProcessScripts(string machineName, string machineVariant, string experimentVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            string dirPath = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", "scripts");
            string[] filePaths = Directory.GetFiles(dirPath, "*.cscript.txt.template");
            string scriptsPath = $"scripts";
            CopyScripts(scriptsPath, filePaths, machineName, machineVariant, experimentVariant, runtimeAssetPath);
            
            // Process Mechanics scripts
            var mechanicsTable = GetTable($"{machineName}Mechanics");
            var pkArray = GetTablePkArray(mechanicsTable);
            foreach (var pk in pkArray)
            {
                var mechanicsData = GetTableArray<string>(mechanicsTable, pk, "Mechanic");
                foreach (var mechanic in mechanicsData)
                {
                    dirPath = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", "mechanics", mechanic, "scripts");
                    filePaths = Directory.GetFiles(dirPath, "*.cscript.txt.template");
                    scriptsPath = $"mechanics/{mechanic}/scripts";
                    CopyScripts(scriptsPath, filePaths, machineName, machineVariant, experimentVariant, runtimeAssetPath);
                }
            }

        }
        
        private static GameObject ProcessBaseGameSymbols(string machineName, string machineVariant, string runtimeAssetPath)
        {
            var templateName = "BaseGameSymbolGroup";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);
            
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();

            AssetDatabase.Refresh();
            
            foreach (var symbolKey in symbolKeys)
            {
                ProcessBaseGameSymbol(machineName, machineVariant, symbolKey, $"{machineName}BaseGameSymbol{symbolKey}", runtimeAssetPath);
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "symbolKeys", symbolKeys},
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"ProcessBaseGameSymbols: {json}");
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();

            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);

            var scriptGroupName = $"{machineName}BaseGameSymbolGroup"; 
            var symbolGroup = BettrPrefabController.ProcessPrefab(scriptGroupName, 
                hierarchyInstance, 
                runtimeAssetPath);
            
            return symbolGroup;
        }

        private static GameObject ProcessBaseGameSymbol(string machineName, string machineVariant, string symbolName, string symbolPrefabName, string runtimeAssetPath)
        {
            var templateName = "BaseGameSymbol";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);
            
            var model = new Dictionary<string, object>
            {
                { "symbolName", symbolName },
                { "symbolPrefabName", symbolPrefabName },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"ProcessBaseGameSymbol: {json}");

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);

            var settingsPrefab = BettrPrefabController.ProcessPrefab(symbolPrefabName, 
                hierarchyInstance, 
                runtimeAssetPath);

            return settingsPrefab;
        }
        
        private static void ProcessBaseGameMachine(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string baseGameMachine = $"{machineName}BaseGameMachine";
            
            string baseGameSettings = $"{machineName}BaseGameSettings";
            
            var scriptName = $"{machineName}BaseGameReel";   
            BettrScriptGenerator.CreateOrLoadScript(scriptName, runtimeAssetPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            var maxOffsetY = GetReelMaxOffsetY(machineName);
            
            var reelBackgroundX = GetReelBackgroundX(machineName);
            var reelBackgroundY = GetReelBackgroundY(machineName);
            
            var reelBackgroundScaleX = GetReelBackgroundScaleX(machineName);
            var reelBackgroundScaleY = GetReelBackgroundScaleY(machineName);
            
            var reelMaskUpperX = GetReelMaskUpperX(machineName);
            var reelMaskUpperY = GetReelMaskUpperY(machineName);
            
            var reelMaskLowerX = GetReelMaskLowerX(machineName);
            var reelMaskLowerY = GetReelMaskLowerY(machineName);
            
            var reelMaskScaleX = GetReelMaskScaleX(machineName);
            var reelMaskScaleY = GetReelMaskScaleY(machineName);

            var scaleX = 0.90;
            var scaleY = 0.90;
            
            var templateName = "BaseGameMachine";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);
            
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();

            var reelCount = GetReelCount(machineName);
            var horizontalReelPositions = GetReelHorizontalPositions(machineName);
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "baseGameMachine", baseGameMachine },
                { "baseGameSettings", baseGameSettings },
                { "symbolKeys", symbolKeys},
                { "reelMaskUpperX", reelMaskUpperX},
                { "reelMaskUpperY", reelMaskUpperY},
                { "reelMaskLowerX", reelMaskLowerX},
                { "reelMaskLowerY", reelMaskLowerY},
                { "reelMaskScaleX", reelMaskScaleX},
                { "reelMaskScaleY", reelMaskScaleY},
                { "reelBackgroundX", reelBackgroundX},
                { "reelBackgroundY", reelBackgroundY},
                { "reelBackgroundScaleX", reelBackgroundScaleX},
                { "reelBackgroundScaleY", reelBackgroundScaleY},
                { "offsetY", maxOffsetY },
                { "scaleX", scaleX },
                { "scaleY", scaleY },
                { "reelCount", reelCount },
                { "horizontalReelPositions", horizontalReelPositions },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"BaseGameMachine: {json}");
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            
            var baseGameMachinePrefab = BettrPrefabController.ProcessPrefab($"{baseGameMachine}", 
                hierarchyInstance, 
                runtimeAssetPath);
        }

        private static IGameObject ProcessBaseGameSymbolGroup(int symbolIndex, string runtimeAssetPath, string machineName)
        {
            var symbolInstance = new InstanceGameObject(new GameObject($"Symbol{symbolIndex}"));
            var pivotInstance = new InstanceGameObject(new GameObject("Pivot"));
            pivotInstance.SetParent(symbolInstance.GameObject);
            
            var symbolGroupPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{runtimeAssetPath}/Prefabs/{machineName}BaseGameSymbolGroup.prefab");
            var prefabGameObject = new PrefabGameObject(symbolGroupPrefab, "SymbolGroup");
            prefabGameObject.SetParent(pivotInstance.GameObject);

            return symbolInstance;
        }

        private static void ProcessBaseGameReels(string machineName, string machineVariant, string runtimeAssetPath)
        {
            var reelCount = GetReelCount(machineName);
            for (int i = 0; i < reelCount; i++)
            {
                var reelIndex = i + 1;
                ProcessBaseGameReel(machineName, machineVariant, reelIndex, runtimeAssetPath);
            }
        }

        private static void ProcessBaseGameReel(string machineName, string machineVariant, int reelIndex, string runtimeAssetPath)
        {
            // refresh the asset database
            AssetDatabase.Refresh();
            
            var reelName = $"{machineName}BaseGameReel{reelIndex}";

            var symbolKeys = GetSymbolKeys(machineName);
            
            var symbolCount = GetSymbolCount(machineName, reelIndex);
            var symbolIndexes = Enumerable.Range(1, symbolCount).ToList();
            
            var symbolPositions = GetSymbolPositions(machineName, reelIndex);
            var symbolVerticalSpacing = GetSymbolVerticalSpacing(machineName, reelIndex);
            var yPositions = symbolPositions.Select(pos => pos * symbolVerticalSpacing).ToList();

            yPositions.Insert(0, 0);
            
            var symbolScaleX = GetSymbolScaleX(machineName, reelIndex);
            var symbolScaleY = GetSymbolScaleY(machineName, reelIndex);
            
            var symbolOffsetY = GetSymbolOffsetY(machineName, reelIndex);

            var templateName = "BaseGameReel";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);

            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "reelIndex", reelIndex },
                { "symbolKeys", symbolKeys },
                { "yPositions", yPositions },
                { "symbolIndexes", symbolIndexes },
                { "symbolScaleX", symbolScaleX },
                { "symbolScaleY", symbolScaleY },
                { "symbolOffsetY", symbolOffsetY },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"template: {templateName} json: {json}");
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            var reelPrefab = BettrPrefabController.ProcessPrefab(reelName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessUICamera(string cameraName, bool includeAudioListener, string runtimeAssetPath)
        {
            BettrPrefabController.ProcessPrefab(cameraName, new List<IComponent>
                {
                    new UICameraComponent(includeAudioListener),
                }, 
                new List<IGameObject>(),
                runtimeAssetPath);
        }
        
        private static void ProcessBaseGameBackground(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string backgroundName = $"{machineName}BaseGameBackground";
            
            var backgroundScriptName = $"{machineName}BaseGameBackground";   
            BettrScriptGenerator.CreateOrLoadScript(backgroundScriptName, runtimeAssetPath);
            
            string templateName = "BaseGameBackground";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);

            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "backgroundName", backgroundName },
            };
            
            var json = scribanTemplate.Render(model);
            Console.WriteLine(json);
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            BettrPrefabController.ProcessPrefab(backgroundName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }

        private static void ProcessGame(Table machineData, string machineName, string machineVariant, string runtimeAssetPath)
        {
            var isBase = (bool) machineData["IsBase"];
            var isBonus = (bool) machineData["IsBonus"];
            var isFreeSpins = (bool) machineData["IsFreeSpins"];
            var isWheel = (bool) machineData["IsWheel"];
            if (isBase)
            {
                ProcessBaseGameBackground(machineName, machineVariant, runtimeAssetPath);
                ProcessBaseGameSymbols(machineName, machineVariant, runtimeAssetPath);
                ProcessBaseGameReels(machineName, machineVariant, runtimeAssetPath);
                ProcessBaseGameMachine(machineName, machineVariant, runtimeAssetPath);
            }
            // else if (isBonus)
            // {
            //     if (isFreeSpins)
            //     {
            //         ProcessFreeSpinsBackground(machineName, machineVariant, runtimeAssetPath);
            //         ProcessFreeSpinsMachine(machineName, machineVariant, runtimeAssetPath);
            //     } 
            //     else if (isWheel)
            //     {
            //         ProcessWheelGameBackground(machineName, machineVariant, runtimeAssetPath);
            //         ProcessWheelGameMachine(machineName, machineVariant, runtimeAssetPath);
            //     }
            // }
        }
        
        private static SceneAsset ProcessScene(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();

            SceneAsset sceneAsset = null;
            Scene scene = default;
            
            var sceneName = $"{machineName}{machineVariant}Scene";
            string scenePath = $"{runtimeAssetPath}/Scenes/{sceneName}.unity";
            
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            if (sceneAsset != null)
            {
                EditorSceneManager.OpenScene(scenePath);
            }
            else
            {
                scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                scene.name = sceneName;
                if (!EditorSceneManager.SaveScene(scene, scenePath))
                {
                    throw new Exception($"Failed to save scene at path: {scenePath}");
                }
            }
            
            AssetDatabase.Refresh();
            
            EditorSceneManager.OpenScene(scenePath);
            
            var templateName = "Scene";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);

            var machineNames = HasTable($"{machineName}Machines") ? GetTableArray<string>(GetTable($"{machineName}Machines"), null, "Name") : new List<string>();
            var machineTransitionNames = HasTable($"{machineName}MachineTransitions") ? GetTableArray<string>(GetTable($"{machineName}MachineTransitions"), null, "Name") : new List<string>();
            var machineActivations = HasTable($"{machineName}MachineTransitionDialogs") ? GetTablePkDict<string>(GetTable($"{machineName}MachineTransitionDialogs")) : new Dictionary<string, List<Dictionary<string, string>>>(); 

            // Create a model object with the machineName variable
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "sceneName", sceneName },
                { "machines", machineNames},
                { "machineTransitions", machineTransitionNames},
                { "machineActivations", machineActivations},
            };
            
            var context = new TemplateContext();
            var scriptObject = new ScriptObject();
            
            context.PushGlobal(scriptObject);
            
            // run it through Scriban
            var json = scribanTemplate.Render(model);
            
            Console.WriteLine(json);
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            JsonConvert.DeserializeObject<InstanceGameObject>(json);
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            
            AssetDatabase.Refresh();
            
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);

            return sceneAsset;
        }
        
        private static void ProcessMechanics(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var mechanicsTable = GetTable($"{machineName}Mechanics");
            var pkArray = GetTablePkArray(mechanicsTable);
            foreach (var pk in pkArray)
            {
                switch (pk)
                {
                    case "BaseGame":
                        var baseGameMechanics = GetTableArray<string>(mechanicsTable, pk, "Mechanic");
                        foreach (var baseGameMechanic in baseGameMechanics)
                        {
                            switch (baseGameMechanic.ToLower())
                            {
                                case "ways":
                                    BaseGameWaysMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "paylines":
                                    BaseGamePaylinesMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "cascadingreels":
                                    BaseGameCascadingReelsMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "chooseaside":
                                    BaseGameChooseASideMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "expandingreels":
                                    BaseGameExpandingReelsMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "horizontalreels":
                                    BaseGameHorizontalReelsMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "horizontalreelsshift":
                                    BaseGameHorizontalReelsShiftMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "wildsmultiplier":
                                    BaseGameWildsMultiplierMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                case "megaways":
                                    BaseGameMegawaysMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                default:
                                    throw new Exception($"BaseGame mechanic not found: {baseGameMechanic}");
                            }
                        }
                        break;
                    default:
                        break;
                }
            }
            // if (HasTable($"{machineName}BaseGameScatterBonusFreeSpinsMechanic"))
            // {
            //     ProcessBaseGameScatterBonusFreeSpinsMechanic(machineName, machineVariant, runtimeAssetPath);
            // }
            //
            // if (HasTable($"{machineName}BaseGameRandomMultiplierWildsMechanic"))
            // {
            //     ProcessBaseGameRandomMultiplierWildsMechanic(machineName, machineVariant, runtimeAssetPath);
            // }
        }
        
        public static Template ParseScribanTemplate(string prefixPath, string templateName)
        {
            var templateText = ReadScribanTemplate(prefixPath, templateName);
            var scribanTemplate = Template.Parse(templateText);
            if (scribanTemplate.HasErrors)
            {
                Debug.LogError($"Scriban template has errors: {scribanTemplate.Messages} filePath: {templateName}");
                throw new Exception($"Scriban template has errors: {scribanTemplate.Messages} filePath: {templateName}");
            }

            return scribanTemplate;
        }
        
        private static string ReadScribanTemplate(string prefixPath, string fileName)
        {
            string path = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", prefixPath, $"{Path.GetFileNameWithoutExtension(fileName)}.template");
            if (!File.Exists(path))
            {
                Debug.LogError($"Scriban template file not found at path: {path}");
                return null;
            }

            return File.ReadAllText(path);
        }
        
        private static bool PostToService()
        {
            var httpRequest = (HttpWebRequest)WebRequest.Create(GatewayUrl);
            httpRequest.Method = "POST";
            httpRequest.ContentType = "application/json";

            var data = @"{
                ""gameId"": ""Game002"",
                ""hashKey"": ""latest""
            }";

            using (var streamWriter = new StreamWriter(httpRequest.GetRequestStream()))
            {
                streamWriter.Write(data);
            }

            try
            {
                var httpResponse = (HttpWebResponse)httpRequest.GetResponse();
                using (var streamReader = new StreamReader(httpResponse.GetResponseStream()))
                {
                    var result = streamReader.ReadToEnd();
                    Debug.Log(result);
                }
            
                if (httpResponse.StatusCode == HttpStatusCode.OK)
                {
                    Debug.Log("Successfully posted to service.");
                    return true;
                }
                else
                {
                    Debug.LogError("Failed to post to service.");
                    return false;
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e);
            }

            return false;
        }
        
        private static Dictionary<string, List<Dictionary<string, T>>> GetTablePkDict<T>(Table table)
        {
            var dict = new Dictionary<string, List<Dictionary<string, T>>>();
            foreach (var pair in table.Pairs)
            {
                dict[pair.Key.String] = GetTableArray<T>((Table) pair.Value.Table["Array"]);
            }
            return dict;
        }
        
        private static List<string> GetTablePkArray(Table table)
        {
            return table.Pairs.Select(pair => pair.Key.String).ToList(); 
        }
        
        private static List<Dictionary<string, T>> GetTableArray<T>(Table table)
        {
            var list = new List<Dictionary<string, T>>();
            foreach (var pair in table.Pairs)
            {
                var dict = new Dictionary<string, T>();
                foreach (var valuePair in pair.Value.Table.Pairs)
                {
                    dict[valuePair.Key.String] = TileController.ProcessResult<T>(valuePair.Value);
                }
                list.Add(dict);
            }
            return list;
        }
        
        private static List<T> GetTableArray<T>(Table table, string pk, string key)
        {
            Table valueTable = table;
            if (!string.IsNullOrEmpty(pk) && table[pk] is Table pkTable)
            {
                valueTable = pkTable["Array"] as Table;
            }
            
            return valueTable?.Pairs.Select(pair => pair.Value.Table[key]).ToList().Cast<T>().ToList();
        }
        
        public static T GetTableValue<T>(Table table, string pk, string key, T d = default(T))
        {
            var valueTable = table;
            if (!string.IsNullOrEmpty(pk) && table[pk] is Table pkTable)
            {
                valueTable = pkTable;
            }
            
            var first = valueTable["First"] as Table;
            if (first == null)
            {
                Debug.LogError($"Key {pk} First not found in table.");
                return default(T);
            }
            
            var value = first[key];
            if (value != null)
            {
                return (T)Convert.ChangeType(value, typeof(T));
            }
            
            return d;
        }
        
        public static T GetTableKeyValue<T>(Table table, string pk, string key)
        {
            var valueTable = table;
            if (!string.IsNullOrEmpty(pk) && table[pk] is Table pkTable)
            {
                valueTable = pkTable;
            }
            
            var arr = valueTable["Array"] as Table;
            if (arr == null)
            {
                Debug.LogError($"Key {pk} Array not found in table.");
                return default(T);
            }

            foreach (var val in arr.Values)
            {
                var valTable = val.Table;
                if (valTable == null)
                {
                    Debug.LogError($"Value is not a table.");
                    return default(T);
                }
                
                var keyVal = (string) valTable["Key"];
                if (keyVal == key)
                {
                    return (T)Convert.ChangeType(valTable["Value"], typeof(T));
                }
            }

            return default(T);
        }

        public static Table GetTable(string tableName)
        {
            var table = TileController.LuaScript.Globals[tableName] as Table;
            if (table == null)
            {
                Debug.LogError($"{tableName} not found in the machine model.");
            }
            return table;
        }
        
        private static bool HasTable(string tableName)
        {
            var table = TileController.LuaScript.Globals[tableName] as Table;
            return table != null;
        }

        public static int GetReelCount(string machineName)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var reelCount = 0;
            foreach (var pair in reelStates.Pairs)
            {
                reelCount++;
            }
            return reelCount;
        }
        
        public static List<float> GetReelHorizontalPositions(string machineName)
        {
            var reelHorizontalPositions = new List<float>();
            var baseGameReelStateTable = GetTable($"{machineName}BaseGameReelState");
            var reelCount = GetReelCount(machineName);
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableValue<float>(baseGameReelStateTable, $"Reel{reelId}", "HorizontalPosition");
                reelHorizontalPositions.Add(value);
            }
            
            return reelHorizontalPositions;
        }
        
        public static float GetReelMaxOffsetY(string machineName)
        {
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameLayout");
            var value = GetTableValue<float>(baseGameLayoutTable, "ReelMaxOffsetY", "Value");
            return value;
        }
        
        public static List<float> GetReelMaskUpperX(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskUpperX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskUpperY(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskUpperY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskLowerX(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskLowerX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskLowerY(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskLowerY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskScaleX(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskScaleX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskScaleY(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskScaleY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundX(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundY(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundScaleX(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundScaleX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundScaleY(string machineName)
        {
            var values = new List<float>();
            var reelCount = GetReelCount(machineName);
            var baseGameLayoutTable = GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundScaleY");
                values.Add(value);
            }
            return values;
        }

        public static List<string> GetSymbolKeys(string machineName)
        {
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            return symbolKeys;
        }
        
        public static List<int> GetSymbolPositions(string machineName, int reelIndex)
        {
            var reelSymbolStates = GetTable($"{machineName}BaseGameReelSymbolsState");
            var symbolPositions = GetTableArray<double>(reelSymbolStates, $"Reel{reelIndex}", "SymbolPosition");
            return symbolPositions.Select(d => (int)d).ToList();
        }
        
        public static int GetSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var topSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "SymbolCount");
            return topSymbolCount;
        }

        public static int GetTopSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var topSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "TopSymbolCount");
            return topSymbolCount;
        }
        
        public static int GetVisibleSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var visibleSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "VisibleSymbolCount");
            return visibleSymbolCount;
        }
        
        public static int GetBottomSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var bottomSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "BottomSymbolCount");
            return bottomSymbolCount;
        }
        
        public static float GetSymbolVerticalSpacing(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolVerticalSpacing");
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolOffsetY(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolOffsetY");
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolScaleX(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolScaleX", 1);
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolScaleY(string machineName, int reelIndex)
        {
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolScaleY", 1);
            return symbolVerticalSpacing;
        }
        
        public static string ConvertCamelToPascalCase(string camelCaseString)
        {
            if (string.IsNullOrEmpty(camelCaseString) || char.IsUpper(camelCaseString[0]))
            {
                return camelCaseString;
            }

            return char.ToUpper(camelCaseString[0]) + camelCaseString.Substring(1);
        }
    }
    
    public class SimpleStringInterpolator
    {
        private readonly Dictionary<string, string> _variables;

        public SimpleStringInterpolator()
        {
            _variables = new Dictionary<string, string>();
        }

        public void SetVariable(string key, string value)
        {
            _variables[key] = value;
        }

        public string Interpolate(string template)
        {
            return Regex.Replace(template, @"\{(\w+)\}", match =>
            {
                string key = match.Groups[1].Value;
                return _variables.TryGetValue(key, out string value) ? value : match.Value;
            });
        }
    }
}