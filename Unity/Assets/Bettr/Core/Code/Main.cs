using System;
using System.Collections;
using System.Linq;
using CrayonScript.Code;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class Main : MonoBehaviour
    {
        [SerializeField] private TextAsset configFile;

        [NonSerialized] private ConfigData _configData;
        [NonSerialized] private BettrServer _bettrServer;
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetScriptsController _bettrAssetScriptsController;
        [NonSerialized] private BettrUserController _bettrUserController;
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

            yield return LoginUser();

            yield return LoadMainLobby();

            yield return new WaitForSeconds(3.0f);
            
            DevTools.Instance.Enable();
            
            DevTools.Instance.OnKeyPressed.AddListener(() =>
            {
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
        }

        private IEnumerator OneTimeSetup()
        {
            if (_oneTimeSetUpComplete) yield break;
            
            Debug.Log("OneTimeSetup started");

            TileController.StaticInit();
            TileController.RegisterModule("Bettr.dll");
            TileController.RegisterModule("casino.bettr.plugin.Core.dll");
            
            // load the config file
            _configData = ConfigReader.Parse(configFile.text);

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
            
            var userId = _bettrUserController.GetUserId();
            
            var assetVersion = "latest";

            _configData.AssetsVersion = assetVersion;
            
            Debug.Log($"userId={userId} AssetsVersion={_configData.AssetsVersion} AssetsBaseURL={_configData.AssetsServerBaseURL} WebAssetsBaseURL={_configData.WebAssetsBaseURL} WebOutcomesBaseURL={_configData.WebOutcomesBaseURL} MainBundleName={_configData.MainBundleName} MainBundleVariant={_configData.MainBundleVariant}");
            
            BettrModel.Init();

            _bettrAssetController = new BettrAssetController
            {
                webAssetBaseURL = _configData.WebAssetsBaseURL,
                useFileSystemAssetBundles = _configData.UseFileSystemAssetBundles,
            };
            
            _bettrVisualsController = new BettrVisualsController();
            
            _bettrAssetScriptsController = _bettrAssetController.BettrAssetScriptsController;
            
            _bettrOutcomeController = new BettrOutcomeController(_bettrAssetScriptsController, _bettrUserController, _configData.AssetsVersion)
                {
                    WebOutcomesBaseURL = _configData.WebOutcomesBaseURL,
                    UseFileSystemOutcomes = _configData.UseFileSystemOutcomes,
                };

            _bettrAudioController = new BettrAudioController();

            BettrVisualsController.SwitchOrientationToPortrait();
            
            if (_oneTimeSetUpComplete) yield break;
            yield return _bettrAssetController.LoadPackage(_configData.MainBundleName, _configData.MainBundleVariant, false);
            
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
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("Login");
            ScriptRunner.Release(scriptRunner);
        }

        private IEnumerator LoadMainLobby()
        {
            var mainTable = _bettrAssetScriptsController.GetScript("Main");
            var scriptRunner = ScriptRunner.Acquire(mainTable);
            yield return scriptRunner.CallAsyncAction("LoadLobbyScene");
            ScriptRunner.Release(scriptRunner);

            yield return UpdateCommitHash();
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
    }
}




