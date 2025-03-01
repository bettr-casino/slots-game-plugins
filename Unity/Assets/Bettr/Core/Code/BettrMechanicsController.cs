using System.Collections.Generic;
using System.Linq;
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
        
        //
        // Mechanics API
        //

        public Table LoadUserMechanicsTable(string mechanicName, object machine, object variant)
        {
            var tableName = $"{machine.ToString().ToLower()}__{variant.ToString().ToLower()}__{mechanicName.ToLower()}";
            var table = (Table) TileController.LuaScript.Globals[tableName];
            return table;
        }
        
        //
        // Reel APIs
        //
        public void SwapReelsForSpin(Table reel1, Table reel2)
        {
            var reel1Controller = (BettrReelController) reel1["BettrReelController"];
            var reel2Controller = (BettrReelController) reel2["BettrReelController"];
            // reel1Strips
            var reel1ReelStripSymbols = reel1Controller.ReelStripSymbolsForThisSpin;
            // reel2Strips
            var reel2ReelStripSymbols = reel2Controller.ReelStripSymbolsForThisSpin;
            // swap reel1Strips with reel2Strips
            reel1Controller.SwapInReelStripSymbolsForSpin(reel2ReelStripSymbols);
            reel2Controller.SwapInReelStripSymbolsForSpin(reel1ReelStripSymbols);
            // reel1 SpinOutcomeTable
            var reel1OutcomeTable = reel1Controller.SpinOutcomeTable;
            // reel2 SpinOutcomeTable
            var reel2OutcomeTable = reel2Controller.SpinOutcomeTable;
            // swap reel1 SpinOutcomeTable with reel2 SpinOutcomeTable
            reel1Controller.SwapInReelSpinOutcomeTableForSpin(reel2OutcomeTable);
            reel2Controller.SwapInReelSpinOutcomeTableForSpin(reel1OutcomeTable);
        }
        
        //
        // Reel APIs
        //
        public void UndoSwapReelsForSpin(Table reel1, Table reel2)
        {
            var reel1Controller = (BettrReelController) reel1["BettrReelController"];
            var reel2Controller = (BettrReelController) reel2["BettrReelController"];
            // undo swap ReelStrips
            reel1Controller.UndoSwapInReelStripSymbolsForSpin();
            reel2Controller.UndoSwapInReelStripSymbolsForSpin();
            // undo swap SpinOutcomeTable
            reel1Controller.UndoSwapInReelSpinOutcomeTableForSpin();
            reel2Controller.UndoSwapInReelSpinOutcomeTableForSpin();
        }

        public string GetMinReelID(params string[] reels)
        {
            var minReel = reels[0];
            var minReelNumber = int.Parse(minReel.Substring(4));

            foreach (var reel in reels.Skip(1))
            {
                var reelNumber = int.Parse(reel.Substring(4));
                if (reelNumber < minReelNumber)
                {
                    minReel = reel;
                    minReelNumber = reelNumber;
                }
            }
            return minReel;
        }

        public string GetMaxReelID(params string[] reels)
        {
            var maxReel = reels[0];
            var maxReelNumber = int.Parse(maxReel.Substring(4));

            foreach (var reel in reels.Skip(1))
            {
                var reelNumber = int.Parse(reel.Substring(4));
                if (reelNumber > maxReelNumber)
                {
                    maxReel = reel;
                    maxReelNumber = reelNumber;
                }
            }
            return maxReel;
        }
        
        //
        // Reel SymbolMatrix APIs
        //

        public int GetReelVisibleSymbolCount(string machineName, int reelIndex)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelVisibleSymbolCount(machineName, reelID);
        }

        public int GetReelVisibleSymbolCount(string machineName, string reelID)
        {
            var globals = TileController.LuaScript.Globals;
            var globalKey = $"{machineName}BaseGame{reelID}";
            var reelTable = (Table) globals[globalKey];
            var reelController = (BettrReelController) reelTable["BettrReelController"];
            var visibleSymbolCount = reelController.GetReelVisibleSymbolCount();
            return visibleSymbolCount;
        }

        public TilePropertyGameObjectGroup GetReelVisibleSymbolGroup(string machineName, int reelIndex, int offset)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelVisibleSymbolGroup(machineName, reelID, offset);
        }

        public TilePropertyGameObjectGroup GetReelVisibleSymbolGroup(string machineName, string reelID, int offset)
        {
            var reelSymbolMatrixGroups = GetReelSymbolMatrixGroups(machineName, reelID);
            if (offset < 0)
            {
                offset = reelSymbolMatrixGroups.Count + offset;
            }

            var visibleSymbolGroup = reelSymbolMatrixGroups[offset];
            return visibleSymbolGroup;
        }

        public GameObject GetReelVisibleSymbol(string machineName, int reelIndex, int offset)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelVisibleSymbol(machineName, reelID, offset);
        }

        public GameObject GetReelVisibleSymbol(string machineName, string reelID, int offset)
        {
            var reelSymbolMatrix = GetReelSymbolMatrix(machineName, reelID);
            if (offset < 0)
            {
                offset = reelSymbolMatrix.Count + offset;
            }

            var visibleSymbol = reelSymbolMatrix[offset];
            var visibleQuad = FindSymbolQuad(visibleSymbol);
            return visibleQuad;
        }

        public GameObject GetReelBottomVisibleSymbol(string machineName, int reelIndex)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelBottomVisibleSymbol(machineName, reelID);
        }

        public GameObject GetReelBottomVisibleSymbol(string machineName, string reelID)
        {
            return GetReelVisibleSymbol(machineName, reelID, -1);
        }

        public GameObject GetReelTopVisibleSymbol(string machineName, int reelIndex)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelTopVisibleSymbol(machineName, reelID);
        }

        public GameObject GetReelTopVisibleSymbol(string machineName, string reelID)
        {
            return GetReelVisibleSymbol(machineName, reelID, 0);
        }

        public Rect GetReelBounds(string machineName, int reelIndex)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelBounds(machineName, reelID);
        }

        public Rect GetReelBounds(string machineName, string reelID)
        {
            var reelSymbolMatrix = GetReelSymbolMatrix(machineName, reelID);
            // first visible symbol matrix
            var topVisibleSymbol = reelSymbolMatrix[0];
            // last visible symbol matrix
            var bottomVisibleSymbol = reelSymbolMatrix[^1];
            var topVisibleSymbolGameObject = topVisibleSymbol.value.GameObject;
            var topVisibleQuad = FindSymbolQuad(topVisibleSymbol);
            var bottomVisibleSymbolGameObject = bottomVisibleSymbol.value.GameObject;
            var bottomSymbolQuad = FindSymbolQuad(bottomVisibleSymbol);
            var topQuadBounds = BettrVisualsController.Instance.GetQuadBounds(topVisibleQuad);
            var bottomQuadBounds = BettrVisualsController.Instance.GetQuadBounds(bottomSymbolQuad);
            return new Rect(bottomQuadBounds.x, bottomQuadBounds.y, topQuadBounds.width,
                topQuadBounds.y - bottomQuadBounds.y + topQuadBounds.height);
        }

        //
        // SymbolMatrix APIs
        //
        
        public List<GameObject> GetSymbolMatrixGameObjects(string machineName, int reelCount, params string[] symbols)
        {
            var symbolMatrixGameObjects = new List<GameObject>();
            var symbolMatrix = GetSymbolMatrix(machineName, reelCount, symbols);
            foreach (var reelMatrixSymbols in symbolMatrix)
            {
                foreach (var symbol in reelMatrixSymbols)
                {
                    var gameObject = symbol.value.GameObject;
                    // get the Quad game object within this gameObject
                    var quadGameObject = gameObject.transform.Find("Pivot").Find("Quad").gameObject;
                    symbolMatrixGameObjects.Add(quadGameObject);
                }
            }

            return symbolMatrixGameObjects;
        }

        public List<List<TilePropertyGameObjectGroup>> GetSymbolMatrixGroups(string machineName, int reelCount,
            params string[] symbols)
        {
            var symbolMatrixSymbolsGroups = new List<List<TilePropertyGameObjectGroup>>();
            for (var i = 0; i < reelCount; i++)
            {
                var reelMatrixSymbolsGroups = GetReelSymbolMatrixGroups(machineName, i, symbols);
                symbolMatrixSymbolsGroups.Add(reelMatrixSymbolsGroups);
            }

            return symbolMatrixSymbolsGroups;
        }

        public List<List<TilePropertyGameObject>> GetSymbolMatrix(string machineName, int reelCount,
            params string[] symbols)
        {
            var symbolMatrixSymbols = new List<List<TilePropertyGameObject>>();
            for (var i = 0; i < reelCount; i++)
            {
                var reelMatrixSymbols = GetReelSymbolMatrix(machineName, i, symbols);
                symbolMatrixSymbols.Add(reelMatrixSymbols);
            }

            return symbolMatrixSymbols;
        }

        public List<TilePropertyGameObjectGroup> GetReelSymbolMatrixGroups(string machineName, int reelIndex,
            params string[] symbols)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelSymbolMatrixGroups(machineName, reelID, symbols);
        }

        public List<TilePropertyGameObjectGroup> GetReelSymbolMatrixGroups(string machineName, string reelID,
            params string[] symbols)
        {
            var globals = TileController.LuaScript.Globals;
            var globalKey = $"{machineName}BaseGame{reelID}";
            var reelTable = (Table) globals[globalKey];
            var reelController = (BettrReelController) reelTable["BettrReelController"];
            var reelMatrixSymbols = reelController.GetReelMatrixVisibleSymbolsGroups(symbols);
            return reelMatrixSymbols;
        }
        
        public List<TilePropertyGameObject> GetReelSymbolMatrix(string machineName, int reelIndex,
            params string[] symbols)
        {
            var reelID = $"Reel{reelIndex + 1}";
            return GetReelSymbolMatrix(machineName, reelID, symbols);
        }

        public List<TilePropertyGameObject> GetReelSymbolMatrix(string machineName, string reelID,
            params string[] symbols)
        {
            var globals = TileController.LuaScript.Globals;
            var globalKey = $"{machineName}BaseGame{reelID}";
            var reelTable = (Table) globals[globalKey];
            var reelController = (BettrReelController) reelTable["BettrReelController"];
            var reelMatrixSymbols = reelController.GetReelMatrixVisibleSymbols(symbols);
            return reelMatrixSymbols;
        }

        //
        // SymbolGroup APIs
        //
        
        public MeshRenderer[] GetSymbolGroupMeshRenderers(GameObject go)
        {
            var symbolGroupGo = FindGameObjectInHierarchy(go, "SymbolGroup");
            var meshRenderers = symbolGroupGo.GetComponentsInChildren<MeshRenderer>();
            return meshRenderers;
        }

        public List<TilePropertyGameObjectGroup> AddSymbolsToReelSymbolGroups(string mechanicName,
            BettrReelController reelController, TilePropertyGameObjectGroup symbolPropertiesGroup)
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

        public void RemoveSymbolsFromReelSymbolGroups(string mechanicName, BettrReelController reelController,
            List<TilePropertyGameObjectGroup> newSymbolPropertiesGroups)
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

        
        //
        // Symbol APIs
        //
        
        public GameObject FindSymbolQuad(TilePropertyGameObject symbol)
        {
            var gameObject = symbol.value.GameObject;
            // get the Quad game object within this gameObject
            var quadGameObject = gameObject.transform.Find("Pivot").Find("Quad").gameObject;
            return quadGameObject;
        }

        //
        // Helper Methods
        //
        
        public GameObject FindGameObjectInHierarchy(GameObject go, string gameObjectName, string parentGameObjectName = null)
        {
            Transform[] allTransforms = go.GetComponentsInChildren<Transform>(true);
            foreach (Transform transform in allTransforms)
            {
                if (transform.gameObject.name == gameObjectName)
                {
                    if (parentGameObjectName != null && transform.parent.name != parentGameObjectName)
                    {
                        continue;
                    }
                    return transform.gameObject;
                }
            }

            return null;
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