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

        public void AddMechanicsReelSymbolGroups(string mechanicName, BettrReelController reelController, TilePropertyGameObjectGroup source)
        {
            var symbolCount = (int) (double) reelController.ReelStateTable["SymbolCount"];
            for (var symbolIndex = 1; symbolIndex <= symbolCount; symbolIndex++)
            {
                AddMechanicsReelSymbolGroup(mechanicName, reelController, source, symbolIndex);
            }
        }

        public void RemoveMechanicsReelSymbolGroups(string mechanicName, BettrReelController reelController)
        {
            var symbolCount = (int) (double) reelController.ReelStateTable["SymbolCount"];
            for (var symbolIndex = 1; symbolIndex <= symbolCount; symbolIndex++)
            {
                RemoveMechanicsReelSymbolGroup(mechanicName, reelController, symbolIndex);
            }
        }

        public void AddMechanicsReelSymbolGroup(string mechanicName, BettrReelController reelController,
            TilePropertyGameObjectGroup source, int symbolIndex)
        {
            var symbolGroupKey = $"SymbolGroup{symbolIndex}";
            var target = (TilePropertyGameObjectGroup) reelController.ReelTable[symbolGroupKey];
            source = BettrVisualsController.Instance.CloneAndOverlayGroup(source);
            BettrVisualsController.Instance.AddSymbolGroup(source, target);
            // append the mechanic symbol group key
            var mechanicSymbolGroupKey = $"{mechanicName}{symbolGroupKey}";
            // add to reelTable
            reelController.ReelTable[mechanicSymbolGroupKey] = source;
        }

        public void RemoveMechanicsReelSymbolGroup(string mechanicName, BettrReelController reelController, int symbolIndex)
        {
            var symbolGroupKey = $"SymbolGroup{symbolIndex}";
            var mechanicSymbolGroupKey = $"{mechanicName}{symbolGroupKey}";
            // remove from ReelTable
            var source = (TilePropertyGameObjectGroup) reelController.ReelTable[mechanicSymbolGroupKey];
            if (source != null)
            {
                BettrVisualsController.Instance.RemoveSymbolGroup(source);
            }
            reelController.ReelTable.Remove(mechanicSymbolGroupKey);
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