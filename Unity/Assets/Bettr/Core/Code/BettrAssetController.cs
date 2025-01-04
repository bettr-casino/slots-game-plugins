using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using CrayonScript.Interpreter.Execution.VM;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Object = UnityEngine.Object;

#if UNITY_EDITOR
using UnityEditor;
#endif


// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public delegate void MultiByteAssetLoadCompleteCallback(BettrAssetMultiByteRange manifest, string assetBundleName, string assetBundleVersion,
        AssetBundle assetBundle, BettrAssetBundleManifest assetBundleManifest, bool success,
        bool previouslyLoaded, string error);
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

    public static class ShaderCaches
    {
        [NonSerialized]
        public static readonly Dictionary<string, Shader> ShaderCache = new Dictionary<string, Shader>();
        [NonSerialized]
        public static readonly Dictionary<string, Shader> TmProShaderCache = new Dictionary<string, Shader>();

        static ShaderCaches()
        {
            var shaderNames = new string[]
            {
                "Bettr/BluishGlow",
                "Bettr/Frame",
                "Bettr/LobbyCardUnlitTexture",
                "Bettr/LobbyMask",
                "Bettr/ReelMask",
                "Bettr/Symbol",
                "Bettr/Shimmer",
                "Bettr/UIImage",
                "Unlit/Texture",
                "Unlit/Color",
                "Legacy Shaders/Particles/Additive",
                "Legacy Shaders/Particles/Alpha Blended Premultiply",
                "Standard",
                
            };
            
            // load the shaders into the cache
            foreach (var shaderName in shaderNames)
            {
                var shader = Shader.Find(shaderName);
                if (shader == null)
                {
                    Debug.LogError($"Failed to load shader={shaderName}");
                    continue;
                }
                ShaderCache[shaderName] = shader;
            }
            
            var tmProShaderNames = new string[]
            {
                "TextMeshPro/Distance Field"
            };
            
            // load the shaders into the cache
            foreach (var tmProShaderName in tmProShaderNames)
            {
                var shader = Shader.Find(tmProShaderName);
                if (shader == null)
                {
                    Debug.LogError($"Failed to load TextMeshPro shader={tmProShaderName}");
                    continue;
                }
                TmProShaderCache[tmProShaderName] = shader;
            }
        }
    }

    [Serializable]
    public class BettrAssetMultiByteRange
    {
        [JsonProperty("bundle")]
        public BettrMultiByteRange Bundle { get; set; }
        
        [JsonProperty("manifest")]
        public BettrMultiByteRange Manifest { get; set; }
    }
    
    [Serializable]
    public class BettrMultiByteRange
    {
        [JsonProperty("file_name")]
        public string FileName { get; set; }
        
        [JsonProperty("file_type")]
        public string FileType { get; set; }
        
        [JsonProperty("byte_start")]
        public int ByteStart { get; set; }

        [JsonProperty("byte_length")]
        public int ByteLength { get; set; }

        [JsonProperty("bundle_name")]
        public string BundleName { get; set; }
        
        [JsonProperty("bundle_version")]
        public string BundleVersion { get; set; }
        
        [NonSerialized] public byte[] Data = null;

        public string BundleId => $"{BundleName}.{BundleVersion}";
    }

    public class BettrMainLobbyManifests
    {
        [JsonProperty("manifests")]
        public List<BettrAssetMultiByteRange> Manifests { get; set; }

        public BettrAssetMultiByteRange FindManifest(string machineBundleName, string machineBundleVariant)
        {
            return Manifests.Find(m => m.Bundle.BundleName == machineBundleName && m.Bundle.BundleVersion == machineBundleVariant);
        }
    }

    [Serializable]
    public class BettrMultiByteRangeAssetBundleController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        
        public BettrMultiByteRangeAssetBundleController(BettrAssetController bettrAssetController)
        {
            _bettrAssetController = bettrAssetController;
        }
        
        private IEnumerator FetchAssetByteRange(string url, BettrMultiByteRange range, Action<BettrMultiByteRange> onComplete)
        {
            // Set up UnityWebRequest with the range header
            UnityWebRequest request = UnityWebRequest.Get(url);
            request.SetRequestHeader("Range", $"bytes={range.ByteStart}-{range.ByteStart + range.ByteLength - 1}");
        
            // Send request
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success && request.responseCode == 206)
            {
                byte[] content = request.downloadHandler.data;
                if (content != null && content.Length > 0)
                {
                    // Process the data if available
                    range.Data = content;
                    onComplete?.Invoke(range);
                }
                else
                {
                    Debug.LogError($"The response did not contain any content. url={url}");
                    onComplete?.Invoke(null);
                }
            }
            else
            {
                Debug.LogError($"Error: {request.result} | Status Code: {request.responseCode} url={url}");
                onComplete?.Invoke(null);
            }
        }

        public IEnumerator LoadMultiByteRangeAsset(string url, BettrAssetMultiByteRange manifest, MultiByteAssetLoadCompleteCallback callback)
        {
            var bettrAssetBundleName = manifest.Bundle.BundleName;
            var bettrAssetBundleVersion = manifest.Bundle.BundleVersion;

            int retries = 0;
            int retryIndex = 0;

            BettrAssetBundleManifest assetBundleManifest = null;

            while (true)
            {
                // Fetch manifest byte range
                yield return FetchAssetByteRange(url, manifest.Manifest, fetchedManifest =>
                {
                    if (fetchedManifest == null)
                    {
                        return;
                    }
                    manifest.Manifest = fetchedManifest;
                    var assetBundleText = Encoding.ASCII.GetString(fetchedManifest.Data);
                    var deserializer = new DeserializerBuilder().WithNamingConvention(PascalCaseNamingConvention.Instance).Build();
                    assetBundleManifest = deserializer.Deserialize<BettrAssetBundleManifest>(assetBundleText);
                    assetBundleManifest.AssetBundleName = bettrAssetBundleName;
                    assetBundleManifest.AssetBundleVersion = bettrAssetBundleVersion;
                });

                if (assetBundleManifest != null)
                {
                    break;
                }
                
                retryIndex++;

                if (retryIndex >= retries)
                {
                    break;
                }
                
                yield return new WaitForSeconds(0.1f);
            }
            
            if (assetBundleManifest == null)
            {
                string error = $"Failed to download BettrAssetBundleManifest. AssetBundleName={bettrAssetBundleName} AssetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(manifest, bettrAssetBundleName, bettrAssetBundleVersion, null, null, false, false, $"error fetching multi byte range AssetManifest manifest url={url}");
                yield break;
            }

            retryIndex = 0;

            AssetBundle downloadedAssetBundle = null;
            
            downloadedAssetBundle = AssetBundle.GetAllLoadedAssetBundles()
                .FirstOrDefault(bundle => bundle.name == manifest.Bundle.BundleId);

            if (downloadedAssetBundle != null)
            {
                callback(manifest, bettrAssetBundleName, bettrAssetBundleVersion, downloadedAssetBundle, assetBundleManifest, true, false, null);
                
                yield break;
            }

            while (true)
            {
                // Fetch asset bundle byte range
                yield return FetchAssetByteRange(url, manifest.Bundle, fetchedBundle =>
                {
                    if (fetchedBundle == null)
                    {
                        string error = $"Failed to fetch bundle byte range. AssetBundleName={bettrAssetBundleName} AssetBundleVersion={bettrAssetBundleVersion}";
                        Debug.LogError(error);
                        return;
                    }
                
                    downloadedAssetBundle = AssetBundle.GetAllLoadedAssetBundles()
                        .FirstOrDefault(bundle => bundle.name == manifest.Bundle.FileName);

                    if (downloadedAssetBundle == null)
                    {
                        try
                        {
                            downloadedAssetBundle = AssetBundle.LoadFromMemory(fetchedBundle.Data);                    
                        }
                        catch (Exception e)
                        {
                            string error = $"Failed to load AssetBundle from memory. AssetBundleName={bettrAssetBundleName} AssetBundleVersion={bettrAssetBundleVersion} error={e.Message}";
                            Debug.LogError(error);    
                        }
                    }
                });
                
                if (downloadedAssetBundle != null)
                {
                    break;
                }
                
                retryIndex++;

                if (retryIndex >= retries)
                {
                    break;
                }

                yield return new WaitForSeconds(0.1f);
            }
            
            if (downloadedAssetBundle == null)
            {
                string error = $"Failed to download AssetBundle. AssetBundleName={bettrAssetBundleName} AssetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(manifest, bettrAssetBundleName, bettrAssetBundleVersion, null, assetBundleManifest, false, false, error);
                yield break;
            }
            
            callback(manifest, bettrAssetBundleName, bettrAssetBundleVersion, downloadedAssetBundle, assetBundleManifest, true, false, null);
        }

        public void LoadMultiByteRangeAssets(string binaryFileName, List<BettrAssetMultiByteRange> manifests, MultiByteAssetLoadCompleteCallback callback)
        {
            var webAssetBaseURL = _bettrAssetController.webAssetBaseURL;
            var binaryAssetURL = $"{webAssetBaseURL}/{binaryFileName}";
            
            foreach (var manifest in manifests)
            {
                var bettrAssetBundleName = manifest.Bundle.BundleName;
                var bettrAssetBundleVersion = manifest.Bundle.BundleVersion;
                var lobbyCardAssetBundleName = $"lobbycard{bettrAssetBundleName}";

                // Check if this asset bundle is already loaded into memory
                AssetBundle previouslyLoadedAssetBundle = _bettrAssetController.GetLoadedAssetBundle(lobbyCardAssetBundleName, bettrAssetBundleVersion);
                if (previouslyLoadedAssetBundle != null)
                {
                    // If already loaded, invoke callback immediately
                    callback(manifest, bettrAssetBundleName, bettrAssetBundleVersion, previouslyLoadedAssetBundle, null, true, true, null);
                    continue;
                }

                // Start fetching byte ranges for the asset
                BettrRoutineRunner.Instance.StartCoroutine(LoadMultiByteRangeAsset(binaryAssetURL, manifest, callback));
            }
        }
    }

    [Serializable]
    public class BettrAssetPackageController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        [NonSerialized] private BettrAssetScriptsController _bettrAssetScriptsController;

        public BettrAssetPackageController(
            BettrAssetController bettrAssetController,
            BettrAssetScriptsController bettrAssetScriptsController)
        {
            _bettrAssetController = bettrAssetController;
            _bettrAssetScriptsController = bettrAssetScriptsController;
        }

        public IEnumerator LoadPackage(string packageName, string packageVersion, bool loadScenes)
        {
            yield return _bettrAssetController.LoadAssetBundle(packageName, packageVersion,
                (name, version, bundle, bundleManifest, success, previouslyLoaded, error) =>
                {
                    if (!success)
                    {
                        Debug.LogError(
                            $"Failed to load asset bundle={packageName} version={packageVersion} loadScenes=false error={error}");
                        return;
                    }

                    if (!previouslyLoaded)
                    {
                        // preload and cache the scripts
                        _bettrAssetScriptsController.AddScripts(bundleManifest.Assets, bundle);
                    }
                    
                }, false);

            if (loadScenes)
            {
                yield return _bettrAssetController.LoadAssetBundle(packageName, packageVersion,
                    (name, version, bundle, manifest, success, loaded, error) =>
                    {
                        if (!success)
                        {
                            Debug.LogError(
                                $"Failed to load asset bundle={packageName} version={packageVersion} loadScenes=true error={error}");
                            return;
                        }
                    }, true);
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
            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }
        
        public IEnumerator ReplacePrefab(string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject replaced)
        {
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
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

        public IEnumerator LoadPrefab(CrayonScriptContext context, string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject parent = null)
        {
            if (string.IsNullOrWhiteSpace(prefabName))
            {
                Debug.LogError($"Prefab name is null or empty for asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                context?.SetError(new ScriptRuntimeException($"Prefab name is null or empty for asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}"));
                yield break;
            }
            
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);

            GameObject prefab = null;
            
#if UNITY_EDITOR            
            
            // find the prefabName from the assetBundle
            string prefabPath = assetBundle.GetAllAssetNames()
                .FirstOrDefault(s => s.EndsWith($"{prefabName}.prefab", System.StringComparison.OrdinalIgnoreCase));
            if (prefabPath != null)
            {
                prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            }
#endif
            if (prefab == null)
            {
                prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            }
            
            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to load prefab={prefabName} from asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }
            
            var instance = Object.Instantiate(prefab, parent == null ? null : parent.transform);
            
            Renderer[] renderers = instance.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (mat == null) continue;
                    if (ShaderCaches.ShaderCache.TryGetValue(mat.shader.name, out Shader bettrShader))
                    {
                        mat.shader = bettrShader;
                    }
                }
            }
            
            // update the TextMeshProUI shaders
            // ReSharper disable once InconsistentNaming
            var textMeshProUGUIs = instance.GetComponentsInChildren<TextMeshProUGUI>(true);
            foreach (var textMeshProUGUI in textMeshProUGUIs)
            {
                // check if the shader is in the cache
                if (textMeshProUGUI == null)
                {
                    Debug.LogWarning($"TextMeshProUGUI fontMaterial is null for prefab={prefabName} textMeshProUGUI=null");
                    continue;
                }
                
                // check if the shader is in the cache
                if (textMeshProUGUI.fontMaterial == null)
                {
                    Debug.LogWarning($"TextMeshProUGUI fontMaterial is null for prefab={prefabName} textMeshProUGUI={textMeshProUGUI.name}");
                    continue;
                }
                
                // check if the shader is in the cache
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshProUGUI.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshProUGUI.fontMaterial.shader = bettrShader;
                }
            }
            // similarly for TextMeshPro shaders
            var textMeshPros = instance.GetComponentsInChildren<TextMeshPro>(true);
            foreach (var textMeshPro in textMeshPros)
            {
                // check if the shader is in the cache
                if (textMeshPro == null)
                {
                    Debug.LogWarning($"TextMeshPro fontMaterial is null for prefab={prefabName} textMeshPro=null");
                    continue;
                }
                
                // check if the shader is in the cache
                if (textMeshPro.fontMaterial == null)
                {
                    Debug.LogWarning($"TextMeshPro fontMaterial is null for prefab={prefabName} textMeshPro={textMeshPro.name}");
                    continue;
                }

                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshPro.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshPro.fontMaterial.shader = bettrShader;
                }
            }
            
            // Update shaders for all Image components that are using a Material which could be null
            var images = instance.GetComponentsInChildren<Image>(true);
            foreach (var image in images)
            {
                if (image.material != null)
                {
                    if (ShaderCaches.ShaderCache.TryGetValue(image.material.shader.name, out Shader bettrShader))
                    {
                        image.material.shader = bettrShader;
                    }
                }
            }
            
            if (context != null)
            {
                context.GameObjectResult = instance;
            }
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
            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }

        public IEnumerator LoadMaterial(string bettrAssetBundleName, string bettrAssetBundleVersion, string materialName,
            GameObject targetGameObject)
        {
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);

            Material material = null;
            
