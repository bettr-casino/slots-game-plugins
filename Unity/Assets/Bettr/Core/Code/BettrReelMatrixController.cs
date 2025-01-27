// ReSharper disable All
using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
namespace Bettr.Core
{
    [Serializable]
    public class BettrReelMatrixController : MonoBehaviour
    {
        [NonSerialized] private TileWithUpdate ReelMatrixTile;
        [NonSerialized] private GameObject ReelGo;
        
        [NonSerialized] private string ReelID;
        [NonSerialized] private int ReelIndex;
        [NonSerialized] private string MachineID;
        [NonSerialized] private string MachineVariantID;

        [NonSerialized] private BettrUserController BettrUserController;
        [NonSerialized] private BettrMathController BettrMathController;

        internal BettrReelMatrixCellController BettrReelMatrixCellController { get; private set; }

        private void Awake()
        {
            ReelMatrixTile = GetComponent<TileWithUpdate>();
            BettrUserController = BettrUserController.Instance;
            BettrMathController = BettrMathController.Instance;
        }
        
        private IEnumerator Start()
        {
            this.ReelGo = this.gameObject;
            this.ReelID = ReelMatrixTile.GetProperty<string>("ReelID");
            this.ReelIndex = ReelMatrixTile.GetProperty<int>("ReelIndex");
            this.MachineID = ReelMatrixTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelMatrixTile.GetProperty<string>("MachineVariantID");
            var reelTable = BettrMathController.GetGlobalTable(ReelMatrixTile.globalTileId);
            var reelStateTable = BettrMathController.GetTableFirst("BaseGameReelState", this.MachineID, this.ReelID);
            
            this.BettrReelMatrixCellController = new BettrReelMatrixCellController(ReelMatrixTile, BettrUserController, BettrMathController);
            
            yield break;
        }
        
    }

    public class BetteReelMatrixOutcomes
    {
        public int RowCount { get; internal set; }
        public int ColumnCount { get; internal set; }

        public BetteReelMatrixOutcomes(BettrReelMatrixCellController cellController)
        {
            this.RowCount = cellController.RowCount;
            this.ColumnCount = cellController.ColumnCount;
        }
    }

    public class BettrReelMatrixOutcome
    {
        
    }
    
    public class BettrReelMatrixReelSet
    {
        public int RowCount { get; internal set; }
        public int ColumnCount { get; internal set; }
        
        public Dictionary<string, BettrReelMatrixReelStrip> BettrReelMatrixReelStrip { get; internal set; }
        public BettrReelMatrixReelSet(BettrReelMatrixCellController cellController)
        {
            this.RowCount = cellController.RowCount;
            this.ColumnCount = cellController.ColumnCount;
            this.BettrReelMatrixReelStrip = new Dictionary<string, BettrReelMatrixReelStrip>();
            
            var machineID = cellController.MachineID;
            var tableName = "BaseGameReelMatrixDataMatrix";
            var reelsTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];

