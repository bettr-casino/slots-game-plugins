using CrayonScript.Code;
using CrayonScript.Interpreter;
using UnityEngine;

// ReSharper disable once CheckNamespace
namespace Bettr.Core
{
    public class BettrReelController
    {
        public BettrReelController()
        {
            TileController.RegisterType<BettrReelController>("BettrReelController");
            TileController.AddToGlobals("BettrReelController", this);
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
        
        public float CalculateSlideDistanceInSymbolUnits(Table reelSpinStateTable)
        {
            // Unity's Time.deltaTime provides the duration of the last frame in seconds
            float frameDurationInSeconds = Time.deltaTime;
            // Get the speed in symbol units per second from reelSpinState
            float speedInSymbolUnits = (float) (double) reelSpinStateTable["SpeedInSymbolUnitsPerSecond"];
            // Calculate distance traveled in this frame
            float distanceInSymbolUnits = speedInSymbolUnits * frameDurationInSeconds;
            // Check spin direction
            bool spinDirectionIsDown = (string) reelSpinStateTable["ReelSpinDirection"] == "Down";
            // Get the current slide distance
            float slideDistanceInSymbolUnits = (float) (double) reelSpinStateTable["SlideDistanceInSymbolUnits"];
            // Update the slide distance by adding the distance traveled
            slideDistanceInSymbolUnits += distanceInSymbolUnits;
            return slideDistanceInSymbolUnits;
        }
        
        public float AdvanceReel(Table reelTable, Table reelStateTable, Table reelSpinStateTable, Table reelSymbolsStateTable, Table reelSymbolsTable, Table spinOutcomeTable)
        {
            var reelSpinDirection = (string) reelSpinStateTable["ReelSpinDirection"];
            bool spinDirectionIsDown = reelSpinDirection == "Down";
            float slideDistanceOffsetInSymbolUnits = spinDirectionIsDown ? 1 : -1;
            float slideDistanceInSymbolUnits = CalculateSlideDistanceInSymbolUnits(reelSpinStateTable);

            while ((spinDirectionIsDown && slideDistanceInSymbolUnits < -1) || (!spinDirectionIsDown && slideDistanceInSymbolUnits > 1))
            {
                AdvanceSymbols(reelTable, reelStateTable, reelSpinStateTable, reelSymbolsStateTable, reelSymbolsTable);
                UpdateReelStopIndexes(reelStateTable, reelSpinStateTable);
                ApplySpinReelStop( reelStateTable, reelSpinStateTable, spinOutcomeTable);
                slideDistanceInSymbolUnits += slideDistanceOffsetInSymbolUnits;
            }

            var spinState = (string) reelSpinStateTable["ReelSpinState"];
            if (spinState is "ReachedOutcomeStopIndex" or "Stopped")
            {
                slideDistanceInSymbolUnits = 0;
            }

            return slideDistanceInSymbolUnits;
        }

        public void AdvanceSymbols(Table reelTable, Table reelStateTable, Table reelSpinStateTable, Table reelSymbolsStateTable, Table reelSymbolsTable)
        {
            var symbolCount = (int) (double) reelStateTable["SymbolCount"];
            for (var i = 1; i <= symbolCount; i++)
            {
                UpdateReelSymbolForSpin(i, reelTable, reelStateTable, reelSpinStateTable, reelSymbolsStateTable, reelSymbolsTable);
            }
        }

        public void UpdateReelSymbolForSpin(int symbolIndex, Table reelTable, Table reelStateTable,
            Table reelSpinStateTable, Table reelSymbolsStateTable, Table reelSymbolsTable)
        {
            var symbolState = (Table) reelSymbolsStateTable[symbolIndex];
            var symbolIsLocked = (bool) symbolState["SymbolIsLocked"];
            if (symbolIsLocked)
            {
                return;
            }

            var rowVisible = (bool) symbolState["RowVisible"];
            var reelStopIndex = (int) (double) reelSpinStateTable["ReelStopIndex"];
            var reelSymbolCount = (int) (double) reelStateTable["ReelSymbolCount"];
            var reelPosition = (int) (double) symbolState["ReelPosition"];
            var symbolStopIndex = 1 + (reelSymbolCount + reelStopIndex + reelPosition) % reelSymbolCount;
            var reelSymbol = (string) ((Table) reelSymbolsTable[symbolStopIndex])["ReelSymbol"];
            var symbolGroupProperty = (TilePropertyGameObjectGroup) reelTable[$"SymbolGroup{symbolIndex}"];
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
        
        public void UpdateReelStopIndexes(Table reelStateTable, Table reelSpinStateTable)
        {
            var reelSymbolCount = (int) (double) reelStateTable["ReelSymbolCount"];
            // Get the current stop index and advance offset
            var reelStopIndex = (int) (double) reelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) reelStateTable["ReelStopIndexAdvanceOffset"];
            // Update the reel stop index
            reelStopIndex += reelStopIndexAdvanceOffset;
            // Wrap the stop index to keep it within bounds using modulus
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Assign the updated stop index back to the spin state
            reelSpinStateTable["ReelStopIndex"] = reelStopIndex;
        }
        
        public void ApplySpinReelStop(Table reelStateTable, Table reelSpinStateTable, Table spinOutcomeTable)
        {
            // Check if the outcome has been received
            if (!(bool) reelStateTable["OutcomeReceived"])
            {
                return;
            }
            // Get the current stop index and outcome-related values
            var reelSymbolCount = (int) (double) reelStateTable["ReelSymbolCount"];
            var reelStopIndex = (int) (double) reelSpinStateTable["ReelStopIndex"];
            var reelStopIndexAdvanceOffset = (int) (double) reelStateTable["ReelStopIndexAdvanceOffset"];
            // Adjust the stop index
            reelStopIndex -= reelStopIndexAdvanceOffset;
            reelStopIndex = (reelSymbolCount + reelStopIndex) % reelSymbolCount;
            // Get the outcome's stop index
            var outcomeReelStopIndex = (int) (double) spinOutcomeTable["OutcomeReelStopIndex"];
            // Check if the reel stop index matches the outcome stop index
            if (outcomeReelStopIndex == reelStopIndex)
            {
                reelSpinStateTable["ReelSpinState"] = "ReachedOutcomeStopIndex";
            }
        }
        
        public void SlideReelSymbols(float slideDistanceInSymbolUnits, Table reelTable, Table reelStateTable, Table reelSpinStateTable, Table reelSymbolsStateTable)
        {
            // Get the symbol count from reelState
            var symbolCount = (int) (double) reelStateTable["SymbolCount"];
            // Iterate through each symbol and apply the slide distance
            for (int i = 1; i <= symbolCount; i++)
            {
                SlideSymbol(i, slideDistanceInSymbolUnits, reelTable, reelStateTable, reelSymbolsStateTable);
            }
            // Set the SlideDistanceInSymbolUnits for the reel spin state
            reelSpinStateTable["SlideDistanceInSymbolUnits"] = slideDistanceInSymbolUnits;
        }
        
        public void SlideSymbol(int symbolIndex, float slideDistanceInSymbolUnits, Table reelTable, Table reelStateTable, Table reelSymbolsStateTable)
        {
            // Get the state of the current symbol
            var symbolState = (Table) reelSymbolsStateTable[symbolIndex];
            // If the symbol is locked, return early
            if ((bool) symbolState["SymbolIsLocked"])
            {
                return;
            }
            // Access the symbol property (assuming you have a way to reference symbols like this)
            var symbolProperty = (PropertyGameObject) reelTable[$"Symbol{symbolIndex}"];
            // Get the current local position of the symbol's game object
            Vector3 localPosition = symbolProperty.gameObject.transform.localPosition;
            // Get symbol's position and other relevant values from the reel state
            float symbolPosition = (float) (double) symbolState["SymbolPosition"];
            float verticalSpacing = (float) (double) reelStateTable["SymbolVerticalSpacing"];
            float symbolOffsetY = (float) (double) reelStateTable["SymbolOffsetY"];

            // Calculate the new Y position
            float yLocalPosition = verticalSpacing * symbolPosition;
            yLocalPosition = yLocalPosition + verticalSpacing * slideDistanceInSymbolUnits + symbolOffsetY;

            // Update the local position of the symbol
            localPosition = new Vector3(localPosition.x, yLocalPosition, localPosition.z);
            symbolProperty.gameObject.transform.localPosition = localPosition;
        }
        
        public void SpliceReel(Table reelStateTable, Table reelSpinStateTable, Table spinOutcomeTable)
        {
            var d1 = (int) (double) spinOutcomeTable["OutcomeReelStopIndex"];
            var d2 = (int) (double) reelStateTable["SpliceDistance"];
            var reelSymbolCount = (int) (double) reelStateTable["ReelSymbolCount"];
            var spinDirectionIsDown = (string) reelSpinStateTable["ReelSpinDirection"] == "Down";

            // Check if splicing should be skipped
            bool skipSplice = SkipSpliceReel(reelStateTable, reelSpinStateTable, spinOutcomeTable);
    
            // Perform splicing if it shouldn't be skipped
            if (spinDirectionIsDown)
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (d1 + d2 + reelSymbolCount) % reelSymbolCount;
                    reelSpinStateTable["ReelStopIndex"] = reelStopIndex - 1;
                }
            }
            else
            {
                if (!skipSplice)
                {
                    int reelStopIndex = (d1 - d2 + reelSymbolCount) % reelSymbolCount;
                    reelSpinStateTable["ReelStopIndex"] = reelStopIndex + 1;
                }
            }
        }
        
