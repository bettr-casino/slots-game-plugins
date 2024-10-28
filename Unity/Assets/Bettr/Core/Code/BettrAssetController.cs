using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using CrayonScript.Interpreter.Execution.VM;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
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
        private static readonly HttpClient HttpClient = new HttpClient(new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,
            MaxConnectionsPerServer = int.MaxValue,
        });
        
        [NonSerialized] private BettrAssetController _bettrAssetController;
        
        public BettrMultiByteRangeAssetBundleController(BettrAssetController bettrAssetController)
        {
            _bettrAssetController = bettrAssetController;
        }
        
        private async Task<BettrMultiByteRange> FetchAssetByteRange(string url, BettrMultiByteRange range)
        {
            try
            {
                // Set up the HttpRequestMessage
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, url);
                request.Headers.Range = new RangeHeaderValue(range.ByteStart, range.ByteStart + range.ByteLength - 1);

                // Send the request and await the response
                HttpResponseMessage response = await HttpClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

                // Check if the request was successful and the response is a partial content (206)
                if (response.StatusCode == HttpStatusCode.PartialContent)
                {
                    // Since it's a single byte range, just read the content
                    byte[] content = await response.Content.ReadAsByteArrayAsync();

                    if (content != null && content.Length > 0)
                    {
                        // You can process the data here, e.g., save to file or parse
                        range.Data = content;  // Assuming BettrMultiByteRange has a 'Data' property to hold the content
                        return range;
                    }
                    else
                    {
                        Debug.LogError($"The response did not contain any content. url={url}");
                    }
                }
                else if (response.IsSuccessStatusCode)
                {
                    Debug.LogWarning($"Request was successful, but not a partial content response (206). url={url}");
                }
                else
                {
                    Debug.LogError($"Error: {response.StatusCode} url={url}");
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"Exception during fetch: {e.Message} url={url}");
            }
            return null;
        }

        private void ParseByteRange(byte[] content, string boundary, BettrMultiByteRange bundleByteRange)
        {
            string boundaryString = $"--{boundary}";

            // Split the content by the boundary string
            var contentParts = Encoding.UTF8.GetString(content).Split(new string[] { boundaryString }, StringSplitOptions.RemoveEmptyEntries);

            foreach (var part in contentParts)
            {
                if (string.IsNullOrWhiteSpace(part))
                    continue;

                // Find headers section and data section in the part
                int headersEndIndex = part.IndexOf("\r\n\r\n");
                if (headersEndIndex == -1)
                    continue;

                string headers = part.Substring(0, headersEndIndex);
                string dataString = part.Substring(headersEndIndex + 4); // Skip "\r\n\r\n"

                // Extract content-range header to determine which byte range this part represents
                string[] headerLines = headers.Split(new[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var headerLine in headerLines)
                {
                    if (headerLine.StartsWith("Content-Range", StringComparison.OrdinalIgnoreCase))
                    {
                        // Extract the byte range information
                        // Example: Content-Range: bytes 0-99/1234
                        string[] contentRangeParts = headerLine.Split(' ');
                        if (contentRangeParts.Length > 1)
                        {
                            string rangePart = contentRangeParts[1];
                            string[] rangeBounds = rangePart.Split('-');
                            if (rangeBounds.Length == 2)
                            {
                                bundleByteRange.Data = Encoding.UTF8.GetBytes(dataString);
                            }
                        }
                    }
                }
            }
        }

        public IEnumerator LoadMultiByteRangeAsset(Task<BettrMultiByteRange> manifestFetchTask, Task<BettrMultiByteRange> bundleFetchTask,
            BettrAssetMultiByteRange assetByteRange, AssetLoadCompleteCallback callback)
        {
            var bettrAssetBundleName = assetByteRange.Bundle.BundleName;
            var bettrAssetBundleVersion = assetByteRange.Bundle.BundleVersion;
            
            // Wait until the task is completed
            yield return new WaitUntil(() => manifestFetchTask.IsCompleted);
            
            // Handle the result once the task is completed
            if (manifestFetchTask.Exception != null)
            {
                var error = $"Error fetching manifest byte range: {manifestFetchTask.Exception.Message} assetBundleName={bettrAssetBundleName} assetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(bettrAssetBundleName, bettrAssetBundleVersion, null, null, false, false, error);
                yield break;
            }
            
            BettrMultiByteRange manifestDataMap = manifestFetchTask.Result;
            var manifestData = manifestDataMap.Data;
            
            if (manifestData == null)
            {
                var error = $"null asset manifest data for assetBundleName={bettrAssetBundleName} assetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(bettrAssetBundleName, bettrAssetBundleVersion, null, null, false, false, error);
                yield break;
            }
                
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(PascalCaseNamingConvention.Instance)
                .Build();

            var assetBundleManifestBytes = manifestData;
            var assetBundleText = Encoding.ASCII.GetString(assetBundleManifestBytes);

            var assetBundleManifest = deserializer.Deserialize<BettrAssetBundleManifest>(assetBundleText);
            assetBundleManifest.AssetBundleName = bettrAssetBundleName;
            assetBundleManifest.AssetBundleVersion = bettrAssetBundleVersion;
            
            // Wait until the task is completed
            yield return new WaitUntil(() => bundleFetchTask.IsCompleted);
            
            // Handle the result once the task is completed
            if (bundleFetchTask.Exception != null)
            {
                var error = $"Error fetching bundle byte range: {bundleFetchTask.Exception.Message} assetBundleName={bettrAssetBundleName} assetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(bettrAssetBundleName, bettrAssetBundleVersion, null, assetBundleManifest, false, false, error);
                yield break;
            }
            
            BettrMultiByteRange bundleDataMap = bundleFetchTask.Result;
            var bundleData = bundleDataMap.Data;
            
            AssetBundle downloadedAssetBundle = AssetBundle.LoadFromMemory(bundleData);
            if (downloadedAssetBundle == null)
            {
                var error = $"null downloaded asset bundle for assetBundleName={bettrAssetBundleName} assetBundleVersion={bettrAssetBundleVersion}";
                Debug.LogError(error);
                callback(bettrAssetBundleName, bettrAssetBundleVersion,
                    null,  assetBundleManifest, false, false, error);
                yield break;
            }
                
            callback(bettrAssetBundleName, bettrAssetBundleVersion,
                downloadedAssetBundle,  assetBundleManifest, true, false, null);   

        }

        public void LoadMultiByteRangeAssets(string binaryFileName, List<BettrAssetMultiByteRange> assetByteRanges, AssetLoadCompleteCallback callback)
        {
            var webAssetBaseURL = _bettrAssetController.webAssetBaseURL;
            var binaryAssetURL = $"{webAssetBaseURL}/{binaryFileName}";

            foreach (var assetByteRange in assetByteRanges)
            {
                var bettrAssetBundleName = assetByteRange.Bundle.BundleName;
                var bettrAssetBundleVersion = assetByteRange.Bundle.BundleVersion;
                
                // check if this asset bundle is already loaded into memory
                AssetBundle previouslyLoadedAssetBundle = null;
                previouslyLoadedAssetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
                if (previouslyLoadedAssetBundle != null)
                {
                    // check if this is a valid bundle
                    callback(bettrAssetBundleName, bettrAssetBundleVersion,
                        previouslyLoadedAssetBundle,  null, true, true, null);
                    continue;
                }
                
                var manifestByteRange = assetByteRange.Manifest;
                Task<BettrMultiByteRange> manifestFetchTask = FetchAssetByteRange(binaryAssetURL, manifestByteRange);
                var bundleByteRange = assetByteRange.Bundle;
                Task<BettrMultiByteRange> bundleFetchTask = FetchAssetByteRange(binaryAssetURL, bundleByteRange);
                BettrRoutineRunner.Instance.StartCoroutine(LoadMultiByteRangeAsset(manifestFetchTask, bundleFetchTask, assetByteRange, callback));
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
                context.SetError(new ScriptRuntimeException($"Prefab name is null or empty for asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}"));
                yield break;
            }
            
            yield return _bettrAssetPackageController.LoadPackage(bettrAssetBundleName, bettrAssetBundleVersion, false);
            
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
            var prefab = assetBundle.LoadAsset<GameObject>(prefabName);
            if (prefab == null)
            {
                Debug.LogError(
                    $"Failed to load prefab={prefabName} from asset bundle={bettrAssetBundleName} version={bettrAssetBundleVersion}");
                yield break;
            }
            
            Renderer[] renderers = prefab.GetComponentsInChildren<Renderer>(true);
            foreach (Renderer renderer in renderers)
            {
                foreach (Material mat in renderer.sharedMaterials)
                {
                    if (ShaderCaches.ShaderCache.TryGetValue(mat.shader.name, out Shader bettrShader))
                    {
                        mat.shader = bettrShader;
                    }
                }
            }
            
            // update the TextMeshProUI shaders
            var textMeshProUGUIs = prefab.GetComponentsInChildren<TMPro.TextMeshProUGUI>(true);
            foreach (var textMeshProUGUI in textMeshProUGUIs)
            {
                // check if the shader is in the cache
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshProUGUI.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshProUGUI.fontMaterial.shader = bettrShader;
                }
            }
            // similarly for TextMeshPro shaders
            var textMeshPros = prefab.GetComponentsInChildren<TMPro.TextMeshPro>(true);
            foreach (var textMeshPro in textMeshPros)
            {
                // check if the shader is in the cache
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshPro.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshPro.fontMaterial.shader = bettrShader;
                }
            }
            
            // Update shaders for all Image components that are using a Material which could be null
            var images = prefab.GetComponentsInChildren<Image>(true);
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
            _bettrAssetController = bettrAssetController;
            _bettrAssetPackageController = bettrAssetPackageController;
        }

        public IEnumerator LoadMaterial(string bettrAssetBundleName, string bettrAssetBundleVersion, string materialName,
            GameObject targetGameObject)
        {
            var assetBundle = _bettrAssetController.GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            
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
            var textMeshProUGUIs = Object.FindObjectsOfType<TMPro.TextMeshProUGUI>(true);
            foreach (var textMeshProUGUI in textMeshProUGUIs)
            {
                if (ShaderCaches.TmProShaderCache.TryGetValue(textMeshProUGUI.fontMaterial.shader.name, out Shader bettrShader))
                {
                    textMeshProUGUI.fontMaterial.shader = bettrShader;
                }
            }

            // Update shaders for all active and inactive TextMeshPro components in the scene
            var textMeshPros = Object.FindObjectsOfType<TMPro.TextMeshPro>(true);
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
        
        public void LoadMultiByteRangeAssets(string binaryFileName, List<BettrAssetMultiByteRange> byteRanges, AssetLoadCompleteCallback callback)
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
            var assetBundle = GetLoadedAssetBundle(bettrAssetBundleName, bettrAssetBundleVersion);
            if (assetBundle == null)
            {
                yield break;
            }

            AsyncOperation asyncOperation = assetBundle.UnloadAsync(true);
            while (!asyncOperation.isDone)
            {
                yield return null;
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