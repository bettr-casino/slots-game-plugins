using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Bettr.Editor.generators;
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
using Scriban.Parsing;
using Scriban.Runtime;
using TMPro;
using UnityEngine.Rendering;
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
        
        [MenuItem("Bettr/PlayMode/Start")] 
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

        [MenuItem("Bettr/Build/Assets")] 
        public static void BuildAssets()
        {
            BuildAssetBundles();
            BuildLocalServer();
        }

        [MenuItem("Bettr/Build/Game001 - Epic Wins!!!")]
        public static void BuildGame001()
        {
            BuildMachines("Game001", "AncientAdventures");
            // BuildMachines("Game001", "AtlantisTreasures");
            // BuildMachines("Game001", "ClockworkChronicles");
            // BuildMachines("Game001", "CosmicVoyage");
            // BuildMachines("Game001", "DragonsHoard");
            // BuildMachines("Game001", "EnchantedForest");
            // BuildMachines("Game001", "GalacticQuest");
            // BuildMachines("Game001", "GuardiansOfOlympus");
            // BuildMachines("Game001", "LostCityOfGold");
            // BuildMachines("Game001", "MysticalLegends");
            // BuildMachines("Game001", "PharosFortune");
            // BuildMachines("Game001", "PiratesPlunder");
            // BuildMachines("Game001", "SamuraisFortune");
        }
        
        [MenuItem("Bettr/Build/Game002")]
        public static void BuildGame002()
        {
            BuildMachines("Game002", "BuffaloTreasureHunter");
        }

        [MenuItem("Bettr/Build/Game003 - LightningLinkHighStakes")]
        public static void BuildGame003()
        {
            BuildMachines("Game003", "LightningLinkHighStakes");
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

        [MenuItem("Bettr/Build/Game007 - DoubleDiamondVegas")]
        public static void BuildGame007()
        {
            BuildMachines("Game007", "DoubleDiamondVegas");
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
            Environment.SetEnvironmentVariable("machineModel", $"{modelsDir}/{machineName}/{machineName}Models.lua");

            BuildMachinesFromCommandLine();
        }

        // ReSharper disable once MemberCanBePrivate.Global
        public static void BuildMachinesFromCommandLine()
        {
            string machineName = GetArgument("-machineName");
            string machineVariant = GetArgument("-machineVariant");
            string machineModel = GetArgument("-machineModel");
            
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

            var sharedAssetBundleOptions = BuildAssetBundleOptions.StrictMode |
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

            string[] subDirectories = { "Animators", "Materials", "Models", "FBX", "Prefabs", "Scenes", "Scripts", "Shaders", "Textures" };
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
            
            string dirPath = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", "scripts", machineName);
            string[] filePaths = Directory.GetFiles(dirPath, "*.cscript.txt.template");
            string scriptsPath = $"scripts/{machineName}";
            CopyScripts(scriptsPath, filePaths, machineName, machineVariant, runtimeAssetPath);
            
            // Process variant scripts
            dirPath = Path.Combine(Application.dataPath, "Bettr", "Editor", "templates", "integration", machineName, machineVariant);
            filePaths = Directory.GetFiles(dirPath, "*.cscript.txt.template");
            scriptsPath = $"integration/{machineName}/{machineVariant}";
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
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);
            
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
            var symbolGroup = ProcessPrefab(scriptGroupName, 
                hierarchyInstance, 
                runtimeAssetPath);
            
            return symbolGroup;
        }

        private static GameObject ProcessBaseGameSymbol(string machineName, string machineVariant, string symbolName, string symbolPrefabName, string runtimeAssetPath)
        {
            var templateName = "BaseGameSymbol";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);
            
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

            var settingsPrefab = ProcessPrefab(symbolPrefabName, 
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
            
            var templateName = "BaseGameMachine";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);
            
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
            
            var baseGameMachinePrefab = ProcessPrefab($"{baseGameMachine}", 
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
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
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

            var reelPrefab = ProcessPrefab(reelName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessUICamera(string cameraName, bool includeAudioListener, string runtimeAssetPath)
        {
            ProcessPrefab(cameraName, new List<IComponent>
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
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

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

            ProcessPrefab(backgroundName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessFreeSpinsMachine(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string baseGameMachine = $"{machineName}FreeSpinsMachine";
            
            string baseGameSettings = $"{machineName}FreeSpinsSettings";
            
            var baseGameSymbolTable = GetTable($"{machineName}FreeSpinsSymbolTable");
            
            var scriptName = $"{machineName}FreeSpinsReel";   
            BettrScriptGenerator.CreateOrLoadScript(scriptName, runtimeAssetPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            var reelStates = GetTable($"{machineName}BaseGameReelState");
            var reelCount = BettrMenu.GetReelCount(machineName);
            
            var reelHPositions = new List<float>();
            var reelMaskUpperYs = new List<float>();
            var reelMaskLowerYs = new List<float>();
            var reelMaskScaleYs = new List<float>();
            var reelBackgroundYs = new List<float>();
            var reelBackgroundScaleYs = new List<float>();
            
            float maxOffsetY = 0.0f;
            
            for (var reelIndex = 1; reelIndex <= reelCount; reelIndex++)
            {
                var topSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "TopSymbolCount");
                var visibleSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "VisibleSymbolCount");
                var bottomSymbolCount = GetTableValue<int>(reelStates, $"Reel{reelIndex}", "BottomSymbolCount");
                var symbolVerticalSpacing = GetTableValue<float>(reelStates, $"Reel{reelIndex}", "SymbolVerticalSpacing");
                var zeroVisibleSymbolIndex = visibleSymbolCount % 2 == 0 ? visibleSymbolCount / 2 + 1 : (visibleSymbolCount - 1) / 2 + 1;
                var reelMaskUpperY = visibleSymbolCount % 2 == 0? (zeroVisibleSymbolIndex) * symbolVerticalSpacing : (zeroVisibleSymbolIndex + 1) * symbolVerticalSpacing;
                var reelMaskLowerY = -(zeroVisibleSymbolIndex + 1) * symbolVerticalSpacing;
                var reelMaskScaleY = (topSymbolCount + 1) * symbolVerticalSpacing;
                var reelBackgroundY = visibleSymbolCount % 2 == 0 ? -symbolVerticalSpacing/2 : 0;
                var reelBackgroundScaleY = (visibleSymbolCount) * symbolVerticalSpacing;
                reelMaskUpperYs.Add(reelMaskUpperY);
                reelMaskLowerYs.Add(reelMaskLowerY);
                reelMaskScaleYs.Add(reelMaskScaleY);
                reelBackgroundYs.Add(reelBackgroundY);
                reelBackgroundScaleYs.Add(reelBackgroundScaleY);

                var offsetY = (visibleSymbolCount % 3) * symbolVerticalSpacing; 
                maxOffsetY = Mathf.Max(maxOffsetY, offsetY);
            }
            
            var templateName = "BaseGameMachine";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);
            
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "baseGameMachine", baseGameMachine },
                { "baseGameSettings", baseGameSettings },
                { "symbolKeys", symbolKeys},
                { "reelMaskUpperY", reelMaskUpperYs[0]},
                { "reelMaskLowerY", reelMaskLowerYs[0]},
                { "reelMaskScaleY", reelMaskScaleYs[0]},
                { "reelBackgroundY", reelBackgroundYs[0]},
                { "reelBackgroundScaleY", reelBackgroundScaleYs[0]},
                { "offsetY", maxOffsetY },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"BaseGameMachine: {json}");
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            
            var baseGameMachinePrefab = ProcessPrefab($"{baseGameMachine}", 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessFreeSpinsBackground(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string backgroundName = $"{machineName}FreeSpinsBackground";
            
            var backgroundScriptName = $"{machineName}FreeSpinsBackground";   
            BettrScriptGenerator.CreateOrLoadScript(backgroundScriptName, runtimeAssetPath);
            
            string templateName = "FreeSpinsBackground";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

            if (scribanTemplate.HasErrors)
            {
                Debug.LogError($"Scriban template has errors: {scribanTemplate.Messages} template: {templateName}");
                throw new Exception($"Scriban template has errors: {scribanTemplate.Messages} template: {{templateName}}");
            }
            
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

            var settingsPrefab = ProcessPrefab(backgroundName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessWheelGameMachine(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string wheelGameMachine = $"{machineName}WheelGameMachine";
            
            var scriptName = $"{machineName}WheelGameReel";   
            BettrScriptGenerator.CreateOrLoadScript(scriptName, runtimeAssetPath);
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            var templateName = "WheelGameMachine";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "wheelGameMachine", wheelGameMachine },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log($"WheelGameMachine: {json}");
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            
            var baseGameMachinePrefab = ProcessPrefab($"{wheelGameMachine}", 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessWheelGameBackground(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string backgroundName = $"{machineName}WheelGameBackground";
            
            var backgroundScriptName = $"{machineName}WheelGameBackground";   
            BettrScriptGenerator.CreateOrLoadScript(backgroundScriptName, runtimeAssetPath);
            
            string templateName = "WheelGameBackground";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

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

            var settingsPrefab = ProcessPrefab(backgroundName, 
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
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

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
                            switch (baseGameMechanic)
                            {
                                case "ways":
                                    BaseGameWaysMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
                                default:
                                    BaseGamePaylinesMechanic.Process(machineName, machineVariant, runtimeAssetPath);
                                    break;
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
        
        private static void ProcessBaseGameScatterBonusFreeSpinsMechanic(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var reelCount = BettrMenu.GetReelCount(machineName);
            
            var scatterSymbolIndexesByReel = new Dictionary<string, List<int>>();
            for (var reelIndex = 1; reelIndex <= reelCount; reelIndex++)
            {
                var topSymbolCount = BettrMenu.GetTopSymbolCount(machineName, reelIndex);
                var visibleSymbolCount = BettrMenu.GetVisibleSymbolCount(machineName, reelIndex);
                
                var scatterSymbolIndexes = new List<int>();
                for (int symbolIndex = topSymbolCount + 1;
                     symbolIndex <= topSymbolCount + visibleSymbolCount;
                     symbolIndex++)
                {
                    scatterSymbolIndexes.Add(symbolIndex);
                }
                
                scatterSymbolIndexesByReel.Add($"{reelIndex}", scatterSymbolIndexes);
            }
            
            string templateName = "BaseGameScatterBonusFreeSpinsMechanic";
            var scribanTemplate = ParseScribanTemplate("mechanics/scatterbonusfreespins", templateName);

            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "reelCount", reelCount },
                { "scatterSymbolIndexesByReel", scatterSymbolIndexesByReel },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log(json);
            
            Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
            if (mechanic == null)
            {
                throw new Exception($"Failed to deserialize mechanic from json: {json}");
            }
            
            foreach (var mechanicAnimation in mechanic.Animations)
            {
                BettrAnimatorController.AddAnimationState(mechanicAnimation.Filename, mechanicAnimation.AnimationStates, mechanicAnimation.AnimatorTransitions, runtimeAssetPath);
            }
            
            foreach (var tilePropertyAnimator in mechanic.TilePropertyAnimators)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{tilePropertyAnimator.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, tilePropertyAnimator.PrefabName);
                if (tilePropertyAnimator.PrefabIds != null)
                {
                    foreach (var prefabId in tilePropertyAnimator.PrefabIds)
                    {
                        var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                        InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                    }
                }
                if (tilePropertyAnimator.AnimatorsProperty != null)
                {
                    var properties = new List<TilePropertyAnimator>();
                    foreach (var animatorProperty in tilePropertyAnimator.AnimatorsProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(animatorProperty.Id, out var referenceGameObject);
                        var tileProperty = new TilePropertyAnimator()
                        {
                            key = animatorProperty.Key,
                            value = new PropertyAnimator()
                            {
                                animator = referenceGameObject?.Animator, 
                                animationStateName = animatorProperty.State,
                                delayBeforeAnimationStart = animatorProperty.DelayBeforeStart,
                                waitForAnimationComplete = animatorProperty.WaitForComplete,
                                overrideAnimationDuration = animatorProperty.OverrideDuration,
                                animationDuration = animatorProperty.AnimationDuration,
                            },
                        };
                        if (tileProperty.value.animator == null)
                        {
                            Debug.LogError($"Failed to find animator with id: {animatorProperty.Id}");
                        }
                        properties.Add(tileProperty);                        
                    }
                    var groupProperties = new List<TilePropertyAnimatorGroup>();
                    foreach (var animatorGroupProperty in tilePropertyAnimator.AnimatorsGroupProperty)
                    {
                        List<TilePropertyAnimator> animatorProperties = new List<TilePropertyAnimator>();
                        foreach (var property in animatorGroupProperty.Group)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                            var tileProperty = new TilePropertyAnimator()
                            {
                                key = property.Key,
                                value = new PropertyAnimator() {
                                    animator = referenceGameObject?.Animator, 
                                    animationStateName = property.State,
                                    delayBeforeAnimationStart = property.DelayBeforeStart,
                                    waitForAnimationComplete = property.WaitForComplete,
                                    overrideAnimationDuration = property.OverrideDuration,
                                    animationDuration = property.AnimationDuration,
                                },
                            };
                            if (tileProperty.value.animator == null)
                            {
                                Debug.LogError($"Failed to find animator with id: {property.Id}");
                            }
                            animatorProperties.Add(tileProperty);
                        }
                        groupProperties.Add(new TilePropertyAnimatorGroup()
                        {
                            groupKey = animatorGroupProperty.GroupKey,
                            tileAnimatorProperties = animatorProperties,
                        });
                    }
                    var component = prefabGameObject.GameObject.GetComponent<TilePropertyAnimators>();
                    component.tileAnimatorProperties.AddRange(properties);
                    component.tileAnimatorGroupProperties.AddRange(groupProperties);
                }
                
                PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
            }
            
            // save the changes
            AssetDatabase.SaveAssets();            
            
            // Modified Prefabs
            foreach (var modifiedPrefab in mechanic.Prefabs)
            {
                AssetDatabase.Refresh();
                
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{modifiedPrefab.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, modifiedPrefab.PrefabName);
                if (modifiedPrefab.PrefabIds != null)
                {
                    foreach (var prefabId in modifiedPrefab.PrefabIds)
                    {
                        var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                        InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                    }
                }

                var parentGameObject = InstanceGameObject.IdGameObjects[modifiedPrefab.ParentId];
                
                modifiedPrefab.SetParent(parentGameObject.GameObject);
                
                modifiedPrefab.OnDeserialized(new StreamingContext());
                
                PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
            }
            
            AssetDatabase.Refresh();

            // Anticipation animation
            foreach (var mechanicParticleSystem in mechanic.ParticleSystems)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{mechanicParticleSystem.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, mechanicParticleSystem.PrefabName);
                if (mechanicParticleSystem.PrefabIds != null)
                {
                    foreach (var prefabId in mechanicParticleSystem.PrefabIds)
                    {
                        var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                        InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                    }
                }
                
                var referenceGameObject = InstanceGameObject.IdGameObjects[mechanicParticleSystem.ReferenceId];
                
                // Create the particle system
                var particleSystem = BettrParticleSystem.AddOrGetParticleSystem(referenceGameObject.GameObject);
                var mainModule = particleSystem.main;
                var emissionModule = particleSystem.emission;
                var shapeModule = particleSystem.shape;
                var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();

                mainModule.playOnAwake = mechanicParticleSystem.ModuleData.PlayOnAwake;
                mainModule.startLifetime = mechanicParticleSystem.ModuleData.StartLifetime;
                mainModule.startSpeed = mechanicParticleSystem.ModuleData.StartSpeed;
                mainModule.startSize = mechanicParticleSystem.ModuleData.StartSize;
                mainModule.startColor = new ParticleSystem.MinMaxGradient(mechanicParticleSystem.ModuleData.GetStartColor());
                mainModule.gravityModifier = mechanicParticleSystem.ModuleData.GravityModifier;
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.SimulationSpace, out ParticleSystemSimulationSpace simulationSpace))
                {
                    mainModule.simulationSpace = simulationSpace;
                }
                mainModule.loop = mechanicParticleSystem.ModuleData.Looping;
                mainModule.duration = mechanicParticleSystem.ModuleData.Duration;
                mainModule.startRotation = mechanicParticleSystem.ModuleData.StartRotation;
                mainModule.startDelay = mechanicParticleSystem.ModuleData.StartDelay;
                mainModule.prewarm = mechanicParticleSystem.ModuleData.Prewarm;
                mainModule.maxParticles = mechanicParticleSystem.ModuleData.MaxParticles;

                // Emission module settings
                emissionModule.rateOverTime = mechanicParticleSystem.ModuleData.EmissionRateOverTime;
                emissionModule.rateOverDistance = mechanicParticleSystem.ModuleData.EmissionRateOverDistance;
                emissionModule.burstCount = mechanicParticleSystem.ModuleData.Bursts.Count;
                for (int i = 0; i < mechanicParticleSystem.ModuleData.Bursts.Count; i++)
                {
                    var burst = mechanicParticleSystem.ModuleData.Bursts[i];
                    emissionModule.SetBurst(i, new ParticleSystem.Burst(burst.Time, burst.MinCount, burst.MaxCount, burst.Cycles, burst.Interval) { probability = burst.Probability });
                }

                // Shape module settings
                shapeModule.shapeType = (ParticleSystemShapeType)Enum.Parse(typeof(ParticleSystemShapeType), mechanicParticleSystem.ModuleData.Shape);
                shapeModule.angle = mechanicParticleSystem.ModuleData.ShapeAngle;
                shapeModule.radius = mechanicParticleSystem.ModuleData.ShapeRadius;
                shapeModule.radiusThickness = mechanicParticleSystem.ModuleData.ShapeRadiusThickness;
                shapeModule.arc = mechanicParticleSystem.ModuleData.ShapeArc;
                
                // Set shape mode if applicable
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.ShapeArcMode, out ParticleSystemShapeMultiModeValue shapeMode))
                {
                    shapeModule.arcMode = shapeMode;
                }
                
                shapeModule.arcSpread = mechanicParticleSystem.ModuleData.ShapeSpread;
                shapeModule.arcSpeed = mechanicParticleSystem.ModuleData.ShapeArcSpeed; // Set arc speed
                shapeModule.position = mechanicParticleSystem.ModuleData.ShapePosition;
                shapeModule.rotation = mechanicParticleSystem.ModuleData.ShapeRotation;
                shapeModule.scale = mechanicParticleSystem.ModuleData.ShapeScale;

                // Renderer module settings
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.RendererSettings.RenderMode, out ParticleSystemRenderMode renderMode))
                {
                    renderer.renderMode = renderMode;
                }
                renderer.normalDirection = mechanicParticleSystem.ModuleData.RendererSettings.NormalDirection;
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.RendererSettings.SortMode, out ParticleSystemSortMode sortMode))
                {
                    renderer.sortMode = sortMode;
                }
                renderer.minParticleSize = mechanicParticleSystem.ModuleData.RendererSettings.MinParticleSize;
                renderer.maxParticleSize = mechanicParticleSystem.ModuleData.RendererSettings.MaxParticleSize;
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.RendererSettings.RenderAlignment, out ParticleSystemRenderSpace renderAlignment))
                {
                    renderer.alignment = renderAlignment;
                }
                renderer.flip = new Vector3(mechanicParticleSystem.ModuleData.RendererSettings.FlipX ? 1 : 0, mechanicParticleSystem.ModuleData.RendererSettings.FlipY ? 1 : 0, 0);
                renderer.pivot = mechanicParticleSystem.ModuleData.RendererSettings.Pivot;
                renderer.allowRoll = mechanicParticleSystem.ModuleData.RendererSettings.AllowRoll;
                renderer.receiveShadows = mechanicParticleSystem.ModuleData.RendererSettings.ReceiveShadows;
                renderer.shadowCastingMode = mechanicParticleSystem.ModuleData.RendererSettings.CastShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                if (Enum.TryParse(mechanicParticleSystem.ModuleData.RendererSettings.LightProbes, out LightProbeUsage lightProbeUsage))
                {
                    renderer.lightProbeUsage = lightProbeUsage;
                }

                renderer.sortingOrder = mechanicParticleSystem.ModuleData.RendererSettings.SortingOrder;
                renderer.sortingLayerName = mechanicParticleSystem.ModuleData.RendererSettings.SortingLayer;
                
                // Check if material properties are provided before generating the material
                Material material = null;
                if (!string.IsNullOrEmpty(mechanicParticleSystem.ModuleData.RendererSettings.Material) &&
                    !string.IsNullOrEmpty(mechanicParticleSystem.ModuleData.RendererSettings.Shader))
                {
                    material = BettrMaterialGenerator.CreateOrLoadMaterial(
                        mechanicParticleSystem.ModuleData.RendererSettings.Material,
                        mechanicParticleSystem.ModuleData.RendererSettings.Shader,
                        mechanicParticleSystem.ModuleData.RendererSettings.Texture,
                        mechanicParticleSystem.ModuleData.RendererSettings.Color,
                        mechanicParticleSystem.ModuleData.RendererSettings.Alpha,
                        runtimeAssetPath
                    );
                }

                // Set the material to the renderer
                if (material != null)
                {
                    renderer.material = material;
                }

                renderer.sortingOrder = mechanicParticleSystem.ModuleData.RendererSettings.SortingOrder;

                // Save changes to the prefab
                PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
            }
            
            // save the changes
            AssetDatabase.SaveAssets();
            
            AssetDatabase.Refresh();
            
            foreach (var tilePropertyParticleSystem in mechanic.TilePropertyParticleSystems)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{tilePropertyParticleSystem.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, tilePropertyParticleSystem.PrefabName);
                if (tilePropertyParticleSystem.PrefabIds != null)
                {
                    foreach (var prefabId in tilePropertyParticleSystem.PrefabIds)
                    {
                        var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                        InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                    }
                }
                if (tilePropertyParticleSystem.ParticleSystemsProperty != null)
                {
                    var properties = new List<TilePropertyParticleSystem>();
                    var groupProperties = new List<TilePropertyParticleSystemGroup>();
                    foreach (var particleSystemProperty in tilePropertyParticleSystem.ParticleSystemsProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(particleSystemProperty.Id, out var referenceGameObject);
                        var tileProperty = new TilePropertyParticleSystem()
                        {
                            key = particleSystemProperty.Key,
                            value = new PropertyParticleSystem()
                            {
                                particleSystem = referenceGameObject?.ParticleSystem, 
                                particleSystemDuration = particleSystemProperty.Duration,
                                delayBeforeParticleSystemStart = particleSystemProperty.DelayBeforeStart,
                                waitForParticleSystemComplete = particleSystemProperty.WaitForComplete,
                            },
                        };
                        if (tileProperty.value.particleSystem == null)
                        {
                            Debug.LogError($"Failed to find particleSystem with id: {particleSystemProperty.Id}");
                        }
                        properties.Add(tileProperty);                        
                    }
                    var component = prefabGameObject.GameObject.GetComponent<TilePropertyParticleSystems>();
                    if (component == null)
                    {
                        component = prefabGameObject.GameObject.AddComponent<TilePropertyParticleSystems>();
                        component.tileParticleSystemProperties = new List<TilePropertyParticleSystem>();
                        component.tileParticleSystemGroupProperties = new List<TilePropertyParticleSystemGroup>();
                    }
                    component.tileParticleSystemProperties.AddRange(properties);
                    component.tileParticleSystemGroupProperties.AddRange(groupProperties);
                }
                
                PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
            }
            
            // save the changes
            AssetDatabase.SaveAssets();
            
            AssetDatabase.Refresh();
        }
        
        private static void ProcessBaseGameRandomMultiplierWildsMechanic(string machineName, string machineVariant, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var reelCount = BettrMenu.GetReelCount(machineName);
            
            var symbolIndexesByReel = new Dictionary<string, List<int>>();
            for (var reelIndex = 1; reelIndex <= reelCount; reelIndex++)
            {
                var topSymbolCount = BettrMenu.GetTopSymbolCount(machineName, reelIndex);
                var visibleSymbolCount = BettrMenu.GetVisibleSymbolCount(machineName, reelIndex);
                
                var scatterSymbolIndexes = new List<int>();
                for (int symbolIndex = topSymbolCount + 1;
                     symbolIndex <= topSymbolCount + visibleSymbolCount;
                     symbolIndex++)
                {
                    scatterSymbolIndexes.Add(symbolIndex);
                }
                
                symbolIndexesByReel.Add($"{reelIndex}", scatterSymbolIndexes);
            }

            string templateName = "BaseGameRandomMultiplierWildsMechanic";
            var scribanTemplate = ParseScribanTemplate($"common/{machineName}/{machineVariant}", templateName);

            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "reelCount", reelCount },
                { "randomMultiplierWildsSymbolIndexesByReel", symbolIndexesByReel },
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log(json);
            
            Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
            if (mechanic == null)
            {
                throw new Exception($"Failed to deserialize mechanic from json: {json}");
            }
            
            foreach (var mechanicAnimation in mechanic.Animations)
            {
                BettrAnimatorController.AddAnimationState(mechanicAnimation.Filename, mechanicAnimation.AnimationStates, mechanicAnimation.AnimatorTransitions, runtimeAssetPath);
            }
            
            foreach (var tilePropertyAnimator in mechanic.TilePropertyAnimators)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{tilePropertyAnimator.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, tilePropertyAnimator.PrefabName);
                if (tilePropertyAnimator.PrefabIds != null)
                {
                    foreach (var prefabId in tilePropertyAnimator.PrefabIds)
                    {
                        var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                        InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                    }
                }
                if (tilePropertyAnimator.AnimatorsProperty != null)
                {
                    var properties = new List<TilePropertyAnimator>();
                    var groupProperties = new List<TilePropertyAnimatorGroup>();
                    foreach (var animatorProperty in tilePropertyAnimator.AnimatorsProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(animatorProperty.Id, out var referenceGameObject);
                        var tileProperty = new TilePropertyAnimator()
                        {
                            key = animatorProperty.Key,
                            value = new PropertyAnimator()
                            {
                                animator = referenceGameObject?.Animator, 
                                animationStateName = animatorProperty.State,
                                delayBeforeAnimationStart = animatorProperty.DelayBeforeStart,
                                waitForAnimationComplete = animatorProperty.WaitForComplete,
                                overrideAnimationDuration = animatorProperty.OverrideDuration,
                                animationDuration = animatorProperty.AnimationDuration,
                            },
                        };
                        if (tileProperty.value.animator == null)
                        {
                            Debug.LogError($"Failed to find animator with id: {animatorProperty.Id}");
                        }
                        properties.Add(tileProperty);                        
                    }
                    var component = prefabGameObject.GameObject.GetComponent<TilePropertyAnimators>();
                    component.tileAnimatorProperties.AddRange(properties);
                    component.tileAnimatorGroupProperties.AddRange(groupProperties);
                }
                
                PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
            }
            
            // save the changes
            AssetDatabase.SaveAssets();
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
        
        public static GameObject ProcessPrefab(string prefabName, IGameObject rootGameObject, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var prefabPath = $"{runtimeAssetPath}/Prefabs/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null)
            {
                prefab = rootGameObject.GameObject;
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            
            AssetDatabase.Refresh();
            
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return prefab;
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
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return prefab;
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

    public static class BaseGameWaysMechanic
    {
        public static void Process(string machineName, string machineVariant, string runtimeAssetPath)
        {
            ProcessWinPrefab(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameSymbolModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameMachineModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameReelModifications(machineName, machineVariant, runtimeAssetPath);
        }

        private static void ProcessWinPrefab(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string symbolName = $"{machineName}BaseGameWaysWin";
            var templateName = "BaseGameWaysWinPrefab";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/ways/", templateName);

            var model = new Dictionary<string, object>
            {
            };
            
            var json = scribanTemplate.Render(model);
            Console.WriteLine(json);
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            BettrMenu.ProcessPrefab(symbolName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessBaseGameSymbolModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string templateName = "BaseGameWaysSymbolModifications";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/ways", templateName);
            
            var baseGameSymbolTable = BettrMenu.GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            var symbolPrefabNames = baseGameSymbolTable.Pairs.Select(pair => $"{machineName}BaseGameSymbol{pair.Key.String}").ToList();
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
                
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "symbolKeys", symbolKeys},
                { "symbolPrefabNames", symbolPrefabNames},
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log(json);
            
            Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
            if (mechanic == null)
            {
                throw new Exception($"Failed to deserialize mechanic from json: {json}");
            }
            
            // Modified Animator Controllers
            if (mechanic.AnimatorControllers != null)
            {
                foreach (var instanceComponent in mechanic.AnimatorControllers)
                {
                    AssetDatabase.Refresh();

                    BettrAnimatorController.AddAnimationState(instanceComponent.Filename,
                        instanceComponent.AnimationStates, instanceComponent.AnimatorTransitions, runtimeAssetPath);
                }
            }
            
            
            AssetDatabase.Refresh();
        }
        
        private static void ProcessBaseGameMachineModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            for (int i = 1; i <= 3; i++)
            {
                string templateName = $"BaseGameWaysMachineModifications{i}";
                var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/ways", templateName);
            
                var symbolKeys = BettrMenu.GetSymbolKeys(machineName);
            
                var model = new Dictionary<string, object>
                {
                    { "machineName", machineName },
                    { "machineVariant", machineVariant },
                    { "symbolKeys", symbolKeys},
                };
                
                var json = scribanTemplate.Render(model);
                Debug.Log(json);
                
                Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
                if (mechanic == null)
                {
                    throw new Exception($"Failed to deserialize mechanic from json: {json}");
                }

                mechanic.Process();
            
                AssetDatabase.Refresh();
            }
        }
        
        private static void ProcessBaseGameReelModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string templateName = "BaseGameWaysReelModifications";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/ways", templateName);
            
            var symbolKeys = BettrMenu.GetSymbolKeys(machineName);
            var reelCount = BettrMenu.GetReelCount(machineName);
            
            for (var reelIndex = 1; reelIndex <= reelCount; reelIndex++)
            {
                var topSymbolCount = BettrMenu.GetTopSymbolCount(machineName, reelIndex);
                var visibleSymbolCount = BettrMenu.GetVisibleSymbolCount(machineName, reelIndex);
                var waysSymbolIndexes = Enumerable.Range(topSymbolCount+1, visibleSymbolCount).ToList();
                
                var symbolPositions = BettrMenu.GetSymbolPositions(machineName, reelIndex);
                var symbolVerticalSpacing = BettrMenu.GetSymbolVerticalSpacing(machineName, reelIndex);
                var yPositions = symbolPositions.Select(pos => pos * symbolVerticalSpacing).ToList();

                yPositions.Insert(0, 0);
                
                var symbolScaleX = BettrMenu.GetSymbolScaleX(machineName, reelIndex);
                var symbolScaleY = BettrMenu.GetSymbolScaleY(machineName, reelIndex);
                
                var symbolOffsetY = BettrMenu.GetSymbolOffsetY(machineName, reelIndex);
                
                InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
                InstanceGameObject.IdGameObjects.Clear();
                
                BettrMaterialGenerator.MachineName = machineName;
                BettrMaterialGenerator.MachineVariant = machineVariant;
                
                var model = new Dictionary<string, object>
                {
                    { "machineName", machineName },
                    { "machineVariant", machineVariant },
                    { "reelIndex", reelIndex },
                    { "yPositions", yPositions },
                    { "waysSymbolIndexes", waysSymbolIndexes },
                    { "symbolKeys", symbolKeys},
                    { "symbolScaleX", symbolScaleX},
                    { "symbolScaleY", symbolScaleY},
                    { "symbolOffsetY", symbolOffsetY},
                };
                
                var json = scribanTemplate.Render(model);
                Debug.Log(json);
                
                Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
                if (mechanic == null)
                {
                    throw new Exception($"Failed to deserialize mechanic from json: {json}");
                }

                mechanic.Process();

            }
            
            AssetDatabase.Refresh();
        }
    }

    public static class BaseGamePaylinesMechanic
    {
        public static void Process(string machineName, string machineVariant, string runtimeAssetPath)
        {
            ProcessPaylinePrefab(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameSymbolModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameMachineModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameReelModifications(machineName, machineVariant, runtimeAssetPath);
        }
        
        private static void ProcessBaseGameMachineModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string templateName = "BaseGamePaylinesMachineModifications";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/paylines", templateName);
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
            };
                
            var json = scribanTemplate.Render(model);
            Debug.Log(json);
                
            Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
            if (mechanic == null)
            {
                throw new Exception($"Failed to deserialize mechanic from json: {json}");
            }

            mechanic.Process();
            
            AssetDatabase.Refresh();
        }
        
        private static void ProcessPaylinePrefab(string machineName, string machineVariant, string runtimeAssetPath)
        {
            var templateName = "BaseGamePaylinesPrefab";
            var prefabName = $"{machineName}{templateName}";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/paylines/", templateName);

            var model = new Dictionary<string, object>
            {
                {"machineName", machineName},
            };
            
            var json = scribanTemplate.Render(model);
            Console.WriteLine(json);
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            BettrMenu.ProcessPrefab(prefabName, 
                hierarchyInstance, 
                runtimeAssetPath);
        }
        
        private static void ProcessBaseGameReelModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string templateName = $"BaseGamePaylinesReelModifications";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/paylines", templateName);
            
            var baseGameSymbolTable = BettrMenu.GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            
            var reelStates = BettrMenu.GetTable($"{machineName}BaseGameReelState");
            var reelCount = BettrMenu.GetReelCount(machineName);
            
            for (var reelIndex = 1; reelIndex <= reelCount; reelIndex++)
            {
                var topSymbolCount = BettrMenu.GetTopSymbolCount(machineName, reelIndex);
                var visibleSymbolCount = BettrMenu.GetVisibleSymbolCount(machineName, reelIndex);
                var paylinesSymbolIndexes = Enumerable.Range(topSymbolCount+1, visibleSymbolCount).ToList();
                
                var symbolPositions = BettrMenu.GetSymbolPositions(machineName, reelIndex);
                var symbolVerticalSpacing = BettrMenu.GetSymbolVerticalSpacing(machineName, reelIndex);
                var yPositions = symbolPositions.Select(pos => pos * symbolVerticalSpacing).ToList();

                yPositions.Insert(0, 0);
                
                var symbolOffsetY = BettrMenu.GetSymbolOffsetY(machineName, reelIndex);
                
                InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
                InstanceGameObject.IdGameObjects.Clear();
                
                BettrMaterialGenerator.MachineName = machineName;
                BettrMaterialGenerator.MachineVariant = machineVariant;
                
                var model = new Dictionary<string, object>
                {
                    { "machineName", machineName },
                    { "machineVariant", machineVariant },
                    { "reelIndex", reelIndex },
                    { "yPositions", yPositions },
                    { "topSymbolCount", topSymbolCount },
                    { "visibleSymbolCount", visibleSymbolCount },
                    { "paylinesSymbolIndexes", paylinesSymbolIndexes },
                    { "symbolKeys", symbolKeys},
                    { "symbolOffsetY", symbolOffsetY},
                };
                
                var json = scribanTemplate.Render(model);
                Debug.Log(json);
                
                Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
                if (mechanic == null)
                {
                    throw new Exception($"Failed to deserialize mechanic from json: {json}");
                }

                mechanic.Process();
            }
            
            AssetDatabase.Refresh();
        }
        
        private static void ProcessBaseGameSymbolModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            string templateName = "BaseGamePaylinesSymbolModifications";
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/paylines", templateName);
            
            var baseGameSymbolTable = BettrMenu.GetTable($"{machineName}BaseGameSymbolTable");
            var symbolKeys = baseGameSymbolTable.Pairs.Select(pair => pair.Key.String).ToList();
            var symbolPrefabNames = baseGameSymbolTable.Pairs.Select(pair => $"{machineName}BaseGameSymbol{pair.Key.String}").ToList();
            
            InstanceComponent.RuntimeAssetPath = runtimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
                
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "symbolKeys", symbolKeys},
                { "symbolPrefabNames", symbolPrefabNames},
            };
            
            var json = scribanTemplate.Render(model);
            Debug.Log(json);
            
            Mechanic mechanic = JsonConvert.DeserializeObject<Mechanic>(json);
            if (mechanic == null)
            {
                throw new Exception($"Failed to deserialize mechanic from json: {json}");
            }
            
            // Modified Animator Controllers
            if (mechanic.AnimatorControllers != null)
            {
                foreach (var instanceComponent in mechanic.AnimatorControllers)
                {
                    AssetDatabase.Refresh();

                    BettrAnimatorController.AddAnimationState(instanceComponent.Filename,
                        instanceComponent.AnimationStates, instanceComponent.AnimatorTransitions, runtimeAssetPath);
                }
            }
            
            
            AssetDatabase.Refresh();
        }
        
    }
}