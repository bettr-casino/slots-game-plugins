using UnityEditor;
using UnityEngine;

namespace Bettr.Editor.generators
{
    public static class BettrScriptGenerator
    {
        public static TextAsset CreateOrLoadScript(string machineName, string runtimeAssetPath)
        {
            AssetDatabase.Refresh();
            var filename = $"{machineName}.cscript.txt";
            var scriptPath = $"{runtimeAssetPath}/Scripts/{filename}";
            var script = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptPath);
            return script;
        }
    }
}