#if UNITY_EDITOR            
            
            // find the prefabName from the assetBundle
            string materialPath = assetBundle.GetAllAssetNames()
                .FirstOrDefault(s => s.EndsWith($"{materialName}.mat", System.StringComparison.OrdinalIgnoreCase));
            if (materialPath != null)
            {
                material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            }
#endif
            if (material == null)
            {
                material = assetBundle.LoadAsset<Material>(materialName);
            }
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
                    
                    foreach (Material mat in meshRenderer.sharedMaterials)
                    {
                        if (mat == null) continue;
                        if (ShaderCaches.ShaderCache.TryGetValue(mat.shader.name, out Shader bettrShader))
                        {
                            mat.shader = bettrShader;
                        }
                    }
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
            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }

        public IEnumerator LoadScene(string bettrAssetBundleName, string bettrAssetBundleVersion, string bettrSceneName, bool loadSingle = true)
        {
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, true);
            
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion, true);
            
            var allScenePaths = assetBundle.GetAllScenePaths();
            var scenePath = string.IsNullOrWhiteSpace(bettrSceneName)
                ? allScenePaths[0]
                : allScenePaths.First(s => Path.GetFileNameWithoutExtension(s).Equals(bettrSceneName));
            
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(scenePath, loadSingle ? LoadSceneMode.Single : LoadSceneMode.Additive);
            if (asyncLoad == null)
            {
                Debug.Log($"failed to load scene={scenePath} bettrAssetBundleName={bettrAssetBundleName} bettrAssetBundleVersion={bettrAssetBundleVersion} allowSceneActivation={loadSingle}");
                yield break;
            }
            
            // Wait until the scene has fully loaded
            while (!asyncLoad.isDone)
            {
                yield return null;
            }
            
            // Update shaders for all active and inactive Renderer components in the scene
            Renderer[] renderers = Object.FindObjectsOfType<Renderer>(true); // 'true' includes inactive objects
            foreach (var renderer in renderers)
            {
                foreach (var mat in renderer.sharedMaterials)
                {
                    if (ShaderCaches.ShaderCache.TryGetValue(mat.shader.name, out Shader bettrShader))
                    {
                        mat.shader = bettrShader;
                    }
                }
            }

            // Update shaders for all active and inactive TextMeshProUGUI components in the scene
            // ReSharper disable once InconsistentNaming
            var textMeshProUGUIs = Object.FindObjectsOfType<TextMeshProUGUI>(true);
            foreach (var textMeshProUGUI in textMeshProUGUIs)
            {
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshProUGUI.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshProUGUI.fontMaterial.shader = bettrShader;
                }
            }

            // Update shaders for all active and inactive TextMeshPro components in the scene
            var textMeshPros = Object.FindObjectsOfType<TextMeshPro>(true);
            foreach (var textMeshPro in textMeshPros)
            {
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshPro.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshPro.fontMaterial.shader = bettrShader;
                }
            }
            
            // Update shaders for all Image components that are using a Material which could be null
            var images = Object.FindObjectsOfType<Image>(true);
            foreach (var image in images)
            {
                if (image.material != null)
                {
                    if (ShaderCaches.ShaderCache.TryGetValue(image.material.shader.name, out Shader bettrShader))
                    {
                        image.material.shader = bettrShader;
                    }
                }
            }
        }
    }

    [Serializable]
    public class BettrAssetScriptsController
    {
        [NonSerialized] private BettrAssetController _bettrAssetController;
        public Dictionary<string, Table> ScriptsTables { get; private set; }

        public BettrAssetScriptsController(BettrAssetController bettrAssetController)
        {
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
                }, false);
        }

        public void AddScripts(string[] assetNames, AssetBundle assetBundle)
        {
            var startTime = Time.realtimeSinceStartup;
            var scriptAssetNames = assetNames.Where(name => name.EndsWith(".cscript.txt")).ToArray();
            foreach (var scriptAssetName in scriptAssetNames)
            {
#if UNITY_EDITOR
                // Load from the Local Asset
                {
                    var textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(scriptAssetName);
                    var script = textAsset.text;
                    var className = Path.GetFileNameWithoutExtension(scriptAssetName);
                    try
                    {
                        className = Path.GetFileNameWithoutExtension(className);
                        TileController.LoadScript(className, script);

                        continue;
                    }
                    catch (Exception e)
                    {
                        Debug.LogError($"error loading script{scriptAssetName} class={className} error={e.Message}");
                        throw;
                    }
                }
#endif
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
        
#if UNITY_IOS
        public string fileSystemAssetBaseURL => "Assets/Bettr/LocalStore/AssetBundles/iOS";
#endif
#if UNITY_ANDROID
        public string fileSystemAssetBaseURL => "Assets/Bettr/LocalStore/AssetBundles/Android";
#endif
#if UNITY_WEBGL
        public string fileSystemAssetBaseURL => "Assets/Bettr/LocalStore/AssetBundles/WebGL";
#endif
#if UNITY_STANDALONE_OSX
        public string fileSystemAssetBaseURL => "Assets/Bettr/LocalStore/AssetBundles/OSX";
#endif


        public BettrAssetScriptsController BettrAssetScriptsController { get; private set; }
        public BettrAssetPrefabsController BettrAssetPrefabsController { get; private set; }
        
        public BettrMultiByteRangeAssetBundleController BettrMultiByteRangeAssetController { get; private set; }
        
        public BettrAssetMaterialsController BettrAssetMaterialsController { get; private set; }
        public BettrAssetPackageController BettrAssetPackageController { get; private set; }
        public BettrAssetScenesController BettrAssetScenesController { get; private set; }
        
        private readonly HashSet<string> _loadingHashes = new HashSet<string>();
        
        private readonly Dictionary<string, string> _loadedBundleHashes = new Dictionary<string, string>();
        
        public static BettrAssetController Instance { get; private set; }

        public BettrAssetController()
        {
            TileController.RegisterType<BettrAssetController>("BettrAssetController");
            TileController.AddToGlobals("BettrAssetController", this);
            
            BettrAssetScriptsController = new BettrAssetScriptsController(this);
            BettrAssetPackageController = new BettrAssetPackageController(this, BettrAssetScriptsController);
            BettrAssetScenesController = new BettrAssetScenesController(this, BettrAssetPackageController);
            BettrAssetPrefabsController = new BettrAssetPrefabsController(this, BettrAssetPackageController);
            BettrMultiByteRangeAssetController = new BettrMultiByteRangeAssetBundleController(this);
            BettrAssetMaterialsController = new BettrAssetMaterialsController(this, BettrAssetPackageController);
            
            Instance = this;
        }
        
        public string GetAssetBundleName(string bundleName, string bundleVariant, bool isScene)
        {
            return isScene ? $"{bundleName}_scenes.{bundleVariant}" : $"{bundleName}.{bundleVariant}";
        }
        
        private string GetAssetBundleManifestName(string bundleName, string bundleVariant, bool isScene)
        {
            return isScene ? $"{bundleName}_scenes.{bundleVariant}.manifest" : $"{bundleName}.{bundleVariant}.manifest";
        }
        
        public void LoadMultiByteRangeAssets(string binaryFileName, List<BettrAssetMultiByteRange> byteRanges, MultiByteAssetLoadCompleteCallback callback)
        {
            BettrMultiByteRangeAssetController.LoadMultiByteRangeAssets(binaryFileName, byteRanges, callback);
        }

        public IEnumerator LoadPackage(string packageName, string packageVersion, bool loadScenes)
        {
            yield return BettrAssetPackageController.LoadPackage(packageName, packageVersion, loadScenes);
        }

        public IEnumerator LoadScene(string bettrAssetBundleName, string bettrAssetBundleVersion, string bettrSceneName, bool loadSingle = true)
        {
            yield return BettrAssetScenesController.LoadScene(bettrAssetBundleName, bettrAssetBundleVersion, bettrSceneName, loadSingle);
        }
        
        public IEnumerator LoadPrefab(CrayonScriptContext context, string bettrAssetBundleName, string bettrAssetBundleVersion, string prefabName,
            GameObject parent = null)
        {
            yield return BettrAssetPrefabsController.LoadPrefab(context, bettrAssetBundleName, bettrAssetBundleVersion, prefabName, parent);
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

        public AssetBundle GetLoadedAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion, bool isScene = false)
        {
            var assetBundleName = GetAssetBundleName(bettrAssetBundleName, bettrAssetBundleVersion, isScene);
            
            var cachedAssetBundleName = assetBundleName;

            var loadedBundles = AssetBundle.GetAllLoadedAssetBundles();
            foreach (var bundle in loadedBundles)
            {
                if (string.Equals(bundle.name, cachedAssetBundleName, StringComparison.OrdinalIgnoreCase))
                {
                    return bundle;
                }
            }

            return null;
        }

        public IEnumerator UnloadCachedAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion)
        {
            var sceneAssetBundle = GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion, isScene:true);
            if (sceneAssetBundle != null)
            {
                AsyncOperation asyncOperation = sceneAssetBundle.UnloadAsync(true);
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }
            
            var assetBundle = GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion, isScene:false);
            if (assetBundle != null)
            {
                AsyncOperation asyncOperation = assetBundle.UnloadAsync(true);
                while (!asyncOperation.isDone)
                {
                    yield return null;
                }
            }
        }

        public IEnumerator LoadAssetBundle(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetLoadCompleteCallback callback, bool isScene)
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
                        Debug.LogError($"Failed to load asset bundle manifest bettrAssetBundleName={bettrAssetBundleName} bettrAssetBundleVersion={bettrAssetBundleVersion} isScene={isScene} assetBundleManifestName={assetBundleManifestName}: {error}");
                    }
                }, isScene);

            yield return LoadAssetBundle(manifest,
                ((name, bundle, success, loaded, error) =>
                {
                    callback(bettrAssetBundleName, bettrAssetBundleVersion, bundle, manifest, success, loaded,
                        error);
                }), isScene);
        }

        public IEnumerator LoadAssetBundle(BettrAssetBundleManifest assetBundleManifest,
            AssetBundleLoadCompleteCallback callback, bool isScene)
        {
            if (useFileSystemAssetBundles)
            {
                yield return LoadFileSystemAssetBundle(assetBundleManifest, callback, isScene);
            }
            else
            {
                yield return LoadWebAssetBundle(assetBundleManifest, callback, isScene);
            }
        }

        public IEnumerator LoadAssetBundleManifest(string bettrAssetBundleName, string bettrAssetBundleVersion,
            AssetBundleManifestLoadCompleteCallback callback, bool isScene)
        {
            if (useFileSystemAssetBundles)
            {
                yield return LoadFileSystemAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion, callback, isScene);
            }
            else
            {
                yield return LoadWebAssetBundleManifest(bettrAssetBundleName, bettrAssetBundleVersion, callback, isScene);
            }
        }

        IEnumerator LoadWebAssetBundle(BettrAssetBundleManifest assetBundleManifest,
            AssetBundleLoadCompleteCallback callback, bool isScene)
        {
            var assetBundleName = GetAssetBundleName(assetBundleManifest.AssetBundleName, assetBundleManifest.AssetBundleVersion, isScene);
            var assetBundleHash = assetBundleManifest.Hashes.AssetFileHash.Hash;
            if (assetBundleManifest.HashAppended == 1)
            {
                // TODO: check if this hash is required
            }
            
            var crc = assetBundleManifest.CRC;
            AssetBundle previouslyDownloadedAssetBundle = null;
            
            if (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
            {
                while (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
                {
                    yield return null;
                }
                previouslyDownloadedAssetBundle = GetLoadedAssetBundle(assetBundleManifest.AssetBundleName,
                    assetBundleManifest.AssetBundleVersion);
                callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                yield break;
            }
            
            //
            // Check if the current bundle is already loaded
            //
            previouslyDownloadedAssetBundle = GetLoadedAssetBundle(assetBundleManifest.AssetBundleName,
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
            AssetBundleLoadCompleteCallback callback, bool isScene)
        {
            var assetBundleName = GetAssetBundleName(assetBundleManifest.AssetBundleName, assetBundleManifest.AssetBundleVersion, isScene);
            var assetBundleHash = assetBundleManifest.Hashes.AssetFileHash.Hash;
            if (assetBundleManifest.HashAppended == 1)
            {
                // TODO: check if this hash is required
            }

            var crc = assetBundleManifest.CRC;
            AssetBundle previouslyDownloadedAssetBundle = null;
            
            if (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
            {
                while (IsAssetBundleLoading(assetBundleManifest.AssetBundleName, assetBundleName))
                {
                    yield return null;
                }
                previouslyDownloadedAssetBundle = GetLoadedAssetBundle(assetBundleManifest.AssetBundleName,
                    assetBundleManifest.AssetBundleVersion);
                callback(assetBundleName, previouslyDownloadedAssetBundle, true, true, null);
                yield break;
            }
            
            //
            // Check if the current bundle is already loaded
            //
            previouslyDownloadedAssetBundle = GetLoadedAssetBundle(assetBundleManifest.AssetBundleName,
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
                var error = $"null bundle for fileSystem assetBundleName={assetBundleName}";
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
            AssetBundleManifestLoadCompleteCallback callback, bool isScene)
        {
            var bettrBundleManifestName = GetAssetBundleManifestName(bettrAssetBundleName, bettrAssetBundleVersion, isScene);

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
            AssetBundleManifestLoadCompleteCallback callback, bool isScene)
        {
            var bettrBundleManifestName = GetAssetBundleManifestName(bettrAssetBundleName, bettrAssetBundleVersion, isScene);

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