using System;
using System.Collections.Generic;
using CrayonScript.Code;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

namespace Bettr.Editor
{
    [Serializable]
    public class GameObjectDefinition
    {
        public string Name { get; set; }
        public GameObjectDefinition Child { get; set; }
        public List<GameObjectDefinition> Children { get; set; }
    }
    
    public interface IGameObject
    {
        public GameObject GameObject { get; }
        public void SetParent(GameObject parentGo);
        public void SetParent(IGameObject parentGo);
    }

    public class InstanceGameObject : IGameObject
    {
        private GameObject _go;
        public string Name;
        
        public GameObject GameObject => _go;
        
        public InstanceGameObject(GameObject go)
        {
            _go = go;
            Name = go.name;
        }

        public InstanceGameObject(string name)
        {
            _go = new GameObject(name);
            Name = name;
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

    public class PrefabGameObject : IGameObject
    {
        private readonly GameObject _prefab;
        private string _name;
        
        public GameObject GameObject => _prefab;
        
        public PrefabGameObject(GameObject prefab, string name)
        {
            _prefab = prefab;
            _name = name;
        }
        
        public void SetParent(GameObject parentGo)
        {
            // Instantiate the child prefab and set it as a child of the new prefab
            var childInstance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, parentGo.transform);
            childInstance.name = _name;
        }
        
        public void SetParent(IGameObject parentGo)
        {
            SetParent(parentGo.GameObject);
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
    public class AnimatorComponent : IComponent
    {
        private readonly AnimatorController _animatorController;

        public AnimatorComponent(AnimatorController animatorController)
        {
            _animatorController = animatorController;
        }

        public void AddComponent(GameObject gameObject)
        {
            var animator = gameObject.AddComponent<Animator>();
            animator.runtimeAnimatorController = _animatorController;
        }
    }

    [Serializable]
    public class UICameraComponent : IComponent
    {
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
        }
    }
    
}