        public bool SkipSpliceReel(Table reelStateTable, Table reelSpinStateTable, Table spinOutcomeTable)
        {
            var spinDirectionIsDown = (string) reelSpinStateTable["ReelSpinDirection"] == "Down";
            var reelStopIndex = (int) (double) reelSpinStateTable["ReelStopIndex"];
            var outcomeReelStopIndex = (int) (double) spinOutcomeTable["OutcomeReelStopIndex"];

            var topSymbolCount = (int) (double) reelStateTable["TopSymbolCount"];
            var bottomSymbolCount = (int) (double) reelStateTable["BottomSymbolCount"];
            var visibleSymbolCount = (int) (double) reelStateTable["VisibleSymbolCount"];

            // Check if the outcome reel stop index is within top or bottom symbol offsets
            bool inTopSymbolOffset = outcomeReelStopIndex >= reelStopIndex - topSymbolCount && outcomeReelStopIndex < reelStopIndex;
            bool inBottomSymbolOffset = outcomeReelStopIndex >= reelStopIndex + visibleSymbolCount && 
                                        outcomeReelStopIndex < reelStopIndex + visibleSymbolCount + bottomSymbolCount;

            if (spinDirectionIsDown)
            {
                // For spin down, skip if the outcome stop index is in the top symbol offset
                return inTopSymbolOffset;
            }
            else
            {
                // For spin up, skip if the outcome stop index is in the bottom symbol offset
                return inBottomSymbolOffset;
            }
        }
        
        
    }
}