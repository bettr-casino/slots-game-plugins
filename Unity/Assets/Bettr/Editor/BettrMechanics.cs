using System;
using System.Collections.Generic;
using System.IO;
using Bettr.Editor.generators;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor
{
    public static class BettrMechanicsHelpers
    {
        public static void EnsureMechanicsDirectory(string runtimeAssetPath)
        {
            if (!Directory.Exists(runtimeAssetPath))
            {
                Directory.CreateDirectory(runtimeAssetPath);
                AssetDatabase.Refresh();
                Debug.Log("Directory created at: " + runtimeAssetPath);
            }
            
            string[] subDirectories = { "Animators", "Materials", "Models", "FBX", "Prefabs", "Scenes", "Scripts", "Textures" };
            foreach (string subDir in subDirectories)
            {
                EnsureDirectory(Path.Combine(runtimeAssetPath, subDir));
            }
        }
        
        public static void EnsureDirectory(string path)
        {
            if (!Directory.Exists(path))
            {
                Directory.CreateDirectory(path);
                AssetDatabase.Refresh();
                Debug.Log("Directory created at: " + path);
            }
        }
        
        public static void ProcessBaseGameMechanic(
            string runtimeAssetPath, Dictionary<string, object> model,
            string templateName, string prefabName, string mechanicName)
        {
            Debug.Log($"ProcessBaseGameMechanic {mechanicName} {templateName} {prefabName}");
            
            var scribanTemplate = BettrMenu.ParseScribanTemplate(Path.Combine("mechanics", mechanicName.ToLower()), templateName);

            // append the mechanic name to the runtimeAssetPath
            var mechanicRuntimeAssetPath = Path.Combine(runtimeAssetPath, "Mechanics", mechanicName);
            EnsureMechanicsDirectory(mechanicRuntimeAssetPath);
            
            var json = scribanTemplate.Render(model);
            Console.WriteLine(json);
            
            InstanceComponent.RuntimeAssetPath = mechanicRuntimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            BettrPrefabController.ProcessPrefab(prefabName, 
                hierarchyInstance, 
                mechanicRuntimeAssetPath, force:true);
        }
    }
    
    public static class BettrMechanics
    {
        public static string RuntimeAssetPath { get; set; }
        
        public static void ProcessCascadingReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModel)
        {
            Debug.Log(
                $"Processing cascading reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModel}");
        }

        public static void ProcessCascadingReelsMultiplierMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing cascading reels multiplier mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "CascadingReelsMultiplier";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                 runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
                
        }

        public static void ProcessChooseASideMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing choose a side mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "ChooseASide";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var dataSummary = BettrMenu.GetTable($"{machineName}BaseGameChooseASideDataSummary");
            
            // get the model
            var steps = BettrMenu.GetTableValue<int>(dataSummary, "ChooseASide", "Steps", 0);
            Debug.Log($"ProcessChooseASideMechanic steps: {steps}");
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "steps", steps},
                { "leftStepStartInclusive", 1},
                { "leftStepEndInclusive", (int)(steps / 2)},
                { "rightStepStartInclusive", (int)(steps / 2) + 2},
                { "rightStepEndInclusive", steps},
                { "middleStep", (int)(steps / 2) + 1},
                { "symbolNames", new string[] { "BN1", "BN2" } }
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessExpandingPaylinesMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing expanding paylines mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessExpandingReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing expanding reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessFlipReelsMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing flip reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessFreeSpinsCollectionMeterMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing free spins collection meter mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessHorizontalReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing horizontal reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessHorizontalReelsShiftMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing horizontal reels shift mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "HorizontalReelsShift";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessHotReelsMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing hot reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }
        
        public static void ProcessIndependentReelsMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing independent reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "IndependentReels";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var dataSummary = BettrMenu.GetTable($"{machineName}BaseGameIndependentReelsDataSummary");
            var rowCount = BettrMenu.GetTableValue<int>(dataSummary, "IndependentReels", "RowCount", 0);
            var columnCount = BettrMenu.GetTableValue<int>(dataSummary, "IndependentReels", "ColumnCount", 0);
            
            Debug.Log($"ProcessIndependentReelsMechanic rowCount: {rowCount} columnCount: {columnCount}");
            
            // get the model
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "rowCount", rowCount },
                { "columnCount", columnCount },
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessInfinityReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing infinity reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessLinkedReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing linked reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessLockedReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing locked reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessMegaSymbolsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing mega symbols mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessMegaWaysMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing megaways mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessMirrorReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModel)
        {
            Debug.Log(
                $"Processing mirror reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModel}");
        }

        public static void ProcessMysteryReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing mystery reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessMysterySymbolsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing mystery symbols mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessNudgingReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing nudging reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessPaylinesMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing paylines mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessProgressiveMultipliersMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing progressive multipliers mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessRandomMultipliersMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModel)
        {
            Debug.Log(
                $"Processing random multipliers mechanic for {machineName} {machineVariant} {experimentVariant} {machineModel}");
        }

        public static void ProcessRandomMultiplierWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing random multiplier wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessRandomWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing random wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessReelBurstMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing reel burst mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessReelRushMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing reel rush mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessReelSplitterMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing reel splitter mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessReelSwapMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing reel swap mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }
        
        public static void ProcessReelAnticipationMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing reelanticipation mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "ReelAnticipation";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessScattersMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing scatters mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "Scatters";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var data2Summary = BettrMenu.GetTable($"{machineName}BaseGameScattersData2");
            var scatterSymbols = BettrMenu.GetTableArray<string>(data2Summary, "Scatters", "ScatterSymbol");
            
            // convert scatterSymbols to array
            var scatterSymbolNames = scatterSymbols.ToArray();
            
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "symbolNames", scatterSymbolNames }
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessScatterBonusFreeSpinsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing scatter bonus free spins mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessScatterRespinsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing scatter respins mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessShiftingReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing shifting reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSplitSymbolsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing split symbols mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessStackedWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing stacked wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessStickyWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing sticky wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSurroundingWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing surrounding wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolBurstMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol burst mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolCollectionMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol collection mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolLockRespinsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol lock respins mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolSlideMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol slide mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolSwapMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol swap mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolTransformationMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol transformation mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSymbolUpgradeMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing symbol upgrade mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessSyncedReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing synced reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWanderingWildsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing wandering wilds mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWaysMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing ways mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWildsGenerationMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing wilds generation mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWildsMultiplierMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing wilds multiplier mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWildsOverlaysMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing wilds overlays mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWildsReelsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing wilds reels mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }

        public static void ProcessWinBothWaysMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            Debug.Log(
                $"Processing win both ways mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
        }
    }
}