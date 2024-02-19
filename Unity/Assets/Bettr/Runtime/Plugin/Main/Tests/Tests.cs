using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Bettr.Core;
using CrayonScript.Code;
using NUnit.Framework;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

// ReSharper disable once CheckNamespace
namespace Bettr.Runtime.Plugin.Main.Tests
{
    public class Tests
    {
        private const string MAIN_BUNDLE_NAME = "main";
        private const string MAIN_BUNDLE_VARIANT = "v0_1_0";
        private const string SERVER_BASE_URL = "https://bettr-casino-assets.s3.us-west-2.amazonaws.com";

        private IBettrAssetController _bettrAssetController;
        private IBettrAssetScriptsController _bettrAssetScriptsController;
        private IBettrUserController _bettrUserController;
        private IBettrVisualsController _bettrVisualsController;
        private IBettrOutcomeController _bettrOutcomeController;
        private IBettrServer _bettrServer;
        
        private Tile _tile;

        private bool _unitySetUpComplete = false;

        [OneTimeSetUp]
        public void OneTimeSetup()
        {
            var commandLineArguments = GetCommandLineArguments();
            if (commandLineArguments.TryGetValue("-timeScale", out string timeScaleStr))
            {
                if (int.TryParse(timeScaleStr, out int timeScale))
                {
                    if (timeScale > 0)
                    {
                        Debug.Log($"Setting Time.timeScale={timeScale}");
                        Time.timeScale = timeScale;
                    }
                    else
                    {
                        Debug.LogError("The 'timeScale' command line argument must be greater than 0.");
                    }
                }
                else
                {
                    // Handle the case where the string cannot be converted to an integer
                    Debug.LogError("The 'timeScale' command line argument is not a valid integer.");
                }
            }
            
            Debug.Log("OneTimeSetup");

            TileController.StaticInit();
            TileController.RegisterModule("casino.bettr.plugin.Core.dll");
            
            SceneManager.LoadScene("Bettr/Runtime/Plugin/Main/Tests/TestScene", LoadSceneMode.Single);
            
            BettrModel.Init();

            _bettrServer = new BettrServer(SERVER_BASE_URL);
            
            _bettrAssetController = new BettrAssetController();
            _bettrUserController = new BettrUserController(_bettrServer);

            _bettrVisualsController = new BettrVisualsController();
            _bettrAssetScriptsController = _bettrAssetController.BettrAssetScriptsController;
            _bettrOutcomeController = new BettrOutcomeController(_bettrAssetScriptsController, _bettrUserController, "fake-hash-key");

            _bettrVisualsController.SwitchOrientationToLandscape();
        }
        
        [UnitySetUp]
        public IEnumerator UnitySetUp()
        {
            Debug.Log("UnitySetUp");
            if (_unitySetUpComplete) yield break;
            yield return _bettrAssetController.LoadPackage(MAIN_BUNDLE_NAME, MAIN_BUNDLE_VARIANT, false);
            _unitySetUpComplete = true;
        }

        [UnityTest, Order(1)]
        public IEnumerator TestInit()
        {
            Debug.Log("TestLogin");
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("Init");
            ScriptRunner.Release(scriptRunner);
        
            Assert.AreEqual(30, Application.targetFrameRate);
        }
        
        [UnityTest, Order(2)]
        public IEnumerator TestLogin()
        {
            Debug.Log("TestLogin");
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("Login");
            ScriptRunner.Release(scriptRunner);
        }
        
        [UnityTest, Order(3)]
        public IEnumerator TestRoutineRunner()
        {
            Debug.Log("TestRoutineRunner");
            var instance = BettrRoutineRunner.Instance;
            Assert.IsNotNull(instance);

            var timeStart = Time.time;

            yield return instance.RunRoutine(WaitForSecondsForRoutineRunner(1, 3));
            
            var timeEnd = Time.time;
            
            var timeElapsed = timeEnd - timeStart;
            
            Debug.Log($"TestRoutineRunner timeElapsed={timeElapsed}");
            
            Assert.IsTrue(timeElapsed >= 3.0f);
        }
        
