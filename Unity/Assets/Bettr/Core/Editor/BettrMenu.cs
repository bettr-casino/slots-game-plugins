using UnityEditor;
using UnityEditor.TestTools.TestRunner.Api;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Runtime.Plugin.Core.Editor
{
    public static class BettrMenu
    {
        //[MenuItem("Bettr/Plugins/Core/Run Tests")] 
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