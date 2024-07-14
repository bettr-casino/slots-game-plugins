using UnityEditor;
using UnityEngine;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Bettr.Editor.generators
{
    public static class BettrFBXController
    {
        public static void ImportFBX(string sourcePath, string destinationPathPrefix, string fbxFilename)
        {
            // Construct paths for FBX and textures
            var fbxDestinationPath = Path.Combine(destinationPathPrefix, "FBX");
            var texturesDestinationPath = Path.Combine(destinationPathPrefix, "Textures");

            // Ensure the FBX and textures directories exist
            CreateDirectoryIfNotExists(fbxDestinationPath);
            CreateDirectoryIfNotExists(texturesDestinationPath);

            // Import the FBX file
            var sourceFilePath = $"{sourcePath}/{fbxFilename}";
            var destinationFilePath = Path.Combine(fbxDestinationPath, fbxFilename);

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
                            foreach (Material mat in renderer.sharedMaterials)
                            {
                                if (mat != null)
                                {
                                    string texturePath = Path.Combine(texturesDestinationPath, mat.name + ".png");
                                    Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(GetRelativePath(texturePath));
                                    if (texture != null)
                                    {
                                        mat.mainTexture = texture;
                                        EditorUtility.SetDirty(mat); // Mark the material as dirty
                                    }
                                }
                            }
                        }

                        // Save the instantiated model as a prefab
                        string prefabPath = Path.Combine(destinationPathPrefix, "Prefabs", Path.GetFileNameWithoutExtension(fbxFilename) + ".prefab");
                        CreateDirectoryIfNotExists(Path.GetDirectoryName(prefabPath));
                        PrefabUtility.SaveAsPrefabAsset(instantiatedModel, GetRelativePath(prefabPath));
                        
                        AssetDatabase.SaveAssets();

                        Debug.Log("FBX imported and textures assigned successfully. Prefab saved at: " + prefabPath);
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
