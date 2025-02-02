using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Bettr.Editor.generators
{
    public static class BettrFBXController
    {
        public static void ImportFBX(string sourcePath, string destinationPathPrefix, string fbxFilename, string targetFbxFilename)
        {
            // Construct paths for FBX, textures, and materials
            var fbxDestinationPath = Path.Combine(destinationPathPrefix, "FBX");
            var texturesDestinationPath = Path.Combine(destinationPathPrefix, "Textures");
            var materialsDestinationPath = Path.Combine(destinationPathPrefix, "Materials");
            var prefabsDestinationPath = Path.Combine(destinationPathPrefix, "Prefabs");

            // Import the FBX file
            var sourceFilePath = Path.Combine(sourcePath, fbxFilename);
            var destinationFilePath = Path.Combine(fbxDestinationPath, targetFbxFilename);

            List<string> texturesPath = new List<string>();
            List<string> materialsPath = new List<string>();

            // Ensure the directories exist
            CreateDirectoryIfNotExists(fbxDestinationPath);
            CreateDirectoryIfNotExists(texturesDestinationPath);
            CreateDirectoryIfNotExists(materialsDestinationPath);
            CreateDirectoryIfNotExists(prefabsDestinationPath);

            File.Copy(sourceFilePath, destinationFilePath, overwrite: true);

            var asset = AssetDatabase.LoadAssetAtPath<Object>(GetRelativePath(destinationFilePath));
            if (asset == null)
            {
                Debug.LogError("Failed to load FBX file at path: " + destinationFilePath);
                return;
            }

            // Save the currently active scene
            var newScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            newScene.name = "ImportFBX";

            try
            {
                // Extract textures
                string assetPath = AssetDatabase.GetAssetPath(asset);
                ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

                if (modelImporter == null)
                {
                    Debug.LogError("Failed to get ModelImporter for asset at path: " + assetPath);
                    return;
                }

                modelImporter.ExtractTextures(GetRelativePath(texturesDestinationPath));

                // Re-import the model to apply changes
                AssetDatabase.ImportAsset(assetPath, ImportAssetOptions.ForceUpdate);

                // Find the imported model
                GameObject fbxModel = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);

                if (fbxModel != null)
                {
                    // Instantiate the model in the scene
                    GameObject instantiatedModel = PrefabUtility.InstantiatePrefab(fbxModel) as GameObject;

                    if (instantiatedModel != null)
                    {
                        Renderer[] renderers = instantiatedModel.GetComponentsInChildren<Renderer>();
                        Dictionary<string, Material> materialMap = new Dictionary<string, Material>();

                        foreach (Renderer renderer in renderers)
                        {
                            var sharedMaterials = renderer.sharedMaterials;
                            var clonedSharedMaterials = new Material[sharedMaterials.Length];
                            
                            for (int i = 0; i < sharedMaterials.Length; i++)
                            {
                                Material originalMat = renderer.sharedMaterials[i];
                                clonedSharedMaterials[i] = originalMat;
                                if (originalMat != null)
                                {
                                    if (!materialMap.TryGetValue(originalMat.name, out Material clonedMat))
                                    {
                                        // Clone the material
                                        clonedMat = new Material(originalMat);
                                        clonedMat.shader = Shader.Find("Unlit/Texture"); // Switch shader to Unlit/Texture

                                        var materialPath = Path.Combine(materialsDestinationPath, originalMat.name + ".mat");
                                        materialsPath.Add(materialPath);
                                        AssetDatabase.CreateAsset(clonedMat, GetRelativePath(materialPath));

                                        // Extract textures and assign to cloned material
                                        string[] texturePropertyNames = originalMat.GetTexturePropertyNames();
                                        foreach (string propertyName in texturePropertyNames)
                                        {
                                            Texture texture = originalMat.GetTexture(propertyName);
                                            if (texture != null)
                                            {
                                                string texturePath = AssetDatabase.GetAssetPath(texture);
                                                if (!string.IsNullOrEmpty(texturePath))
                                                {
                                                    string destinationTexturePath = Path.Combine(texturesDestinationPath, Path.GetFileName(texturePath));
                                                    texturesPath.Add(destinationTexturePath);
                                                    AssetDatabase.CopyAsset(texturePath, destinationTexturePath);
                                                    clonedMat.SetTexture(propertyName, AssetDatabase.LoadAssetAtPath<Texture>(destinationTexturePath));
                                                }
                                            }
                                        }

                                        materialMap[originalMat.name] = clonedMat;
                                    }

                                    // Assign the cloned material
                                    clonedSharedMaterials[i] = clonedMat;
                                }
                            }

                            // Apply cloned materials to renderer
                            renderer.sharedMaterials = clonedSharedMaterials;
                        }

                        // Save the instantiated model as a prefab
                        string prefabPath = Path.Combine(prefabsDestinationPath, Path.GetFileNameWithoutExtension(targetFbxFilename) + ".prefab");
                        CreateDirectoryIfNotExists(Path.GetDirectoryName(prefabPath));
                        PrefabUtility.SaveAsPrefabAsset(instantiatedModel, GetRelativePath(prefabPath));
                        
                        AssetDatabase.SaveAssets();

                        Debug.Log($"FBX imported successfully. Prefab saved at: {prefabPath}");
                        Debug.Log($"FBX textures extracted successfully. Textures saved at: {string.Join(",", texturesPath)}");
                        Debug.Log($"FBX materials extracted successfully. Materials saved at: {string.Join(",", materialsPath)}");
                    }
                    else
                    {
                        Debug.LogError("Failed to instantiate the model in the scene.");
                    }
                }
                else
                {
                    Debug.LogError("Failed to load imported model at path: " + assetPath);
                }
            }
            finally
            {
                try
                {
                    var replacementScene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
                    SceneManager.SetActiveScene(replacementScene);
                    EditorSceneManager.CloseScene(newScene, true);
                }
                catch (System.Exception e)
                {
                    Debug.LogError("Failed to replace the new scene 'ImportFBX': " + e.Message);
                }
            }
        }

        private static void CreateDirectoryIfNotExists(string path)
        {
            if (string.IsNullOrEmpty(path))
                return;

            if (!AssetDatabase.IsValidFolder(path))
            {
                string[] folders = path.Split(Path.DirectorySeparatorChar);
                string currentPath = folders[0];

                for (int i = 1; i < folders.Length; i++)
                {
                    string nextFolder = Path.Combine(currentPath, folders[i]);
                    if (!AssetDatabase.IsValidFolder(nextFolder))
                    {
                        AssetDatabase.CreateFolder(currentPath, folders[i]);
                    }
                    currentPath = nextFolder;
                }
            }
        }

        private static string GetRelativePath(string fullPath)
        {
            if (fullPath.StartsWith(Application.dataPath))
            {
                return "Assets" + fullPath.Substring(Application.dataPath.Length).Replace('\\', '/');
            }
            return fullPath;
        }
    }
}
