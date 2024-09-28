# GameBaseGameMachine Functions

| Function Name                | Arguments           | Description                                                                 |
|------------------------------|---------------------|-----------------------------------------------------------------------------|
| `Start()`                    | None                | Initializes the game sequence by stopping audio, configuring settings, and starting reel engines. |
| `OnSpinButtonClicked()`       | None                | Handles the spin button click event, starts reels, loads the server outcome, and processes the result. |
| `ConfigureSettings()`         | None                | Configures and updates UI elements such as credits, bet, and win text.      |
| `ShowSettings()`              | None                | Displays the game settings menu.                                            |
| `HideSettings()`              | None                | Hides the game settings menu.                                               |
| `ResetSettings()`             | None                | Resets the win text to zero after a spin.                                   |
| `Update()`                    | None                | Updates the game state in each frame, dispatching to the appropriate handler based on the game state. |
| `BaseGameSpinning()`          | None                | Handles the actions for when the game is in the spinning state (currently a placeholder). |
| `BaseGameWaiting()`           | None                | Handles the actions for when the game is in the waiting state (currently a placeholder). |
| `BaseGameCompleted()`         | None                | Handles the actions for when the base game spin is completed, including showing settings and resetting the spin state to waiting. |
| `OnBaseGameSpinCompleted()`   | None                | Updates the state to "Completed" after the base game spin is done.           |
| `BaseGamePayout()`            | None                | Triggers the payout process for the base game by calling the mechanics’ payout function. |
| `UpdateStatusText(text)`      | text (string)       | Updates the status text in the game (currently a placeholder).               |
| `PlayStartAnimations()`       | None                | Plays the start animations using the visual controller.                     |
| `StartEngines()`              | None                | Starts the reel engines for all reels in parallel.                          |
| `LoadServerOutcome()`         | None                | Loads the game outcome from the server using an outcome controller.         |
| `WaitForApplyOutcomeDelay()`  | None                | Introduces a delay before applying the outcome based on the outcome delay property. |
| `SpinEngines()`               | None                | Handles the reel spin logic, deducts coins, resets settings, and starts the reel engines. |
| `UpdateBaseGameReelsSpinState(state)` | state (string) | Updates the spin state for each reel in the base game.                      |
| `TryPaying()`                 | None                | Changes the state to "Paying" to initiate the payout phase.                 |
| `CurrentSpinState()`          | None                | Returns the current spin state of the base game.                            |
| `OnPayingCompleted()`         | None                | Stops the audio and marks the spin as completed.                            |
| `OnSpinReelsStopped()`        | None                | Marks the reels as stopped, updates the reel state to waiting, and triggers the payout. |
| `OnOutcomeReceived()`         | None                | Handles the outcome reception for all reels in parallel.                    |
| `OnPointerClick()`            | None                | Handles pointer clicks to start the spin when the user clicks.              |
| `OnPointerClick1Param(param)` | param (string)      | Handles pointer click events with a parameter to navigate to the next/previous machine or lobby. |
| `OnBecameVisible()`           | None                | Logs when the game machine becomes visible in the game window.              |

---

# GameBaseGameReel Functions

