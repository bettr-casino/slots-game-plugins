using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrModelController
    {
        public static GameObject ImportAndLoadModelAsPrefab(string modelFileName, string runtimeAssetPath)
        {
            string extension = Path.GetExtension(modelFileName);
            if (string.IsNullOrEmpty(extension))
            {
                modelFileName = $"{modelFileName}.fbx";
            }
            
            ImportModelAsPrefab(modelFileName, runtimeAssetPath);
            
            AssetDatabase.Refresh();
            
            string prefabPath = Path.Combine(runtimeAssetPath, "Prefabs", Path.GetFileNameWithoutExtension(modelFileName) + ".prefab");
            GameObject modelPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            
            return modelPrefab;
        }
        
        public static void ImportModelAsPrefab(string modelFileName, string runtimeAssetPath)
        {
            string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "fbx", modelFileName);
            string destPath = Path.Combine(runtimeAssetPath, "FBX", modelFileName);
            
            string sourceMaterialsBasePath = Path.Combine("Assets", "Bettr", "Editor", "materials");
            string destMaterialsBasePath = Path.Combine(runtimeAssetPath, "Materials");

            // Copy the FBX file to the destination path
            File.Copy(sourcePath, destPath, overwrite: true);

            // Import the copied FBX file as an asset
            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);

            // Load the imported FBX asset
            GameObject importedFbx = AssetDatabase.LoadAssetAtPath<GameObject>(destPath);

            if (importedFbx != null)
            {
                // Save and refresh the asset database
                // Create a prefab from the imported FBX and save it
                string prefabPath = Path.Combine(runtimeAssetPath, "Prefabs", Path.GetFileNameWithoutExtension(modelFileName) + ".prefab");
                
                PrefabUtility.SaveAsPrefabAsset(importedFbx, prefabPath);
            }

            // Save and refresh the asset database
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
    }
}