using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrPrefabController
    {
        public static GameObject ProcessPrefab(string prefabName, IGameObject rootGameObject, string runtimeAssetPath, bool force = false)
        {
            var prefabPath = $"{runtimeAssetPath}/Prefabs/{prefabName}.prefab";
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);

            if (prefab == null || force)
            {
                prefab = rootGameObject.GameObject;
                PrefabUtility.SaveAsPrefabAsset(prefab, prefabPath);
            }
            
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return prefab;
        }
        
        public static GameObject ProcessPrefab(string prefabName, List<IComponent> components, List<IGameObject> gameObjects, string runtimeAssetPath)
        {
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
            
            prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return prefab;
        }
    }
}