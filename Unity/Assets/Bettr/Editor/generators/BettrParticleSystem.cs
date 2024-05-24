using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrParticleSystem
    {
        public static ParticleSystem AddOrGetParticleSystem(string prefabName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var prefabPath = $"{runtimeAssetPath}/Prefabs/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            var particleSystem = prefab.GetComponent<ParticleSystem>();
            if (particleSystem == null)
            {
                particleSystem = prefab.AddComponent<ParticleSystem>();
            }
            
            var customData = particleSystem.customData;
            customData.enabled = true;
            customData.SetMode(ParticleSystemCustomData.Custom1, ParticleSystemCustomDataMode.Vector);
            
            return particleSystem;
        }

        public static void SaveParticleSystem(ParticleSystem particleSystem, string prefabName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var prefab = particleSystem.gameObject;
            
            var prefabPath = $"{runtimeAssetPath}/Prefabs/{prefabName}.prefab";
            // Save changes to the prefab
            PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            
            AssetDatabase.SaveAssets();
        }
        
        public static int GetParticleSystemId(ParticleSystem particleSystem)
        {
            var customData = particleSystem.customData;
            if (customData.enabled && customData.GetMode(ParticleSystemCustomData.Custom1) == ParticleSystemCustomDataMode.Vector)
            {
                ParticleSystem.MinMaxCurve curve = customData.GetVector(ParticleSystemCustomData.Custom1, 0);
                return Mathf.RoundToInt(curve.constant);
            }
            return -1; // Return an invalid ID or handle it accordingly
        }
    }
}