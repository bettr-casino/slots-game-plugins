using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using Bettr.Editor.generators;
using CrayonScript.Code;
using TMPro;
using UnityEditor;
using UnityEditor.Animations;
using UnityEditor.Events;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

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
        
        private GameObject _go;
        public string Name { get; set; }
        
        public string Id { get; set; }
        
        public List<PrefabId> PrefabIds { get; set; }
        
        public string PrefabName { get; set; }
        
        public bool IsPrefab { get; set; }
        
        public string PrimitiveMaterial { get; set; }
        
        public string PrimitiveShader { get; set; }
        
        public string PrimitiveTexture { get; set; }
        
        public string PrimitiveColor { get; set; }
        
        public int Primitive { get; set; }
        
        public bool IsPrimitive { get; set; }
        
        public bool Active { get; set; }
        
        public string Layer { get; set; }
        
        public Vector3? Position { get; set; }
        
        public Vector3? Rotation { get; set; }
        
        public Vector3? Scale { get; set; }
        
        public List<InstanceComponent> Components { get; set; }

        public InstanceGameObject Child { get; set; }
        
        public List<InstanceGameObject> Children { get; set; }

        public GameObject GameObject => _go;

        public Animator Animator => _go.GetComponent<Animator>();
        
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
                if (IsPrefab)
                {
                    Debug.Log($"loading prefab from path: {InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab");
                    var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{InstanceComponent.RuntimeAssetPath}/Prefabs/{PrefabName}.prefab");
                    var prefabGameObject = new PrefabGameObject(prefab, Name);
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
                    var primitiveMaterial = BettrMaterialGenerator.CreateOrLoadMaterial(PrimitiveMaterial, PrimitiveShader, PrimitiveTexture, PrimitiveColor, InstanceComponent.RuntimeAssetPath);
                    
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
        
        public PrefabGameObject(GameObject prefab, string name)
        {
            _prefab = prefab;
            _name = name;
            _go = (GameObject)PrefabUtility.InstantiatePrefab(_prefab);
            _go.name = _name;
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
        public static string RuntimeAssetPath;
        
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
        
        public List<EventTriggerData> EventTriggers { get; set; }
        
        public List<AnimationState> AnimationStates { get; set; }
        
        public List<AnimatorTransition> AnimatorTransitions { get; set; }
        
        public List<GameObjectProperty> GameObjectsProperty { get; set; }
        
        public List<GameObjectGroupProperty> GameObjectGroupsProperty { get; set; }
        
        public List<AnimatorProperty> AnimatorsProperty { get; set; }
        
        public List<AnimatorGroupProperty> AnimatorsGroupProperty { get; set; }
        
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
                        var renderCamera = referenceGameObject?.GameObject.GetComponent<Camera>();
                        var canvasComponent = new CanvasComponent(renderCamera);
                        canvasComponent.AddComponent(gameObject);
                    }
                    break;
                case "EventSystem":
                    var eventSystemComponent = new EventSystemComponent();
                    eventSystemComponent.AddComponent(gameObject);
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
                        var eventTriggerComponent = new EventTriggerComponent(tile, Params);
                        eventTriggerComponent.AddComponent(referenceGameObject?.GameObject);
                        
                    }
                }
                    break;
                case "TileWithUpdate":
                {
                    var globalTileId = string.IsNullOrEmpty(Name) ? Filename : Name;
                    var scriptAsset = BettrScriptGenerator.CreateOrLoadScript(Filename, RuntimeAssetPath);
                    var tileComponent = new TileWithUpdateComponent(globalTileId, scriptAsset);
                    tileComponent.AddComponent(gameObject);
                    
                    // add in the event triggers
                    var tile = gameObject.GetComponent<TileWithUpdate>();
                    foreach (var eventTrigger in EventTriggers)
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(eventTrigger.ReferenceId, out var referenceGameObject);
                        var eventTriggerComponent = new EventTriggerComponent(tile, Params);
                        eventTriggerComponent.AddComponent(referenceGameObject?.GameObject);
                    }
                }
                    break;
                case "TilePropertyTextMeshPros":
                    var tileTextMeshProProperties = new List<TilePropertyTextMeshPro>();
                    var tileTextMeshProGroupProperties = new List<TilePropertyTextMeshProGroup>();
                    var tilePropertyTextMeshProsComponent = new TilePropertyTextMeshProsComponent(tileTextMeshProProperties, tileTextMeshProGroupProperties);
                    tilePropertyTextMeshProsComponent.AddComponent(gameObject);
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
                    var tileGameObjectProperties = new List<TilePropertyGameObject>();
                    var tileGameObjectGroupProperties = new List<TilePropertyGameObjectGroup>();
                    var tilePropertyGameObjectsComponent = new TilePropertyGameObjectsComponent(tileGameObjectProperties, tileGameObjectGroupProperties);
                    tilePropertyGameObjectsComponent.AddComponent(gameObject);
                    if (GameObjectsProperty != null)
                    {
                        foreach (var kvPair in GameObjectsProperty)
                        {
                            InstanceGameObject.IdGameObjects.TryGetValue(kvPair.Id, out var referenceGameObject);
                            var tilePropertyGameObject = new TilePropertyGameObject()
                            {
                                key = kvPair.Key,
                                value = new PropertyGameObject() {gameObject = referenceGameObject?.GameObject },
                            };
                            tileGameObjectProperties.Add(tilePropertyGameObject);
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
                    var properties = new List<TilePropertyAnimator>();
                    var groupProperties = new List<TilePropertyAnimatorGroup>();
                    var tilePropertyAnimatorsComponent = new TilePropertyAnimatorsComponent(properties, groupProperties);
                    tilePropertyAnimatorsComponent.AddComponent(gameObject);
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
                    var tileIntProperties = new List<TilePropertyInt>();
                    var tileIntGroupProperties = new List<TilePropertyIntGroup>();
                    var tilePropertyIntsComponent = new TilePropertyIntsComponent(tileIntProperties, tileIntGroupProperties);
                    tilePropertyIntsComponent.AddComponent(gameObject);
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
                    var tileStringProperties = new List<TilePropertyString>();
                    var tileStringGroupProperties = new List<TilePropertyStringGroup>();
                    var tilePropertyStringsComponent = new TilePropertyStringsComponent(tileStringProperties, tileStringGroupProperties);
                    tilePropertyStringsComponent.AddComponent(gameObject);
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
            AssetDatabase.Refresh();
            
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
            uiCamera.clearFlags = CameraClearFlags.Depth;
            uiCamera.cullingMask = LayerMask.GetMask("SLOT_BACKGROUND");
            uiCamera.orthographic = true;
            uiCamera.orthographicSize = 4.1f;
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
                AssetDatabase.Refresh();
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
        public static Vector2 DefaultReferenceResolution = new Vector2(800, 600);
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
            canvas.pixelPerfect = DefaultPixelPerfect;
            canvas.worldCamera = RenderCamera;
            canvas.planeDistance = DefaultPlaneDistance;
            canvas.renderMode = DefaultRenderMode;
            canvas.sortingLayerName = DefaultSortingLayerName;
            canvas.sortingOrder = DefaultSortOrder;

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
        private readonly int _paramCount;
        private readonly string _param;
        
        public EventTriggerComponent(Tile tile, params string[] param)
        {
            _tile = tile;
            _param = param.Length > 0 ? param[0] : null;
            _paramCount = param.Length;
        }
        
        public void AddComponent(GameObject gameObject)
        {
            EventTrigger eventTrigger =gameObject.AddComponent<EventTrigger>();
            EventTrigger.Entry entry = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            var triggerEvent = entry.callback;
            UnityEventTools.AddVoidPersistentListener(triggerEvent, _tile.OnPointerClick);
            eventTrigger.triggers.Add(entry);
        }
    }
    
    [Serializable]
    public class MechanicInstanceGameObject : InstanceGameObject
    {
        // ReSharper disable once InconsistentNaming
        public string ParentId { get; set; }
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
    }
    
    [Serializable]
    public class MechanicParticleSystem
    {
        // ReSharper disable once InconsistentNaming
        public string Filename { get; set; }
        // ReSharper disable once InconsistentNaming
        public int Id { get; set; }
        // ReSharper disable once InconsistentNaming
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
    public class Mechanic
    {
        // ReSharper disable once InconsistentNaming
        public List<MechanicInstanceGameObject> GameObjects { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicAnimation> Animations { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicTilePropertyAnimators> TilePropertyAnimators { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicParticleSystem> ParticleSystems { get; set; }
        
        // ReSharper disable once InconsistentNaming
        public List<MechanicTilePropertyParticleSystems> TilePropertyParticleSystems { get; set; }
    }
}