using UnityEditor;
using UnityEngine;
using System.IO;

namespace Bettr.Editor.generators
{
    public static class BettrFBXController
    {
        public static void ImportFBX(string sourcePath, string destinationPathPrefix)
        {
            // Construct paths for FBX and textures
            string fbxDestinationPath = Path.Combine(destinationPathPrefix, "FBX");
            string texturesDestinationPath = Path.Combine(destinationPathPrefix, "Textures");

            // Ensure the FBX and textures directories exist
            if (!Directory.Exists(fbxDestinationPath))
            {
                Directory.CreateDirectory(fbxDestinationPath);
            }
            if (!Directory.Exists(texturesDestinationPath))
            {
                Directory.CreateDirectory(texturesDestinationPath);
            }

            // Import the FBX file
            string destinationFilePath = Path.Combine(fbxDestinationPath, Path.GetFileName(sourcePath));
            File.Copy(sourcePath, destinationFilePath, true);
            AssetDatabase.Refresh();

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(destinationFilePath);
            if (asset == null)
            {
                Debug.LogError("Failed to load FBX file at path: " + destinationFilePath);
                return;
            }

            // Extract textures
            string assetPath = AssetDatabase.GetAssetPath(asset);
            ModelImporter modelImporter = AssetImporter.GetAtPath(assetPath) as ModelImporter;

            if (modelImporter == null)
            {
                Debug.LogError("Failed to get ModelImporter for asset at path: " + assetPath);
                return;
            }

            modelImporter.ExtractTextures(texturesDestinationPath);

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
                    // Apply extracted textures to the model
                    Renderer[] renderers = instantiatedModel.GetComponentsInChildren<Renderer>();
                    foreach (Renderer renderer in renderers)
                    {
                        foreach (Material mat in renderer.sharedMaterials)
                        {
                            if (mat != null)
                            {
                                string texturePath = Path.Combine(texturesDestinationPath, mat.name + ".png");
                                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
                                if (texture != null)
                                {
                                    mat.mainTexture = texture;
                                    EditorUtility.SetDirty(mat); // Mark the material as dirty
                                }
                            }
                        }
                    }

                    // Save changes to the asset
                    PrefabUtility.SaveAsPrefabAsset(instantiatedModel, assetPath);

                    Debug.Log("FBX imported and textures assigned successfully.");
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
    }
}
