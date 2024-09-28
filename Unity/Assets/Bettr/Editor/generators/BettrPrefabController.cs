using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrPrefabController
    {
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
        
        public static GameObject ProcessPrefab(string prefabName, List<IComponent> components, List<IGameObject> gameObjects, string runtimeAssetPath)
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
    }
}