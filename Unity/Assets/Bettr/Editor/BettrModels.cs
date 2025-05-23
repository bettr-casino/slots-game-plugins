using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Bettr.Core;
using Bettr.Editor.generators;
using CrayonScript.Code;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.Rendering;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using PrimitiveType = UnityEngine.PrimitiveType;

namespace Bettr.Editor
{
    public interface IGameObject
    {
        public GameObject GameObject { get; }
        public void SetParent(GameObject parentGo);
        
        public void SetParent(IGameObject parentGo);
    }

    [Serializable]
    public class InstanceGameObject : IGameObject
    {
        public static Dictionary<string, InstanceGameObject> IdGameObjects = new Dictionary<string, InstanceGameObject>();
        
        // Cache to store processed materials
        public static Dictionary<string, Material> SymbolMaterialCache = new Dictionary<string, Material>();
        
        private GameObject _go;
        public string Name { get; set; }
        
        public string Id { get; set; }
        
        public List<PrefabId> PrefabIds { get; set; }
        
        public string PrefabName { get; set; }
        
        public bool PrefabUnpacked { get; set; }
        
        public string PrefabShaderOld { get; set; }
        
        public string PrefabShaderNew { get; set; }
        
        public string PrefabMaterialNewPrefix { get; set; }
        
        public string PrefabTextureNewPrefix { get; set; }
        
        public bool IsPrefab { get; set; }
        
        public bool IsMainLobbyPrefab { get; set; }
        
        public string ModelName { get; set; }
        
        public bool IsModel { get; set; }
        
        public string PrimitiveMaterial { get; set; }
        
        public string PrimitiveShader { get; set; }
        
        public string PrimitiveTexture { get; set; }
        
        public bool PrimitiveTextureCreate { get; set; }
        
        public string PrimitiveTextureCreateSource { get; set; }
        
        public bool PrimitiveTextureForceReplace { get; set; }
        
        public string PrimitiveColor { get; set; }
        
        public float PrimitiveAlpha { get; set; } = 1.0f;
        
        public int Primitive { get; set; }
        
        public bool IsPrimitive { get; set; }
        
        public Dictionary<string, float> ShaderProperties { get; set; }
        
        public bool Active { get; set; }
        
        public string Layer { get; set; }
        
        public Vector3? LocalPosition { get; set; }
        
        public Vector3? Position { get; set; }
        
        public Vector3? Rotation { get; set; }
        
        public Vector3? Scale { get; set; }
        
        public string AnchorPresets { get; set; }
        
        public List<InstanceComponent> Components { get; set; }

        public InstanceGameObject Child { get; set; }
        
        public List<InstanceGameObject> Children { get; set; }

        public GameObject GameObject => _go;

        public Animator Animator => _go.GetComponent<Animator>();
        
        public TMP_Text TextMeshPro => _go.GetComponent<TMP_Text>();
        
        public ParticleSystem ParticleSystem => _go.GetComponent<ParticleSystem>();

        public InstanceGameObject()
        {
            Active = true;
            Layer = "Default";
        }
        
        public InstanceGameObject(GameObject go)
        {
            _go = go;
            Name = go.name;
            EnsureGameObject();
        }

        public InstanceGameObject(string name)
        {
            Name = name;
            EnsureGameObject();
        }
        
        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            Debug.Log($"Deserialization complete name={Name}");
            
            EnsureGameObject();
            
            if (Id != null && Id.Length > 0)
            {
                IdGameObjects[Id] = this;
            }
            
            if (Child != null)
            {
                Child.SetParent(_go);
            } 
            else if (Children != null && Children.Count > 0)
            {
                foreach (var child in Children)
                {
                    child.SetParent(_go);
                }
            }

            if (Components != null)
            {
                foreach (var component in Components)
                {
                    component.AddComponent(_go);
                }
            }
            
            _go.transform.position = new Vector3(0, 0, 0);
            if (Position != null)
            {
                _go.transform.position = (Vector3) Position;
            }
            _go.transform.rotation = Quaternion.Euler(0, 0, 0);
            if (Rotation != null)
            {
                _go.transform.rotation = Quaternion.Euler((Vector3) Rotation);
            }
            _go.transform.localScale = new Vector3(1, 1, 1);
            if (Scale != null)
            {
                _go.transform.localScale = (Vector3) Scale;
            }
            
            // Set the AnchorPresets
            if (AnchorPresets != null)
            {
                RectTransform rectTransform = _go.GetComponent<RectTransform>();
                if (rectTransform == null)
                {
                    Debug.LogError($"RectTransform component not found on gameObject={_go.name}");
                    return;
                }
                switch (AnchorPresets)
                {
                    case "BottomStretch":
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(1, 0);
                        rectTransform.pivot = new Vector2(0.5f, 0);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "TopStretch":
                        rectTransform.anchorMin = new Vector2(0, 1);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.pivot = new Vector2(0.5f, 1);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "LeftStretch":
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.pivot = new Vector2(0, 0.5f);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "RightStretch":
                        rectTransform.anchorMin = new Vector2(1, 0);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.pivot = new Vector2(1, 0.5f);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "TopLeft":
                        rectTransform.anchorMin = new Vector2(0, 1);
                        rectTransform.anchorMax = new Vector2(0, 1);
                        rectTransform.pivot = new Vector2(0, 1);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "TopRight":
                        rectTransform.anchorMin = new Vector2(1, 1);
                        rectTransform.anchorMax = new Vector2(1, 1);
                        rectTransform.pivot = new Vector2(1, 1);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "BottomLeft":
                        rectTransform.anchorMin = new Vector2(0, 0);
                        rectTransform.anchorMax = new Vector2(0, 0);
                        rectTransform.pivot = new Vector2(0, 0);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    case "BottomRight":
                        rectTransform.anchorMin = new Vector2(1, 0);
                        rectTransform.anchorMax = new Vector2(1, 0);
                        rectTransform.pivot = new Vector2(1, 0);
                        rectTransform.anchoredPosition = Vector2.zero;
                        break;
                    default:
                        Debug.LogError($"AnchorPresets={AnchorPresets} is not supported.");
                        throw new ArgumentOutOfRangeException(nameof(AnchorPresets), AnchorPresets, "Unsupported anchor preset.");
                }
            }
        }
        
