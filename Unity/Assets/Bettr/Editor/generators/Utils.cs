using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class Utils
    {
        public static readonly int MainTex = Shader.PropertyToID("_MainTex");
        
        public static readonly int Color = Shader.PropertyToID("_Color");
        
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
            Shader shader = null;
            if (shaderName.StartsWith("Bettr/"))
            {
                shaderName = shaderName.Substring(6);
                var shaderFilepath = $"{runtimeAssetPath}/Shaders/{shaderName}.shader";
                shader = AssetDatabase.LoadAssetAtPath<Shader>(shaderFilepath);
            }
            else
            {
                shader = Shader.Find(shaderName);
            }
            return shader;
        }
    }
}