using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        
        private string _Id;

        public string Id
        {
            get => _Id;
            set { _Id = value;
                IdGameObjects[_Id] = this;
            }
        }
        
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
        
        private List<InstanceComponent> _components;
        
        public List<InstanceComponent> Components {
            get => _components;
            set
            {
                _components = value;
                EnsureGameObject();
                foreach (var component in _components)
                {
                    component.AddComponent(_go);
                }
            }
        }

        private InstanceGameObject _child;
        
        public InstanceGameObject Child {
            get => _child;
            set
            {
                _child = value;
                EnsureGameObject();
                _child.SetParent(_go);
            }
        }
        
        private List<InstanceGameObject> _children;

        public List<InstanceGameObject> Children
        {
            get => _children;
            set
            {
                _children = value;
                EnsureGameObject();
                foreach (var child in _children)
                {
                    child.SetParent(_go);
                }
            }
        }

        public GameObject GameObject => _go;

        public Animator Animator => _go.GetComponent<Animator>();

        public InstanceGameObject()
        {
            Active = true;
            Layer = "Default";
        }
        
        public InstanceGameObject(GameObject go)
        {
            _go = go;
            Name = go.name;
        }

        public InstanceGameObject(string name)
        {
            Name = name;
            EnsureGameObject();
        }
        
        public void SetParent(GameObject parentGo)
        {
            EnsureGameObject();
            _go.transform.SetParent(parentGo.transform);
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
                            InstanceGameObject.IdGameObjects[prefabId.Id] = new InstanceGameObject(referencedGameObject);
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
    }

    [Serializable]    
    public struct PrefabId
    {
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
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
        }

        public GameObject FindReferencedId(string id, int index)
        {
            var currentIndex = 0;
            return FindByIdDepthFirst(_go.transform, id, ref index, ref currentIndex);
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
        
        public string ComponentType { get; set; }
        
        public string Filename { get; set; }
        
        public string Color { get; set; }
        
        public string Text { get; set; }
        
        public int FontSize { get; set; }
        
        public string FontAsset { get; set; }
        
        public Rect? Rect { get; set; }
        
        public string ReferenceId { get; set; }
        
        public bool IncludeAudioListener { get; set; }
        
        public List<AnimationState> AnimationStates { get; set; }
        
        public List<AnimationTransition> AnimatorTransitions { get; set; }
        
        public List<GameObjectProperty> GameObjectsProperty { get; set; }
        
        public List<GameObjectGroupProperty> GameObjectGroupsProperty { get; set; }
        
        public List<AnimatorProperty> AnimatorsProperty { get; set; }
        
        public List<AnimatorGroupProperty> AnimatorsGroupProperty { get; set; }
        
        public List<TextMeshProProperty> TextMeshProsProperty { get; set; }
        
        public List<TextMeshProGroupProperty> TextMeshProGroupsProperty { get; set; }
        
        public string[] Params { get; set; }
        
        public InstanceComponent()
        {
            FontAsset = "Roboto-Bold SDF";
            IncludeAudioListener = true;
            Params = Array.Empty<string>();
            GameObjectsProperty = new List<GameObjectProperty>();
            GameObjectGroupsProperty = new List<GameObjectGroupProperty>();
            AnimatorsProperty = new List<AnimatorProperty>();
            AnimatorsGroupProperty = new List<AnimatorGroupProperty>();
            AnimationStates = new List<AnimationState>();
            AnimatorTransitions = new List<AnimationTransition>();
            TextMeshProsProperty = new List<TextMeshProProperty>();
            TextMeshProGroupsProperty = new List<TextMeshProGroupProperty>();
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
                    var textMeshProComponent = new TextMeshProComponent(Text, FontSize, Color, Rect, FontAsset);
                    textMeshProComponent.AddComponent(gameObject);
                    break;
                case "TextMeshProUI":
                    var textMeshProUIComponent = new TextMeshProUIComponent(Text, FontSize, Color, Rect, FontAsset);
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
                case "EventTrigger":
                    {
                        InstanceGameObject.IdGameObjects.TryGetValue(ReferenceId, out var referenceGameObject);
                        var tile = referenceGameObject?.GameObject.GetComponent<Tile>();
                        var eventTriggerComponent = new EventTriggerComponent(tile, Params);
                        eventTriggerComponent.AddComponent(gameObject);
                    }
                    break;
                case "Tile":
                    var globalTileId = Filename;
                    var scriptAsset = BettrScriptGenerator.CreateOrLoadScript(Filename, RuntimeAssetPath);
                    var tileComponent = new TileComponent(globalTileId, scriptAsset);
                    tileComponent.AddComponent(gameObject);
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
                            value = new PropertyTextMeshPro() {textMeshPro = textMeshPro },
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
                                value = new PropertyTextMeshPro() { textMeshPro = textMeshPro },
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
                            value = new PropertyAnimator() {animator = referenceGameObject?.Animator, animationStateName = kvPair.State},
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
                                value = new PropertyAnimator() {animator = referenceGameObject?.Animator, animationStateName = property.State},
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
            }
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
    public class AnimatorProperty
    {
        // ReSharper disable once InconsistentNaming
        public string Key;

        // ReSharper disable once InconsistentNaming
        public string Id;
        
        // ReSharper disable once InconsistentNaming
        public string State;
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
        public string Key;

        public string Id;
    }
    
    [Serializable]
    public class TextMeshProGroupProperty
    {
        public string GroupKey;

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
    public class AnimationTransition
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
        
        private List<AnimationTransition> _animationTransitions;
        
        public AnimatorComponent(string fileName, List<AnimationState> animationStates, List<AnimationTransition> animationTransitions, string runtimeAssetPath)
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
            var animatorControllerName = $"{_fileName}_anims.controller";
            var animatorControllerPath = $"{_runtimeAssetPath}/Animators/{animatorControllerName}";
            var animatorController = AnimatorController.CreateAnimatorControllerAtPath(animatorControllerPath);
            var stateMachine = animatorController.layers[0].stateMachine;
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();
            
            foreach (var animationState in _animationStates)
            {
                var animationClip = new AnimationClip
                {
                    name = animationState.Name,
                    wrapMode = animationState.IsLoop ? WrapMode.Loop : WrapMode.Once,
                    frameRate = animationState.Speed
                };

                var dopesheets = animationState.Dopesheet;
                if (dopesheets != null)
                {
                    foreach (var dopesheet in dopesheets)
                    {
                        AnimationCurve curve = new AnimationCurve
                        {
                            keys = dopesheet.Keyframes.Times.Select((t, i) => new Keyframe(t, dopesheet.Keyframes.Values[i])).ToArray()
                        };
                        switch (dopesheet.Property)
                        {
                            case "m_IsActive":
                                AnimationUtility.SetEditorCurve(animationClip, EditorCurveBinding.FloatCurve(dopesheet.Path, typeof(GameObject), "m_IsActive"), curve);
                                break;
                        }
                    }
                }
                
                var animationStateName = animationClip.name;
                var state = stateMachine.AddState(animationStateName);
                state.motion = animationClip;
                if (animationClip.wrapMode == WrapMode.Loop)
                {
                    SerializedObject serializedClip = new SerializedObject(animationClip);
                    var properties = serializedClip.GetIterator();
                    while (properties.NextVisible(true))
                    {
                        if (properties.name == "m_AnimationClipSettings")
                        {
                            properties.FindPropertyRelative("m_LoopTime").boolValue = true;
                            serializedClip.ApplyModifiedProperties();
                        }
                    }
                }
                if (stateMachine.defaultState == null && animationState.IsDefault)
                {
                    stateMachine.defaultState = state;
                }
                var animationClipName = $"{_fileName}_anim_{animationStateName}.anim";
                var animationClipPath = $"{_runtimeAssetPath}/Animators/{animationClipName}";
                AssetDatabase.CreateAsset(animationClip, animationClipPath);
                AssetDatabase.SaveAssets();
                AssetDatabase.Refresh();
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            foreach (var animationStateTransition in _animationTransitions)
            {
                var stateTransition = animationStateTransition;
                var transitionFrom = stateMachine.states.FirstOrDefault(s => s.state.name == stateTransition.TransitionFrom);
                var transitionTo = stateMachine.states.FirstOrDefault(s => s.state.name == animationStateTransition.TransitionTo);
                var transition = transitionFrom.state.AddTransition(transitionTo.state);
                if (animationStateTransition.TransitionDuration > 0)
                {
                    transition.duration = animationStateTransition.TransitionDuration;
                }
            }
            
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            var runtimeAnimatorController = AssetDatabase.LoadAssetAtPath<AnimatorController>(animatorControllerPath);
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
            uiCamera.orthographicSize = 5;
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
            
        
        public string Text { get; set; }
        public float FontSize { get; set; }
        public Color FontColor { get; set; }
        
        public Rect? Rect { get; set; }
        
        public TMP_FontAsset FontAsset { get; set; }
        
        public TextMeshProComponent(string text, float fontSize, string colorHex, Rect? rect = null, string fontAssetName = "Roboto-Bold SDF")
        {
            Text = text;
            FontSize = fontSize;
            Rect = rect;
            
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
            textMeshPro.fontSize = FontSize;
            textMeshPro.fontMaterial = FontAsset.material;
            textMeshPro.enableAutoSizing = true;
            textMeshPro.fontSizeMin = FontSize;
            textMeshPro.fontSizeMax = FontSize;
            textMeshPro.color = FontColor;
            textMeshPro.alignment = TextAlignmentOptions.Center;
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
    public class TextMeshProUIComponent : TextMeshProComponent
    {
        public TextMeshProUIComponent(string text, float fontSize, string colorHex, Rect? rect = null, string fontAssetName = "Roboto-Bold SDF") : base(text, fontSize, colorHex, rect, fontAssetName)
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
}