        public void SetParent(GameObject parentGo)
        {
            _go.transform.SetParent(parentGo == null ? null : parentGo.transform);
        }

        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }

        private void EnsureGameObject()
        {
            if (_go == null)
            {
                if (IsModel)
                {
                    Debug.Log($"loading model from path: {InstanceComponent.RuntimeAssetPath}/Models/{ModelName}.fbx");
                    BettrModelController.ImportModelAsPrefab(ModelName, PrefabName, InstanceComponent.RuntimeAssetPath);
                    string prefabPath = Path.Combine(InstanceComponent.RuntimeAssetPath, "Prefabs",  $"{PrefabName}.prefab");
                    GameObject modelAsPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var modelGameObject = new PrefabGameObject(modelAsPrefab, Name, false);
                    _go = modelGameObject.GameObject;
                }
                else if (IsPrefab)
                {
                    Debug.Log($"loading prefab from path: {InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab");
                    GameObject prefab = null;
                    string prefabPath = $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab";
                    prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    if (prefab == null)
                    {
                        prefabPath = $"{InstanceComponent.DefaultRuntimeAssetPath}/Prefabs/{PrefabName}.prefab";
                        prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    }
                    
                    var prefabGameObject = new PrefabGameObject(prefab, Name, PrefabUnpacked);
                    _go = prefabGameObject.GameObject;
                    
                    // START - update Prefab Shaders
                    
                    // Replace the shader if required
                    if (!string.IsNullOrEmpty(PrefabShaderOld) && !string.IsNullOrEmpty(PrefabShaderNew))
                    {
                        // Get all renderers in the GameObject and its children
                        var renderers = _go.GetComponentsInChildren<Renderer>(true);
                        Debug.Log($"Updating shaders for {renderers.Length} renderers in {Name}");
                    
                        foreach (var renderer in renderers)
                        {
                            Debug.Log($"Updating renderer for {renderer.name} renderers in {Name}");

                            var materials = renderer.sharedMaterials.ToArray();
                            bool materialsChanged = false;
                    
                            for (int i = 0; i < materials.Length; i++)
                            {
                                var material = materials[i];
                    
                                // Check if the material uses the old shader
                                if (material != null && material.shader.name == PrefabShaderOld)
                                {
                                    // Check if the material is already in the cache
                                    if (SymbolMaterialCache.TryGetValue(material.name, out Material cachedMaterial))
                                    {
                                        // Use the cached material
                                        Debug.Log($"Using the cached material for {material.name}");
                                        materials[i] = cachedMaterial;
                                        materialsChanged = true;
                                    }
                                    else
                                    {
                                        // Convert color to hex string
                                        string hexColor = $"#{ColorUtility.ToHtmlStringRGBA(material.color)}";

                                        string materialName = $"{PrefabMaterialNewPrefix}{material.name}";
                                        // Get the main texture name if it exists
                                        string textureName = material.mainTexture != null ? material.mainTexture.name : "";
                                        textureName = $"{PrefabTextureNewPrefix}{textureName}";

                                        // Save the material using BettrMaterialGenerator
                                        var savedMaterial = BettrMaterialGenerator.CreateOrLoadMaterial(
                                            materialName,           // materialName
                                            PrefabShaderNew,         // shaderName
                                            textureName,             // textureName
                                            hexColor,                // hexColor
                                            material.color.a,        // alpha
                                            InstanceComponent.RuntimeAssetPath,  // runtimeAssetPath
                                            true,                   // createTextureIfNotExists
                                            null                     // sourceTexture
                                        );
                                        
                                        Debug.Log($"Creating a new saved material for {material.name}");
                    
                                        if (savedMaterial != null)
                                        {
                                            // Cache the created material
                                            SymbolMaterialCache[material.name] = savedMaterial;
                    
                                            // Update the material in the materials array
                                            materials[i] = savedMaterial;
                                            materialsChanged = true;
                                        }
                                        else
                                        {
                                            Debug.LogWarning($"Failed to create/load material for: {material.name}");
                                        }
                                    }
                                }
                            }
                    
                            // If any materials were changed, update the renderer
                            if (materialsChanged)
                            {
                                renderer.sharedMaterials = materials;
                                EditorUtility.SetDirty(renderer);
                                EditorUtility.SetDirty(_go);
                            }
                        }
                    }
                    
                    
                    // - END - Prefab Shader Update
                    
                    if (PrefabIds != null)
                    {
                        foreach (var prefabId in PrefabIds)
                        {
                            var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                            InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                        }
                    }
                }
                else if (IsMainLobbyPrefab)
                {
                    Debug.Log($"loading prefab from path: {InstanceComponent.MainLobbyPath}/Prefabs/{PrefabName}.prefab");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{InstanceComponent.MainLobbyPath}/Prefabs/{PrefabName}.prefab");
                    var prefabGameObject = new PrefabGameObject(prefab, Name, false);
                    _go = prefabGameObject.GameObject;
                    if (PrefabIds != null)
                    {
                        foreach (var prefabId in PrefabIds)
                        {
                            var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                            InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                        }
                    }
                }
                else if (IsPrimitive)
                {
                    var primitiveGameObject = GameObject.CreatePrimitive(Enum.GetValues(typeof(PrimitiveType)).GetValue(Primitive) as PrimitiveType? ?? PrimitiveType.Quad);
                    var primitiveMaterial = BettrMaterialGenerator.CreateOrLoadMaterial(PrimitiveMaterial, PrimitiveShader, PrimitiveTexture, PrimitiveColor, PrimitiveAlpha, InstanceComponent.RuntimeAssetPath, PrimitiveTextureCreate, PrimitiveTextureCreateSource, PrimitiveTextureForceReplace);
                    // update the shader properties
                    if (ShaderProperties != null && ShaderProperties.Count > 0)
                    {
                        foreach (var shaderPropertyPair in ShaderProperties)
                        {
                            var shaderProperty = shaderPropertyPair.Key;
                            var shaderValue = shaderPropertyPair.Value;
                            var abs = Math.Abs(shaderValue - (int) shaderValue);
                            if (abs < 0.000001)
                            {
                                var shaderValueInt = (int) shaderValue;
                                if (shaderProperty == "RenderQueue")
                                {
                                    primitiveMaterial.renderQueue = shaderValueInt;
                                }
                                else
                                {
                                    primitiveMaterial.SetInt(shaderProperty, shaderValueInt);
                                }
                            }
                            else
                            {
                                primitiveMaterial.SetFloat(shaderProperty, shaderValue);
                            }
                        }
                    }
                    
                    var primitiveMeshRenderer = primitiveGameObject.GetComponent<MeshRenderer>();
                    primitiveMeshRenderer.material = primitiveMaterial;
                    primitiveGameObject.name = Name;
                    
                    var primitiveMeshCollider = primitiveGameObject.GetComponent<MeshCollider>();
                    primitiveMeshCollider.includeLayers = LayerMask.GetMask(Layer);
                    
                    _go = primitiveGameObject;
                }
                else
                {
                    _go = new GameObject(Name);
                }
                
                _go.SetActive(Active);
                _go.layer = LayerMask.NameToLayer(Layer);
            }
        }
        
        public static GameObject FindReferencedId(GameObject parentGameObject, string id, int index)
        {
            var currentIndex = 0;
            return FindByIdDepthFirst(parentGameObject.transform, id, ref index, ref currentIndex);
        }
        
        private static GameObject FindByIdDepthFirst(Transform current, string id, ref int targetIndex, ref int currentIndex)
        {
            var identifier = current.gameObject;
            if (identifier != null && identifier.name == id)
            {
                if (currentIndex == targetIndex)
                {
                    return current.gameObject;
                }
                currentIndex++;  // Only increment if the ID matches
            }

            foreach (Transform child in current)
            {
                var found = FindByIdDepthFirst(child, id, ref targetIndex, ref currentIndex);
                if (found != null)
                {
                    return found;
                }
            }

            return null;
        }
    }

    [Serializable]    
    public struct PrefabId
    {
        // ReSharper disable once InconsistentNaming
        public string Prefix;
        // ReSharper disable once InconsistentNaming
        public string Id;
        // ReSharper disable once InconsistentNaming
        public int Index;
    }

    public class PrefabGameObject : IGameObject
    {
        private readonly GameObject _prefab;
        private readonly GameObject _go;
        private string _name;
        
        public GameObject GameObject => _go;
        public bool Active { get; set; } = true;
        
        public PrefabGameObject(GameObject prefab, string name, bool unpack)
        {
            _prefab = prefab;
            _name = name;
            _go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            _go.name = _name;

            if (unpack)
            {
                PrefabUtility.UnpackPrefabInstance(_go, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            }
        }
        
        public void SetParent(GameObject parentGo)
        {
            // Instantiate the child prefab and set it as a child of the new prefab
            _go.transform.SetParent(parentGo.transform);
            _go.SetActive(Active);
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }

        public GameObject FindReferencedId(string id, int index)
        {
            return InstanceGameObject.FindReferencedId(_go, id, index);
        }
        
    }
    
    public class PrimitiveGameObject : IGameObject
    {
        private PrimitiveType _primitiveType;
        private GameObject _go;
        
        public GameObject GameObject => _go;
        
        public PrimitiveGameObject(PrimitiveType primitiveType)
        {
            _primitiveType = primitiveType;
            _go = GameObject.CreatePrimitive(_primitiveType);
        }
        
        public void SetParent(GameObject parentGo)
        {
            _go.transform.SetParent(parentGo.transform);   
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }
    }
    
    public interface IComponent
    {
        public void AddComponent(GameObject gameObject);
    }
    
    [Serializable]
    public class InstanceComponent : IComponent
    {
        public static string DefaultRuntimeAssetPath;
        public static string RuntimeAssetPath;
        public static string CorePath;
        public static string MainLobbyPath;
        
        public string Name { get; set; }
        
        public string[] Params { get; set; }
        
        public string ComponentType { get; set; }
        
        public string Filename { get; set; }
        
        public string Color { get; set; }
        
        public string Text { get; set; }
        
        public int FontSize { get; set; }
        
        public string FontAsset { get; set; }
        
        public string HorizontalAlignment { get; set; }
        
        public string VerticalAlignment { get; set; }
        
        public Rect? Rect { get; set; }
        
        public string ReferenceId { get; set; }
        
        public bool IncludeAudioListener { get; set; }
        
        public ParticleSystemModuleData ParticleSystemModuleData { get; set; }
        
        public List<EventTriggerData> EventTriggers { get; set; }
        
        public List<AnimationState> AnimationStates { get; set; }
        
        public List<AnimatorTransition> AnimatorTransitions { get; set; }
        
        public List<GameObjectProperty> GameObjectsProperty { get; set; }
        
        public List<GameObjectGroupProperty> GameObjectGroupsProperty { get; set; }
        
        public List<AnimatorProperty> AnimatorsProperty { get; set; }
        
        public List<AnimatorGroupProperty> AnimatorsGroupProperty { get; set; }
        
        public List<MeshRendererProperty> MeshRenderersProperty { get; set; }
        
        public List<MeshRendererGroupProperty> MeshRendererGroupsProperty { get; set; }
        
        public List<TextMeshProProperty> TextMeshProsProperty { get; set; }
        
        public List<TextMeshProGroupProperty> TextMeshProGroupsProperty { get; set; }
        
        public List<StringProperty> StringsProperty { get; set; }
        
        public List<StringGroupProperty> StringGroupsProperty { get; set; }
        
        public List<IntProperty> IntsProperty { get; set; }
        
        public List<IntGroupProperty> IntGroupsProperty { get; set; }
        
        public InstanceComponent()
        {
            FontAsset = "Roboto-Bold SDF";
            HorizontalAlignment = "Center";
            VerticalAlignment = "Middle";
            IncludeAudioListener = true;
            Params = Array.Empty<string>();
            GameObjectsProperty = new List<GameObjectProperty>();
            GameObjectGroupsProperty = new List<GameObjectGroupProperty>();
            AnimatorsProperty = new List<AnimatorProperty>();
            AnimatorsGroupProperty = new List<AnimatorGroupProperty>();
            AnimationStates = new List<AnimationState>();
            AnimatorTransitions = new List<AnimatorTransition>();
            TextMeshProsProperty = new List<TextMeshProProperty>();
            TextMeshProGroupsProperty = new List<TextMeshProGroupProperty>();
            MeshRenderersProperty = new List<MeshRendererProperty>();
            MeshRendererGroupsProperty = new List<MeshRendererGroupProperty>();
            EventTriggers = new List<EventTriggerData>();
        }
        
        public void AddComponent(GameObject gameObject)
        {
            switch (ComponentType)
            {
                case "AnimatorController":
                    var animatorComponent = new AnimatorComponent(Filename, AnimationStates, AnimatorTransitions, RuntimeAssetPath);
                    animatorComponent.AddComponent(gameObject);
                    break;
                case "ParticleSystem":
                    var particleSystemComponent = new ParticleSystemComponent(ParticleSystemModuleData, RuntimeAssetPath);
                    particleSystemComponent.AddComponent(gameObject);
                    break;
                case "TextMeshPro":
                    var textMeshProComponent = new TextMeshProComponent(Text, FontSize, Color, HorizontalAlignment, VerticalAlignment, Rect, FontAsset);
                    textMeshProComponent.AddComponent(gameObject);
                    break;
                case "TextMeshProUI":
                    var textMeshProUIComponent = new TextMeshProUIComponent(Text, FontSize, Color, HorizontalAlignment, VerticalAlignment, Rect, FontAsset);
                    textMeshProUIComponent.AddComponent(gameObject);
                    break;
                case "Image":
                    var imageComponent = new ImageComponent(RuntimeAssetPath, Filename, Color, Rect);
                    imageComponent.AddComponent(gameObject);
                    break;
                case "RectTransform":
                    var rectTransformComponent = new RectTransformComponent(RuntimeAssetPath, Filename, Color, Rect);
                    rectTransformComponent.AddComponent(gameObject);
                    break;
                case "UICamera":
                    var uiCameraComponent = new UICameraComponent(IncludeAudioListener);
                    uiCameraComponent.AddComponent(gameObject);
                    break;
                case "BackgroundCamera":
                    var backgroundCameraComponent = new BackgroundCameraComponent();
                    backgroundCameraComponent.AddComponent(gameObject);
                    break;
                case "ReelsCamera":
                    var reelsCameraComponent = new ReelsCameraComponent();
                    reelsCameraComponent.AddComponent(gameObject);
                    break;                
                case "FrameCamera":
                    var frameCameraComponent = new FrameCameraComponent();
                    frameCameraComponent.AddComponent(gameObject);
                    break;                
                case "OverlayCamera":
                    var overlayCameraComponent = new OverlayCameraComponent();
                    overlayCameraComponent.AddComponent(gameObject);
                    break;                
                case "ReelsOverlayCamera":
                    var reelsOverlayCameraComponent = new ReelsOverlayCameraComponent();
                    reelsOverlayCameraComponent.AddComponent(gameObject);
                    break;                
                case "TransitionCamera":
                    var transitionComponent = new TransitionCameraComponent();
                    transitionComponent.AddComponent(gameObject);
                    break;
                case "Canvas":
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(ReferenceId, out var referenceGameObject);
                        var renderCamera = ReferenceId == "screenspaceoverlay" ? null : referenceGameObject?.GameObject.GetComponent<Camera>();
                        var canvasComponent = new CanvasComponent(renderCamera);
                        canvasComponent.AddComponent(gameObject);
                    }
                    break;
                case "EventSystem":
                    var eventSystemComponent = new EventSystemComponent();
                    eventSystemComponent.AddComponent(gameObject);
                    break;
                case "BettrEventListener":
                    var eventListener = gameObject.GetComponent<BettrEventListener>();
                    if (eventListener == null)
                    {
                        eventListener = gameObject.AddComponent<BettrEventListener>();
                    }
                    foreach (var eventTrigger in EventTriggers)
                    {
                        var eventTriggerComponent = new EventTriggerComponent(eventListener, eventTrigger.ReferenceId, eventTrigger.Params);
                        eventTriggerComponent.AddComponent(gameObject);
                        
                    }
                    break;
                case "MonoBehaviour":
                    var monoBehaviourComponent = new MonoBehaviourComponent(Name);
                    monoBehaviourComponent.AddComponent(gameObject);
                    break;
                case "AudioSource":
                    var audioComponent = new AudioComponent(Name);
                    audioComponent.AddComponent(gameObject);
                    break;
                case "DirectionalLight":
                    var directionalLightComponent = new DirectionalLightComponent();
                    directionalLightComponent.AddComponent(gameObject);
                    break;
                case "Tile":
                {
                    var globalTileId = string.IsNullOrEmpty(Name) ? Filename : Name;
                    var scriptAsset = BettrScriptGenerator.CreateOrLoadScript( Filename, RuntimeAssetPath);
                    var tileComponent = new TileComponent(globalTileId, scriptAsset);
                    tileComponent.AddComponent(gameObject);
                    
                    // add in the event triggers
                    var tile = gameObject.GetComponent<Tile>();
                    foreach (var eventTrigger in EventTriggers)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(eventTrigger.ReferenceId, out var referenceGameObject);
                        var eventTriggerComponent = new EventTriggerComponent(tile, eventTrigger.Params);
                        eventTriggerComponent.AddComponent(referenceGameObject?.GameObject);
                        
                    }
                }
                    break;
                case "TileWithUpdate":
                {
                    var globalTileId = string.IsNullOrEmpty(Name) ? Filename : Name;
                    var scriptAsset = BettrScriptGenerator.CreateOrLoadScript(Filename, RuntimeAssetPath);
                    if (scriptAsset == null)
                    {
                        throw new Exception($"Failed to load script asset: Name={Name} Filename={Filename} RuntimeAssetPath={RuntimeAssetPath}");
                    }
                    var tileComponent = new TileWithUpdateComponent(globalTileId, scriptAsset);
                    tileComponent.AddComponent(gameObject);
                    
                    // add in the event triggers
                    var tile = gameObject.GetComponent<TileWithUpdate>();
                    foreach (var eventTrigger in EventTriggers)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(eventTrigger.ReferenceId, out var referenceGameObject);
                        var eventTriggerComponent = new EventTriggerComponent(tile, eventTrigger.Params);
                        eventTriggerComponent.AddComponent(referenceGameObject?.GameObject);
                    }
                }
                    break;

                case "TilePropertyMeshRenderers":
                case "TilePropertyMeshRenderersInjected":
                {
                    var tileMeshRendererProperties = new List<TilePropertyMeshRenderer>();
                    var tileMeshRendererGroupProperties = new List<TilePropertyMeshRendererGroup>();
                    if (ComponentType == "TilePropertyMeshRenderersInjected")
                    {
                        var tilePropertyMeshRenderersComponent = new TilePropertyMeshRenderersInjectedComponent(tileMeshRendererProperties, tileMeshRendererGroupProperties);
                        tilePropertyMeshRenderersComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyMeshRenderersComponent =
                            new TilePropertyMeshRenderersComponent(tileMeshRendererProperties,
                                tileMeshRendererGroupProperties);
                        tilePropertyMeshRenderersComponent.AddComponent(gameObject);
                    }

                    foreach (var kvPair in MeshRenderersProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(kvPair.Id, out var referenceGameObject);
                        var meshRenderer = referenceGameObject?.GameObject.GetComponent<MeshRenderer>();
                        var tilePropertyMeshRenderer = new TilePropertyMeshRenderer()
                        {
                            key = kvPair.Key,
                            value = new PropertyMeshRenderer() {meshRenderer = meshRenderer},
                        };
                        tileMeshRendererProperties.Add(tilePropertyMeshRenderer);
                    }
                    foreach (var kvPair in MeshRendererGroupsProperty)
                    {
                        List<TilePropertyMeshRenderer> meshRenderersProperties = new List<TilePropertyMeshRenderer>();
                        foreach (var property in kvPair.Group)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                            var meshRenderer = referenceGameObject?.GameObject.GetComponent<MeshRenderer>();
                            var gameObjectProperty = new TilePropertyMeshRenderer()
                            {
                                key = property.Key,
                                value = new PropertyMeshRenderer() { meshRenderer = meshRenderer },
                            };
                            meshRenderersProperties.Add(gameObjectProperty);
                        }
                        tileMeshRendererGroupProperties.Add(new TilePropertyMeshRendererGroup()
                        {
                            groupKey = kvPair.GroupKey,
                            meshRendererProperties = meshRenderersProperties,
                        });
                    }
                }
                    break;                
                
                case "TilePropertyTextMeshPros":
                case "TilePropertyTextMeshProsInjected":
                    var tileTextMeshProProperties = new List<TilePropertyTextMeshPro>();
                    var tileTextMeshProGroupProperties = new List<TilePropertyTextMeshProGroup>();
                    if (ComponentType == "TilePropertyTextMeshProsInjected")
                    {
                        var tilePropertyTextMeshProsComponent = new TilePropertyTextMeshProsInjectedComponent(tileTextMeshProProperties, tileTextMeshProGroupProperties);
                        tilePropertyTextMeshProsComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyTextMeshProsComponent =
                            new TilePropertyTextMeshProsComponent(tileTextMeshProProperties,
                                tileTextMeshProGroupProperties);
                        tilePropertyTextMeshProsComponent.AddComponent(gameObject);
                    }

                    foreach (var kvPair in TextMeshProsProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(kvPair.Id, out var referenceGameObject);
                        var textMeshPro = referenceGameObject?.GameObject.GetComponent<TMP_Text>();
                        var tilePropertyTextMeshPro = new TilePropertyTextMeshPro()
                        {
                            key = kvPair.Key,
                            value = new PropertyTextMeshPro() {textMeshPro = textMeshPro, isFormatText = kvPair.IsFormatText},
                        };
                        tileTextMeshProProperties.Add(tilePropertyTextMeshPro);
                    }
                    foreach (var kvPair in TextMeshProGroupsProperty)
                    {
                        List<TilePropertyTextMeshPro> textMeshProsProperties = new List<TilePropertyTextMeshPro>();
                        foreach (var property in kvPair.Group)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                            var textMeshPro = referenceGameObject?.GameObject.GetComponent<TMP_Text>();
                            var gameObjectProperty = new TilePropertyTextMeshPro()
                            {
                                key = property.Key,
                                value = new PropertyTextMeshPro() { textMeshPro = textMeshPro, isFormatText = property.IsFormatText },
                            };
                            textMeshProsProperties.Add(gameObjectProperty);
                        }
                        tileTextMeshProGroupProperties.Add(new TilePropertyTextMeshProGroup()
                        {
                            groupKey = kvPair.GroupKey,
                            textMeshProProperties = textMeshProsProperties,
                        });
                    }
                    break;
                case "TilePropertyGameObjects":
                case "TilePropertyGameObjectsInjected":
                    var tileGameObjectProperties = new List<TilePropertyGameObject>();
                    var tileGameObjectGroupProperties = new List<TilePropertyGameObjectGroup>();
                    if (ComponentType == "TilePropertyGameObjectsInjected")
                    {
                        var tilePropertyGameObjectsComponent = new TilePropertyGameObjectsInjectedComponent(tileGameObjectProperties, tileGameObjectGroupProperties);
                        tilePropertyGameObjectsComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyGameObjectsComponent = new TilePropertyGameObjectsComponent(tileGameObjectProperties, tileGameObjectGroupProperties);
                        tilePropertyGameObjectsComponent.AddComponent(gameObject);
                    }
                    if (GameObjectsProperty != null)
                    {
                        foreach (var kvPair in GameObjectsProperty)
                        {
                            try
                            {
                                InstanceGameObject.IdGameObjects.TryGetValue(kvPair.Id, out var referenceGameObject);
                                var tilePropertyGameObject = new TilePropertyGameObject()
                                {
                                    key = kvPair.Key,
                                    value = new PropertyGameObject() {gameObject = referenceGameObject?.GameObject },
                                };
                                tileGameObjectProperties.Add(tilePropertyGameObject);
                            }
                            catch (Exception e)
                            {
                                Debug.LogError($"Failed to find game object with id: {kvPair.Id}, error: {e.Message}");
                                throw;
                            }
                        }
                    }
                    if (GameObjectGroupsProperty != null)
                    {
                        foreach (var kvPair in GameObjectGroupsProperty)
                        {
                            List<TilePropertyGameObject> gameObjectProperties = new List<TilePropertyGameObject>();
                            foreach (var property in kvPair.Group)
                            {
                                InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                                var gameObjectProperty = new TilePropertyGameObject()
                                {
                                    key = property.Key,
                                    value = new PropertyGameObject() {gameObject = referenceGameObject?.GameObject },
                                };
                                gameObjectProperties.Add(gameObjectProperty);
                            }
                            tileGameObjectGroupProperties.Add(new TilePropertyGameObjectGroup()
                            {
                                groupKey = kvPair.GroupKey,
                                gameObjectProperties = gameObjectProperties,
                            });
                        }
                    }
                    break;
                case "TilePropertyAnimators":
                case "TilePropertyAnimatorsInjected":
                    var properties = new List<TilePropertyAnimator>();
                    var groupProperties = new List<TilePropertyAnimatorGroup>();
                    if (ComponentType == "TilePropertyAnimatorsInjected")
                    {
                        var tilePropertyAnimatorsComponent = new TilePropertyAnimatorsInjectedComponent(properties, groupProperties);
                        tilePropertyAnimatorsComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyAnimatorsComponent = new TilePropertyAnimatorsComponent(properties, groupProperties);
                        tilePropertyAnimatorsComponent.AddComponent(gameObject);
                    }
                    foreach (var kvPair in AnimatorsProperty)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(kvPair.Id, out var referenceGameObject);
                        var tileProperty = new TilePropertyAnimator()
                        {
                            key = kvPair.Key,
                            value = new PropertyAnimator()
                            {
                                animator = referenceGameObject?.Animator, 
                                animationStateName = kvPair.State,
                                delayBeforeAnimationStart = kvPair.DelayBeforeStart,
                                waitForAnimationComplete = kvPair.WaitForComplete,
                                overrideAnimationDuration = kvPair.OverrideDuration,
                                animationDuration = kvPair.AnimationDuration,
                            },
                        };
                        if (tileProperty.value.animator == null)
                        {
                            Debug.LogError($"Failed to find animator with id: {kvPair.Id}");
                        }
                        properties.Add(tileProperty);
                    }
                    foreach (var kvPair in AnimatorsGroupProperty)
                    {
                        List<TilePropertyAnimator> animatorProperties = new List<TilePropertyAnimator>();
                        foreach (var property in kvPair.Group)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                            var tileProperty = new TilePropertyAnimator()
                            {
                                key = property.Key,
                                value = new PropertyAnimator() {
                                    animator = referenceGameObject?.Animator, 
                                    animationStateName = property.State,
                                    delayBeforeAnimationStart = property.DelayBeforeStart,
                                    waitForAnimationComplete = property.WaitForComplete,
                                    overrideAnimationDuration = property.OverrideDuration,
                                    animationDuration = property.AnimationDuration,
                                },
                            };
                            if (tileProperty.value.animator == null)
                            {
                                Debug.LogError($"Failed to find animator with id: {property.Id}");
                            }
                            animatorProperties.Add(tileProperty);
                        }
                        groupProperties.Add(new TilePropertyAnimatorGroup()
                        {
                            groupKey = kvPair.GroupKey,
                            tileAnimatorProperties = animatorProperties,
                        });
                    }
                    break;
                case "TilePropertyInts":
                case "TilePropertyIntsInjected":
                    var tileIntProperties = new List<TilePropertyInt>();
                    var tileIntGroupProperties = new List<TilePropertyIntGroup>();
                    if (ComponentType == "TilePropertyIntsInjected")
                    {
                        var tilePropertyIntsComponent = new TilePropertyIntsInjectedComponent(tileIntProperties, tileIntGroupProperties);
                        tilePropertyIntsComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyIntsComponent = new TilePropertyIntsComponent(tileIntProperties, tileIntGroupProperties);
                        tilePropertyIntsComponent.AddComponent(gameObject);
                    }
                    
                    if (IntsProperty != null)
                    {
                        foreach (var kvPair in IntsProperty)
                        {
                            var tilePropertyInt = new TilePropertyInt()
                            {
                                key = kvPair.Key,
                                value = kvPair.Value,
                            };
                            tileIntProperties.Add(tilePropertyInt);
                        }
                    }
                    if (IntGroupsProperty != null)
                    {
                        foreach (var kvPair in IntGroupsProperty)
                        {
                            List<TilePropertyInt> tilePropertyInts = new List<TilePropertyInt>();
                            foreach (var property in kvPair.Group)
                            {
                                var tilePropertyString = new TilePropertyInt()
                                {
                                    key = property.Key,
                                    value = property.Value,
                                };
                                tilePropertyInts.Add(tilePropertyString);
                            }
                            tileIntGroupProperties.Add(new TilePropertyIntGroup()
                            {
                                groupKey = kvPair.GroupKey,
                                values = tilePropertyInts,
                            });
                        }
                    }
                    break;                
                case "TilePropertyStrings":
                case "TilePropertyStringsInjected":
                    var tileStringProperties = new List<TilePropertyString>();
                    var tileStringGroupProperties = new List<TilePropertyStringGroup>();
                    if (ComponentType == "TilePropertyStringsInjected")
                    {
                        var tilePropertyStringsComponent = new TilePropertyStringsInjectedComponent(tileStringProperties, tileStringGroupProperties);
                        tilePropertyStringsComponent.AddComponent(gameObject);
                    }
                    else
                    {
                        var tilePropertyStringsComponent =
                            new TilePropertyStringsComponent(tileStringProperties, tileStringGroupProperties);
                        tilePropertyStringsComponent.AddComponent(gameObject);
                    }

                    if (StringsProperty != null)
                    {
                        foreach (var kvPair in StringsProperty)
                        {
                            var tilePropertyString = new TilePropertyString()
                            {
                                key = kvPair.Key,
                                value = kvPair.Value,
                            };
                            tileStringProperties.Add(tilePropertyString);
                        }
                    }
                    if (StringGroupsProperty != null)
                    {
                        foreach (var kvPair in StringGroupsProperty)
                        {
                            List<TilePropertyString> tilePropertyStrings = new List<TilePropertyString>();
                            foreach (var property in kvPair.Group)
                            {
                                var tilePropertyString = new TilePropertyString()
                                {
                                    key = property.Key,
                                    value = property.Value,
                                };
                                tilePropertyStrings.Add(tilePropertyString);
                            }
                            tileStringGroupProperties.Add(new TilePropertyStringGroup()
                            {
                                groupKey = kvPair.GroupKey,
                                values = tilePropertyStrings,
                            });
                        }
                    }
                    break;
            }
        }
    }

    [Serializable]
    public class EventTriggerData
    {
        // ReSharper disable once InconsistentNaming
        public string ReferenceId;

        // ReSharper disable once InconsistentNaming
        public string[] Params;

        public EventTriggerData()
        {
            Params = Array.Empty<string>();
        }
    }
    
    [Serializable]
    public class TileComponent : IComponent
    {
        private readonly TextAsset _scriptAsset;
        private readonly string _globalTileId;
        
        public TileComponent(string globalTileId, TextAsset scriptAsset)
        {
            _globalTileId = globalTileId;
            _scriptAsset = scriptAsset;
        }

        public void AddComponent(GameObject gameObject)
        {
            var tile = gameObject.AddComponent<Tile>();
            tile.scriptAsset = _scriptAsset;
            tile.globalTileId = _globalTileId;
        }
    }
    
    [Serializable]
    public class TileWithUpdateComponent : IComponent
    {
        private readonly TextAsset _scriptAsset;
        private readonly string _globalTileId;
        
        public TileWithUpdateComponent(string globalTileId, TextAsset scriptAsset)
        {
            _globalTileId = globalTileId;
            _scriptAsset = scriptAsset;
        }

        public void AddComponent(GameObject gameObject)
        {
            var tile = gameObject.AddComponent<TileWithUpdate>();
            tile.scriptAsset = _scriptAsset;
            tile.globalTileId = _globalTileId;
        }
    }
    
    [Serializable]
    public class MonoBehaviourComponent : IComponent
    {
        private readonly Type _scriptType;
        
        public MonoBehaviourComponent(string scriptName)
        {
            Assembly runtimeAssembly = Assembly.Load("casino.bettr.plugin.Core");
            _scriptType = runtimeAssembly.GetType(scriptName);
        }

        public void AddComponent(GameObject gameObject)
        {
            gameObject.AddComponent(_scriptType);
        }
    }
    
    [Serializable]
    public class DirectionalLightComponent : IComponent
    {
        public DirectionalLightComponent()
        {
        }

        public void AddComponent(GameObject gameObject)
        {
            var light = gameObject.AddComponent<Light>();
            // Set the Light Type to Directional
            light.type = LightType.Directional;
            // Set the Light Color to white
            light.color = Color.white;
            // Set the Light Intensity to 1.0
            light.intensity = 1.0f;
            // Set the Light Rotation to 45, 45, 0
            light.transform.rotation = Quaternion.Euler(50, -30, 0);
            // Set the Light Position to 0, 0, 0
            light.transform.position = new Vector3(0, 0, 0);
            // no shadows
            light.shadows = LightShadows.None;
            
            string scenesDirectory = Path.Combine(InstanceComponent.RuntimeAssetPath, "Scenes");
            
            string lightingSettingsPath = $"{scenesDirectory}/LightingSettings.asset";
            
            LightingSettings existingSettings = AssetDatabase.LoadAssetAtPath<LightingSettings>(lightingSettingsPath);
            if (existingSettings != null)
            {
                Debug.Log($"LightingSettings asset already exists: {lightingSettingsPath}");
                Lightmapping.lightingSettings = existingSettings; // Assign existing settings
                return; // Skip creating a new asset
            }
            
            LightingSettings lightingSettings = new LightingSettings();
            AssetDatabase.CreateAsset(lightingSettings, lightingSettingsPath);
            Lightmapping.lightingSettings = lightingSettings;
        }
    }

    
    [Serializable]
    public class AudioComponent : IComponent
    {
        private readonly string _audioSourceFilename;
        
        private const string AUDIO_FORMAT = ".wav";
        
        public AudioComponent(string audioFileName)
        {
            string audioDirectory = Path.Combine(InstanceComponent.CorePath, "Audio");
            // get the audio files under the audio directory
            string[] audioFiles = Directory.GetFiles(audioDirectory, $"*{AUDIO_FORMAT}", SearchOption.AllDirectories);
            foreach (var audioFile in audioFiles)
            {
                var fileNameWithoutExtension = Path.GetFileNameWithoutExtension(audioFile);
                if (fileNameWithoutExtension == audioFileName)
                {
                    _audioSourceFilename = audioFile;
                    break;
                }
            }
        }

        public void AddComponent(GameObject gameObject)
        {
            if (_audioSourceFilename == null)
            {
                Debug.LogError($"Failed to find audio file with name: {_audioSourceFilename}");
                return;
            }
            // load the audio clip
            AudioClip audioClip = AssetDatabase.LoadAssetAtPath<AudioClip>(_audioSourceFilename);
            if (audioClip == null)
            {
                Debug.LogError($"Failed to load audio clip: {_audioSourceFilename}");
                return;
            }
                            
            // add an audio source to the top level game object of the machine prefab
            AudioSource audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.clip = audioClip;
            audioSource.playOnAwake = false;
            audioSource.loop = true;
            audioSource.pitch = 1;
            audioSource.priority = 128;
            audioSource.spatialBlend = 0; // 0 = 2D
            audioSource.volume = 1; // can turn off using Key "V"
            audioSource.panStereo = 0; // -1 = left, 1 = right
        }
    }

    [Serializable]
    public class AnimatorProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Id;
        
        // ReSharper disable once InconsistentNaming
        public string State;
        
        // ReSharper disable once InconsistentNaming
        public float AnimationDuration;

        // ReSharper disable once InconsistentNaming
        public float DelayBeforeStart;

        // ReSharper disable once InconsistentNaming
        public bool WaitForComplete;

        // ReSharper disable once InconsistentNaming
        public bool OverrideDuration;
    }
    
    [Serializable]
    public class AnimatorGroupProperty
    {
        public string GroupKey;

        public List<AnimatorProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyAnimatorsInjectedComponent : IComponent
    {
        public List<TilePropertyAnimator> tileAnimatorProperties;
        public List<TilePropertyAnimatorGroup> tileAnimatorGroupProperties;
        
        public TilePropertyAnimatorsInjectedComponent(List<TilePropertyAnimator> tileAnimatorProperties, List<TilePropertyAnimatorGroup> tileAnimatorGroupProperties)
        {
            this.tileAnimatorProperties = tileAnimatorProperties;
            this.tileAnimatorGroupProperties = tileAnimatorGroupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyAnimatorsInjected>();
            component.tileAnimatorProperties = tileAnimatorProperties;
            component.tileAnimatorGroupProperties = tileAnimatorGroupProperties;
        }
    }
    
    
    [Serializable]
    public class TilePropertyAnimatorsComponent : IComponent
    {
        public List<TilePropertyAnimator> tileAnimatorProperties;
        public List<TilePropertyAnimatorGroup> tileAnimatorGroupProperties;
        
        public TilePropertyAnimatorsComponent(List<TilePropertyAnimator> tileAnimatorProperties, List<TilePropertyAnimatorGroup> tileAnimatorGroupProperties)
        {
            this.tileAnimatorProperties = tileAnimatorProperties;
            this.tileAnimatorGroupProperties = tileAnimatorGroupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyAnimators>();
            component.tileAnimatorProperties = tileAnimatorProperties;
            component.tileAnimatorGroupProperties = tileAnimatorGroupProperties;
        }
    }
    
    [Serializable]
    public class GameObjectProperty
    {
        public string Key;

        public string Id;
    }
    
    [Serializable]
    public class GameObjectGroupProperty
    {
        public string GroupKey;

        public List<GameObjectProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyGameObjectsInjectedComponent : IComponent
    {
        private readonly List<TilePropertyGameObject> _tileGameObjectProperties;
        private readonly List<TilePropertyGameObjectGroup> _tileGameObjectGroupProperties;
        
        public TilePropertyGameObjectsInjectedComponent(List<TilePropertyGameObject> tileGameObjectProperties, List<TilePropertyGameObjectGroup> tileGameObjectGroupProperties)
        {
            this._tileGameObjectProperties = tileGameObjectProperties;
            this._tileGameObjectGroupProperties = tileGameObjectGroupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyGameObjectsInjected>();
            component.tileGameObjectProperties = _tileGameObjectProperties;
            component.tileGameObjectGroupProperties = _tileGameObjectGroupProperties;
        }
    }
    
    [Serializable]
    public class TilePropertyGameObjectsComponent : IComponent
    {
        private readonly List<TilePropertyGameObject> _tileGameObjectProperties;
        private readonly List<TilePropertyGameObjectGroup> _tileGameObjectGroupProperties;
        
        public TilePropertyGameObjectsComponent(List<TilePropertyGameObject> tileGameObjectProperties, List<TilePropertyGameObjectGroup> tileGameObjectGroupProperties)
        {
            this._tileGameObjectProperties = tileGameObjectProperties;
            this._tileGameObjectGroupProperties = tileGameObjectGroupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyGameObjects>();
            component.tileGameObjectProperties = _tileGameObjectProperties;
            component.tileGameObjectGroupProperties = _tileGameObjectGroupProperties;
        }
    }
    
    [Serializable]
    public class MeshRendererProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Id;
    }
    
    [Serializable]
    public class MeshRendererGroupProperty
    {
        // ReSharper disable once InconsistentNaming
        public string GroupKey;

        // ReSharper disable once InconsistentNaming
        public List<MeshRendererProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyMeshRenderersInjectedComponent : IComponent
    {
        private readonly List<TilePropertyMeshRenderer> _tileMeshRendererProperties;
        private readonly List<TilePropertyMeshRendererGroup> _tileTextMeshProGroupProperties;
        
        public TilePropertyMeshRenderersInjectedComponent(List<TilePropertyMeshRenderer> properties, List<TilePropertyMeshRendererGroup> groupProperties)
        {
            this._tileMeshRendererProperties = properties;
            this._tileTextMeshProGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyMeshRenderersInjected>();
            component.tileMeshRendererProperties = _tileMeshRendererProperties;
            component.tileMeshRendererGroupProperties = _tileTextMeshProGroupProperties;
        }
    }
    
    [Serializable]
    public class TilePropertyMeshRenderersComponent : IComponent
    {
        private readonly List<TilePropertyMeshRenderer> _tileTextMeshProProperties;
        private readonly List<TilePropertyMeshRendererGroup> _tileTextMeshProGroupProperties;
        
        public TilePropertyMeshRenderersComponent(List<TilePropertyMeshRenderer> properties, List<TilePropertyMeshRendererGroup> groupProperties)
        {
            this._tileTextMeshProProperties = properties;
            this._tileTextMeshProGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyMeshRenderers>();
            component.tileMeshRendererProperties = _tileTextMeshProProperties;
            component.tileMeshRendererGroupProperties = _tileTextMeshProGroupProperties;
        }
    }
    
    [Serializable]
    public class TextMeshProProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Id;
        
        // ReSharper disable once InconsistentNaming
        public bool IsFormatText;
    }
    
    [Serializable]
    public class TextMeshProGroupProperty
    {
        // ReSharper disable once InconsistentNaming
        public string GroupKey;

        // ReSharper disable once InconsistentNaming
        public List<TextMeshProProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyTextMeshProsInjectedComponent : IComponent
    {
        private readonly List<TilePropertyTextMeshPro> _tileTextMeshProProperties;
        private readonly List<TilePropertyTextMeshProGroup> _tileTextMeshProGroupProperties;
        
        public TilePropertyTextMeshProsInjectedComponent(List<TilePropertyTextMeshPro> properties, List<TilePropertyTextMeshProGroup> groupProperties)
        {
            this._tileTextMeshProProperties = properties;
            this._tileTextMeshProGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyTextMeshProsInjected>();
            component.tileTextMeshProProperties = _tileTextMeshProProperties;
            component.tileTextMeshProGroupProperties = _tileTextMeshProGroupProperties;
        }
    }
    
    [Serializable]
    public class TilePropertyTextMeshProsComponent : IComponent
    {
        private readonly List<TilePropertyTextMeshPro> _tileTextMeshProProperties;
        private readonly List<TilePropertyTextMeshProGroup> _tileTextMeshProGroupProperties;
        
        public TilePropertyTextMeshProsComponent(List<TilePropertyTextMeshPro> properties, List<TilePropertyTextMeshProGroup> groupProperties)
        {
            this._tileTextMeshProProperties = properties;
            this._tileTextMeshProGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyTextMeshPros>();
            component.tileTextMeshProProperties = _tileTextMeshProProperties;
            component.tileTextMeshProGroupProperties = _tileTextMeshProGroupProperties;
        }
    }
    
    [Serializable]
    public class ParticleSystemProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Id;
        
        // ReSharper disable once InconsistentNaming
        public float DelayBeforeStart;
        
        // ReSharper disable once InconsistentNaming
        public float Duration;
        
        // ReSharper disable once InconsistentNaming
        public bool WaitForComplete;
    }
    
    [Serializable]
    public class ParticleSystemGroupProperty
    {
        // ReSharper disable once InconsistentNaming
        public string GroupKey;

        // ReSharper disable once InconsistentNaming
        public List<ParticleSystemProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyParticleSystemsComponent : IComponent
    {
        private readonly List<TilePropertyParticleSystem> _tileTextMeshProProperties;
        private readonly List<TilePropertyParticleSystemGroup> _tileTextMeshProGroupProperties;
        
        public TilePropertyParticleSystemsComponent(List<TilePropertyParticleSystem> properties, List<TilePropertyParticleSystemGroup> groupProperties)
        {
            this._tileTextMeshProProperties = properties;
            this._tileTextMeshProGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyParticleSystems>();
            component.tileParticleSystemProperties = _tileTextMeshProProperties;
            component.tileParticleSystemGroupProperties = _tileTextMeshProGroupProperties;
        }
    }
    
    [Serializable]
    public class IntProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public long Value;
    }
    
    [Serializable]
    public class IntGroupProperty
    {
        public string GroupKey;

        public List<IntProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyIntsInjectedComponent : IComponent
    {
        private readonly List<TilePropertyInt> _tileIntProperties;
        private readonly List<TilePropertyIntGroup> _tileIntGroupProperties;
        
        public TilePropertyIntsInjectedComponent(List<TilePropertyInt> properties, List<TilePropertyIntGroup> groupProperties)
        {
            this._tileIntProperties = properties;
            this._tileIntGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyIntsInjected>();
            component.tileGameIntProperties = _tileIntProperties;
            component.tileGameIntGroupProperties = _tileIntGroupProperties;
        }
    }
    
    [Serializable]
    public class TilePropertyIntsComponent : IComponent
    {
        private readonly List<TilePropertyInt> _tileIntProperties;
        private readonly List<TilePropertyIntGroup> _tileIntGroupProperties;
        
        public TilePropertyIntsComponent(List<TilePropertyInt> properties, List<TilePropertyIntGroup> groupProperties)
        {
            this._tileIntProperties = properties;
            this._tileIntGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyInts>();
            component.tileGameIntProperties = _tileIntProperties;
            component.tileGameIntGroupProperties = _tileIntGroupProperties;
        }
    }
    
    [Serializable]
    public class StringProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Value;
    }
    
    [Serializable]
    public class StringGroupProperty
    {
        public string GroupKey;

        public List<StringProperty> Group;
    }
    
    [Serializable]
    public class TilePropertyStringsInjectedComponent : IComponent
    {
        private readonly List<TilePropertyString> _tileStringProperties;
        private readonly List<TilePropertyStringGroup> _tileStringGroupProperties;
        
        public TilePropertyStringsInjectedComponent(List<TilePropertyString> properties, List<TilePropertyStringGroup> groupProperties)
        {
            this._tileStringProperties = properties;
            this._tileStringGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyStringsInjected>();
            component.tileGameStringProperties = _tileStringProperties;
            component.tileGameStringGroupProperties = _tileStringGroupProperties;
        }
    }
    
    [Serializable]
    public class TilePropertyStringsComponent : IComponent
    {
        private readonly List<TilePropertyString> _tileStringProperties;
        private readonly List<TilePropertyStringGroup> _tileStringGroupProperties;
        
        public TilePropertyStringsComponent(List<TilePropertyString> properties, List<TilePropertyStringGroup> groupProperties)
        {
            this._tileStringProperties = properties;
            this._tileStringGroupProperties = groupProperties;
        }

        public void AddComponent(GameObject gameObject)
        {
            var component = gameObject.AddComponent<TilePropertyStrings>();
            component.tileGameStringProperties = _tileStringProperties;
            component.tileGameStringGroupProperties = _tileStringGroupProperties;
        }
    }

    public class ParticleSystemModuleData
    {
        public float StartLifetime { get; set; }
        // ReSharper disable once InconsistentNaming
        public float StartSpeed { get; set; }
        // ReSharper disable once InconsistentNaming
        public float StartSize { get; set; }
        // ReSharper disable once InconsistentNaming
        public string StartColor { get; set; } // Stored as a string in RGBA format
        // ReSharper disable once InconsistentNaming
        public float GravityModifier { get; set; }
        // ReSharper disable once InconsistentNaming
        
        public float EmissionRateOverTime { get; set; }
        // ReSharper disable once InconsistentNaming
        public float EmissionRateOverDistance { get; set; }
        // ReSharper disable once InconsistentNaming
        public List<EmissionBurst> Bursts { get; set; } = new List<EmissionBurst>();
        
        // ReSharper disable once InconsistentNaming
        public string Shape { get; set; }
        // ReSharper disable once InconsistentNaming
        public float ShapeAngle { get; set; }
        // ReSharper disable once InconsistentNaming
        public float ShapeRadius { get; set; }
        // ReSharper disable once InconsistentNaming
        public float ShapeRadiusThickness { get; set; }
        // ReSharper disable once InconsistentNaming
        public float ShapeArc { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ShapeArcMode { get; set; }
        // ReSharper disable once InconsistentNaming
        public float ShapeSpread { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ShapeEmitFrom { get; set; }
        // ReSharper disable once InconsistentNaming
        public Vector3 ShapePosition { get; set; }
        // ReSharper disable once InconsistentNaming
        public Vector3 ShapeRotation { get; set; }
        // ReSharper disable once InconsistentNaming
        public Vector3 ShapeScale { get; set; }
        
        public float ShapeArcSpeed { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string SimulationSpace { get; set; }
        // ReSharper disable once InconsistentNaming
        public bool Looping { get; set; }
        // ReSharper disable once InconsistentNaming
        public float Duration { get; set; }
        // ReSharper disable once InconsistentNaming
        public bool PlayOnAwake { get; set; }

        // Additional properties
        // ReSharper disable once InconsistentNaming
        public float StartRotation { get; set; }
        // ReSharper disable once InconsistentNaming
        public float StartDelay { get; set; }
        // ReSharper disable once InconsistentNaming
        public bool Prewarm { get; set; }
        // ReSharper disable once InconsistentNaming
        public int MaxParticles { get; set; }

        // ReSharper disable once InconsistentNaming
        public ParticleSystemRendererSettings RendererSettings { get; set; } = new ParticleSystemRendererSettings();
        
        [Serializable]
        public class ParticleSystemRendererSettings
        {
            // ReSharper disable once InconsistentNaming
            public string Material { get; set; }
            // ReSharper disable once InconsistentNaming
            public string Texture { get; set; }
            // ReSharper disable once InconsistentNaming
            public string Color { get; set; }
            // ReSharper disable once InconsistentNaming
            public float Alpha { get; set; }
            // ReSharper disable once InconsistentNaming
            public string Shader { get; set; }
            // ReSharper disable once InconsistentNaming
            public int SortingOrder { get; set; }
            // ReSharper disable once InconsistentNaming
            
            // ReSharper disable once InconsistentNaming
            public string SortingLayer { get; set; }
            public string RenderMode { get; set; }
            // ReSharper disable once InconsistentNaming
            public float NormalDirection { get; set; }
            // ReSharper disable once InconsistentNaming
            public string SortMode { get; set; }
            // ReSharper disable once InconsistentNaming
            public float MinParticleSize { get; set; }
            // ReSharper disable once InconsistentNaming
            public float MaxParticleSize { get; set; }
            // ReSharper disable once InconsistentNaming
            public string RenderAlignment { get; set; }
            // ReSharper disable once InconsistentNaming
            public bool FlipX { get; set; }
            // ReSharper disable once InconsistentNaming
            public bool FlipY { get; set; }
            // ReSharper disable once InconsistentNaming
            public Vector3 Pivot { get; set; }
            // ReSharper disable once InconsistentNaming
            public bool AllowRoll { get; set; }
            // ReSharper disable once InconsistentNaming
            public bool CastShadows { get; set; }
            // ReSharper disable once InconsistentNaming
            public bool ReceiveShadows { get; set; }
            // ReSharper disable once InconsistentNaming
            public string LightProbes { get; set; }
        }
        
        [Serializable]
        public class EmissionBurst
        {
            // ReSharper disable once InconsistentNaming
            public float Time { get; set; }
            // ReSharper disable once InconsistentNaming
            public short MinCount { get; set; }
            // ReSharper disable once InconsistentNaming
            public short MaxCount { get; set; }
            // ReSharper disable once InconsistentNaming
            public int Cycles { get; set; }
            // ReSharper disable once InconsistentNaming
            public float Interval { get; set; }
            // ReSharper disable once InconsistentNaming
            public float Probability { get; set; }
        }
        
        public Color32 GetStartColor()
        {
            if (ColorUtility.TryParseHtmlString(StartColor, out var color))
            {
                return color;
            }
            return new Color32(255, 255, 255, 255); // Default to white if parsing fails
        }
    }
    
    [Serializable]
    public class ParticleSystemComponent : IComponent
    {
        private ParticleSystem _particleSystem;

        private string _runtimeAssetPath;

        private ParticleSystemModuleData _moduleData;

        public ParticleSystemComponent(ParticleSystemModuleData moduleData, string runtimeAssetPath)
        {
            _runtimeAssetPath = runtimeAssetPath;
            _moduleData = moduleData;
        }

        public void AddComponent(GameObject gameObject)
        {
            BuildParticleSystem(gameObject);

            var particleSystem = _particleSystem;
            
            var mainModule = particleSystem.main;
            var emissionModule = particleSystem.emission;
            var shapeModule = particleSystem.shape;
            var renderer = particleSystem.GetComponent<ParticleSystemRenderer>();

            mainModule.playOnAwake = _moduleData.PlayOnAwake;
            mainModule.startLifetime = _moduleData.StartLifetime;
            mainModule.startSpeed = _moduleData.StartSpeed;
            mainModule.startSize = _moduleData.StartSize;
            mainModule.startColor = new ParticleSystem.MinMaxGradient(_moduleData.GetStartColor());
            mainModule.gravityModifier = _moduleData.GravityModifier;
            if (Enum.TryParse(_moduleData.SimulationSpace, out ParticleSystemSimulationSpace simulationSpace))
            {
                mainModule.simulationSpace = simulationSpace;
            }
            mainModule.loop = _moduleData.Looping;
            mainModule.duration = _moduleData.Duration;
            mainModule.startRotation = _moduleData.StartRotation;
            mainModule.startDelay = _moduleData.StartDelay;
            mainModule.prewarm = _moduleData.Prewarm;
            mainModule.maxParticles = _moduleData.MaxParticles;

            // Emission module settings
            emissionModule.rateOverTime = _moduleData.EmissionRateOverTime;
            emissionModule.rateOverDistance = _moduleData.EmissionRateOverDistance;
            emissionModule.burstCount = _moduleData.Bursts.Count;
            for (int i = 0; i < _moduleData.Bursts.Count; i++)
            {
                var burst = _moduleData.Bursts[i];
                emissionModule.SetBurst(i, new ParticleSystem.Burst(burst.Time, burst.MinCount, burst.MaxCount, burst.Cycles, burst.Interval) { probability = burst.Probability });
            }

            // Shape module settings
            shapeModule.shapeType = (ParticleSystemShapeType)Enum.Parse(typeof(ParticleSystemShapeType), _moduleData.Shape);
            shapeModule.angle = _moduleData.ShapeAngle;
            shapeModule.radius = _moduleData.ShapeRadius;
            shapeModule.radiusThickness = _moduleData.ShapeRadiusThickness;
            shapeModule.arc = _moduleData.ShapeArc;
            
            // Set shape mode if applicable
            if (Enum.TryParse(_moduleData.ShapeArcMode, out ParticleSystemShapeMultiModeValue shapeMode))
            {
                shapeModule.arcMode = shapeMode;
            }
            
            shapeModule.arcSpread = _moduleData.ShapeSpread;
            shapeModule.arcSpeed = _moduleData.ShapeArcSpeed; // Set arc speed
            shapeModule.position = _moduleData.ShapePosition;
            shapeModule.rotation = _moduleData.ShapeRotation;
            shapeModule.scale = _moduleData.ShapeScale;

            // Renderer module settings
            if (Enum.TryParse(_moduleData.RendererSettings.RenderMode, out ParticleSystemRenderMode renderMode))
            {
                renderer.renderMode = renderMode;
            }
            renderer.normalDirection = _moduleData.RendererSettings.NormalDirection;
            if (Enum.TryParse(_moduleData.RendererSettings.SortMode, out ParticleSystemSortMode sortMode))
            {
                renderer.sortMode = sortMode;
            }
            renderer.minParticleSize = _moduleData.RendererSettings.MinParticleSize;
            renderer.maxParticleSize = _moduleData.RendererSettings.MaxParticleSize;
            if (Enum.TryParse(_moduleData.RendererSettings.RenderAlignment, out ParticleSystemRenderSpace renderAlignment))
            {
                renderer.alignment = renderAlignment;
            }
            renderer.flip = new Vector3(_moduleData.RendererSettings.FlipX ? 1 : 0, _moduleData.RendererSettings.FlipY ? 1 : 0, 0);
            renderer.pivot = _moduleData.RendererSettings.Pivot;
            renderer.allowRoll = _moduleData.RendererSettings.AllowRoll;
            renderer.receiveShadows = _moduleData.RendererSettings.ReceiveShadows;
            renderer.shadowCastingMode = _moduleData.RendererSettings.CastShadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
            if (Enum.TryParse(_moduleData.RendererSettings.LightProbes, out LightProbeUsage lightProbeUsage))
            {
                renderer.lightProbeUsage = lightProbeUsage;
            }

            renderer.sortingOrder = _moduleData.RendererSettings.SortingOrder;
            renderer.sortingLayerName = _moduleData.RendererSettings.SortingLayer;
            
            // Check if material properties are provided before generating the material
            Material material = null;
            if (!string.IsNullOrEmpty(_moduleData.RendererSettings.Material) &&
                !string.IsNullOrEmpty(_moduleData.RendererSettings.Shader))
            {
                material = BettrMaterialGenerator.CreateOrLoadMaterial(
                    _moduleData.RendererSettings.Material,
                    _moduleData.RendererSettings.Shader,
                    _moduleData.RendererSettings.Texture,
                    _moduleData.RendererSettings.Color,
                    _moduleData.RendererSettings.Alpha,
                    _runtimeAssetPath
                );
            }

            // Set the material to the renderer
            if (material != null)
            {
                renderer.material = material;
            }

            renderer.sortingOrder = _moduleData.RendererSettings.SortingOrder;
        }

        private void BuildParticleSystem(GameObject gameObject)
        {
            var particleSystem = BettrParticleSystem.AddOrGetParticleSystem(gameObject);
            _particleSystem = particleSystem;
        }
    }
    
    [Serializable]
    public struct AnimationKeyframes
    {
        // ReSharper disable once InconsistentNaming
        public float[] Times;
        // ReSharper disable once InconsistentNaming
        public float[] Values;
    }

    [Serializable]
    public struct AnimationDopesheet
    {
        // ReSharper disable once InconsistentNaming
        public string Path;
        // ReSharper disable once InconsistentNaming
        public string Type;
        // ReSharper disable once InconsistentNaming
        public string Property;
        // ReSharper disable once InconsistentNaming
        public AnimationKeyframes Keyframes;
    }
    
    [Serializable]
    public class AnimationState
    {
        // ReSharper disable once InconsistentNaming
        public string Name;
        
        // ReSharper disable once InconsistentNaming
        public bool IsLoop;
        
        // ReSharper disable once InconsistentNaming
        public bool IsDefault;
        
        // ReSharper disable once InconsistentNaming
        public int LoopTime;
        
        // ReSharper disable once InconsistentNaming
        public int Speed;
                
        // ReSharper disable once InconsistentNaming
        public List<AnimationDopesheet> Dopesheet;
    }

    [Serializable]
    public class AnimatorTransition
    {
        // ReSharper disable once InconsistentNaming
        public string TransitionFrom;
        
        // ReSharper disable once InconsistentNaming
        public string TransitionTo;
        
        // ReSharper disable once InconsistentNaming
        public int TransitionDuration;
        
        // ReSharper disable once InconsistentNaming
        public int TransitionType;
    }
    
    [Serializable]
    public class AnimatorComponent : IComponent
    {
        private AnimatorController _animatorController;

        private string _runtimeAssetPath;
        
        private string _fileName;

        private List<AnimationState> _animationStates;
        
        private List<AnimatorTransition> _animationTransitions;
        
        public AnimatorComponent(string fileName, List<AnimationState> animationStates, List<AnimatorTransition> animationTransitions, string runtimeAssetPath)
        {
            _fileName = fileName;
            _runtimeAssetPath = runtimeAssetPath;
            _animationStates = animationStates;
            _animationTransitions = animationTransitions;
        }

        public AnimatorComponent(AnimatorController animatorController)
        {
            _animatorController = animatorController;
        }

        public void AddComponent(GameObject gameObject)
        {
            if (_animatorController == null)
            {
                BuildAnimatorController(gameObject);
            }
            
            var animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = _animatorController;
        }

        private void BuildAnimatorController(GameObject gameObject)
        {
            var runtimeAnimatorController = BettrAnimatorController.CreateOrLoadAnimatorController(_fileName, _animationStates, _animationTransitions, _runtimeAssetPath);
            _animatorController = runtimeAnimatorController;
        }
    }

    [Serializable]
    public class UICameraComponent : IComponent
    {

        private bool _includeAudioListener;
        
        public UICameraComponent(bool includeAudioListener = true)
        {
            _includeAudioListener = includeAudioListener;
        }
        
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("UI");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("UI");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = 10;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            if (_includeAudioListener)
            {
                gameObject.AddComponent<AudioListener>();
            }
        }
    }
    
    [Serializable]
    public class BackgroundCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_BACKGROUND");
            uiCamera.clearFlags = CameraClearFlags.SolidColor;
            uiCamera.backgroundColor = Color.black;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_BACKGROUND");
            uiCamera.orthographic = false;
            uiCamera.fieldOfView = 12.3f;
            // uiCamera.orthographicSize = 4.1f;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = -13;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
        }
    }
    
    [Serializable]
    public class ReelsCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_REELS");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_REELS");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 4.1f;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = -10;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            var physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            physicsRaycaster.eventMask = LayerMask.GetMask("SLOT_REELS");
            physicsRaycaster.maxRayIntersections = 0;
        }
    }
    
    [Serializable]
    public class FrameCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_FRAME");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_FRAME");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = -5;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            var physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            physicsRaycaster.eventMask = LayerMask.GetMask("SLOT_FRAME");
            physicsRaycaster.maxRayIntersections = 0;
        }
    }
    
    [Serializable]
    public class OverlayCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_OVERLAY");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_OVERLAY");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 5;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = -1;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            var physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            physicsRaycaster.eventMask = LayerMask.GetMask("SLOT_OVERLAY");
            physicsRaycaster.maxRayIntersections = 0;
        }
    }
    
    [Serializable]
    public class ReelsOverlayCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_REELS_OVERLAY");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_REELS_OVERLAY");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 4.1f;
            uiCamera.nearClipPlane = 0.3f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = 1;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = true;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            var physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            physicsRaycaster.eventMask = LayerMask.GetMask("SLOT_REELS_OVERLAY");
            physicsRaycaster.maxRayIntersections = 0;
        }
    }
    
    [Serializable]
    public class TransitionCameraComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var uiCamera = gameObject.AddComponent<Camera>();
            gameObject.layer = LayerMask.NameToLayer("SLOT_TRANSITION");
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_TRANSITION");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 4.1f;
            uiCamera.nearClipPlane = 1f;
            uiCamera.farClipPlane = 1000;
            uiCamera.depth = 2;
            uiCamera.useOcclusionCulling = true;
            uiCamera.renderingPath = RenderingPath.UsePlayerSettings;
            uiCamera.allowHDR = false;
            uiCamera.allowMSAA = true;
            uiCamera.allowDynamicResolution = false;
            uiCamera.targetDisplay = 0;
            
            var physicsRaycaster = gameObject.AddComponent<PhysicsRaycaster>();
            physicsRaycaster.eventMask = LayerMask.GetMask("SLOT_TRANSITION");
            physicsRaycaster.maxRayIntersections = 0;
        }
    }
    
    [Serializable]
    public class TextMeshProComponent : IComponent
    {
        public static Dictionary<string, TMP_FontAsset> FontAssetsMap = new Dictionary<string, TMP_FontAsset>()
        {
            { "Anton SDF", LoadFontAsset("Anton SDF") },
            { "Bangers SDF", LoadFontAsset("Bangers SDF") },
            { "Oswald Bold SDF", LoadFontAsset("Oswald Bold SDF") },
            { "Roboto-Bold SDF", LoadFontAsset("Roboto-Bold SDF") },
            { "LiberationSans SDF", LoadFontAsset("LiberationSans SDF") },
        };
        
        private static TMP_FontAsset LoadFontAsset(string fontAssetName)
        {
            Debug.Log($"Loading fontAssetName:{fontAssetName}");
            var fontPath = $"Assets/Bettr/Editor/fonts/{fontAssetName}.asset";
            var fontAsset =AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath);;
            Debug.Log($"Loaded font asset:{fontAsset.name}");
            return fontAsset;
        }
            
        public string HorizontalAlignment { get; set; }
        
        public string VerticalAlignment { get; set; }
        
        public string Text { get; set; }
        public float FontSize { get; set; }
        public Color FontColor { get; set; }
        
        public Rect? Rect { get; set; }
        
        public TMP_FontAsset FontAsset { get; set; }
        
        public TextMeshProComponent(string text, float fontSize, string colorHex, string horizontalAlignment = "Left", string verticalAlignment = "Middle", Rect? rect = null, string fontAssetName = "Roboto-Bold SDF")
        {
            Text = text;
            FontSize = fontSize;
            Rect = rect;
            HorizontalAlignment = horizontalAlignment;
            VerticalAlignment = verticalAlignment;
            
            // ReSharper disable once InlineOutVariableDeclaration
            TMP_FontAsset tmpFontAsset;
            FontAsset = FontAssetsMap.TryGetValue(fontAssetName, out tmpFontAsset) ? tmpFontAsset : FontAssetsMap["Roboto-Bold SDF"];
            Debug.Log($"TextMeshProComponent Retrieved FontAsset:{FontAsset.name}");
            
            FontColor = Color.white;
            if (ColorUtility.TryParseHtmlString(colorHex, out var tempColor))
            {
                FontColor = tempColor;
            }
            else
            {
                Debug.LogWarning($"Failed to parse color hex: {colorHex}. Using default color.");
            }
        }

        public void AddComponent(GameObject gameObject)
        {
            AddTextMeshPro(gameObject);
        }

        private void AddTextMeshPro(GameObject gameObject)
        {
            var textMeshPro = gameObject.AddComponent<TextMeshPro>();
            textMeshPro.text = Text;
            textMeshPro.fontMaterial = FontAsset.material;
            textMeshPro.fontSize = FontSize;
            textMeshPro.fontSizeMin = FontSize;
            textMeshPro.fontSizeMax = FontSize;
            textMeshPro.enableAutoSizing = true;
            textMeshPro.color = FontColor;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.enableWordWrapping = false;
            textMeshPro.horizontalAlignment = (HorizontalAlignmentOptions) Enum.Parse(typeof(HorizontalAlignmentOptions), HorizontalAlignment);
            textMeshPro.verticalAlignment = (VerticalAlignmentOptions) Enum.Parse(typeof(VerticalAlignmentOptions), VerticalAlignment);
            textMeshPro.font = FontAsset;
            
            if (Rect is not null)
            {
                var rect = (Rect) Rect;
                textMeshPro.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                textMeshPro.rectTransform.pivot = new Vector2(rect.x, rect.y);
            }
        }
    }
    
    [Serializable]
    public class TextMeshProUIComponent : TextMeshProComponent
    {
        public TextMeshProUIComponent(string text, float fontSize, string colorHex, string horizontalAlignment = "Left", string verticalAlignment = "Middle", Rect? rect = null, string fontAssetName = "Roboto-Bold SDF") : base(text, fontSize, colorHex, horizontalAlignment, verticalAlignment, rect, fontAssetName)
        {
        }
        
        public new void AddComponent(GameObject gameObject)
        {
            AddTextMeshProUI(gameObject);
        }

        private void AddTextMeshProUI(GameObject gameObject)
        {
            var textMeshPro = gameObject.AddComponent<TextMeshProUGUI>();
            textMeshPro.text = Text;
            textMeshPro.fontSize = FontSize;
            textMeshPro.enableAutoSizing = false; // Ensure fixed font size
            textMeshPro.color = FontColor;
            textMeshPro.alignment = TextAlignmentOptions.Center;
            textMeshPro.horizontalAlignment = (HorizontalAlignmentOptions) Enum.Parse(typeof(HorizontalAlignmentOptions), HorizontalAlignment);
            textMeshPro.verticalAlignment = (VerticalAlignmentOptions) Enum.Parse(typeof(VerticalAlignmentOptions), VerticalAlignment);
            textMeshPro.enableWordWrapping = false;
            textMeshPro.font = FontAsset;
            
            if (Rect is not null)
            {
                var rect = (Rect) Rect;
                textMeshPro.rectTransform.sizeDelta = new Vector2(rect.width, rect.height);
                textMeshPro.rectTransform.pivot = new Vector2(rect.x, rect.y);
            }
        }
    }
    
    [Serializable]
    public class ImageComponent : IComponent
    {
        public Color Color { get; set; }
        public Rect? Rect { get; set; }
        
        public string TextureName { get; set; }
        
        public string RuntimeAssetPath { get; set; }
        
        // Constructor that takes a path to a Texture2D
        public ImageComponent(string runtimeAssetPath, string textureName, string colorHex, Rect? rect = null)
        {
            RuntimeAssetPath = runtimeAssetPath;
            TextureName = textureName;
            Color = Color.white;
            if (!string.IsNullOrEmpty(colorHex))
            {
                if (ColorUtility.TryParseHtmlString(colorHex, out var tempColor))
                {
                    Color = tempColor;
                }
                else
                {
                    Debug.LogWarning($"Failed to parse color hex: {colorHex}. Using default color.");
                }
            }
            Rect = rect;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Add the Image component
            var image = gameObject.AddComponent<Image>();
            image.raycastTarget = true;
            image.maskable = true;

            if (!string.IsNullOrEmpty(TextureName))
            {
                string sourcePath = Path.Combine("Assets", "Bettr", "Editor", "textures", TextureName);
                var destPath = $"{RuntimeAssetPath}/Textures/{TextureName}";
                string extension = Path.GetExtension(sourcePath);
                if (string.IsNullOrEmpty(extension))
                {
                    extension = File.Exists($"{sourcePath}.jpg") ? ".jpg" : ".png";
                    sourcePath += extension;
                    destPath += extension;
                }
                BettrMaterialGenerator.ImportTexture2D(sourcePath, destPath);
                var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(destPath);
                image.sprite = sprite;
                image.type = Image.Type.Simple;
            }
            else
            {
                image.color = Color;
            }
            
            // Configure the RectTransform
            if (Rect is not null)
            {
                RectTransform rectTransform = gameObject.GetComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(Rect.Value.x, Rect.Value.y);
                rectTransform.sizeDelta = new Vector2(Rect.Value.width, Rect.Value.height);
            }
        }
    }
    
    [Serializable]
    public class RectTransformComponent : IComponent
    {
        public Rect? Rect { get; set; }

        // Constructor that takes a path to a Texture2D
        public RectTransformComponent(string runtimeAssetPath, string textureName, string colorHex, Rect? rect = null)
        {
            Rect = rect;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Configure the RectTransform
            if (Rect is not null)
            {
                RectTransform rectTransform = gameObject.AddComponent<RectTransform>();
                rectTransform.anchoredPosition = new Vector2(Rect.Value.x, Rect.Value.y);
                rectTransform.sizeDelta = new Vector2(Rect.Value.width, Rect.Value.height);
            }
        }
    }
    
    [Serializable]
    public class CanvasComponent : IComponent
    {
        public Camera RenderCamera { get; set; }
        
        // Canvas settings
        public static float DefaultMatchWidthOrHeight = 0f;
        public static CanvasScaler.ScreenMatchMode DefaultScreenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
        public static CanvasScaler.ScaleMode DefaultScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        public static RenderMode DefaultRenderMode = RenderMode.ScreenSpaceCamera;
        public static Vector2 DefaultReferenceResolution = new Vector2(960, 600);
        public static int DefaultSortOrder = 0;
        public static int DefaultPlaneDistance = 100;
        public static bool DefaultPixelPerfect = false;
        public static string DefaultSortingLayerName = "Default";

        // Scaler settings
        public static int DefaultReferencePixelsPerUnit = 100;
        
        // Raycaster settings
        public static bool DefaultIgnoreReversedGraphics = true;

        // Constructor
        public CanvasComponent(
            Camera renderCamera = null)
        {
            RenderCamera = renderCamera;
        }

        public void AddComponent(GameObject gameObject)
        {
            // Ensure the GameObject has a RectTransform component
            if (gameObject.GetComponent<RectTransform>() == null)
            {
                gameObject.AddComponent<RectTransform>();
            }

            // Add the Canvas component
            Canvas canvas = gameObject.AddComponent<Canvas>();
            if (RenderCamera != null)
            {
                canvas.pixelPerfect = DefaultPixelPerfect;
                canvas.worldCamera = RenderCamera;
                canvas.planeDistance = DefaultPlaneDistance;
                canvas.renderMode = DefaultRenderMode;
                canvas.sortingLayerName = DefaultSortingLayerName;
                canvas.sortingOrder = DefaultSortOrder;
            }
            else
            {
                // ScreenSpace Overlay settings
                canvas.pixelPerfect = DefaultPixelPerfect;
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            }

            // Add the Canvas Scaler component
            CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = DefaultScaleMode;
            scaler.referenceResolution = DefaultReferenceResolution;
            scaler.screenMatchMode = DefaultScreenMatchMode;
            scaler.matchWidthOrHeight = DefaultMatchWidthOrHeight;
            scaler.referencePixelsPerUnit = DefaultReferencePixelsPerUnit;

            // Add the Graphic Raycaster component
            GraphicRaycaster raycaster = gameObject.AddComponent<GraphicRaycaster>();
            raycaster.ignoreReversedGraphics = DefaultIgnoreReversedGraphics;
        }
    }
    
    public class EventSystemComponent : IComponent
    {
        public void AddComponent(GameObject gameObject)
        {
            var eventSystem = gameObject.AddComponent<EventSystem>();
            eventSystem.sendNavigationEvents = true;
            eventSystem.pixelDragThreshold = 10;
            var standaloneInputModule = gameObject.AddComponent<StandaloneInputModule>();
            standaloneInputModule.horizontalAxis = "Horizontal";
            standaloneInputModule.verticalAxis = "Vertical";
            standaloneInputModule.submitButton = "Submit";
            standaloneInputModule.cancelButton = "Cancel";
            standaloneInputModule.inputActionsPerSecond = 10;
            standaloneInputModule.repeatDelay = 0.5f;
        }
    }
    
    public class EventTriggerComponent : IComponent
    {
        private readonly Tile _tile;
        private readonly BettrEventListener _eventListener;
        private readonly int _paramCount;
        private readonly string _param;
        private readonly string _referenceId;
        
        public EventTriggerComponent(Tile tile, params string[] param)
        {
            _tile = tile;
            param ??= Array.Empty<string>();
            _param = param.Length > 0 ? param[0] : null;
            _paramCount = (int)param?.Length;
            if (_paramCount > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(param), "EventTriggerComponent only supports 0 or 1 parameters");
            }
        }
        
        public EventTriggerComponent(BettrEventListener eventListener, string referenceId, params string[] param)
        {
            _eventListener = eventListener;
            param ??= Array.Empty<string>();
            _param = param.Length > 0 ? $"{referenceId}__{param[0]}" : referenceId;
            _paramCount = (int)param?.Length;
            if (_paramCount > 1)
            {
                throw new ArgumentOutOfRangeException(nameof(param), "EventTriggerComponent only supports 0 or 1 parameters");
            }
        }
        
        public void AddComponent(GameObject gameObject)
        {
            if (_paramCount == 0)
            {
                AddPointerClick(gameObject);
            }
            else
            {
                AddPointerClick1Param(gameObject);
            }
        }

        private void AddPointerClick(GameObject gameObject)
        {
            EventTrigger eventTrigger =gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            var triggerEvent = entry.callback;
            if (_tile != null)
            {
                UnityEventTools.AddVoidPersistentListener(triggerEvent, _tile.OnPointerClick);
            } 
            else if (_eventListener != null)
            {
                UnityEventTools.AddVoidPersistentListener(triggerEvent, _eventListener.OnPointerClick);
            }
            else
            {
                throw new ArgumentNullException(nameof(_tile), "Tile or Type must be provided");
            }
            eventTrigger.triggers.Add(entry);
        }
        
        private void AddPointerClick1Param(GameObject gameObject)
        {
            EventTrigger eventTrigger =gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            var triggerEvent = entry.callback;
            if (_tile != null)
            {
                UnityEventTools.AddStringPersistentListener(triggerEvent, _tile.OnPointerClick, _param);
            } 
            else if (_eventListener != null)
            {
                UnityEventTools.AddStringPersistentListener(triggerEvent, _eventListener.OnPointerClick, _param);
            } 
            else
            {
                throw new ArgumentNullException(nameof(_tile), "Tile or Type must be provided");
            }
            eventTrigger.triggers.Add(entry);
        }
    }
    
    [Serializable]
    public class MechanicPrefab : InstanceGameObject
    {
        // ReSharper disable once InconsistentNaming
        public string ParentId { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string ThisId { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string Action { get; set; }
    }

    [Serializable]
    public class MechanicAnimation
    {
        // ReSharper disable once InconsistentNaming
        public string Filename { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<AnimationState> AnimationStates { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<AnimatorTransition> AnimatorTransitions { get; set; }
    }

    [Serializable]
    public class MechanicTilePropertyAnimators
    {
        // ReSharper disable once InconsistentNaming
        public List<PrefabId> PrefabIds { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string PrefabName { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<AnimatorProperty> AnimatorsProperty { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<AnimatorGroupProperty> AnimatorsGroupProperty { get; set; }
    }
    
    [Serializable]
    public class MechanicTilePropertyTextMeshPros
    {
        // ReSharper disable once InconsistentNaming
        public List<PrefabId> PrefabIds { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string PrefabName { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string ThisId { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<TextMeshProProperty> TextMeshProsProperty { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<TextMeshProGroupProperty> TextMeshProsGroupProperty { get; set; }
    }
    
    [Serializable]
    public class MechanicParticleSystem
    {
        // ReSharper disable once InconsistentNaming
        public string PrefabName { get; set; }
        // ReSharper disable once InconsistentNaming
        public List<PrefabId> PrefabIds { get; set; }
        // ReSharper disable once InconsistentNaming
        public string ReferenceId { get; set; }
        // ReSharper disable once InconsistentNaming
        public ParticleSystemModuleData ModuleData { get; set; }
    }
    
    [Serializable]
    public class MechanicTilePropertyParticleSystems
    {
        // ReSharper disable once InconsistentNaming
        public List<PrefabId> PrefabIds { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public string PrefabName { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<ParticleSystemProperty> ParticleSystemsProperty { get; set; }
    }
    
    [Serializable]
    public class MechanicAnimatorController : InstanceComponent
    {
        public string Action { get; set; }
    }

    [Serializable]
    public class Mechanic
    {
        // ReSharper disable once InconsistentNaming
        public List<MechanicPrefab> Prefabs { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicAnimation> Animations { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicTilePropertyAnimators> TilePropertyAnimators { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicAnimatorController> AnimatorControllers { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicParticleSystem> ParticleSystems { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicTilePropertyParticleSystems> TilePropertyParticleSystems { get; set; }

        // ReSharper disable once InconsistentNaming
        public List<MechanicTilePropertyTextMeshPros> TilePropertyTextMeshPros { get; set; }
        
        public void Process()
        {
            ProcessModifiedPrefabs();
            ProcessTextMeshPros();
        }

        private void ProcessModifiedPrefabs()
        {
            // Modified Prefabs
            if (this.Prefabs != null)
            {
                foreach (var instanceGameObject in this.Prefabs)
                {
                    var prefabPath =
                        $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{instanceGameObject.PrefabName}.prefab";
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var prefabGameObject = new PrefabGameObject(prefab, instanceGameObject.PrefabName, false);
                    if (instanceGameObject.PrefabIds != null)
                    {
                        foreach (var prefabId in instanceGameObject.PrefabIds)
                        {
                            var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                            InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                        }
                    }

                    if (instanceGameObject.Action == "add")
                    {
                        var parentGameObject = InstanceGameObject.IdGameObjects[instanceGameObject.ParentId];
                        instanceGameObject.SetParent(parentGameObject.GameObject);
                    
                        instanceGameObject.OnDeserialized(new StreamingContext());
                    
                        PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
                    }
                    else if (instanceGameObject.Action == "modify")
                    {
                        var gameObjectInPrefab = InstanceGameObject.IdGameObjects[instanceGameObject.ThisId];
                        
                        instanceGameObject.OnDeserialized(new StreamingContext());
                        
                        foreach (var child in instanceGameObject.Children)
                        {
                            child.SetParent(gameObjectInPrefab.GameObject);

                            if (child.PrefabIds != null)
                            {
                                foreach (var prefabId in child.PrefabIds)
                                {
                                    var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                                    InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                                }
                            }
                        }

                        foreach (var component in instanceGameObject.Components)
                        {
                            component.AddComponent(gameObjectInPrefab.GameObject);
                        }
                        
                        PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
                    }
                }
            }
        }

        private void ProcessTextMeshPros()
        {
            if (TilePropertyTextMeshPros != null)
            {
                foreach (var tilePropertyTextMeshPro in this.TilePropertyTextMeshPros)
                {
                    var prefabPath =
                        $"{InstanceComponent.RuntimeAssetPath}/Prefabs/{tilePropertyTextMeshPro.PrefabName}.prefab";
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
                    var prefabGameObject = new PrefabGameObject(prefab, tilePropertyTextMeshPro.PrefabName, false);
                    if (tilePropertyTextMeshPro.PrefabIds != null)
                    {
                        foreach (var prefabId in tilePropertyTextMeshPro.PrefabIds)
                        {
                            var referencedGameObject = prefabGameObject.FindReferencedId(prefabId.Id, prefabId.Index);
                            InstanceGameObject.IdGameObjects[$"{prefabId.Prefix}{prefabId.Id}"] = new InstanceGameObject(referencedGameObject);
                        }
                    }
                    InstanceGameObject.IdGameObjects.TryGetValue(tilePropertyTextMeshPro.ThisId, out var thisGameObject);
                    var component = thisGameObject.GameObject.GetComponent<TilePropertyTextMeshPros>();
                    var properties = new List<TilePropertyTextMeshPro>();
                    if (tilePropertyTextMeshPro.TextMeshProsProperty != null)
                    {
                        foreach (var textMeshProProperty in tilePropertyTextMeshPro.TextMeshProsProperty)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(textMeshProProperty.Id, out var referenceGameObject);
                            var tileProperty = new TilePropertyTextMeshPro()
                            {
                                key = textMeshProProperty.Key,
                                value = new PropertyTextMeshPro() {textMeshPro = referenceGameObject?.TextMeshPro, isFormatText = textMeshProProperty.IsFormatText}
                            };
                            if (tileProperty.value.textMeshPro == null)
                            {
                                Debug.LogError($"Failed to find textMeshPro with id: {textMeshProProperty.Id}");
                            }
                            properties.Add(tileProperty);                        
                        }
                    }
                    component.tileTextMeshProProperties.AddRange(properties);
                    var groupProperties = new List<TilePropertyTextMeshProGroup>();
                    if (tilePropertyTextMeshPro.TextMeshProsGroupProperty != null)
                    {
                        foreach (var textMeshProGroupProperty in tilePropertyTextMeshPro.TextMeshProsGroupProperty)
                        {
                            List<TilePropertyTextMeshPro> textMeshProsProperties = new List<TilePropertyTextMeshPro>();
                            foreach (var property in textMeshProGroupProperty.Group)
                            {
                                InstanceGameObject.IdGameObjects.TryGetValue(property.Id, out var referenceGameObject);
                                var textMeshPro = referenceGameObject?.GameObject.GetComponent<TMP_Text>();
                                var gameObjectProperty = new TilePropertyTextMeshPro()
                                {
                                    key = property.Key,
                                    value = new PropertyTextMeshPro() { textMeshPro = textMeshPro, isFormatText = property.IsFormatText },
                                };
                                textMeshProsProperties.Add(gameObjectProperty);
                            }
                            var group = component.tileTextMeshProGroupProperties.Find(x =>
                                x.groupKey == textMeshProGroupProperty.GroupKey);
                            if (group != null)
                            {
                                group.textMeshProProperties.AddRange(textMeshProsProperties);
                            }
                            else
                            {
                                groupProperties.Add(new TilePropertyTextMeshProGroup()
                                {
                                    groupKey = textMeshProGroupProperty.GroupKey,
                                    textMeshProProperties = textMeshProsProperties,
                                });
                            }
                        }
                    }
                    component.tileTextMeshProGroupProperties.AddRange(groupProperties);
                    PrefabUtility.SaveAsPrefabAsset(prefabGameObject.GameObject, prefabPath);
                }
            }
        }
    }
}