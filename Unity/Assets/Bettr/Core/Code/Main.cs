using System;
using System.Collections;
using System.Linq;
using System.Text.RegularExpressions;
using CrayonScript.Code;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class Main : MonoBehaviour
    {
        [SerializeField] private TextAsset unityEditorConfigFile;
        [SerializeField] private TextAsset iOSConfigFile;
        [SerializeField] private TextAsset androidConfigFile;
        [SerializeField] private TextAsset webGLConfigFile;
        [SerializeField] private TextAsset macOSConfigFile;
        [SerializeField] private TextAsset windowsConfigFile;    

        [NonSerialized] private ConfigData _configData;
        [NonSerialized] private BettrServer _bettrServer;
        [NonSerialized] private BettrMainLobbySceneController _bettrMainLobbySceneController;
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetScriptsController _bettrAssetScriptsController;
        [NonSerialized] private BettrUserController _bettrUserController;
        [NonSerialized] private BettrExperimentController _bettrExperimentController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private BettrVisualsController _bettrVisualsController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private BettrOutcomeController _bettrOutcomeController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private BettrAudioController _bettrAudioController;

        private bool _oneTimeSetUpComplete;

        public void StartApp()
        {
            StartCoroutine(StartAppAsync());
        }

        // Start is called before the first frame update
        public IEnumerator StartAppAsync()
        {
            yield return OneTimeSetup();

            DevTools.Instance.Enable();
            
            DevTools.Instance.OnKeyPressed.AddListener(() =>
            {
                // Check for Backspace or Delete key press
                if (Input.GetKeyDown(KeyCode.P))
                {
                    StartCoroutine(LoadPreviousMachine());
                    return;
                }
                if (Input.GetKeyDown(KeyCode.N))
                {
                    StartCoroutine(LoadNextMachine());
                    return;
                }
                if (Input.GetKeyDown(KeyCode.L))
                {
                    StartCoroutine(LoadMainLobby());
                    return;
                }
                if (Input.GetKeyDown(KeyCode.V))
                {
                    TurnOffVolume();
                    return;
                }
                // Find the SpinImage GameObject
                var spinImage = GameObject.Find("SpinImage");
                if (spinImage != null)
                {
                    _bettrOutcomeController.OutcomeNumber = DevTools.Instance.ValidCombination;
                    
                    // Use Unity's EventSystem to simulate a click event
                    var eventData = new PointerEventData(EventSystem.current);
                    ExecuteEvents.Execute(spinImage, eventData, ExecuteEvents.pointerClickHandler);
                }
                else
                {
                    Debug.LogWarning("SpinImage GameObject not found.");
                }
            });
            
            yield return LoadMachine();
        }

        private IEnumerator OneTimeSetup()
        {
            if (_oneTimeSetUpComplete) yield break;
            
            Debug.Log("OneTimeSetup started");
            
            TileController.StaticInit();
            TileController.RegisterModule("Bettr.dll");
            TileController.RegisterModule("casino.bettr.plugin.Core.dll");

            _configData = null;
#if UNITY_EDITOR_OSX || UNITY_EDITOR_WIN || UNITY_EDITOR_LINUX || UNITY_EDITOR
            _configData = ConfigReader.Parse(unityEditorConfigFile.text);
#elif UNITY_IOS
            _configData = ConfigReader.Parse(iOSConfigFile.text);
#elif UNITY_ANDROID
            _configData = ConfigReader.Parse(androidConfigFile.text);
#elif UNITY_WEBGL
            _configData = ConfigReader.Parse(webGLConfigFile.text);
#elif UNITY_STANDALONE_OSX
            _configData = ConfigReader.Parse(macOSConfigFile.text);
#elif UNITY_STANDALONE_WIN
            _configData = ConfigReader.Parse(windowsConfigFile.text);
#endif
            // throw an error if the config data is not set
            if (_configData == null)
            {
                Debug.LogError("Config data is not set.");
                yield break;
            }

            _bettrServer = new BettrServer()
            {
                useLocalServer = _configData.UseLocalServer,
                configData = _configData,
            };

            _bettrUserController = new BettrUserController()
            {
                bettrServer = _bettrServer,
                configData = _configData,
            };
            
            _bettrExperimentController = new BettrExperimentController()
            {
                bettrServer = _bettrServer,
                configData = _configData,
            };
            
            var userId = _bettrUserController.GetUserId();
            
            var assetVersion = "latest";

            _configData.AssetsVersion = assetVersion;
            
            Debug.Log($"userId={userId} AssetsVersion={_configData.AssetsVersion} AssetsBaseURL={_configData.AssetsServerBaseURL} WebAssetsBaseURL={_configData.WebAssetsBaseURL} WebOutcomesBaseURL={_configData.WebOutcomesBaseURL}");
            
            BettrModel.Init();

            _bettrMainLobbySceneController = new BettrMainLobbySceneController(_bettrExperimentController);

            _bettrAssetController = new BettrAssetController
            {
                webAssetBaseURL = _configData.WebAssetsBaseURL,
                useFileSystemAssetBundles = _configData.UseFileSystemAssetBundles,
            };
            
            _bettrVisualsController = new BettrVisualsController();
            
            _bettrAssetScriptsController = _bettrAssetController.BettrAssetScriptsController;
            
            _bettrOutcomeController = new BettrOutcomeController(_bettrAssetScriptsController, _bettrUserController, _bettrExperimentController, _configData.AssetsVersion)
                {
                    WebOutcomesBaseURL = _configData.WebOutcomesBaseURL,
                    UseFileSystemOutcomes = _configData.UseFileSystemOutcomes,
                };

            _bettrAudioController = new BettrAudioController();
            
            // Register the specialized performance controllers
            TileController.RegisterType<BettrReelController>("BettrReelController");

            BettrVisualsController.SwitchOrientationToLandscape();
            
            if (_oneTimeSetUpComplete) yield break;
            
            yield return LoginUser();

            var bettrUser = BettrUserController.Instance.BettrUserConfig;
            
            yield return _bettrExperimentController.GetUserExperiments();
            
            yield return _bettrAssetController.LoadPackage(bettrUser.Main.BundleName, bettrUser.Main.BundleVersion, false);
            
            ScriptRunner.Initialize();
            
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("Init");
            ScriptRunner.Release(scriptRunner);
            
            DontDestroyOnLoad(gameObject);
            
            Debug.Log("OneTimeSetup ended");
            
            _oneTimeSetUpComplete = true;
        }

        private IEnumerator LoginUser()
        {
            yield return BettrUserController.Instance.Login();
        }

        private IEnumerator LoadMachine()
        {
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("LoadMachine");
            ScriptRunner.Release(scriptRunner);

            // yield return UpdateCommitHash();
        }
        
        private IEnumerator LoadNextMachine()
        {
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("LoadNextMachine");
            ScriptRunner.Release(scriptRunner);

            // yield return UpdateCommitHash();
        }
        
        private IEnumerator LoadPreviousMachine()
        {
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("LoadPreviousMachine");
            ScriptRunner.Release(scriptRunner);

            // yield return UpdateCommitHash();
        }
        
        private IEnumerator LoadMainLobby()
        {
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("LoadMainLobby");
            ScriptRunner.Release(scriptRunner);

            // yield return UpdateCommitHash();
        }

        private void TurnOffVolume()
        {
            _bettrAudioController.IsVolumeMuted = !_bettrAudioController.IsVolumeMuted;
        }
        
        private IEnumerator UpdateCommitHash()
        {
            var activeScene = SceneManager.GetActiveScene();
            while (activeScene.name != "MainLobbyScene")
            {
                yield return null;
                activeScene = SceneManager.GetActiveScene();
            }
            
            var allRootGameObjects = activeScene.GetRootGameObjects();
            var appGameObject = allRootGameObjects.First((o => o.name == "App"));
            var appTile = appGameObject.GetComponent<Tile>();
            appTile.Call("SetCommitHash", _configData.AssetsVersion);
        }
        
        public IEnumerator BackToLobby()
        {
            var mainLobbySceneName = "MainLobbyScene";
            var activeSceneName = SceneManager.GetActiveScene().name;
            if (activeSceneName == mainLobbySceneName)
            {
                yield break; // Do nothing and exit the coroutine
            }
            const string gameScenePattern = @"^Game\d{3}Scene$";
            if (!Regex.IsMatch(activeSceneName, gameScenePattern))
            {
                yield break; // Exit the coroutine early if it doesn't match
            }
            AsyncOperation loadNewSceneOperation = SceneManager.LoadSceneAsync(mainLobbySceneName, LoadSceneMode.Single);
            while (!loadNewSceneOperation.isDone)
            {
                yield return null; // Wait for the next frame
            }
            BettrVisualsController.SwitchOrientationToPortrait();
        }
    }
}




