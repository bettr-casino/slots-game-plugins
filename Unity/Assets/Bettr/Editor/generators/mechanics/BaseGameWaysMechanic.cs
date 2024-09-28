using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators.mechanics
{
    public static class BaseGameWaysMechanic
    {
        public static void Process(string machineName, string machineVariant, string runtimeAssetPath)
        {
            ProcessBaseGameSymbolModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameMachineModifications(machineName, machineVariant, runtimeAssetPath);
            ProcessBaseGameReelModifications(machineName, machineVariant, runtimeAssetPath);
        }

        private static void ProcessBaseGameSymbolModifications(string machineName, string machineVariant, string runtimeAssetPath)
        {
            for (int i = 1; i <= 2; i++)
            {
                string templateName = $"BaseGameWaysSymbolModifications{i}";
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
                else
                {
                    mechanic.Process();
                }
                
                AssetDatabase.Refresh();
            }
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
}