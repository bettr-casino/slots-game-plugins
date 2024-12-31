# GameBaseGameMachine Functions (Base Game)

| Function Name                | Status  | Arguments           | Description                                                                 |
|------------------------------|---------|---------------------|-----------------------------------------------------------------------------|
| `Start()`                    | Done    | None                | Initializes the game sequence by stopping audio, configuring settings, and starting reel engines. |
| `OnSpinButtonClicked()`       | Done    | None                | Handles the spin button click event, starts reels, loads the server outcome, and processes the result. |
| `ConfigureSettings()`         | Done    | None                | Configures and updates UI elements such as credits, bet, and win text.      |
| `ShowSettings()`              | Done    | None                | Displays the game settings menu.                                            |
| `HideSettings()`              | Done    | None                | Hides the game settings menu.                                               |
| `ResetSettings()`             | Done    | None                | Resets the win text to zero after a spin.                                   |
| `Update()`                    | Done    | None                | Updates the game state in each frame, dispatching to the appropriate handler based on the game state. |
| `BaseGameSpinning()`          | Done    | None                | Handles the actions for when the game is in the spinning state (currently a placeholder). |
| `BaseGameWaiting()`           | Done    | None                | Handles the actions for when the game is in the waiting state (currently a placeholder). |
| `BaseGameCompleted()`         | Done    | None                | Handles the actions for when the base game spin is completed, including showing settings and resetting the spin state to waiting. |
| `OnBaseGameSpinCompleted()`   | Done    | None                | Updates the state to "Completed" after the base game spin is done.          |
| `BaseGamePayout()`            | Done    | None                | Triggers the payout process for the base game by calling the mechanics’ payout function. |
| `UpdateBaseGameReelsSpinState(state)` | Done    | state (string) | Updates the spin state for each reel in the base game.                      |
| `TryPaying()`                 | Done    | None                | Changes the state to "Paying" to initiate the payout phase.                 |
| `CurrentSpinState()`          | Done    | None                | Returns the current spin state of the base game.                            |
| `OnPayingCompleted()`         | Done    | None                | Stops the audio and marks the spin as completed.                            |
| `OnSpinReelsStopped()`        | Done    | None                | Marks the reels as stopped, updates the reel state to waiting, and triggers the payout. |
| `OnOutcomeReceived()`         | Done    | None                | Handles the outcome reception for all reels in parallel.                    |
| `OnPointerClick()`            | Done    | None                | Handles pointer clicks to start the spin when the user clicks.              |
| `OnPointerClick1Param(param)` | Done    | param (string)      | Handles pointer click events with a parameter to navigate to the next/previous machine or lobby. |
| `OnBecameVisible()`           | Done    | None                | Logs when the game machine becomes visible in the game window.              |
| `CalculateAndDisplayPayout()` | TODO    | None                | Calculates the final payout and triggers a celebratory display for the player. |
| `ResetToIdleState()`          | TODO    | None                | Resets the game to the idle state after all processes are complete.          |
| `ActivateWildReels(reels)`    | Done    | reels (array)       | Highlights the reels that will turn wild, applying special visual effects and turning all symbols on those reels into wilds. |
| `CheckTriggerCondition()`     | Done    | None                | Evaluates the condition to trigger the Wild Reels feature during a spin.    |
| `TransformReelsToWild(reels)` | Done    | reels (array)       | Transforms the selected reels into wild reels with special visual effects and sound cues. |
| `BonusRoundTriggered()`       | Done    | None                | Handles the transition to a bonus round if triggered by a scatter or bonus symbol. |

---

# Functions Operating on Entire Reel Set

| Function Name                            | Status  | Arguments                              | Description                                                                 |
|------------------------------------------|---------|----------------------------------------|-----------------------------------------------------------------------------|
| `StartEngines()`                         | Done    | None                                   | Starts the reel engine for all reels.                                       |
| `SpinEngines()`                          | Done    | None                                   | Spins the reels by triggering the reel controller.                          |
| `DetectWinningCombinationGroupedByReel(reels)` | TODO    | reels (array)                         | Detects winning combinations across the specified reels grouped by their reel positions. |
| `CheckForNewWinsAfterCascade(reels)`     | TODO    | reels (array)                         | Checks for any new winning combinations after a symbol cascade across the specified reels. |
| `ActivateMirrorReels(reels)`             | Done    | reels (array)                         | Highlights the reels that will mirror and applies visual effects to synchronize their symbols. |
| `DisplayMirroredSymbols(reels)`          | Done    | reels (array)                         | Displays identical symbols on the mirrored reels when they stop spinning.   |
| `ActivateMysteryReels(reels)`            | Done    | reels (array)                         | Covers the designated reels with mystery symbols and applies visual effects to indicate the transformation. |
| `RevealMysterySymbols(reels)`            | Done    | reels (array)                         | Reveals the mystery symbols on the designated reels and displays the matching symbol. |
| `ActivateReelSwap(reels)`                | Done    | reels (array)                         | Highlights the reels that will swap positions and applies visual effects to indicate their movement. |
| `SwapReelsPositions(reels)`              | Done    | reels (array)                         | Swaps the positions of the specified reels and updates the visual alignment of symbols. |

