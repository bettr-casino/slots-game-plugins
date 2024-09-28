# GameBaseGameMachine Functions (Base Game)

| Function Name                | Arguments           | Description                                                                 | Status |
|------------------------------|---------------------|-----------------------------------------------------------------------------|--------|
| `Start()`                    | None                | Initializes the game sequence by stopping audio, configuring settings, and starting reel engines. | Done   |
| `OnSpinButtonClicked()`       | None                | Handles the spin button click event, starts reels, loads the server outcome, and processes the result. | Done   |
| `ConfigureSettings()`         | None                | Configures and updates UI elements such as credits, bet, and win text.      | Done   |
| `ShowSettings()`              | None                | Displays the game settings menu.                                            | Done   |
| `HideSettings()`              | None                | Hides the game settings menu.                                               | Done   |
| `ResetSettings()`             | None                | Resets the win text to zero after a spin.                                   | Done   |
| `Update()`                    | None                | Updates the game state in each frame, dispatching to the appropriate handler based on the game state. | Done   |
| `BaseGameSpinning()`          | None                | Handles the actions for when the game is in the spinning state (currently a placeholder). | Done   |
| `BaseGameWaiting()`           | None                | Handles the actions for when the game is in the waiting state (currently a placeholder). | Done   |
| `BaseGameCompleted()`         | None                | Handles the actions for when the base game spin is completed, including showing settings and resetting the spin state to waiting. | Done   |
| `OnBaseGameSpinCompleted()`   | None                | Updates the state to "Completed" after the base game spin is done.          | Done   |
| `BaseGamePayout()`            | None                | Triggers the payout process for the base game by calling the mechanics’ payout function. | Done   |
| `UpdateBaseGameReelsSpinState(state)` | state (string) | Updates the spin state for each reel in the base game.                      | Done   |
| `TryPaying()`                 | None                | Changes the state to "Paying" to initiate the payout phase.                 | Done   |
| `CurrentSpinState()`          | None                | Returns the current spin state of the base game.                            | Done   |
| `OnPayingCompleted()`         | None                | Stops the audio and marks the spin as completed.                            | Done   |
| `OnSpinReelsStopped()`        | None                | Marks the reels as stopped, updates the reel state to waiting, and triggers the payout. | Done   |
| `OnOutcomeReceived()`         | None                | Handles the outcome reception for all reels in parallel.                    | Done   |
| `OnPointerClick()`            | None                | Handles pointer clicks to start the spin when the user clicks.              | Done   |
| `OnPointerClick1Param(param)` | param (string)      | Handles pointer click events with a parameter to navigate to the next/previous machine or lobby. | Done   |
| `OnBecameVisible()`           | None                | Logs when the game machine becomes visible in the game window.              | Done   |
| `CalculateAndDisplayPayout()` | None                | Calculates the final payout and triggers a celebratory display for the player. | TODO   |
| `ResetToIdleState()`          | None                | Resets the game to the idle state after all processes are complete.          | TODO   |

---

# Functions Operating on Entire Reel Set

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `StartEngines()`                         | None                                   | Starts the reel engine for all reels.                                       | Done   |
| `SpinEngines()`                          | None                                   | Spins the reels by triggering the reel controller.                          | Done   |
| `DetectWinningCombinationGroupedByReel(reels)` | reels (array)                         | Detects winning combinations across the specified reels grouped by their reel positions. | TODO   |
| `CheckForNewWinsAfterCascade(reels)`     | reels (array)                         | Checks for any new winning combinations after a symbol cascade across the specified reels. | TODO   |

---

# Functions Operating on Reel Strip Groups (Multiple Reels)

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `OnOutcomeReceived()`                    | None                                   | Processes the outcome for all reels after a spin.                           | Done   |
| `PlaySpinReelSpinEndingRollBackAnimation()` | None                                  | Plays the ending rollback animation for all reels.                          | Done   |
| `CascadeSymbols(fromRow, toRow, reels)`  | fromRow (int), toRow (int), reels (array) | Cascades symbols from one row to another across the specified reels.        | TODO   |

---

# Functions Operating on Reel Strip (Single Reel)

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `SpinReelSpinStartedRollBack()`          | None                                   | Plays the roll-back animation for the reel during the spin.                 | Done   |
| `SpinReelSpinStartedRollForward()`       | None                                   | Plays the roll-forward animation for the reel during the spin.              | Done   |
| `SpinReelSpinEndingRollBack()`           | None                                   | Plays the final backward roll animation when the reel is ending its spin.   | Done   |
| `SpinReelSpinEndingRollForward()`        | None                                   | Plays the final forward roll animation when the reel is ending its spin.    | Done   |
| `SpinReelReachedOutcomeStopIndex()`      | None                                   | Handles actions when the reel reaches the outcome stop index, including playing reel stop audio and transitioning to the next state. | Done   |
| `SpinReelStopped()`                      | None                                   | Marks the reel as stopped and triggers game-wide events if all reels have stopped. | Done   |
| `SpinReelWaiting()`                      | None                                   | Handles the reel's waiting state (currently a placeholder).                 | Done   |
| `SpinReelSpinning()`                     | None                                   | Manages the spinning action for the reel.                                   | Done   |
| `RemoveWinningSymbols(row, reels)`       | row (int), reels (array)               | Removes winning symbols from the specified row and reels with a visual effect. | TODO   |
| `EndCascadeProcess()`                    | None                                   | Ends the cascading process and prepares for payout.                         | TODO   |

---

# Functions Operating on Symbol Groups (Multiple Symbols)

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `AnimateWinningSymbols(row, reels)`      | row (int), reels (array)               | Plays a winning animation for the symbols in the given row across the specified reels. | TODO   |
| `PlaySymbolAnimation(rowIndex, animatorGroupPrefix, waitForAnimationComplete)` | rowIndex (int), animatorGroupPrefix (string), waitForAnimationComplete (bool) | Plays the symbol animation for a specific row index in the reel. | Done   |
| `CloneAndOverlayCurrentSymbol(rowIndex)` | rowIndex (int)                         | Clones the current symbol in a row and overlays it on top for visual effects. | Done   |
| `CloneCurrentSymbol(rowIndex)`           | rowIndex (int)                         | Clones the current symbol at the specified row index.                       | Done   |

---

# Functions Operating on Single Symbols

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `SlideReelSymbols(float)`                | float                                  | Slides all symbols on the reel by a given distance.                         | Done   |
| `SlideSymbol(int, float)`                | int (symbol index), float (distance)   | Slides a single symbol by the specified distance.                           | Done   |
