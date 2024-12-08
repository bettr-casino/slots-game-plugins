using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrMaterialGenerator
    {
        public static string MachineName;
        
        public static string MachineVariant;
        
        public static Material CreateOrLoadMaterial(string materialName, string shaderName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var materialFilename = $"{materialName}.mat";
            var materialFilepath = $"{runtimeAssetPath}/Materials/{materialFilename}";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            if (material == null)
            {
                Debug.Log($"Creating material for {materialName} at {materialFilepath}");
                try
                {
                    material = new Material(Shader.Find(shaderName));
                    AssetDatabase.CreateAsset(material, materialFilepath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);

            return material;
        }

        public static Material CreateOrLoadMaterial(string materialName, string shaderName, string textureName,
            string hexColor, float alpha, string runtimeAssetPath)
        {
            return CreateOrLoadMaterial(materialName, shaderName, textureName, hexColor, alpha, runtimeAssetPath, false);
        }
        
        public static Material CreateOrLoadMaterial(string materialName, string shaderName, string textureName, string hexColor, float alpha, string runtimeAssetPath, bool createTextureIfNotExists = false)
        {
            AssetDatabase.Refresh();
            
            var shader = LoadShader(shaderName, runtimeAssetPath);
            var materialFilename = $"{materialName}.mat";
            var materialFilepath = $"{runtimeAssetPath}/Materials/{materialFilename}";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            if (material == null)
            {
                Debug.Log($"Creating material for {materialName} at {materialFilepath}");
                try
                {
                    material = new Material(shader);
                    AssetDatabase.CreateAsset(material, materialFilepath);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                    throw new Exception($"Shader {shaderName} not found.", e);
                }
            }
            
            AssetDatabase.Refresh();
            
            material = AssetDatabase.LoadAssetAtPath<Material>(materialFilepath);
            material.shader = shader;

            if (!string.IsNullOrEmpty(textureName))
            {
                string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "textures", MachineName, MachineVariant, textureName);
                var destPath = $"{runtimeAssetPath}/Textures/{textureName}";
                string extension = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = File.Exists($"{sourcePath}.jpg") ? ".jpg" : ".png";
                    sourcePath += extension;
                    destPath += extension;
                }
                // check if its in the path
                if (!File.Exists(sourcePath))
                {
                    if (createTextureIfNotExists)
                    {
                        Debug.Log($"Creating texture for {textureName} at {sourcePath}");
                        Texture2D newTexture = new Texture2D(1, 1);
                        byte[] bytes = File.ReadAllBytes(Path.Combine("Assets", "Bettr", "Editor", "textures", "default.png"));
                        newTexture.LoadImage(bytes);
                        File.WriteAllBytes(sourcePath, newTexture.EncodeToPNG());
                        
                        // add to the asset database
                        AssetDatabase.Refresh();
                    }
                    else
                    {
                        throw new Exception($"{textureName} texture not found.");
                    }
                }
                
                ImportTexture2D( sourcePath, destPath);
                AssetDatabase.Refresh();
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>(destPath);
                if (texture == null)
                {
                    throw new Exception($"{textureName} texture not found.");
                }
                material.SetTexture((int) MainTex, texture);
            }
            if (!string.IsNullOrEmpty(hexColor))
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(hexColor, out color))
                {
                    material.SetColor((int) Color, color);
                }
                else
                {
                    throw new Exception($"Invalid color {hexColor}.");
                }
            }
            if (alpha >= 0)
            {
                if (!string.IsNullOrEmpty(textureName) || !string.IsNullOrEmpty(hexColor))
                {
                    if (material.HasColor((int) Color))
                    {
                        Color color = material.GetColor((int) Color);
                        color.a = alpha;
                        material.SetColor((int) Color, color);
                    }
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            return material;
        }
        
        public static void ImportTexture2D(string sourcePath, string destPath, TextureImporterType textureImporterType = TextureImporterType.Sprite)
        {
            File.Copy(sourcePath, destPath, overwrite: true);
            // Import the copied image file as a Texture2D asset
            AssetDatabase.ImportAsset(destPath, ImportAssetOptions.ForceUpdate);
            TextureImporter textureImporter = AssetImporter.GetAtPath(destPath) as TextureImporter;
            if (textureImporter != null)
            {
                textureImporter.textureType = textureImporterType;
                textureImporter.mipmapEnabled = false;
                textureImporter.isReadable = true;
                textureImporter.SaveAndReimport();
            }
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
        }
        
        public static Shader LoadShader(string shaderName, string runtimeAssetPath)
        {
            Shader shader = Shader.Find(shaderName);
            return shader;
        }

        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        public static readonly int Color = Shader.PropertyToID("_Color");
    }
}