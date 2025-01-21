using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using CrayonScript.Code;
using Unity.Plastic.Newtonsoft.Json;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

// TODO: FIXME: This class is not implemented yet. It is a placeholder for the ScatterBonusMechanic feature.
namespace Bettr.Editor.generators.mechanics
{
    public static class ScatterBonusMechanic
    {
        private static void ProcessBaseGameScatterBonusFreeSpinsMechanic(string machineName, string machineVariant, string runtimeAssetPath)
        {
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
            var scribanTemplate = BettrMenu.ParseScribanTemplate("mechanics/scatterbonusfreespins", templateName);

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
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{modifiedPrefab.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, modifiedPrefab.PrefabName, false);
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
            
            // Anticipation animation
            foreach (var mechanicParticleSystem in mechanic.ParticleSystems)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{mechanicParticleSystem.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, mechanicParticleSystem.PrefabName, false);
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
            
            foreach (var tilePropertyParticleSystem in mechanic.TilePropertyParticleSystems)
            {
                var prefabPath =
                    $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{tilePropertyParticleSystem.PrefabName}.prefab";
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                var prefabGameObject = new PrefabGameObject(prefab, tilePropertyParticleSystem.PrefabName, false);
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
        }
    }
}