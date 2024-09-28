# Orchestration for Nudging Reels Mechanic in a 5x3 Grid

This document describes an example scenario for a **5x3 grid slot machine** where the **Nudging Reels** mechanic is triggered:

- A near-win occurs where one symbol on Reel 3 is just out of position to complete a winning combination.
- The Nudging Reels mechanic adjusts the reel position to form a winning combination.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Near-Win Condition**: 
  - A combination on **Reels 1 and 2** is one symbol away from completing a high-value win on **Reel 3**.

---

## Orchestration for 5x3 Grid with Nudging Reels

| Step | Event                         | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | New Function |
|------|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Spin Initiated**             | The reels spin as usual, with the player pressing the spin button.                                                                                                                                                                               | **Start the spinning animation** for all reels, with standard sound effects and visual effects.                                                                                                                 | `SpinEngines()` |
| 2    | **Reels Stopped**              | Reels come to a stop, displaying symbols. Reel 3 stops one position away from completing a high-value combination.                                                                                                                                | **Highlight** the near-win combination with a **pulsing effect** to signal the potential for a nudge.                                                                                                           | `DetectNearWinCondition(reels)` |
| 3    | **Nudge Triggered**            | The game detects the near-win on Reel 3 and triggers the **Nudge Reels** event.                                                                                                                                                                   | A **nudge animation** on Reel 3 nudges the reel down by one position, accompanied by a sound effect indicating the reel is moving.                                                                              | `TriggerNudge(reels)` |
| 4    | **Winning Symbols Animation**  | The nudge completes the winning combination, forming a line across Reels 1, 2, and 3.                                                                                                                                                            | **Winning symbols** are highlighted with an **enlarging, glowing, or shaking animation**. Sound effects play to signal the win.                                                                                 | `AnimateWinningSymbols(row, reels)` |
| 5    | **Nudge Completed**            | The nudge process finishes, and the game recalculates the new winning combinations.                                                                                                                                                              | Play a **settling animation** for the nudged reel, ensuring the reel has locked into place.                                                                                                                      | `CompleteNudgeProcess(reels)` |
| 6    | **Payout Calculation**         | The game calculates the payout based on the winning combination after the nudge.                                                                                                                                                                 | **Display the win** on-screen with celebratory animations such as **coin showers** or **flashing lights**. Accompany this with celebratory sound effects.                                                       | `CalculateAndDisplayPayout()` |
| 7    | **Return to Idle**             | The game resets to its idle state, waiting for the player to spin again.                                                                                                                                                                          | **Idle animations** (e.g., button highlights, background effects) are displayed as the game prepares for the next spin.                                                                                         | `ResetToIdleState()` |

---

## Function Headers for New Functions

- `DetectNearWinCondition(reels)`: Detects near-win conditions across the reels and prepares for the nudge.
- `TriggerNudge(reels)`: Activates the nudge mechanic, moving the reel(s) slightly to complete or enhance winning combinations.
- `CompleteNudgeProcess(reels)`: Marks the nudge as complete and triggers any further recalculations or animations.
- `AnimateWinningSymbols(row, reels)`: Plays the winning animation for the symbols in the given `row` across the specified `reels`.
- `CalculateAndDisplayPayout()`: Calculates the final payout for the nudge win and triggers a celebratory display.
- `ResetToIdleState()`: Resets the game to the idle state after all processes are complete.

---

## Summary of Nudging Reels Orchestration

- **Reels Stop**: After spinning, the game checks for any near-win conditions on the reels.
- **Nudge Triggered**: If a near-win is detected, the nudge mechanic activates to shift the reel into place.
- **Winning Symbols Animation**: Once the reel is nudged, the winning symbols play a celebratory animation.
- **Payout Calculation**: The game calculates the payout based on the newly formed winning combination.
- **Return to Idle**: After payout, the game returns to its idle state, awaiting the next spin.

This orchestration ensures a clear and exciting flow for the **Nudging Reels** mechanic, allowing players to experience heightened anticipation as the reels are nudged into winning combinations.
