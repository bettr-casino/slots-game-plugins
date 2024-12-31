# Orchestration for Locked Reels in a 5x3 Grid

This document describes an example scenario for a **5x3 grid slot machine** where the **Locked Reels** mechanic is triggered:

- A special symbol lands on Reel 1, causing adjacent reels (Reels 2 and 3) to lock together.
- The locked reels spin together and display identical symbols, enhancing the player’s chances of forming winning combinations.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Locked Reels**:
  - **Reels 2 and 3** are locked after a special symbol lands on Reel 1.
  - Reels 2 and 3 will spin together and display identical symbols in the next spin.

---

## Orchestration for Locked Reels in a 5x3 Grid

| Step | Event                         | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Special Symbol Lands on Reel 1**             | The player spins the reels, and a special symbol lands on Reel 1, triggering the Locked Reels mechanic.                                                                                                                                            | Highlight the special symbol on Reel 1 with a **glowing or pulsing effect**. Play a sound effect to indicate that the Locked Reels feature has been activated.                                                                            | `TriggerLockedReelsBySpecialSymbol(reels)` |
| 2    | **Payout After Triggering Spin**    | The game evaluates any winning combinations from the triggering spin and pays out the player. Afterward, adjacent reels (Reels 2 and 3) lock together for the next spin.                                                                                                                                                      | Play a **payout animation** for any wins from the initial spin. Apply a visual effect on Reels 2 and 3 to show they are locked for the next spin.                                                                                                                               | `PayoutForTriggeringSpin()` |
| 3    | **Locked Reels Spin Together** | Reels 2 and 3 spin together in sync, displaying identical symbols when they stop.                                                                                                                                                                                                        | Reels 2 and 3 spin together, showing the same symbols as they stop. Emphasize the synchronization with a **glowing or linking animation**.                                                   | `SpinEngines()` + `ActivateLockedReels(reels)` |
| 4    | **Evaluate Wins**              | The game evaluates the reels, including the locked reels, to detect any winning combinations.                                                                                                                                                    | Trigger the standard **win detection animation** for paylines that involve symbols on Reels 2 and 3. Highlight matching symbols with a **winning glow effect**.                                             | `DetectWinningCombinationGroupedByReel(reels)` |
| 5    | **Winning Symbols Animation**  | If a winning combination is found that includes symbols on the locked reels (Reels 2 and 3), the winning symbols play a celebratory animation.                                                                                                   | Winning symbols play a **celebratory animation**, such as **glowing, pulsing, or expanding**. Include a sound effect to emphasize the win.                                                                        | `AnimateWinningSymbols(row, reels)` |
| 6    | **Payout Calculation**         | After the locked reels have stopped and winning combinations are evaluated, the game calculates the payout for the player.                                                                                                                                                               | Display a **payout calculation animation**, showing the total credits earned from the locked reels spin. Add celebratory sound effects or visual effects to accompany the payout display.                                 | `CalculateAndDisplayPayout()` |
| 7    | **Locked Reels Deactivation**   | After the locked reels spin, the mechanic deactivates, and the game returns to normal gameplay. Reels 2 and 3 resume independent spinning in the next spin.                                                                                                                    | Reels 2 and 3 return to independent behavior. Play a **fade-out animation** or visual effect to indicate the end of the locked reels feature.                                                                         | `DeactivateLockedReels()` |
| 8    | **Return to Idle**             | Once the payout has been displayed and the Locked Reels feature ends, the game resets to the idle state, ready for the next spin.                                                                                                                                                   | Reset the reel animations and return to the **idle state**. Display idle animations for buttons and background elements, preparing for the next spin.                                                                     | `ResetToIdleState()` |

---

## Function Headers for New Functions

- `TriggerLockedReelsBySpecialSymbol(reels)`: Detects when the special symbol lands on Reel 1, triggering the Locked Reels mechanic.
- `PayoutForTriggeringSpin()`: Calculates and displays the payout for the spin that triggered the Locked Reels feature.
- `ActivateLockedReels(reels)`: Highlights the locked reels and applies a special animation to indicate they are synchronized.
- `DeactivateLockedReels()`: Deactivates the locked reels and returns them to normal spinning behavior.
- `CalculateAndDisplayPayout()`: Calculates the payout for the spin and displays it on screen.
- `ResetToIdleState()`: Resets the game to its idle state after the locked reels mechanic ends.

---

## Summary of Locked Reels Orchestration

- **Special Symbol Triggers Locked Reels**: A special symbol lands on Reel 1, causing adjacent reels (Reels 2 and 3) to lock and spin together in the next spin.
- **Locked Reels Spin Together**: Reels 2 and 3 spin in sync, showing identical symbols and increasing the chances of forming winning combinations.
- **Winning Symbols**: Locked reels display matching symbols, and any winning combinations are evaluated.
- **Payout Calculation**: After the spin, the game calculates the payout and awards the player.
- **Return to Normal**: Once the Locked Reels feature ends, the game returns to normal, independent spinning for the reels.

This orchestration ensures a clear process for handling the Locked Reels mechanic, triggered by a special symbol on Reel 1, and provides a visually engaging experience for players.
