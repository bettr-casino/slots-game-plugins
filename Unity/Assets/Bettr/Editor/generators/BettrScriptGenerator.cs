using System;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrScriptGenerator
    {
        public static TextAsset CreateOrLoadScript(string name, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            
            var scriptName = $"{name}.cscript.txt";
            var scriptPath = $"{runtimeAssetPath}/Scripts/{scriptName}";
            var script = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath);
            if (script == null)
            {
                Debug.Log($"Creating script for {name} at {scriptPath}");
                try
                {
                    var defaultScriptContentPath = "Assets/Bettr/Editor/DefaultCScript.cscript.txt"; // Adjust the path as needed
                    var defaultScriptTemplateContent = File.ReadAllText(defaultScriptContentPath);
                    var defaultScriptContent = string.Format(defaultScriptTemplateContent, name);
                    File.WriteAllText(scriptPath, defaultScriptContent);
                }
                catch (Exception e)
                {
                    Debug.LogError(e);
                }
            }
            
            AssetDatabase.Refresh();
            
            script = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath);

            return script;
        }
    }
}