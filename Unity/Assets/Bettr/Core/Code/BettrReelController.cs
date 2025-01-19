// ReSharper disable All
using System;
using System.Collections;
using System.Collections.Generic;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
namespace Bettr.Core
{
    public class ReelStripOutcomeDelay
    {
        public int ReelIndex { get; private set; }
        public int PreviousReelIndex { get; private set; }
        public bool IsStoppedForSpin { get; private set; }
        
        public float ReelStopDelayInSecondsForSpin { get; private set; }
        
        public float TimerEndTimeForSpin { get; private set; }
        
        public bool IsTimerStartedForSpin { get; private set; }
        
        public bool IsApplySpiceReel => !this.IsStoppedForSpin && this.IsTimerStartedForSpin && Time.time >= this.TimerEndTimeForSpin;
        
        public ReelStripOutcomeDelay(int thisReelIndex, int totalSize)
        {
            this.ReelIndex = Mod(thisReelIndex, totalSize);
            this.PreviousReelIndex = Mod(this.ReelIndex - 1, totalSize);
            this.IsStoppedForSpin = false;
            this.ReelStopDelayInSecondsForSpin = 0;
            this.TimerEndTimeForSpin = 0;
            this.IsTimerStartedForSpin = false;
        }
        
        private int Mod(int a, int b) {
            return ((a % b) + b) % b;
        }
        
        public void SetStopped(bool isStopped)
        {
            this.IsStoppedForSpin = isStopped;
        }

        public void StartTimer()
        {
            this.IsTimerStartedForSpin = true;
            this.TimerEndTimeForSpin += Time.time + this.ReelStopDelayInSecondsForSpin;
            
            // Debug.Log($"ReelIndex: {this.ReelIndex} Time.time={Time.time} TimerEndTimeForSpin: {this.TimerEndTimeForSpin}");
        }
        
        public void ExtendTimer(float durationInSeconds)
        {
            this.TimerEndTimeForSpin += durationInSeconds;
        }

        public void SetReelStopDelayInSeconds(float delayInSeconds)
        {
            this.ReelStopDelayInSecondsForSpin = delayInSeconds;
        }
        
        public void Reset()
        {
            this.IsStoppedForSpin = false;
            this.IsTimerStartedForSpin = false;
            this.TimerEndTimeForSpin = 0;
            this.ReelStopDelayInSecondsForSpin = 0;
        }
    }
    
    public class ReelStripSwapInSymbolsCommand
    {
        private List<string> reelStripSymbolsForSpin;
        private bool _undoCompleted;

        public ReelStripSwapInSymbolsCommand(List<string> reelStripSymbolsForSpin)
        {
            this.reelStripSymbolsForSpin = reelStripSymbolsForSpin;
            this._undoCompleted = false;
        }

        public void Undo(BettrReelStripController reelController)
        {
            if (!_undoCompleted)
            {
                reelController.ReelStripSymbolsForThisSpin = reelStripSymbolsForSpin;
                _undoCompleted = true;
            }
        }
    }
    
    public class ReelStripSwapInSpinOutcomeTableCommand
    {
        private Table _spinOutcomeTable;
        private bool _undoCompleted;

        public ReelStripSwapInSpinOutcomeTableCommand(Table spinOutcomeTable)
        {
            this._spinOutcomeTable = spinOutcomeTable;
            this._undoCompleted = false;
        }

        public void Undo(BettrReelStripController reelController)
        {
            if (!_undoCompleted)
            {
                reelController.SpinOutcomeTable = _spinOutcomeTable;
                _undoCompleted = true;
            }
        }
    }
    
    
    [Serializable]
    public class BettrReelController : MonoBehaviour
    {
        [NonSerialized] private TileWithUpdate ReelTile;
        [NonSerialized] private GameObject ReelGo;
        
        [NonSerialized] private string ReelID;
        [NonSerialized] private int ReelIndex;
        [NonSerialized] private string MachineID;
        [NonSerialized] private string MachineVariantID;

        [NonSerialized] internal bool IsReelMatrixEnabled;
        
        [NonSerialized] private BettrUserController BettrUserController;
        [NonSerialized] private BettrMathController BettrMathController;
        
        public BettrReelMatrixController BettrReelMatrixController { get; private set; }
        public BettrReelStripController BettrReelStripController { get; private set; }

        private void Awake()
        {
            ReelTile = GetComponent<TileWithUpdate>();
            BettrUserController = BettrUserController.Instance;
            BettrMathController = BettrMathController.Instance;
            
            IsReelMatrixEnabled = false;

            IsReelMatrixEnabled = true;
        }
        
        private IEnumerator Start()
        {
            this.ReelGo = this.gameObject;
            this.ReelID = ReelTile.GetProperty<string>("ReelID");
            this.ReelIndex = ReelTile.GetProperty<int>("ReelIndex");
            this.MachineID = ReelTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelTile.GetProperty<string>("MachineVariantID");
            var reelTable = BettrMathController.GetGlobalTable(ReelTile.globalTileId);
            var reelStateTable = BettrMathController.GetTableFirst("BaseGameReelState", this.MachineID, this.ReelID);

            reelTable["BettrReelController"] = this;
            
            this.BettrReelStripController = new BettrReelStripController(ReelTile, BettrUserController, BettrMathController);
            reelTable["BettrReelStripController"] = this;
            
            this.BettrReelMatrixController = new BettrReelMatrixController(ReelTile, BettrUserController, BettrMathController);
            
            yield break;
        }
        
        public IEnumerator StartEngines()
        {
            if (IsReelMatrixEnabled)
            {
                yield return this.BettrReelMatrixController.StartEngines();
            }
            else
            {
                yield return this.BettrReelStripController.StartEngines();
            }
        }

        public IEnumerator OnOutcomeReceived()
        {
            if (IsReelMatrixEnabled)
            {
                yield return this.BettrReelMatrixController.OnOutcomeReceived();
            }
            else
            {
                yield return this.BettrReelStripController.OnOutcomeReceived();
            }
        }

        public IEnumerator OnApplyOutcomeReceived()
        {
            if (IsReelMatrixEnabled)
            {
                yield return this.BettrReelMatrixController.OnApplyOutcomeReceived();
            }
            else
            {
                yield return this.BettrReelStripController.OnApplyOutcomeReceived();
            }
        }
        
