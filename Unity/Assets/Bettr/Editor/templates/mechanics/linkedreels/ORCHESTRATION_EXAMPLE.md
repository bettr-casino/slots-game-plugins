# Orchestration for Linked Reels in a 5x3 Grid

This document describes an example scenario for a **5x3 grid slot machine** where the **Linked Reels** mechanic is triggered:

- Two adjacent reels (Reel 2 and Reel 3) are linked together and display the same symbols.
- This mechanic enhances the player's chances of winning by duplicating reels.

---

## Scenario:
- **Grid Configuration**: 5 reels, each with 3 rows.
- **Linked Reels**:
  - **Reels 2 and 3** are linked and display identical symbols after the spin.

---

## Orchestration for Linked Reels in a 5x3 Grid

| Step | Event                         | Description                                                                                                                                                                                                                                      | Animation Orchestration                                                                                                                                                                                        | Function |
|------|-------------------------------|--------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|----------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------|--------------|
| 1    | **Spin Initiated**             | The reels begin spinning as usual. The Linked Reels mechanic is triggered, linking Reel 2 and Reel 3.                                                                                                                                            | Start the spin animation for all reels. Reels 2 and 3 should display a **synchronized spinning effect** to indicate they are linked.                                                                            | `SpinEngines()` + `ActivateLinkedReels(reels)` |
| 2    | **Linked Reels Activation**    | The game detects the activation of Linked Reels, causing Reel 2 and Reel 3 to spin in sync.                                                                                                                                                      | Apply a **glowing border** or other visual effect to emphasize that Reel 2 and Reel 3 are linked. Include a sound cue to signify the linked reels.                                                                | `ActivateLinkedReels(reels)` |
| 3    | **Linked Reels Display Symbols** | Reels 2 and 3 stop spinning and display the same symbols, increasing the chances of winning combinations.                                                                                                                                        | Reels 2 and 3 display the **exact same symbol sequence**, synchronized to stop at the same time. Highlight the linked symbols with a **glow or pulsating effect**.                                                   | `DisplayLinkedSymbols(reels)` |
| 4    | **Evaluate Wins**              | The game evaluates the reels, including the linked reels, to detect any winning combinations.                                                                                                                                                    | Trigger the standard **win detection animation** for any paylines that involve symbols on Reel 2 and Reel 3. Highlight matching symbols with a **winning glow effect**.                                             | `DetectWinningCombinationGroupedByReel(reels)` |
| 5    | **Winning Symbols Animation**  | If a winning combination is found that includes symbols on the linked reels (Reels 2 and 3), the winning symbols play a celebratory animation.                                                                                                   | Winning symbols play a **celebratory animation**, such as **glowing, pulsing, or expanding**. Include a sound effect to emphasize the win.                                                                        | `AnimateWinningSymbols(row, reels)` |
| 6    | **Linked Reels Expansion**     | During a bonus round or special feature, additional reels may be linked. For example, Reel 4 is linked to Reels 2 and 3.                                                                                                                          | Apply the same **linked reel animation** to Reel 4, expanding the visual glow to show that 3 reels are now linked (Reels 2, 3, and 4).                                                                             | `ExpandLinkedReels(reels)` |
| 7    | **Second Linked Reels Spin**   | During the bonus round, the newly linked reels (Reel 4, along with Reels 2 and 3) spin together, displaying identical symbols.                                                                                                                    | Reels 2, 3, and 4 spin simultaneously, and once again display the **same symbols**. Animate the synchronized stopping of all three reels.                                                                         | `SpinEngines()` + `ActivateLinkedReels(reels)` |
| 8    | **Massive Win Evaluation**     | The game checks if the expanded linked reels (Reels 2, 3, and 4) have contributed to a massive win.                                                                                                                                               | Evaluate the newly linked reels for winning combinations. If a massive win is detected, trigger a **special animation**, such as **flashing lights, coin showers, or fireworks**.                                   | `DetectWinningCombinationGroupedByReel(reels)` |
| 9    | **Payout Calculation**         | After all linked reels have stopped, the game calculates the payout for the player.                                                                                                                                                               | Display a **payout calculation animation**, showing the total credits earned from the linked reels. Add celebratory sound effects or visual effects to accompany the payout display.                                 | `CalculateAndDisplayPayout()` |
| 10   | **Return to Idle**             | Once the payout has been displayed, the game resets to the idle state, ready for the next spin.                                                                                                                                                   | Reset the reel animations and return to the **idle state**. Display idle animations for buttons and backgrounds, preparing for the next spin.                                                                     | `ResetToIdleState()` |

---

## Function Headers for New Functions

- `ActivateLinkedReels(reels)`: Highlights the linked reels and applies a special animation to indicate they are synchronized.
- `DisplayLinkedSymbols(reels)`: Displays identical symbols on the linked reels when they stop spinning.
- `ExpandLinkedReels(reels)`: Adds more reels to the linked reels group during a bonus round.
- `CalculateAndDisplayPayout()`: Calculates the payout for the spin and displays it on screen.
- `ResetToIdleState()`: Resets the game to its idle state after the linked reels mechanic ends.

---

## Summary of Linked Reels Orchestration

- **Linked Reels Activation**: Reels 2 and 3 are linked during a regular spin or bonus round.
- **Winning Symbols**: Linked reels display identical symbols, increasing the player's chances of winning.
- **Linked Reels Expansion**: More reels may be linked during bonus rounds, further increasing win potential.
- **Massive Win Evaluation**: The game evaluates whether the linked reels result in a massive win, and triggers special animations if they do.
- **Return to Idle**: After payouts are calculated, the game returns to its idle state, awaiting the next spin.

This orchestration ensures a clear process for handling linked reels, expanding them during bonus rounds, and providing a visually engaging experience for players.
