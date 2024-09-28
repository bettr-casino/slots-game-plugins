# GameBaseGameMachine Functions

| Function Name                | Arguments           | Description                                                                 | Status |
|------------------------------|---------------------|-----------------------------------------------------------------------------|--------|
| `Start()`                    | None                | Initializes the game sequence by stopping audio, configuring settings, and starting reel engines. | Done |
| `OnSpinButtonClicked()`       | None                | Handles the spin button click event, starts reels, loads the server outcome, and processes the result. | Done |
| `ConfigureSettings()`         | None                | Configures and updates UI elements such as credits, bet, and win text.      | Done |
| `ShowSettings()`              | None                | Displays the game settings menu.                                            | Done |
| `HideSettings()`              | None                | Hides the game settings menu.                                               | Done |
| `ResetSettings()`             | None                | Resets the win text to zero after a spin.                                   | Done |
| `Update()`                    | None                | Updates the game state in each frame, dispatching to the appropriate handler based on the game state. | Done |
| `BaseGameSpinning()`          | None                | Handles the actions for when the game is in the spinning state (currently a placeholder). | Done |
| `BaseGameWaiting()`           | None                | Handles the actions for when the game is in the waiting state (currently a placeholder). | Done |
| `BaseGameCompleted()`         | None                | Handles the actions for when the base game spin is completed, including showing settings and resetting the spin state to waiting. | Done |
| `OnBaseGameSpinCompleted()`   | None                | Updates the state to "Completed" after the base game spin is done.           | Done |
| `BaseGamePayout()`            | None                | Triggers the payout process for the base game by calling the mechanics’ payout function. | Done |
| `UpdateStatusText(text)`      | text (string)       | Updates the status text in the game (currently a placeholder).               | Done |
| `PlayStartAnimations()`       | None                | Plays the start animations using the visual controller.                     | Done |
| `StartEngines()`              | None                | Starts the reel engines for all reels in parallel.                          | Done |
| `LoadServerOutcome()`         | None                | Loads the game outcome from the server using an outcome controller.         | Done |
| `WaitForApplyOutcomeDelay()`  | None                | Introduces a delay before applying the outcome based on the outcome delay property. | Done |
| `SpinEngines()`               | None                | Handles the reel spin logic, deducts coins, resets settings, and starts the reel engines. | Done |
| `UpdateBaseGameReelsSpinState(state)` | state (string) | Updates the spin state for each reel in the base game.                      | Done |
| `TryPaying()`                 | None                | Changes the state to "Paying" to initiate the payout phase.                 | Done |
| `CurrentSpinState()`          | None                | Returns the current spin state of the base game.                            | Done |
| `OnPayingCompleted()`         | None                | Stops the audio and marks the spin as completed.                            | Done |
| `OnSpinReelsStopped()`        | None                | Marks the reels as stopped, updates the reel state to waiting, and triggers the payout. | Done |
| `OnOutcomeReceived()`         | None                | Handles the outcome reception for all reels in parallel.                    | Done |
| `OnPointerClick()`            | None                | Handles pointer clicks to start the spin when the user clicks.              | Done |
| `OnPointerClick1Param(param)` | param (string)      | Handles pointer click events with a parameter to navigate to the next/previous machine or lobby. | Done |
| `OnBecameVisible()`           | None                | Logs when the game machine becomes visible in the game window.              | Done |

---

# GameBaseGameReel Functions

