using System;
using System.Collections.Generic;
using CrayonScript.Code;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;

// TODO: FIXME: This class is not implemented yet. It is a placeholder for the RandomWildsMultiplierMechanic feature.
namespace Bettr.Editor.generators.mechanics
{
    public static class RandomWildsMultiplierMechanic
    {
        public static void ProcessBaseGameRandomMultiplierWildsMechanic(string machineName, string machineVariant, string runtimeAssetPath)
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
            var scribanTemplate = BettrMenu.ParseScribanTemplate($"common", templateName);

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
                var prefabGameObject = new PrefabGameObject(prefab, tilePropertyAnimator.PrefabName, false);
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
    }
}