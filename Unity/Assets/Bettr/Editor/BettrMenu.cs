using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using UnityEditor.Animations;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DirectoryInfo = System.IO.DirectoryInfo;
using Object = System.Object;

using Scriban;

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
        private const string AssetsServerBaseURL = "https://bettr-casino-assets.s3.us-west-2.amazonaws.com";
        private const string GatewayUrl = "https://3wcgnl14qb.execute-api.us-west-2.amazonaws.com/gateway";
        
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

        [MenuItem("Bettr/Verify Install")]
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
        
        [MenuItem("Bettr/Enter Play Mode")] 
        public static void Start()
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
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.iOS, BuildTarget.iOS);

            // Path to your specific scene. Adjust the path as necessary.
            const string scenePath = "Assets/Bettr/Core/Scenes/MainScene.unity";

            // Open the specified scene
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);

            // Enter play mode
            EditorApplication.EnterPlaymode();
        }

        [MenuItem("Bettr/Assets/Build")] 
        public static void BuildAssets()
        {
            BuildAssetBundles();
            BuildLocalServer();
        }
        
        //[MenuItem("Bettr/Assets/Cleanup")]
        public static void CleanupTestScenes()
        {
            RemoveTestScenes(new DirectoryInfo("Assets/"));
        }

        private static void BuildAssetBundles()
        {
            Debug.Log("Building asset bundles...");
            
            EnsurePluginAssetsHaveLabels(PluginRootDirectory);
            
            Debug.Log("...refreshing database before building asset bundles..");
            AssetDatabase.Refresh();

            var sharedAssetBundleOptions = BuildAssetBundleOptions.StrictMode |
                                           BuildAssetBundleOptions.ChunkBasedCompression;

#if UNITY_IOS
            EmptyDirectory(new DirectoryInfo(AssetBundlesIOSDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesIOSDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.iOS);
            AssetDatabase.Refresh();
            
            EmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
            AssetDatabase.Refresh();
            
#endif
#if UNITY_ANDROID
            EmptyDirectory(new DirectoryInfo(AssetBundlesAndroidDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesAndroidDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.Android);
#endif
#if UNITY_WEBGL
            EmptyDirectory(new DirectoryInfo(AssetBundlesWebglDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWebglDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.WebGL);
#endif
#if UNITY_OSX
            EmptyDirectory(new DirectoryInfo(AssetBundlesOSXDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesOSXDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneOSX);
#endif
#if UNITY_WIN
            EmptyDirectory(new DirectoryInfo(AssetBundlesWindowsDirectory));
            AssetDatabase.Refresh();
            BuildPipeline.BuildAssetBundles(AssetBundlesWindowsDirectory, 
                sharedAssetBundleOptions,
                BuildTarget.StandaloneWindows64);
#endif
#if UNITY_LINUX   
            EmptyDirectory(new DirectoryInfo(AssetBundlesLinuxDirectory));
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
                    importer.assetBundleName = GetAssetBundleName(assetLabel, assetType);
                    importer.assetBundleVariant = GetAssetBundleVariant(assetSubLabel);
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
                        importer.assetBundleName = GetAssetBundleName(assetLabel, assetType);
                        importer.assetBundleVariant = GetAssetBundleVariant(assetSubLabel);
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
        
        private static void BuildDirectory(this DirectoryInfo directory, bool force = false)
        {
            if (!directory.Exists || force)
            {
                directory.Create();
            }
        }

        private static string GetAssetBundleName(string assetLabel, Type assetType)
        {
            var isScene = assetType.Name == "SceneAsset";
            var suffix = isScene ? "_scenes" :"";
            var assetBundleName = $"{assetLabel}{suffix}";
            return assetBundleName;
        }
        
        private static string GetAssetBundleVariant(string assetSubLabel)
        {
            return assetSubLabel;
        }
        
        private static void ModifyAssetBundleManifestFiles()
        {
            var files = Directory.GetFiles(AssetBundlesDirectory);
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
        
        private static void BuildLocalServer()
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

            using (var webClient = new System.Net.WebClient())
            {
                try
                {
                    byte[] jsonBytes = webClient.DownloadData(assetBundleURL);
                    string jsonString = System.Text.Encoding.UTF8.GetString(jsonBytes);
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

        public static void SyncMachine()
        {
            string machineName = GetArgument("-machineName");
            string machineVariant = GetArgument("-machineVariant");
            string machineModel = GetArgument("-machineModel");
            string machineModelName = Path.GetFileNameWithoutExtension(machineModel);

            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset";
            EnsureDirectory(runtimeAssetPath);

            string[] subDirectories = { "Animators", "Materials", "Models", "Prefabs", "Scenes", "Scripts", "Shaders", "Textures" };
            foreach (string subDir in subDirectories)
            {
                EnsureDirectory(Path.Combine(runtimeAssetPath, subDir));
            }
            
            // Copy the shader files
            string shaderSourcePath = Path.Combine("Assets", "Bettr", "Editor", "Shaders");
            string shaderDestinationPath = Path.Combine(runtimeAssetPath, "Shaders");
            Debug.Log($"Copying shaders from: {machineModel} to: {shaderDestinationPath}");
            // Ensure the destination directory exists
            if (!Directory.Exists(shaderDestinationPath))
            {
                Directory.CreateDirectory(shaderDestinationPath);
            }

            // Get all shader files in the source directory
            string[] shaderFiles = Directory.GetFiles(shaderSourcePath, "*.shader");

            // Copy each shader file to the destination directory
            foreach (string file in shaderFiles)
            {
                string fileName = Path.GetFileName(file);
                string destFile = Path.Combine(shaderDestinationPath, fileName);
                AssetDatabase.CopyAsset(file, destFile);
            }

            Debug.Log("Shader files copied successfully.");
            
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

            ProcessBaseGameSymbols(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameMachine(machineName, machineVariant, runtimeAssetPath);
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

        private static GameObject ProcessBaseGameSymbols(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            
            var symbolPrefabs = new List<IGameObject>();
            foreach (var pair in baseGameSymbolTable.Pairs)
            {
                var symbolName = pair.Key.String;
                var symbolPrefabName = $"{machineName}BaseGameSymbol{symbolName}";   
                var symbolPrefab = ProcessBaseGameSymbol(symbolName, symbolPrefabName, runtimeAssetPath);
                symbolPrefabs.Add(new PrefabGameObject(symbolPrefab, symbolName));
            }
            
            var scriptGroupName = $"{machineName}BaseGameSymbolGroup"; 
            var symbolGroup = ProcessPrefab(scriptGroupName, new List<IComponent>(), 
                symbolPrefabs,
                runtimeAssetPath);
            
            return symbolGroup;
        }

        private static GameObject ProcessBaseGameSymbol(string symbolName, string symbolPrefabName, string runtimeAssetPath)
        {
            SimpleStringInterpolator interpolator = new SimpleStringInterpolator();
            interpolator.SetVariable("symbolName", symbolName);
            interpolator.SetVariable("symbolPrefabName", symbolPrefabName);
            
            string jsonTemplate = ReadJson("BaseGameSymbol");
            string json = interpolator.Interpolate(jsonTemplate);

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            List<IGameObject> runtimeObjects = hierarchyInstance.Child != null ? new List<IGameObject>() {hierarchyInstance.Child} : hierarchyInstance.Children != null ? hierarchyInstance.Children.Cast<IGameObject>().ToList() : new List<IGameObject>();
            List<IComponent> components = hierarchyInstance.Components.Cast<IComponent>().ToList();

            var settingsPrefab = ProcessPrefab(symbolPrefabName, 
                components, 
                runtimeObjects,
                runtimeAssetPath);

            return settingsPrefab;
        }
        
        private static void ProcessBaseGameMachine(string machineName, string machineVariant, string runtimeAssetPath)
        {
            var scriptName = $"{machineName}BaseGameReel";   
            var scriptTextAsset = CreateOrLoadScript(scriptName, runtimeAssetPath);
            
            var baseGameReelState = GetTable($"{machineName}BaseGameReelState");
            
            var gameObjectInstances = new List<IGameObject>();
            
            //
            // Cameras
            //
            var camerasGameObject = new InstanceGameObject(new GameObject($"Cameras"));
            var camerasPivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            camerasPivotGameObject.SetParent(camerasGameObject.GameObject);
            ProcessUICamera($"{machineName}BaseGameUICamera", runtimeAssetPath);
            var cameraPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{runtimeAssetPath}/Prefabs/{machineName}BaseGameUICamera.prefab");
            var cameraPrefabGameObject = new PrefabGameObject(cameraPrefab, $"{machineName}BaseGameUICamera");
            cameraPrefabGameObject.SetParent(camerasPivotGameObject.GameObject);
            
            gameObjectInstances.Add(camerasGameObject);
            
            var machinePivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            gameObjectInstances.Add(machinePivotGameObject);
            
            //
            // Reels
            //
            var reelsGameObject = new InstanceGameObject(new GameObject($"Reels"));
            reelsGameObject.SetParent(machinePivotGameObject.GameObject);
            var reelsPivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            reelsPivotGameObject.SetParent(reelsGameObject.GameObject);
            
            int reelCount = 0;
            foreach (var pair in baseGameReelState.Pairs)
            {
                reelCount++;
                ProcessBaseGameReel(machineName, machineVariant, reelCount, scriptTextAsset, runtimeAssetPath);
                // load the reel prefab
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{runtimeAssetPath}/Prefabs/{machineName}BaseGameReel{reelCount}.prefab");
                var prefabGameObject = new PrefabGameObject(prefab, $"Reel{reelCount}");
                prefabGameObject.SetParent(reelsPivotGameObject.GameObject);
            }
            
            //
            // Background
            //
            var backgroundGameObject = new InstanceGameObject(new GameObject($"Reels Background"));
            backgroundGameObject.SetParent(machinePivotGameObject.GameObject);
            var backgroundPivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            backgroundPivotGameObject.SetParent(backgroundGameObject.GameObject);
            
            var backgroundScriptName = $"{machineName}BaseGameBackground";   
            var backgroundScriptTextAsset = CreateOrLoadScript(backgroundScriptName, runtimeAssetPath);
            ProcessBaseGameBackground(machineName, machineVariant, backgroundScriptTextAsset, runtimeAssetPath);
            
            var backgroundPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{runtimeAssetPath}/Prefabs/{machineName}BaseGameBackground.prefab");
            var backgroundPrefabGameObject = new PrefabGameObject(backgroundPrefab, $"Reels Background");
            backgroundPrefabGameObject.SetParent(backgroundPivotGameObject.GameObject);
            
            //
            // Scrim
            //
            var reelsScrimGameObject = new InstanceGameObject(new GameObject($"Reels Scrim"));
            reelsScrimGameObject.SetParent(machinePivotGameObject.GameObject);
            var reelsScrimPivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            reelsScrimPivotGameObject.SetParent(reelsScrimGameObject.GameObject);
            var reelsScrimQuadInstance = new InstanceGameObject(GameObject.CreatePrimitive(PrimitiveType.Quad));
            reelsScrimQuadInstance.GameObject.SetActive(false);
            reelsScrimQuadInstance.SetParent(reelsScrimPivotGameObject.GameObject);
            
            //
            // Win Symbols
            //

            var winSymbolsGameObject = ProcessWinSymbols(runtimeAssetPath);
            winSymbolsGameObject.SetParent(machinePivotGameObject.GameObject);

            // ReSharper disable once PossibleNullReferenceException
            var winSymbolsPivotGameObject = (winSymbolsGameObject as InstanceGameObject).Child;
            
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            foreach (var pair in baseGameSymbolTable.Pairs)
            {
                var symbolKey = pair.Key.String;
                var symbolPrefabName = $"{machineName}BaseGameSymbol{symbolKey}";   
                var winSymbolGameObject = ProcessWinSymbol( symbolKey, symbolPrefabName, runtimeAssetPath);
                winSymbolGameObject.GameObject.SetActive(false);
                winSymbolGameObject.SetParent(winSymbolsPivotGameObject.GameObject);
            }
            
            //
            // Settings Prefab
            //
            var settingsGameObject = new InstanceGameObject(new GameObject($"Settings"));
            settingsGameObject.GameObject.SetActive(false);
            settingsGameObject.SetParent(machinePivotGameObject.GameObject);
            var settingsPivotGameObject = new InstanceGameObject(new GameObject($"Pivot"));
            settingsPivotGameObject.SetParent(settingsGameObject.GameObject);
            
            var settingsName = $"{machineName}BaseGameSettings";   
            ProcessSettings(settingsName, runtimeAssetPath);
            
            var settingsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{runtimeAssetPath}/Prefabs/{machineName}BaseGameSettings.prefab");
            var settingsPrefabGameObject = new PrefabGameObject(settingsPrefab, $"Settings");
            settingsPrefabGameObject.SetParent(settingsPivotGameObject.GameObject);
            
            ProcessPrefab($"{machineName}BaseGameMachine", new List<IComponent>(), 
                gameObjectInstances,
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

        private static IGameObject ProcessBaseGameWaysWin(int symbolIndex, string runtimeAssetPath,
            string machineName)
        {
            var waysInstance = new InstanceGameObject(new GameObject($"Ways{symbolIndex}"));
            var waysPivotInstance = new InstanceGameObject(new GameObject("Pivot"));
            waysPivotInstance.SetParent(waysInstance.GameObject);
                    
            var waysWinPrefab = ProcessWaysWin($"{machineName}BaseGameWaysWin", runtimeAssetPath);
            var waysWinPrefabGameObject = new PrefabGameObject(waysWinPrefab, $"WaysWin");
            waysWinPrefabGameObject.SetParent(waysPivotInstance.GameObject);

            return waysInstance;
        }

        private static void ProcessBaseGameReel(string machineName, string machineVariant, int reelIndex, TextAsset scriptTextAsset, string runtimeAssetPath)
        {
            // refresh the asset database
            AssetDatabase.Refresh();
            
            var reelName = $"{machineName}BaseGameReel{reelIndex}";
            
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var topSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "TopSymbolCount");
            var visibleSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "VisibleSymbolCount");
            var bottomSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "BottomSymbolCount");
            var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolVerticalSpacing");
            
            int half = (topSymbolCount + visibleSymbolCount + bottomSymbolCount) / 2;
            var startVerticalPosition = half * symbolVerticalSpacing;
            
            var gameObjectInstances = new List<IGameObject>();

            for (int symbolIndex = 1; symbolIndex <= topSymbolCount; symbolIndex++)
            {
                var yPosition = startVerticalPosition - symbolIndex * symbolVerticalSpacing;
                var symbolInstance = (InstanceGameObject) ProcessBaseGameSymbolGroup(symbolIndex, runtimeAssetPath, machineName);
                symbolInstance.GameObject.transform.position = new Vector3(0, yPosition, 0);
                gameObjectInstances.Add(symbolInstance);
            }
            
            var baseGameOverviewTable = GetTable($"{machineName}BaseGameOverview");
            var payType = GetTableValue<string>(baseGameOverviewTable, "PayType", "Value");
            
            for (int symbolIndex = topSymbolCount + 1 ; symbolIndex <= topSymbolCount + visibleSymbolCount; symbolIndex++)
            {
                var yPosition = startVerticalPosition - symbolIndex * symbolVerticalSpacing;
                if (payType == "Ways")
                {
                    // add ways reel processing here
                    var waysInstance = (InstanceGameObject) ProcessBaseGameWaysWin(symbolIndex, runtimeAssetPath, machineName);
                    waysInstance.GameObject.transform.position = new Vector3(0, yPosition, 0);
                    waysInstance.GameObject.SetActive(false);
                    gameObjectInstances.Add(waysInstance);
                } 
                else if (payType == "Paylines")
                {
                    // add paylines reel processing here
                }
                
                var symbolInstance = (InstanceGameObject) ProcessBaseGameSymbolGroup(symbolIndex, runtimeAssetPath, machineName);
                symbolInstance.GameObject.transform.position = new Vector3(0, yPosition, 0);
                gameObjectInstances.Add(symbolInstance);
            }

            for (int symbolIndex = topSymbolCount + visibleSymbolCount + 1;
                 symbolIndex <= topSymbolCount + visibleSymbolCount + bottomSymbolCount;
                 symbolIndex++)
            {
                var yPosition = startVerticalPosition - symbolIndex * symbolVerticalSpacing;
                var symbolInstance = (InstanceGameObject) ProcessBaseGameSymbolGroup(symbolIndex, runtimeAssetPath, machineName);
                symbolInstance.GameObject.transform.position = new Vector3(0, yPosition, 0);
                gameObjectInstances.Add(symbolInstance);
            }

            var reelPrefab = ProcessPrefab(reelName, new List<IComponent>()
                {
                    new TileComponent(reelName, scriptTextAsset),
                }, 
                gameObjectInstances,
                runtimeAssetPath);
            
        }
        
        private static GameObject ProcessWaysWin(string symbolName, string runtimeAssetPath)
        {
            var winInstance = new InstanceGameObject(new GameObject("Win"));
            var waysPivotInstance = new InstanceGameObject(new GameObject("Pivot"));
            waysPivotInstance.SetParent(winInstance.GameObject);
            var waysQuadInstance = new InstanceGameObject(GameObject.CreatePrimitive(PrimitiveType.Quad));
            waysQuadInstance.SetParent(waysPivotInstance.GameObject);
            
            var materialName = "WaysWin";
            var shaderName = "Unlit/Texture";
            var material = CreateOrLoadMaterial(materialName, shaderName, runtimeAssetPath);
            var symbolPrefab = ProcessPrefab(symbolName, new List<IComponent>
                {
                }, 
                new List<IGameObject>()
                {
                    winInstance
                },
                runtimeAssetPath);
            return symbolPrefab;
        }
        
        private static void ProcessUICamera(string cameraName, string runtimeAssetPath)
        {
            ProcessPrefab(cameraName, new List<IComponent>
                {
                    new UICameraComponent(),
                }, 
                new List<IGameObject>(),
                runtimeAssetPath);
        }
        
        private static void ProcessBaseGameBackground(string machineName, string machineVariant, TextAsset scriptTextAsset, string runtimeAssetPath)
        {
            SimpleStringInterpolator interpolator = new SimpleStringInterpolator();
            
            string backgroundName = $"{machineName}BaseGameBackground";
            interpolator.SetVariable("backgroundName", backgroundName);
            
            string jsonTemplate = ReadJson("BaseGameBackground");
            string json = interpolator.Interpolate(jsonTemplate);

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            List<IGameObject> runtimeObjects = hierarchyInstance.Child != null ? new List<IGameObject>() {hierarchyInstance.Child} : hierarchyInstance.Children != null ? hierarchyInstance.Children.Cast<IGameObject>().ToList() : new List<IGameObject>();
            List<IComponent> components = hierarchyInstance.Components.Cast<IComponent>().ToList();

            var settingsPrefab = ProcessPrefab(backgroundName, 
                components, 
                runtimeObjects,
                runtimeAssetPath);
        }
        
        private static IGameObject ProcessWinSymbols(string runtimeAssetPath)
        {
            SimpleStringInterpolator interpolator = new SimpleStringInterpolator();
            
            string jsonTemplate = ReadJson("BaseGameWinSymbols");
            string json = interpolator.Interpolate(jsonTemplate);

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);

            return hierarchyInstance;
        }
        
        private static IGameObject ProcessWinSymbol(string symbolName, string symbolPrefabName, string runtimeAssetPath)
        {
            SimpleStringInterpolator interpolator = new SimpleStringInterpolator();
            interpolator.SetVariable("symbolName", symbolName);
            interpolator.SetVariable("symbolPrefabName", symbolPrefabName);
            
            string jsonTemplate = ReadJson("BaseGameWinSymbol");
            string json = interpolator.Interpolate(jsonTemplate);

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);

            return hierarchyInstance;
        }
        
        private static SceneAsset ProcessScene(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();

            var template = Template.Parse(null);
            
            SceneAsset sceneAsset = null;
            Scene scene = default;
            
            var sceneName = $"{machineName}Scene";
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
                EditorSceneManager.SaveScene(scene, scenePath);
            }
            
            AssetDatabase.Refresh();
            
            EditorSceneManager.OpenScene(scenePath);
            
            // run it through Scriban
            var scribanModel = new
            {
                machineName = machineName,
                machineVariant = machineVariant,
                sceneName = sceneName,
                machines = new string[]
                {
                    "BaseGame",
                    "FreeSpins"
                }
            };
            
            string scribanTemplateText = ReadScribanTemplate("Scene");

            var scribanTemplate = Template.Parse(scribanTemplateText);
            if (scribanTemplate.HasErrors)
            {
                Debug.LogError($"Scriban template has errors: {scribanTemplate.Messages}");
                throw new Exception($"Scriban template has errors: {scribanTemplate.Messages}");
            }

            var json = scribanTemplate.Render(scribanModel);
            
            JsonConvert.DeserializeObject<InstanceGameObject>(json);
            
            // add a delay to allow the scene to be saved
            
            EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
            
            AssetDatabase.Refresh();
            
            sceneAsset = AssetDatabase.LoadAssetAtPath<SceneAsset>(scenePath);
            
            return sceneAsset;
        }
        
        private static string ReadJson(string fileName)
        {
            string path = Path.Combine(Application.dataPath, "Bettr", "Editor", "json", $"{fileName}.json");
            if (!File.Exists(path))
            {
                Debug.LogError($"JSON file not found at path: {path}");
                return null;
            }

            return File.ReadAllText(path);
        }
        
        private static string ReadScribanTemplate(string fileName)
        {
            string path = Path.Combine(Application.dataPath, "Bettr", "Editor", "scriban", $"{fileName}.template");
            if (!File.Exists(path))
            {
                Debug.LogError($"Scriban template file not found at path: {path}");
                return null;
            }

            return File.ReadAllText(path);
        }

        
        private static void ProcessSettings(string settingsName, string runtimeAssetPath)
        {
            SimpleStringInterpolator interpolator = new SimpleStringInterpolator();
            interpolator.SetVariable("settingsName", settingsName);
            
            string jsonTemplate = ReadJson("BaseGameSettings");
            string json = interpolator.Interpolate(jsonTemplate);

            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            List<IGameObject> runtimeObjects = hierarchyInstance.Child != null ? new List<IGameObject>() {hierarchyInstance.Child} : hierarchyInstance.Children.Cast<IGameObject>().ToList();
            List<IComponent> components = hierarchyInstance.Components.Cast<IComponent>().ToList();

            var settingsPrefab = ProcessPrefab(settingsName, 
                components, 
                runtimeObjects,
                runtimeAssetPath);
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
        
        private static GameObject ProcessPrefab(string prefabName, List<IComponent> components, List<IGameObject> gameObjects, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var prefabPath = $"{runtimeAssetPath}/Prefabs/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                prefab = new GameObject(prefabName);
                foreach (var component in components)
                {
                    component.AddComponent(prefab);
                }
                
                foreach (var go in gameObjects)
                {
                    go.SetParent(prefab);
                }
            
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            
            AssetDatabase.Refresh();
            
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return prefab;
        }

        private static TextAsset CreateOrLoadScript(string name, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var scriptName = $"{name}.cscript.txt";
            var scriptPath = $"{runtimeAssetPath}/Scripts/{scriptName}";
            var script = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath);
            if (script == null)
            {
                Debug.Log($"Creating script for {name} at {scriptPath}");
                try
                {
                    var defaultScriptContentPath = "Assets/Bettr/Editor/DefaultCScript.cscript.txt"; // Adjust the path as needed
                    var defaultScriptTemplateContent = File.ReadAllText(defaultScriptContentPath);
                    var defaultScriptContent = string.Format(defaultScriptTemplateContent, name);
                    File.WriteAllText(scriptPath, defaultScriptContent);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            script = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath);

            return script;
        }

        private static AnimatorController CreateOrLoadAnimatorController(string name, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var animatorControllerName = $"{name}_anims.controller";
            var animatorControllerPath = $"{runtimeAssetPath}/Animators/{animatorControllerName}";
            var animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
            if (animatorController == null)
            {
                Debug.Log($"Creating animator controller for {name} at {animatorControllerPath}");
                try
                {
                    animatorController = AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            animatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);

            return animatorController;
        }
        
        private static Material CreateOrLoadMaterial(string materialName, string shaderName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var materialFilename = $"{materialName}.mat";
            var materialFilepath = $"{runtimeAssetPath}/Materials/{materialFilename}";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            if (material == null)
            {
                Debug.Log($"Creating material for {materialName} at {materialFilepath}");
                try
                {
                    material = new Material(Shader.Find(shaderName));
                    AssetDatabase.CreateAsset(material, materialFilepath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);

            return material;
        }
        
        private static T GetTableValue<T>(Table table, string pk, string key)
        {
            var pkTable = table[pk] as Table;
            if (pkTable == null)
            {
                Debug.LogError($"Key {pk} not found in table.");
                return default(T);
            }

            var first = pkTable["First"] as Table;
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
            
            return default(T);
        }

        private static Table GetTable(string tableName)
        {
            var table = TileController.LuaScript.Globals[tableName] as Table;
            if (table == null)
            {
                Debug.LogError($"{tableName} not found in the machine model.");
            }
            return table;
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