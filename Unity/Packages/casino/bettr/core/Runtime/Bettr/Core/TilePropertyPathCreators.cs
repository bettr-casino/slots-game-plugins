using System;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using PathCreation;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    [Serializable]
    public class PropertyPathCreator
    {
        public PathCreator pathCreator;
    }

    [Serializable]
    public class TilePropertyPathCreatorGroup
    {
        [Tooltip("Key to use for locating path creator group property. Should be unique per Tile.")]
        public string groupKey;
        [Tooltip("Group of tile path creator properties to run in parallel")]
        public List<TilePropertyPathCreator> tilePathCreatorProperties;

        public PropertyPathCreator this[int luaIndex] => tilePathCreatorProperties[luaIndex - 1]?.value;

        public PropertyPathCreator this[string key] => tilePathCreatorProperties.Find(x => x.key == key)?.value;

        public int Count => tilePathCreatorProperties.Count;
    }

    [Serializable]
    public class TilePropertyPathCreator
    {
        [Tooltip("Key to use for locating path creator property. Should be unique per Tile")]
        public string key;
        public PropertyPathCreator value;
    }

    [RequireComponent(typeof(Tile))]
    [Serializable]
    public class TilePropertyPathCreators : MonoBehaviour, ITileProperty
    {
        public List<TilePropertyPathCreator> tilePathCreatorProperties;

        public List<TilePropertyPathCreatorGroup> tilePathCreatorGroupProperties;

        public void Enable(Table luaTable)
        {
            if (luaTable == null)
            {
                throw new TilePrefabConfigurationException("TilePropertyPathCreators requires Tile to have a LuaTable. Ensure TilePropertyPathCreators component is below the Tile component in the prefab.");
            }

            AddTilePathCreatorProperties(tilePathCreatorProperties, luaTable);

            AddTilePathCreatorGroupProperties(tilePathCreatorGroupProperties, luaTable);
        }

        private void AddTilePathCreatorProperties(List<TilePropertyPathCreator> tilePathCreatorPropertyList, Table tileTable)
        {
            foreach (var tilePathCreatorProperty in tilePathCreatorPropertyList)
            {
                tileTable[tilePathCreatorProperty.key] = tilePathCreatorProperty.value;
            }
        }

        private void AddTilePathCreatorGroupProperties(List<TilePropertyPathCreatorGroup> tilePathCreatorPropertyGroupList, Table tileTable)
        {
            foreach (var tilePathCreatorPropertyGroup in tilePathCreatorPropertyGroupList)
            {
                tileTable[tilePathCreatorPropertyGroup.groupKey] = tilePathCreatorPropertyGroup;
            }
        }
    }
}