        public void SpinEngines()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SpinEngines();
            }
            else
            {
                this.BettrReelStripController.SpinEngines();
            }
        }

        public string ReplaceSymbolForSpin(int zeroIndex, string newSymbol)
        {
            if (IsReelMatrixEnabled)
            {
                return this.BettrReelMatrixController.ReplaceSymbolForSpin(zeroIndex, newSymbol);
            }
            else
            {
                return this.BettrReelStripController.ReplaceSymbolForSpin(zeroIndex, newSymbol);
            }
        }

        public void SwapInReelStripSymbolsForSpin(params List<string>[] reelStripSymbolsForSpins)
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelStripController.SwapInReelStripSymbolsForSpin(reelStripSymbolsForSpins);
            }
            else
            {
                this.BettrReelStripController.SwapInReelStripSymbolsForSpin(reelStripSymbolsForSpins);
            }
        }
        
        public void UndoSwapInReelStripSymbolsForSpin()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.UndoSwapInReelStripSymbolsForSpin();
            }
            else
            {
                this.BettrReelStripController.UndoSwapInReelStripSymbolsForSpin();
            }
        }
        
        public void SwapInReelSpinOutcomeTableForSpin(params Table[] spinOutcomeTables)
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SwapInReelSpinOutcomeTableForSpin(spinOutcomeTables);
            }
            else
            {
                this.BettrReelStripController.SwapInReelSpinOutcomeTableForSpin(spinOutcomeTables);
            }
        }
        
        public void UndoSwapInReelSpinOutcomeTableForSpin()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.UndoSwapInReelSpinOutcomeTableForSpin();
            }
            else
            {
                this.BettrReelStripController.UndoSwapInReelSpinOutcomeTableForSpin();
            }
        }
        
        public void SwitchToReelMatrix()
        {
            this.IsReelMatrixEnabled = true;
        }
        
        public void SwitchToReelStrip()
        {
            this.IsReelMatrixEnabled = false;
        }
        
        public void SetupReelStripSymbolsForSpin()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SetupReelStripSymbolsForSpin();
            }
            else
            {
                this.BettrReelStripController.SetupReelStripSymbolsForSpin();
            }
        }
        
        public void SpinReelSpinStartedRollBack()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SpinReelSpinStartedRollBack();
            }
            else
            {
                this.BettrReelStripController.SpinReelSpinStartedRollBack();
            }
        }
        
        public void SpinReelSpinStartedRollForward()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SpinReelSpinStartedRollForward();                
            }
            else
            {
                this.BettrReelStripController.SpinReelSpinStartedRollForward();
            }
        }
        
        public void SpinReelSpinEndingRollBack()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SpinReelSpinEndingRollBack();
            }
            else
            {
                this.BettrReelStripController.SpinReelSpinEndingRollBack();
            }
        }
        
        public void SpinReelSpinEndingRollForward()
        {
            if (IsReelMatrixEnabled)
            {
                this.BettrReelMatrixController.SpinReelSpinEndingRollForward();
            }
            else
            {
                this.BettrReelStripController.SpinReelSpinEndingRollForward();
            }
        }
        
        public bool SpinReelSpinning()
        {
            if (IsReelMatrixEnabled)
            {
                return this.BettrReelMatrixController.SpinReelSpinning();
            }
            else
            {
                return this.BettrReelStripController.SpinReelSpinning();
            }
        }
        
        private float CalculateSlideDistanceInSymbolUnits()
        {
            if (IsReelMatrixEnabled)
            {
                return this.BettrReelMatrixController.CalculateSlideDistanceInSymbolUnits();
            }
            else
            {
                return this.BettrReelStripController.CalculateSlideDistanceInSymbolUnits();
            }
        }
    }
    
    [Serializable]
    public class BettrReelStripController
    {
        [NonSerialized] private TileWithUpdate ReelTile;
        [NonSerialized] private GameObject ReelGo;

        [NonSerialized] private string ReelID;
        [NonSerialized] private int ReelIndex;
        [NonSerialized] private string MachineID;
        [NonSerialized] private string MachineVariantID;
        
        [NonSerialized] private ReelStripSwapInSymbolsCommand _reelStripSwapInSymbolsCommand;
        [NonSerialized] private ReelStripSwapInSpinOutcomeTableCommand _reelStripSwapInSpinOutcomeTableCommand;
        
        public Table ReelTable { get; private set; }
        public Table ReelStateTable { get; private set; }
        public Table ReelSpinStateTable { get; private set; }
        public Table SpinOutcomeTable { get; internal set; }
        public Table ReelSymbolsStateTable { get; private set; }
        public Table ReelSymbolsTable { get; private set; }
        
        // TODO: FIXME move this to ReelStateTable
        [NonSerialized] private bool ShouldSpliceReel;
        
        [NonSerialized] internal bool ShouldSwitchToReelMatrix;
        
        [NonSerialized] private BettrUserController BettrUserController;
        [NonSerialized] private BettrMathController BettrMathController;
        
        public List<string> ReelStripSymbolsForThisSpin { get; internal set; }
        
        public static ReelStripOutcomeDelay[] ReelOutcomeDelays { get; private set; }
        
        public BettrReelStripController(TileWithUpdate reelTile, BettrUserController userController, BettrMathController mathController)
        {
            ReelTile = reelTile;
            BettrUserController = userController;
            BettrMathController = mathController;
            
            var totalSize = 32;
            ReelOutcomeDelays = new ReelStripOutcomeDelay[totalSize];
            for (var i = 0; i < totalSize; i++)
            {
                ReelOutcomeDelays[i] = new ReelStripOutcomeDelay(i, totalSize);
            }
            // special case for handling ReelIndex=0
            ReelOutcomeDelays[totalSize-1].SetStopped(true);
            
            this.ReelGo = this.ReelTile.gameObject;
            
            this.ReelID = ReelTile.GetProperty<string>("ReelID");
            this.ReelIndex = ReelTile.GetProperty<int>("ReelIndex");
            this.MachineID = ReelTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelTile.GetProperty<string>("MachineVariantID");

            this.ReelTable = BettrMathController.GetGlobalTable(ReelTile.globalTileId);
            this.ReelStateTable = BettrMathController.GetTableFirst("BaseGameReelState", this.MachineID, this.ReelID);
            this.ReelSpinStateTable = BettrMathController.GetTableFirst("BaseGameReelSpinState", this.MachineID, this.ReelID);
            this.SpinOutcomeTable = BettrMathController.GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            this.ReelSymbolsStateTable = BettrMathController.GetTableArray("BaseGameReelSymbolsState", this.MachineID, this.ReelID);
            this.ReelSymbolsTable = BettrMathController.GetTableArray("BaseGameReelSet", this.MachineID, this.ReelID);            
        }
        
        public IEnumerator StartEngines()
        {
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelSymbolsCount = this.ReelSymbolsStateTable.Length;
            for (int i = 1; i <= reelSymbolsCount; i++)
            {
                var symbolStateTable = (Table) this.ReelSymbolsStateTable[i];
                var reelPosition = (int) (double) symbolStateTable["ReelPosition"];
                int symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
                var reelSymbol = (string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{i}"];
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
            this.SpinOutcomeTable = BettrMathController.GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            float delayInSeconds = (float) (double) this.ReelStateTable["ReelStopDelayInSeconds"];
            ReelOutcomeDelays[this.ReelIndex].SetReelStopDelayInSeconds(delayInSeconds);
            yield break;
        }

        public IEnumerator OnApplyOutcomeReceived()
        {
            this.ShouldSpliceReel = true;
            this.ReelStateTable["OutcomeReceived"] = true;
            // Debug.Log($"OnApplyOutcomeReceived ReelIndex: {this.ReelIndex} time: {Time.time}");
            yield break;
        }
        
        public void SpinEngines()
        {
            this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollBack";
            this.ReelStateTable["OutcomeReceived"] = false;
            this.ShouldSpliceReel = false;
            
            // undo any swap in reel strip symbols or spin outcome table
            if (this._reelStripSwapInSymbolsCommand != null)
            {
                this._reelStripSwapInSymbolsCommand.Undo(this);
            }
            if (this._reelStripSwapInSpinOutcomeTableCommand != null)
            {
                this._reelStripSwapInSpinOutcomeTableCommand.Undo(this);
            }

            this.SetupReelStripSymbolsForSpin();
            ReelOutcomeDelays[this.ReelIndex].Reset();
        }

        public string ReplaceSymbolForSpin(int zeroIndex, string newSymbol)
        {
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            int oneIndexed = 1 +zeroIndex % reelSymbolCount;
            var oldSymbol = this.ReelStripSymbolsForThisSpin[oneIndexed];
            this.ReelStripSymbolsForThisSpin[oneIndexed] = newSymbol;
            return oldSymbol;
        }

        public void SwapInReelStripSymbolsForSpin(params List<string>[] reelStripSymbolsForSpin)
        {
            this._reelStripSwapInSymbolsCommand = new ReelStripSwapInSymbolsCommand(this.ReelStripSymbolsForThisSpin);
            this.ReelStripSymbolsForThisSpin = reelStripSymbolsForSpin[0];
        }
        
        public void UndoSwapInReelStripSymbolsForSpin()
        {
            if (this._reelStripSwapInSymbolsCommand != null)
            {
                this._reelStripSwapInSymbolsCommand.Undo(this);
            }
        }
        
        public void SwapInReelSpinOutcomeTableForSpin(params Table[] spinOutcomeTables)
        {
            this._reelStripSwapInSpinOutcomeTableCommand = new ReelStripSwapInSpinOutcomeTableCommand(this.SpinOutcomeTable);
            this.SpinOutcomeTable = spinOutcomeTables[0];
        }
        
        public void UndoSwapInReelSpinOutcomeTableForSpin()
        {
            if (this._reelStripSwapInSpinOutcomeTableCommand != null)
            {
                this._reelStripSwapInSpinOutcomeTableCommand.Undo(this);
            }
        }
        
        public void SetupReelStripSymbolsForSpin()
        {
            // create a copy of the ReelSymbolsTable
            if (this.ReelStripSymbolsForThisSpin == null)
            {
                this.ReelStripSymbolsForThisSpin = new List<string>();
            }
            this.ReelStripSymbolsForThisSpin.Clear();
            this.ReelStripSymbolsForThisSpin.Add("Blank");

            var length = this.ReelSymbolsTable.Length;
            for (int i = 0; i < length; i++)
            {
                // 1 indexed
                this.ReelStripSymbolsForThisSpin.Add((string) ((Table) this.ReelSymbolsTable[i + 1])["ReelSymbol"]);
            }
        }
        
        public void SpinReelSpinStartedRollBack()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelStateTable["SpinStartedRollBackDistanceInSymbolUnits"];
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
                {
                    _SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
                {
                    _SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinStartedRollForward()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < 0)
                {
                    _SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits > 0)
                {
                    _SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinEndingRollBack()
        {
            // -- spin ending roll back animation
            ReelTile.StartCoroutine(this.ReelTile.CallAction("PlaySpinReelSpinEndingRollBackAnimation"));
            
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinEndingRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > 0)
                {
                    _SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits < 0)
                {
                    _SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public void SpinReelSpinEndingRollForward()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinEndingRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelStateTable["SpinEndingRollForwardDistanceInSymbolUnits"];
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
                {
                    _SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
            else
            {
                if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
                {
                    _SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
                }
                else
                {
                    _SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public bool SpinReelSpinning()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinSpeedInSymbolUnitsPerSecond"] * speed;
            float slideDistanceInSymbolUnits = _AdvanceReel();
            _SlideReelSymbols(slideDistanceInSymbolUnits);
            return true;
        }
        
        public float CalculateSlideDistanceInSymbolUnits()
        {
            // Unity's Time.deltaTime provides the duration of the last frame in seconds
            float frameDurationInSeconds = Time.deltaTime;
            // Get the speed in symbol units per second from reelSpinState
            float speedInSymbolUnits = (float) (double) this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"];
            // Calculate distance traveled in this frame
            float distanceInSymbolUnits = speedInSymbolUnits * frameDurationInSeconds;
            // Check spin direction
            bool spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";
            // Get the current slide distance
            float slideDistanceInSymbolUnits = (float) (double) this.ReelSpinStateTable["SlideDistanceInSymbolUnits"];
            // Update the slide distance by adding the distance traveled
            slideDistanceInSymbolUnits += distanceInSymbolUnits;
            return slideDistanceInSymbolUnits;
        }
        
        private float _AdvanceReel()
        {
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            bool spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceOffsetInSymbolUnits = spinDirectionIsDown ? 1 : -1;
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();

            while ((spinDirectionIsDown && slideDistanceInSymbolUnits < -1) || (!spinDirectionIsDown && slideDistanceInSymbolUnits > 1))
            {
                _AdvanceSymbols();
                _UpdateReelStopIndexes();
                _ApplySpinReelStop();
                slideDistanceInSymbolUnits += slideDistanceOffsetInSymbolUnits;
                if (ReelOutcomeDelays[this.ReelIndex].IsStoppedForSpin)
                {
                    break;
                }
            }

            var spinState = (string) this.ReelSpinStateTable["ReelSpinState"];
            if (spinState is "ReachedOutcomeStopIndex" or "Stopped")
            {
                slideDistanceInSymbolUnits = 0;
            }

            return slideDistanceInSymbolUnits;
        }

        private void _AdvanceSymbols()
        {
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            for (var i = 1; i <= symbolCount; i++)
            {
                _UpdateReelSymbolForSpin(i);
            }
        }
        
        public int GetReelVisibleSymbolCount()
        {
            return (int) (double) this.ReelStateTable["VisibleSymbolCount"];
        }
        
        private int _GetReelTopSymbolCount()
        {
            return (int) (double) this.ReelStateTable["TopSymbolCount"];
        }
        
        public List<TilePropertyGameObjectGroup> GetReelMatrixVisibleSymbolsGroups(params string[] symbols)
        {
            var reelMatrixSymbolGroups = new List<TilePropertyGameObjectGroup>();
            var topSymbolCount = (int) (double) this.ReelStateTable["TopSymbolCount"];
            var bottomSymbolCount = (int) (double) this.ReelStateTable["BottomSymbolCount"];
            var visibleSymbolCount = (int) (double) this.ReelStateTable["VisibleSymbolCount"];
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            var startSymbolIndexOneIndexed = 1 + topSymbolCount;
            var endSymbolIndexOneIndexed = symbolCount - bottomSymbolCount;
            for (var symbolIndex = startSymbolIndexOneIndexed; symbolIndex <= endSymbolIndexOneIndexed; symbolIndex++)
            {
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{symbolIndex}"];
                reelMatrixSymbolGroups.Add(symbolGroupProperty);
            }
            return reelMatrixSymbolGroups;
        }

        public List<TilePropertyGameObject> GetReelMatrixVisibleSymbols(params string[] symbols)
        {
            var reelMatrixSymbols = new List<TilePropertyGameObject>();
            var reelMatrixSymbolGroups = GetReelMatrixVisibleSymbolsGroups(symbols);
            foreach (var symbolGroupProperty in reelMatrixSymbolGroups)
            {
                var currentKey = symbolGroupProperty.CurrentKey;
                // find the corresponding TilePropertyGameObject in the symbolGroupProperty.GameObjectProperties
                var symbolProperty = symbolGroupProperty.gameObjectProperties.Find(x => x.key == currentKey);
                if (symbolProperty != null)
                {
                    // if symbols is not null, then the symbol should be in the symbols array to be added 
                    // to the reelMatrixSymbols list else add it by default
                    if (symbols != null && symbols.Length > 0)
                    {
                        if (Array.Exists(symbols, symbol => symbol == currentKey))
                        {
                            reelMatrixSymbols.Add(symbolProperty);
                        }
                    }
                    else
                    {
                        reelMatrixSymbols.Add(symbolProperty);
                    }
                }
            }
            return reelMatrixSymbols;
        }

        private void _UpdateReelSymbolForSpin(int symbolIndex)
        {
            var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            var symbolIsLocked = (bool) symbolState["SymbolIsLocked"];
            if (symbolIsLocked)
            {
                return;
            }

            var rowVisible = (bool) symbolState["RowVisible"];
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelPosition = (int) (double) symbolState["ReelPosition"];
            var symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            var reelSymbol = this.ReelStripSymbolsForThisSpin[symbolStopIndex]; //(string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
            var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{symbolIndex}"];
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
        
        private void _UpdateReelStopIndexes()
        {
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            // Get the current stop index and advance offset
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) this.ReelStateTable["ReelStopIndexAdvanceOffset"];
            // Update the reel stop index
            reelStopIndex += reelStopIndexAdvanceOffset;
            // Wrap the stop index to keep it within bounds using modulus
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Assign the updated stop index back to the spin state
            this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex;
        }
        
        private void _ApplySpinReelStop()
        {
            // Check if the outcome has been received
            if (!(bool) this.ReelStateTable["OutcomeReceived"])
            {
                return;
            }
            if (this.ShouldSpliceReel)
            {
                // get the previousReelIndex
                var thisReelOutcomeDelay = ReelOutcomeDelays[this.ReelIndex];
                if (!thisReelOutcomeDelay.IsTimerStartedForSpin)
                {
                    var previousReelOutcomeDelay = ReelOutcomeDelays[thisReelOutcomeDelay.PreviousReelIndex];
                    if (previousReelOutcomeDelay.IsStoppedForSpin)
                    {
                        thisReelOutcomeDelay.StartTimer();
                    }
                }
                if (!thisReelOutcomeDelay.IsApplySpiceReel)
                {
                    return;
                }
                _SpliceReel();
                this.ShouldSpliceReel = false;
            }
            // Get the current stop index and outcome-related values
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) this.ReelStateTable["ReelStopIndexAdvanceOffset"];
            // Adjust the stop index
            reelStopIndex -= reelStopIndexAdvanceOffset;
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Get the outcome's stop index
            var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            // Check if the reel stop index matches the outcome stop index
            if (outcomeReelStopIndex == reelStopIndex)
            {
                this.ReelSpinStateTable["ReelSpinState"] = "ReachedOutcomeStopIndex";
                // let the ReelOutcomeDelay know that this reel has stopped
                ReelOutcomeDelays[this.ReelIndex].SetStopped(true);
            }
        }
        
        private void _SlideReelSymbols(float slideDistanceInSymbolUnits)
        {
            // Get the symbol count from reelState
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            // Iterate through each symbol and apply the slide distance
            for (int i = 1; i <= symbolCount; i++)
            {
                _SlideSymbol(i, slideDistanceInSymbolUnits);
            }
            // Set the SlideDistanceInSymbolUnits for the reel spin state
            this.ReelSpinStateTable["SlideDistanceInSymbolUnits"] = slideDistanceInSymbolUnits;
        }

        private GameObject _FindSymbolQuad(TilePropertyGameObjectGroup symbolGroupProperty)
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
        
        private IEnumerator _SymbolRemovalAction(TilePropertyGameObjectGroup symbolGroupProperty)
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

        private IEnumerator _SymbolCascadeAction(int fromSymbolIndex, int cascadeDistance, string cascadeSymbol)
        {
            var slideDistance = -cascadeDistance;
            float duration = 0.4f;
            float elapsedTime = 0f;
            
            float verticalSpacing = (float) (double) this.ReelStateTable["SymbolVerticalSpacing"];
            float symbolOffsetY = (float) (double) this.ReelStateTable["SymbolOffsetY"];
            
            int toSymbolIndex = fromSymbolIndex + cascadeDistance;

            var fromSymbolState = (Table) this.ReelSymbolsStateTable[fromSymbolIndex];
            var fromSymbolIsLocked = (bool) fromSymbolState["SymbolIsLocked"];
            var fromSymbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{fromSymbolIndex}"];
            float fromSymbolPosition = (float) (double) fromSymbolState["SymbolPosition"];
            var fromSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{fromSymbolIndex}"];
            var fromSymbolLocalPosition = fromSymbolProperty.gameObject.transform.localPosition;
            // set the fromSymbolGroupProperty.CurrentKey to cascadeSymbol
            fromSymbolGroupProperty.SetCurrentActive(cascadeSymbol);
            
            var toSymbolState = (Table) this.ReelSymbolsStateTable[toSymbolIndex];
            var toSymbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{toSymbolIndex}"];
            float toSymbolPosition = (float) (double) toSymbolState["SymbolPosition"];
            var toSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{toSymbolIndex}"];
            
            if (fromSymbolIsLocked)
            {
                yield break;
            }
            
            float yLocalPosition = verticalSpacing * fromSymbolPosition;
            
            while (elapsedTime < duration)
            {
                float slideDistanceInSymbolUnits = (slideDistance / duration) * Time.deltaTime;
                yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;
                Vector3 localPosition = fromSymbolProperty.gameObject.transform.localPosition;
                localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
                fromSymbolProperty.gameObject.transform.localPosition = localPosition;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // swap symbols out
            // from symbol property will be hidden and localPosition reset to the original localPosition
            // to symbol property will be visible and its symbol set to the from symbol

            {
                // hide the fromSymbolGroupProperty.Current
                fromSymbolGroupProperty.Current.SetActive(false);
                // reset fromSymbolProperty localPosition
                fromSymbolProperty.gameObject.transform.localPosition = fromSymbolLocalPosition;
                
                toSymbolGroupProperty.SetCurrentActive(fromSymbolGroupProperty.CurrentKey);
            }
            
        }
        
        private void _SlideSymbol(int symbolIndex, float slideDistanceInSymbolUnits)
        {
            // Get the state of the current symbol
            var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            // If the symbol is locked, return early
            if ((bool) symbolState["SymbolIsLocked"])
            {
                return;
            }
            // Access the symbol property (assuming you have a way to reference symbols like this)
            var symbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{symbolIndex}"];
            // Get the current local position of the symbol's game object
            Vector3 localPosition = symbolProperty.gameObject.transform.localPosition;
            // Get symbol's position and other relevant values from the reel state
            float symbolPosition = (float) (double) symbolState["SymbolPosition"];
            float verticalSpacing = (float) (double) this.ReelStateTable["SymbolVerticalSpacing"];
            float symbolOffsetY = (float) (double) this.ReelStateTable["SymbolOffsetY"];

            // Calculate the new Y position
            float yLocalPosition = verticalSpacing * symbolPosition;
            yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;

            // Update the local position of the symbol
            localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            symbolProperty.gameObject.transform.localPosition = localPosition;
        }
        
        private void _SpliceReel()
        {
            var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            var spliceDistance = (int) (double) this.ReelStateTable["SpliceDistance"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";

            // Check if splicing should be skipped
            bool skipSplice = _SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown);
    
            // Perform splicing if it shouldn't be skipped
            if (spinDirectionIsDown)
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex + spliceDistance + reelSymbolCount) % reelSymbolCount;
                    this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex - 1;
                }
            }
            else
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex - spliceDistance + reelSymbolCount) % reelSymbolCount;
                    this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex + 1;
                }
            }
        }
        
        private bool _SkipSpliceReel(int spliceDistance, int outcomeReelStopIndex, bool spinDirectionIsDown)
        {
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            
            var visibleSymbolCount = (int) (double) this.ReelStateTable["VisibleSymbolCount"];

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

        private void _StartReelAnticipation(float anticipationSpeed, float anticipationDuration)
        {
            Debug.Log($"StartReelAnticipation reelID={this.ReelID} reelIndex={this.ReelIndex} anticipationSpeed={anticipationSpeed} anticipationDuration={anticipationDuration}");
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            ReelOutcomeDelays[this.ReelIndex].ExtendTimer(anticipationDuration);
        }
        
    }
    
    [Serializable]
    public class BettrReelMatrixController
    {
        [NonSerialized] private TileWithUpdate ReelTile;
        [NonSerialized] private GameObject ReelGo;

        [NonSerialized] private string ReelID;
        [NonSerialized] private int ReelIndex;
        [NonSerialized] private string MachineID;
        [NonSerialized] private string MachineVariantID;
        
        public Table ReelTable { get; private set; }
        public Table ReelStateTable { get; private set; }
        public Table ReelSpinStateTable { get; private set; }
        public Table SpinOutcomeTable { get; internal set; }
        public Table ReelSymbolsStateTable { get; private set; }
        public Table ReelSymbolsTable { get; private set; }
        
        // TODO: FIXME move this to ReelStateTable
        [NonSerialized] private bool ShouldSpliceReel;
        
        [NonSerialized] private BettrUserController BettrUserController;
        [NonSerialized] private BettrMathController BettrMathController;
        
        public List<string> ReelStripSymbolsForThisSpin { get; internal set; }
        
        public static ReelStripOutcomeDelay[] ReelOutcomeDelays { get; private set; }

        public BettrReelMatrixController(TileWithUpdate reelTile, BettrUserController userController, BettrMathController mathController)
        {
            ReelTile = reelTile;
            BettrUserController = userController;
            BettrMathController = mathController;
            
            var totalSize = 32;
            ReelOutcomeDelays = new ReelStripOutcomeDelay[totalSize];
            for (var i = 0; i < totalSize; i++)
            {
                ReelOutcomeDelays[i] = new ReelStripOutcomeDelay(i, totalSize);
            }
            // special case for handling ReelIndex=0
            ReelOutcomeDelays[totalSize-1].SetStopped(true);
            
            this.ReelGo = ReelTile.GetProperty<GameObject>("gameObject");
            
            this.ReelID = ReelTile.GetProperty<string>("ReelID");
            this.ReelIndex = ReelTile.GetProperty<int>("ReelIndex");
            this.MachineID = ReelTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelTile.GetProperty<string>("MachineVariantID");

            this.ReelTable = BettrMathController.GetGlobalTable(ReelTile.globalTileId);
            this.ReelStateTable = BettrMathController.GetTableFirst("BaseGameReelState", this.MachineID, this.ReelID);
            this.ReelSpinStateTable = BettrMathController.GetTableFirst("BaseGameReelSpinState", this.MachineID, this.ReelID);
            this.SpinOutcomeTable = BettrMathController.GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            this.ReelSymbolsStateTable = BettrMathController.GetTableArray("BaseGameReelSymbolsState", this.MachineID, this.ReelID);
            this.ReelSymbolsTable = BettrMathController.GetTableArray("BaseGameReelSet", this.MachineID, this.ReelID);
        }
        
        public IEnumerator StartEngines()
        {
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelSymbolsCount = this.ReelSymbolsStateTable.Length;
            for (int i = 1; i <= reelSymbolsCount; i++)
            {
                var symbolStateTable = (Table) this.ReelSymbolsStateTable[i];
                var reelPosition = (int) (double) symbolStateTable["ReelPosition"];
                int symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
                var reelSymbol = (string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{i}"];
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
            this.SpinOutcomeTable = BettrMathController.GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            float delayInSeconds = (float) (double) this.ReelStateTable["ReelStopDelayInSeconds"];
            ReelOutcomeDelays[this.ReelIndex].SetReelStopDelayInSeconds(delayInSeconds);
            yield break;
        }

        public IEnumerator OnApplyOutcomeReceived()
        {
            this.ShouldSpliceReel = true;
            this.ReelStateTable["OutcomeReceived"] = true;
            // Debug.Log($"OnApplyOutcomeReceived ReelIndex: {this.ReelIndex} time: {Time.time}");
            yield break;
        }
        
        public void SpinEngines()
        {
            this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollBack";
            this.ReelStateTable["OutcomeReceived"] = false;
            this.ShouldSpliceReel = false;
            
            this.SetupReelStripSymbolsForSpin();
            ReelOutcomeDelays[this.ReelIndex].Reset();
        }

        public string ReplaceSymbolForSpin(int zeroIndex, string newSymbol)
        {
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            int oneIndexed = 1 +zeroIndex % reelSymbolCount;
            var oldSymbol = this.ReelStripSymbolsForThisSpin[oneIndexed];
            this.ReelStripSymbolsForThisSpin[oneIndexed] = newSymbol;
            return oldSymbol;
        }

        public void SetupReelStripSymbolsForSpin()
        {
            // create a copy of the ReelSymbolsTable
            if (this.ReelStripSymbolsForThisSpin == null)
            {
                this.ReelStripSymbolsForThisSpin = new List<string>();
            }
            this.ReelStripSymbolsForThisSpin.Clear();
            this.ReelStripSymbolsForThisSpin.Add("Blank");

            var length = this.ReelSymbolsTable.Length;
            for (int i = 0; i < length; i++)
            {
                // 1 indexed
                this.ReelStripSymbolsForThisSpin.Add((string) ((Table) this.ReelSymbolsTable[i + 1])["ReelSymbol"]);
            }
        }
        
        public void SpinReelSpinStartedRollBack()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelStateTable["SpinStartedRollBackDistanceInSymbolUnits"];
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
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
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollForward";
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
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < 0)
                {
                    SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
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
                    this.ReelSpinStateTable["ReelSpinState"] = "Spinning";
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
            ReelTile.StartCoroutine(this.ReelTile.CallAction("PlaySpinReelSpinEndingRollBackAnimation"));
            
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 1;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinEndingRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits > 0)
                {
                    SlideReelSymbols(0);
                    this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
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
                    this.ReelSpinStateTable["ReelSpinState"] = "Stopped";
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
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinEndingRollForwardSpeedInSymbolUnitsPerSecond"] * speed;
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            var spinDirectionIsDown = reelSpinDirection == "Down";
            var slideDistanceThresholdInSymbolUnits = (float) (double) this.ReelStateTable["SpinEndingRollForwardDistanceInSymbolUnits"];
            var slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();
            if (spinDirectionIsDown)
            {
                if (slideDistanceInSymbolUnits < slideDistanceThresholdInSymbolUnits)
                {
                    SlideReelSymbols(slideDistanceThresholdInSymbolUnits);
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
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
                    this.ReelSpinStateTable["ReelSpinState"] = "SpinEndingRollBack";
                }
                else
                {
                    SlideReelSymbols(slideDistanceInSymbolUnits);
                }
            }
        }
        
        public bool SpinReelSpinning()
        {
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinSpeedInSymbolUnitsPerSecond"] * speed;
            float slideDistanceInSymbolUnits = AdvanceReel();
            SlideReelSymbols(slideDistanceInSymbolUnits);
            return true;
        }
        
        public float CalculateSlideDistanceInSymbolUnits()
        {
            // Unity's Time.deltaTime provides the duration of the last frame in seconds
            float frameDurationInSeconds = Time.deltaTime;
            // Get the speed in symbol units per second from reelSpinState
            float speedInSymbolUnits = (float) (double) this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"];
            // Calculate distance traveled in this frame
            float distanceInSymbolUnits = speedInSymbolUnits * frameDurationInSeconds;
            // Check spin direction
            bool spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";
            // Get the current slide distance
            float slideDistanceInSymbolUnits = (float) (double) this.ReelSpinStateTable["SlideDistanceInSymbolUnits"];
            // Update the slide distance by adding the distance traveled
            slideDistanceInSymbolUnits += distanceInSymbolUnits;
            return slideDistanceInSymbolUnits;
        }
        
        public float AdvanceReel()
        {
            var reelSpinDirection = (string) this.ReelSpinStateTable["ReelSpinDirection"];
            bool spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceOffsetInSymbolUnits = spinDirectionIsDown ? 1 : -1;
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits();

            while ((spinDirectionIsDown && slideDistanceInSymbolUnits < -1) || (!spinDirectionIsDown && slideDistanceInSymbolUnits > 1))
            {
                AdvanceSymbols();
                UpdateReelStopIndexes();
                ApplySpinReelStop();
                slideDistanceInSymbolUnits += slideDistanceOffsetInSymbolUnits;
                if (ReelOutcomeDelays[this.ReelIndex].IsStoppedForSpin)
                {
                    break;
                }
            }

            var spinState = (string) this.ReelSpinStateTable["ReelSpinState"];
            if (spinState is "ReachedOutcomeStopIndex" or "Stopped")
            {
                slideDistanceInSymbolUnits = 0;
            }

            return slideDistanceInSymbolUnits;
        }

        public void AdvanceSymbols()
        {
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            for (var i = 1; i <= symbolCount; i++)
            {
                UpdateReelSymbolForSpin(i);
            }
        }
        
        public int GetReelVisibleSymbolCount()
        {
            return (int) (double) this.ReelStateTable["VisibleSymbolCount"];
        }
        
        public List<TilePropertyGameObjectGroup> GetReelMatrixVisibleSymbolsGroups(params string[] symbols)
        {
            var reelMatrixSymbolGroups = new List<TilePropertyGameObjectGroup>();
            var topSymbolCount = (int) (double) this.ReelStateTable["TopSymbolCount"];
            var bottomSymbolCount = (int) (double) this.ReelStateTable["BottomSymbolCount"];
            var visibleSymbolCount = (int) (double) this.ReelStateTable["VisibleSymbolCount"];
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            var startSymbolIndexOneIndexed = 1 + topSymbolCount;
            var endSymbolIndexOneIndexed = symbolCount - bottomSymbolCount;
            for (var symbolIndex = startSymbolIndexOneIndexed; symbolIndex <= endSymbolIndexOneIndexed; symbolIndex++)
            {
                var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{symbolIndex}"];
                reelMatrixSymbolGroups.Add(symbolGroupProperty);
            }
            return reelMatrixSymbolGroups;
        }

        public List<TilePropertyGameObject> GetReelMatrixVisibleSymbols(params string[] symbols)
        {
            var reelMatrixSymbols = new List<TilePropertyGameObject>();
            var reelMatrixSymbolGroups = GetReelMatrixVisibleSymbolsGroups(symbols);
            foreach (var symbolGroupProperty in reelMatrixSymbolGroups)
            {
                var currentKey = symbolGroupProperty.CurrentKey;
                // find the corresponding TilePropertyGameObject in the symbolGroupProperty.GameObjectProperties
                var symbolProperty = symbolGroupProperty.gameObjectProperties.Find(x => x.key == currentKey);
                if (symbolProperty != null)
                {
                    // if symbols is not null, then the symbol should be in the symbols array to be added 
                    // to the reelMatrixSymbols list else add it by default
                    if (symbols != null && symbols.Length > 0)
                    {
                        if (Array.Exists(symbols, symbol => symbol == currentKey))
                        {
                            reelMatrixSymbols.Add(symbolProperty);
                        }
                    }
                    else
                    {
                        reelMatrixSymbols.Add(symbolProperty);
                    }
                }
            }
            return reelMatrixSymbols;
        }

        public void UpdateReelSymbolForSpin(int symbolIndex)
        {
            var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            var symbolIsLocked = (bool) symbolState["SymbolIsLocked"];
            if (symbolIsLocked)
            {
                return;
            }

            var rowVisible = (bool) symbolState["RowVisible"];
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelPosition = (int) (double) symbolState["ReelPosition"];
            var symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            var reelSymbol = this.ReelStripSymbolsForThisSpin[symbolStopIndex]; //(string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
            var symbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{symbolIndex}"];
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
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            // Get the current stop index and advance offset
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) this.ReelStateTable["ReelStopIndexAdvanceOffset"];
            // Update the reel stop index
            reelStopIndex += reelStopIndexAdvanceOffset;
            // Wrap the stop index to keep it within bounds using modulus
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Assign the updated stop index back to the spin state
            this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex;
        }
        
        public void ApplySpinReelStop()
        {
            // Check if the outcome has been received
            if (!(bool) this.ReelStateTable["OutcomeReceived"])
            {
                return;
            }
            if (this.ShouldSpliceReel)
            {
                // get the previousReelIndex
                var thisReelOutcomeDelay = ReelOutcomeDelays[this.ReelIndex];
                if (!thisReelOutcomeDelay.IsTimerStartedForSpin)
                {
                    var previousReelOutcomeDelay = ReelOutcomeDelays[thisReelOutcomeDelay.PreviousReelIndex];
                    if (previousReelOutcomeDelay.IsStoppedForSpin)
                    {
                        thisReelOutcomeDelay.StartTimer();
                    }
                }
                if (!thisReelOutcomeDelay.IsApplySpiceReel)
                {
                    return;
                }
                SpliceReel();
                this.ShouldSpliceReel = false;
            }
            // Get the current stop index and outcome-related values
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) this.ReelStateTable["ReelStopIndexAdvanceOffset"];
            // Adjust the stop index
            reelStopIndex -= reelStopIndexAdvanceOffset;
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Get the outcome's stop index
            var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            // Check if the reel stop index matches the outcome stop index
            if (outcomeReelStopIndex == reelStopIndex)
            {
                this.ReelSpinStateTable["ReelSpinState"] = "ReachedOutcomeStopIndex";
                // let the ReelOutcomeDelay know that this reel has stopped
                ReelOutcomeDelays[this.ReelIndex].SetStopped(true);
            }
        }
        
        public void SlideReelSymbols(float slideDistanceInSymbolUnits)
        {
            // Get the symbol count from reelState
            var symbolCount = (int) (double) this.ReelStateTable["SymbolCount"];
            // Iterate through each symbol and apply the slide distance
            for (int i = 1; i <= symbolCount; i++)
            {
                SlideSymbol(i, slideDistanceInSymbolUnits);
            }
            // Set the SlideDistanceInSymbolUnits for the reel spin state
            this.ReelSpinStateTable["SlideDistanceInSymbolUnits"] = slideDistanceInSymbolUnits;
        }

        public GameObject FindSymbolQuad(TilePropertyGameObjectGroup symbolGroupProperty)
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
            var symbolQuad = FindSymbolQuad(symbolGroupProperty);
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
            var slideDistance = -cascadeDistance;
            float duration = 0.4f;
            float elapsedTime = 0f;
            
            float verticalSpacing = (float) (double) this.ReelStateTable["SymbolVerticalSpacing"];
            float symbolOffsetY = (float) (double) this.ReelStateTable["SymbolOffsetY"];
            
            int toSymbolIndex = fromSymbolIndex + cascadeDistance;

            var fromSymbolState = (Table) this.ReelSymbolsStateTable[fromSymbolIndex];
            var fromSymbolIsLocked = (bool) fromSymbolState["SymbolIsLocked"];
            var fromSymbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{fromSymbolIndex}"];
            float fromSymbolPosition = (float) (double) fromSymbolState["SymbolPosition"];
            var fromSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{fromSymbolIndex}"];
            var fromSymbolLocalPosition = fromSymbolProperty.gameObject.transform.localPosition;
            // set the fromSymbolGroupProperty.CurrentKey to cascadeSymbol
            fromSymbolGroupProperty.SetCurrentActive(cascadeSymbol);
            
            var toSymbolState = (Table) this.ReelSymbolsStateTable[toSymbolIndex];
            var toSymbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{toSymbolIndex}"];
            float toSymbolPosition = (float) (double) toSymbolState["SymbolPosition"];
            var toSymbolGroupProperty = (TilePropertyGameObjectGroup) this.ReelTable[$"SymbolGroup{toSymbolIndex}"];
            
            if (fromSymbolIsLocked)
            {
                yield break;
            }
            
            float yLocalPosition = verticalSpacing * fromSymbolPosition;
            
            while (elapsedTime < duration)
            {
                float slideDistanceInSymbolUnits = (slideDistance / duration) * Time.deltaTime;
                yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;
                Vector3 localPosition = fromSymbolProperty.gameObject.transform.localPosition;
                localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
                fromSymbolProperty.gameObject.transform.localPosition = localPosition;
                elapsedTime += Time.deltaTime;
                yield return null;
            }
            
            // swap symbols out
            // from symbol property will be hidden and localPosition reset to the original localPosition
            // to symbol property will be visible and its symbol set to the from symbol

            {
                // hide the fromSymbolGroupProperty.Current
                fromSymbolGroupProperty.Current.SetActive(false);
                // reset fromSymbolProperty localPosition
                fromSymbolProperty.gameObject.transform.localPosition = fromSymbolLocalPosition;
                
                toSymbolGroupProperty.SetCurrentActive(fromSymbolGroupProperty.CurrentKey);
            }
            
        }
        
        public void SlideSymbol(int symbolIndex, float slideDistanceInSymbolUnits)
        {
            // Get the state of the current symbol
            var symbolState = (Table) this.ReelSymbolsStateTable[symbolIndex];
            // If the symbol is locked, return early
            if ((bool) symbolState["SymbolIsLocked"])
            {
                return;
            }
            // Access the symbol property (assuming you have a way to reference symbols like this)
            var symbolProperty = (PropertyGameObject) this.ReelTable[$"Symbol{symbolIndex}"];
            // Get the current local position of the symbol's game object
            Vector3 localPosition = symbolProperty.gameObject.transform.localPosition;
            // Get symbol's position and other relevant values from the reel state
            float symbolPosition = (float) (double) symbolState["SymbolPosition"];
            float verticalSpacing = (float) (double) this.ReelStateTable["SymbolVerticalSpacing"];
            float symbolOffsetY = (float) (double) this.ReelStateTable["SymbolOffsetY"];

            // Calculate the new Y position
            float yLocalPosition = verticalSpacing * symbolPosition;
            yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;

            // Update the local position of the symbol
            localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            symbolProperty.gameObject.transform.localPosition = localPosition;
        }
        
        public void SpliceReel()
        {
            var outcomeReelStopIndex = (int) (double) this.SpinOutcomeTable["OutcomeReelStopIndex"];
            var spliceDistance = (int) (double) this.ReelStateTable["SpliceDistance"];
            var reelSymbolCount = (int) (double) this.ReelStateTable["ReelSymbolCount"];
            var spinDirectionIsDown = (string) this.ReelSpinStateTable["ReelSpinDirection"] == "Down";

            // Check if splicing should be skipped
            bool skipSplice = SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown);
    
            // Perform splicing if it shouldn't be skipped
            if (spinDirectionIsDown)
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex + spliceDistance + reelSymbolCount) % reelSymbolCount;
                    this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex - 1;
                }
            }
            else
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (outcomeReelStopIndex - spliceDistance + reelSymbolCount) % reelSymbolCount;
                    this.ReelSpinStateTable["ReelStopIndex"] = reelStopIndex + 1;
                }
            }
        }
        
        public bool SkipSpliceReel(int spliceDistance, int outcomeReelStopIndex, bool spinDirectionIsDown)
        {
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            
            var visibleSymbolCount = (int) (double) this.ReelStateTable["VisibleSymbolCount"];

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

        public void StartReelAnticipation(float anticipationSpeed, float anticipationDuration)
        {
            Debug.Log($"StartReelAnticipation reelID={this.ReelID} reelIndex={this.ReelIndex} anticipationSpeed={anticipationSpeed} anticipationDuration={anticipationDuration}");
            var speed = BettrUserController.UserInSlamStopMode ? 4 : 4;
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"] * speed;
            ReelOutcomeDelays[this.ReelIndex].ExtendTimer(anticipationDuration);
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