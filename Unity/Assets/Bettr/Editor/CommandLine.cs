using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Bettr.Editor
{
    public class CommandLine
    {
        public static void BuildIOS()
        {
            Debug.Log("BuildIOS...");
                
            // Get command line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Find the index of the 'buildOutput' argument
            int buildOutputIndex = Array.IndexOf(args, "-buildOutput") + 1;
            if (buildOutputIndex <= 0 || buildOutputIndex >= args.Length)
            {
                throw new ArgumentException("Build output path not specified in command line arguments.");
            }

            // Get the build output path from the command line arguments
            string buildDirectory = args[buildOutputIndex];
            string buildPath = Path.Combine(buildDirectory, "BettrSlots");

            if (Directory.Exists(buildPath))
            {
                Debug.Log("Removing existing build directory: " + buildPath);
                Directory.Delete(buildPath, true);  // true for recursive delete
            }

            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }

            // Set the build settings
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = buildPath, // Specify the build name here
                target = BuildTarget.iOS,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            // Perform the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Check the report for success
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }

        public static void BuildWebGL()
        {
            Debug.Log("BuildWebGL...");
                
            // Get command line arguments
            string[] args = Environment.GetCommandLineArgs();

            // Find the index of the 'buildOutput' argument
            int buildOutputIndex = Array.IndexOf(args, "-buildOutput") + 1;
            if (buildOutputIndex <= 0 || buildOutputIndex >= args.Length)
            {
                throw new ArgumentException("Build output path not specified in command line arguments.");
            }

            // Get the build output path from the command line arguments
            string buildDirectory = args[buildOutputIndex];
            string buildPath = Path.Combine(buildDirectory, "BettrSlots");

            if (Directory.Exists(buildPath))
            {
                Debug.Log("Removing existing build directory: " + buildPath);
                Directory.Delete(buildPath, true);  // true for recursive delete
            }

            if (!Directory.Exists(buildDirectory))
            {
                Directory.CreateDirectory(buildDirectory);
            }

            // Set the build settings
            BuildPlayerOptions buildPlayerOptions = new BuildPlayerOptions
            {
                scenes = EditorBuildSettings.scenes.Where(scene => scene.enabled).Select(scene => scene.path).ToArray(),
                locationPathName = buildPath, // Specify the build name here
                target = BuildTarget.WebGL,
                options = BuildOptions.Development | BuildOptions.AllowDebugging
            };

            // Perform the build
            BuildReport report = BuildPipeline.BuildPlayer(buildPlayerOptions);
            
            // Check the report for success
            if (report.summary.result == BuildResult.Succeeded)
            {
                Debug.Log("Build succeeded: " + report.summary.totalSize + " bytes");
            }
            else if (report.summary.result == BuildResult.Failed)
            {
                Debug.LogError("Build failed");
            }
        }
    }
}

