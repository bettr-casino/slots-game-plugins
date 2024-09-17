using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using Bettr.Core;
using Bettr.Editor.generators;
using Bettr.Editor.generators.mechanics;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEditor;
using UnityEngine;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using DirectoryInfo = System.IO.DirectoryInfo;

using Scriban;
using Scriban.Runtime;
using Exception = System.Exception;

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
        private const string AssetsServerBaseURL = "https://bettr-casino-assets.s3.us-west-2.amazonaws.com";
        private const string OutcomesServerBaseURL = "https://bettr-casino-outcomes.s3.us-west-2.amazonaws.com";
        private const string GatewayUrl = "https://3wcgnl14qb.execute-api.us-west-2.amazonaws.com/gateway";
        
        [MenuItem("Bettr/Tools/Check Asset Bundle Load")]
        public static void CheckAssetBundleLoad()
        {
            string mainPath = "Assets/Bettr/LocalStore/AssetBundles/WebGL/main.v0_1_0";
            AssetBundle mainBundle = AssetBundle.LoadFromFile(mainPath);

            string bundle1Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001.epicancientadventures";
            AssetBundle bundle1 = AssetBundle.LoadFromFile(bundle1Path);
            if (bundle1 == null)
            {
                Debug.LogError($"Bundle 1 AssetBundle at {bundle1Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 1 loaded successfully.");
            }
            string bundle2Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001.epicatlantistreasures";
            AssetBundle bundle2 = AssetBundle.LoadFromFile(bundle2Path);
            if (bundle2 == null)
            {
                Debug.LogError($"Bundle 2 AssetBundle at {bundle2Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 2 loaded successfully.");
            }
            string bundle3Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001_scenes.epicancientadventures";
            AssetBundle bundle3 = AssetBundle.LoadFromFile(bundle3Path);
            if (bundle3 == null)
            {
                Debug.LogError($"Bundle 3 AssetBundle at {bundle3Path} failed to load.");
                return;
            }
            {
                Debug.Log("Bundle 3 loaded successfully.");
            }
            string bundle4Path = "Assets/Bettr/LocalStore/AssetBundles/WebGL/game001_scenes.epicatlantistreasures";
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

        [MenuItem("Bettr/Build/Assets")] 
        public static void BuildAssets()
        {
            BuildAssetBundles();
            BuildLocalServer();
            BuildLocalOutcomes();
        }

        [MenuItem("Bettr/Build/Game001 - Epic Wins")]
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
        
        [MenuItem("Bettr/Build/Game002 - Buffalo")]
        public static void BuildGame002()
        {
            BuildMachines("Game002", "BuffaloTreasureHunter");
        }

        [MenuItem("Bettr/Build/Game003 - HighStakes")]
        public static void BuildGame003()
        {
            BuildMachines("Game003", "HighStakesAlpineAdventure");
        }

        [MenuItem("Bettr/Build/Game004 - CleopatraRiches")]
        public static void BuildGame004()
        {
            BuildMachines("Game004", "CleopatraRiches");
        }

        [MenuItem("Bettr/Build/Game005 - 88FortunesDancingDrums")]
        public static void BuildGame005()
        {
            BuildMachines("Game005", "88FortunesDancingDrums");
        }

        [MenuItem("Bettr/Build/Game006 - WheelOfFortuneTripleExtremeSpin")]
        public static void BuildGame006()
        {
            BuildMachines("Game006", "WheelOfFortuneTripleExtremeSpin");
        }

        [MenuItem("Bettr/Build/Game007 - TrueVegas")]
        public static void BuildGame007()
        {
            BuildMachines("Game007", "TrueVegasInfiniteSpins");
        }

        [MenuItem("Bettr/Build/Game008 - GodsOfOlympusZeus")]
        public static void BuildGame008()
        {
            BuildMachines("Game008", "GodsOfOlympusZeus");
        }

        [MenuItem("Bettr/Build/Game009 - PlanetMooneyMooCash")]
        public static void BuildGame009()
        {
            BuildMachines("Game009", "PlanetMooneyMooCash");
        }
        
        private static void ImportFBX(string machineName, string machineVariant)
        {
            // // Copy across the background texture
            // string sourceTexturePath =  $"Assets/Bettr/Editor/textures/{machineName}/{machineVariant}/Background.jpg";
            // string destinationTexturePath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset/Textures/Background.jpg";
            // File.Copy(sourceTexturePath, destinationTexturePath, overwrite: true);
            // AssetDatabase.Refresh();

            
            string sourcePath =  $"Assets/Bettr/Editor/fbx/{machineName}/{machineVariant}/";
            string destinationPathPrefix = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset/";
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
            string machineName = GetArgument("-machineName");
            string machineVariant = GetArgument("-machineVariant");
            string machineModel = GetArgument("-machineModel");
            
            SetupCoreAssetPath();
            ClearRuntimeAssetPath(machineName, machineVariant);
            SetupMachine(machineName, machineVariant, machineModel);
            ImportFBX(machineName, machineVariant);
            BuildMachine(machineName, machineVariant);
        }
        
        private static void CreateOrReplaceMaterial(string machineName, string machineVariant)
        {
            string materialName = $"{machineName}__{machineVariant}__LobbyCard";
            string materialPath = $"Assets/Bettr/Runtime/Plugin/LobbyCard/variants/v0_1_0/Runtime/Asset/{machineName}/LobbyCard/Materials/{materialName}.mat";
            string texturePath = $"Assets/Bettr/Runtime/Plugin/LobbyCard/variants/v0_1_0/Runtime/Asset/{machineName}/LobbyCard/Materials/{materialName}.jpg";
    
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
            CreateOrReplaceMaterial("Game001", "AncientAdventures");
            CreateOrReplaceMaterial("Game001", "AtlantisTreasures");
            CreateOrReplaceMaterial("Game001", "ClockworkChronicles");
            CreateOrReplaceMaterial("Game001", "DragonsHoard");
            CreateOrReplaceMaterial("Game001", "EnchantedForest");
            CreateOrReplaceMaterial("Game001", "GalacticQuest");
            CreateOrReplaceMaterial("Game001", "GuardiansOfOlympus");
            CreateOrReplaceMaterial("Game001", "LostCityOfGold");
            CreateOrReplaceMaterial("Game001", "MysticalLegends");
            CreateOrReplaceMaterial("Game001", "PharosFortune");
            CreateOrReplaceMaterial("Game001", "PiratesPlunder");
            CreateOrReplaceMaterial("Game001", "SamuraisFortune");

            // Game002 Variants
            CreateOrReplaceMaterial("Game002", "BuffaloAdventureQuest");
            CreateOrReplaceMaterial("Game002", "BuffaloCanyonRiches");
            CreateOrReplaceMaterial("Game002", "BuffaloFrontierFortune");
            CreateOrReplaceMaterial("Game002", "BuffaloJackpotMadness");
            CreateOrReplaceMaterial("Game002", "BuffaloMagicSpins");
            CreateOrReplaceMaterial("Game002", "BuffaloMoonlitMagic");
            CreateOrReplaceMaterial("Game002", "BuffaloSafariExpedition");
            CreateOrReplaceMaterial("Game002", "BuffaloSpiritQuest");
            CreateOrReplaceMaterial("Game002", "BuffaloThunderstorm");
            CreateOrReplaceMaterial("Game002", "BuffaloTreasureHunter");
            CreateOrReplaceMaterial("Game002", "BuffaloWheelOfRiches");
            CreateOrReplaceMaterial("Game002", "BuffaloWildPicks");

            // Game003 Variants
            CreateOrReplaceMaterial("Game003", "HighStakesAlpineAdventure");
            CreateOrReplaceMaterial("Game003", "HighStakesCascadingCash");
            CreateOrReplaceMaterial("Game003", "HighStakesHotLinks");
            CreateOrReplaceMaterial("Game003", "HighStakesJungleQuest");
            CreateOrReplaceMaterial("Game003", "HighStakesMegaMultipliers");
            CreateOrReplaceMaterial("Game003", "HighStakesMonacoThrills");
            CreateOrReplaceMaterial("Game003", "HighStakesSafariAdventure");
            CreateOrReplaceMaterial("Game003", "HighStakesSpaceOdyssey");
            CreateOrReplaceMaterial("Game003", "HighStakesStackedSpins");
            CreateOrReplaceMaterial("Game003", "HighStakesUnderwaterAdventure");
            CreateOrReplaceMaterial("Game003", "HighStakesWildSpins");
            CreateOrReplaceMaterial("Game003", "HighStakesWonderWays");

            // Game004 Variants
            CreateOrReplaceMaterial("Game004", "RichesBeverlyHillMansions");
            CreateOrReplaceMaterial("Game004", "RichesBillionaireBets");
            CreateOrReplaceMaterial("Game004", "RichesDiamondDash");
            CreateOrReplaceMaterial("Game004", "RichesGalacticGoldRush");
            CreateOrReplaceMaterial("Game004", "RichesJetsetJackpot");
            CreateOrReplaceMaterial("Game004", "RichesMysticForest");
            CreateOrReplaceMaterial("Game004", "RichesPharaohsRiches");
            CreateOrReplaceMaterial("Game004", "RichesPiratesBounty");
            CreateOrReplaceMaterial("Game004", "RichesRaceToRiches");
            CreateOrReplaceMaterial("Game004", "RichesRoyalHeist");
            CreateOrReplaceMaterial("Game004", "RichesRubyRush");
            CreateOrReplaceMaterial("Game004", "RichesSapphireSprint");

            // Game005 Variants
            CreateOrReplaceMaterial("Game005", "FortunesCelestialFortune");
            CreateOrReplaceMaterial("Game005", "FortunesFortuneTeller");
            CreateOrReplaceMaterial("Game005", "FortunesFourLeafClover");
            CreateOrReplaceMaterial("Game005", "FortunesJadeOfFortune");
            CreateOrReplaceMaterial("Game005", "FortunesLuckyBamboo");
            CreateOrReplaceMaterial("Game005", "FortunesLuckyCharms");
            CreateOrReplaceMaterial("Game005", "FortunesManekiNeko");
            CreateOrReplaceMaterial("Game005", "FortunesMysticForest");
            CreateOrReplaceMaterial("Game005", "FortunesNorseAcorns");
            CreateOrReplaceMaterial("Game005", "FortunesPharaohsRiches");
            CreateOrReplaceMaterial("Game005", "FortunesShootingStars");
            CreateOrReplaceMaterial("Game005", "FortunesVikingVoyage");

            // Game006 Variants
            CreateOrReplaceMaterial("Game006", "WheelsAncientKingdom");
            CreateOrReplaceMaterial("Game006", "WheelsCapitalCityTycoon");
            CreateOrReplaceMaterial("Game006", "WheelsEmpireBuilder");
            CreateOrReplaceMaterial("Game006", "WheelsFantasyKingdom");
            CreateOrReplaceMaterial("Game006", "WheelsGlobalInvestor");
            CreateOrReplaceMaterial("Game006", "WheelsIndustrialRevolution");
            CreateOrReplaceMaterial("Game006", "WheelsJurassicJungle");
            CreateOrReplaceMaterial("Game006", "WheelsMythicalRealm");
            CreateOrReplaceMaterial("Game006", "WheelsRealEstateMoghul");
            CreateOrReplaceMaterial("Game006", "WheelsSpaceColonization");
            CreateOrReplaceMaterial("Game006", "WheelsTreasuresIslandTycoon");
            CreateOrReplaceMaterial("Game006", "WheelsUnderwaterEmpire");

            // Game007 Variants
            CreateOrReplaceMaterial("Game007", "TrueVegasDiamondDazzle");
            CreateOrReplaceMaterial("Game007", "TrueVegasGoldRush");
            CreateOrReplaceMaterial("Game007", "TrueVegasInfiniteSpins");
            CreateOrReplaceMaterial("Game007", "TrueVegasLucky7s");
            CreateOrReplaceMaterial("Game007", "TrueVegasLuckyCharms");
            CreateOrReplaceMaterial("Game007", "TrueVegasMegaJackpot");
            CreateOrReplaceMaterial("Game007", "TrueVegasMegaWheels");
            CreateOrReplaceMaterial("Game007", "TrueVegasRubyRiches");
            CreateOrReplaceMaterial("Game007", "TrueVegasSuper7s");
            CreateOrReplaceMaterial("Game007", "TrueVegasTripleSpins");
            CreateOrReplaceMaterial("Game007", "TrueVegasWheelBonanza");
            CreateOrReplaceMaterial("Game007", "TrueVegasWildCherries");

            // Game008 Variants
            CreateOrReplaceMaterial("Game008", "GodsAncientEgyptian");
            CreateOrReplaceMaterial("Game008", "GodsCelestialBeasts");
            CreateOrReplaceMaterial("Game008", "GodsCelestialGuardians");
            CreateOrReplaceMaterial("Game008", "GodsDivineRiches");
            CreateOrReplaceMaterial("Game008", "GodsElementalMasters");
            CreateOrReplaceMaterial("Game008", "GodsEternalDivinity");
            CreateOrReplaceMaterial("Game008", "GodsHeavenlyMonarchs");
            CreateOrReplaceMaterial("Game008", "GodsMysticPantheon");
            CreateOrReplaceMaterial("Game008", "GodsMythicDeities");
            CreateOrReplaceMaterial("Game008", "GodsNorseLegends");
            CreateOrReplaceMaterial("Game008", "GodsSacredLegends");
            CreateOrReplaceMaterial("Game008", "GodsTitansOfWealth");

            // Game009 Variants
            CreateOrReplaceMaterial("Game009", "SpaceInvadersApolloAdventures");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersAsteroidMiners");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersBlackHoleExplorers");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersCosmicRaiders");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersGalacticPioneers");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersInterstellarTreasureHunters");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersNebulaNavigators");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersQuantumExplorers");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersRaidersOfPlanetMooney");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersStarshipSalvagers");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersStellarExpedition");
            CreateOrReplaceMaterial("Game009", "SpaceInvadersVoyagersOfTheCosmos");
            
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

                                            // Sort scripts second
                                            if (assetStr.EndsWith(".cs", StringComparison.OrdinalIgnoreCase) || assetStr.EndsWith(".js", StringComparison.OrdinalIgnoreCase))
                                                return 1;

                                            // Sort materials third
                                            if (assetStr.EndsWith(".mat", StringComparison.OrdinalIgnoreCase))
                                                return 2;

                                            // Everything else last
                                            return 3;
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
        
        private static void BuildLocalOutcomes()
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

        private static void ClearRuntimeAssetPath(string machineName, string machineVariant)
        {
            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset";
            
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
        
        private static void SetupMachine(string machineName, string machineVariant, string machineModel)
        {
            string machineModelName = Path.GetFileNameWithoutExtension(machineModel);

            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset";
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
            
            ProcessScripts(machineName, machineVariant, runtimeAssetPath);
        }

        private static void BuildMachine(string machineName, string machineVariant)
        {
            string runtimeAssetPath = $"Assets/Bettr/Runtime/Plugin/{machineName}/variants/{machineVariant}/Runtime/Asset";
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

        private static void CopyScripts(string scriptsPath, string[] filePaths, string machineName, string machineVariant, string runtimeAssetPath)
        {
            var mechanicsTable = BettrMenu.GetTable($"{machineName}Mechanics");
            var baseGameMechanics = BettrMenu.GetTableArray<string>(mechanicsTable, "BaseGame", "Mechanic");
            // convert baseGameMechanics array to PascalCase using ConvertCamelToPascalCase
            baseGameMechanics = new List<string>(baseGameMechanics.Select(ConvertCamelToPascalCase).ToArray());
            
            foreach (string filePath in filePaths)
            {
                var reelCount = BettrMenu.GetReelCount(machineName);
                var scribanTemplate = ParseScribanTemplate(scriptsPath, filePath);
                var model = new Dictionary<string, object>
                {
                    { "machineName", machineName },
                    { "machineVariant", machineVariant },
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

        private static void ProcessScripts(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            string dirPath = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", "scripts");
            string[] filePaths = Directory.GetFiles(dirPath, "*.cscript.txt.template");
            string scriptsPath = $"scripts";
            CopyScripts(scriptsPath, filePaths, machineName, machineVariant, runtimeAssetPath);
            
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
                    CopyScripts(scriptsPath, filePaths, machineName, machineVariant, runtimeAssetPath);
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
            
            var maxOffsetY = BettrMenu.GetReelMaxOffsetY(machineName);
            
            var reelBackgroundX = BettrMenu.GetReelBackgroundX(machineName);
            var reelBackgroundY = BettrMenu.GetReelBackgroundY(machineName);
            
            var reelBackgroundScaleX = BettrMenu.GetReelBackgroundScaleX(machineName);
            var reelBackgroundScaleY = BettrMenu.GetReelBackgroundScaleY(machineName);
            
            var reelMaskUpperX = BettrMenu.GetReelMaskUpperX(machineName);
            var reelMaskUpperY = BettrMenu.GetReelMaskUpperY(machineName);
            
            var reelMaskLowerX = BettrMenu.GetReelMaskLowerX(machineName);
            var reelMaskLowerY = BettrMenu.GetReelMaskLowerY(machineName);
            
            var reelMaskScaleX = BettrMenu.GetReelMaskScaleX(machineName);
            var reelMaskScaleY = BettrMenu.GetReelMaskScaleY(machineName);

            var scaleX = 0.90;
            var scaleY = 0.90;
            
            var templateName = "BaseGameMachine";
            var scribanTemplate = ParseScribanTemplate($"common", templateName);
            
            var baseGameSymbolTable = GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();

            var reelCount = BettrMenu.GetReelCount(machineName);
            var horizontalReelPositions = BettrMenu.GetReelHorizontalPositions(machineName);
            
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
            var reelCount = BettrMenu.GetReelCount(machineName);
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

            var symbolKeys = BettrMenu.GetSymbolKeys(machineName);
            
            var symbolCount = BettrMenu.GetSymbolCount(machineName, reelIndex);
            var symbolIndexes = Enumerable.Range(1, symbolCount).ToList();
            
            var symbolPositions = BettrMenu.GetSymbolPositions(machineName, reelIndex);
            var symbolVerticalSpacing = BettrMenu.GetSymbolVerticalSpacing(machineName, reelIndex);
            var yPositions = symbolPositions.Select(pos => pos * symbolVerticalSpacing).ToList();

            yPositions.Insert(0, 0);
            
            var symbolScaleX = BettrMenu.GetSymbolScaleX(machineName, reelIndex);
            var symbolScaleY = BettrMenu.GetSymbolScaleY(machineName, reelIndex);
            
            var symbolOffsetY = BettrMenu.GetSymbolOffsetY(machineName, reelIndex);

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
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
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
            var baseGameReelStateTable = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var reelCount = BettrMenu.GetReelCount(machineName);
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableValue<float>(baseGameReelStateTable, $"Reel{reelId}", "HorizontalPosition");
                reelHorizontalPositions.Add(value);
            }
            
            return reelHorizontalPositions;
        }
        
        public static float GetReelMaxOffsetY(string machineName)
        {
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameLayout");
            var value = BettrMenu.GetTableValue<float>(baseGameLayoutTable, "ReelMaxOffsetY", "Value");
            return value;
        }
        
        public static List<float> GetReelMaskUpperX(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskUpperX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskUpperY(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskUpperY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskLowerX(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskLowerX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskLowerY(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskLowerY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskScaleX(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskScaleX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelMaskScaleY(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelMaskScaleY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundX(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundY(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundY");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundScaleX(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundScaleX");
                values.Add(value);
            }
            return values;
        }
        
        public static List<float> GetReelBackgroundScaleY(string machineName)
        {
            var values = new List<float>();
            var reelCount = BettrMenu.GetReelCount(machineName);
            var baseGameLayoutTable = BettrMenu.GetTable($"{machineName}BaseGameReelsLayout");
            for (int reelId = 1; reelId <= reelCount; reelId++)
            {
                var value = BettrMenu.GetTableKeyValue<float>(baseGameLayoutTable, $"Reel{reelId}", "ReelBackgroundScaleY");
                values.Add(value);
            }
            return values;
        }

        public static List<string> GetSymbolKeys(string machineName)
        {
            var baseGameSymbolTable = BettrMenu.GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            return symbolKeys;
        }
        
        public static List<int> GetSymbolPositions(string machineName, int reelIndex)
        {
            var reelSymbolStates = BettrMenu.GetTable($"{machineName}BaseGameReelSymbolsState");
            var symbolPositions = BettrMenu.GetTableArray<double>(reelSymbolStates, $"Reel{reelIndex}", "SymbolPosition");
            return symbolPositions.Select(d => (int)d).ToList();
        }
        
        public static int GetSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var topSymbolCount = BettrMenu.GetTableValue<int>(reelStates, $"Reel{reelIndex}", "SymbolCount");
            return topSymbolCount;
        }

        public static int GetTopSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var topSymbolCount = BettrMenu.GetTableValue<int>(reelStates, $"Reel{reelIndex}", "TopSymbolCount");
            return topSymbolCount;
        }
        
        public static int GetVisibleSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var visibleSymbolCount = BettrMenu.GetTableValue<int>(reelStates, $"Reel{reelIndex}", "VisibleSymbolCount");
            return visibleSymbolCount;
        }
        
        public static int GetBottomSymbolCount(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var bottomSymbolCount = BettrMenu.GetTableValue<int>(reelStates, $"Reel{reelIndex}", "BottomSymbolCount");
            return bottomSymbolCount;
        }
        
        public static float GetSymbolVerticalSpacing(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = BettrMenu.GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolVerticalSpacing");
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolOffsetY(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = BettrMenu.GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolOffsetY");
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolScaleX(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = BettrMenu.GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolScaleX", 1);
            return symbolVerticalSpacing;
        }
        
        public static float GetSymbolScaleY(string machineName, int reelIndex)
        {
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var symbolVerticalSpacing = BettrMenu.GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolScaleY", 1);
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