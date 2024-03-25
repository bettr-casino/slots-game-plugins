using System;
using System.Collections.Generic;
using CrayonScript.Code;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = System.Object;

namespace Bettr.Editor
{
    public interface IGameObject
    {
        public void AddChild(GameObject parentGo);
    }

    public class InstanceGameObject : IGameObject
    {
        public readonly GameObject Go;
        
        public InstanceGameObject(GameObject go)
        {
            Go = go;
        }
        
        public void AddChild(GameObject parentGo)
        {
            Go.transform.SetParent(parentGo.transform);
        }
    }

    public class PrefabGameObject : IGameObject
    {
        private readonly GameObject _prefab;
        private string _name;
        
        public PrefabGameObject(GameObject prefab, string name)
        {
            _prefab = prefab;
            _name = name;
        }
        
        public void AddChild(GameObject parentGo)
        {
            // Instantiate the child prefab and set it as a child of the new prefab
            var childInstance = (GameObject)PrefabUtility.InstantiatePrefab(_prefab, parentGo.transform);
            childInstance.name = _name;
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
    public class MeshComponent : IComponent
    {
        private readonly Material _material;

        public MeshComponent(Material material)
        {
            _material = material;
        }

        public void AddComponent(GameObject gameObject)
        {
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            meshRenderer.material = _material;
        }
    }
}