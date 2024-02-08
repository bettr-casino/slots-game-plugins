using System.Collections;
using System.Numerics;
using Bettr.Core;
using CrayonScript.Code;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;
using Quaternion = UnityEngine.Quaternion;
using Vector3 = UnityEngine.Vector3;

// ReSharper disable once CheckNamespace
namespace Bettr.Runtime.Plugin.Core.Tests
{
    public class Tests
    {
        private Tile _tile;
        
        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            Debug.Log("OneTimeSetUp");
            // AddCameraToScene();
            // AddEventSystemToScene();
            Application.targetFrameRate = 30;
            RenderSettings.fog = false;
            RenderSettings.skybox = null;
            RenderSettings.sun = null;
            
            SceneManager.LoadScene("Bettr/Core/Tests/TestScene", LoadSceneMode.Single);
        }

        [UnityTest, Order(1)]
        public IEnumerator TestDisplayCounters()
        {
            Debug.Log("Start TestDisplayCounters");
            
            var displayCountersGo = new GameObject("DisplayCounters");
            var displayCounters = displayCountersGo.AddComponent<BettrDisplayCounters>();
            
            var textGo1 = new GameObject("TextGameObject1")
            {
                transform =
                {
                    position = new Vector3(0, 2, 0)
                }
            };
            var textMeshPro1 = AddTextComponent(textGo1);
            displayCounters.AddCounter("Counter1", textMeshPro1, 0, 10000, 18, 1);
            
            var textGo2 = new GameObject("TextGameObject2")
            {
                transform =
                {
                    position = new Vector3(0, -2, 0)
                }
            };
            var textMeshPro2 = AddTextComponent(textGo2);
            displayCounters.AddCounter("Counter2", textMeshPro2, 10000, 0, 18,-1);
            
            yield return new WaitForSeconds(5.0f);
            
            displayCounters.PauseCounter("Counter1");
            displayCounters.PauseCounter("Counter2");
            
            yield return null;
            
            Object.Destroy(displayCountersGo);
            Object.Destroy(textGo1);
            Object.Destroy(textGo2);
        }

        private void AddCameraToScene()
        {
            var cameraGo = new GameObject("Main Camera");
            var camera = cameraGo.AddComponent<Camera>();
            camera.orthographic = true;
            
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = Color.black;
            cameraGo.transform.position = new Vector3(0, 0, -10);
            cameraGo.transform.rotation = Quaternion.identity;
        }
        
        private void AddEventSystemToScene()
        {
            var eventSystemGo = new GameObject("EventSystem");
            eventSystemGo.AddComponent<EventSystem>();
            eventSystemGo.AddComponent<StandaloneInputModule>();
        }

        private TextMeshPro AddTextComponent(GameObject displayCountersGo)
        {
            var tc = displayCountersGo.AddComponent<TextMeshPro>();
            tc.enableAutoSizing = true;
            tc.fontSizeMax = 12;
            tc.horizontalAlignment = HorizontalAlignmentOptions.Center;
            tc.verticalAlignment = VerticalAlignmentOptions.Middle;
            
            BigInteger displayNumber = new BigInteger(0);
            int fixedDigits = 12;
            
            tc.text = displayNumber.ToString("D" + fixedDigits);

            return tc;
        }
    }
}