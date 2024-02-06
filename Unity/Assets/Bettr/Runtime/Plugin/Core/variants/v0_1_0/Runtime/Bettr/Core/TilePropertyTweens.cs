using System;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class PropertyTween
    {
        [Header("Move Between Properties")]
        [Tooltip("(Optional) Set this flag to true to move the game object.")]
        public bool useMoveBetween;
        [Tooltip("(Optional) This game object's world position will be used to start the tween.")]
        public GameObject startGameObject;
        [Tooltip("(Optional) Set this flag to true to use the startGameObject world position.")]
        public bool useStartGameObject;
        [Tooltip("(Optional) The world position will be used to end the tween.")]
        public GameObject endGameObject;
        [Tooltip("(Optional) Set this flag to true to use the endGameObject world position.")]
        public bool useEndGameObject;
        [Header("Scale To Properties")]
        [Tooltip("(Optional) Set this to the scale target for the game object.")]
        public Vector3 scaleTo;
        [Tooltip("(Optional) Set this flag to true to scale the game object.")]
        public bool useScale;
        [Header("Fade To Properties")]
        [Tooltip("(Optional) Set this flag to true to fade out/fade in the game object.")]
        public bool useFade;
        [Tooltip("(Optional) Set this to the fade to target for the game object.")]
        public float fadeTo;
        [Header("Tween Properties")]
        [Tooltip("Tween delay.")]
        public float tweenDelay = 0.0f;
        [Tooltip("Tween duration.")]
        public float tweenDuration = 3.0f;
        [Tooltip("Tween ease in out sine.")]
        public iTween.EaseType easeType = iTween.EaseType.easeInOutSine;
    }

    [Serializable]
    public class TilePropertyTweenGroup
    {
        [Tooltip("Key to use for locating path creator group property. Should be unique per Tile.")]
        public string groupKey;
        [Tooltip("Group of tile path creator properties to run in parallel")]
        public List<TilePropertyTween> tileTweenProperties;

        public PropertyTween this[int luaIndex] => tileTweenProperties[luaIndex - 1]?.value;

        public PropertyTween this[string key] => tileTweenProperties.Find(x => x.key == key)?.value;

        public int Count => tileTweenProperties.Count;
    }

    [Serializable]
    public class TilePropertyTween
    {
        [Tooltip("Key to use for locating path creator property. Should be unique per Tile")]
        public string key;
        public PropertyTween value;
    }

    [RequireComponent(typeof(Tile))]
    [Serializable]
    public class TilePropertyTweens : MonoBehaviour, ITileProperty
    {
        public List<TilePropertyTween> tileTweenProperties;

        public List<TilePropertyTweenGroup> tileTweenGroupProperties;

        public void Enable(Table luaTable)
        {
            if (luaTable == null)
            {
                throw new TilePrefabConfigurationException("TilePropertyTweens requires Tile to have a LuaTable. Ensure TilePropertyTweens component is below the Tile component in the prefab.");
            }

            AddTileTweenProperties(tileTweenProperties, luaTable);

            AddTileTweenGroupProperties(tileTweenGroupProperties, luaTable);
        }

        private void AddTileTweenProperties(List<TilePropertyTween> tileTweenPropertyList, Table tileTable)
        {
            foreach (var tileTweenProperty in tileTweenPropertyList)
            {
                tileTable[tileTweenProperty.key] = tileTweenProperty.value;
            }
        }

        private void AddTileTweenGroupProperties(List<TilePropertyTweenGroup> tileTweenPropertyGroupList, Table tileTable)
        {
            foreach (var tileTweenPropertyGroup in tileTweenPropertyGroupList)
            {
                tileTable[tileTweenPropertyGroup.groupKey] = tileTweenPropertyGroup;
            }
        }
    }
}