| Function Name                            | Arguments                              | Description                                                                 |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|
| `StartEngines()`                         | None                                   | Starts the reel engine by calling the reel controller.                      |
| `OnOutcomeReceived()`                    | None                                   | Processes the outcome for the reel after a spin.                            |
| `SpinEngines()`                          | None                                   | Spins the reel by triggering the reel controller.                           |
| `Update()`                               | None                                   | Updates the reel state on each frame and dispatches to the appropriate function based on the spin state. |
| `SpinReelSpinStartedRollBack()`          | None                                   | Plays the roll-back animation for the reel during the spin.                 |
| `SpinReelSpinStartedRollForward()`       | None                                   | Plays the roll-forward animation for the reel during the spin.              |
| `SpinReelSpinEndingRollBack()`           | None                                   | Plays the final backward roll animation when the reel is ending its spin.   |
| `SpinReelSpinEndingRollForward()`        | None                                   | Plays the final forward roll animation when the reel is ending its spin.    |
| `SpinReelReachedOutcomeStopIndex()`      | None                                   | Handles actions when the reel reaches the outcome stop index, including playing reel stop audio and transitioning to the next state. |
| `SpinReelStopped()`                      | None                                   | Marks the reel as stopped and triggers game-wide events if all reels have stopped. |
| `SpinReelWaiting()`                      | None                                   | Handles the reel's waiting state (currently a placeholder).                 |
| `SpinReelSpinning()`                     | None                                   | Manages the spinning action for the reel.                                   |
| `PlaySpinReelSpinEndingRollBackAnimation()` | None                                 | Plays the animation for the reel’s ending rollback, including symbol animations. |
| `PlaySymbolAnimation(rowIndex, animatorGroupPrefix, waitForAnimationComplete)` | rowIndex (int), animatorGroupPrefix (string), waitForAnimationComplete (bool) | Plays the symbol animation for a specific row index in the reel. |
| `CloneAndOverlayCurrentSymbol(rowIndex)` | rowIndex (int)                        | Clones the current symbol in a row and overlays it on top for visual effects. |
| `CloneCurrentSymbol(rowIndex)`           | rowIndex (int)                        | Clones the current symbol at the specified row index.                       |
| `CalculateSlideDistanceInSymbolUnits()`  | None                                  | Calculates the slide distance in symbol units based on speed and direction. |
| `SlideReelSymbols(slideDistanceInSymbolUnits)` | slideDistanceInSymbolUnits (float)   | Slides all symbols by the specified distance.                               |
| `SlideSymbol(symbolIndex, slideDistanceInSymbolUnits)` | symbolIndex (int), slideDistanceInSymbolUnits (float) | Slides a specific symbol by the specified distance.                         |
| `AdvanceReel()`                          | None                                  | Advances the reel to the next position based on the spin direction.         |
| `AdvanceSymbols()`                       | None                                  | Advances all symbols in the reel during a spin.                             |
| `UpdateReelSymbolForSpin(symbolIndex)`   | symbolIndex (int)                     | Updates the reel symbol for a specific position during a spin.              |
| `UpdateReelStopIndexes()`                | None                                  | Updates the stop indexes for the reel based on the spin state.              |
| `ApplySpinReelStop()`                    | None                                  | Applies the outcome stop for the reel and checks if splicing is needed.     |
| `SpliceReel()`                           | None                                  | Splices the reel for a smooth stop based on the outcome.                    |
| `SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown)` | spliceDistance (int), outcomeReelStopIndex (int), spinDirectionIsDown (bool) | Determines if the reel splicing should be skipped.                         |

---

# BaseGame State Dispatch Table

| State       | Function Handler                       | Description                                                                 |
|-------------|----------------------------------------|-----------------------------------------------------------------------------|
| `Waiting`   | `GameBaseGameMachine.BaseGameWaiting`   | Handles the waiting state of the base game.                                 |
| `Spinning`  | `GameBaseGameMachine.BaseGameSpinning`  | Handles the spinning state of the base game.                                |
| `Completed` | `GameBaseGameMachine.BaseGameCompleted` | Handles the completed state of the base game.                               |

---

# Reel Spin State Dispatch Table

| State                  | Function Handler                           | Description                                                                 |
|------------------------|--------------------------------------------|-----------------------------------------------------------------------------|
| `Waiting`              | `GameBaseGameReel.SpinReelWaiting`          | Handles the waiting state of the reel.                                      |
| `Spinning`             | `GameBaseGameReel.SpinReelSpinning`         | Handles the spinning state of the reel.                                     |
| `Stopped`              | `GameBaseGameReel.SpinReelStopped`          | Handles the stopped state of the reel.                                      |
| `ReachedOutcomeStopIndex` | `GameBaseGameReel.SpinReelReachedOutcomeStopIndex` | Handles the state when the reel reaches the outcome stop index.            |
| `SpinStartedRollBack`   | `GameBaseGameReel.SpinReelSpinStartedRollBack` | Handles the backward roll animation during the spin.                       |
| `SpinStartedRollForward`| `GameBaseGameReel.SpinReelSpinStartedRollForward` | Handles the forward roll animation during the spin.                        |
| `SpinEndingRollForward` | `GameBaseGameReel.SpinReelSpinEndingRollForward` | Handles the forward roll animation when the spin is ending.                |
| `SpinEndingRollBack`    | `GameBaseGameReel.SpinReelSpinEndingRollBack` | Handles the backward roll animation when the spin is ending.               |
