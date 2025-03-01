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
        
        private string ExperimentVariantID { get; set; }
        private string MechanicName { get; set; }
        
        private BettrUserController BettrUserController { get; set; }
        private BettrMathController BettrMathController { get; set; }
        
        internal BettrReelMatrixCellController BettrReelMatrixCellController { get; private set; }
        
        internal Dictionary<string, BettrReelMatrixCellController> BettrReelMatrixCellControllers { get; private set; }
        
        public int[] RowCounts { get; private set; }
        public int ColumnCount { get; private set;  }
        
        public int OutcomeIndex { get; private set; }
        
        const string ReelID = "ReelID";

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
            this.ExperimentVariantID = Tile.GetProperty<string>("ExperimentVariantID");
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
        
        //
        // APIs
        // 

        public void ShowReelMatrix(MeshRenderer[] meshRenderers)
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                int rowCount = this.RowCounts[columnIndex - 1];
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex - 1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    int meshIndex = (columnIndex - 1) * rowCount + (rowCount - rowIndex);
                    var meshRenderer = meshRenderers[meshIndex];
                    var symbolTexture = meshRenderer.material.GetTexture("_MainTex");
                    bettrReelMatrixCellController.OverrideVisibleSymbolTexture(symbolTexture);
                }
            }

            ((PropertyGameObject) this.TileTable["Pivot"]).SetActive(true);
        }
        
        public void SetOutcomes(Table outcomesTable)
        {
            for (int i = 0; i < outcomesTable.Length; i++)
            {
                var outcomeRow = (Table) outcomesTable[i + 1];
                var cell = (string) outcomeRow[ReelID];
                var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[cell];
                var outcomeReelStopIndexesStr = (string) outcomeRow["OutcomeReelStopIndexes"];
                // parse the comma separated string
                // first remove the trailing , if any
                if (outcomeReelStopIndexesStr.EndsWith(","))
                {
                    outcomeReelStopIndexesStr = outcomeReelStopIndexesStr.Remove(outcomeReelStopIndexesStr.Length - 1);
                }
                var outcomeReelStopIndexes = outcomeReelStopIndexesStr.Split(',').Select(int.Parse).ToArray();
                // TODO: replace with initial reel stop index
                var initialReelStopIndex = 0;
                // remove the 1st entry in the outcomeReelStopIndexes
                outcomeReelStopIndexes = outcomeReelStopIndexes.Skip(1).ToArray();
                bettrReelMatrixCellController.SetReelOutcomes(outcomeReelStopIndexes);
                bettrReelMatrixCellController.BettrReelMatrixSpinState.SetProperty<int>("ReelStopIndex", initialReelStopIndex);
            }
        }

        /**
         * This sets the reel strip data on the ReelMatrix.
         * ReelMatrix will use a default reel strip if not set.
         */
        public void SetReelStripData(Table reelStripData)
        {
            // construct the reelStripData column from "ReelSymbol"
            var reelSymbols = new string[reelStripData.Length];
            for (var i = 0; i < reelStripData.Length; i++)
            {
                var row = (Table) reelStripData[i + 1];
                var reelSymbol = (string) row["ReelSymbol"];
                reelSymbols[i] = reelSymbol;
            }
            
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

        public void SetReelStripSymbolTextures(MeshRenderer[] meshRenderers)
        {
            foreach (var symbolTexture in meshRenderers)
            {
                var symbolName = symbolTexture.name;
                var texture = symbolTexture.material.GetTexture("_MainTex");
                SetReelStripSymbolTexture(symbolName, texture);
            }
        }

        public void SetReelStripSymbolTexture(string symbolName, Texture texture)
        {
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex - 1]; rowIndex++)
                {
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                    bettrReelMatrixCellController.SetReelStripSymbolTexture(symbolName, texture);
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
            OutcomeIndex = -1;
            yield break;
        }

        public IEnumerator SpinEngines()
        {
            int totalTasks = 0;
            int finishedTasks = 0;

            OutcomeIndex++;
    
            for (int columnIndex = 1; columnIndex <= ColumnCount; columnIndex++)
            {
                for (int rowIndex = 1; rowIndex <= this.RowCounts[columnIndex - 1]; rowIndex++)
                {
                    totalTasks++;
                    var key = $"Row{rowIndex}Col{columnIndex}";
                    var cellController = this.BettrReelMatrixCellControllers[key];
                    StartCoroutine(RunSpinEngine(cellController, () => finishedTasks++));
                }
            }
    
            while (finishedTasks < totalTasks)
            {
                yield return null;
            }
    
            // All coroutines finished
        }

        private IEnumerator RunSpinEngine(BettrReelMatrixCellController controller, Action onComplete)
        {
            yield return controller.SpinEngines();
            onComplete?.Invoke();
        }
        
        public void LockEngines(Table table)
        {
            // Loop over table and lock the engines
            for (int i = 0; i < table.Length; i++)
            {
                var row = (Table) table[i + 1];
                var key = (string) row[ReelID];
                var bettrReelMatrixCellController = this.BettrReelMatrixCellControllers[key];
                bettrReelMatrixCellController.LockEngine();
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
    public class BettrReelMatrixCellEngineLock
    {
        public bool IsLocked { get; private set; }
        
        public BettrReelMatrixCellEngineLock()
        {
            IsLocked = false;
        }
        
        public void Lock()
        {
            IsLocked = true;
        }
    }
    
    [Serializable]
    public class BettrReelMatrixCellOutcomeDelay
    {
        public bool IsStoppedForSpin { get; private set; }
        
        public float ReelStopDelayInMsForSpin { get; private set; }
        
        public float TimerEndTimeForSpin { get; private set; }
        
        public bool IsTimerStartedForSpin { get; private set; }
        
        public bool IsApplySpiceReel => !this.IsStoppedForSpin && this.IsTimerStartedForSpin && Time.time >= this.TimerEndTimeForSpin;
        
        public BettrReelMatrixCellOutcomeDelay(float delayInMs)
        {
            this.IsStoppedForSpin = false;
            this.TimerEndTimeForSpin = 0;
            this.IsTimerStartedForSpin = false;
            
            this.ReelStopDelayInMsForSpin = delayInMs * 10;
        }
        
        public void SetStopped(bool isStopped)
        {
            this.IsStoppedForSpin = isStopped;
        }

        public void StartTimer()
        {
            this.IsTimerStartedForSpin = true;
            this.TimerEndTimeForSpin += Time.time + this.ReelStopDelayInMsForSpin / 1000;
        }
        
        public void ExtendTimer(float durationInSeconds)
        {
            this.TimerEndTimeForSpin += durationInSeconds;
        }

        public void Reset()
        {
            this.IsStoppedForSpin = false;
            this.IsTimerStartedForSpin = false;
            this.TimerEndTimeForSpin = 0;
        }
    }

    [Serializable]
    public class BettrReelMatrixOutcome
    {
        public int OutcomeReelStopIndex { get; internal set; }
        
        public BettrReelMatrixOutcome(int outcomeReelStopIndex)
        {
            this.OutcomeReelStopIndex = outcomeReelStopIndex;
        }
    }
    
    [Serializable]
    public class BettrReelMatrixOutcomes
    {
        public List<BettrReelMatrixOutcome> Outcomes { get; internal set; }
        
        public int CurrentOutcomeIndex { get; private set; }
        
        public int OutcomeCount => Outcomes.Count;

        public int LastOutcomeReelStopIndex => Outcomes.Count > 0 ? Outcomes[Outcomes.Count - 1].OutcomeReelStopIndex : -1;
        
        public BettrReelMatrixOutcomes()
        {
            Outcomes = new List<BettrReelMatrixOutcome>();
            CurrentOutcomeIndex = -1;
        }
        
        public void AddOutcomes(int[] outcomeReelStopIndexes)
        {
            for (int i = 0; i < outcomeReelStopIndexes.Length; i++)
            {
                Outcomes.Add(new BettrReelMatrixOutcome(outcomeReelStopIndexes[i]));
            }
        }

        public int GetNextOutcome()
        {
            CurrentOutcomeIndex++;
            return Outcomes[CurrentOutcomeIndex].OutcomeReelStopIndex;
        }
        
        public int GetOutcome()
        {
            return Outcomes[CurrentOutcomeIndex].OutcomeReelStopIndex;
        }

        public bool HasNextOutcome()
        {
            var nextOutcomeIndex = CurrentOutcomeIndex + 1;
            return nextOutcomeIndex < Outcomes.Count;
        }
    }

    public class BettrReelStripSymbolTexture
    {
        public string SymbolName { get; internal set; }
        public Texture SymbolTexture { get; internal set; }
        
        public BettrReelStripSymbolTexture(string symbolName, Texture symbolTexture)
        {
            this.SymbolName = symbolName;
            this.SymbolTexture = symbolTexture;
        }
    }
    
    public class BettrReelStripSymbolTextures
    {
        public List<BettrReelStripSymbolTexture> SymbolTextures { get; internal set; }

        public BettrReelStripSymbolTextures()
        {
            SymbolTextures = new List<BettrReelStripSymbolTexture>();
        }
        
        public void AddSymbolTexture(string symbolName, Texture symbolTexture)
        {
            SymbolTextures.Add(new BettrReelStripSymbolTexture(symbolName, symbolTexture));
        }

        public void UpdateReelSymbolTexture(string symbolName, MeshRenderer meshRenderer)
        {
            // get the material
            var material = meshRenderer.material;
            // find the symbol texture
            var symbolTexture = SymbolTextures.Find(texture => texture.SymbolName == symbolName);
            if (symbolTexture != null)
            {
                material.SetTexture("_MainTex", symbolTexture.SymbolTexture); 
            }
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
        
        const string ReelID = "ReelID";
        
        const string Cell = "Cell";
        
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
            var row = MathController.GetTableRow(SpinStateTable, Cell, key);
            var propValue = row[propKey];
            if (propValue is T value) { return value; }
            // Handle special case: double to int conversion
            if (typeof(T) == typeof(int) && propValue is double d) { return (T)(object)Convert.ToInt32(d); }
            return (T)Convert.ChangeType(propValue, typeof(T));
        }
        
        public void SetProperty<T>(string propKey, T propValue)
        {
            var key = $"Row{RowIndex}Col{ColumnIndex}";
            var row = MathController.GetTableRow(SpinStateTable, Cell, key);
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
        public BettrReelMatrixOutcomes BettrReelMatrixOutcomes { get; internal set; }
        
        public BettrReelStripSymbolsForThisSpin BettrReelStripSymbolsForThisSpin { get; internal set; }
        
        public BettrReelStripSymbolTextures BettrSymbolTextures { get; internal set; }
        
        private bool ShouldSpliceReel { get; set; }
        
        private BettrUserController BettrUserController { get; set; }
        
        private BettrMathController BettrMathController { get; set; }
        
        private BettrReelMatrixCellDispatcher BettrReelMatrixCellDispatcher { get; set; }
        
        private  BettrReelMatrixCellOutcomeDelay BettrReelMatrixCellOutcomeDelay { get; set; }
        
        private BettrReelMatrixCellEngineLock BettrReelMatrixCellEngineLock { get; set; }
        
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
            
            this.BettrReelMatrixState = new BettrReelMatrixState(this, BettrMathController);
            this.BettrReelMatrixOutcomes = new BettrReelMatrixOutcomes();

            this.BettrReelStripSymbolsForThisSpin = new BettrReelStripSymbolsForThisSpin(this);

            this.BettrReelMatrixCellDispatcher = new BettrReelMatrixCellDispatcher(this);
            
            this.BettrSymbolTextures = new BettrReelStripSymbolTextures();
            
            var applyOutcomeDelayInMs = this.BettrReelMatrixLayoutPropertiesData.GetProperty<int>("ApplyOutcomeDelay");
            this.BettrReelMatrixCellOutcomeDelay = new BettrReelMatrixCellOutcomeDelay(applyOutcomeDelayInMs);
            
            this.BettrReelMatrixCellEngineLock = new BettrReelMatrixCellEngineLock();
        }

        public void SetReelStripSymbolTexture(string symbolName, Texture symbolTexture)
        {
            this.BettrSymbolTextures.AddSymbolTexture(symbolName, symbolTexture);
        }

        public void SetReelStrip(string[] reelSymbols)
        {
            this.BettrReelMatrixReelStrip = new BettrReelMatrixReelStrip(reelSymbols);
        }
        
        public void SetReelOutcomes(int[] outcomes)
        {
            this.BettrReelMatrixOutcomes.AddOutcomes(outcomes);
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
            
            this.BettrReelMatrixState = null;
            this.BettrReelMatrixOutcomes = null;
            
            this.BettrReelMatrixCellOutcomeDelay = null;
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
                var symbolGroupKey = $"Row{RowIndex}Col{ColumnIndex}SymbolGroup{symbolIndex}";
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.TileTable[symbolGroupKey];
                var currentValue = (PropertyGameObject) symbolGroupProperty[symbolGroupProperty.Keys[0]];
                currentValue.SetActive(true);
                symbolGroupProperty.Current = currentValue;
                symbolGroupProperty.CurrentIndex = 0;
                symbolGroupProperty.CurrentKey = symbolGroupProperty.Keys[0];
            }
            AdvanceSymbols();
            yield break;
        }

        public void LockEngine()
        {
            this.BettrReelMatrixCellEngineLock.Lock();
        }
        
        public IEnumerator OnApplyOutcomeReceived()
        {
            this.ShouldSpliceReel = true;
            this.BettrReelMatrixSpinState.SetProperty<bool>("OutcomeReceived", true);
            yield break;
        }
        
        public IEnumerator SpinEngines()
        {
            if (this.BettrReelMatrixCellEngineLock.IsLocked)
            {
                yield break;
            }
            
            ResetSpin();

            var nextOutcome = this.BettrReelMatrixOutcomes.GetNextOutcome();
                
            this.BettrReelMatrixSpinState.SetProperty<int>("ReelStopIndex", nextOutcome);
                
            yield return this.OnApplyOutcomeReceived();
                
            this.BettrReelMatrixSpinState.SetProperty<string>("ReelSpinState", "SpinStartedRollBack");

            while (true)
            {
                yield return null;
                var state = this.BettrReelMatrixSpinState.GetProperty<string>("ReelSpinState");
                if (state == "Stopped")
                {
                    break;
                }
            }
        }

        private void ResetSpin()
        {
            this.ShouldSpliceReel = false;
            this.BettrReelMatrixSpinState.SetProperty<bool>("ReachedOutcomeStopIndex", false);
            this.BettrReelMatrixSpinState.SetProperty<bool>("OutcomeReceived", false);

            this.BettrReelStripSymbolsForThisSpin.Reset();
            
            this.BettrReelMatrixCellOutcomeDelay.Reset();
        }
    
        public void SpinReelSpinStartedRollBack()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 2;
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
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 2;
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
            
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 2;
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
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 2;
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
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 2;
            
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

        public void OverrideVisibleSymbolTexture(Texture symbolTexture)
        {
            var symbolGroupProperty = GetSymbolGroupProperty(2);
            var fixedSymbol = symbolGroupProperty.Current;
            var go = fixedSymbol.GameObject;
            var meshRenderer = go.GetComponent<MeshRenderer>();
            var material = meshRenderer.material;
            material.SetTexture("_MainTex", symbolTexture); 
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

            var fixedSymbol = symbolGroupProperty.Current;
            
            var go = fixedSymbol.GameObject;
            var meshRenderer = go.GetComponent<MeshRenderer>();
            
            this.BettrSymbolTextures.UpdateReelSymbolTexture(reelSymbol, meshRenderer);
            
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
                var thisReelOutcomeDelay = BettrReelMatrixCellOutcomeDelay;
                if (!thisReelOutcomeDelay.IsTimerStartedForSpin)
                {
                    thisReelOutcomeDelay.StartTimer();
                }
                if (!thisReelOutcomeDelay.IsApplySpiceReel)
                {
                    return;
                }
                
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
            var outcomeReelStopIndex = this.BettrReelMatrixOutcomes.GetOutcome();
            // Uncomment below to Debug
            // outcomeReelStopIndex = this.BettrReelMatrixOutcomes.CurrentOutcomeIndex;
            
            // Check if the reel stop index matches the outcome stop index
            if (outcomeReelStopIndex == reelStopIndex)
            {
                spinState.SetProperty<string>("ReelSpinState", "ReachedOutcomeStopIndex");
                spinState.SetProperty<bool>("ReachedOutcomeStopIndex", true);
                BettrReelMatrixCellOutcomeDelay.SetStopped(true);
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

            var outcomeReelStopIndex = outcomesTable.LastOutcomeReelStopIndex;
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