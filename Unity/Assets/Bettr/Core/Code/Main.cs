using System;
using System.Collections;
using System.Linq;
using CrayonScript.Code;
using UnityEngine;
using UnityEngine.SceneManagement;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class Main : MonoBehaviour
    {
        [SerializeField] private TextAsset configFile;

        [NonSerialized] private ConfigData _configData;
        [NonSerialized] private IBettrServer _bettrServer;
        [NonSerialized] private IBettrAssetController _bettrAssetController;
        [NonSerialized] private IBettrAssetScriptsController _bettrAssetScriptsController;
        [NonSerialized] private IBettrUserController _bettrUserController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private IBettrVisualsController _bettrVisualsController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private IBettrOutcomeController _bettrOutcomeController;
        // ReSharper disable once NotAccessedField.Local
        [NonSerialized] private IBettrAudioController _bettrAudioController;
        
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

            yield return LoadUserBlob();
            
            yield return LoadMainLobby();

            yield return new WaitForSeconds(3.0f);
            
            DevTools.Instance.Enable();
            yield return DevTools.Instance.CaptureSceneState();
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
            
            _bettrServer = new BettrServer(_configData.ServerURL);
            
            _bettrUserController = new BettrUserController(_bettrServer);
            
            _bettrAssetController = new BettrAssetController
            {
                webAssetBaseURL = _configData.WebAssetsBaseURL,
                useFileSystemAssetBundles = _configData.UseFileSystemAssetBundles,
            };
            
            _bettrVisualsController = new BettrVisualsController();
            
            _bettrAssetScriptsController = _bettrAssetController.BettrAssetScriptsController;
            
            var userId = _bettrUserController.GetUserId();
            
            var assetVersion = "latest";
            
            // yield return _bettrServer.Get($"/commit_hash.txt", (url, payload, success, error) =>
            // {
            //     if (!success)
            //     {
            //         Debug.LogError($"User JSON retrieved Success: url={url} error={error}");
            //         return;
            //     }
            //     
            //     if (payload.Length == 0)
            //     {
            //         Debug.LogError("empty payload retrieved from url={url}");
            //         return;
            //     }
            //     
            //     assetVersion = System.Text.Encoding.UTF8.GetString(payload);
            //     
            // });

            // if (String.IsNullOrWhiteSpace(assetVersion))
            // {
            //     Debug.LogError($"Unable to retrieve commit_hash for user url={userId}");
            //     yield break;
            // }
            
            _configData.AssetsVersion = assetVersion;
            
            Debug.Log($"userId={userId} AssetsVersion={_configData.AssetsVersion} AssetsBaseURL={_configData.AssetsBaseURL} WebAssetsBaseURL={_configData.WebAssetsBaseURL} WebOutcomesBaseURL={_configData.WebOutcomesBaseURL} MainBundleName={_configData.MainBundleName} MainBundleVariant={_configData.MainBundleVariant}");
            
            BettrModel.Init();

            _bettrOutcomeController = new BettrOutcomeController(_bettrAssetScriptsController, _bettrUserController, _configData.AssetsVersion)
                {
                    UseFileSystemOutcomes = _configData.UseFileSystemOutcomes,
                    WebOutcomesBaseURL = _configData.WebOutcomesBaseURL,
                };

            _bettrAudioController = new BettrAudioController();

            _bettrVisualsController.SwitchOrientationToLandscape();
            
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
        
        private IEnumerator LoadUserBlob()
        {
            yield return _bettrUserController.LoadUserBlob();
            TileController.AddToGlobals("BettrUser", _bettrUserController.BettrUserConfig);
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




