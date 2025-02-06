// ReSharper disable All
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
namespace Bettr.Core
{
    
    [Serializable]
    public class BettrReelMatrixController : MonoBehaviour 
    {
        private Tile Tile { get; set; }
        private Table TileTable { get; set; }
        
        private string MachineID { get; set; }
        private string MachineVariantID { get; set; }
        private string MechanicName { get; set; }

        private BettrUserController BettrUserController { get; set; }
        private BettrMathController BettrMathController { get; set; }

        internal BettrReelMatrixCellController BettrReelMatrixCellController { get; private set; }
        
        internal Dictionary<string, BettrReelMatrixCellController> BettrReelMatrixCellControllers { get; private set; }
        
        public int[] RowCounts { get; private set; }
        public int ColumnCount { get; private set;  }

        private void Awake()
        {
            Tile = GetComponent<Tile>();
            BettrUserController = BettrUserController.Instance;
            BettrMathController = BettrMathController.Instance;
        }

        private void Start()
        {
            this.MachineID = Tile.GetProperty<string>("MachineID");
            this.MachineVariantID = Tile.GetProperty<string>("MachineVariantID");
            this.MechanicName = Tile.GetProperty<string>("MechanicName");
            
            this.BettrReelMatrixCellControllers = new Dictionary<string, BettrReelMatrixCellController>();
            
            var reelMatrixDataSummaryTable = BettrMathController.GetBaseGameMechanicDataSummary(this.MachineID, this.MechanicName, "ReelMatrix");
            this.ColumnCount = (int) (double) reelMatrixDataSummaryTable["ColumnCount"];
            this.RowCounts = new int[this.ColumnCount];

            var reelMatrixDataTable = BettrMathController.GetBaseGameMechanicData(this.MachineID, this.MechanicName, "ReelMatrix");
            for (var i = 0; i < this.ColumnCount; i++)
            {
                var row = (Table) reelMatrixDataTable[i + 1];
                var rowCount = (int) (double) row["RowCount"];
                var columnIndex = (int) (double) row["ColumnIndex"];
                this.RowCounts[columnIndex] = rowCount;
            }

            this.TileTable = BettrMathController.GetGlobalTable(Tile.globalTileId);
            
            for (var columnIndex = 1; columnIndex <= this.ColumnCount; columnIndex++)
            {
                for (var rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var bettrReelMatrixCellController = this.gameObject.AddComponent<BettrReelMatrixCellController>();
                    bettrReelMatrixCellController.Initialize(Tile, TileTable, BettrUserController, BettrMathController, MachineID, MachineVariantID, MechanicName, rowIndex, columnIndex);
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    this.BettrReelMatrixCellControllers[key] = bettrReelMatrixCellController;
                }
            }
            
            this.TileTable["BettrReelMatrixController"] = this;
        }
        
        private void OnDisable()
        {
            var components = this.gameObject.GetComponents<BettrReelMatrixCellController>();
            foreach (var component in components)
            {
                Destroy(component);   
            }

            if (BettrReelMatrixCellControllers != null)
            {
                this.BettrReelMatrixCellControllers.Clear();
            }
        }
        
        //
        // APIs
        // 
        