        [UnityTest, Order(4), Timeout(600000)]
        public IEnumerator TestLoadMainLobby()
        {
            Debug.Log("TestLoadMainLobby");
            
            for (int i = 0; i < 1; i++)
            {
                var mainTable = _bettrAssetScriptsController.GetScript("Main");
                var scriptRunner = ScriptRunner.Acquire(mainTable);
        
                yield return scriptRunner.CallAsyncAction("LoadLobbyScene");
                ScriptRunner.Release(scriptRunner);
                
                // wait 3 seconds and then verify MainLobby is configured with userdata
                yield return new WaitForSeconds(3.0f);
                
                var activeScene = SceneManager.GetActiveScene();
                Assert.IsTrue(activeScene.name.Equals("MainLobbyScene"));
                var allRootGameObjects = activeScene.GetRootGameObjects();
                var appGameObject = allRootGameObjects.First((o => o.name == "App"));
                var tmPros = appGameObject.GetComponentsInChildren<TextMeshPro>();
                var tmProUserLvl = tmPros.First((o => o.name == "UserLvlText"));
                Assert.IsTrue(tmProUserLvl.text == _bettrUserController.BettrUserConfig.Level.ToString());
                var tmProUserXp = tmPros.First((o => o.name == "UserXPText"));
                Assert.IsTrue(tmProUserXp.text == _bettrUserController.BettrUserConfig.XP.ToString());
                var tmProUserCoins = tmPros.First((o => o.name == "UserCoinsText"));
                Assert.IsTrue(tmProUserCoins.text == _bettrUserController.BettrUserConfig.Coins.ToString());
                
                var lobbyGameObject = allRootGameObjects.First((o => o.name == "Lobby"));
                var pointerClickHandlers = lobbyGameObject.GetComponentsInChildren<IPointerClickHandler>();
                
                var pointerClickHandler = pointerClickHandlers[i];
                
                // Create a new PointerEventData instance and set the desired properties.
                // For example, the button that was 'pressed'.
                PointerEventData data = new PointerEventData(EventSystem.current)
                {
                    button = PointerEventData.InputButton.Left
                };
                
                pointerClickHandler.OnPointerClick(data);
                LogAssert.Expect(LogType.Log, new Regex(".*OnPointerClick.*"));

                yield return WaitForGameSceneToLoad(3.0f);

                yield return WaitForSpinButtonToBeClicked(3.0f);

                yield return ClickSpinButton(i, 1);
                
                yield return WaitAndLoadTestOutcome(3.0f);
                
                yield return ClickSpinButton(i,2);
                
                yield return WaitAndLoadTestOutcome(3.0f);
                
                yield return ClickSpinButton(i, 3);
                
                yield return WaitAndLoadTestOutcome(3.0f, false);
                
                yield return RunFreeSpins(i);
                
                yield return new WaitForSeconds(3.0f);
                
                yield return ClickSpinButton(i, 3);
                
                yield return WaitAndLoadTestOutcome(3.0f, false);
                
                yield return RunFreeSpins(i);
                
                yield return new WaitForSeconds(3.0f);
                
                yield return ClickSpinButton(i, 4);
                
                yield return WaitAndLoadTestOutcome(3.0f);
                
                yield return new WaitForSeconds(3.0f);
            }
        }

        private IEnumerator WaitForSecondsForRoutineRunner(float duration, int count)
        {
            for (int i = 0; i < count; i++)
            {
                yield return new WaitForSeconds(duration);
            }
        }

        private IEnumerator WaitForGameSceneToLoad(float duration)
        {
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator WaitForSpinButtonToBeClicked(float duration)
        {
            // wait for the spin button to be clicked
            yield return new WaitForSeconds(duration);
        }

        private IEnumerator RunFreeSpins(int gameIndex)
        {
            yield return ClickFreeSpinsStartButton(gameIndex);

            while (HasNextFreeSpin(gameIndex))
            {
                var currentFreeSpin = GetCurrentFreeSpin(gameIndex);
                Debug.Log("Tests.cs RunFreeSpins currentFreeSpin=" + currentFreeSpin);
                yield return new WaitForSeconds(1.0f);
            }

            yield return new WaitForSeconds(30.0f);
        }

        private IEnumerator ClickSpinButton(int gameIndex, int outcomeID)
        {
            yield return WaitForSpinState("Waiting");
            
            _bettrOutcomeController.OutcomeNumber = outcomeID;
            
            // click spin button
            var settingsGameObject = GameObject.Find("Settings");
            var pointerClickHandlers = settingsGameObject.GetComponentsInChildren<IPointerClickHandler>();
            var pointerClickHandler = pointerClickHandlers[0];
            // Create a new PointerEventData instance and set the desired properties.
            // For example, the button that was 'pressed'.
            var data = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left
            };
            
            pointerClickHandler.OnPointerClick(data);
            LogAssert.Expect(LogType.Log, new Regex(".*OnPointerClick.*"));

            yield return WaitForSpinState("Spinning");
        }

