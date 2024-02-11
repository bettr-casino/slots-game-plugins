using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bettr.Runtime.Plugin.Main.Editor
{
    public static class BettrMenu
    {
        [MenuItem("Bettr/Plugins/Main/Run Tests")] 
        public static void RunUnitTests()
        {
            // Get the ITestRunnerApi instance
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var playModeFilter = new Filter() { testMode = TestMode.PlayMode, groupNames = new []{ "^Bettr.Runtime.Plugin.Main.Tests$" }};

            // Run the tests
            testRunnerApi.Execute(new ExecutionSettings(playModeFilter));
        }

    }
}