        /**
         * This sets the reel strip on the ReelMatrix.
         * ReelMatrix will use a default reel strip if not set.
         */
        public void SetReelStrip(string[] reelSymbols)
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex - 1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SetReelStrip(reelSymbols);
                }
            }
        }

        public void SetReelStripSymbolMaterials(BettrReelStripSymbolMaterials symbolMaterials)
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex - 1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SetReelStripSymbolMaterials(symbolMaterials);
                }
            }
        }

        public IEnumerator StartEngines()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    StartCoroutine(bettrReelMatrixCellController.StartEngines());
                }
            }
            yield break;
        }
        
        public IEnumerator OnOutcomeReceived()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    StartCoroutine(bettrReelMatrixCellController.OnOutcomeReceived());
                }
            }
            yield break;
        }
        
        public IEnumerator OnApplyOutcomeReceived()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    StartCoroutine(bettrReelMatrixCellController.OnApplyOutcomeReceived());
                }
            }
            yield break;
        }

        public void SpinEngines()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinEngines();
                }
            }
        }

        // Dispatch Handler
        public void SpinReelSpinStartedRollBack()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinReelSpinStartedRollBack();
                }
            }
        }

        public void SpinReelSpinStartedRollForward()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinReelSpinStartedRollForward();
                }
            }
        }
        
        public void SpinReelSpinEndingRollBack()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinReelSpinEndingRollBack();
                }
            }
        }
        
        public void SpinReelSpinEndingRollForward()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinReelSpinEndingRollForward();
                }
            }
        }
        
        public void SpinReelSpinning()
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex-1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    // add to the dictionary
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SpinReelSpinning();
                }
            }
        }
    }
    
    [Serializable]
    public class BettrReelMatrixOutcomes
    {
        private int RowIndex { get; set; }
        private int ColumnIndex { get; set; }
        
        private string MachineID { get; set; }
        private string MechanicName { get; set; }
        private BettrMathController MathController { get; set; }
        
        public BettrReelMatrixOutcomes(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            this.MachineID = cellController.MachineID;
            this.MechanicName = cellController.MechanicName;
            
            this.RowIndex = cellController.RowIndex;
            this.ColumnIndex = cellController.ColumnIndex;
        }
        
        public T GetProperty<T>(string propKey)
        {
            var key = $"Row{RowIndex}Col{ColumnIndex}";
            var rows = MathController.GetBaseGameMechanicMatrix(this.MachineID, this.MechanicName, key);
            var row = (Table) rows[1];
            var propValue = row[propKey];
            if (propValue is T value) { return value; }
            // Handle special case: double to int conversion
            if (typeof(T) == typeof(int) && propValue is double d) { return (T)(object)Convert.ToInt32(d); }
            return (T)Convert.ChangeType(propValue, typeof(T));
        }
    }

    public class BettrReelStripSymbolMaterial
    {
        public string symbolName { get; internal set; }
        public Material symbolMaterial { get; internal set; }
        
        public BettrReelStripSymbolMaterial(string symbolName, Material symbolMaterial)
        {
            this.symbolName = symbolName;
            this.symbolMaterial = symbolMaterial;
        }
    }
    
    public class BettrReelStripSymbolMaterials
    {
        public List<BettrReelStripSymbolMaterial> SymbolMaterials { get; internal set; }

        public BettrReelStripSymbolMaterials(List<GameObject> symbolGameObjects)
        {
            SymbolMaterials = new List<BettrReelStripSymbolMaterial>();
            foreach (var symbolGameObject in symbolGameObjects)
            {
                var symbolName = symbolGameObject.name;
                var symbolRenderer = symbolGameObject.GetComponent<MeshRenderer>();
                var sharedMaterial = symbolRenderer.sharedMaterial;
                SymbolMaterials.Add(new BettrReelStripSymbolMaterial(symbolName, sharedMaterial));
            }
        }

        public Material UpdateReelSymbolMaterial(string symbolName, MeshRenderer meshRenderer)
        {
            // get the shared material
            var sharedMaterial = meshRenderer.sharedMaterial;
            // find the symbol material
            var symbolMaterial = SymbolMaterials.Find(material => material.symbolName == symbolName);
            // replace the shared material
            meshRenderer.sharedMaterial = symbolMaterial.symbolMaterial;
            return sharedMaterial;
        }
    }

    public class BettrReelStripSymbolsForSpin
    { 
        public List<string> ReelSymbolsForThisSpin { get; set; }
        public BettrReelStripSymbolsForSpin()
        {
            // create a copy of the ReelSymbolsTable
            this.ReelSymbolsForThisSpin = new List<string>();
        }
        public void Reset(string[] reelSymbols)
        {
            this.ReelSymbolsForThisSpin.AddRange(reelSymbols); // 1 indexed
        }
    }
    
    public class BettrReelStripSymbolsForThisSpin
    {
        public BettrReelStripSymbolsForSpin ReelStripSymbolsForThisSpin { get; internal set; }
        
        private BettrReelMatrixReelStrip ReelStrip { get; set; }
        public BettrReelStripSymbolsForThisSpin(BettrReelMatrixCellController cellController)
        {
            ReelStripSymbolsForThisSpin = new BettrReelStripSymbolsForSpin();
            ReelStrip = cellController.BettrReelMatrixReelStrip;
        }
        public void Reset()
        {
            ReelStripSymbolsForThisSpin.Reset(ReelStrip.ReelSymbols);
        }
    }

    public class BettrReelMatrixLayoutPropertiesData
    { 
        public Table LayoutPropertiesTable { get; internal set; }
        
        private BettrMathController MathController;
        public int TopSymbolCount { get; internal set; }
        public int VisibleSymbolCount { get; internal set; }
        public int BottomSymbolCount { get; internal set; }
        public int SymbolCount { get; internal set; }
        
        public BettrReelMatrixLayoutPropertiesData(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            LayoutPropertiesTable = mathController.GetBaseGameMechanicData(2, cellController.MachineID, cellController.MechanicName, "LayoutProperties");
            MathController = mathController;
            
            var row = (Table) LayoutPropertiesTable[1];
            
            TopSymbolCount = (int) (double) row["TopSymbolCount"];
            VisibleSymbolCount = (int) (double) row["VisibleSymbolCount"];
            BottomSymbolCount = (int) (double) row["BottomSymbolCount"];
            SymbolCount = (int) (double) row["SymbolCount"];
        }
        
        public T GetProperty<T>(string key)
        {
            var row = (Table) LayoutPropertiesTable[1];
            var propValue = row[key];
            if (propValue is T value) { return value; }
            // Handle special case: double to int conversion
            if (typeof(T) == typeof(int) && propValue is double d) { return (T)(object)Convert.ToInt32(d); }
            return (T)Convert.ChangeType(propValue, typeof(T));
        }
    }
    
    public class BettrReelMatrixSpinPropertiesData
    {
        public Table SpinPropertiesTable { get; internal set; }
        
        private BettrMathController MathController;
        public BettrReelMatrixSpinPropertiesData(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            SpinPropertiesTable = mathController.GetBaseGameMechanicData(3, cellController.MachineID, cellController.MechanicName, "SpinProperties");
            MathController = mathController;
        }
    }
    
    public class BettrReelMatrixSymbolsData
    {
        public Table SymbolsTable { get; internal set; }
        
        private BettrMathController MathController;
        public BettrReelMatrixSymbolsData(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            SymbolsTable = mathController.GetBaseGameMechanicData(4, cellController.MachineID, cellController.MechanicName, "Symbols");
            MathController = mathController;
        }
    }
    
    public class BettrReelMatrixColumnsData
    {
        public Table ColumnsTable { get; internal set; }
        
        private BettrMathController MathController;
        public BettrReelMatrixColumnsData(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            ColumnsTable = mathController.GetBaseGameMechanicData(5, cellController.MachineID, cellController.MechanicName, "Columns");
            MathController = mathController;
        }
    }
    
    public class BettrReelMatrixSymbolGroupsData
    {
        public Table SymbolGroupsTable { get; internal set; }
        
        private BettrMathController MathController;
        
        private int RowIndex { get; set; }
        
        public BettrReelMatrixSymbolGroupsData(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            SymbolGroupsTable = mathController.GetBaseGameMechanicData(6, cellController.MachineID, cellController.MechanicName, "SymbolGroups");
            MathController = mathController;
            
            RowIndex = cellController.RowIndex;
        }
        
        public T GetProperty<T>(string propKey, int symbolIndex)
        {
            var key = $"Row{RowIndex}";
            var row = MathController.GetTableRow(SymbolGroupsTable, "Row", key, "SymbolGroup", $"SymbolGroup{symbolIndex}");
            var propValue = row[propKey];
            if (propValue is T value) { return value; }
            // Handle special case: double to int conversion
            if (typeof(T) == typeof(int) && propValue is double d) { return (T)(object)Convert.ToInt32(d); }
            return (T)Convert.ChangeType(propValue, typeof(T));
        }
    }
    
    public class BettrReelMatrixSpinState
    {
        public Table SpinStateTable { get; internal set; }
        
        private BettrMathController MathController;
        private int RowIndex { get; set; }
        private int ColumnIndex { get; set; }
        
        public BettrReelMatrixSpinState(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            SpinStateTable = mathController.GetBaseGameMechanic(2, cellController.MachineID, cellController.MechanicName, "SpinState");
            MathController = mathController;

            this.RowIndex = cellController.RowIndex;
            this.ColumnIndex = cellController.ColumnIndex;
        }

        public T GetProperty<T>(string propKey)
        {
            var key = $"Row{RowIndex}Col{ColumnIndex}";
            var row = MathController.GetTableRow(SpinStateTable, "Cell", key);
            var propValue = row[propKey];
            if (propValue is T value) { return value; }
            // Handle special case: double to int conversion
            if (typeof(T) == typeof(int) && propValue is double d) { return (T)(object)Convert.ToInt32(d); }
            return (T)Convert.ChangeType(propValue, typeof(T));
        }
        
        public void SetProperty<T>(string propKey, T propValue)
        {
            var key = $"Row{RowIndex}Col{ColumnIndex}";
            var row = MathController.GetTableRow(SpinStateTable, "Cell", key);
            var oldPropValue = row[propKey];
            row[propKey] = propValue;
        }
    }
    
    public class BettrReelMatrixState
    {
        public Table StateTable { get; internal set; }
        
        public BettrReelMatrixState(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            StateTable = mathController.GetBaseGameMechanic(cellController.MachineID, cellController.MechanicName, "ReelMatrix");
        }
    }
    
    public class BettrReelMatrixStateSummary
    {
        public Table StateSummaryTable { get; internal set; }
        
        public BettrReelMatrixStateSummary(BettrReelMatrixCellController cellController, BettrMathController mathController)
        {
            StateSummaryTable = mathController.GetBaseGameMechanicSummary(cellController.MachineID, cellController.MechanicName, "ReelMatrix");
        }
    }
    
    public class BettrReelMatrixReelSet
    {
        private BettrReelMatrixReelStrip ReelStrip { get; set; }
        public BettrReelMatrixReelSet(BettrReelMatrixReelStrip reelStrip)
        {
            ReelStrip = reelStrip;            
        }
        
        public int GetReelSymbolCount()
        {
            return (int) (double) ReelStrip.ReelSymbolCount;
        }
        
        public string[] GetReelSymbols()
        {
            return ReelStrip.ReelSymbols;
        }
        
        public string GetReelSymbol(int symbolIndex)
        {
            var reelSymbols = GetReelSymbols();
            return reelSymbols[symbolIndex];
        }
    }

    public class BettrReelMatrixReelStrip
    {
        public int ReelSymbolCount { get; internal set; }
        public string[] ReelSymbols { get; internal set; }
        public int[] ReelIndexes { get; internal set; }
        public int[] ReelWeights { get; internal set; }

        public BettrReelMatrixReelStrip(string[] reelSymbols)
        {
            ReelSymbolCount = reelSymbols.Length;
            
            ReelSymbols = new string[ReelSymbolCount + 1];  // 1 indexes
            ReelIndexes = new int[ReelSymbolCount + 1];     // 1 indexed
            ReelWeights = new int[ReelSymbolCount + 1];     // 1 indexed

            // for 1 indexing
            ReelSymbols[0] = "Blank";
            ReelIndexes[0] = -1;
            ReelWeights[0] = 1;
            
            Array.Copy(reelSymbols, 0, ReelSymbols, 1, ReelSymbolCount);
        }


        public BettrReelMatrixReelStrip() : this(Enumerable.Repeat("SC", 20).ToArray())
        {
        }
        
        public string GetReelSymbol(int symbolIndex)
        {
            var reelSymbols = ReelSymbols;
            return reelSymbols[symbolIndex];
        }
    }

    public delegate void BettrReelMatrixCellDispatchHandler();
    
    public class BettrReelMatrixCellDispatcher
    {
        public Dictionary<string, BettrReelMatrixCellDispatchHandler> DispatchHandlers { get; internal set; }
        
        public BettrReelMatrixCellDispatcher(BettrReelMatrixCellController cellController)
        {
            DispatchHandlers = new Dictionary<string, BettrReelMatrixCellDispatchHandler>();
            
            DispatchHandlers["Waiting"] = cellController.SpinReelWaiting;
            DispatchHandlers["Spinning"] = cellController.SpinReelSpinning;
            DispatchHandlers["Stopped"] = cellController.SpinReelStopped;
            DispatchHandlers["StoppedWaiting"] = cellController.SpinReelStoppedWaiting;
            DispatchHandlers["ReachedOutcomeStopIndex"] = cellController.SpinReelReachedOutcomeStopIndex;
            DispatchHandlers["SpinStartedRollBack"] = cellController.SpinReelSpinStartedRollBack;
            DispatchHandlers["SpinStartedRollForward"] = cellController.SpinReelSpinStartedRollForward;
            DispatchHandlers["SpinEndingRollForward"] = cellController.SpinReelSpinEndingRollForward;
            DispatchHandlers["SpinEndingRollBack"] = cellController.SpinReelSpinEndingRollBack;
        }
    }
    
    
    [Serializable]
    public class BettrReelMatrixCellController : MonoBehaviour
    {
        private Tile Tile { get; set; }
        private Table TileTable { get; set; }
        public string MachineID { get; internal set; }
        public string MachineVariantID { get; internal set; }
        public string MechanicName { get; internal set; }
        
        public int RowIndex { get; private set; }
        public int ColumnIndex { get; private set; }
        
        public BettrReelMatrixReelStrip BettrReelMatrixReelStrip { get; internal set; }
        public BettrReelMatrixLayoutPropertiesData BettrReelMatrixLayoutPropertiesData { get; internal set; }
        
        public BettrReelMatrixSpinPropertiesData BettrReelMatrixSpinPropertiesData { get; internal set; }
        
        public BettrReelMatrixSymbolsData BettrReelMatrixSymbolsData { get; internal set; }
        
        public BettrReelMatrixSymbolGroupsData BettrReelMatrixSymbolGroupsData { get; internal set; }
        public BettrReelMatrixSpinState BettrReelMatrixSpinState { get; internal set; }
        public BettrReelMatrixState BettrReelMatrixState { get; internal set; }
        public BettrReelMatrixStateSummary BettrReelMatrixStateSummary { get; internal set; }
        public BettrReelMatrixOutcomes BettrReelMatrixOutcomes { get; internal set; }
        
        public BettrReelStripSymbolsForThisSpin BettrReelStripSymbolsForThisSpin { get; internal set; }
        
        public BettrReelStripSymbolMaterials BettrSymbolMaterials { get; internal set; }
        
        private bool ShouldSpliceReel { get; set; }
        
        private BettrUserController BettrUserController { get; set; }
        
        private BettrMathController BettrMathController { get; set; }
        
        private BettrReelMatrixCellDispatcher BettrReelMatrixCellDispatcher { get; set; }
        
        public void Initialize(Tile tile, Table tileTable, BettrUserController userController, BettrMathController mathController, string machineID, string machineVariantID, string mechanicName, int rowIndex, int columnIndex)
        {
            this.BettrUserController = userController;
            this.BettrMathController = mathController;
            
            this.Tile = tile;
            this.TileTable = tileTable;

            this.RowIndex = rowIndex;
            this.ColumnIndex = columnIndex;

            this.MachineID = machineID;
            this.MachineVariantID = machineVariantID;
            this.MechanicName = mechanicName;
            
            this.BettrReelMatrixLayoutPropertiesData = new BettrReelMatrixLayoutPropertiesData(this, BettrMathController);
            this.BettrReelMatrixSpinPropertiesData = new BettrReelMatrixSpinPropertiesData(this, BettrMathController);
            this.BettrReelMatrixSymbolsData = new BettrReelMatrixSymbolsData(this, BettrMathController);
            this.BettrReelMatrixSymbolGroupsData = new BettrReelMatrixSymbolGroupsData(this, BettrMathController);
            
            // *DEFAULT REEL STRIP* which can be replaced with the actual reel strip from the mechanic
            this.BettrReelMatrixReelStrip = new BettrReelMatrixReelStrip();
            
            this.BettrReelMatrixSpinState = new BettrReelMatrixSpinState(this, BettrMathController);
            
            this.BettrReelMatrixStateSummary = new BettrReelMatrixStateSummary(this, BettrMathController);
            this.BettrReelMatrixState = new BettrReelMatrixState(this, BettrMathController);
            this.BettrReelMatrixOutcomes = new BettrReelMatrixOutcomes(this, BettrMathController);

            this.BettrReelStripSymbolsForThisSpin = new BettrReelStripSymbolsForThisSpin(this);

            this.BettrReelMatrixCellDispatcher = new BettrReelMatrixCellDispatcher(this);
        }

        public void SetReelStripSymbolMaterials(BettrReelStripSymbolMaterials symbolMaterials)
        {
            this.BettrSymbolMaterials = symbolMaterials;
        }

        public void SetReelStrip(string[] reelSymbols)
        {
            this.BettrReelMatrixReelStrip = new BettrReelMatrixReelStrip(reelSymbols);
        }

        private void Update()
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var spinState = spinStateTable.GetProperty<string>("ReelSpinState");
            this.BettrReelMatrixCellDispatcher.DispatchHandlers[spinState]();
        }

        private void OnDestroy()
        {
            this.BettrUserController = null;
            this.BettrMathController = null;
            
            this.Tile = null;
            this.TileTable = null;

            this.RowIndex = 0;
            this.ColumnIndex = 0;

            this.MachineID = null;
            this.MachineVariantID = null;
            this.MechanicName = null;

            this.BettrReelMatrixLayoutPropertiesData = null;
            this.BettrReelMatrixSpinPropertiesData = null;
            this.BettrReelMatrixSymbolsData = null;
            this.BettrReelMatrixSymbolGroupsData = null;
            
            this.BettrReelMatrixReelStrip = null;
            this.BettrReelMatrixSpinState = null;
            
            this.BettrReelMatrixStateSummary = null;
            this.BettrReelMatrixState = null;
            this.BettrReelMatrixOutcomes = null;
        }

        public IEnumerator StartEngines()
        {
            // loop over all the cells and start the engines
            var reelStopIndex = this.BettrReelMatrixSpinState.GetProperty<int>("ReelStopIndex");
            var reelSymbolCount = this.BettrReelMatrixReelStrip.ReelSymbolCount;
            var symbolCount = (int) (double) this.BettrReelMatrixLayoutPropertiesData.SymbolCount;
            var reelSymbols = this.BettrReelMatrixReelStrip.ReelSymbols;
            for (int symbolIndex = 1; symbolIndex <= symbolCount; symbolIndex++)
            {
                var symbolGroupsTable = this.BettrReelMatrixSymbolGroupsData;
                var reelPosition = symbolGroupsTable.GetProperty<int>("ReelPosition", symbolIndex);
                int symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
                var reelSymbol = reelSymbols[symbolIndex]; // 1 indexed
                var symbolGroupKey = $"Row{RowIndex}Col{ColumnIndex}SymbolGroup{symbolIndex}";
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.TileTable[symbolGroupKey];
                if (symbolGroupProperty.Current != null)
                {
                    symbolGroupProperty.Current.SetActive(false);
                    symbolGroupProperty.CurrentKey = null;
                }
                var currentValue = (PropertyGameObject) symbolGroupProperty[reelSymbol];
                currentValue.SetActive(true);
                symbolGroupProperty.Current = currentValue;
                symbolGroupProperty.CurrentKey = reelSymbol;
            }
            yield break;
        }
    
        public IEnumerator OnOutcomeReceived()
        {
            yield break;
        }
    
        public IEnumerator OnApplyOutcomeReceived()
        {
            this.ShouldSpliceReel = true;
            this.BettrReelMatrixSpinState.SetProperty<bool>("OutcomeReceived", true);
            yield break;
        }
        
        public void SpinEngines()
        {
            this.ShouldSpliceReel = false;
            this.BettrReelMatrixSpinState.SetProperty<string>("ReelSpinState", "SpinStartedRollBack");
            this.BettrReelMatrixSpinState.SetProperty<bool>("ReachedOutcomeStopIndex", false);
            this.BettrReelMatrixSpinState.SetProperty<bool>("OutcomeReceived", false);

            this.BettrReelStripSymbolsForThisSpin.Reset();
        }
    
        public void SpinReelSpinStartedRollBack()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            var layoutProperties = this.BettrReelMatrixLayoutPropertiesData;
            var spinState = this.BettrReelMatrixSpinState;
            
            var slideDistanceThresholdInSymbolUnits = (float) layoutProperties.GetProperty<double>("SpinStartedRollBackDistanceInSymbolUnits");
            
            spinState.SetProperty<double>("SpeedInSymbolUnitsPerSecond", layoutProperties.GetProperty<double>("SpinStartedRollBackSpeedInSymbolUnitsPerSecond") * speed);
            var reelSpinDirection = spinState.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    spinState.SetProperty<string>("ReelSpinState", "SpinStartedRollForward");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    spinState.SetProperty<string>("ReelSpinState", "SpinStartedRollForward");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinStartedRollForward()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            var spinState = this.BettrReelMatrixSpinState;
            var layoutProperties = this.BettrReelMatrixLayoutPropertiesData;
            
            spinState.SetProperty<double>("SpeedInSymbolUnitsPerSecond", layoutProperties.GetProperty<double>("SpinStartedRollForwardSpeedInSymbolUnitsPerSecond") * speed);
                    
            var reelSpinDirection = spinState.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < 0)
                {
                    SlideReelSymbols(0);
                    spinState.SetProperty<string>("ReelSpinState", "Spinning");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits > 0)
                {
                    SlideReelSymbols(0);
                    spinState.SetProperty<string>("ReelSpinState", "Spinning");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinEndingRollBack()
        {
            // -- spin ending roll back animation
            Tile.StartCoroutine(this.Tile.CallAction("PlaySpinReelSpinEndingRollBackAnimation"));
            
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            var spinState = this.BettrReelMatrixSpinState;
            var layoutProperties = this.BettrReelMatrixLayoutPropertiesData;
            
            // loop over the RowCount and ColumnCount
            spinState.SetProperty<double>("SpeedInSymbolUnitsPerSecond", layoutProperties.GetProperty<double>("SpinEndingRollBackSpeedInSymbolUnitsPerSecond") * speed);
            
            var reelSpinDirection = spinState.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
                    
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > 0)
                {
                    SlideReelSymbols(0);
                    spinState.SetProperty<string>("ReelSpinState", "Stopped");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits < 0)
                {
                    SlideReelSymbols(0);
                    spinState.SetProperty<string>("ReelSpinState", "Stopped");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinEndingRollForward()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            var spinState = this.BettrReelMatrixSpinState;
            var layoutProperties = this.BettrReelMatrixLayoutPropertiesData;
            
            var slideDistanceThresholdInSymbolUnits = (float) layoutProperties.GetProperty<double>("SpinEndingRollForwardDistanceInSymbolUnits");
            
            // loop over the RowCount and ColumnCount
            spinState.SetProperty<double>("SpeedInSymbolUnitsPerSecond", layoutProperties.GetProperty<double>("SpinEndingRollForwardSpeedInSymbolUnitsPerSecond") * speed);
            var reelSpinDirection = spinState.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
                    
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    spinState.SetProperty<string>("ReelSpinState", "SpinEndingRollBack");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    spinState.SetProperty<string>("ReelSpinState", "SpinEndingRollBack");
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        // Dispatch Handler
        public void SpinReelWaiting()
        {
            
        }
        
        // Dispatch Handler
        public void SpinReelStopped()
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            spinStateTable.SetProperty<string>("ReelSpinState", "StoppedWaiting");
        }
        
        // Dispatch Handler
        public void SpinReelStoppedWaiting()
        {
            
        }

        // Dispatch Handler
        public void SpinReelReachedOutcomeStopIndex()
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            spinStateTable.SetProperty<string>("ReelSpinState", "SpinEndingRollForward");
        }

        // Dispatch Handler
        public void SpinReelSpinning()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            
            var spinState = this.BettrReelMatrixSpinState;
            var layoutProperties = this.BettrReelMatrixLayoutPropertiesData;
            
            var slideDistanceThresholdInSymbolUnits = (float) layoutProperties.GetProperty<double>("SpinEndingRollForwardDistanceInSymbolUnits");
            
            // loop over the RowCount and ColumnCount
            spinState.SetProperty<double>("SpeedInSymbolUnitsPerSecond", layoutProperties.GetProperty<double>("SpinSpeedInSymbolUnitsPerSecond") * speed);
                    
            float slideDistanceInSymbolUnits = AdvanceReel();
            SlideReelSymbols(slideDistanceInSymbolUnits);
        }
        
        public float CalculateSlideDistanceInSymbolUnits()
        {
            // Unity's Time.deltaTime provides the duration of the last frame in seconds
            float frameDurationInSeconds = Time.deltaTime;
            var layoutPropertiesData = this.BettrReelMatrixLayoutPropertiesData;
            var spinState = this.BettrReelMatrixSpinState;
            // Get the speed in symbol units per second from reelSpinState
            float speedInSymbolUnits = (float) spinState.GetProperty<double>("SpeedInSymbolUnitsPerSecond");
            // Calculate distance traveled in this frame
            float distanceInSymbolUnits = speedInSymbolUnits * frameDurationInSeconds;
            // Check spin direction
            var reelSpinDirection = spinState.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
            // Get the current slide distance
            float slideDistanceInSymbolUnits = (float) spinState.GetProperty<double>("SlideDistanceInSymbolUnits");
            // Update the slide distance by adding the distance traveled
            slideDistanceInSymbolUnits += distanceInSymbolUnits;
            return slideDistanceInSymbolUnits;
        }
        
        public float AdvanceReel()
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var layoutPropertiesTable = this.BettrReelMatrixLayoutPropertiesData;
            
            var reelSpinDirection = spinStateTable.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceOffsetInSymbolUnits = spinDirectionIsDown ? 1 : -1;
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            
            while ((spinDirectionIsDown && slideDistanceInSymbolUnits < -1) || (!spinDirectionIsDown && slideDistanceInSymbolUnits > 1))
            {
                AdvanceSymbols();
                UpdateReelStopIndexes();
                ApplySpinReelStop();
                slideDistanceInSymbolUnits += slideDistanceOffsetInSymbolUnits;
                var isReachedOutcomeStopIndex = spinStateTable.GetProperty<bool>("ReachedOutcomeStopIndex");
                if (isReachedOutcomeStopIndex)
                {
                    break;
                }
            }
            
            var spinState = spinStateTable.GetProperty<string>("ReelSpinState");
            if (spinState is "ReachedOutcomeStopIndex" or "Stopped")
            {
                slideDistanceInSymbolUnits = 0;
            }
            
            return slideDistanceInSymbolUnits;
        }
    
        public void AdvanceSymbols()
        {
            var layoutPropertiesTable = this.BettrReelMatrixLayoutPropertiesData;
            var symbolCount = (int) (double) layoutPropertiesTable.SymbolCount;
            for (var i = 1; i <= symbolCount; i++)
            {
                UpdateReelSymbolForSpin(i);
            }
        }
        
        public int GetReelVisibleSymbolCount()
        {
            var layoutPropertiesTable = this.BettrReelMatrixLayoutPropertiesData;
            var visibleSymbolCount = (int) (double) layoutPropertiesTable.VisibleSymbolCount;
            return 0;
        }
        
        public void UpdateReelSymbolForSpin(int symbolIndex)
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var symbolGroupsTable = this.BettrReelMatrixSymbolGroupsData;
            var reelStrip = this.BettrReelMatrixReelStrip;
            
            var symbolIsLocked = spinStateTable.GetProperty<bool>("IsLocked");
            if (symbolIsLocked)
            {
                return;
            }
            
            var rowVisible = symbolGroupsTable.GetProperty<bool>("Visible", symbolIndex);
            var reelStopIndex = spinStateTable.GetProperty<int>("ReelStopIndex");
            var reelSymbolCount = reelStrip.ReelSymbolCount;

            var reelPosition = symbolGroupsTable.GetProperty<int>("ReelPosition", symbolIndex);

            var symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            
            var reelSymbol = reelStrip.GetReelSymbol(symbolStopIndex);
            
            var symbolGroupProperty = GetSymbolGroupProperty(symbolIndex);

            if (symbolGroupProperty.Current != null)
            {
                symbolGroupProperty.Current.SetActive(false);
                symbolGroupProperty.CurrentKey = null;
            }
            
            var currentValue = symbolGroupProperty[reelSymbol];
            currentValue.SetActive(true);
            symbolGroupProperty.Current = currentValue;
            symbolGroupProperty.CurrentKey = reelSymbol;
            
        }
        
        public void UpdateReelStopIndexes()
        {
            var spinState = this.BettrReelMatrixSpinState;
            var layoutPropertiesData = this.BettrReelMatrixLayoutPropertiesData;
            var reelStrip = this.BettrReelMatrixReelStrip;
            // Get the symbol count from reelState
            var reelSymbolCount = reelStrip.ReelSymbolCount;
            // Get the current stop index and advance offset
            var reelStopIndex = spinState.GetProperty<int>("ReelStopIndex");            
            var reelStopIndexAdvanceOffset = layoutPropertiesData.GetProperty<int>("ReelStopIndexAdvanceOffset");
            // Update the reel stop index
            reelStopIndex += reelStopIndexAdvanceOffset;
            // Wrap the stop index to keep it within bounds using modulus
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Assign the updated stop index back to the spin state
            this.BettrReelMatrixSpinState.SetProperty<int>("ReelStopIndex", reelStopIndex);
        }
        
        public void ApplySpinReelStop()
        {
            var spinState = this.BettrReelMatrixSpinState;
            var reelStrip = this.BettrReelMatrixReelStrip;
            var layoutPropertiesData = this.BettrReelMatrixLayoutPropertiesData;
            
            // Check if the outcome has been received
            if (!spinState.GetProperty<bool>("OutcomeReceived"))
            {
                return;
            }
            if (this.ShouldSpliceReel)
            {
                SpliceReel();
                this.ShouldSpliceReel = false;
            }
            
            // // Get the current stop index and outcome-related values
            var reelSymbolCount = reelStrip.ReelSymbolCount;
            var reelStopIndex = spinState.GetProperty<int>("ReelStopIndex");
            var reelStopIndexAdvanceOffset = layoutPropertiesData.GetProperty<int>("ReelStopIndexAdvanceOffset");
            
            // Adjust the stop index
            reelStopIndex -= reelStopIndexAdvanceOffset;
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            
            // Get the outcome's stop index
            var outcomeReelStopIndex = this.BettrReelMatrixOutcomes.GetProperty<int>("OutcomeReelStopIndex");
            
            // Check if the reel stop index matches the outcome stop index
            if (outcomeReelStopIndex == reelStopIndex)
            {
                spinState.SetProperty<string>("ReelSpinState", "ReachedOutcomeStopIndex");
                spinState.SetProperty<bool>("ReachedOutcomeStopIndex", true);
            }
        }
        
        public void SlideReelSymbols(float slideDistanceInSymbolUnits)
        {
            var spinState = this.BettrReelMatrixSpinState;
            var layoutPropertiesData = this.BettrReelMatrixLayoutPropertiesData;

            // // Get the symbol count from reelState
            var symbolCount = layoutPropertiesData.SymbolCount;

            // Iterate through each symbol and apply the slide distance
            for (int i = 1; i <= symbolCount; i++)
            {
                SlideSymbol(i, slideDistanceInSymbolUnits);
            }
            // // Set the SlideDistanceInSymbolUnits for the reel spin state
            spinState.SetProperty<double>("SlideDistanceInSymbolUnits", slideDistanceInSymbolUnits);
        }
    
        public GameObject _FindSymbolQuad(TilePropertyGameObjectGroup symbolGroupProperty)
        {
            var pivotGameObject = symbolGroupProperty.Current["Pivot"];
            var childCount = pivotGameObject.transform.childCount;
            for (int i = 0; i < childCount; i++)
            {
                var child = pivotGameObject.transform.GetChild(i);
                if (child.name == "Quad")
                {
                    return child.gameObject;
                }
            }
            
            return null;
        }
        
        public IEnumerator SymbolRemovalAction(TilePropertyGameObjectGroup symbolGroupProperty)
        {
            var symbolQuad = _FindSymbolQuad(symbolGroupProperty);
            var symbolMeshRenderer = symbolQuad.GetComponent<MeshRenderer>();
            var originalMaterial = symbolMeshRenderer.material;
            
            float dissolveTime = 0.4f;
            float elapsedTime = 0.0f;
            
            // Get the original color of the material
            Color originalColor = originalMaterial.color;
            var originalAlpha = originalMaterial.color.a;
            
            while (elapsedTime < dissolveTime)
            {
                // Calculate the new alpha value based on elapsed time
                float alpha = Mathf.Lerp(originalAlpha, 0.0f, elapsedTime / dissolveTime);
            
                // Set the material's color with the new alpha
                originalMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // Reset the material's alpha to 1.0f
            originalMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);
            
            // Hide the object by setting it inactive
            symbolGroupProperty.SetAllInactive();
            
            // Restore the original material or perform any final actions as needed
            symbolMeshRenderer.material = originalMaterial;
        }
    
        public IEnumerator SymbolCascadeAction(int fromSymbolIndex, int cascadeDistance, string cascadeSymbol)
        {
            // Not Supported Yet
            yield break;
            
        }
        
        public void SlideSymbol(int symbolIndex, float slideDistanceInSymbolUnits)
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var symbolGroupsDataTable = this.BettrReelMatrixSymbolGroupsData;
            var layoutPropertiesDataTable = this.BettrReelMatrixLayoutPropertiesData;
            
            // Get the locked state of the current symbol
            var isLocked = spinStateTable.GetProperty<bool>("IsLocked");
            // If the symbol is locked, return early
            if (isLocked)
            {
                return;
            }
            // Access the symbol property (assuming you have a way to reference symbols like this)
            var key = $"Row{RowIndex}Col{ColumnIndex}Symbol{symbolIndex}";
            var symbolProperty = (PropertyGameObject) this.TileTable[key];
            
            // Get the current local position of the symbol's game object
            Vector3 localPosition = symbolProperty.gameObject.transform.localPosition;
            
            // Get symbol's position and other relevant values from the reel state
            float symbolPosition = (float) symbolGroupsDataTable.GetProperty<double>("SymbolPosition", symbolIndex);
            float verticalSpacing = (float) layoutPropertiesDataTable.GetProperty<double>("SymbolVerticalSpacing");
            float symbolOffsetY = (float) layoutPropertiesDataTable.GetProperty<double>("SymbolOffsetY");
            
            // Calculate the new Y position
            float yLocalPosition = verticalSpacing * symbolPosition;
            yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;
            
            // Update the local position of the symbol
            localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            symbolProperty.gameObject.transform.localPosition = localPosition;
        }
        
        public void SpliceReel()
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var layoutPropertiesDataTable = this.BettrReelMatrixLayoutPropertiesData;
            var outcomesTable = this.BettrReelMatrixOutcomes;
            var reelStrip = this.BettrReelMatrixReelStrip;

            var outcomeReelStopIndex = outcomesTable.GetProperty<int>("OutcomeReelStopIndex");
            var spliceDistance = layoutPropertiesDataTable.GetProperty<int>("SpliceDistance");
            var reelSymbolCount = reelStrip.ReelSymbolCount;

            var reelSpinDirection = spinStateTable.GetProperty<string>("ReelSpinDirection");
            var spinDirectionIsDown = reelSpinDirection == "Down";
            
            // Check if splicing should be skipped
            bool skipSplice = SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown);
            
            // Perform splicing if it shouldn't be skipped
            if (spinDirectionIsDown)
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex + spliceDistance + reelSymbolCount) % reelSymbolCount;
                    spinStateTable.SetProperty<int>("ReelStopIndex", reelStopIndex - 1);
                }
            }
            else
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex - spliceDistance + reelSymbolCount) % reelSymbolCount;
                    spinStateTable.SetProperty<int>("ReelStopIndex", reelStopIndex + 1);
                }
            }
        }

        private TilePropertyGameObjectGroup GetSymbolGroupProperty(int symbolIndex)
        {
            var symbolGroupKey = $"Row{RowIndex}Col{ColumnIndex}SymbolGroup{symbolIndex}";
            var symbolGroupProperty = (TilePropertyGameObjectGroup) this.TileTable[symbolGroupKey];
            return symbolGroupProperty;
        }
        
        public bool SkipSpliceReel(int spliceDistance, int outcomeReelStopIndex, bool spinDirectionIsDown)
        {
            var spinStateTable = this.BettrReelMatrixSpinState;
            var layoutPropertiesDataTable = this.BettrReelMatrixLayoutPropertiesData;
            var outcomesTable = this.BettrReelMatrixOutcomes;
            var reelStrip = this.BettrReelMatrixReelStrip;
            
            var reelStopIndex = spinStateTable.GetProperty<int>("ReelStopIndex");
            
            var visibleSymbolCount = GetReelVisibleSymbolCount();
            
            // Check if the outcome reel stop index is within top or bottom splice bands
            bool inTopSpliceBand = outcomeReelStopIndex >= reelStopIndex - 1 - spliceDistance && outcomeReelStopIndex < reelStopIndex;
            bool inBottomSpliceBand = outcomeReelStopIndex >= reelStopIndex + visibleSymbolCount && 
                                        outcomeReelStopIndex < reelStopIndex + visibleSymbolCount + spliceDistance;
            
            if (spinDirectionIsDown)
            {
                // For spin down, skip if the outcome stop index is in the top symbol offset
                return inTopSpliceBand;
            }
            else
            {
                // For spin up, skip if the outcome stop index is in the bottom symbol offset
                return inBottomSpliceBand;
            }
        }
    }
}