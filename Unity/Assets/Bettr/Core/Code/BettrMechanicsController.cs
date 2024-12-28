using System.Collections.Generic;
using System.Text;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrMechanicsController
    {
        // Singleton instance
        public static BettrMechanicsController Instance { get; private set; }

        // Constructor
        public BettrMechanicsController()
        {
            // Register the BettrMechanicsController type
            TileController.RegisterType<BettrMechanicsController>("BettrMechanicsController");
            TileController.AddToGlobals("BettrMechanicsController", this);

            // Set the singleton instance
            Instance = this;
        }

        public Table LoadUserMechanicsTable(string mechanicName, object machine, object variant)
        {
            var tableName = $"{machine.ToString().ToLower()}__{variant.ToString().ToLower()}__{mechanicName.ToLower()}";
            var table = (Table) TileController.LuaScript.Globals[tableName];
            return table;
        }

        public List<TilePropertyGameObjectGroup> AddSymbolsToReelSymbolGroups(string mechanicName, BettrReelController reelController, TilePropertyGameObjectGroup symbolPropertiesGroup)
        {
            var newSymbolPropertiesGroups = new List<TilePropertyGameObjectGroup>();
            var symbolCount = (int) (double) reelController.ReelStateTable["SymbolCount"];
            for (var symbolIndex = 1; symbolIndex <= symbolCount; symbolIndex++)
            {
                var symbolGroupKey = $"SymbolGroup{symbolIndex}";
                var symbolGroupProperty = (TilePropertyGameObjectGroup) reelController.ReelTable[symbolGroupKey];
                if (symbolGroupProperty == null)
                {
                    throw new System.Exception($"SymbolGroup{symbolIndex} is null");
                }
                // at least one symbol should exist in symbolGroupProperty
                if (symbolGroupProperty.gameObjectProperties.Count == 0)
                {
                    throw new System.Exception($"SymbolGroup{symbolIndex} gameObjectProperties.Count == 0");
                }
                // get the 1st symbol
                var firstSymbolProperty = symbolGroupProperty.gameObjectProperties[0];
                var firstSymbolGameObject = firstSymbolProperty.value.GameObject;
                // get the parent
                var parent = firstSymbolGameObject.transform.parent;
                // clone symbolPropertiesGroup
                var newSymbolPropertiesGroup =
                    BettrVisualsController.Instance.CloneGameObjectGroup(symbolPropertiesGroup);
                foreach (var newSymbolProperty in newSymbolPropertiesGroup.gameObjectProperties)
                {
                    var newSymbolGameObject = newSymbolProperty.value.GameObject;
                    symbolGroupProperty.gameObjectProperties.Add(newSymbolProperty); 
                    newSymbolGameObject.transform.SetParent(parent, false);
                }
                newSymbolPropertiesGroups.Add(newSymbolPropertiesGroup);
            }

            return newSymbolPropertiesGroups;
        }

        public void RemoveSymbolsFromReelSymbolGroups(string mechanicName, BettrReelController reelController, List<TilePropertyGameObjectGroup> newSymbolPropertiesGroups)
        {
            var symbolCount = (int) (double) reelController.ReelStateTable["SymbolCount"];
            for (var symbolIndex = 1; symbolIndex <= symbolCount; symbolIndex++)
            {
                var symbolGroupKey = $"SymbolGroup{symbolIndex}";
                var symbolGroupProperty = (TilePropertyGameObjectGroup) reelController.ReelTable[symbolGroupKey];
                if (symbolGroupProperty == null)
                {
                    throw new System.Exception($"SymbolGroup{symbolIndex} is null");
                }
                var newSymbolPropertiesGroup = newSymbolPropertiesGroups[symbolIndex - 1];
                var newSymbolProperties = newSymbolPropertiesGroup.gameObjectProperties;
                // remove symbolGroupProperty.gameObjectProperties entries if the key matches a  newSymbolProperties
                symbolGroupProperty.gameObjectProperties.RemoveAll(newSymbolProperties.Contains);
                // set parent of newSymbolProperties.Value.GameObject to null and destroy the GameObject
                foreach (var newSymbolProperty in newSymbolProperties)
                {
                    newSymbolProperty.value.GameObject.transform.SetParent(null);
                    Object.Destroy(newSymbolProperty.value.GameObject);
                }
            }
        }
        

        // Function to print the table with recursion for nested tables using StringBuilder
        public void PrintTable(Table table, int indent = 2)
        {
            // Create a StringBuilder to accumulate the output
            StringBuilder sb = new StringBuilder();

            // Generate indentation based on depth
            string indentStr = new string(' ', indent * 2);

            // Iterate over each TablePair in the table
            foreach (var pair in table.Pairs)
            {
                var key = pair.Key.ToString();
                var value = pair.Value;

                if (value.Type == DataType.Table) // If value is a nested table, recursively print it
                {
                    sb.AppendLine(indentStr + key + ":");
                    // Recursively call PrintTable for nested table
                    PrintTableHelper(value.Table, sb, indent + 1);
                }
                else
                {
                    sb.AppendLine(indentStr + key + ": " +
                                  value.ToString()); // Append the key-value pair to the StringBuilder
                }
            }

            // Log the accumulated string once
            Debug.Log(sb.ToString());
        }

        // Helper method to handle recursion and build the string
        private void PrintTableHelper(Table table, StringBuilder sb, int indent)
        {
            // Generate indentation for nested tables
            string indentStr = new string(' ', indent * 2);

            // Iterate over each TablePair in the nested table
            foreach (var pair in table.Pairs)
            {
                var key = pair.Key.ToString();
                var value = pair.Value;

                if (value.Type == DataType.Table) // If value is a nested table, recursively print it
                {
                    sb.AppendLine(indentStr + key + ":");
                    // Recursively call PrintTable for nested table
                    PrintTableHelper(value.Table, sb, indent + 1);
                }
                else
                {
                    sb.AppendLine(indentStr + key + ": " +
                                  value.ToString()); // Append the key-value pair to the StringBuilder
                }
            }
        }
    }
}