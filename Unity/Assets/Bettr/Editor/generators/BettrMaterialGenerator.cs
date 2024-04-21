using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrMaterialGenerator
    {
        public static Material CreateOrLoadMaterial(string materialName, string shaderName, string textureName, string hexColor, string runtimeAssetPath)
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
                    var shader = Utils.LoadShader(shaderName, runtimeAssetPath);
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

            if (string.IsNullOrEmpty(hexColor))
            {
                string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "textures", textureName);
                var destPath = $"{runtimeAssetPath}/Textures/{textureName}";
                string extension = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = File.Exists($"{sourcePath}.jpg") ? ".jpg" : ".png";
                    sourcePath += extension;
                    destPath += extension;
                }
                Utils.ImportTexture2D( sourcePath, destPath);
                AssetDatabase.Refresh();
                Texture2D texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{InstanceComponent.RuntimeAssetPath}/Textures/{textureName}.jpg");
                if (texture == null)
                {
                    throw new Exception($"{textureName} texture not found.");
                }
                material.SetTexture(Utils.MainTex, texture);
            }
            else
            {
                Color color;
                if (ColorUtility.TryParseHtmlString(hexColor, out color))
                {
                    material.SetColor(Utils.Color, color);
                }
                else
                {
                    throw new Exception($"Invalid color {hexColor}.");
                }
            }

            AssetDatabase.Refresh();

            return material;
        }
    }
}