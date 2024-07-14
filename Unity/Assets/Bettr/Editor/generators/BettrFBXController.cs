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

            AssetDatabase.Refresh();

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
                        // Set rotation to 0, 180, 0
                        instantiatedModel.transform.rotation = Quaternion.Euler(0, 180, 0);

                        // Apply extracted textures to the model
                        Renderer[] renderers = instantiatedModel.GetComponentsInChildren<Renderer>();
                        foreach (Renderer renderer in renderers)
                        {
                            for (int i = 0; i < renderer.sharedMaterials.Length; i++)
                            {
                                Material originalMat = renderer.sharedMaterials[i];
                                if (originalMat != null)
                                {
                                    // Clone the material
                                    Material clonedMat = new Material(originalMat);
                                    string texturePath = Path.Combine(texturesDestinationPath, originalMat.name + ".png");
                                    texturesPath.Add(texturePath);
                                    var materialPath = Path.Combine(materialsDestinationPath, originalMat.name + ".mat");
                                    materialsPath.Add(materialPath);
                                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetRelativePath(texturePath));
                                    if (texture != null)
                                    {
                                        clonedMat.mainTexture = texture;
                                    }

                                    // Save the cloned material
                                    CreateDirectoryIfNotExists(Path.GetDirectoryName(materialPath));
                                    AssetDatabase.CreateAsset(clonedMat, GetRelativePath(materialPath));

                                    // Assign the cloned material
                                    renderer.sharedMaterials[i] = clonedMat;
                                }
                            }
                        }

                        // Save the instantiated model as a prefab
                        string prefabPath = Path.Combine(prefabsDestinationPath, Path.GetFileNameWithoutExtension(targetFbxFilename) + ".prefab");
                        CreateDirectoryIfNotExists(Path.GetDirectoryName(prefabPath));
                        PrefabUtility.SaveAsPrefabAsset(instantiatedModel, GetRelativePath(prefabPath));
                        
                        AssetDatabase.SaveAssets();
                        AssetDatabase.Refresh();

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