| Function Name                            | Arguments                              | Description                                                                 | Status |
|------------------------------------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `StartEngines()`                         | None                                   | Starts the reel engine by calling the reel controller.                      | Done |
| `OnOutcomeReceived()`                    | None                                   | Processes the outcome for the reel after a spin.                            | Done |
| `SpinEngines()`                          | None                                   | Spins the reel by triggering the reel controller.                           | Done |
| `Update()`                               | None                                   | Updates the reel state on each frame and dispatches to the appropriate function based on the spin state. | Done |
| `SpinReelSpinStartedRollBack()`          | None                                   | Plays the roll-back animation for the reel during the spin.                 | Done |
| `SpinReelSpinStartedRollForward()`       | None                                   | Plays the roll-forward animation for the reel during the spin.              | Done |
| `SpinReelSpinEndingRollBack()`           | None                                   | Plays the final backward roll animation when the reel is ending its spin.   | Done |
| `SpinReelSpinEndingRollForward()`        | None                                   | Plays the final forward roll animation when the reel is ending its spin.    | Done |
| `SpinReelReachedOutcomeStopIndex()`      | None                                   | Handles actions when the reel reaches the outcome stop index, including playing reel stop audio and transitioning to the next state. | Done |
| `SpinReelStopped()`                      | None                                   | Marks the reel as stopped and triggers game-wide events if all reels have stopped. | Done |
| `SpinReelWaiting()`                      | None                                   | Handles the reel's waiting state (currently a placeholder).                 | Done |
| `SpinReelSpinning()`                     | None                                   | Manages the spinning action for the reel.                                   | Done |
| `PlaySpinReelSpinEndingRollBackAnimation()` | None                                 | Plays the animation for the reel’s ending rollback, including symbol animations. | Done |
| `PlaySymbolAnimation(rowIndex, animatorGroupPrefix, waitForAnimationComplete)` | rowIndex (int), animatorGroupPrefix (string), waitForAnimationComplete (bool) | Plays the symbol animation for a specific row index in the reel. | Done |
| `CloneAndOverlayCurrentSymbol(rowIndex)` | rowIndex (int)                        | Clones the current symbol in a row and overlays it on top for visual effects. | Done |
| `CloneCurrentSymbol(rowIndex)`           | rowIndex (int)                        | Clones the current symbol at the specified row index.                       | Done |
| `CalculateSlideDistanceInSymbolUnits()`  | None                                  | Calculates the slide distance in symbol units based on speed and direction. | Done |
| `SlideReelSymbols(slideDistanceInSymbolUnits)` | slideDistanceInSymbolUnits (float)   | Slides all symbols by the specified distance.                               | Done |
| `SlideSymbol(symbolIndex, slideDistanceInSymbolUnits)` | symbolIndex (int), slideDistanceInSymbolUnits (float) | Slides a specific symbol by the specified distance.                         | Done |
| `AdvanceReel()`                          | None                                  | Advances the reel to the next position based on the spin direction.         | Done |
| `AdvanceSymbols()`                       | None                                  | Advances all symbols in the reel during a spin.                             | Done |
| `UpdateReelSymbolForSpin(symbolIndex)`   | symbolIndex (int)                     | Updates the reel symbol for a specific position during a spin.              | Done |
| `UpdateReelStopIndexes()`                | None                                  | Updates the stop indexes for the reel based on the spin state.              | Done |
| `ApplySpinReelStop()`                    | None                                  | Applies the outcome stop for the reel and checks if splicing is needed.     | Done |
| `SpliceReel()`                           | None                                  | Splices the reel for a smooth stop based on the outcome.                    | Done |
| `SkipSpliceReel(spliceDistance, outcomeReelStopIndex, spinDirectionIsDown)` | spliceDistance (int), outcomeReelStopIndex (int), spinDirectionIsDown (bool) | Determines if the reel splicing should be skipped.                         | Done |

---

# BaseGame State Dispatch Table

| State       | Function Handler                       | Description                                                                 | Status |
|-------------|----------------------------------------|-----------------------------------------------------------------------------|--------|
| `Waiting`   | `GameBaseGameMachine.BaseGameWaiting`   | Handles the waiting state of the base game.                                 | Done |
| `Spinning`  | `GameBaseGameMachine.BaseGameSpinning`  | Handles the spinning state of the base game.                                | Done |
| `Completed` | `GameBaseGameMachine.BaseGameCompleted` | Handles the completed state of the base game.                               | Done |

---

# Reel Spin State Dispatch Table

| State                  | Function Handler                           | Description                                                                 | Status |
|------------------------|--------------------------------------------|-----------------------------------------------------------------------------|--------|
| `Waiting`              | `GameBaseGameReel.SpinReelWaiting`          | Handles the waiting state of the reel.                                      | Done |
| `Spinning`             | `GameBaseGameReel.SpinReelSpinning`         | Handles the spinning state of the reel.                                     | Done |
| `Stopped`              | `GameBaseGameReel.SpinReelStopped`          | Handles the stopped state of the reel.                                      | Done |
| `ReachedOutcomeStopIndex` | `GameBaseGameReel.SpinReelReachedOutcomeStopIndex` | Handles the state when the reel reaches the outcome stop index.            | Done |
| `SpinStartedRollBack`   | `GameBaseGameReel.SpinReelSpinStartedRollBack` | Handles the backward roll animation during the spin.                       | Done |
| `SpinStartedRollForward`| `GameBaseGameReel.SpinReelSpinStartedRollForward` | Handles the forward roll animation during the spin.                        | Done |
| `SpinEndingRollForward` | `GameBaseGameReel.SpinReelSpinEndingRollForward` | Handles the forward roll animation when the spin is ending.                | Done |
| `SpinEndingRollBack`    | `GameBaseGameReel.SpinReelSpinEndingRollBack` | Handles the backward roll animation when the spin is ending.               | Done |