            for (var rowIndex = 1; rowIndex <= this.RowCount; rowIndex++)
            {
                for (var columnIndex = 1; columnIndex <= this.ColumnCount; columnIndex++)
                {
                    var reelStrip = new BettrReelMatrixReelStrip(reelsTable, rowIndex, columnIndex);
                    var key = reelStrip.CellId;
                    BettrReelMatrixReelStrip[key] = reelStrip;
                }
            }
        }
    }

    public class BettrReelMatrixReelStrip
    {
        public string CellId { get; internal set; }
        
        public Table ReelTable { get; internal set; }
        
        public BettrReelMatrixReelStrip(Table reelsTable, int rowIndex, int cellIndex)
        {
            var pk = $"Row{rowIndex}Cell{cellIndex}";
            var reelTable = (Table) reelsTable[pk];
            
            CellId = pk;
            ReelTable = reelTable;
        }
    }


    [Serializable]
    public class BettrReelMatrixCells
    {
        public int RowCount { get; internal set; }
        public int ColumnCount { get; internal set; }
        
        public TilePropertyGameObjectGroup Cells { get; internal set; }

        public BettrReelMatrixCells(BettrReelMatrixCellController cellController)
        {
            // RowCount
            this.RowCount = (int) (double) cellController.ReelMatrixDataSummaryTable["RowCount"];
            // ColumnCount
            this.ColumnCount = (int) (double) cellController.ReelMatrixDataSummaryTable["ColumnCount"];
            // Cells
            this.Cells = cellController.ReelMatrixTile.GetProperty<TilePropertyGameObjectGroup>("Cells");
        }
        
        public GameObject GetCellGameObject(int row, int column)
        {
            var key = $"Row{row}Col{column}";
            var property = Cells[key];
            var go = property.gameObject;
            return go;
        }
        
    }
    
    [Serializable]
    public class BettrReelMatrixCellController
    {
        public TileWithUpdate ReelMatrixTile { get; internal set; }
        public BettrReelMatrixCells Cells { get; internal set; }
    
        public string MachineID { get; internal set; }
        public string MachineVariantID { get; internal set; }
        public string MechanicName { get; internal set; }
        
        // Data Tables
        public Table ReelMatrixDataSummaryTable { get; internal set; }
        public Table ReelMatrixDataTable { get; internal set; }
        public Table ReelMatrixDataMatrixTable { get; internal set; }
        public Table ReelMatrixDataMatrix2Table { get; internal set; }
        public Table ReelMatrixDataMatrix3Table { get; internal set; }
        
        // State Tables
        public Table ReelMatrixStateTable { get; internal set; }
        public Table ReelMatrixSummaryTable { get; internal set; }
        public Table ReelMatrixTable { get; internal set; }
        public Table ReelMatrix2Table { get; internal set; }
        public Table ReelMatrix3Table { get; internal set; }
        public Table ReelMatrix4Table { get; internal set; }
        
        public Table SpinOutcomeTable { get; internal set; }
        
        public int RowCount { get; internal set; }
        public int ColumnCount { get; internal set; }
        
        public BettrReelMatrixCells BettrReelMatrixCells { get; internal set; }
        
        public BettrReelMatrixReelSet BettrReelMatrixReelSet { get; internal set; }
        
        // [NonSerialized] private float ReelOutcomeDelay { get; private set; }
        
        [NonSerialized] private bool ShouldSpliceReel;
        
        [NonSerialized] private BettrUserController BettrUserController;
        [NonSerialized] private BettrMathController BettrMathController;
        
        public BettrReelMatrixCellController(TileWithUpdate reelMatrixTile, BettrUserController userController, BettrMathController mathController)
        {
            ReelMatrixTile = reelMatrixTile;
            BettrUserController = userController;
            BettrMathController = mathController;
            
            this.MachineID = ReelMatrixTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelMatrixTile.GetProperty<string>("MachineVariantID");
            this.MechanicName = ReelMatrixTile.GetProperty<string>("MechanicName");
            
            // Data Tables
            
            this.ReelMatrixDataSummaryTable = BettrMathController.GetBaseGameMechanicDataSummary(this.MachineID, this.MechanicName);
            this.ReelMatrixDataTable = BettrMathController.GetBaseGameMechanicData(this.MachineID, this.MechanicName);
            this.ReelMatrixDataMatrixTable = BettrMathController.GetBaseGameMechanicDataMatrix(this.MachineID, this.MechanicName, "Symbols");
            this.ReelMatrixDataMatrix2Table = BettrMathController.GetBaseGameMechanicDataMatrix(2, this.MachineID, this.MechanicName, "SpinProperties");
            this.ReelMatrixDataMatrix3Table = BettrMathController.GetBaseGameMechanicDataMatrix(3, this.MachineID, this.MechanicName, "LayoutProperties");
            
            // State Tables
            this.ReelMatrixStateTable = BettrMathController.GetBaseGameMechanicSummary(this.MachineID, this.MechanicName);
            this.ReelMatrixSummaryTable = BettrMathController.GetBaseGameMechanic(this.MachineID, this.MechanicName);
            this.ReelMatrixTable = BettrMathController.GetBaseGameMechanic(1, this.MachineID, this.MechanicName, "State");
            this.ReelMatrix2Table = BettrMathController.GetBaseGameMechanic(2, this.MachineID, this.MechanicName, "ColumnState");
            this.ReelMatrix3Table = BettrMathController.GetBaseGameMechanic(3, this.MachineID, this.MechanicName, "SymbolsState");
            this.ReelMatrix4Table = BettrMathController.GetBaseGameMechanic(4, this.MachineID, this.MechanicName, "CellMask");
            
            this.BettrReelMatrixCells = new BettrReelMatrixCells(this);
            this.BettrReelMatrixReelSet = new BettrReelMatrixReelSet(this);
            
            this.ReelMatrixDataSummaryTable["BettrReelMatrixCellController"] = this;
        }
        
        public IEnumerator StartEngines()
        {
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // var reelSymbolsCount = this.ReelSymbolsStateTable.Length;
            // for (int i = 1; i <= reelSymbolsCount; i++)
            // {
            //     var symbolStateTable = (Table) this.ReelSymbolsStateTable[i];
            //     var reelPosition = (int) (double) symbolStateTable["ReelPosition"];
            //     int symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            //     var reelSymbol = (string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
            //     var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelMatrixDataSummaryTable[$"SymbolGroup{i}"];
            //     if (symbolGroupProperty.Current != null)
            //     {
            //         symbolGroupProperty.Current.SetActive(false);
            //         symbolGroupProperty.CurrentKey = null;
            //     }
            //     var currentValue = (PropertyGameObject) symbolGroupProperty[reelSymbol];
            //     currentValue.SetActive(true);
            //     symbolGroupProperty.Current = currentValue;
            //     symbolGroupProperty.CurrentKey = reelSymbol;
            // }
            yield break;
        }
    
        public IEnumerator OnOutcomeReceived()
        {
            // this.SpinOutcomeTable = BettrMathController.GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            // float delayInSeconds = (float) (double) this.ReelMatrixStateTable["ReelStopDelayInSeconds"];
            // ReelOutcomeDelays[this.ReelIndex].SetReelStopDelayInSeconds(delayInSeconds);
            yield break;
        }
    
        public IEnumerator OnApplyOutcomeReceived()
        {
            // this.ShouldSpliceReel = true;
            // this.ReelMatrixStateTable["OutcomeReceived"] = true;
            // // Debug.Log($"OnApplyOutcomeReceived ReelIndex: {this.ReelIndex} time: {Time.time}");
            yield break;
        }
        
        public void SpinEngines()
        {
            // this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollBack";
            // this.ReelMatrixStateTable["OutcomeReceived"] = false;
            // this.ShouldSpliceReel = false;
            //
            // this.SetupReelStripSymbolsForSpin();
            // ReelOutcomeDelays[this.ReelIndex].Reset();
        }
    
        public string ReplaceSymbolForSpin(int zeroIndex, string newSymbol)
        {
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // int oneIndexed = 1 +zeroIndex % reelSymbolCount;
            // var oldSymbol = this.ReelStripSymbolsForThisSpin[oneIndexed];
            // this.ReelStripSymbolsForThisSpin[oneIndexed] = newSymbol;
            // return oldSymbol;
            return "TODO";
        }
    
        public void SetupReelStripSymbolsForSpin()
        {
            // // create a copy of the ReelSymbolsTable
            // if (this.ReelStripSymbolsForThisSpin == null)
            // {
            //     this.ReelStripSymbolsForThisSpin = new List<string>();
            // }
            // this.ReelStripSymbolsForThisSpin.Clear();
            // this.ReelStripSymbolsForThisSpin.Add("Blank");
            //
            // var length = this.ReelSymbolsTable.Length;
            // for (int i = 0; i < length; i++)
            // {
            //     // 1 indexed
            //     this.ReelStripSymbolsForThisSpin.Add((string) ((Table) this.ReelSymbolsTable[i + 1])["ReelSymbol"]);
            // }
        }
        
        public void SpinReelSpinStartedRollBack()
        {
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            // var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            // var spinDirectionIsDown = reelSpinDirection == "Down";
            // var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelMatrixStateTable["SpinStartedRollBackDistanceInSymbolUnits"];
            // float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            // if (spinDirectionIsDown)
            // {
            //     if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
            //     {
            //         SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
            //         this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
            // else
            // {
            //     if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
            //     {
            //         SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
            //         this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
        }
        
        public void SpinReelSpinStartedRollForward()
        {
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinStartedRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            // var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            // var spinDirectionIsDown = reelSpinDirection == "Down";
            // var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            // if (spinDirectionIsDown)
            // {
            //     if (slideDistanceInSymbolUnits < 0)
            //     {
            //         SlideReelSymbols(0);
            //         this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
            // else
            // {
            //     if (slideDistanceInSymbolUnits > 0)
            //     {
            //         SlideReelSymbols(0);
            //         this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
        }
        
        public void SpinReelSpinEndingRollBack()
        {
            // // -- spin ending roll back animation
            // Tile.StartCoroutine(this.Tile.CallAction("PlaySpinReelSpinEndingRollBackAnimation"));
            //
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinEndingRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            // var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            // var spinDirectionIsDown = reelSpinDirection == "Down";
            // var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            // if (spinDirectionIsDown)
            // {
            //     if (slideDistanceInSymbolUnits > 0)
            //     {
            //         SlideReelSymbols(0);
            //         this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
            // else
            // {
            //     if (slideDistanceInSymbolUnits < 0)
            //     {
            //         SlideReelSymbols(0);
            //         this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
        }
        
        public void SpinReelSpinEndingRollForward()
        {
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinEndingRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            // var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            // var spinDirectionIsDown = reelSpinDirection == "Down";
            // var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelMatrixStateTable["SpinEndingRollForwardDistanceInSymbolUnits"];
            // var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            // if (spinDirectionIsDown)
            // {
            //     if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
            //     {
            //         SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
            //         this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
            // else
            // {
            //     if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
            //     {
            //         SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
            //         this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
            //     }
            //     else
            //     {
            //         SlideReelSymbols(slideDistanceInSymbolUnits);
            //     }
            // }
        }
        
        public bool SpinReelSpinning()
        {
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinSpeedInSymbolUnitsPerSecond"] * speed;
            // float slideDistanceInSymbolUnits = AdvanceReel();
            // SlideReelSymbols(slideDistanceInSymbolUnits);
            return true;
        }
        
        public float CalculateSlideDistanceInSymbolUnits()
        {
            // // Unity's Time.deltaTime provides the duration of the last frame in seconds
            // float frameDurationInSeconds = Time.deltaTime;
            // // Get the speed in symbol units per second from reelSpinState
            // float speedInSymbolUnits = (float) (double) this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"];
            // // Calculate distance traveled in this frame
            // float distanceInSymbolUnits = speedInSymbolUnits * frameDurationInSeconds;
            // // Check spin direction
            // bool spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";
            // // Get the current slide distance
            // float slideDistanceInSymbolUnits = (float) (double) this.ReelSpinStateTable["SlideDistanceInSymbolUnits"];
            // // Update the slide distance by adding the distance traveled
            // slideDistanceInSymbolUnits += distanceInSymbolUnits;
            // return slideDistanceInSymbolUnits;
            return 0.0f;
        }
        
        public float AdvanceReel()
        {
            // var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            // bool spinDirectionIsDown = reelSpinDirection == "Down";
            // float slideDistanceOffsetInSymbolUnits = spinDirectionIsDown ? 1 : -1;
            // float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            //
            // while ((spinDirectionIsDown && slideDistanceInSymbolUnits < -1) || (!spinDirectionIsDown && slideDistanceInSymbolUnits > 1))
            // {
            //     AdvanceSymbols();
            //     UpdateReelStopIndexes();
            //     ApplySpinReelStop();
            //     slideDistanceInSymbolUnits += slideDistanceOffsetInSymbolUnits;
            //     if (ReelOutcomeDelays[this.ReelIndex].IsStoppedForSpin)
            //     {
            //         break;
            //     }
            // }
            //
            // var spinState = (string) this.ReelSpinStateTable["ReelSpinState"];
            // if (spinState is "ReachedOutcomeStopIndex" or "Stopped")
            // {
            //     slideDistanceInSymbolUnits = 0;
            // }
            //
            // return slideDistanceInSymbolUnits;
            return 0.0f;
        }
    
        public void AdvanceSymbols()
        {
            // var symbolCount = (int) (double) this.ReelMatrixStateTable["SymbolCount"];
            // for (var i = 1; i <= symbolCount; i++)
            // {
            //     UpdateReelSymbolForSpin(i);
            // }
        }
        
        public int GetReelVisibleSymbolCount()
        {
            // return (int) (double) this.ReelMatrixStateTable["VisibleSymbolCount"];
            return 0;
        }
        
        public void UpdateReelSymbolForSpin(int symbolIndex)
        {
            // var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            // var symbolIsLocked = (bool) symbolState["SymbolIsLocked"];
            // if (symbolIsLocked)
            // {
            //     return;
            // }
            //
            // var rowVisible = (bool) symbolState["RowVisible"];
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // var reelPosition = (int) (double) symbolState["ReelPosition"];
            // var symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            // var reelSymbol = this.ReelStripSymbolsForThisSpin[symbolStopIndex]; //(string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
            // var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelMatrixDataSummaryTable[$"SymbolGroup{symbolIndex}"];
            // if (symbolGroupProperty.Current != null)
            // {
            //     symbolGroupProperty.Current.SetActive(false);
            //     symbolGroupProperty.CurrentKey = null;
            // }
            //
            // var currentValue = symbolGroupProperty[reelSymbol];
            // currentValue.SetActive(true);
            // symbolGroupProperty.Current = currentValue;
            // symbolGroupProperty.CurrentKey = reelSymbol;
        }
        
        public void UpdateReelStopIndexes()
        {
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // // Get the current stop index and advance offset
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            // var reelStopIndexAdvanceOffset = (int) (double) this.ReelMatrixStateTable["ReelStopIndexAdvanceOffset"];
            // // Update the reel stop index
            // reelStopIndex += reelStopIndexAdvanceOffset;
            // // Wrap the stop index to keep it within bounds using modulus
            // reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // // Assign the updated stop index back to the spin state
            // this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex;
        }
        
        public void ApplySpinReelStop()
        {
            // // Check if the outcome has been received
            // if (!(bool) this.ReelMatrixStateTable["OutcomeReceived"])
            // {
            //     return;
            // }
            // if (this.ShouldSpliceReel)
            // {
            //     // get the previousReelIndex
            //     var thisReelOutcomeDelay = ReelOutcomeDelays[this.ReelIndex];
            //     if (!thisReelOutcomeDelay.IsTimerStartedForSpin)
            //     {
            //         var previousReelOutcomeDelay = ReelOutcomeDelays[thisReelOutcomeDelay.PreviousReelIndex];
            //         if (previousReelOutcomeDelay.IsStoppedForSpin)
            //         {
            //             thisReelOutcomeDelay.StartTimer();
            //         }
            //     }
            //     if (!thisReelOutcomeDelay.IsApplySpiceReel)
            //     {
            //         return;
            //     }
            //     SpliceReel();
            //     this.ShouldSpliceReel = false;
            // }
            // // Get the current stop index and outcome-related values
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            // var reelStopIndexAdvanceOffset = (int) (double) this.ReelMatrixStateTable["ReelStopIndexAdvanceOffset"];
            // // Adjust the stop index
            // reelStopIndex -= reelStopIndexAdvanceOffset;
            // reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // // Get the outcome's stop index
            // var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            // // Check if the reel stop index matches the outcome stop index
            // if (outcomeReelStopIndex == reelStopIndex)
            // {
            //     this.ReelSpinStateTable["ReelSpinState"] = "ReachedOutcomeStopIndex";
            //     // let the ReelOutcomeDelay know that this reel has stopped
            //     ReelOutcomeDelays[this.ReelIndex].SetStopped(true);
            // }
        }
        
        public void SlideReelSymbols(float slideDistanceInSymbolUnits)
        {
            // // Get the symbol count from reelState
            // var symbolCount = (int) (double) this.ReelMatrixStateTable["SymbolCount"];
            // // Iterate through each symbol and apply the slide distance
            // for (int i = 1; i <= symbolCount; i++)
            // {
            //     SlideSymbol(i, slideDistanceInSymbolUnits);
            // }
            // // Set the SlideDistanceInSymbolUnits for the reel spin state
            // this.ReelSpinStateTable["SlideDistanceInSymbolUnits"] = slideDistanceInSymbolUnits;
        }
    
        public GameObject _FindSymbolQuad(TilePropertyGameObjectGroup symbolGroupProperty)
        {
            // var pivotGameObject = symbolGroupProperty.Current["Pivot"];
            // var childCount = pivotGameObject.transform.childCount;
            // for (int i = 0; i < childCount; i++)
            // {
            //     var child = pivotGameObject.transform.GetChild(i);
            //     if (child.name == "Quad")
            //     {
            //         return child.gameObject;
            //     }
            // }
            //
            // return null;
            return null; // PLACEHOLDER!
        }
        
        public IEnumerator SymbolRemovalAction(TilePropertyGameObjectGroup symbolGroupProperty)
        {
            // var symbolQuad = _FindSymbolQuad(symbolGroupProperty);
            // var symbolMeshRenderer = symbolQuad.GetComponent<MeshRenderer>();
            // var originalMaterial = symbolMeshRenderer.material;
            //
            // float dissolveTime = 0.4f;
            // float elapsedTime = 0.0f;
            //
            // // Get the original color of the material
            // Color originalColor = originalMaterial.color;
            // var originalAlpha = originalMaterial.color.a;
            //
            // while (elapsedTime < dissolveTime)
            // {
            //     // Calculate the new alpha value based on elapsed time
            //     float alpha = Mathf.Lerp(originalAlpha, 0.0f, elapsedTime / dissolveTime);
            //
            //     // Set the material's color with the new alpha
            //     originalMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
            //
            //     elapsedTime += Time.deltaTime;
            //     yield return null;
            // }
            //
            // // Reset the material's alpha to 1.0f
            // originalMaterial.color = new Color(originalColor.r, originalColor.g, originalColor.b, originalColor.a);
            //
            // // Hide the object by setting it inactive
            // symbolGroupProperty.SetAllInactive();
            //
            // // Restore the original material or perform any final actions as needed
            // symbolMeshRenderer.material = originalMaterial;
            
            yield break; // PLACEHOLDER!
        }
    
        public IEnumerator SymbolCascadeAction(int fromSymbolIndex, int cascadeDistance, string cascadeSymbol)
        {
            // var slideDistance = -cascadeDistance;
            // float duration = 0.4f;
            // float elapsedTime = 0f;
            //
            // float verticalSpacing = (float) (double) this.ReelMatrixStateTable["SymbolVerticalSpacing"];
            // float symbolOffsetY = (float) (double) this.ReelMatrixStateTable["SymbolOffsetY"];
            //
            // int toSymbolIndex = fromSymbolIndex + cascadeDistance;
            //
            // var fromSymbolState = (Table) this.ReelSymbolsStateTable[fromSymbolIndex];
            // var fromSymbolIsLocked = (bool) fromSymbolState["SymbolIsLocked"];
            // var fromSymbolProperty = (PropertyGameObject) this.ReelMatrixDataSummaryTable[$"Symbol{fromSymbolIndex}"];
            // float fromSymbolPosition = (float) (double) fromSymbolState["SymbolPosition"];
            // var fromSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelMatrixDataSummaryTable[$"SymbolGroup{fromSymbolIndex}"];
            // var fromSymbolLocalPosition = fromSymbolProperty.gameObject.transform.localPosition;
            // // set the fromSymbolGroupProperty.CurrentKey to cascadeSymbol
            // fromSymbolGroupProperty.SetCurrentActive(cascadeSymbol);
            //
            // var toSymbolState = (Table) this.ReelSymbolsStateTable[toSymbolIndex];
            // var toSymbolProperty = (PropertyGameObject) this.ReelMatrixDataSummaryTable[$"Symbol{toSymbolIndex}"];
            // float toSymbolPosition = (float) (double) toSymbolState["SymbolPosition"];
            // var toSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelMatrixDataSummaryTable[$"SymbolGroup{toSymbolIndex}"];
            //
            // if (fromSymbolIsLocked)
            // {
            //     yield break;
            // }
            //
            // float yLocalPosition = verticalSpacing * fromSymbolPosition;
            //
            // while (elapsedTime < duration)
            // {
            //     float slideDistanceInSymbolUnits = (slideDistance / duration) * Time.deltaTime;
            //     yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;
            //     Vector3 localPosition = fromSymbolProperty.gameObject.transform.localPosition;
            //     localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            //     fromSymbolProperty.gameObject.transform.localPosition = localPosition;
            //     elapsedTime += Time.deltaTime;
            //     yield return null;
            // }
            //
            // // swap symbols out
            // // from symbol property will be hidden and localPosition reset to the original localPosition
            // // to symbol property will be visible and its symbol set to the from symbol
            //
            // {
            //     // hide the fromSymbolGroupProperty.Current
            //     fromSymbolGroupProperty.Current.SetActive(false);
            //     // reset fromSymbolProperty localPosition
            //     fromSymbolProperty.gameObject.transform.localPosition = fromSymbolLocalPosition;
            //     
            //     toSymbolGroupProperty.SetCurrentActive(fromSymbolGroupProperty.CurrentKey);
            // }
            
            yield break; // PLACEHOLDER!
            
        }
        
        public void SlideSymbol(int symbolIndex, float slideDistanceInSymbolUnits)
        {
            // // Get the state of the current symbol
            // var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            // // If the symbol is locked, return early
            // if ((bool) symbolState["SymbolIsLocked"])
            // {
            //     return;
            // }
            // // Access the symbol property (assuming you have a way to reference symbols like this)
            // var symbolProperty = (PropertyGameObject) this.ReelMatrixDataSummaryTable[$"Symbol{symbolIndex}"];
            // // Get the current local position of the symbol's game object
            // Vector3 localPosition = symbolProperty.gameObject.transform.localPosition;
            // // Get symbol's position and other relevant values from the reel state
            // float symbolPosition = (float) (double) symbolState["SymbolPosition"];
            // float verticalSpacing = (float) (double) this.ReelMatrixStateTable["SymbolVerticalSpacing"];
            // float symbolOffsetY = (float) (double) this.ReelMatrixStateTable["SymbolOffsetY"];
            //
            // // Calculate the new Y position
            // float yLocalPosition = verticalSpacing * symbolPosition;
            // yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;
            //
            // // Update the local position of the symbol
            // localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            // symbolProperty.gameObject.transform.localPosition = localPosition;
        }
        
        public void SpliceReel()
        {
            // var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            // var spliceDistance = (int) (double) this.ReelMatrixStateTable["SpliceDistance"];
            // var reelSymbolCount = (int) (double) this.ReelMatrixStateTable["ReelSymbolCount"];
            // var spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";
            //
            // // Check if splicing should be skipped
            // bool skipSplice = SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown);
            //
            // // Perform splicing if it shouldn't be skipped
            // if (spinDirectionIsDown)
            // {
            //     if (!skipSplice)
            //     {
            //         int reelStopIndex = (outcomeReelStopIndex + spliceDistance + reelSymbolCount) % reelSymbolCount;
            //         this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex - 1;
            //     }
            // }
            // else
            // {
            //     if (!skipSplice)
            //     {
            //         int reelStopIndex = (outcomeReelStopIndex - spliceDistance + reelSymbolCount) % reelSymbolCount;
            //         this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex + 1;
            //     }
            // }
        }
        
        public bool SkipSpliceReel(int spliceDistance, int outcomeReelStopIndex, bool spinDirectionIsDown)
        {
            // var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            //
            // var visibleSymbolCount = (int) (double) this.ReelMatrixStateTable["VisibleSymbolCount"];
            //
            // // Check if the outcome reel stop index is within top or bottom splice bands
            // bool inTopSpliceBand = outcomeReelStopIndex >= reelStopIndex - 1 - spliceDistance && outcomeReelStopIndex < reelStopIndex;
            // bool inBottomSpliceBand = outcomeReelStopIndex >= reelStopIndex + visibleSymbolCount && 
            //                             outcomeReelStopIndex < reelStopIndex + visibleSymbolCount + spliceDistance;
            //
            // if (spinDirectionIsDown)
            // {
            //     // For spin down, skip if the outcome stop index is in the top symbol offset
            //     return inTopSpliceBand;
            // }
            // else
            // {
            //     // For spin up, skip if the outcome stop index is in the bottom symbol offset
            //     return inBottomSpliceBand;
            // }
            
            return false; // PLACEHOLDER!
        }
    
        public void StartReelAnticipation(float anticipationSpeed, float anticipationDuration)
        {
            // Debug.Log($"StartReelAnticipation reelID={this.ReelID} reelIndex={this.ReelIndex} anticipationSpeed={anticipationSpeed} anticipationDuration={anticipationDuration}");
            // var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            // this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelMatrixStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            // ReelOutcomeDelays[this.ReelIndex].ExtendTimer(anticipationDuration);
        }
    
        public void SwapInReelStripSymbolsForSpin(params List<string>[] reelStripSymbolsForSpinList)
        {
            
        }
        
        public void UndoSwapInReelStripSymbolsForSpin()
        {
        }
        
        public void SwapInReelSpinOutcomeTableForSpin(params Table[] spinOutcomeTables)
        {
        }
        
        public void UndoSwapInReelSpinOutcomeTableForSpin()
        {
        }
        
    }
}