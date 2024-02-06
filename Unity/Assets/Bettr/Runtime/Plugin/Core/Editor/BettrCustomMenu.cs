using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

namespace Bettr.Runtime.Plugin.Core.Editor
{
    public static class BettrCustomMenu
    {
        [MenuItem("Bettr/Plugins/Core/Run Unit Tests")] 
        public static void RunUnitTests()
        {
            // Get the ITestRunnerApi instance
            var testRunnerApi = ScriptableObject.CreateInstance<TestRunnerApi>();
            
            var playModeFilter = new Filter() { testMode = TestMode.PlayMode, groupNames = new []{ "^Bettr.Runtime.Plugin.Core.Tests$" }};

            // Run the tests
            testRunnerApi.Execute(new ExecutionSettings(playModeFilter));
        }
    }
}