---

# Functions Operating on Reel Strip Groups (Multiple Reels)

| Function Name                            | Status  | Arguments                              | Description                                                                 |
|------------------------------------------|---------|----------------------------------------|-----------------------------------------------------------------------------|
| `OnOutcomeReceived()`                    | Done    | None                                   | Processes the outcome for all reels after a spin.                           |
| `PlaySpinReelSpinEndingRollBackAnimation()` | Done    | None                                  | Plays the ending rollback animation for all reels.                          |
| `CascadeSymbols(fromRow, toRow, reels)`  | TODO    | fromRow (int), toRow (int), reels (array) | Cascades symbols from one row to another across the specified reels.        |
| `DisplaySyncedSymbols(reels)`            | Done    | reels (array)                         | Displays identical symbols on the synced reels when they stop spinning, based on identical reel strips. |
| `ActivateSyncedReels(reels)`             | Done    | reels (array)                         | Applies the synced reel animation and ensures identical symbols across reels. |

---

# Functions Operating on Reel Strip (Single Reel)

| Function Name                            | Status  | Arguments                              | Description                                                                 |
|------------------------------------------|---------|----------------------------------------|-----------------------------------------------------------------------------|
| `SpinReelSpinStartedRollBack()`          | Done    | None                                   | Plays the roll-back animation for the reel during the spin.                 |
| `SpinReelSpinStartedRollForward()`       | Done    | None                                   | Plays the roll-forward animation for the reel during the spin.              |
| `SpinReelSpinEndingRollBack()`           | Done    | None                                   | Plays the final backward roll animation when the reel is ending its spin.   |
| `SpinReelSpinEndingRollForward()`        | Done    | None                                   | Plays the final forward roll animation when the reel is ending its spin.    |
| `SpinReelReachedOutcomeStopIndex()`      | Done    | None                                   | Handles actions when the reel reaches the outcome stop index, including playing reel stop audio and transitioning to the next state. |
| `SpinReelStopped()`                      | Done    | None                                   | Marks the reel as stopped and triggers game-wide events if all reels have stopped. |
| `SpinReelWaiting()`                      | Done    | None                                   | Handles the reel's waiting state (currently a placeholder).                 |
| `SpinReelSpinning()`                     | Done    | None                                   | Manages the spinning action for the reel.                                   |
| `RemoveWinningSymbols(row, reels)`       | TODO    | row (int), reels (array)               | Removes winning symbols from the specified row and reels with a visual effect. |
| `EndCascadeProcess()`                    | TODO    | None                                   | Ends the cascading process and prepares for payout.                         |

---

# Functions Operating on Symbol Groups (Multiple Symbols)

| Function Name                            | Status  | Arguments                              | Description                                                                 |
|------------------------------------------|---------|----------------------------------------|-----------------------------------------------------------------------------|
| `AnimateWinningSymbols(row, reels)`      | TODO    | row (int), reels (array)               | Plays a winning animation for the symbols in the given row across the specified reels. |
| `PlaySymbolAnimation(rowIndex, animatorGroupPrefix, waitForAnimationComplete)` | Done    | rowIndex (int), animatorGroupPrefix (string), waitForAnimationComplete (bool) | Plays the symbol animation for a specific row index in the reel. |
| `CloneAndOverlayCurrentSymbol(rowIndex)` | Done    | rowIndex (int)                         | Clones the current symbol in a row and overlays it on top for visual effects. |
| `CloneCurrentSymbol(rowIndex)`           | Done    | rowIndex (int)                         | Clones the current symbol at the specified row index.                       |

---

# Functions Operating on Single Symbols

| Function Name                            | Status  | Arguments                              | Description                                                                 |
|------------------------------------------|---------|----------------------------------------|-----------------------------------------------------------------------------|
| `SlideReelSymbols(float)`                | Done    | float                                  | Slides all symbols on the reel by a given distance.                         |
| `SlideSymbol(int, float)`                | Done    | int (symbol index), float (distance)   | Slides a single symbol by the specified distance.                           |
