// ReSharper disable All
using System;
using System.Collections;
using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;
namespace Bettr.Core
{
    [Serializable]
    public class BettrReelController : MonoBehaviour
    {
        [NonSerialized] private TileWithUpdate ReelTile;
        [NonSerialized] private GameObject ReelGo;

        [NonSerialized] private string ReelID;
        [NonSerialized] private string MachineID;
        [NonSerialized] private string MachineVariantID;
        
        [NonSerialized] private Table ReelTable;
        [NonSerialized] private Table ReelStateTable;
        [NonSerialized] private Table ReelSpinStateTable;
        [NonSerialized] private Table SpinOutcomeTable;
        [NonSerialized] private Table ReelSymbolsStateTable;
        [NonSerialized] private Table ReelSymbolsTable;
        
        // TODO: FIXME move this to ReelStateTable
        [NonSerialized] private bool ShouldSpliceReel; 
        
        private void Awake()
        {
            ReelTile = GetComponent<TileWithUpdate>();
        }
        
        private IEnumerator Start()
        {
            this.ReelGo = ReelTile.GetProperty<GameObject>("gameObject");
            
            this.ReelID = ReelTile.GetProperty<string>("ReelID");
            this.MachineID = ReelTile.GetProperty<string>("MachineID");
            this.MachineVariantID = ReelTile.GetProperty<string>("MachineVariantID");

            this.ReelTable = GetGlobalTable(ReelTile.globalTileId);
            this.ReelStateTable = GetTableFirst("BaseGameReelState", this.MachineID, this.ReelID);
            this.ReelSpinStateTable = GetTableFirst("BaseGameReelSpinState", this.MachineID, this.ReelID);
            this.SpinOutcomeTable = GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            this.ReelSymbolsStateTable = GetTableArray("BaseGameReelSymbolsState", this.MachineID, this.ReelID);
            this.ReelSymbolsTable = GetTableArray("BaseGameReelSet", this.MachineID, this.ReelID);
            
            // add this to the ReelTile properties
            this.ReelTable["BettrReelController"] = this;
            
            yield break;
        }
        
        private Table GetGlobalTable(string tableName)
        {
            var globalTable = (Table) TileController.LuaScript.Globals[tableName];
            return globalTable;
        }

        private Table GetTableFirst(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelStateTable = (Table) reelTable["First"];
            return reelStateTable;
        }

        private Table GetTableArray(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelStateTable = (Table) reelTable["Array"];
            return reelStateTable;
        }
        
        private int GetTableCount(string tableName, string machineID, string reelID)
        {
            var machineTable = (Table) TileController.LuaScript.Globals[$"{machineID}{tableName}"];
            var reelTable = (Table) machineTable[reelID];
            var reelTableCount = (int) (double) reelTable["Count"];
            return reelTableCount;
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
            this.SpinOutcomeTable = GetTableFirst("BaseGameReelSpinOutcome", this.MachineID, this.ReelID);
            float delayInSeconds = (float) (double) this.ReelStateTable["ReelStopDelayInSeconds"];
            yield return new WaitForSeconds(delayInSeconds);
            this.ShouldSpliceReel = true;
            this.ReelStateTable["OutcomeReceived"] = true;
        }
        
        public void SpinEngines()
        {
            this.ReelSpinStateTable["ReelSpinState"] = "SpinStartedRollBack";
            this.ReelStateTable["OutcomeReceived"] = false;
            this.ShouldSpliceReel = false;
        }
        
        public void SpinReelSpinStartedRollBack()
        {
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = this.ReelStateTable["SpinStartedRollBackSpeedInSymbolUnitsPerSecond"];
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
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = this.ReelStateTable["SpinStartedRollForwardSpeedInSymbolUnitsPerSecond"];
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
            StartCoroutine(this.ReelTile.CallAction("PlaySpinReelSpinEndingRollBackAnimation"));
            
            var reelStopIndex = (int) (double) this.ReelSpinStateTable["ReelStopIndex"];
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = this.ReelStateTable["SpinEndingRollBackSpeedInSymbolUnitsPerSecond"];
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
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = this.ReelStateTable["SpinEndingRollForwardSpeedInSymbolUnitsPerSecond"];
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
            this.ReelSpinStateTable["SpeedInSymbolUnitsPerSecond"] = (double) this.ReelStateTable["SpinSpeedInSymbolUnitsPerSecond"] * 2;
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
            var reelSymbol = (string) ((Table) this.ReelSymbolsTable[symbolStopIndex])["ReelSymbol"];
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
        
        
    }
}