        private IEnumerator ClickFreeSpinsStartButton(int gameIndex)
        {
            yield return WaitForGame("FreeSpins");
            
            var gameParent = GameObject.Find("Game");
            var gameMachineTile = gameParent.GetComponentsInChildren<Tile>().First( tile => tile.tileId == "Game001FreeSpinsMachine" );
            
            var currentSpinButtonState = gameMachineTile.CallFunction<string>("CurrentSpinButtonClickState");
            while (currentSpinButtonState != "IsWaiting")
            {
                yield return new WaitForSeconds(1.0f);
                currentSpinButtonState = gameMachineTile.CallFunction<string>("CurrentSpinButtonClickState");
            }
            
            // click spin button
            var spinButtonGameObject = GameObject.Find("SpinButton");
            while (!spinButtonGameObject.activeSelf)
            {
                yield return null;
            }

            var pointerClickHandlers = spinButtonGameObject.GetComponentsInChildren<IPointerClickHandler>();
            var pointerClickHandler = pointerClickHandlers[0];
            // Create a new PointerEventData instance and set the desired properties.
            // For example, the button that was 'pressed'.
            var data = new PointerEventData(EventSystem.current)
            {
                button = PointerEventData.InputButton.Left,
                
            };
            
            pointerClickHandler.OnPointerClick(data);
            LogAssert.Expect(LogType.Log, new Regex(".*OnPointerClick.*"));
            
            yield return new WaitForSeconds(6.0f);
        }

        private IEnumerator WaitAndLoadTestOutcome(float waitForSeconds, bool waitForWaitingState = true)
        {
            // wait for a few seconds
            yield return new WaitForSeconds(waitForSeconds);

            if (waitForWaitingState) yield return WaitForSpinState("Waiting");
        }

        private IEnumerator WaitForSpinState(string spinState)
        {
            var gameParent = GameObject.Find("Game");
            Tile baseGameMachineTile = null;
            while (baseGameMachineTile == null)
            {
                yield return new WaitForSeconds(1.0f);
                var tileComponents = gameParent.GetComponentsInChildren<Tile>();
                if (tileComponents != null)
                {
                    baseGameMachineTile = tileComponents.FirstOrDefault( tile => tile.tileId == "Game001BaseGameMachine" );
                }
            }
            
            var currentSpinState = baseGameMachineTile.CallFunction<string>("CurrentSpinState");
            while (currentSpinState != spinState)
            {
                yield return new WaitForSeconds(1.0f);
                currentSpinState = baseGameMachineTile.CallFunction<string>("CurrentSpinState");
            }
        }
        
        private IEnumerator WaitForGame(string activeGame)
        {
            var gameParent = GameObject.Find("Game");
            var sceneTile = gameParent.GetComponentsInChildren<Tile>().First( tile => tile.tileId == "Game001" );
            
            var currentActiveGame = sceneTile.CallFunction<string>("ActiveGame");
            while (currentActiveGame != activeGame)
            {
                yield return new WaitForSeconds(1.0f);
                currentActiveGame = sceneTile.CallFunction<string>("ActiveGame");
            }
        }

        private bool HasNextFreeSpin(int gameIndex)
        {
            var gameParent = GameObject.Find("Game");
            var gameMachineTile = gameParent.GetComponentsInChildren<Tile>().First( tile => tile.tileId == "Game001FreeSpinsMachine" );
            
            var hasNextFreeSpin = gameMachineTile.CallFunction<bool>("HasNextFreeSpin");
            return hasNextFreeSpin;
        }
        
        private int GetCurrentFreeSpin(int gameIndex)
        {
            var gameParent = GameObject.Find("Game");
            var gameMachineTile = gameParent.GetComponentsInChildren<Tile>().First( tile => tile.tileId == "Game001FreeSpinsMachine" );
            
            var currentFreeSpin = gameMachineTile.CallFunction<int>("GetCurrentFreeSpin");
            return currentFreeSpin;
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
        
        public string GetEnvironmentVariable(string key)
        {
            return Environment.GetEnvironmentVariable(key);
        }
        
        public static Dictionary<string, string> GetCommandLineArguments()
        {
            string[] args = Environment.GetCommandLineArgs();
            Debug.Log("GetCommandLineArguments args=" + string.Join(",", args));
            Dictionary<string, string> arguments = new Dictionary<string, string>();

            foreach (string argument in args)
            {
                string[] split = argument.Split('=');
                if (split.Length == 2)
                {
                    arguments[split[0]] = split[1];
                }
            }

            return arguments;
        }
    }
}