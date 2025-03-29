using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Bettr.Editor.generators;
using CrayonScript.Interpreter;
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
            SplitJsonIntoChunks(json);
            
            InstanceComponent.DefaultRuntimeAssetPath = runtimeAssetPath;
            InstanceComponent.RuntimeAssetPath = mechanicRuntimeAssetPath;
            InstanceGameObject.IdGameObjects.Clear();
            InstanceGameObject.SymbolMaterialCache.Clear();
            
            InstanceGameObject hierarchyInstance = JsonConvert.DeserializeObject<InstanceGameObject>(json);
            hierarchyInstance.SetParent((GameObject) null);

            BettrPrefabController.ProcessPrefab(prefabName, 
                hierarchyInstance, 
                mechanicRuntimeAssetPath, force:true);
        }
        
        public static void SplitJsonIntoChunks(string json, int linesPerChunk = 300)
        {
            string[] lines = json.Split('\n'); // Split JSON by lines
            int totalLines = lines.Length;
            int chunkCount = Mathf.CeilToInt((float)totalLines / linesPerChunk);

            for (int chunkIndex = 0; chunkIndex < chunkCount; chunkIndex++)
            {
                int startLine = chunkIndex * linesPerChunk;
                int endLine = Mathf.Min(startLine + linesPerChunk, totalLines);
                string chunk = string.Join("\n", lines, startLine, endLine - startLine);

                Debug.Log($"Chunk {chunkIndex + 1}/{chunkCount}:\n{chunk}");
            }
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
        
        public static void ProcessReelMatrixMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing reel matrix mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "ReelMatrix";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var dataSummary = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixDataSummary");
            var columnCount = BettrMenu.GetTableValue<int>(dataSummary, "ReelMatrix", "ColumnCount", 0);
            
            var data = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData");
            var rowCounts = new int[columnCount];
            for (int i = 0; i < columnCount; i++)
            {
                var rowCount = BettrMenu.GetTableValue<int, int>(data, $"ReelMatrix", "ColumnIndex", i, "RowCount", 0);
                rowCounts[i] = (int) rowCount;
            }
            
            var data2 = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData2");
            var topSymbolCount = BettrMenu.GetTableValue<int>(data2, "LayoutProperties", "TopSymbolCount", 0);
            var bottomSymbolOffset = BettrMenu.GetTableValue<int>(data2, "LayoutProperties", "BottomSymbolCount", 0);
            var visibleSymbolOffset = BettrMenu.GetTableValue<int>(data2, "LayoutProperties", "VisibleSymbolCount", 0);

            var totalSymbolCount = topSymbolCount + bottomSymbolOffset + visibleSymbolOffset;

            var symbolScaleX = BettrMenu.GetTableValue<int>(data2, "LayoutProperties", "SymbolScaleX", 1);
            var symbolScaleY = BettrMenu.GetTableValue<int>(data2, "LayoutProperties", "SymbolScaleY", 1);
            var symbolOffsetY = BettrMenu.GetTableValue<float>(data2, "LayoutProperties", "SymbolOffsetY", 0.0f);
            
            var data4 = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData4");
            var symbols = BettrMenu.GetTableArray<string>(data4, "Symbols", "Symbol");
            
            var data5 = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData5");
            var horizontalReelPositions = new float[columnCount];
            for (var i = 0; i < columnCount; i++)
            {
                horizontalReelPositions[i] = BettrMenu.GetTableValue<float, string>(data5, "Columns", "Column", $"Col{i + 1}", "HorizontalPosition", 0.0f);
            }
            
            var data6 = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData6");
            var symbolPositions = BettrMenu.GetTableArray<double>(data6, $"SymbolGroups", "SymbolPosition");
            var symbolYPositions = symbolPositions.Select(d => (int)d).Reverse().ToList();
            
            var data7 = BettrMenu.GetTable($"{machineName}BaseGameReelMatrixData7");
            var cellMaskPositions = BettrMenu.GetTableArray<double>(data7, $"CellMask", "CellMaskPosition");
            var cellMaskYPositions = cellMaskPositions.Select(d => (int)d).Reverse().ToList();
            
            // get the model
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "experimentVariant", experimentVariant },
                { "mechanicName", mechanicName },
                { "rowCounts", rowCounts },
                { "columnCount", columnCount },
                { "totalSymbolCount", totalSymbolCount },
                { "symbolKeys", symbols },
                { "symbolYPositions", symbolYPositions },
                { "cellMaskYPositions", cellMaskYPositions },
                { "symbolScaleX", symbolScaleX },
                { "symbolScaleY", symbolScaleY },
                { "topSymbolOffset", topSymbolCount },
                { "bottomSymbolOffset", bottomSymbolOffset },
                { "visibleSymbolOffset", visibleSymbolOffset },
                { "symbolOffsetY", symbolOffsetY },
                { "horizontalReelPositions", horizontalReelPositions }
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }
        
        public static void ProcessFreeSpinsMechanic(string machineName, string machineVariant, string experimentVariant,
            string machineModelPath)
        {
            Debug.Log(
                $"Processing free spins mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var mechanicName = "FreeSpins";
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var dataMatrix = BettrMenu.GetTable($"{machineName}BaseGameFreeSpinsDataMatrix");
            var symbolKeys = BettrMenu.GetTableArray<string>(dataMatrix, "ReelStrip", "ReelSymbol");
            
            var symbolCount = symbolKeys.Count;
            
            // get the model
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "experimentVariant", experimentVariant },
                { "mechanicName", mechanicName },
                { "symbolKeys", symbolKeys },
                { "symbolCount", symbolCount },
            };
            
            BettrMechanicsHelpers.ProcessBaseGameMechanic(
                runtimeAssetPath, model,
                templateName, prefabName, mechanicName);
        }

        public static void ProcessLockedSymbolsMechanic(string machineName, string machineVariant,
            string experimentVariant, string machineModelPath)
        {
            var mechanicName = "LockedSymbols";
            
            Debug.Log(
                $"Processing {mechanicName} mechanic for {machineName} {machineVariant} {experimentVariant} {machineModelPath}");
            
            var templateName = $"BaseGame{mechanicName}Mechanic";
            var prefabName = $"BaseGameMachine{mechanicName}";
            var runtimeAssetPath = BettrMechanics.RuntimeAssetPath;
            
            BettrMaterialGenerator.MachineName = machineName;
            BettrMaterialGenerator.MachineVariant = machineVariant;
            
            var dataMatrix = BettrMenu.GetTable($"{machineName}BaseGameLockedSymbolsDataMatrix");
            var replacementSymbols = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "Symbol");
            var symbolTypes = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "SymbolType");
            var lockSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "LockSymbolID");
            var specialMultipliersSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "SpecialMultipliersSymbolID");
            var specialCreditsSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "SpecialCreditsSymbolID");
            var heapMultipliersSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "HeapMultipliersSymbolID");
            var heapCreditsSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "HeapCreditsSymbolID");
            var freeSpinsSymbolIDs = BettrMenu.GetTableArray<string>(dataMatrix, "ValueSymbols", "FreeSpinsSymbolID");
            // var values = BettrMenu.GetTableArray<int>(dataMatrix, "ValueSymbols", "Value");
            
            var symbolNames = new List<string>();
            for (int i = 0; i < symbolTypes.Count; i++)
            {
                var symbolType = symbolTypes[i];
                symbolNames.Add($"{symbolType}");
            }

            var symbolIDs = new HashSet<string>();
            for (int i = 0; i < symbolTypes.Count; i++)
            {
                var symbolType = symbolTypes[i];
                var lockSymbolID = lockSymbolIDs[i];
                var specialCreditsSymbolID = specialCreditsSymbolIDs[i];
                var specialMultipliersSymbolID = specialMultipliersSymbolIDs[i];
                var heapMultipliersSymbolID = heapMultipliersSymbolIDs[i];
                var heapCreditsSymbolID = heapCreditsSymbolIDs[i];
                var freeSpinsSymbolID = freeSpinsSymbolIDs[i];

                symbolIDs.Add(symbolType);
                symbolIDs.Add(lockSymbolID);
                symbolIDs.Add(specialCreditsSymbolID);
                symbolIDs.Add(specialMultipliersSymbolID);
                symbolIDs.Add(heapMultipliersSymbolID);
                symbolIDs.Add(heapCreditsSymbolID);
                symbolIDs.Add(freeSpinsSymbolID);
            }

            var symbolIDsList = symbolIDs.ToList();
            
            // get the model
            var model = new Dictionary<string, object>
            {
                { "machineName", machineName },
                { "machineVariant", machineVariant },
                { "experimentVariant", experimentVariant },
                { "mechanicName", mechanicName },
                { "symbolNames", symbolNames },
                { "replacementSymbols", replacementSymbols},
                { "symbolIDs", symbolIDsList }
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