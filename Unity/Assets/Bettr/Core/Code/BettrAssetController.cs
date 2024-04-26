using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Object = UnityEngine.Object;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void AssetLoadCompleteCallback(string assetBundleName, string assetBundleVersion,
        AssetBundle assetBundle, BettrAssetBundleManifest assetBundleManifest, bool success,
        bool previouslyLoaded, string error);

    public delegate void AssetBundleLoadCompleteCallback(string assetBundleName, AssetBundle assetBundle, bool success,
        bool previouslyLoaded, string error);

    public delegate void AssetBundleManifestLoadCompleteCallback(string assetName,
        BettrAssetBundleManifest assetBundleManifest, bool success,
        string error);


    public delegate void AssetClearCompleteCallback(string assetBundleName, string assetBundleVersion,
        BettrAssetBundleManifest assetBundleManifest, bool success, string error);

    [Serializable]
    public class BettrAssetPackageController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetScriptsController _bettrAssetScriptsController;

        public BettrAssetPackageController(
            BettrAssetController bettrAssetController,
            BettrAssetScriptsController bettrAssetScriptsController)
        {
            // TileController.RegisterType<BettrAssetPackageController>("BettrAssetPackageController");
            // TileController.AddToGlobals("BettrAssetPackageController", this);

            _bettrAssetController = bettrAssetController;
            _bettrAssetScriptsController = bettrAssetScriptsController;
        }

        public IEnumerator LoadPackage(string packageName, string packageVersion, bool loadScenes)
        {
            var baseBundleName = $"{packageName}";
            var scenesBundleName = $"{packageName}_scenes";
            
            yield return _bettrAssetController.LoadAssetBundle(baseBundleName, packageVersion,
                (name, version, bundle, bundleManifest, success, previouslyLoaded, error) =>
                {
                    if (!success)
                    {
                        Debug.LogError(
                            $"Failed to load asset bundle={baseBundleName} version={packageVersion}: {error}");
                        return;
                    }

                    if (!previouslyLoaded)
                    {
                        // preload and cache the scripts
                        _bettrAssetScriptsController.AddScripts(bundleManifest.Assets, bundle);
                    }
                    
                });

            if (loadScenes)
            {
                yield return _bettrAssetController.LoadAssetBundle(scenesBundleName, packageVersion,
                    (name, version, bundle, manifest, success, loaded, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError(
                                $"Failed to load asset bundle={scenesBundleName} version={packageVersion}: {error}");
                            return;
                        }
                    });
            }
            
        }
    }

    [Serializable]
    public class BettrAssetPrefabsController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetPackageController _bettrAssetPackageController;

        public BettrAssetPrefabsController(
            BettrAssetController bettrAssetController,
            BettrAssetPackageController bettrAssetPackageController)
        {
            // TileController.RegisterType<BettrAssetPrefabsController>("BettrPrefabsController");
            // TileController.AddToGlobals("BettrPrefabsController", this);

            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }
        
        public IEnumerator ReplacePrefab(string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject replaced)
        {
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetCachedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
            var prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to load prefab={prefabName} from asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }
            
            // Instantiate the new GameObject at the same position and rotation as the original
            GameObject newGameObject = Object.Instantiate(prefab, replaced.transform.position, replaced.transform.rotation);

            // If you want to maintain the same parent for the new GameObject
            newGameObject.transform.SetParent(replaced.transform.parent);
            
            newGameObject.transform.localScale = replaced.transform.localScale;

            // Destroy the original GameObject
            Object.Destroy(replaced);
        }

        public IEnumerator LoadPrefab(string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject parent = null)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                Debug.LogError($"Prefab name is null or empty for asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }
            
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetCachedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
            var prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to load prefab={prefabName} from asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }

            Object.Instantiate(prefab, parent == null ? null : parent.transform);
        }
    }
    
    [Serializable]
    public class BettrAssetMaterialsController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetPackageController _bettrAssetPackageController;

        public BettrAssetMaterialsController(
            BettrAssetController bettrAssetController,
            BettrAssetPackageController bettrAssetPackageController)
        {
            // TileController.RegisterType<BettrAssetPrefabsController>("BettrPrefabsController");
            // TileController.AddToGlobals("BettrPrefabsController", this);

            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }

        public IEnumerator LoadMaterial(string bettrAssetBundleName, string bettrAssetBundleVersion, string materialName,
            GameObject targetGameObject)
        {
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetCachedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
            var material = assetBundle.LoadAsset<Material>(materialName);
            if (material == null)
            {
                Debug.LogError(
                    $"Failed to load material={materialName} from asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }

            if (targetGameObject != null)
            {
                var meshRenderer = targetGameObject.GetComponent<MeshRenderer>();
                if (meshRenderer != null)
                {
                    meshRenderer.material = material;
                }
                else
                {
                    Debug.LogError("Target GameObject does not have a MeshRenderer component.");
                }
            }
            else
            {
                Debug.LogError("Target GameObject is null.");
            }
        }
    }

    [Serializable]
    public class BettrAssetScenesController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetPackageController _bettrAssetPackageController;

        public BettrAssetScenesController(
            BettrAssetController bettrAssetController,
            BettrAssetPackageController bettrAssetPackageController)
        {
            // TileController.RegisterType<BettrAssetScenesController>("BettrAssetScenesController");
            // TileController.AddToGlobals("BettrAssetScenesController", this);

            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }

        public IEnumerator LoadScene(string bettrAssetBundleName, string bettrAssetBundleVersion, string bettrSceneName)
        {
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, true);
            
            var scenesBundleName = $"{bettrAssetBundleName}_scenes";
            var scenesBundleVersion = $"{bettrAssetBundleVersion}";
            
            var assetBundle = _bettrAssetController.GetCachedAssetBundle(scenesBundleName, scenesBundleVersion);
            
            var allScenePaths = assetBundle.GetAllScenePaths();
            var scenePath = string.IsNullOrWhiteSpace(bettrSceneName)
                ? allScenePaths[0]
                : allScenePaths.First(s => Path.GetFileNameWithoutExtension(s).Equals(bettrSceneName));

            SceneManager.LoadScene(scenePath, LoadSceneMode.Single);
        }
    }

    [Serializable]
    public class BettrAssetScriptsController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        public Dictionary<string, Table> ScriptsTables { get; private set; }

        public BettrAssetScriptsController(BettrAssetController bettrAssetController)
        {
            // TileController.RegisterType<BettrAssetScriptsController>("BettrAssetScriptsController");
            // TileController.AddToGlobals("BettrAssetScriptsController", this);

            _bettrAssetController = bettrAssetController;

            ScriptsTables = new Dictionary<string, Table>();
        }

        public Table GetScript(string scriptName)
        {
            return ScriptsTables[scriptName];
        }

        public void ClearScripts()
        {
            ScriptsTables.Clear();
        }

        public IEnumerator LoadScripts(string bundleName, string bundleVersion)
        {
            var baseBundleName = $"{bundleName}";

            yield return _bettrAssetController.LoadAssetBundle(baseBundleName, bundleVersion,
                (name, version, bundle, bundleManifest, success, previouslyLoaded, error) =>
                {
                    if (success)
                    {
                        if (previouslyLoaded)
                        {
                            Debug.LogWarning($"Asset bundle={baseBundleName} version={bundleVersion} was previously loaded. Skipping AddScriptsToTable");
                            return;
                        }
                        // preload and cache the scripts
                        AddScripts(bundleManifest.Assets, bundle);
                    }
                    else
                    {
                        Debug.LogError(
                            $"Failed to load asset bundle={baseBundleName} version={bundleVersion}: {error}");
                    }
                });
        }

        public void AddScripts(string[] assetNames, AssetBundle assetBundle)
        {
            var startTime = Time.realtimeSinceStartup;
            var scriptAssetNames = assetNames.Where(name => name.EndsWith(".cscript.txt")).ToArray();
            foreach (var scriptAssetName in scriptAssetNames)
            {
                var textAsset = assetBundle.LoadAsset<TextAsset>(scriptAssetName);
                var script = textAsset.text;
                var className = Path.GetFileNameWithoutExtension(scriptAssetName);
                try
                {
                    className = Path.GetFileNameWithoutExtension(className);
                    TileController.LoadScript(className, script);
                }
                catch (Exception e)
                {
                    Debug.LogError($"error loading script{scriptAssetName} class={className} error={e.Message}");
                    throw;
                }
            }

            foreach (var scriptAssetName in scriptAssetNames)
            {
                var className = Path.GetFileNameWithoutExtension(scriptAssetName);
                try
                {
                    className = Path.GetFileNameWithoutExtension(className);
                    var scriptTable = TileController.RunScript<Table>(scriptName: className);
                    ScriptsTables[className] = scriptTable;
                    // DEBUG SECTION
                    // var globals = TileController.LuaScript.Globals;
                    // var t = (Table) globals["Game001BaseGameFreeSpinsTriggerSummary"];
                    // if (t != null)
                    // {
                    //     Debug.Log($"DEBUG1 script={scriptAssetName} globals={t.TableToJson()}");
                    // }
                }
                catch (Exception e)
                {
                    Debug.LogError($"error running script={scriptAssetName} class={className} error={e.Message}");
                    throw;
                }
            }
            var endTime = Time.realtimeSinceStartup;
            Debug.Log($"AddScriptsToTable {scriptAssetNames.Length} scripts took {endTime - startTime} seconds");
        }
        
        public void AddScript(string className, string script)
        {
            try
            {
                TileController.LoadScript(className, script);
            }
            catch (Exception e)
            {
                Debug.LogError($"error loading class={className} error={e.Message}");
                throw;
            }

            try
            {
                var scriptTable = TileController.RunScript<Table>(scriptName: className);
                ScriptsTables[className] = scriptTable;
                // DEBUG SECTION
                // var globals = TileController.LuaScript.Globals;
                // var t = (Table) globals["Game001BaseGameFreeSpinsTriggerSummary"];
                // if (t != null)
                // {
                //     Debug.Log($"DEBUG1 globals={t.TableToJson()}");
                // }
            }
            catch (Exception e)
            {
                Debug.LogError($"error running class={className} error={e.Message}");
                throw;
            }
        }
    }

    [Serializable]
    public class BettrAssetController
    {
        public bool useFileSystemAssetBundles = true;
        public string webAssetBaseURL;
        public string fileSystemAssetBaseURL = "Assets/Bettr/LocalStore/AssetBundles/OSX";

        public BettrAssetScriptsController BettrAssetScriptsController { get; private set; }
        public BettrAssetPrefabsController BettrAssetPrefabsController { get; private set; }
        
        public BettrAssetMaterialsController BettrAssetMaterialsController { get; private set; }
        public BettrAssetPackageController BettrAssetPackageController { get; private set; }
        public BettrAssetScenesController BettrAssetScenesController { get; private set; }
        
        private readonly HashSet<string> _loadingHashes = new HashSet<string>();
        
        private readonly Dictionary<string, string> _loadedBundleHashes = new Dictionary<string, string>();

        public BettrAssetController()
        {
            TileController.RegisterType<BettrAssetController>("BettrAssetController");
            TileController.AddToGlobals("BettrAssetController", this);
            
            BettrAssetScriptsController = new BettrAssetScriptsController(this);
            BettrAssetPackageController = new BettrAssetPackageController(this, BettrAssetScriptsController);
            BettrAssetScenesController = new BettrAssetScenesController(this, BettrAssetPackageController);
            BettrAssetPrefabsController = new BettrAssetPrefabsController(this, BettrAssetPackageController);
            BettrAssetMaterialsController = new BettrAssetMaterialsController(this, BettrAssetPackageController);
        }

        public IEnumerator LoadPackage(string packageName, string packageVersion, bool loadScenes)
        {
            yield return BettrAssetPackageController.LoadPackage(packageName, packageVersion, loadScenes);
        }

        public IEnumerator LoadScene(string bettrAssetBundleName, string bettrAssetBundleVersion, string bettrSceneName)
        {
            yield return BettrAssetScenesController.LoadScene(bettrAssetBundleName, bettrAssetBundleVersion, bettrSceneName);
        }
        
        public IEnumerator LoadPrefab(string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject parent = null)
        {
            yield return BettrAssetPrefabsController.LoadPrefab(bettrAssetBundleName, bettrAssetBundleVersion, prefabName, parent);
        }

        public IEnumerator ReplacePrefab(string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject replaced)
        {
            yield return BettrAssetPrefabsController.ReplacePrefab(bettrAssetBundleName, bettrAssetBundleVersion, prefabName, replaced);
        }

        public IEnumerator LoadMaterial(string bettrAssetBundleName, string bettrAssetBundleVersion, string materialName,
            GameObject targetGameObject)
        {
            yield return BettrAssetMaterialsController.LoadMaterial(bettrAssetBundleName, bettrAssetBundleVersion, materialName, targetGameObject);
        }

        public AssetBundle GetCachedAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion)
        {
            var suffix = string.IsNullOrEmpty(bettrAssetBundleVersion) ? "" : $".{bettrAssetBundleVersion}";
            var assetBundleName = $"{bettrAssetBundleName}{suffix}";
            var cachedAssetBundleName = assetBundleName;

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in loadedBundles)
            {
                if (bundle.name == cachedAssetBundleName)
                {
                    return bundle;
                }
            }

            return null;
        }

        public IEnumerator UnloadCachedAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion)
        {
            var assetBundle = GetCachedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            if (assetBundle == null)
            {
                yield break;
            }

            yield return assetBundle.UnloadAsync(true);
        }
        
        public IEnumerator ClearAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetClearCompleteCallback callback)
        {
            yield return LoadAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion,
                (assetBundleManifestName, assetBundleManifest, success, error) =>
                {
                    if (!success)
                    {
                        Debug.LogError($"Failed to load asset bundle manifest {assetBundleManifestName}: {error}");
                    }
                    callback(bettrAssetBundleName, bettrAssetBundleVersion, assetBundleManifest, success, error);
                });
        }

        public IEnumerator LoadAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetLoadCompleteCallback callback)
        {
            BettrAssetBundleManifest manifest = null;

            yield return LoadAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion,
                (assetBundleManifestName, assetBundleManifest, success, error) =>
                {
                    if (success)
                    {
                        manifest = assetBundleManifest;
                    }
                    else
                    {
                        Debug.LogError($"Failed to load asset bundle manifest {assetBundleManifestName}: {error}");
                    }
                });

            yield return LoadAssetBundle(manifest,
                ((name, bundle, success, loaded, error) =>
                {
                    callback(bettrAssetBundleName, bettrAssetBundleVersion, bundle, manifest, success, loaded,
                        error);
                }));
        }

        public IEnumerator LoadAssetBundle(BettrAssetBundleManifest assetBundleManifest,
            AssetBundleLoadCompleteCallback callback)
        {
            if (useFileSystemAssetBundles)
            {
                yield return LoadFileSystemAssetBundle(assetBundleManifest, callback);
            }
            else
            {
                yield return LoadWebAssetBundle(assetBundleManifest, callback);
            }
        }

        public IEnumerator LoadAssetBundleManifest(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetBundleManifestLoadCompleteCallback callback)
        {
            if (useFileSystemAssetBundles)
            {
                yield return LoadFileSystemAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion, callback);
            }
            else
            {
                yield return LoadWebAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion, callback);
            }
        }

        IEnumerator LoadWebAssetBundle(BettrAssetBundleManifest assetBundleManifest,
            AssetBundleLoadCompleteCallback callback)
        {
            var suffix = string.IsNullOrEmpty(assetBundleManifest.AssetBundleVersion)
                ? ""
                : $".{assetBundleManifest.AssetBundleVersion}";
            var assetBundleName = $"{assetBundleManifest.AssetBundleName}{suffix}";
            var assetBundleHash = assetBundleManifest.Hashes.AssetFileHash.Hash;
            if (assetBundleManifest.HashAppended == 1)
            {
                assetBundleName = $"{assetBundleManifest.AssetBundleName}_{assetBundleHash}{suffix}";
            }
            
            var crc = assetBundleManifest.CRC;
            AssetBundle previouslyDownloadedAssetBundle = null;
            
            if (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
            {
                while (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
                {
                    yield return null;
                }
                previouslyDownloadedAssetBundle = GetCachedAssetBundle(assetBundleManifest.AssetBundleName,
                    assetBundleManifest.AssetBundleVersion);
                callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                yield break;
            }
            
            //
            // Check if the current bundle is already loaded
            //
            previouslyDownloadedAssetBundle = GetCachedAssetBundle(assetBundleManifest.AssetBundleName,
                assetBundleManifest.AssetBundleVersion);
            if (previouslyDownloadedAssetBundle != null)
            {
                // check if this is a valid bundle
                if (_loadedBundleHashes.TryGetValue(assetBundleName, out string bundleHash))
                {
                    if (bundleHash != null && bundleHash.Equals(assetBundleHash))
                    {
                        callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                        yield break;
                    }
                }
            }
            
            // Currently loaded bundle hash doesnt exist or doesnt match hashes. Reload.
            
            //
            // Do load
            //
            AddToLoadingAssetBundleCache(assetBundleHash);
            
            var assetBundleURL = $"{webAssetBaseURL}/{assetBundleName}";
            using UnityWebRequest www = UnityWebRequestAssetBundle.GetAssetBundle(assetBundleURL, crc);
            float startTime = Time.realtimeSinceStartup;
            yield return www.SendWebRequest();
            float elapsedTime = Time.realtimeSinceStartup - startTime;
            Debug.Log($"LoadWebAssetBundle bundle={assetBundleName} took {elapsedTime} seconds.");

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                callback(assetBundleName, null, false, false, www.error);
                yield break;
            }

            AssetBundle downloadedAssetBundle = DownloadHandlerAssetBundle.GetContent(www);
            if (downloadedAssetBundle == null)
            {
                var error = $"null bundle for webAssetName={assetBundleName}";
                Debug.LogError(error);
                callback(assetBundleName, null, false, false, error);
                yield break;
            }
            
            // update the loaded bundle hash
            _loadedBundleHashes[assetBundleName] = assetBundleHash;

            callback(assetBundleName, downloadedAssetBundle, true, false, null);
            
            //
            // clear the loading cache so other requests are unblocked
            //
            ClearLoadingAssetBundleCache(assetBundleName);
        }

        IEnumerator LoadFileSystemAssetBundle(BettrAssetBundleManifest assetBundleManifest,
            AssetBundleLoadCompleteCallback callback)
        {
            var suffix = string.IsNullOrEmpty(assetBundleManifest.AssetBundleVersion)
                ? ""
                : $".{assetBundleManifest.AssetBundleVersion}";
            var assetBundleName = $"{assetBundleManifest.AssetBundleName}{suffix}";
            var assetBundleHash = assetBundleManifest.Hashes.AssetFileHash.Hash;
            if (assetBundleManifest.HashAppended == 1)
            {
                assetBundleName =
                    $"{assetBundleManifest.AssetBundleName}_{assetBundleManifest.Hashes.AssetFileHash.Hash}{suffix}";
            }

            var crc = assetBundleManifest.CRC;
            AssetBundle previouslyDownloadedAssetBundle = null;
            
            if (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
            {
                while (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
                {
                    yield return null;
                }
                previouslyDownloadedAssetBundle = GetCachedAssetBundle(assetBundleManifest.AssetBundleName,
                    assetBundleManifest.AssetBundleVersion);
                callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                yield break;
            }
            
            //
            // Check if the current bundle is already loaded
            //
            previouslyDownloadedAssetBundle = GetCachedAssetBundle(assetBundleManifest.AssetBundleName,
                assetBundleManifest.AssetBundleVersion);
            if (previouslyDownloadedAssetBundle != null)
            {
                // check if this is a valid bundle
                if (_loadedBundleHashes.TryGetValue(assetBundleName, out string bundleHash))
                {
                    if (bundleHash != null && bundleHash.Equals(assetBundleHash))
                    {
                        callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                        yield break;
                    }
                }
            }
            
            // Currently loaded bundle hash doesnt exist or doesnt match hashes. Reload.
            
            //
            // Do load
            //
            AddToLoadingAssetBundleCache(assetBundleHash);

            var assetBundleURL = $"{fileSystemAssetBaseURL}/{assetBundleName}";
            var downloadedAssetBundle = AssetBundle.LoadFromFile(assetBundleURL, crc);
            if (downloadedAssetBundle == null)
            {
                var error = $"null bundle for webAssetName={assetBundleName}";
                Debug.LogError(error);
                callback(assetBundleName, null, false, false, error);
                yield break;
            }
            
            // update the loaded bundle hash
            _loadedBundleHashes[assetBundleName] = assetBundleHash;

            // wait for callback to complete
            callback(assetBundleName, downloadedAssetBundle, true, false, null);
            
            //
            // clear the loading cache so other requests are unblocked
            //
            ClearLoadingAssetBundleCache(assetBundleName);
        }

        IEnumerator LoadWebAssetBundleManifest(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetBundleManifestLoadCompleteCallback callback)
        {
            var suffix = string.IsNullOrEmpty(bettrAssetBundleVersion)
                ? ".manifest"
                : $".{bettrAssetBundleVersion}.manifest";
            var bettrBundleManifestName = $"{bettrAssetBundleName}{suffix}";

            var webAssetName = bettrBundleManifestName;

            var assetBundleURL = $"{webAssetBaseURL}/{webAssetName}";
            using UnityWebRequest www = UnityWebRequest.Get(assetBundleURL);
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError(www.error);
                callback(webAssetName, null, false, www.error);
                yield break;
            }

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var assetBundleManifestBytes = www.downloadHandler.data;

            var assetBundleText = Encoding.ASCII.GetString(assetBundleManifestBytes);

            var assetBundleManifest = deserializer.Deserialize<BettrAssetBundleManifest>(assetBundleText);
            assetBundleManifest.AssetBundleName = bettrAssetBundleName;
            assetBundleManifest.AssetBundleVersion = bettrAssetBundleVersion;

            callback(webAssetName, assetBundleManifest, true, null);
        }

        IEnumerator LoadFileSystemAssetBundleManifest(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetBundleManifestLoadCompleteCallback callback)
        {
            var suffix = string.IsNullOrEmpty(bettrAssetBundleVersion)
                ? ".manifest"
                : $".{bettrAssetBundleVersion}.manifest";
            var bettrBundleManifestName = $"{bettrAssetBundleName}{suffix}";

            var fileSystemAssetName = bettrBundleManifestName;

            var assetBundleManifestURL = $"{fileSystemAssetBaseURL}/{fileSystemAssetName}";
            var assetBundleManifestBytes = File.ReadAllBytes(assetBundleManifestURL);

            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var assetBundleText = Encoding.ASCII.GetString(assetBundleManifestBytes);

            var assetBundleManifest = deserializer.Deserialize<BettrAssetBundleManifest>(assetBundleText);
            assetBundleManifest.AssetBundleName = bettrAssetBundleName;
            assetBundleManifest.AssetBundleVersion = bettrAssetBundleVersion;

            callback(fileSystemAssetName, assetBundleManifest, true, null);
            yield break;
        }

        private void AddToLoadingAssetBundleCache(string assetBundleHash)
        {
            _loadingHashes.Add(assetBundleHash);
        }

        private void ClearLoadingAssetBundleCache(string bundleName)
        {
            _loadingHashes.Remove(bundleName);
        }

        private bool IsAssetBundleLoading(string bundleName, string bundleHash)
        {
            return _loadingHashes.Contains(bundleName);
        }
    }
}